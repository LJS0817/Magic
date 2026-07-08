using UnityEngine;

namespace Magic.Combat
{
    public class PlayerController : MonoBehaviour
    {
        [Header("Status")]
        public float maxHealth = 100f;
        public float health = 100f;

        void Awake()
        {
            health = maxHealth;
        }

        void Update()
        {
            // Turn-based logic is handled by CombatManager
        }

        public void TakeDamage(float damage)
        {
            health -= damage;
            Debug.Log($"<color=orange>[Player] Took {damage} damage! Remaining Health: {health}</color>");
            
            if (health <= 0)
            {
                Die();
            }
        }

        private void Die()
        {
            Debug.Log("<color=gray>[Player] Defeated!</color>");
        }
    }
}
