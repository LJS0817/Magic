using UnityEngine;
using UnityEngine.EventSystems;

public class HexTile : MonoBehaviour, IPointerClickHandler
{
    public int q;
    public int r;
    
    public HexTileType Type { get; private set; }
    public HexTileEventData EventData { get; private set; }
    public bool IsDiscovered { get; private set; }
    public bool IsEventCleared { get; private set; }
    public bool isTrapRevealed { get; private set; }
    
    [SerializeField] private GameObject fogVisual;
    [SerializeField] private GameObject highlightVisual;
    [SerializeField] private SpriteRenderer iconRenderer;

    public void Init(int q, int r, HexTileEventData eventData)
    {
        this.q = q;
        this.r = r;
        this.EventData = eventData;
        this.Type = eventData.type;
        this.IsDiscovered = false;
        
        // 시작 지점이나 빈 공간 등은 처음부터 클리어된 것으로 간주
        this.IsEventCleared = (Type == HexTileType.Start || Type == HexTileType.Empty || Type == HexTileType.Exit);
        UpdateVisuals();
    }

    public void Discover()
    {
        IsDiscovered = true;
        UpdateVisuals();
    }

    public void ClearEvent()
    {
        IsEventCleared = true;
        UpdateVisuals();
    }
    
    public void RevealTrap()
    {
        isTrapRevealed = true;
        UpdateVisuals();
    }
    
    public void SetHighlight(bool show)
    {
        if (highlightVisual != null) highlightVisual.SetActive(show);
    }

    private void UpdateVisuals()
    {
        if (fogVisual != null) fogVisual.SetActive(!IsDiscovered);
        
        if (iconRenderer != null)
        {
            if (Type == HexTileType.Trap && !isTrapRevealed)
            {
                iconRenderer.enabled = false;
            }
            else
            {
                iconRenderer.enabled = IsDiscovered && !IsEventCleared;
            }
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (DungeonManager.Instance != null)
            {
                DungeonManager.Instance.MoveTo(new Vector2Int(q, r));
            }
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            if (DungeonManager.Instance != null)
            {
                DungeonManager.Instance.InspectTile(new Vector2Int(q, r));
            }
        }
    }
}
