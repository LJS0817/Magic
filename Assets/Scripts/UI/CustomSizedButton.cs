using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public class CustomSizedButton : CustomButton
{
    [Header("Size & Rotation Settings")]
    [SerializeField] private float hoverScale = 1.2f;
    [SerializeField] private Vector3 hoverRotation = new Vector3(0, 0, -5f); // 호버시 Z축으로 살짝 회전
    
    private Tween scaleTween;
    private Tween rotationTween;
    private Vector3 originalScale;
    private Quaternion originalRotation;

    protected override void Awake()
    {
        base.Awake();
        originalScale = transform.localScale;
        originalRotation = transform.localRotation;
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        scaleTween?.Kill();
        rotationTween?.Kill();
        
        transform.localScale = isHovering ? originalScale * hoverScale : originalScale;
        transform.localRotation = isHovering ? originalRotation * Quaternion.Euler(hoverRotation) : originalRotation;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        scaleTween?.Kill();
        rotationTween?.Kill();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        scaleTween?.Kill();
        rotationTween?.Kill();
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        base.OnPointerEnter(eventData);
        AnimateScaleAndRotation(originalScale * hoverScale, originalRotation * Quaternion.Euler(hoverRotation));
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        base.OnPointerExit(eventData);
        AnimateScaleAndRotation(originalScale, originalRotation);
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);
        // 클릭 시 스케일을 살짝 줄여서 클릭감을 줍니다.
        AnimateScaleAndRotation(originalScale * (hoverScale * 0.9f), originalRotation * Quaternion.Euler(hoverRotation));
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        base.OnPointerUp(eventData);
        if (isHovering)
        {
            AnimateScaleAndRotation(originalScale * hoverScale, originalRotation * Quaternion.Euler(hoverRotation));
        }
        else
        {
            AnimateScaleAndRotation(originalScale, originalRotation);
        }
    }

    private void AnimateScaleAndRotation(Vector3 targetScale, Quaternion targetRot)
    {
        scaleTween?.Kill();
        rotationTween?.Kill();

        scaleTween = transform.DOScale(targetScale, tweenDuration).SetEase(tweenEase);
        rotationTween = transform.DOLocalRotateQuaternion(targetRot, tweenDuration).SetEase(tweenEase);
    }
}
