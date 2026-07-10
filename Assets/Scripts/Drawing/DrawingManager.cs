using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Magic.Inventory;
using DG.Tweening;

namespace Magic.Drawing
{
    public class DrawingManager : MonoBehaviour
    {
        [Header("Magic Combo Stats")]
        public List<DrawnShape> drawnShapes = new List<DrawnShape>();

        [Header("References")]
        public RectTransform drawingArea; // 스크롤 UI 영역

        [Header("Settings")]
        public Camera mainCamera;


        public DrawingData currentDrawingData { get; protected set; }
        protected DrawingLine currentLine;
        protected bool isDrawing = false;
        protected bool isRefilling = false;
        protected bool isOutsideArea = false;
        protected bool isPenReady = false;

        protected DrawingDatabase drawingDatabase;

        // 글로벌 인벤토리 시스템 참조
        public virtual List<ItemInstance> inventory => (InventoryManager.Instance != null && InventoryManager.Instance.items != null) ? InventoryManager.Instance.items : new List<ItemInstance>();
        
        public int selectedScrollIndex = -1;
        public int selectedInkIndex = -1;
        public int selectedPenIndex = -1;
        public float inkDrainRate = 10f; // 잉크 소모 속도 (초당)

        [Header("Pen Visuals")]
        public RectTransform penVisual;
        public RectTransform inkBottleVisual;

        protected virtual void Start()
        {
            if (mainCamera == null)
                mainCamera = Camera.main;

            currentDrawingData = new DrawingData();
            drawingDatabase = DrawingDatabase.Instance;

            // 처음에 펜을 잉크통에 위치시키고 바깥에 있는 것으로 처리
            if (penVisual != null && inkBottleVisual != null)
            {
                penVisual.position = inkBottleVisual.position;
            }
            isOutsideArea = true;

            // 테스트 편의성을 위해 장착된 아이템이 없으면 인벤토리에서 첫 번째 항목을 자동 선택
            if (inventory != null)
            {
                if (selectedScrollIndex == -1) selectedScrollIndex = inventory.FindIndex(i => i is Item_Scroll);
                if (selectedInkIndex == -1) selectedInkIndex = inventory.FindIndex(i => i is Item_Ink);
                if (selectedPenIndex == -1) selectedPenIndex = inventory.FindIndex(i => i is Item_Pen);
            }

            // 시작 시 잉크가 비어있을 수 있으므로 충전 한 번 시도
            TryExecuteRefillLogic();

        }

        protected virtual void Update()
        {
            HandleDrawingInput();
        }

        protected virtual void HandleDrawingInput()
        {
            // 그리기 영역(UI) 제한 검사
            if (drawingArea != null)
            {
                Camera uiCamera = null;
                Canvas canvas = drawingArea.GetComponentInParent<Canvas>();
                if (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
                {
                    uiCamera = canvas.worldCamera != null ? canvas.worldCamera : mainCamera;
                }

                if (!RectTransformUtility.RectangleContainsScreenPoint(drawingArea, Input.mousePosition, uiCamera))
                {
                    if (isDrawing)
                        EndStroke(); // 영역을 벗어나면 획 강제 종료
                    
                    if (!isOutsideArea)
                    {
                        isOutsideArea = true;
                        RefillPen();
                    }
                        
                    return;
                }
                else
                {
                    if (isOutsideArea)
                    {
                        isOutsideArea = false;
                        ReturnPenToMouse();
                    }
                }
            }

            if (penVisual != null && !isRefilling)
            {
                penVisual.position = Input.mousePosition;
            }

            if (Input.GetMouseButtonDown(0))
            {
                StartStroke();
            }
            else if (Input.GetMouseButton(0))
            {
                if (!isDrawing && isPenReady)
                {
                    StartStroke();
                }
                else
                {
                    UpdateStroke();
                }
            }
            else if (Input.GetMouseButtonUp(0))
            {
                EndStroke();
            }

            // 일반 그리기 모드에서의 마법 조합 (스페이스바)
            if (Input.GetKeyDown(KeyCode.Space))
            {
                MatchCombo();
            }

            // 개발자 도구: 현재 그려진 선을 'Star' 템플릿으로 저장 (엔터 키)
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                if (currentDrawingData != null && currentDrawingData.strokes.Count > 0)
                {
                    ShapeRecognizer.SaveTemplateAsset(currentDrawingData, "Star");
                    Debug.Log("<color=magenta>[개발자 도구] 방금 그린 모양을 'Star' 커스텀 템플릿으로 저장했습니다!</color>");
                    ClearDrawing();
                }
                else
                {
                    Debug.LogWarning("저장할 도형이 없습니다. 먼저 별을 그려주세요.");
                }
            }
        }

