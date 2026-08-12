using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class CustomInteractableSlider : CustomSlider, IPointerDownHandler, IDragHandler
{
    public float maxValue = 100f;
    [SerializeField] private float m_Value;
    [SerializeField] private RectTransform handle;
    RectTransform handleArea;

    public UnityEvent<float> onValueChanged = new UnityEvent<float>();

    protected override void Awake()
    {
        base.Awake();
        handleArea = handle.parent.GetComponent<RectTransform>();
    }

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
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(handleArea, eventData.position, eventData.pressEventCamera, out Vector2 localPoint))
        {
            float percentage = Mathf.InverseLerp(
                handleArea.rect.xMin,
                handleArea.rect.xMax,
                localPoint.x
            );

            float newValue = Mathf.Clamp01(percentage);
            value = newValue;
        }
    }

    public override void SetValue(float value, float max)
    {
        float scaleX = canvas == null ? (Screen.width / 1920.0f) : canvas.lossyScale.x;
        float fullWidth = handleArea.rect.width;
        float currentScaleX = mask.rectTransform.lossyScale.x / scaleX;
        float halfHandle = handle.rect.width * handle.lossyScale.x / scaleX * 0.5f;

        float paddingRight = halfHandle + (fullWidth * currentScaleX) * (1f - value);
        mask.padding = new Vector4(0, 0, paddingRight, 0);

        // 2. Handle 위치 업데이트
        if (handle != null && handleArea != null)
        {
            handle.anchorMin = new Vector2(value, handle.anchorMin.y);
            handle.anchorMax = new Vector2(value, handle.anchorMax.y);
            handle.anchoredPosition = new Vector2(0, handle.anchoredPosition.y);
        }

        if (valueText != null)
        {
            valueText.text = $"{Mathf.RoundToInt(value * 100f)}%";
        }
    }
}
