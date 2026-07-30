using UnityEngine;

[CreateAssetMenu(fileName = "New Quest", menuName = "Magic/Item/Quest")]
public class ItemQuestSO : ItemDataSO
{
    [Header("Quest Template Settings")]
    [Tooltip("{0}: 아이템 이름")]
    public string questTitleFormat = "{0} 납품 의뢰";
    [Tooltip("{0}: 아이템 이름, {1}: 수량")]
    public string questDescFormat = "모험자 길드에서 {0} {1}개를 급히 찾고 있습니다.";
    
    // 퀘스트 완료 조건 (동적 할당됨)
    [Header("Dynamic Data (Do not set in template)")]
    public ItemDataSO targetItem;
    public int targetAmount;
    public long rewardCopper;
    public int deadlineDays;

    private void OnEnable()
    {
        type = ItemType.Quest;
    }

    public ItemQuestSO CreateDynamicQuest(ItemDataSO target, int amount, long reward, int deadline)
    {
        ItemQuestSO dynamicQuest = Instantiate(this);
        dynamicQuest.name = this.name + "_Dynamic";
        dynamicQuest.targetItem = target;
        dynamicQuest.targetAmount = amount;
        dynamicQuest.rewardCopper = reward;
        dynamicQuest.deadlineDays = deadline;

        // 이름과 설명 동적 생성
        dynamicQuest.itemName = string.Format(questTitleFormat, target != null ? target.itemName : "알 수 없는 아이템");
        dynamicQuest.itemDescription = string.Format(questDescFormat, target != null ? target.itemName : "알 수 없는 아이템", amount);
        
        dynamicQuest.basePriceInCopper = 0; // 퀘스트 수주 보증금은 없음
        
        return dynamicQuest;
    }
}
