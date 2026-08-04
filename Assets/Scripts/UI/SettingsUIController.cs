using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class SettingsUIController : MonoBehaviour
{
    [Header("Display Settings")]
    public TMP_Dropdown resolutionDropdown;
    public Toggle fullscreenToggle;
    public Toggle vsyncToggle;
    public TMP_Dropdown fpsLimitDropdown;

    [Header("Sound Settings")]
    public CustomInteractableSlider masterVolumeSlider;
    public CustomInteractableSlider bgmVolumeSlider;
    public CustomInteractableSlider sfxVolumeSlider;
    public CustomInteractableSlider uiVolumeSlider;

    [Header("Gameplay Settings")]
    public Toggle damageTextToggle;
    public Toggle screenShakeToggle;

    [Header("Control Settings")]
    public Toggle confineCursorToggle;
    public Slider drawingSensitivitySlider;

    [Header("Navigation")]
    public CustomButton backButton;

    private SettingsManager SM => SettingsManager.Instance;
    private bool _isInitialized = false;

    private void Start()
    {
        // SettingsManager가 없으면 오류 방지
        if (SM == null) return;

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
        // 1. 디스플레이: 해상도 드롭다운 구성
        if (resolutionDropdown != null)
        {
            Resolution[] resList = SM.GetResolutions();
            resolutionDropdown.ClearOptions();
            List<string> options = new List<string>();
            int currentResIndex = 0;

            for (int i = 0; i < resList.Length; i++)
            {
                options.Add(resList[i].width + " x " + resList[i].height);
                // 현재 화면 해상도와 일치하는지 확인 (선택적)
                if (resList[i].width == Screen.currentResolution.width && resList[i].height == Screen.currentResolution.height)
                {
                    currentResIndex = i;
                }
            }
            resolutionDropdown.AddOptions(options);
            // PlayerPrefs에 저장된 인덱스를 우선 사용하도록 RefreshUI에서 덮어씌움
        }

        // 이벤트 리스너 연결
        if (resolutionDropdown != null) resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);
        if (fullscreenToggle != null) fullscreenToggle.onValueChanged.AddListener(OnFullscreenChanged);
        if (vsyncToggle != null) vsyncToggle.onValueChanged.AddListener(OnVSyncChanged);
        if (fpsLimitDropdown != null) fpsLimitDropdown.onValueChanged.AddListener(OnFPSLimitChanged);

        if (masterVolumeSlider != null) masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
        if (bgmVolumeSlider != null) bgmVolumeSlider.onValueChanged.AddListener(OnBGMVolumeChanged);
        if (sfxVolumeSlider != null) sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
        if (uiVolumeSlider != null) uiVolumeSlider.onValueChanged.AddListener(OnUIVolumeChanged);

        if (damageTextToggle != null) damageTextToggle.onValueChanged.AddListener(OnDamageTextChanged);
        if (screenShakeToggle != null) screenShakeToggle.onValueChanged.AddListener(OnScreenShakeChanged);

        if (confineCursorToggle != null) confineCursorToggle.onValueChanged.AddListener(OnConfineCursorChanged);
        if (drawingSensitivitySlider != null) drawingSensitivitySlider.onValueChanged.AddListener(OnDrawingSensitivityChanged);

        if (backButton != null) backButton.onClick.AddListener(OnBackClicked);

        _isInitialized = true;
        RefreshUI();
    }

    // UI 요소들을 현재 설정값(PlayerPrefs)에 맞게 동기화합니다.
    private void RefreshUI()
    {
        if (SM == null) return;

        if (resolutionDropdown != null) resolutionDropdown.value = PlayerPrefs.GetInt("ResIndex", resolutionDropdown.options.Count - 1);
        if (fullscreenToggle != null) fullscreenToggle.isOn = PlayerPrefs.GetInt("Fullscreen", 1) == 1;
        if (vsyncToggle != null) vsyncToggle.isOn = PlayerPrefs.GetInt("VSync", 1) == 1;
        
        if (fpsLimitDropdown != null)
        {
            int fpsLimit = PlayerPrefs.GetInt("FPSLimit", 60);
            if (fpsLimit == 60) fpsLimitDropdown.value = 0;
            else if (fpsLimit == 144) fpsLimitDropdown.value = 1;
            else fpsLimitDropdown.value = 2; // 제한 없음
        }

        if (masterVolumeSlider != null) masterVolumeSlider.value = PlayerPrefs.GetFloat("VolMaster", 1f);
        if (bgmVolumeSlider != null) bgmVolumeSlider.value = PlayerPrefs.GetFloat("VolBGM", 1f);
        if (sfxVolumeSlider != null) sfxVolumeSlider.value = PlayerPrefs.GetFloat("VolSFX", 1f);
        if (uiVolumeSlider != null) uiVolumeSlider.value = PlayerPrefs.GetFloat("VolUI", 1f);

        if (damageTextToggle != null) damageTextToggle.isOn = SM.isDamageTextEnabled;
        if (screenShakeToggle != null) screenShakeToggle.isOn = SM.isScreenShakeEnabled;

        if (confineCursorToggle != null) confineCursorToggle.isOn = PlayerPrefs.GetInt("ConfineCursor", 0) == 1;
        if (drawingSensitivitySlider != null) drawingSensitivitySlider.value = SM.drawingSensitivity;
    }

    // ================= 콜백 함수들 =================
    private void OnResolutionChanged(int index)
    {
        bool isFullscreen = fullscreenToggle != null ? fullscreenToggle.isOn : true;
        SM.SetResolution(index, isFullscreen);
    }

    private void OnFullscreenChanged(bool isOn)
    {
        int resIndex = resolutionDropdown != null ? resolutionDropdown.value : 0;
        SM.SetResolution(resIndex, isOn);
    }

    private void OnVSyncChanged(bool isOn) => SM.SetVSync(isOn);

    private void OnFPSLimitChanged(int index)
    {
        int limit = 60;
        if (index == 0) limit = 60;
        else if (index == 1) limit = 144;
        else if (index == 2) limit = -1; // -1 means no limit
        SM.SetFPSLimit(limit);
    }

    private void OnMasterVolumeChanged(float val) => SM.SetMasterVolume(val);
    private void OnBGMVolumeChanged(float val) => SM.SetBGMVolume(val);
    private void OnSFXVolumeChanged(float val) => SM.SetSFXVolume(val);
    private void OnUIVolumeChanged(float val) => SM.SetUIVolume(val);

    private void OnDamageTextChanged(bool isOn) => SM.SetDamageText(isOn);
    private void OnScreenShakeChanged(bool isOn) => SM.SetScreenShake(isOn);

    private void OnConfineCursorChanged(bool isOn) => SM.SetConfineCursor(isOn);
    private void OnDrawingSensitivityChanged(float val) => SM.SetDrawingSensitivity(val);

    private void OnBackClicked()
    {
        // SettingsManager에서 설정 저장 등을 마무리하는 용도 (현재는 실시간 저장이므로 불필요)
        
        // 만약 PauseMenuController가 씬에 존재한다면 옵션 창을 닫고 메인 일시정지 메뉴로 돌아갑니다.
        if (PauseMenuController.Instance != null)
        {
            PauseMenuController.Instance.CloseOptions();
        }
    }
}
