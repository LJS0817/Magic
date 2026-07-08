using UnityEngine;

namespace Magic.Combat
{
    public class EnemyController : MonoBehaviour
    {
        public float maxHealth = 100f;
        public float health = 100f;
        public float attackDamage = 15f;

        private void Start()
        {
            health = maxHealth;
        }

        public void TakeDamage(float damage, SpellElement element = SpellElement.None)
        {
            health -= damage;

            string elementColor = "orange";
            string elementName = "";
            switch (element)
            {
                case SpellElement.Fire: elementColor = "red"; elementName = " Fire"; break;
                case SpellElement.Ice: elementColor = "aqua"; elementName = " Ice"; break;
                case SpellElement.Lightning: elementColor = "yellow"; elementName = " Lightning"; break;
                case SpellElement.Earth: elementColor = "#8B4513"; elementName = " Earth"; break;
            }

            Debug.Log($"<color={elementColor}>[Enemy] Took {damage}{elementName} damage! Remaining Health: {health}</color>");
            
            if (health <= 0)
            {
                Die();
            }
        }

        public void PrepareAttack()
        {
            Debug.Log("<color=red>[Enemy] Preparing to attack!</color>");
            // 몬스터가 공격 모션을 시작하거나 경고를 표시합니다.
            // 그런 다음 CombatManager에 패링 윈도우를 열라고 알립니다.
            CombatManager.Instance.OpenParryWindow();
        }

        public void ExecuteAttack()
        {
            Debug.Log("<color=red>[Enemy] Attack Executed!</color>");
            if (CombatManager.Instance.player != null)
            {
                CombatManager.Instance.player.TakeDamage(attackDamage);
            }
        }

        public void OnParried()
        {
            Debug.Log("<color=cyan>[Enemy] Attack was Parried! Staggered!</color>");
            // 패링 성공 시 적 스턴 모션 등의 처리
        }

        private void Die()
        {
            Debug.Log("<color=gray>[Enemy] Defeated!</color>");
            // Destroy(gameObject); // 일단은 놔둠 (CombatManager에서 체크)
        }
    }
}
