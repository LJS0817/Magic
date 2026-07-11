using UnityEngine;
using System.Collections.Generic;

namespace Magic.Drawing
{
    public class ShapeRecognizer : MonoBehaviour
    {
        private static List<GestureTemplate> templates = new List<GestureTemplate>();
        private static bool initialized = false;

        // 스레드 안전성을 보장하기 위한 락 객체
        private static readonly object lockObj = new object();

        private void Awake()
        {
            EnsureInitialized();
        }

        public static void EnsureInitialized()
        {
            if (!initialized)
            {
                lock (lockObj)
                {
                    if (!initialized)
                    {
                        InitializeTemplates();
                    }
                }
            }
        }

        public static RecognizerResult Recognize(DrawingData data)
        {
            if (data == null || data.strokes.Count == 0) return new RecognizerResult { Name = "Unknown", Score = 0f };

            EnsureInitialized();

            // Convert DrawingData to Point array
            List<Point> pointsList = new List<Point>();
            for (int i = 0; i < data.strokes.Count; i++)
            {
                var stroke = data.strokes[i];
                foreach (var pt in stroke.points)
                {
                    // StrokeID를 1부터 시작하게 하여 구분
                    pointsList.Add(new Point(pt.x, pt.y, i + 1));
                }
            }

            // 백그라운드 스레드에서 안전하게 읽을 수 있도록 리스트의 복사본(배열)을 넘깁니다.
            List<GestureTemplate> templatesSnapshot;
            lock (lockObj)
            {
                templatesSnapshot = new List<GestureTemplate>(templates);
            }

            // Run $P Recognizer
            RecognizerResult result = PointCloudRecognizer.Classify(pointsList.ToArray(), templatesSnapshot);

            // 별(Star) 모양은 특성상 인식률이 떨어지기 쉬우므로 점수 보정치(+0.5)를 부여합니다.
            // 단, 원래 점수가 0.25 이하일 정도로 전혀 비슷하지 않다면 보정하지 않습니다.
            if (result.Name == "Star" && result.Score > 0.25f)
            {
                result.Score = UnityEngine.Mathf.Clamp01(result.Score + 0.5f);
            }

            if (result.Score < 0.5f)
            {
                Debug.Log($"<color=orange>[ShapeRecognizer] 인식 실패: {result.Name} (Score: {result.Score:F3} < 0.5)</color>");
                return new RecognizerResult { Name = "Unknown", Score = result.Score };
            }

            Debug.Log($"<color=green>[ShapeRecognizer] 획 인식 완료: {result.Name} (Score: {result.Score:F3})</color>");
            return result;
        }

        // 개발자 도구: 방금 그린 그림을 ScriptableObject 에셋 파일로 저장합니다.
        public static void SaveTemplateAsset(DrawingData data, string shapeName)
        {
#if UNITY_EDITOR
            if (data == null || data.strokes.Count == 0) return;

            List<Point> pointsList = new List<Point>();
            for (int i = 0; i < data.strokes.Count; i++)
            {
                var stroke = data.strokes[i];
                foreach (var pt in stroke.points)
                {
                    pointsList.Add(new Point(pt.x, pt.y, i + 1));
                }
            }

            Point[] normalized = PointCloudRecognizer.Normalize(pointsList.ToArray());
            
            GestureTemplateAsset asset = ScriptableObject.CreateInstance<GestureTemplateAsset>();
            asset.shapeName = shapeName;
            asset.points = normalized;

            string folderPath = "Assets/Resources/GestureTemplates";
            if (!System.IO.Directory.Exists(Application.dataPath + "/Resources/GestureTemplates"))
            {
                System.IO.Directory.CreateDirectory(Application.dataPath + "/Resources/GestureTemplates");
            }

            string assetPath = $"{folderPath}/{shapeName}_{System.DateTime.Now.Ticks}.asset";
            UnityEditor.AssetDatabase.CreateAsset(asset, assetPath);
            UnityEditor.AssetDatabase.SaveAssets();

            Debug.Log($"<color=cyan>[ShapeRecognizer] 새로운 템플릿 에셋 저장 완료: {assetPath}</color>");
            
            // 즉시 템플릿 목록에도 추가
            lock (lockObj)
            {
                templates.Add(new GestureTemplate(shapeName, normalized));
            }
#else
            Debug.LogWarning("SaveTemplateAsset는 에디터에서만 작동합니다.");
#endif
        }

        private static void InitializeTemplates()
        {
            initialized = true;
            templates.Clear();

            // 1. Resources 폴더에서 사용자가 만든 에셋(ScriptableObject) 모두 불러오기
            GestureTemplateAsset[] loadedAssets = Resources.LoadAll<GestureTemplateAsset>("GestureTemplates");
            foreach (var asset in loadedAssets)
            {
                templates.Add(new GestureTemplate(asset.shapeName, asset.points));
            }
            Debug.Log($"[ShapeRecognizer] 커스텀 에셋 템플릿 {loadedAssets.Length}개 로드 완료.");

            // Template 1: Circle
            List<Point> circlePts = new List<Point>();
            for (int i = 0; i <= 32; i++)
            {
                float angle = i * Mathf.PI * 2f / 32f;
                circlePts.Add(new Point(Mathf.Cos(angle), Mathf.Sin(angle), 1));
            }
            templates.Add(new GestureTemplate("Circle", circlePts.ToArray()));

            // Template 2: Triangle
            List<Point> trianglePts = new List<Point>();
            trianglePts.Add(new Point(0, 1, 1));
            trianglePts.Add(new Point(-0.866f, -0.5f, 1));
            trianglePts.Add(new Point(0.866f, -0.5f, 1));
            trianglePts.Add(new Point(0, 1, 1)); // back to start
            templates.Add(new GestureTemplate("Triangle", trianglePts.ToArray()));

            // Template 3: Star
            List<Point> starPts = new List<Point>();
            float a = Mathf.PI / 2; // start at top
            for (int i = 0; i <= 5; i++) // 5 points to close the star
            {
                float angle = a + i * (4f * Mathf.PI / 5f);
                starPts.Add(new Point(Mathf.Cos(angle), Mathf.Sin(angle), 1));
            }
            templates.Add(new GestureTemplate("Star", starPts.ToArray()));
            
            // Template 4: Square
            List<Point> squarePts = new List<Point>();
            squarePts.Add(new Point(-1, 1, 1));
            squarePts.Add(new Point(1, 1, 1));
            squarePts.Add(new Point(1, -1, 1));
            squarePts.Add(new Point(-1, -1, 1));
            squarePts.Add(new Point(-1, 1, 1));
            templates.Add(new GestureTemplate("Square", squarePts.ToArray()));
        }
    }
}