        protected virtual void StartStroke()
        {
            if (!isPenReady) 
            {
                Debug.LogWarning("[그리기 실패] 펜이 아직 완전히 나타나지 않았습니다.");
                return; 
            }

            if (drawingDatabase == null || drawingDatabase.linePrefab == null)
            {
                Debug.LogError("[그리기 실패] LinePrefab is not assigned in DrawingDatabase.");
                return;
            }

            // 스크롤 확인
            var inv = inventory;
            if (inv == null)
            {
                Debug.LogError("[그리기 실패] inventory가 null입니다! InventoryManager.Instance.items를 확인하세요.");
                return;
            }
            
            if (selectedScrollIndex == -1 || inv.Count <= selectedScrollIndex)
            {
                Debug.LogWarning("[그리기 실패] 스크롤을 먼저 선택해주세요.");
                return;
            }
            
            Item_Scroll currentScroll = inv[selectedScrollIndex] as Item_Scroll;
            if (currentScroll == null || !currentScroll.isEmpty)
            {
                Debug.LogWarning("[그리기 실패] 유효한 빈 스크롤이 아닙니다.");
                return;
            }

            // 펜 확인
            Item_Pen currentPen = selectedPenIndex != -1 && selectedPenIndex < inv.Count ? inv[selectedPenIndex] as Item_Pen : null;
            if (currentPen == null || !CanDrawWithPen(currentPen))
            {
                string warnMsg = currentPen != null && currentPen.PenData != null && currentPen.PenData.consumesMana 
                    ? "[그리기 실패] 마력이 부족하여 마나 펜을 사용할 수 없습니다." 
                    : "[그리기 실패] 펜을 선택하지 않았거나 펜에 잉크가 없습니다. (스크롤 밖으로 나가서 충전하세요)";
                Debug.LogWarning(warnMsg);
                return;
            }

            isDrawing = true;
            
            GameObject lineObj = Instantiate(drawingDatabase.linePrefab, transform);
            currentLine = lineObj.GetComponent<DrawingLine>();

            UpdateStroke();
        }

        protected virtual void UpdateStroke()
        {
            if (!isDrawing || currentLine == null) return;

            Vector3 mousePos = Input.mousePosition;
            if (mainCamera == null) Debug.LogError("[UpdateStroke] mainCamera가 null입니다!");
            
            mousePos.z = Mathf.Abs(mainCamera.transform.position.z);
            Vector3 worldPos = mainCamera.ScreenToWorldPoint(mousePos);

            Vector2 newPoint = new Vector2(worldPos.x, worldPos.y);

            if (currentLine.currentStroke == null) Debug.LogError("[UpdateStroke] currentStroke가 null입니다!");
            int prevCount = currentLine.currentStroke.points.Count;

            if (ShouldConsumeInk())
            {
                Item_Pen currentPen = selectedPenIndex != -1 && selectedPenIndex < inventory.Count ? inventory[selectedPenIndex] as Item_Pen : null;
                if (currentPen == null || !CanDrawWithPen(currentPen))
                {
                    EndStroke();
                    string warnMsg = currentPen != null && currentPen.PenData != null && currentPen.PenData.consumesMana
                        ? "[그리기] 플레이어 마력이 바닥나 그릴 수 없습니다!"
                        : "[그리기] 펜의 잉크가 바닥났습니다! 스크롤 밖으로 나가 잉크를 충전하세요.";
                    Debug.LogWarning(warnMsg);
                    return;
                }

                ConsumePenResource(currentPen);
            }

            currentLine.AddPoint(newPoint);
        }

        /// <summary>
        /// 현재 장착한 펜으로 드로잉을 시작하거나 유지할 수 있는지 체크합니다.
        /// </summary>
        protected virtual bool CanDrawWithPen(Item_Pen pen)
        {
            if (pen == null) return false;
            if (pen.PenData != null && pen.PenData.consumesMana) return true; // 일반 매니저에서는 마나 제한 없음
            return pen.currentInkCapacity > 0f;
        }

