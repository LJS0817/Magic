using UnityEngine;
using Magic.Data;
using UnityEngine.SceneManagement;

namespace Magic.Combat
{
    public class PlayerController : MonoBehaviour
    {
        [Header("Status")]
        public float maxHealth 
        {
            get => PlayerDataManager.Instance != null ? PlayerDataManager.Instance.GetMaxHealth() : 100f;
            set { if (PlayerDataManager.Instance != null) PlayerDataManager.Instance.baseMaxHealth = value; }
        }

        public float health 
        {
            get => PlayerDataManager.Instance != null ? PlayerDataManager.Instance.currentHealth : 100f;
            set { if (PlayerDataManager.Instance != null) PlayerDataManager.Instance.currentHealth = value; }
        }

        void Awake()
        {
            // Initialization is handled by PlayerDataManager
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

        public void Change()
        {
            SceneManager.LoadScene("StoreGameScene");
        }
    }
}
