using UnityEngine;
using TMPro;

public class DungeonUIController : MonoBehaviour
{
    [SerializeField] private CustomSlider apSlider;
    [SerializeField] private TMP_Text hoverTileInfoText;
    
    private void Start()
    {
        if (DungeonManager.Instance != null)
        {
            DungeonManager.Instance.OnAPChanged += UpdateAPUI;
            DungeonManager.Instance.OnTileHovered += UpdateHoverUI;
            UpdateAPUI(DungeonManager.Instance.currentAP);
        }
        
        if (hoverTileInfoText != null)
        {
            hoverTileInfoText.text = "";
        }
    }
    
    private void UpdateAPUI(int currentAP)
    {
        if (apSlider != null)
        {
            apSlider.SetValue(currentAP, DungeonManager.Instance.initialAP);
        }
    }
    
    private void UpdateHoverUI(HexTileData tileData)
    {
        if (hoverTileInfoText == null) return;
        
        if (tileData == null)
        {
            hoverTileInfoText.text = "";
            return;
        }

        bool isPeeked = DungeonManager.Instance.PeekedTilePos.HasValue && 
                        DungeonManager.Instance.PeekedTilePos.Value == tileData.position;

        if (!tileData.IsDiscovered && !isPeeked)
        {
            hoverTileInfoText.text = "미탐험 구역";
        }
        else
        {
            hoverTileInfoText.text = GetTileTypeName(tileData.Type);
        }
    }

    private string GetTileTypeName(HexTileType type)
    {
        switch (type)
        {
            case HexTileType.Empty: return "평범한 바닥";
            case HexTileType.NormalMonster: return "일반 몬스터";
            case HexTileType.Boss: return "보스";
            case HexTileType.Trap: return "함정";
            case HexTileType.Obstacle: return "장애물";

            case HexTileType.RandomPortal: return "무작위 포탈";
            case HexTileType.TreasureBox: return "보물상자";
            case HexTileType.Merchant: return "상인";
            case HexTileType.NPC: return "NPC";
            case HexTileType.Exit: return "출구";
            case HexTileType.Start: return "시작 지점";
            default: return "알 수 없음";
        }
    }
    
    private void OnDestroy()
    {
        if (DungeonManager.Instance != null)
        {
            DungeonManager.Instance.OnAPChanged -= UpdateAPUI;
            DungeonManager.Instance.OnTileHovered -= UpdateHoverUI;
        }
    }
}
