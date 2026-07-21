using UnityEngine;
using DG.Tweening;
using Magic.Inventory;
using Magic.UI;

using System.Collections.Generic;

namespace Magic.Combat
{
    public enum StatusEffectType
    {
        None,
        Stun,
        Confusion,
        Burn,
        Freeze
    }

    [System.Serializable]
    public class ActiveStatusEffect
    {
        public StatusEffectType type;
        public float duration;
    }

    public class EnemyController : MonoBehaviour
    {
        public float maxHealth = 100f;
        public float health = 100f;
        public float attackDamage = 15f;
        public SpellElement currentAttackElement = SpellElement.None;

        [Header("Visuals")]
        public SpriteRenderer spriteRenderer;
        public CustomSlider hpSlider;
        private Vector3 originalPosition;

        [Header("Status Effects")]
        public List<ActiveStatusEffect> activeEffects = new List<ActiveStatusEffect>();

        private void Start()
        {
            originalPosition = transform.position;
            if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();

            Spawn(); // Initial spawn for wave 1
        }

        public void OnTurnEnd()
        {
            // 턴 종료 시 상태이상 지속시간 1 감소
            for (int i = activeEffects.Count - 1; i >= 0; i--)
            {
                activeEffects[i].duration -= 1f;
                if (activeEffects[i].duration <= 0)
                {
                    activeEffects.RemoveAt(i);
                }
            }
        }

        public void ApplyStatusEffect(StatusEffectType type, float duration)
        {
            var existing = activeEffects.Find(e => e.type == type);
            if (existing != null)
            {
                existing.duration = Mathf.Max(existing.duration, duration); // Refresh duration
            }
            else
            {
                activeEffects.Add(new ActiveStatusEffect { type = type, duration = duration });
                OnStatusEffectAdded(type);
            }
        }

        private void OnStatusEffectAdded(StatusEffectType type)
        {
            if (spriteRenderer != null)
            {
                if (type == StatusEffectType.Confusion)
                {
                    spriteRenderer.DOColor(new Color(0.8f, 0.2f, 0.8f), 0.3f).SetLoops(2, LoopType.Yoyo).OnComplete(() => spriteRenderer.color = Color.white);
                }
                else if (type == StatusEffectType.Stun)
                {
                    spriteRenderer.DOColor(Color.gray, 0.3f).SetLoops(2, LoopType.Yoyo).OnComplete(() => spriteRenderer.color = Color.white);
                }
            }
        }

        public bool IsStunned() => activeEffects.Exists(e => e.type == StatusEffectType.Stun);
        public bool IsConfused() => activeEffects.Exists(e => e.type == StatusEffectType.Confusion);

        public void Spawn()
        {
            activeEffects.Clear();
            health = maxHealth;
            transform.DOComplete();
            transform.position = originalPosition;
            
            if (spriteRenderer != null)
            {
                Color c = spriteRenderer.color;
                c.a = 0f;
                spriteRenderer.color = c;
                spriteRenderer.DOFade(1f, 0.8f);
            }

            transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
            transform.DOScale(Vector3.one, 0.8f).SetEase(Ease.OutBounce);
            
            if (hpSlider != null) hpSlider.SetValue(health, maxHealth);
        }

        public void TakeDamage(float damage, SpellElement element = SpellElement.None)
        {
            health -= damage;
            if (hpSlider != null) hpSlider.SetValue(Mathf.Max(0, health), maxHealth);

            Color flashColor = Color.white;
            string elementColor = "orange";
            string elementName = "";
            switch (element)
            {
                case SpellElement.Fire: elementColor = "red"; elementName = " Fire"; flashColor = Color.red; break;
                case SpellElement.Ice: elementColor = "aqua"; elementName = " Ice"; flashColor = Color.cyan; break;
                case SpellElement.Lightning: elementColor = "yellow"; elementName = " Lightning"; flashColor = Color.yellow; break;
                case SpellElement.Earth: elementColor = "#8B4513"; elementName = " Earth"; flashColor = new Color(0.5f, 0.25f, 0f); break;
            }

            Debug.Log($"<color={elementColor}>[Enemy] Took {damage}{elementName} damage! Remaining Health: {health}</color>");
            
            // 피격 애니메이션: 붉은색으로 변했다가 흰색(FFF)으로 복귀
            if (spriteRenderer != null)
            {
                spriteRenderer.DOColor(Color.red, 0.1f).SetLoops(2, LoopType.Yoyo).OnComplete(() => spriteRenderer.color = Color.white);
            }
            transform.DOComplete(); // 진행 중인 쉐이크 취소
            transform.DOShakePosition(0.3f, strength: new Vector3(0.5f, 0f, 0f), vibrato: 20);

            if (health <= 0)
            {
                Die();
            }
        }

