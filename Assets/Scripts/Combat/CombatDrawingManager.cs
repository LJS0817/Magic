using UnityEngine;
using System.Collections.Generic;
using Magic.Drawing;

namespace Magic.Combat
{
    public class CombatDrawingManager : MonoBehaviour
    {
        [Header("References")]
        public PlayerController player;
        public Camera mainCamera;
        public SpellRecipeAsset[] recipes;
        public GameObject linePrefab;

        [Header("Resources")]
        public float maxMana = 100f;
        public float currentMana = 100f;
        public float manaCostPerSpell = 30f;

        public float maxInk = 100f;
        public float currentInk = 100f;
        public float inkDrainRate = 10f; // 잉크 소모 속도 (초당)

        [Header("Scroll Economy & Inventory")]
        public List<ScrollItem> inventory = new List<ScrollItem>();
        public int selectedScrollIndex = 0;

        // 상태 관리
        private bool isScrollUIVisible = false;
        private bool isDrawing = false;
        
        // 그리기 데이터
        private DrawingData currentDrawingData;
        private DrawingLine currentLine;
        private List<DrawnShape> drawnShapes = new List<DrawnShape>();

        // 화면 우측 하단의 스크롤 캔버스 영역 (화면 비율에 맞게 OnGUI에서 계산)
        private Rect scrollCanvasRect;

        void Start()
        {
            if (player == null) player = FindObjectOfType<PlayerController>();
            if (mainCamera == null) mainCamera = Camera.main;
            
            currentDrawingData = new DrawingData();

            // 테스트용 초기 인벤토리 셋팅
            if (inventory.Count == 0)
            {
                inventory.Add(new ScrollItem { isEmpty = true, currentDurability = 5 });
                inventory.Add(new ScrollItem { isEmpty = true, currentDurability = 5 });
                inventory.Add(new ScrollItem { isEmpty = false, spellName = "Meteor", currentDurability = 5 }); // 테스트용 완성 스크롤
            }
        }

        void Update()
        {
            // 1. 발사 처리 (스크롤 모드가 아니고, 장전 상태일 때 좌클릭)
            if (!isScrollUIVisible && player.isArmed && Input.GetMouseButtonDown(0))
            {
                if (currentMana >= manaCostPerSpell)
                {
                    currentMana -= manaCostPerSpell;
                    Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
                    mouseWorldPos.z = 0;
                    player.CastArmedSpell(mouseWorldPos);
                }
                else
                {
                    Debug.Log("<color=red>[전투] 마나가 부족하여 발사할 수 없습니다!</color>");
                }
                return;
            }

            // 2. 스크롤 모드 토글 (Space)
            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (player.isArmed)
                {
                    Debug.Log("[전투] 장전된 마법을 취소하고 빈 스크롤을 꺼냅니다.");
                    player.isArmed = false;
                    player.armedSpellName = "";
                }

                isScrollUIVisible = !isScrollUIVisible;
                if (isScrollUIVisible)
                {
                    if (inventory.Count == 0)
                    {
                        Debug.LogWarning("[전투] 인벤토리에 스크롤이 하나도 없습니다!");
                        isScrollUIVisible = false;
                    }
                    else
                    {
                        Debug.Log($"[전투] 스크롤 펼침! (인벤토리 총 {inventory.Count}개)");
                    }
                }
                else
                {
                    ClearDrawing();
                }
            }

            // 3. 스크롤 모드가 켜져 있을 때의 로직
            if (isScrollUIVisible && inventory.Count > 0)
            {
                // 마우스 휠로 스크롤 전환
                float scrollWheel = Input.GetAxis("Mouse ScrollWheel");
                if (scrollWheel != 0f)
                {
                    if (scrollWheel > 0f) selectedScrollIndex--; // 위로 굴리면 이전
                    if (scrollWheel < 0f) selectedScrollIndex++; // 아래로 굴리면 다음
                    
                    // 인덱스 순환
                    if (selectedScrollIndex < 0) selectedScrollIndex = inventory.Count - 1;
                    if (selectedScrollIndex >= inventory.Count) selectedScrollIndex = 0;
                    
                    ClearDrawing(); // 스크롤을 넘기면 그리던 내용은 취소됨
                    Debug.Log($"[전투] 스크롤 전환: {selectedScrollIndex + 1}/{inventory.Count}");
                }

                ScrollItem currentScroll = inventory[selectedScrollIndex];

                // 스크롤 캔버스 영역 계산 (화면 우측 하단)
                float canvasWidth = Screen.width * 0.4f;
                float canvasHeight = Screen.height * 0.6f;
                scrollCanvasRect = new Rect(Screen.width - canvasWidth - 20, Screen.height - canvasHeight - 20, canvasWidth, canvasHeight);
                
                Vector2 mousePosGUI = new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y);

