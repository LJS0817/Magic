using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class CustomInteractableSlider : CustomSlider, IPointerDownHandler, IDragHandler
{
    public float maxValue = 1f;
    [SerializeField] private float m_Value;

    public UnityEvent<float> onValueChanged = new UnityEvent<float>();

    public float value
    {
        get => m_Value;
        set
        {
            float clampedValue = Mathf.Clamp(value, 0, maxValue);
            if (!Mathf.Approximately(m_Value, clampedValue))
            {
                m_Value = clampedValue;
                SetValue(m_Value, maxValue);
                onValueChanged?.Invoke(m_Value);
            }
            else
            {
                SetValue(m_Value, maxValue);
            }
        }
    }

    public void SetValueWithoutNotify(float input)
    {
        float clampedValue = Mathf.Clamp(input, 0, maxValue);
        m_Value = clampedValue;
        SetValue(m_Value, maxValue);
    }

    protected virtual void Start()
    {
        SetValue(m_Value, maxValue);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        UpdateDrag(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        UpdateDrag(eventData);
    }

    private void UpdateDrag(PointerEventData eventData)
    {
        RectTransform rectTransform = GetComponent<RectTransform>();
        if (rectTransform == null) return;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, eventData.position, eventData.pressEventCamera, out Vector2 localPoint))
        {
            float width = rectTransform.rect.width;
            if (width <= 0) return;

            // rectTransform.rect.xMin is the left edge of the rect
            float xPos = localPoint.x - rectTransform.rect.xMin;
            float percent = Mathf.Clamp01(xPos / width);

            value = percent * maxValue;
        }
    }
}