        public void PrepareAttack()
        {
            // 화상 데미지 처리 (턴 시작 시)
            if (activeEffects.Exists(e => e.type == StatusEffectType.Burn))
            {
                Debug.Log("<color=red>[Enemy] 화상(Burn) 지속 피해를 입습니다!</color>");
                TakeDamage(15f); // 턴 당 데미지
                if (spriteRenderer != null)
                {
                    spriteRenderer.DOColor(Color.red, 0.2f).SetLoops(2, LoopType.Yoyo).OnComplete(() => spriteRenderer.color = Color.white);
                }
            }

            // 사망 시 바로 리턴
            if (health <= 0) return;

            // 기절 처리
            if (IsStunned())
            {
                Debug.Log("<color=gray>[Enemy] 기절(Stun)하여 행동하지 못하고 턴을 넘깁니다!</color>");
                if (CombatManager.Instance != null)
                {
                    CombatManager.Instance.EndEnemyTurn();
                }
                return;
            }

            if (IsConfused())
            {
                Debug.Log("<color=magenta>[Enemy] Enemy is Confused!</color>");
                if (Random.value > 0.5f)
                {
                    Debug.Log("<color=magenta>[Enemy] It attacked itself in confusion!</color>");
                    TakeDamage(attackDamage);
                    transform.DOShakePosition(0.5f, 0.3f, 20);
                }
                else
                {
                    Debug.Log("<color=magenta>[Enemy] It skipped its turn in confusion!</color>");
                }
                
                // End turn immediately without attacking player
                if (CombatManager.Instance != null)
                {
                    CombatManager.Instance.EndEnemyTurn();
                }
                return;
            }

            // 무작위 속성 선택 (None 제외 1~4)
            currentAttackElement = (SpellElement)Random.Range(1, 5);

            string elementColor = "white";
            switch (currentAttackElement)
            {
                case SpellElement.Fire: elementColor = "red"; break;
                case SpellElement.Ice: elementColor = "aqua"; break;
                case SpellElement.Lightning: elementColor = "yellow"; break;
                case SpellElement.Earth: elementColor = "#8B4513"; break;
            }

            Debug.Log($"<color=red>[Enemy] Preparing to attack with </color><color={elementColor}>{currentAttackElement}</color><color=red> element!</color>");
            
            // 공격 준비 애니메이션: 살짝 작아지며 붉게 빛남 (Anticipation)
            transform.DOScale(0.85f, 0.5f).SetEase(Ease.OutQuad);
            if (spriteRenderer != null) spriteRenderer.DOColor(new Color(1f, 0.5f, 0.5f), 0.5f);

            CombatManager.Instance.OpenParryWindow();
        }

        public void ExecuteAttack(float damageMultiplier = 1f)
        {
            Debug.Log($"<color=red>[Enemy] Attack Executed! (Multiplier: {damageMultiplier:F1})</color>");
            
            // 공격 실행 애니메이션: 크기가 커지며 플레이어에게 다가오는 연출 후 원래 크기로 복귀
            transform.DOScale(1.5f, 0.1f).SetEase(Ease.InExpo).OnComplete(() =>
            {
                transform.DOScale(1.0f, 0.3f).SetEase(Ease.OutQuad);
                if (spriteRenderer != null) spriteRenderer.color = Color.white;
            });

            if (CombatManager.Instance.player != null)
            {
                CombatManager.Instance.player.TakeDamage(attackDamage * damageMultiplier);
            }
        }

        public void OnParried()
        {
            Debug.Log("<color=cyan>[Enemy] Attack was Parried! Staggered!</color>");
            // 패링 성공 시 크게 작아지며 스턴 (뒤로 밀려나는 느낌)
            transform.DOComplete();
            transform.DOScale(0.7f, 0.2f).SetEase(Ease.OutBack).OnComplete(() =>
            {
                transform.DOShakePosition(1f, new Vector3(0.2f, 0f, 0f), 10);
                transform.DOScale(1.0f, 0.5f).SetDelay(1f);
            });
            if (spriteRenderer != null) spriteRenderer.color = Color.white;
        }

        private void Die()
        {
            Debug.Log("<color=gray>[Enemy] Defeated!</color>");
            
            // --- 전리품 지급 로직 추가 ---
            if (InventoryManager.Instance != null)
            {
                var lootSO = ScriptableObject.CreateInstance<ItemMaterialSO>();
                lootSO.itemName = "하급 마석";
                lootSO.basePriceInCopper = Random.Range(30, 80); // 30~80 동화 사이의 가치
                
                var lootInstance = new Item_Material(lootSO);
                InventoryManager.Instance.AddItem(lootInstance);
                Debug.Log($"<color=yellow>[Loot] {lootSO.itemName}을(를) 획득했습니다! (판매가: {lootSO.basePriceInCopper} Copper)</color>");

                // --- Drop Rate Bonus 적용 ---
                if (Magic.Upgrade.UpgradeManager.Instance != null)
                {
                    float dropBonus = Magic.Upgrade.UpgradeManager.Instance.GetTotalUpgradeValue(Magic.Upgrade.UpgradeType.DropRateBonus);
                    
                    while (dropBonus >= 100f)
                    {
                        var extraLoot = new Item_Material(lootSO);
                        InventoryManager.Instance.AddItem(extraLoot);
                        Debug.Log($"<color=yellow>[Loot Bonus] {lootSO.itemName} 추가 획득!</color>");
                        dropBonus -= 100f;
                    }

                    if (Random.value * 100f < dropBonus)
                    {
                        var extraLoot = new Item_Material(lootSO);
                        InventoryManager.Instance.AddItem(extraLoot);
                        Debug.Log($"<color=yellow>[Loot Bonus] 아이템 드롭 확률 증가({dropBonus}%) 발동! {lootSO.itemName} 추가 획득!</color>");
                    }
                }
            }

            // 사망 애니메이션: 크기가 0.3으로 줄어들고 투명해짐 (등장의 역순)
            transform.DOComplete();
            transform.DOScale(new Vector3(0.3f, 0.3f, 0.3f), 0.8f).SetEase(Ease.InBack);
            if (spriteRenderer != null) spriteRenderer.DOFade(0f, 0.8f);
            
            // 애니메이션 종료 후 다음 웨이브 시작 알림
            DOVirtual.DelayedCall(0.8f, () => 
            {
                if (CombatManager.Instance != null)
                {
                    CombatManager.Instance.OnEnemyDefeated();
                }
            });
        }
    }
}
