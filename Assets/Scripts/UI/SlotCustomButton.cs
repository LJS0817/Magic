using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public class SlotCustomButton : CustomButton
{
    [Header("Selection Settings")]
    [SerializeField] protected Color selectedColor = new Color(0.6f, 0.8f, 1f, 1f);

    public bool IsSelected { get; private set; }

    protected override void OnEnable()
    {
        base.OnEnable();
        if (targetGraphic != null && IsSelected)
        {
            colorTween?.Kill();
            targetGraphic.color = selectedColor;
        }
    }

    public virtual void Select()
    {
        IsSelected = true;
        AnimateColor(selectedColor);
    }

    public virtual void Deselect(bool immediate = false)
    {
        if (!IsSelected) return;

        IsSelected = false;
        
        if (immediate)
        {
            colorTween?.Kill();
            if (targetGraphic != null) targetGraphic.color = normalColor;
        }
        else
        {
            AnimateColor(normalColor);
        }
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        isHovering = true;
        if (IsSelected) return; // 선택된 상태에서는 Hover 색상 변경 안 함 (선택 색상 고정)
        
        if (!isPressed)
            AnimateColor(hoverColor);
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;
        if (IsSelected) return; // 선택된 상태에서는 Normal 색상으로 돌아가지 않음 (선택 색상 고정)
        
        if (!isPressed)
            AnimateColor(normalColor);
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        isPressed = true;
        if (IsSelected) return; // 선택된 상태에서는 눌림 색상 변경 안 함
        
        AnimateColor(pressedColor);
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        isPressed = false;
        if (IsSelected) return;
        
        AnimateColor(isHovering ? hoverColor : normalColor);
    }
}
