using UnityEngine;
using UnityEngine.UI;

namespace Magic.Upgrade
{
    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(Image))]
    public class UILineConnection : MonoBehaviour
    {
        private RectTransform rectTransform;
        
        public void DrawLine(Vector2 pointA, Vector2 pointB, float thickness, Color color)
        {
            if (rectTransform == null)
            {
                rectTransform = GetComponent<RectTransform>();
            }

            Image img = GetComponent<Image>();
            if (img != null)
            {
                img.color = color;
            }

            // Calculate distance and angle
            Vector2 differenceVector = pointB - pointA;
            float distance = differenceVector.magnitude;
            float angle = Mathf.Atan2(differenceVector.y, differenceVector.x) * Mathf.Rad2Deg;

            // Set pivot to start of line
            rectTransform.pivot = new Vector2(0, 0.5f);
            
            // Set position to pointA
            rectTransform.anchoredPosition = pointA;
            
            // Set size: width = distance, height = thickness
            rectTransform.sizeDelta = new Vector2(distance, thickness);
            
            // Set rotation
            rectTransform.localRotation = Quaternion.Euler(0, 0, angle);
        }
    }
}
