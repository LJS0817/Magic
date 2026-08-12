using UnityEngine;

/// <summary>
/// 설정 UI의 각 섹션(Display, Sound, Gameplay, Control)을 위한 베이스 클래스입니다.
/// ScrollRect 내부에서 항상 표시되며, RectTransform을 통해 스크롤 위치 계산에 사용됩니다.
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class SettingSection : MonoBehaviour
{
    private RectTransform _sectionRect;

    /// <summary>
    /// 이 섹션의 RectTransform. ScrollRect 내에서 스크롤 위치 계산에 사용됩니다.
    /// </summary>
    public RectTransform SectionRect
    {
        get
        {
            if (_sectionRect == null)
                _sectionRect = GetComponent<RectTransform>();
            return _sectionRect;
        }
    }

    /// <summary>
    /// 자식 클래스에서 재정의하여 각종 UI 이벤트 초기화를 수행합니다.
    /// </summary>
    public virtual void Initialize()
    {
    }

    /// <summary>
    /// 자식 클래스에서 재정의하여 현재 설정값을 UI에 반영합니다.
    /// </summary>
    public virtual void Refresh()
    {
    }

    /// <summary>
    /// 자식 클래스에서 재정의하여 설정을 기본값으로 초기화합니다.
    /// </summary>
    public virtual void ResetToDefaults()
    {
    }
}
