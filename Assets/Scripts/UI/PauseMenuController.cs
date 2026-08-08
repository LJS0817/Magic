using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;

public class PauseMenuController : MonoBehaviour
{
    [Header("Panels")]
    [Tooltip("The main pause menu overlay (Continue, Save, Load, etc)")]
    public CanvasGroup pauseMenuPanel;
    public CanvasGroup pauseMenuButtons;
    
    [Tooltip("The settings/options overlay")]
    public CanvasGroup settingsPanel;

    [Header("Buttons")]
    public CustomButton continueButton;
    public CustomButton saveButton;
    public CustomButton loadButton;
    public CustomButton optionButton;
    public CustomButton returnToMenuButton;
    public CustomButton returnToDesktopButton;

    public bool IsPaused { get; private set; }

    private void Awake()
    {
        // UI 이벤트 바인딩
        if (continueButton != null) continueButton.onClick.AddListener(ResumeGame);
        if (saveButton != null) saveButton.onClick.AddListener(OnSaveClicked);
        if (loadButton != null) loadButton.onClick.AddListener(OnLoadClicked);
        if (optionButton != null) optionButton.onClick.AddListener(OpenOptions);
        if (returnToMenuButton != null) returnToMenuButton.onClick.AddListener(ReturnToMenu);
        if (returnToDesktopButton != null) returnToDesktopButton.onClick.AddListener(ReturnToDesktop);

        // 초기 상태 숨김
        SwitchPanel(false, false);
    }

    void SwitchPanel(bool onPause, bool onSetting)
    {
        if(pauseMenuPanel.interactable != onPause)
        {
            pauseMenuPanel.alpha = onPause ? 1f : 0f;
            pauseMenuPanel.blocksRaycasts = onPause;
            pauseMenuPanel.interactable = onPause;
        }

        if(settingsPanel.interactable != onSetting)
        {
            settingsPanel.alpha = onSetting ? 1f : 0f;
            settingsPanel.blocksRaycasts = onSetting;
            settingsPanel.interactable = onSetting;

            pauseMenuButtons.alpha = onSetting ? 0f : 1f;
            pauseMenuButtons.blocksRaycasts = !onSetting;
            pauseMenuButtons.interactable = !onSetting;
        }
    }

    private void Update()
    {
        // ESC 키를 누르면 토글
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {
        if (IsPaused)
        {
            // 설정 창이 열려있든, 일시정지 메인 창이 열려있든 전부 닫고 게임 재개
            ResumeGame();
        }
        else
        {
            PauseGame();
        }
    }

    public void PauseGame()
    {
        IsPaused = true;
        Time.timeScale = 0f; // 게임 시간 정지

        SwitchPanel(true, false);
    }

    public void ResumeGame()
    {
        IsPaused = false;
        Time.timeScale = 1f; // 게임 시간 재개

        SwitchPanel(false, false);
    }

    private void OpenOptions()
    {
        SwitchPanel(true, true);
    }

    public void CloseOptions()
    {
        SwitchPanel(true, false);
    }

    private void OnSaveClicked()
    {
        // TODO: 향후 SaveManager.Instance.Save() 등으로 연동
        Debug.Log("[PauseMenu] 저장이 완료되었습니다. (임시)");
        loadButton.SetText($"불러오기 <size=60%>( {DateTime.Now.ToLocalTime()} )");
        LayoutRebuilder.ForceRebuildLayoutImmediate(loadButton.GetComponent<RectTransform>());
    }

    private void OnLoadClicked()
    {
        // TODO: 향후 SaveManager.Instance.Load() 등으로 연동
        Debug.Log("[PauseMenu] 데이터를 불러옵니다. (임시)");
    }

    private void ReturnToMenu()
    {
        Time.timeScale = 1f;
        // 메인 메뉴 씬 이름이 "MainMenu"라고 가정합니다. 실제 이름으로 변경해야 할 수 있습니다.
        SceneManager.LoadScene("MainMenu");
    }

    private void ReturnToDesktop()
    {
        Time.timeScale = 1f;
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
