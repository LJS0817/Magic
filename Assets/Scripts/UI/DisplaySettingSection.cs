using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class DisplaySettingSection : SettingSection
{
    [Header("Display Settings")]
    public TMP_Dropdown resolutionDropdown;
    public TMP_Dropdown fullscreenModeDropdown;
    public CustomToggle vsyncToggle;
    public TMP_Dropdown fpsLimitDropdown;

    private SettingsManager SM => SettingsManager.Instance;

    public override void Initialize()
    {
        base.Initialize();
        if (SM == null) return;

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
        }

        if (fullscreenModeDropdown != null)
        {
            fullscreenModeDropdown.ClearOptions();
            List<string> modeOptions = new List<string>
            {
                "Exclusive FullScreen",
                "FullScreen Window (Borderless)",
                "Maximized Window",
                "Windowed"
            };
            fullscreenModeDropdown.AddOptions(modeOptions);
        }

        if (fpsLimitDropdown != null)
        {
            fpsLimitDropdown.ClearOptions();
            List<string> fpsOptions = new List<string>
            {
                "60 FPS",
                "144 FPS",
                "Unlimited"
            };
            fpsLimitDropdown.AddOptions(fpsOptions);
        }

        // 이벤트 리스너 연결
        if (resolutionDropdown != null) resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);
        if (fullscreenModeDropdown != null) fullscreenModeDropdown.onValueChanged.AddListener(OnFullscreenModeChanged);
        if (vsyncToggle != null) vsyncToggle.onValueChanged.AddListener(OnVSyncChanged);
        if (fpsLimitDropdown != null) fpsLimitDropdown.onValueChanged.AddListener(OnFPSLimitChanged);
    }

    public override void Refresh()
    {
        base.Refresh();
        if (SM == null) return;

        if (resolutionDropdown != null) resolutionDropdown.value = PlayerPrefs.GetInt("ResIndex", resolutionDropdown.options.Count - 1);
        if (fullscreenModeDropdown != null)
        {
            int modeInt = PlayerPrefs.GetInt("FullscreenMode", (int)FullScreenMode.FullScreenWindow);
            fullscreenModeDropdown.value = modeInt;
        }
        if (vsyncToggle != null) vsyncToggle.SetIsOnWithoutNotify(PlayerPrefs.GetInt("VSync", 1) == 1);
        
        if (fpsLimitDropdown != null)
        {
            int fpsLimit = PlayerPrefs.GetInt("FPSLimit", 60);
            if (fpsLimit == 60) fpsLimitDropdown.value = 0;
            else if (fpsLimit == 144) fpsLimitDropdown.value = 1;
            else fpsLimitDropdown.value = 2; // 제한 없음
        }
    }

    public override void ResetToDefaults()
    {
        if (SM == null) return;

        // 기본값: 최대 해상도, FullScreenWindow, VSync ON, FPS 60
        Resolution[] resList = SM.GetResolutions();
        int defaultResIndex = resList.Length - 1;
        SM.SetResolution(defaultResIndex, FullScreenMode.FullScreenWindow);
        SM.SetVSync(true);
        SM.SetFPSLimit(60);

        Refresh();
    }

    private void OnResolutionChanged(int index)
    {
        FullScreenMode mode = fullscreenModeDropdown != null ? (FullScreenMode)fullscreenModeDropdown.value : FullScreenMode.FullScreenWindow;
        SM.SetResolution(index, mode);
    }

    private void OnFullscreenModeChanged(int modeIndex)
    {
        int resIndex = resolutionDropdown != null ? resolutionDropdown.value : 0;
        SM.SetResolution(resIndex, (FullScreenMode)modeIndex);
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
}
