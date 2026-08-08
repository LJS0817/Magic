using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TimeUIController : MonoBehaviour
{
    [Header("Day References")]
    [SerializeField] private CustomButton nextDayButton;
    [SerializeField] private TMP_Text dayText;

    private void Start()
    {
        if (nextDayButton != null)
        {
            nextDayButton.onClick.AddListener(OnNextDayClicked);
        }

        if (PlayerDataManager.Instance != null)
        {
            PlayerDataManager.Instance.OnDayChanged += UpdateDayUI;
            
            // Initial UI Update
            UpdateDayUI(PlayerDataManager.Instance.currentDay);
        }
    }

    private void OnDestroy()
    {
        if (PlayerDataManager.Instance != null)
        {
            PlayerDataManager.Instance.OnDayChanged -= UpdateDayUI;
        }
    }

    private void OnNextDayClicked()
    {
        if (PlayerDataManager.Instance != null)
        {
            PlayerDataManager.Instance.AdvanceDay();
        }
    }

    private void UpdateDayUI(int day)
    {
        if (dayText != null)
        {
            dayText.text = $"Day\n{day}";
        }
    }
}
