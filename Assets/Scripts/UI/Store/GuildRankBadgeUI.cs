using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class GuildRankBadgeUI : MonoBehaviour
{
    [SerializeField] private Image badgeImage;
    
    [Header("Rank Sprites")]
    [SerializeField] private Sprite ironSprite;
    [SerializeField] private Sprite bronzeSprite;
    [SerializeField] private Sprite silverSprite;
    [SerializeField] private Sprite goldSprite;
    [SerializeField] private Sprite mithrilSprite;
    [SerializeField] private Sprite orichalcumSprite;

    private void Start()
    {
        if (badgeImage == null)
        {
            badgeImage = GetComponent<Image>();
        }

        if (PlayerDataManager.Instance != null)
        {
            PlayerDataManager.Instance.OnGuildRankChanged += UpdateBadge;
            // 초기화
            UpdateBadge(PlayerDataManager.Instance.CurrentGuildRank);
        }
    }

    private void OnDestroy()
    {
        if (PlayerDataManager.Instance != null)
        {
            PlayerDataManager.Instance.OnGuildRankChanged -= UpdateBadge;
        }
    }

    private void UpdateBadge(GuildRank rank)
    {
        if (badgeImage == null) return;

        switch (rank)
        {
            case GuildRank.Iron:
                badgeImage.sprite = ironSprite;
                break;
            case GuildRank.Bronze:
                badgeImage.sprite = bronzeSprite;
                break;
            case GuildRank.Silver:
                badgeImage.sprite = silverSprite;
                break;
            case GuildRank.Gold:
                badgeImage.sprite = goldSprite;
                break;
            case GuildRank.Mithril:
                badgeImage.sprite = mithrilSprite;
                break;
            case GuildRank.Orichalcum:
                badgeImage.sprite = orichalcumSprite;
                break;
        }
    }
}
