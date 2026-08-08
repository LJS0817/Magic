using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
public class CustomToggle : CustomButton
{
    [Header("Toggle Specifics")]
    [SerializeField] private Graphic checkmarkGraphic;
    [SerializeField] private Color toggleOnColor = Color.white;
    [SerializeField] private Color toggleOffColor = new Color(0.5f, 0.5f, 0.5f, 1f);
    public bool isOn;
    public UnityEvent<bool> onValueChanged;
    protected override void Awake()
    {
        base.Awake();
        UpdateVisuals(false);
    }
    public override void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            onClick?.Invoke();
            isOn = !isOn;
            UpdateVisuals(true);
            onValueChanged?.Invoke(isOn);
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            onRightClick?.Invoke();
        }
    }
    public void SetIsOnWithoutNotify(bool value)
    {
        isOn = value;
        UpdateVisuals(false);
    }
    private void UpdateVisuals(bool animate)
    {
        if (checkmarkGraphic != null)
        {
            checkmarkGraphic.gameObject.SetActive(isOn);
        }
        // 상태에 따른 기본(Normal) 색상을 변경합니다.
        normalColor = isOn ? toggleOnColor : toggleOffColor;
        // 현재 마우스 오버나 프레스 상태가 아니라면 즉시 또는 애니메이션으로 기본색상을 반영합니다.
        if (!isHovering && !isPressed)
        {
            if (animate) AnimateColor(normalColor);
            else if (targetGraphic != null) targetGraphic.color = normalColor;
        }
    }
}
