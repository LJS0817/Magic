using UnityEngine;

public class SettingsUIController : MonoBehaviour
{
    [Header("Navigation - Buttons")]
    public CustomButton displayButton;
    public CustomButton soundButton;
    public CustomButton gameplayButton;
    public CustomButton controlButton;

    [Header("Navigation - Windows")]
    public SettingWindow displayWindow;
    public SettingWindow soundWindow;
    public SettingWindow gameplayWindow;
    public SettingWindow controlWindow;

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

    private void InitializeUI()
    {
        if (displayWindow != null) displayWindow.Initialize();
        if (soundWindow != null) soundWindow.Initialize();
        if (gameplayWindow != null) gameplayWindow.Initialize();
        if (controlWindow != null) controlWindow.Initialize();

        InitializeNavigation();

        _isInitialized = true;
        RefreshUI();
    }

    private void RefreshUI()
    {
        if (displayWindow != null) displayWindow.Refresh();
        if (soundWindow != null) soundWindow.Refresh();
        if (gameplayWindow != null) gameplayWindow.Refresh();
        if (controlWindow != null) controlWindow.Refresh();
    }

    private void InitializeNavigation()
    {
        if (displayButton != null) displayButton.onClick.AddListener(() => SwitchTab(0));
        if (soundButton != null) soundButton.onClick.AddListener(() => SwitchTab(1));
        if (gameplayButton != null) gameplayButton.onClick.AddListener(() => SwitchTab(2));
        if (controlButton != null) controlButton.onClick.AddListener(() => SwitchTab(3));

        // 기본 탭 표시
        SwitchTab(0);
    }

    public void SwitchTab(int tabIndex)
    {
        Debug.Log(tabIndex);
        if (tabIndex == 0) displayWindow.Open(); else displayWindow.Close();
        if (tabIndex == 1) soundWindow.Open(); else soundWindow.Close();
        if (tabIndex == 2) gameplayWindow.Open(); else gameplayWindow.Close();
        if (tabIndex == 3) controlWindow.Open(); else controlWindow.Close();
    }
}
