using UnityEngine;
using UnityEngine.Audio;
using System.Collections.Generic;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance { get; private set; }

    [Header("Audio")]
    public AudioMixer audioMixer; // 오디오 믹서를 유니티에서 할당해야 합니다.

    [Header("Current Settings")]
    public bool isDamageTextEnabled = true;
    public bool isScreenShakeEnabled = true;
    public float drawingSensitivity = 1.0f;
    
    // 상수 키값
    private const string KEY_RES_INDEX = "ResIndex";
    private const string KEY_FULLSCREEN_MODE = "FullscreenMode";
    private const string KEY_VSYNC = "VSync";
    private const string KEY_FPS_LIMIT = "FPSLimit";

    private const string KEY_VOL_MASTER = "VolMaster";
    private const string KEY_VOL_BGM = "VolBGM";
    private const string KEY_VOL_SFX = "VolSFX";
    private const string KEY_VOL_UI = "VolUI";

    private const string KEY_DMG_TEXT = "DamageText";
    private const string KEY_SCREEN_SHAKE = "ScreenShake";
    private const string KEY_CONFINE_CURSOR = "ConfineCursor";
    private const string KEY_DRAW_SENSITIVITY = "DrawSensitivity";

    private Resolution[] _resolutions;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadSettings();
    }

    private void LoadSettings()
    {
        // 해상도 초기화 (유니티에서 지원하는 해상도 목록)
        _resolutions = Screen.resolutions;

        // 디스플레이
        int resIndex = PlayerPrefs.GetInt(KEY_RES_INDEX, _resolutions.Length - 1);
        int modeInt = PlayerPrefs.GetInt(KEY_FULLSCREEN_MODE, (int)FullScreenMode.FullScreenWindow);
        SetResolution(resIndex, (FullScreenMode)modeInt);

        bool isVsync = PlayerPrefs.GetInt(KEY_VSYNC, 1) == 1;
        SetVSync(isVsync);

        int fpsLimit = PlayerPrefs.GetInt(KEY_FPS_LIMIT, 60);
        SetFPSLimit(fpsLimit);

        // 사운드
        SetMasterVolume(PlayerPrefs.GetFloat(KEY_VOL_MASTER, 1f));
        SetBGMVolume(PlayerPrefs.GetFloat(KEY_VOL_BGM, 1f));
        SetSFXVolume(PlayerPrefs.GetFloat(KEY_VOL_SFX, 1f));
        SetUIVolume(PlayerPrefs.GetFloat(KEY_VOL_UI, 1f));

        // 게임플레이
        isDamageTextEnabled = PlayerPrefs.GetInt(KEY_DMG_TEXT, 1) == 1;
        isScreenShakeEnabled = PlayerPrefs.GetInt(KEY_SCREEN_SHAKE, 1) == 1;

        // 조작
        bool confineCursor = PlayerPrefs.GetInt(KEY_CONFINE_CURSOR, 0) == 1;
        SetConfineCursor(confineCursor);

        drawingSensitivity = PlayerPrefs.GetFloat(KEY_DRAW_SENSITIVITY, 1.0f);
    }

    // ================== Display ==================
    public void SetResolution(int resolutionIndex, FullScreenMode mode)
    {
        if (_resolutions == null || resolutionIndex < 0 || resolutionIndex >= _resolutions.Length) return;
        
        Resolution res = _resolutions[resolutionIndex];
        Screen.SetResolution(res.width, res.height, mode);
        
        PlayerPrefs.SetInt(KEY_RES_INDEX, resolutionIndex);
        PlayerPrefs.SetInt(KEY_FULLSCREEN_MODE, (int)mode);
        PlayerPrefs.Save();
    }

    public void SetVSync(bool isOn)
    {
        QualitySettings.vSyncCount = isOn ? 1 : 0;
        PlayerPrefs.SetInt(KEY_VSYNC, isOn ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void SetFPSLimit(int limit)
    {
        Application.targetFrameRate = limit;
        PlayerPrefs.SetInt(KEY_FPS_LIMIT, limit);
        PlayerPrefs.Save();
    }

    // ================== Sound ==================
    // 오디오 믹서에 파라미터(MasterVolume, BGMVolume 등)를 Exposed 해야 동작합니다.
    private void SetMixerVolume(string parameterName, float linearValue)
    {
        if (audioMixer == null) return;
        
        // 0.0001 to 1 선형값을 -80dB to 0dB 로 변환
        float db = linearValue > 0.0001f ? Mathf.Log10(linearValue) * 20f : -80f;
        audioMixer.SetFloat(parameterName, db);
    }

    public void SetMasterVolume(float volume)
    {
        SetMixerVolume("MasterVolume", volume);
        PlayerPrefs.SetFloat(KEY_VOL_MASTER, volume);
        PlayerPrefs.Save();
    }

    public void SetBGMVolume(float volume)
    {
        SetMixerVolume("BGMVolume", volume);
        PlayerPrefs.SetFloat(KEY_VOL_BGM, volume);
        PlayerPrefs.Save();
    }

    public void SetSFXVolume(float volume)
    {
        SetMixerVolume("SFXVolume", volume);
        PlayerPrefs.SetFloat(KEY_VOL_SFX, volume);
        PlayerPrefs.Save();
    }

    public void SetUIVolume(float volume)
    {
        SetMixerVolume("UIVolume", volume);
        PlayerPrefs.SetFloat(KEY_VOL_UI, volume);
        PlayerPrefs.Save();
    }

    // ================== Gameplay ==================
    public void SetDamageText(bool isOn)
    {
        isDamageTextEnabled = isOn;
        PlayerPrefs.SetInt(KEY_DMG_TEXT, isOn ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void SetScreenShake(bool isOn)
    {
        isScreenShakeEnabled = isOn;
        PlayerPrefs.SetInt(KEY_SCREEN_SHAKE, isOn ? 1 : 0);
        PlayerPrefs.Save();
    }

    // ================== Controls ==================
    public void SetConfineCursor(bool isConfined)
    {
        Cursor.lockState = isConfined ? CursorLockMode.Confined : CursorLockMode.None;
        PlayerPrefs.SetInt(KEY_CONFINE_CURSOR, isConfined ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void SetDrawingSensitivity(float sensitivity)
    {
        drawingSensitivity = sensitivity;
        PlayerPrefs.SetFloat(KEY_DRAW_SENSITIVITY, sensitivity);
        PlayerPrefs.Save();
    }

    public Resolution[] GetResolutions()
    {
        if (_resolutions == null) _resolutions = Screen.resolutions;
        return _resolutions;
    }
}