        /// <summary>
        /// 펜 사용 시 드로잉 자원(잉크 혹은 마나)을 소모합니다.
        /// </summary>
        protected virtual void ConsumePenResource(Item_Pen pen)
        {
            if (pen == null) return;
            if (pen.PenData != null && pen.PenData.consumesMana) return; // 일반 매니저에서는 마나 차감 구현 생략 (전투 매니저에서 재정의)
            float rate = pen.PenData != null ? pen.PenData.inkConsumptionRate : 0f;
            pen.currentInkCapacity -= rate * Time.deltaTime;
            if (pen.currentInkCapacity < 0) pen.currentInkCapacity = 0;
        }

        protected virtual bool ShouldConsumeInk()
        {
            return true;
        }

        protected virtual async void EndStroke()
        {
            if (!isDrawing) return;
            isDrawing = false;
            

            if (currentLine != null)
            {
                if (currentLine.currentStroke.points.Count < 2)
                {
                    Debug.LogWarning("[그리기 종료] 포인트가 2개 미만이라 선이 삭제됩니다.");
                    Destroy(currentLine.gameObject);
                }
                else
                {
                    currentDrawingData.AddStroke(currentLine.currentStroke);

                    DrawingData singleStrokeData = new DrawingData();
                    singleStrokeData.AddStroke(currentLine.currentStroke);
                    
                    Rect bounds = GetBounds(currentLine.currentStroke);
                    Vector2 center = new Vector2(bounds.center.x, bounds.center.y);
                    int pointCount = currentLine.currentStroke.points.Count;

                    // 백그라운드 스레드에서 인식 수행 전, 메인 스레드에서 필요한 에셋 로드 보장
                    ShapeRecognizer.EnsureInitialized();

                    // 유저 경험을 위해 무거운 도형 판별 작업을 백그라운드 스레드로 넘깁니다 (렉 방지)
                    RecognizerResult result = await System.Threading.Tasks.Task.Run(() => ShapeRecognizer.Recognize(singleStrokeData));

                    string shapeName = result.Name;
                    float score = result.Score;
                    
                    drawnShapes.Add(new DrawnShape(shapeName, bounds, center, score));

                    PrintCurrentStats();
                }
                currentLine = null;
            }
        }

        protected virtual void MatchCombo()
        {
            if (drawnShapes.Count == 0) return;

            if (drawingDatabase == null || drawingDatabase.recipes == null || drawingDatabase.recipes.Length == 0)
            {
                Debug.LogWarning("[Combo Matcher] 전역 드로잉 데이터베이스가 없거나 등록된 레시피 에셋이 없습니다.");
                return;
            }

            var matchResult = ComboMatcher.Match(drawnShapes, drawingDatabase.recipes);
            OnSpellMatched(matchResult.Item1, matchResult.Item2);
        }

        protected virtual void OnSpellMatched(string matchedSpell, float averageScore)
        {
            if (matchedSpell.Contains("실패") || matchedSpell.Contains("아무것도"))
            {
                Debug.Log($"<color=red>퍼석... {matchedSpell} (Score: {averageScore:F2})</color>");
            }
            else
            {
                string rank = averageScore >= 0.85f ? "[대성공]" : "[성공]";
                Debug.Log($"<color=cyan>✨ [마법 발동 / 완성] {rank} {matchedSpell} (Score: {averageScore:F2}) ✨</color>");
            }
            ClearDrawing();
        }

        protected Rect GetBounds(Stroke stroke)
        {
            if (stroke.points.Count == 0) return new Rect();
            float minX = float.MaxValue, minY = float.MaxValue;
            float maxX = float.MinValue, maxY = float.MinValue;
            foreach (var p in stroke.points)
            {
                if (p.x < minX) minX = p.x;
                if (p.x > maxX) maxX = p.x;
                if (p.y < minY) minY = p.y;
                if (p.y > maxY) maxY = p.y;
            }
            return new Rect(minX, minY, maxX - minX, maxY - minY);
        }

        public virtual void ClearDrawing()
        {
            currentDrawingData.Clear();
            drawnShapes.Clear();
            foreach (Transform child in transform)
            {
                Destroy(child.gameObject);
            }
        }

        protected virtual void PrintCurrentStats()
        {
            Debug.Log($"<color=yellow>[누적 도형] 현재 그려진 도형 수: {drawnShapes.Count}</color>");
        }

        // 인벤토리에서 스크롤 직접 선택
        public virtual void SelectScroll(int index)
        {
            if (index >= 0 && index < inventory.Count && inventory[index] is Item_Scroll)
            {
                selectedScrollIndex = index;
            }
        }

        // 인벤토리에서 잉크 직접 선택
        public virtual void SelectInk(int index)
        {
            if (index >= 0 && index < inventory.Count && inventory[index] is Item_Ink)
            {
                selectedInkIndex = index;
            }
        }

