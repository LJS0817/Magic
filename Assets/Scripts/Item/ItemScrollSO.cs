using UnityEngine;

[CreateAssetMenu(fileName = "New Scroll Data", menuName = "Magic/Items/Scroll Data")]
public class ItemScrollSO : ItemDataSO
{
    [Header("Scroll Specifics")]
    public string spellName = "";
    public int maxDurability = 5;
    public float accuracyScore = 1.0f; // 마법 완성 시 평균 점수 (기본 1.0)
    public SpellElement scrollElement = SpellElement.None;
    public bool isPreCompleted = false;
    public Sprite sampleImage; // 유저가 직접 그리지 않은(미리 완성된) 스크롤용 표시 이미지

    private void OnEnable()
    {
        type = ItemType.Scroll;
    }
}

