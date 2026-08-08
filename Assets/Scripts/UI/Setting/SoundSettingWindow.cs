using UnityEngine;
public class SoundSettingWindow : SettingWindow
{
    [Header("Sound Settings")]
    public CustomInteractableSlider masterVolumeSlider;
    public CustomInteractableSlider bgmVolumeSlider;
    public CustomInteractableSlider sfxVolumeSlider;
    public CustomInteractableSlider uiVolumeSlider;
    private SettingsManager SM => SettingsManager.Instance;
    public override void Initialize()
    {
        base.Initialize();
        if (SM == null) return;
        if (masterVolumeSlider != null) masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
        if (bgmVolumeSlider != null) bgmVolumeSlider.onValueChanged.AddListener(OnBGMVolumeChanged);
        if (sfxVolumeSlider != null) sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
        if (uiVolumeSlider != null) uiVolumeSlider.onValueChanged.AddListener(OnUIVolumeChanged);
    }
    public override void Refresh()
    {
        base.Refresh();
        if (SM == null) return;
        if (masterVolumeSlider != null) masterVolumeSlider.SetValueWithoutNotify(PlayerPrefs.GetFloat("VolMaster", 1f));
        if (bgmVolumeSlider != null) bgmVolumeSlider.SetValueWithoutNotify(PlayerPrefs.GetFloat("VolBGM", 1f));
        if (sfxVolumeSlider != null) sfxVolumeSlider.SetValueWithoutNotify(PlayerPrefs.GetFloat("VolSFX", 1f));
        if (uiVolumeSlider != null) uiVolumeSlider.SetValueWithoutNotify(PlayerPrefs.GetFloat("VolUI", 1f));
    }
    private void OnMasterVolumeChanged(float val) => SM.SetMasterVolume(val);
    private void OnBGMVolumeChanged(float val) => SM.SetBGMVolume(val);
    private void OnSFXVolumeChanged(float val) => SM.SetSFXVolume(val);
    private void OnUIVolumeChanged(float val) => SM.SetUIVolume(val);
}