        protected void ConsumeInkBottle()
        {
            if (selectedInkIndex == -1 || inventory.Count <= selectedInkIndex) return;
            Item_Ink ink = inventory[selectedInkIndex] as Item_Ink;
            if (ink != null && ink.currentAmount <= 0)
            {
                int idxToRemove = selectedInkIndex;
                InventoryManager.Instance.RemoveItem(ink);
                
                if (selectedScrollIndex > idxToRemove) selectedScrollIndex--;
                
                selectedInkIndex = -1;
            }
        }

        // 인벤토리에서 다음/이전 스크롤 찾기
        public void FindNextScroll(int dir)
        {
            if (inventory.Count == 0) { selectedScrollIndex = -1; return; }
            int startIdx = selectedScrollIndex == -1 ? 0 : selectedScrollIndex;
            int idx = startIdx;
            
            for (int i = 0; i < inventory.Count; i++)
            {
                idx += dir;
                if (idx >= inventory.Count) idx = 0;
                if (idx < 0) idx = inventory.Count - 1;

                if (inventory[idx] is Item_Scroll)
                {
                    selectedScrollIndex = idx;
                    return;
                }
            }
            selectedScrollIndex = -1; // 스크롤이 아예 없음
        }

        // 인벤토리에서 다음/이전 잉크 찾기
        public void FindNextInk(int dir)
        {
            if (inventory.Count == 0) { selectedInkIndex = -1; return; }
            int startIdx = selectedInkIndex == -1 ? 0 : selectedInkIndex;
            int idx = startIdx;
            
            for (int i = 0; i < inventory.Count; i++)
            {
                idx += dir;
                if (idx >= inventory.Count) idx = 0;
                if (idx < 0) idx = inventory.Count - 1;

                if (inventory[idx] is Item_Ink)
                {
                    selectedInkIndex = idx;
                    return;
                }
            }
            selectedInkIndex = -1;
        }

        // 인벤토리에서 펜 직접 선택
        public virtual void SelectPen(int index)
        {
            if (index >= 0 && index < inventory.Count && inventory[index] is Item_Pen)
            {
                selectedPenIndex = index;
            }
        }

        // 인벤토리에서 다음/이전 펜 찾기
        public void FindNextPen(int dir)
        {
            if (inventory.Count == 0) { selectedPenIndex = -1; return; }
            int startIdx = selectedPenIndex == -1 ? 0 : selectedPenIndex;
            int idx = startIdx;
            
            for (int i = 0; i < inventory.Count; i++)
            {
                idx += dir;
                if (idx >= inventory.Count) idx = 0;
                if (idx < 0) idx = inventory.Count - 1;

                if (inventory[idx] is Item_Pen)
                {
                    selectedPenIndex = idx;
                    return;
                }
            }
            selectedPenIndex = -1;
        }

        protected virtual void RefillPen()
        {
            isPenReady = false; // 밖으로 나가면 그리기 불가

            if (selectedPenIndex != -1 && selectedPenIndex < inventory.Count)
            {
                Item_Pen pen = inventory[selectedPenIndex] as Item_Pen;
                if (pen != null && pen.PenData != null && pen.PenData.consumesMana)
                {
                    // 마나 소모 펜은 잉크 리필 프로세스를 거치지 않고 즉시 그리기 가능
                    isPenReady = true;
                    return;
                }
            }

            if (inkBottleVisual == null) {
                Debug.LogWarning("[RefillPen] inkBottleVisual이 할당되지 않아 잉크병 위치로 이동할 수 없습니다! 인스펙터를 확인해주세요.");
            }

            isRefilling = true;

            Image penImage = penVisual != null ? penVisual.GetComponent<Image>() : null;
            if (penImage != null && penImage.material != null && penImage.material.HasProperty("_DissolveAmount"))
            {
                Material mat = penImage.material;
                mat.DOKill();
                mat.DOFloat(1f, "_DissolveAmount", 0.5f).OnComplete(() => {
                    
                    TryExecuteRefillLogic();
                    
                    if (penVisual != null && inkBottleVisual != null)
                    {
                        penVisual.position = inkBottleVisual.position;
                    }
                    
                    mat.DOFloat(0f, "_DissolveAmount", 0.5f).OnComplete(() => {
                        isPenReady = true;
                    });
                });
            }
            else
            {
                Debug.LogWarning("[RefillPen] 펜 이미지에 디졸브 머티리얼이 없어 연출 없이 즉시 이동합니다.");
                TryExecuteRefillLogic();
                if (penVisual != null && inkBottleVisual != null)
                {
                    penVisual.position = inkBottleVisual.position;
                }
                isPenReady = true;
            }
        }

