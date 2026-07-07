#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Magic.Drawing;

namespace Magic.EditorTools
{
    public class DrawingSetupEditor : EditorWindow
    {
        [MenuItem("Magic 게임 설정/마법진 그리기 시스템 셋업")]
        public static void SetupDrawingSystem()
        {
            // 1. 카메라 설정 (2D용)
            Camera cam = Camera.main;
            if (cam == null)
            {
                GameObject camObj = new GameObject("Main Camera");
                cam = camObj.AddComponent<Camera>();
                camObj.tag = "MainCamera";
            }
            cam.orthographic = true;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.2f, 0.2f, 0.2f); // 배경색

            // 2. LinePrefab 자동 생성
            string prefabPath = "Assets/LinePrefab.prefab";
            GameObject existingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (existingPrefab == null)
            {
                GameObject lineObj = new GameObject("LinePrefab");
                LineRenderer lr = lineObj.AddComponent<LineRenderer>();
                lr.startWidth = 0.1f;
                lr.endWidth = 0.1f;
                lr.useWorldSpace = true;
                lr.material = new Material(Shader.Find("Sprites/Default"));
                lr.startColor = Color.cyan;
                lr.endColor = Color.blue;
                lr.numCapVertices = 5;
                lr.numCornerVertices = 5;

                lineObj.AddComponent<DrawingLine>();

                PrefabUtility.SaveAsPrefabAsset(lineObj, prefabPath);
                DestroyImmediate(lineObj);
                Debug.Log("LinePrefab이 Assets/LinePrefab.prefab 에 생성되었습니다.");
            }

            // 3. UI 캔버스 및 스크롤 영역(그리기 구역) 생성
            Canvas canvas = Object.FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                GameObject canvasObj = new GameObject("Canvas");
                canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasObj.AddComponent<CanvasScaler>();
                canvasObj.AddComponent<GraphicRaycaster>();
            }

            RectTransform drawingArea = null;
            Transform scrollTransform = canvas.transform.Find("ScrollArea");
            if (scrollTransform == null)
            {
                GameObject scrollObj = new GameObject("ScrollArea");
                scrollObj.transform.SetParent(canvas.transform, false);
                Image img = scrollObj.AddComponent<Image>();
                img.color = new Color(0.9f, 0.85f, 0.7f, 0.8f); // 스크롤(양피지) 느낌의 반투명 색상
                
                drawingArea = scrollObj.GetComponent<RectTransform>();
                drawingArea.sizeDelta = new Vector2(600, 800); // 그리기 구역 크기
                drawingArea.anchoredPosition = Vector2.zero;
            }
            else
            {
                drawingArea = scrollTransform.GetComponent<RectTransform>();
            }

            // 4. EventSystem 자동 생성 (UI 상호작용용)
            if (Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                GameObject eventSystem = new GameObject("EventSystem");
                eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
                eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }

            // 5. DrawingManager 씬에 배치
            DrawingManager manager = Object.FindObjectOfType<DrawingManager>();
            if (manager == null)
            {
                GameObject managerObj = new GameObject("DrawingManager");
                manager = managerObj.AddComponent<DrawingManager>();
            }
            
            manager.drawingArea = drawingArea;
            manager.mainCamera = cam;

            // 6. DrawingDatabase 씬에 배치
            DrawingDatabase db = Object.FindObjectOfType<DrawingDatabase>();
            if (db == null)
            {
                GameObject dbObj = new GameObject("DrawingDatabase");
                db = dbObj.AddComponent<DrawingDatabase>();
            }
            
            // 프리팹은 이제 데이터베이스에서 관리
            db.linePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

            // 7. InventoryManager 씬에 배치
            Magic.Inventory.InventoryManager inv = Object.FindObjectOfType<Magic.Inventory.InventoryManager>();
            if (inv == null)
            {
                GameObject invObj = new GameObject("InventoryManager");
                invObj.AddComponent<Magic.Inventory.InventoryManager>();
            }

            Debug.Log("✅ [마법진 그리기 시스템] 셋업이 성공적으로 완료되었습니다! 플레이(Play) 버튼을 눌러 스크롤 영역 위에서 마우스로 그림을 그려보세요.");
        }
    }
}
#endif
