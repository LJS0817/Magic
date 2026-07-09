using UnityEngine;
using System.Collections.Generic;
using Magic.Combat;
using Magic.Drawing;

namespace Magic.Combat
{
    public enum CombatState
    {
        Idle,
        PlayerTurn,
        PlayerChanting,
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
                case CombatState.PlayerChanting:
                    // Chanting phase is primarily handled by CombatDrawingManager.
                    // ATB and Turn timers are paused here.
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

        public void StartPlayerChanting()
        {
            CurrentState = CombatState.PlayerChanting;
            Debug.Log("<color=magenta>[Combat] Chanting Phase Started!</color>");
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
                    enemy.ExecuteAttack(1f); // 패링 실패: 100% 데미지
                    EndEnemyTurn();
                }
            }
        }

        public void EvaluateParryClash(string spellName, SpellType spellType, SpellElement playerElement)
        {
            if (!IsParryWindowOpen) return;
            IsParryWindowOpen = false;

            if (spellType == Magic.Drawing.SpellType.Defense)
            {
                Debug.Log($"<color=cyan>[Combat] 방어 마법({spellName}) 전개! 데미지 80% 경감!</color>");
                enemy.ExecuteAttack(0.2f); // 방어 시 20% 데미지
                EndEnemyTurn();
                return;
            }

            SpellElement enemyElement = enemy.currentAttackElement;
            bool isAdvantage = CheckAdvantage(playerElement, enemyElement);

            if (isAdvantage)
            {
                // 패링 성공! 상성 우위
                Debug.Log($"<color=lime>[Combat] 패링 성공! {playerElement} 속성이 적의 {enemyElement} 속성을 파훼했습니다!</color>");
                enemy.OnParried();
                enemy.TakeDamage(30f, playerElement); // 카운터 데미지
                EndEnemyTurn();
            }
            else
            {
                // 상성 열위 또는 무속성 -> 크로스 카운터 (적은 데미지 안입고 유저만 피해 입음 - 수정된 룰 적용)
                Debug.Log($"<color=red>[Combat] 패링 실패! 상성({playerElement} vs {enemyElement})이 불리하여 마법이 상쇄되었습니다!</color>");
                enemy.ExecuteAttack(1f); // 유저 피해 100%
                // 적 데미지 생략
                EndEnemyTurn();
            }
        }

        private bool CheckAdvantage(SpellElement player, SpellElement enemy)
        {
            if (player == SpellElement.Fire && enemy == SpellElement.Ice) return true;
            if (player == SpellElement.Ice && enemy == SpellElement.Lightning) return true;
            if (player == SpellElement.Lightning && enemy == SpellElement.Earth) return true;
            if (player == SpellElement.Earth && enemy == SpellElement.Fire) return true;
            return false;
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
