using UnityEngine;
using System.Collections.Generic;
using Magic.Combat;

namespace Magic.Combat
{
    public enum CombatState
    {
        Idle,
        PlayerTurn,
        EnemyTurn,
        BattleEnd
    }

    public class CombatManager : MonoBehaviour
    {
        public static CombatManager Instance { get; private set; }

        public CombatState CurrentState { get; private set; } = CombatState.Idle;

        [Header("ATB Settings")]
        public float maxATB = 100f;
        public float playerATBFillRate = 20f; // per second
        public float enemyATBFillRate = 15f; // per second

        public float currentPlayerATB = 0f;
        public float currentEnemyATB = 0f;

        [Header("Turn Settings")]
        public float playerTurnTimeLimit = 5f; // 5 seconds to draw
        public float currentPlayerTurnTimer = 0f;

        public float parryWindowDuration = 1.5f; // Time player has to parry when enemy attacks
        public float currentParryTimer = 0f;
        public bool IsParryWindowOpen { get; private set; } = false;

        public PlayerController player;
        public EnemyController enemy;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void Start()
        {
            if (player == null) player = FindObjectOfType<PlayerController>();
            if (enemy == null) enemy = FindObjectOfType<EnemyController>();

            CurrentState = CombatState.Idle;
        }

        private void Update()
        {
            if (CurrentState == CombatState.BattleEnd) return;

            if (player == null || enemy == null) return;
            
            if (player.health <= 0 || enemy.health <= 0)
            {
                CurrentState = CombatState.BattleEnd;
                Debug.Log("[Combat] Battle Ended!");
                return;
            }

            switch (CurrentState)
            {
                case CombatState.Idle:
                    FillATBGauges();
                    break;
                case CombatState.PlayerTurn:
                    HandlePlayerTurn();
                    break;
                case CombatState.EnemyTurn:
                    HandleEnemyTurn();
                    break;
            }
        }

        private void FillATBGauges()
        {
            currentPlayerATB += playerATBFillRate * Time.deltaTime;
            currentEnemyATB += enemyATBFillRate * Time.deltaTime;

            if (currentPlayerATB >= maxATB)
            {
                currentPlayerATB = maxATB;
                StartPlayerTurn();
            }
            else if (currentEnemyATB >= maxATB)
            {
                currentEnemyATB = maxATB;
                StartEnemyTurn();
            }
        }

        private void StartPlayerTurn()
        {
            CurrentState = CombatState.PlayerTurn;
            currentPlayerTurnTimer = playerTurnTimeLimit;
            Debug.Log("<color=green>[Combat] Player's Turn Started!</color>");
        }

        private void HandlePlayerTurn()
        {
            currentPlayerTurnTimer -= Time.deltaTime;
            if (currentPlayerTurnTimer <= 0)
            {
                Debug.Log("<color=red>[Combat] Player Turn Time out!</color>");
                EndPlayerTurn();
            }
        }

        public void EndPlayerTurn()
        {
            currentPlayerATB = 0f;
            CurrentState = CombatState.Idle;
        }

        private void StartEnemyTurn()
        {
            CurrentState = CombatState.EnemyTurn;
            Debug.Log("<color=red>[Combat] Enemy's Turn Started!</color>");
            enemy.PrepareAttack();
        }

        public void OpenParryWindow()
        {
            IsParryWindowOpen = true;
            currentParryTimer = parryWindowDuration;
            Debug.Log("<color=yellow>[Combat] PARRY WINDOW OPEN!</color>");
        }

        private void HandleEnemyTurn()
        {
            if (IsParryWindowOpen)
            {
                currentParryTimer -= Time.deltaTime;
                if (currentParryTimer <= 0)
                {
                    IsParryWindowOpen = false;
                    enemy.ExecuteAttack();
                    EndEnemyTurn();
                }
            }
        }

        public void SuccessfulParry()
        {
            if (IsParryWindowOpen)
            {
                IsParryWindowOpen = false;
                Debug.Log("<color=cyan>[Combat] PARRY SUCCESSFUL!</color>");
                enemy.OnParried();
                EndEnemyTurn();
            }
        }

        public void EndEnemyTurn()
        {
            currentEnemyATB = 0f;
            IsParryWindowOpen = false;
            CurrentState = CombatState.Idle;
        }
    }
}
