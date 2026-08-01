using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class CombatPreparationManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private PreparationInventoryUI _inventoryUI;
    [SerializeField] private PreparationLoadoutUI _loadoutUI;
    [SerializeField] private CanvasGroup _window;

    [Header("Buttons")]
    [SerializeField] private Button _startCombatButton;
    [SerializeField] private Button _clearAllButton;

    private void Start()
    {
        if (_startCombatButton != null) _startCombatButton.onClick.AddListener(StartCombat);
        if (_clearAllButton != null) _clearAllButton.onClick.AddListener(ClearLoadout);
        
        Close();
    }

    public void OpenPreparation()
    {
        _window.alpha = 1;
        _window.blocksRaycasts = true;
        _window.interactable = true;
    }

    public void Close()
    {
        _window.alpha = 0;
        _window.blocksRaycasts = false;
        _window.interactable = false;
    }

    private void StartCombat()
    {
        Close();
        SceneManager.LoadScene("CombatGameScene");
    }

    private void ClearLoadout()
    {
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.ClearLoadout();
        }
    }
}
