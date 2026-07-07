using UnityEngine;
using System.Collections.Generic;
using Magic.Inventory;

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

        protected DrawingDatabase drawingDatabase;

        // 글로벌 인벤토리 시스템 참조
        public List<ItemData> inventory => InventoryManager.Instance != null ? InventoryManager.Instance.items : new List<ItemData>();
        
        public int selectedScrollIndex = -1;
        public int selectedInkIndex = -1;
        public float inkDrainRate = 10f; // 잉크 소모 속도 (초당)

        protected virtual void Start()
        {
            if (mainCamera == null)
                mainCamera = Camera.main;

            currentDrawingData = new DrawingData();
            drawingDatabase = DrawingDatabase.Instance;

            Debug.Log("DrawingManager initialized.");
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
                    return;
                }
            }

            if (Input.GetMouseButtonDown(0))
            {
                StartStroke();
            }
            else if (Input.GetMouseButton(0))
            {
                UpdateStroke();
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
        }

        protected virtual void StartStroke()
        {
            if (drawingDatabase == null || drawingDatabase.linePrefab == null)
            {
                Debug.LogError("LinePrefab is not assigned in DrawingDatabase.");
                return;
            }

            // 스크롤 확인
            if (selectedScrollIndex == -1 || inventory.Count <= selectedScrollIndex)
            {
                Debug.LogWarning("[그리기] 스크롤을 먼저 선택해주세요.");
                return;
            }
            Item_Scroll currentScroll = inventory[selectedScrollIndex] as Item_Scroll;
            if (currentScroll == null || !currentScroll.isEmpty)
            {
                Debug.LogWarning("[그리기] 유효한 빈 스크롤이 아닙니다.");
                return;
            }

            // 잉크 확인
            Item_Ink currentInk = selectedInkIndex != -1 && selectedInkIndex < inventory.Count ? inventory[selectedInkIndex] as Item_Ink : null;
            if (currentInk == null || currentInk.currentAmount <= 0)
            {
                Debug.LogWarning("[그리기] 잉크를 선택하지 않았거나 잔량이 부족합니다.");
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
            mousePos.z = Mathf.Abs(mainCamera.transform.position.z);
            Vector3 worldPos = mainCamera.ScreenToWorldPoint(mousePos);

            Vector2 newPoint = new Vector2(worldPos.x, worldPos.y);

            // 잉크 소모 처리 (마우스를 움직인 거리에 비례 또는 시간에 비례)
            // 여기서는 시간에 비례하여 간단히 잉크 소모
            Item_Ink currentInk = selectedInkIndex != -1 && selectedInkIndex < inventory.Count ? inventory[selectedInkIndex] as Item_Ink : null;
            if (currentInk == null || currentInk.currentAmount <= 0)
            {
                EndStroke();
                Debug.LogWarning("[그리기] 잉크가 바닥났습니다! 병이 버려집니다.");
                ConsumeInkBottle();
                return;
            }

            currentInk.currentAmount -= inkDrainRate * Time.deltaTime;
            currentLine.AddPoint(newPoint);
        }

        protected virtual void EndStroke()
        {
            if (!isDrawing) return;
            isDrawing = false;

            if (currentLine != null && currentLine.currentStroke.points.Count > 0)
            {
                currentDrawingData.AddStroke(currentLine.currentStroke);

                DrawingData singleStrokeData = new DrawingData();
                singleStrokeData.AddStroke(currentLine.currentStroke);

                RecognizerResult result = ShapeRecognizer.Recognize(singleStrokeData);
                string shapeName = result.Name;
                float score = result.Score;
                
                Rect bounds = GetBounds(currentLine.currentStroke);
                Vector2 center = new Vector2(bounds.center.x, bounds.center.y);
                
                drawnShapes.Add(new DrawnShape(shapeName, bounds, center, score));

                PrintCurrentStats();
            }
            currentLine = null;
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
                Debug.Log($"[그리기] 스크롤 선택됨: {inventory[index].itemName}");
            }
        }

        // 인벤토리에서 잉크 직접 선택
        public virtual void SelectInk(int index)
        {
            if (index >= 0 && index < inventory.Count && inventory[index] is Item_Ink)
            {
                selectedInkIndex = index;
                Debug.Log($"[그리기] 잉크 선택됨: {inventory[index].itemName}");
            }
        }

        protected void ConsumeInkBottle()
        {
            if (selectedInkIndex == -1 || inventory.Count <= selectedInkIndex) return;
            Item_Ink ink = inventory[selectedInkIndex] as Item_Ink;
            if (ink != null && ink.currentAmount <= 0)
            {
                InventoryManager.Instance.RemoveItem(ink);
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

        protected virtual void OnGUI()
        {
            if (drawnShapes.Count == 0 || mainCamera == null) return;

            GUIStyle style = new GUIStyle();
            style.fontSize = 20;
            style.alignment = TextAnchor.MiddleCenter;

            foreach (var shape in drawnShapes)
            {
                if (shape.Name == "Unknown" || shape.Name == "None") continue;

                // 월드 좌표인 center를 스크린 좌표로 변환
                Vector3 screenPos = mainCamera.WorldToScreenPoint(new Vector3(shape.Center.x, shape.Center.y, 0));
                // GUI 좌표계는 좌상단이 (0,0)이므로 y축 반전
                float guiY = Screen.height - screenPos.y;

                // 점수에 따른 색상 지정
                if (shape.Accuracy >= 0.85f)
                    style.normal.textColor = Color.yellow; // 대성공
                else if (shape.Accuracy >= 0.5f)
                    style.normal.textColor = Color.green; // 성공
                else
                    style.normal.textColor = Color.red; // 실패

                Rect labelRect = new Rect(screenPos.x - 100, guiY - 20, 200, 40);
                GUI.Label(labelRect, $"{shape.Name}\n{shape.Accuracy:F2}", style);
            }
        }
    }
}