                // 빈 스크롤일 때만 그리기 허용
                if (currentScroll.isEmpty)
                {
                    if (Input.GetMouseButtonDown(0) && scrollCanvasRect.Contains(mousePosGUI))
                    {
                        StartStroke();
                    }
                    else if (Input.GetMouseButton(0) && isDrawing)
                    {
                        UpdateStroke(mousePosGUI);
                    }
                    else if (Input.GetMouseButtonUp(0) && isDrawing)
                    {
                        EndStroke();
                    }
                }

                // 4. 장전 처리 (우클릭)
                if (Input.GetMouseButtonDown(1))
                {
                    ArmAndCloseScroll();
                }
            }
        }

        private void StartStroke()
        {
            if (currentInk <= 0)
            {
                Debug.LogWarning("[전투] 잉크가 부족하여 그릴 수 없습니다!");
                return;
            }

            isDrawing = true;
            GameObject lineObj = Instantiate(linePrefab, this.transform);
            currentLine = lineObj.GetComponent<DrawingLine>();
            currentLine.lineRenderer.startWidth = 0.05f;
            currentLine.lineRenderer.endWidth = 0.05f;
            currentLine.lineRenderer.startColor = Color.black;
            currentLine.lineRenderer.endColor = Color.black;
        }

        private void UpdateStroke(Vector2 mousePosGUI)
        {
            if (!scrollCanvasRect.Contains(mousePosGUI))
            {
                EndStroke(); // 영역 벗어나면 그리기 종료
                return;
            }

            if (currentInk <= 0)
            {
                EndStroke();
                Debug.LogWarning("[전투] 잉크가 고갈되었습니다!");
                return;
            }

            currentInk -= inkDrainRate * Time.deltaTime;

            Vector3 mousePos = Input.mousePosition;
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
                currentDrawingData.AddStroke(currentLine.currentStroke);

                DrawingData singleStrokeData = new DrawingData();
                singleStrokeData.AddStroke(currentLine.currentStroke);
                
                string shapeName = ShapeRecognizer.Recognize(singleStrokeData);
                
                // TODO: 정확도(Accuracy) 추출 및 스코어 표시 기능은 $P 알고리즘 내부 수정 후 반영할 예정 (임시 100점 처리)
                float accuracy = 100f; 

                DrawnShape newShape = new DrawnShape
                {
                    Name = shapeName,
                    Bounds = GetBounds(currentLine.currentStroke),
                    Accuracy = accuracy
                };
                newShape.Center = newShape.Bounds.center;
                drawnShapes.Add(newShape);
                
                Debug.Log($"[전투 캔버스] 도형 인식됨: {shapeName} (정확도: {accuracy:F1}점)");
            }
        }

        private void ArmAndCloseScroll()
        {
            if (inventory.Count == 0) return;
            ScrollItem currentScroll = inventory[selectedScrollIndex];

            if (currentScroll.isEmpty)
            {
                // 빈 스크롤인 경우: 그려진 도형들을 매칭
                if (drawnShapes.Count == 0)
                {
                    isScrollUIVisible = false;
                    return;
                }

                string matchedSpell = ComboMatcher.Match(drawnShapes, recipes);
                
                if (matchedSpell.Contains("실패") || matchedSpell.Contains("아무것도"))
                {
                    Debug.Log($"<color=red>[전투 캔버스] 매칭 실패! 마법이 흩어집니다.</color>");
                }
                else
                {
                    player.ArmSpell(matchedSpell);
                    ConsumeScrollDurability(currentScroll);
                }
            }
            else
            {
                // 완성된 스크롤인 경우: 즉시 장전하고 빈 스크롤로 반환
                player.ArmSpell(currentScroll.spellName);
                
                // 마법이 발동된 완성 스크롤은 빈 스크롤로 되돌아감 (내구도는 유지되거나 깎일 수 있음)
                currentScroll.isEmpty = true;
                currentScroll.spellName = "";
                Debug.Log($"[전투] 완성된 스크롤을 사용하여 마법을 장전했습니다. (빈 스크롤로 반환됨)");
            }

            isScrollUIVisible = false;
            ClearDrawing();
        }

        private void ConsumeScrollDurability(ScrollItem item)
        {
            item.currentDurability--;
            if (item.currentDurability <= 0)
            {
                Debug.LogWarning("[전투] 빈 스크롤이 파괴되었습니다!");
                inventory.Remove(item);
                // 삭제 시 인덱스 조정
                if (selectedScrollIndex >= inventory.Count)
                    selectedScrollIndex = inventory.Count - 1;
            }
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

        private void ClearDrawing()
        {
            currentDrawingData.Clear();
            drawnShapes.Clear();
            foreach (Transform child in transform)
            {
                Destroy(child.gameObject);
            }
        }

        void OnGUI()
        {
            // 상단 자원 바
            GUI.color = Color.white;
            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.fontSize = 20;
            
            GUI.Label(new Rect(20, 20, 300, 30), $"Mana: {currentMana:F0} / {maxMana}", style);
            GUI.Label(new Rect(20, 50, 300, 30), $"Ink: {currentInk:F0} / {maxInk}", style);

            // 조작법 힌트
            GUI.Label(new Rect(20, 90, 400, 100), 
                "[Space] 스크롤 열기/닫기\n" +
                "[스크롤 모드] 마우스 휠: 인벤토리 스크롤 전환\n" +
                "[일반 모드] 좌클릭: 장전된 마법 발사 (마나소모)");

            // 인벤토리 (우측 상단에 간단히 표시)
            GUI.Label(new Rect(Screen.width - 250, 20, 200, 30), $"--- Scroll Inventory ---", style);
            for (int i = 0; i < inventory.Count; i++)
            {
                ScrollItem item = inventory[i];
                string prefix = (i == selectedScrollIndex && isScrollUIVisible) ? "▶ " : "  ";
                string name = item.isEmpty ? $"빈 스크롤 (내구도:{item.currentDurability})" : $"완성 스크롤 [{item.spellName}]";
                GUI.Label(new Rect(Screen.width - 250, 50 + (i * 30), 250, 30), prefix + name, style);
            }

            // 스크롤 캔버스 그리기
            if (isScrollUIVisible && inventory.Count > 0)
            {
                ScrollItem currentScroll = inventory[selectedScrollIndex];

                // 약간 노란색 바탕 (양피지 느낌)
                GUI.color = currentScroll.isEmpty ? new Color(1f, 0.9f, 0.7f, 0.8f) : new Color(0.7f, 0.9f, 1f, 0.8f);
                GUI.DrawTexture(scrollCanvasRect, Texture2D.whiteTexture);
                
                GUI.color = Color.black;
                if (currentScroll.isEmpty)
                {
                    GUI.Label(new Rect(scrollCanvasRect.x + 10, scrollCanvasRect.y + 10, 200, 30), $"[빈 스크롤] 좌클릭: 그리기, 우클릭: 장전");
                }
                else
                {
                    GUIStyle hugeStyle = new GUIStyle(GUI.skin.label);
                    hugeStyle.fontSize = 40;
                    hugeStyle.alignment = TextAnchor.MiddleCenter;
                    GUI.Label(new Rect(scrollCanvasRect.x, scrollCanvasRect.y, scrollCanvasRect.width, scrollCanvasRect.height), 
                        $"{currentScroll.spellName}\n<size=20>우클릭으로 즉시 장전!</size>", hugeStyle);
                }
            }
        }
    }

    [System.Serializable]
    public class ScrollItem
    {
        public bool isEmpty = true;
        public string spellName = "";
        public int currentDurability = 5;
    }
}
