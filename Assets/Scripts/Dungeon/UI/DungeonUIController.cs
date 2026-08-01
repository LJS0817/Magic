using UnityEngine;

public class DungeonUIController : MonoBehaviour
{
    [SerializeField] private CustomSlider apSlider;
    
    private void Start()
    {
        if (DungeonManager.Instance != null)
        {
            DungeonManager.Instance.OnAPChanged += UpdateAPUI;
            UpdateAPUI(DungeonManager.Instance.currentAP);
        }
    }
    
    private void UpdateAPUI(int currentAP)
    {
        if (apSlider != null)
        {
            apSlider.SetValue(currentAP, DungeonManager.Instance.initialAP);
        }
    }
    
    private void OnDestroy()
    {
        if (DungeonManager.Instance != null)
        {
            DungeonManager.Instance.OnAPChanged -= UpdateAPUI;
        }
    }
}
