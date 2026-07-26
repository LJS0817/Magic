using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Events;
using DG.Tweening;

public class CustomButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
{
    [Header("Visuals")]
    [SerializeField] protected Graphic targetGraphic;
    
    [Header("Colors")]
    [SerializeField] protected Color normalColor = Color.white;
    [SerializeField] protected Color hoverColor = new Color(0.9f, 0.9f, 0.9f, 1f);
    [SerializeField] protected Color pressedColor = new Color(0.7f, 0.7f, 0.7f, 1f);
    
    [Header("Animation Settings")]
    [SerializeField] protected float tweenDuration = 0.15f;
    [SerializeField] protected Ease tweenEase = Ease.OutQuad;

    [Header("Events")]
    public UnityEvent onClick;

    protected Tween colorTween;
    protected bool isHovering = false;
    protected bool isPressed = false;

    protected virtual void Reset()
    {
        targetGraphic = GetComponent<Graphic>();
    }

    protected virtual void Awake()
    {
        if (targetGraphic == null)
            targetGraphic = GetComponent<Graphic>();
            
        if (targetGraphic != null)
            targetGraphic.color = normalColor;
    }

    protected virtual void OnEnable()
    {
        if (targetGraphic != null)
        {
            colorTween?.Kill();
            targetGraphic.color = isPressed ? pressedColor : (isHovering ? hoverColor : normalColor);
        }
    }

    protected virtual void OnDisable()
    {
        colorTween?.Kill();
        isHovering = false;
        isPressed = false;
    }
    
    protected virtual void OnDestroy()
    {
        colorTween?.Kill();
    }

    public virtual void OnPointerEnter(PointerEventData eventData)
    {
        isHovering = true;
        if (!isPressed)
            AnimateColor(hoverColor);
    }

    public virtual void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;
        if (!isPressed)
            AnimateColor(normalColor);
    }

    public virtual void OnPointerDown(PointerEventData eventData)
    {
        isPressed = true;
        AnimateColor(pressedColor);
    }

    public virtual void OnPointerUp(PointerEventData eventData)
    {
        isPressed = false;
        AnimateColor(isHovering ? hoverColor : normalColor);
    }

    public virtual void OnPointerClick(PointerEventData eventData)
    {
        onClick?.Invoke();
    }

    protected void AnimateColor(Color targetColor)
    {
        if (targetGraphic == null) return;
        
        colorTween?.Kill();
        colorTween = targetGraphic.DOColor(targetColor, tweenDuration).SetEase(tweenEase);
    }
}
