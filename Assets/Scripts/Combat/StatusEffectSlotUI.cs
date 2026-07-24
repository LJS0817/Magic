using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StatusEffectSlotUI : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text durationText;

    private StatusEffectType currentType;

    public void Setup(StatusEffectType type, float duration, Sprite iconSprite)
    {
        currentType = type;
        
        if (iconImage != null && iconSprite != null)
        {
            iconImage.sprite = iconSprite;
        }

        UpdateDuration(duration);
    }

    public void UpdateDuration(float duration)
    {
        if (durationText != null)
        {
            // 보통 턴제에서 지속시간은 정수(턴 수)로 표시하지만, 
            // 실시간이라면 소수점 한자리 등 필요에 맞게 포맷할 수 있습니다.
            durationText.text = Mathf.CeilToInt(duration).ToString();
        }
    }

    public StatusEffectType GetEffectType()
    {
        return currentType;
    }
}
