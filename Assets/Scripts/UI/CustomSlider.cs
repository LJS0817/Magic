using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CustomSlider : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] protected RectMask2D mask;
    [SerializeField] protected TMP_Text valueText;

    protected RectTransform canvas;

    protected virtual void Awake()
    {
        if (canvas == null)
        {
            Canvas parentCanvas = GetComponentInParent<Canvas>();
            if (parentCanvas != null) canvas = parentCanvas.GetComponent<RectTransform>();
        }
    }

    public virtual void SetValue(float value, float max)
    {
        if (valueText != null)
        {
            valueText.text = $"{Mathf.RoundToInt(value)} / {max}";
        }

        // 값의 비율(0~1)로 변환
        float ratio = Mathf.Clamp01(value / max);
        
        // CustomSlicedSlider 방식의 스케일 보정 패딩 계산 적용
        if (canvas == null)
        {
            Canvas parentCanvas = GetComponentInParent<Canvas>();
            if (parentCanvas != null) canvas = parentCanvas.GetComponent<RectTransform>();
        }

        float scaleX = canvas == null ? (Screen.width / 1920.0f) : canvas.lossyScale.x;
        float fullWidth = mask.rectTransform.rect.width;
        float currentScaleX = mask.rectTransform.lossyScale.x / scaleX;

        // RectMask2D Padding Z(Right) 조절
        Vector4 padding = mask.padding;
        padding.z = (fullWidth * currentScaleX) * (1f - ratio);
        mask.padding = padding;
    }
}