        protected virtual void ReturnPenToMouse()
        {
            Image penImage = penVisual != null ? penVisual.GetComponent<Image>() : null;
            if (penImage != null && penImage.material != null && penImage.material.HasProperty("_DissolveAmount"))
            {
                isRefilling = true;
                isPenReady = false; // 아직 그리기 불가
                Material mat = penImage.material;
                mat.DOKill();
                
                mat.DOFloat(1f, "_DissolveAmount", 0.3f).OnComplete(() => {
                    
                    if (penVisual != null)
                        penVisual.position = Input.mousePosition;
                        
                    isRefilling = false; // 마우스를 부드럽게 따라감
                    mat.DOFloat(0f, "_DissolveAmount", 0.3f).OnComplete(() => {
                        isPenReady = true; // 완전히 나타나면 그리기 허용!
                    });
                });
            }
            else
            {
                isRefilling = false;
                isPenReady = true;
            }
        }

        private void TryExecuteRefillLogic()
        {
            if (selectedPenIndex == -1 || selectedPenIndex >= inventory.Count) return;
            Item_Pen pen = inventory[selectedPenIndex] as Item_Pen;
            if (pen == null || (pen.PenData != null && pen.PenData.consumesMana)) return;

            if (selectedInkIndex == -1 || selectedInkIndex >= inventory.Count) return;
            Item_Ink ink = inventory[selectedInkIndex] as Item_Ink;
            if (ink == null || ink.currentAmount <= 0) return;

            float maxCap = pen.PenData != null ? pen.PenData.maxInkCapacity : 0f;
            float amountNeeded = maxCap - pen.currentInkCapacity;
            if (amountNeeded <= 0) return;

            ExecuteRefillLogic(pen, ink, amountNeeded);
        }

        private void ExecuteRefillLogic(Item_Pen pen, Item_Ink ink, float amountNeeded)
        {
            if (ink.currentAmount >= amountNeeded)
            {
                ink.currentAmount -= amountNeeded;
                pen.currentInkCapacity += amountNeeded;
            }
            else
            {
                pen.currentInkCapacity += ink.currentAmount;
                ink.currentAmount = 0;
            }


            if (ink.currentAmount <= 0)
            {
                Debug.LogWarning("[그리기] 잉크병이 바닥났습니다! 병이 버려집니다.");
                ConsumeInkBottle();
            }
        }

        protected virtual void OnGUI()
        {
            if (drawnShapes == null || drawnShapes.Count == 0) return;

            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.fontSize = 20;
            style.alignment = TextAnchor.MiddleLeft;
            style.richText = true; // 리치 텍스트 활성화

            float startX = 20f;
            float startY = Screen.height * 0.4f;

            // 가독성을 위해 배경 박스 하나 깔아주기
            GUI.Box(new Rect(startX - 10, startY - 10, 300, 40 + (drawnShapes.Count * 30)), "");

            GUI.Label(new Rect(startX, startY, 200, 30), "<color=white>--- Drawn Shapes ---</color>", style);
            
            int displayIndex = 0;
            foreach (var shape in drawnShapes)
            {
                string displayName = shape.Name;
                string scoreText = $" : {shape.Accuracy:F2}";
                string colorTag = "<color=red>";

                // 이름이 Unknown이거나 정확도가 0.5 미만이면 실패로 간주
                bool isFailed = shape.Name == "Unknown" || shape.Name == "None" || shape.Accuracy < 0.5f;

                if (isFailed)
                {
                    displayName = "판정 실패";
                    scoreText = ""; // 실패 시 점수는 숨기거나 표시 안함 (어떤 도형으로 판정 실패했는지 숨김)
                    colorTag = "<color=gray>";
                }
                else if (shape.Accuracy >= 0.85f)
                {
                    colorTag = "<color=yellow>"; // 대성공
                }
                else if (shape.Accuracy >= 0.5f)
                {
                    colorTag = "<color=green>"; // 성공
                }

                GUI.Label(new Rect(startX, startY + 30 + (displayIndex * 30), 300, 30), 
                    $"{colorTag}[{displayIndex + 1}] {displayName}{scoreText}</color>", style);
                
                displayIndex++;
            }
        }
    }
}
