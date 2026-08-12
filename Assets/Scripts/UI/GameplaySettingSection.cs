using UnityEngine;

public class GameplaySettingSection : SettingSection
{
    [Header("Gameplay Settings")]
    public CustomToggle damageTextToggle;
    public CustomToggle screenShakeToggle;

    private SettingsManager SM => SettingsManager.Instance;

    public override void Initialize()
    {
        base.Initialize();
        if (SM == null) return;

        if (damageTextToggle != null) damageTextToggle.onValueChanged.AddListener(OnDamageTextChanged);
        if (screenShakeToggle != null) screenShakeToggle.onValueChanged.AddListener(OnScreenShakeChanged);
    }

    public override void Refresh()
    {
        base.Refresh();
        if (SM == null) return;

        if (damageTextToggle != null) damageTextToggle.SetIsOnWithoutNotify(SM.isDamageTextEnabled);
        if (screenShakeToggle != null) screenShakeToggle.SetIsOnWithoutNotify(SM.isScreenShakeEnabled);
    }

    public override void ResetToDefaults()
    {
        if (SM == null) return;

        // 기본값: DamageText ON, ScreenShake ON
        SM.SetDamageText(true);
        SM.SetScreenShake(true);

        Refresh();
    }

    private void OnDamageTextChanged(bool isOn) => SM.SetDamageText(isOn);
    private void OnScreenShakeChanged(bool isOn) => SM.SetScreenShake(isOn);
}
