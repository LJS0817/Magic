using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class SettingWindow : MonoBehaviour
{
    protected CanvasGroup canvasGroup;

    protected virtual void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public virtual void Open()
    {
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
        
        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }

    public virtual void Close()
    {
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
        
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    public virtual void Initialize()
    {
        // 자식 클래스에서 재정의하여 각종 UI 이벤트 초기화를 수행합니다.
    }

    public virtual void Refresh()
    {
        // 자식 클래스에서 재정의하여 현재 설정값을 UI에 반영합니다.
    }
}
