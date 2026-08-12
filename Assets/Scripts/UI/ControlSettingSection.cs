using UnityEngine;

public class ControlSettingSection : SettingSection
{
    [Header("Control Settings")]
    public CustomToggle confineCursorToggle;
    public CustomInteractableSlider drawingSensitivitySlider;

    private SettingsManager SM => SettingsManager.Instance;

    public override void Initialize()
    {
        base.Initialize();
        if (SM == null) return;

        if (confineCursorToggle != null) confineCursorToggle.onValueChanged.AddListener(OnConfineCursorChanged);
        if (drawingSensitivitySlider != null) drawingSensitivitySlider.onValueChanged.AddListener(OnDrawingSensitivityChanged);
    }

    public override void Refresh()
    {
        base.Refresh();
        if (SM == null) return;

        if (confineCursorToggle != null) confineCursorToggle.SetIsOnWithoutNotify(PlayerPrefs.GetInt("ConfineCursor", 0) == 1);
        if (drawingSensitivitySlider != null) drawingSensitivitySlider.SetValueWithoutNotify(SM.drawingSensitivity);
    }

    public override void ResetToDefaults()
    {
        if (SM == null) return;

        // 기본값: ConfineCursor OFF, DrawingSensitivity 1.0
        SM.SetConfineCursor(false);
        SM.SetDrawingSensitivity(1.0f);

        Refresh();
    }

    private void OnConfineCursorChanged(bool isOn) => SM.SetConfineCursor(isOn);
    private void OnDrawingSensitivityChanged(float val) => SM.SetDrawingSensitivity(val);
}
