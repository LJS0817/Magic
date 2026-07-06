using UnityEngine;

namespace Magic.Drawing
{
    public class DrawingManager : MonoBehaviour
    {
        [Header("References")]
        public GameObject linePrefab;
        public RectTransform drawingArea; // 스크롤 UI 영역

        [Header("Settings")]
        public Camera mainCamera;

        [Header("Template Generator")]
        [Tooltip("체크하면 마우스를 뗄 때마다 아래의 이름으로 템플릿 파일이 자동 생성됩니다.")]
        public bool autoSaveTemplateOnRelease = false;
        [Tooltip("템플릿 저장에 사용될 이름입니다.")]
        public string shapeNameToSave = "MyNewShape";

        [Header("Magic Combo Stats")]
        public System.Collections.Generic.List<DrawnShape> drawnShapes = new System.Collections.Generic.List<DrawnShape>();

        [Header("Spell Recipes")]
        [Tooltip("매칭에 사용할 마법 레시피 에셋들을 넣어주세요.")]
        public SpellRecipeAsset[] recipes;

        [Header("Recipe Generator (Auto Create)")]
        [Tooltip("켜두면 스페이스바를 누를 때 마법을 쏘는 대신, 방금 그린 마법진을 분석해 새로운 레시피 에셋으로 자동 저장합니다.")]
        public bool recordRecipeMode = false;
        [Tooltip("자동 생성될 레시피의 이름입니다.")]
        public string newRecipeName = "MyAwesomeSpell";

        public DrawingData currentDrawingData { get; private set; }
        private DrawingLine currentLine;
        private bool isDrawing = false;

        private void Awake()
        {
            if (mainCamera == null)
                mainCamera = Camera.main;

            currentDrawingData = new DrawingData();

            Debug.Log("DrawingManager initialized.");
        }

        private void Update()
        {
            HandleDrawingInput();

            if (Input.GetKeyDown(KeyCode.Space))
            {
                CastMagicAndClear();
            }
        }

        private void CastMagicAndClear()
        {
            if (recordRecipeMode)
            {
                Debug.Log($"<color=orange>[Recipe Generator] '{newRecipeName}' 레시피 자동 생성을 시작합니다...</color>");
                RecipeGenerator.GenerateAndSaveRecipe(drawnShapes, newRecipeName);
            }
            else
            {
                Debug.Log($"<color=magenta>[Magic Cast] 펑! 매칭을 시작합니다. (그려진 도형 수: {drawnShapes.Count})</color>");
                
                if (recipes == null || recipes.Length == 0)
                {
                    Debug.LogWarning("[Combo Matcher] 등록된 레시피 에셋이 없습니다. 인스펙터에 레시피를 추가해주세요.");
                }
                else
                {
                    string matchedSpell = ComboMatcher.Match(drawnShapes, recipes);
                    if (matchedSpell.Contains("실패") || matchedSpell.Contains("아무것도"))
                    {
                        Debug.Log($"<color=red>퍼석... {matchedSpell}</color>");
                    }
                    else
                    {
                        Debug.Log($"<color=cyan>✨ [초필살기 발동] {matchedSpell} ✨</color>");
                    }
                }
            }
            
            ClearDrawing();
            drawnShapes.Clear();
        }

        private void PrintCurrentStats()
        {
            Debug.Log($"<color=yellow>[누적 도형] 현재 그려진 도형 수: {drawnShapes.Count}</color>");
        }

        private void HandleDrawingInput()
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
                Debug.Log("Start");
                StartStroke();
            }else if (Input.GetMouseButton(0))
            {
                UpdateStroke();
            }
            else if (Input.GetMouseButtonUp(0))
            {
                EndStroke();
            }
        }

        private void StartStroke()
        {
            if (linePrefab == null)
            {
                Debug.LogError("LinePrefab is not assigned in DrawingManager.");
                return;
            }

            isDrawing = true;
            GameObject lineObj = Instantiate(linePrefab, transform);
            currentLine = lineObj.GetComponent<DrawingLine>();

            UpdateStroke();
        }

        private void UpdateStroke()
        {
            if (!isDrawing || currentLine == null) return;

            Vector3 mousePos = Input.mousePosition;
            // 2D orthographic 카메라 기준, Z 거리를 카메라 위치에 맞춰 보정
            mousePos.z = Mathf.Abs(mainCamera.transform.position.z);
            Vector3 worldPos = mainCamera.ScreenToWorldPoint(mousePos);

            currentLine.AddPoint(new Vector2(worldPos.x, worldPos.y));
        }

        private void EndStroke()
        {
            if (!isDrawing) return;
            isDrawing = false;

            if (currentLine != null && currentLine.currentStroke.points.Count > 0)
            {
                // 그림 데이터를 전체 누적하지만, 인식은 '방금 그린 획'만 독립적으로 수행합니다.
                currentDrawingData.AddStroke(currentLine.currentStroke);

                DrawingData singleStrokeData = new DrawingData();
                singleStrokeData.AddStroke(currentLine.currentStroke);

                if (autoSaveTemplateOnRelease)
                {
                    // 자동 저장 모드일 때는 방금 그린 모양을 에셋으로 저장
                    ShapeRecognizer.SaveTemplateAsset(singleStrokeData, shapeNameToSave);
                    // 템플릿 양산 시 화면이 지저분해지지 않도록 바로바로 지워줍니다.
                    ClearDrawing();
                }
                else
                {
                    // 일반 모드일 때는 방금 그린 모양 인식
                    string shapeName = ShapeRecognizer.Recognize(singleStrokeData);
                    
                    Rect bounds = GetBounds(currentLine.currentStroke);
                    Vector2 center = new Vector2(bounds.center.x, bounds.center.y);
                    
                    drawnShapes.Add(new DrawnShape(shapeName, bounds, center));

                    PrintCurrentStats();
                }
            }
            currentLine = null;
        }

        private Rect GetBounds(Stroke stroke)
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

        public void ClearDrawing()
        {
            currentDrawingData.Clear();
            foreach (Transform child in transform)
            {
                Destroy(child.gameObject);
            }
        }
    }
}