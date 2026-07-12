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
        FloatingEffect scrollFloatingEffect;

        [Header("Settings")]
        public Camera mainCamera;

        public DrawingData currentDrawingData { get; protected set; }
        protected DrawingLine currentLine;
        protected bool isDrawing = false;
        protected bool isRefilling = false;
        protected bool isOutsideArea = false;
        protected bool isPenReady = false;

        protected DrawingDatabase drawingDatabase;

        [Header("Pen Visuals")]
        public PenController penController;
        [Header("Ink Controller")]
        public InkController inkController;

        protected virtual void Awake()
        {
            ShapeRecognizer.EnsureInitialized();
        }

        protected virtual void Start()
        {
            if (mainCamera == null)
                mainCamera = Camera.main;

            currentDrawingData = new DrawingData();
            drawingDatabase = DrawingDatabase.Instance;
            // 처음에 펜을 잉크통에 위치시키고 바깥에 있는 것으로 처리
            if (penController != null && inkController != null && inkController.inkBottleVisual != null)
            {
                penController.GoToInkBottle(inkController.inkBottleVisual, null, () => {
                    isPenReady = true;
                });
            }
            isOutsideArea = true;

            // 시작 시 잉크가 비어있을 수 있으므로 충전 한 번 시도
            if (inkController != null && penController != null)
            {
                inkController.TryRefillPen(penController.CurrentPen);
            }

            scrollFloatingEffect = drawingArea.parent.GetComponent<FloatingEffect>();

            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.OnInkEquipped += HandleInkEquipped;
                InventoryManager.Instance.OnPenEquipped += HandlePenEquipped;
            }
        }

        private void HandleInkEquipped(Item_Ink ink)
        {
            if (penController != null && penController.CurrentPen != null && ink != null)
            {
                if (inkController != null) inkController.TryRefillPen(penController.CurrentPen);
            }
        }

        private void HandlePenEquipped(Item_Pen pen)
        {
            if (inkController != null && inkController.CurrentInk != null && pen != null)
            {
                inkController.TryRefillPen(pen);
            }
        }

        protected virtual void OnDestroy()
        {
            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.OnInkEquipped -= HandleInkEquipped;
                InventoryManager.Instance.OnPenEquipped -= HandlePenEquipped;
            }
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
                        if (scrollFloatingEffect != null) scrollFloatingEffect.ResumeFloating();
                        RefillPen();
                    }
                        
                    return;
                }
                else
                {
                    if (isOutsideArea)
                    {
                        isOutsideArea = false;
                        if (scrollFloatingEffect != null) scrollFloatingEffect.PauseFloating();
                        ReturnPenToMouse();
                    }
                }
            }

            if (penController != null && !isRefilling)
            {
                penController.TrackMouse(mainCamera);
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
            if (!isPenReady || isRefilling || isOutsideArea) 
            {
                return; 
            }

            Item_Scroll currentScroll = InventoryManager.Instance != null ? InventoryManager.Instance.EquippedScroll : null;
            if (currentScroll == null || !currentScroll.isEmpty)
            {
                return;
            }

            // 펜 확인
            Item_Pen currentPen = penController != null ? penController.CurrentPen : null;
            if (penController == null || !penController.CanDraw())
            {
                string warnMsg = currentPen != null && currentPen.PenData != null && currentPen.PenData.consumesMana 
                    ? "[그리기 실패] 마력이 부족하여 마나 펜을 사용할 수 없습니다." 
                    : "[그리기 실패] 펜을 선택하지 않았거나 펜에 잉크가 없습니다. (스크롤 밖으로 나가서 충전하세요)";
                Debug.LogWarning(warnMsg);
                return;
            }

            isDrawing = true;
            if (penController != null) penController.PlayDrawAnimation();
            
            GameObject lineObj = Instantiate(drawingDatabase.linePrefab, drawingArea);
            currentLine = lineObj.GetComponent<DrawingLine>();

            // 장착된 잉크의 색상으로 선 색상 변경
            Color lineColor = Color.black;
            if (inkController != null && penController != null)
            {
                lineColor = inkController.GetLineColor(penController.CurrentPen);
            }
            currentLine.SetColor(lineColor);

            UpdateStroke();
        }

        protected virtual void UpdateStroke()
        {
            if (!isDrawing || currentLine == null) return;

            Vector3 mousePos = Input.mousePosition;
            if (mainCamera == null) Debug.LogError("[UpdateStroke] mainCamera가 null입니다!");
            
            mousePos.z = Mathf.Abs(mainCamera.transform.position.z);
            Vector3 worldPos = mainCamera.ScreenToWorldPoint(mousePos);

            // LineRenderer가 로컬 좌표계를 사용하므로 월드 좌표를 로컬 좌표로 변환합니다.
            Vector3 localPos = currentLine.transform.InverseTransformPoint(worldPos);
            Vector2 newPoint = new Vector2(localPos.x, localPos.y);

            if (currentLine.currentStroke == null) Debug.LogError("[UpdateStroke] currentStroke가 null입니다!");
            int prevCount = currentLine.currentStroke.points.Count;

            if (ShouldConsumeInk() && penController != null)
            {
                Item_Pen currentPen = penController.CurrentPen;
                if (!penController.CanDraw())
                {
                    EndStroke();
                    string warnMsg = currentPen != null && currentPen.PenData != null && currentPen.PenData.consumesMana
                        ? "[그리기] 플레이어 마력이 바닥나 그릴 수 없습니다!"
                        : "[그리기] 펜의 잉크가 바닥났습니다! 스크롤 밖으로 나가 잉크를 충전하세요.";
                    Debug.LogWarning(warnMsg);
                    return;
                }

                penController.ConsumeResource();
            }

            currentLine.AddPoint(newPoint);
        }


        protected virtual bool ShouldConsumeInk()
        {
            return true;
        }

        protected virtual bool CanDrawWithPen(Item_Pen pen)
        {
            if (pen == null) return false;
            if (pen.PenData != null && pen.PenData.consumesMana) return true;
            return pen.currentInkCapacity > 0f;
        }

        protected virtual void ConsumePenResource(Item_Pen pen)
        {
            if (pen == null) return;
            if (pen.PenData != null && pen.PenData.consumesMana) return;
            float rate = pen.PenData != null ? pen.PenData.inkConsumptionRate : 0f;
            pen.currentInkCapacity -= rate * Time.deltaTime;
            if (pen.currentInkCapacity < 0) pen.currentInkCapacity = 0;
        }

        protected virtual async void EndStroke()
        {
            if (!isDrawing) return;
            isDrawing = false;
            if (penController != null) penController.PlayIdleAnimation(true); // 마우스 클릭을 뗄 때는 부드럽게 복귀
            
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
            
            if (drawingArea != null)
            {
                foreach (Transform child in drawingArea)
                {
                    if (child.GetComponent<DrawingLine>() != null)
                    {
                        Destroy(child.gameObject);
                    }
                }
            }
            else
            {
                foreach (Transform child in transform)
                {
                    if (child.GetComponent<DrawingLine>() != null)
                    {
                        Destroy(child.gameObject);
                    }
                }
            }
        }

        protected virtual void PrintCurrentStats()
        {
            Debug.Log($"<color=yellow>[누적 도형] 현재 그려진 도형 수: {drawnShapes.Count}</color>");
        }

        protected virtual void RefillPen()
        {
            isPenReady = false; 
            
            Item_Pen pen = penController != null ? penController.CurrentPen : null;
            if (pen != null && pen.PenData != null && pen.PenData.consumesMana)
            {
                isPenReady = true;
                return;
            }

            isRefilling = true;

            if (penController != null && inkController != null)
            {
                penController.GoToInkBottle(inkController.inkBottleVisual, 
                    () => { inkController.TryRefillPen(penController.CurrentPen); }, 
                    () => { isPenReady = true; }
                );
            }
            else
            {
                if (inkController != null && penController != null) inkController.TryRefillPen(penController.CurrentPen);
                isPenReady = true;
                isRefilling = false;
            }
        }

        protected virtual void ReturnPenToMouse()
        {
            isRefilling = true;
            isPenReady = false;

            if (penController != null)
            {
                penController.ReturnToMouse(mainCamera, () => {
                    isRefilling = false;
                    isPenReady = true;
                });
            }
            else
            {
                isRefilling = false;
                isPenReady = true;
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
