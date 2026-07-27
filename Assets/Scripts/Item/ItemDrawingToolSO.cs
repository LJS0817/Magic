using UnityEngine;

public enum DrawingToolShape
{
    Circle,     // 원
    Triangle,   // 세모
    Square,     // 네모
    Rhombus     // 마름모
}

[CreateAssetMenu(fileName = "New Drawing Tool", menuName = "Magic/Item/DrawingTool")]
public class ItemDrawingToolSO : ItemDataSO
{
    [Header("Drawing Tool Specifics")]
    [Tooltip("찍어낼 모양 (원, 세모, 네모, 마름모)")]
    public DrawingToolShape targetShape = DrawingToolShape.Circle;
    
    [Tooltip("도장 사용 시 적용될 마법 완성도 (정확도 보정)")]
    [Range(0.8f, 1.0f)]
    public float accuracyBonus = 0.95f;

    [Tooltip("도장 사용 시 소모되는 잉크량 배율")]
    public float inkConsumptionMultiplier = 1.0f;

    private void Reset()
    {
        type = ItemType.DrawingTool;
    }

    private void OnValidate()
    {
        type = ItemType.DrawingTool;
    }

    public string GetShapeName()
    {
        switch (targetShape)
        {
            case DrawingToolShape.Circle: return "Circle";
            case DrawingToolShape.Triangle: return "Triangle";
            case DrawingToolShape.Square: return "Square";
            case DrawingToolShape.Rhombus: return "Rhombus";
            default: return "Circle";
        }
    }
}
