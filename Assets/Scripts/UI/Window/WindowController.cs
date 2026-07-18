using UnityEngine;
using UnityEngine.UI;

namespace Magic.UI
{
    public class WindowController : MonoBehaviour
    {
        [Header("Window CanvasGroups")]
        [SerializeField] CanvasGroup _drawing;
        
        public void ShowDrawingArea() {
            _drawing.alpha = 1f;
            _drawing.blocksRaycasts = true;
            _drawing.interactable = true;
        }

        public void HideDrawingArea() {
            _drawing.alpha = 0f;
            _drawing.blocksRaycasts = false;
            _drawing.interactable = false;
        }
    }
}
