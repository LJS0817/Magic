using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class DrawingLine : MonoBehaviour
{
    public LineRenderer lineRenderer { get; private set; }
    public float minDistanceToUpdate = 0.1f;
    
    public Stroke currentStroke { get; private set; }

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.useWorldSpace = false;
        currentStroke = new Stroke();

        // 매테리얼이 할당되지 않아 안보이는 현상 방지
        if (lineRenderer.sharedMaterial == null)
        {
            lineRenderer.sharedMaterial = new Material(Shader.Find("Sprites/Default"));
        }
    }

    public void SetColor(Color color)
    {
        if (lineRenderer != null)
        {
            lineRenderer.startColor = color;
            lineRenderer.endColor = color;
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
        // useWorldSpace가 false이므로 로컬 좌표계를 사용합니다. Z는 0으로 둡니다.
        lineRenderer.SetPosition(currentStroke.points.Count - 1, new Vector3(point.x, point.y, 0f));
    }
}

