using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class SettingsUIController : MonoBehaviour
{
    [Header("Navigation - Buttons")]
    public CustomButton displayButton;
    public CustomButton soundButton;
    public CustomButton gameplayButton;
    public CustomButton controlButton;

    [Header("Navigation - Sections")]
    public SettingSection displaySection;
    public SettingSection soundSection;
    public SettingSection gameplaySection;
    public SettingSection controlSection;

    [Header("Scroll")]
    public ScrollRect scrollRect;

    [Header("Reset")]
    public CustomButton resetButton;

    [Header("Scroll Animation")]
    [SerializeField] private float scrollDuration = 0.3f;
    [SerializeField] private Ease scrollEase = Ease.OutQuad;

    private SettingSection[] _sections;
    private Tween _scrollTween;
    private bool _isInitialized = false;

    private void Start()
    {
        InitializeUI();
    }

    private void OnEnable()
    {
        if (_isInitialized)
        {
            RefreshUI();
        }
    }

    private void OnDestroy()
    {
        _scrollTween?.Kill();
    }

    private void InitializeUI()
    {
        _sections = new[] { displaySection, soundSection, gameplaySection, controlSection };

        foreach (var section in _sections)
        {
            if (section != null) section.Initialize();
        }

        InitializeNavigation();

        _isInitialized = true;
        RefreshUI();
    }

    private void RefreshUI()
    {
        foreach (var section in _sections)
        {
            if (section != null) section.Refresh();
        }
    }

    private void InitializeNavigation()
    {
        if (displayButton != null) displayButton.onClick.AddListener(() => ScrollToSection(0));
        if (soundButton != null) soundButton.onClick.AddListener(() => ScrollToSection(1));
        if (gameplayButton != null) gameplayButton.onClick.AddListener(() => ScrollToSection(2));
        if (controlButton != null) controlButton.onClick.AddListener(() => ScrollToSection(3));

        if (resetButton != null) resetButton.onClick.AddListener(OnResetClicked);
    }

    /// <summary>
    /// 지정된 인덱스의 섹션으로 스크롤을 부드럽게 이동합니다.
    /// </summary>
    public void ScrollToSection(int sectionIndex)
    {
        if (_sections == null || sectionIndex < 0 || sectionIndex >= _sections.Length) return;
        if (_sections[sectionIndex] == null || scrollRect == null) return;

        RectTransform content = scrollRect.content;
        RectTransform viewport = scrollRect.viewport;
        RectTransform targetRect = _sections[sectionIndex].SectionRect;

        if (content == null || viewport == null || targetRect == null) return;

        // Content 내에서 타겟 섹션의 로컬 위치를 계산
        // 섹션의 상단이 뷰포트 상단에 오도록 위치를 계산합니다.
        Vector2 targetLocalPos = content.InverseTransformPoint(targetRect.position);
        Vector2 contentLocalPos = content.InverseTransformPoint(content.position);

        float contentHeight = content.rect.height;
        float viewportHeight = viewport.rect.height;

        // 스크롤 가능한 범위가 없으면 무시
        float scrollableHeight = contentHeight - viewportHeight;
        if (scrollableHeight <= 0f) return;

        // 타겟 위치 계산 (0 = 맨 아래, 1 = 맨 위)
        float targetY = contentLocalPos.y - targetLocalPos.y;
        float normalizedPos = Mathf.Clamp01(1f - (targetY / scrollableHeight));

        // DOTween으로 부드러운 스크롤 애니메이션
        _scrollTween?.Kill();
        _scrollTween = DOTween.To(
            () => scrollRect.verticalNormalizedPosition,
            v => scrollRect.verticalNormalizedPosition = v,
            normalizedPos,
            scrollDuration
        ).SetEase(scrollEase).SetUpdate(true);
    }

    /// <summary>
    /// 모든 설정을 기본값으로 초기화합니다.
    /// </summary>
    private void OnResetClicked()
    {
        if (SettingsManager.Instance != null)
        {
            SettingsManager.Instance.ResetToDefaults();
        }

        RefreshUI();
    }
}
