using TMPro;
using UnityEngine;

public class MapMonocle : MonoBehaviour
{
    [SerializeField] TMP_Text _text;
    RectTransform _rectTransform;
    Canvas _canvas;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        _canvas = GetComponentInParent<Canvas>();
    }

    private void Start()
    {
        ClearInfo();
    }

    public void UpdatePosition()
    {
        if (_rectTransform == null || _rectTransform.parent == null) return;

        Camera cam = null;
        if (_canvas != null && _canvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            cam = _canvas.worldCamera != null ? _canvas.worldCamera : Camera.main;
        }

        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(
            (RectTransform)_rectTransform.parent, 
            Input.mousePosition, 
            cam, 
            out Vector3 worldPoint))
        {
            _rectTransform.position = worldPoint;
        }
    }

    public void SetInfo(string info)
    {
        if (_text != null)
        {
            _text.text = info;
            _text.gameObject.SetActive(true);
        }
    }

    public void ClearInfo()
    {
        if (_text != null)
        {
            _text.text = "";
            _text.gameObject.SetActive(false);
        }
    }
}
