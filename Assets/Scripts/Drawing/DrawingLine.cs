using UnityEngine;

namespace Magic.Drawing
{
    [RequireComponent(typeof(LineRenderer))]
    public class DrawingLine : MonoBehaviour
    {
        public LineRenderer lineRenderer { get; private set; }
        public float minDistanceToUpdate = 0.1f;
        
        public Stroke currentStroke { get; private set; }

        private void Awake()
        {
            lineRenderer = GetComponent<LineRenderer>();
            currentStroke = new Stroke();

            // 매테리얼이 할당되지 않아 안보이는 현상 방지
            if (lineRenderer.sharedMaterial == null)
            {
                lineRenderer.sharedMaterial = new Material(Shader.Find("Sprites/Default"));
            }
        }

        public void AddPoint(Vector2 point)
        {
            if (currentStroke.points.Count > 0)
            {
                // Check distance from the last point
                Vector2 lastPoint = currentStroke.points[currentStroke.points.Count - 1];
                if (Vector2.Distance(lastPoint, point) < minDistanceToUpdate)
                    return;
            }

            currentStroke.AddPoint(point);
            
            lineRenderer.positionCount = currentStroke.points.Count;
            // Z=0 for 2D
            lineRenderer.SetPosition(currentStroke.points.Count - 1, new Vector3(point.x, point.y, 0f));
        }
    }
}
