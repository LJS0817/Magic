using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class StatusEffectIconMapping
{
    public StatusEffectType type;
    public Sprite icon;
}

public class StatusEffectContainerUI : MonoBehaviour
{
    [SerializeField] private StatusEffectSlotUI slotPrefab;
    [SerializeField] private Transform container;
    
    // 에디터에서 각 상태 이상 타입에 맞는 아이콘을 할당하기 위한 리스트
    [SerializeField] private List<StatusEffectIconMapping> iconMappings = new List<StatusEffectIconMapping>();

    private Dictionary<StatusEffectType, Sprite> iconDict = new Dictionary<StatusEffectType, Sprite>();
    private List<StatusEffectSlotUI> activeSlots = new List<StatusEffectSlotUI>();

    private void Awake()
    {
        // 최적화를 위해 리스트를 딕셔너리로 변환
        foreach (var mapping in iconMappings)
        {
            if (!iconDict.ContainsKey(mapping.type))
            {
                iconDict.Add(mapping.type, mapping.icon);
            }
        }
    }

    public void UpdateEffects(List<ActiveStatusEffect> effects)
    {
        // 1. 필요한 슬롯 개수 맞추기 (오브젝트 풀링 대용)
        while (activeSlots.Count < effects.Count)
        {
            StatusEffectSlotUI newSlot = Instantiate(slotPrefab, container);
            activeSlots.Add(newSlot);
        }

        // 2. 현재 상태 이상 데이터로 슬롯 업데이트 및 활성화
        for (int i = 0; i < effects.Count; i++)
        {
            var effect = effects[i];
            var slot = activeSlots[i];
            
            Sprite icon = iconDict.ContainsKey(effect.type) ? iconDict[effect.type] : null;
            slot.Setup(effect.type, effect.duration, icon);
            slot.gameObject.SetActive(true);
        }

        // 3. 남는 슬롯들은 비활성화
        for (int i = effects.Count; i < activeSlots.Count; i++)
        {
            activeSlots[i].gameObject.SetActive(false);
        }
    }
}
