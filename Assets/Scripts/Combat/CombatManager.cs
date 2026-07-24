using UnityEngine;
using System.Collections.Generic;

public enum CombatState
{
    Idle,
    PlayerTurn,
    PlayerChanting,
    EnemyTurn,
    WaveTransition,
    FloorCleared,
    BattleEnd
}

public class CombatManager : MonoBehaviour
{
    public static CombatManager Instance { get; private set; }

    public CombatState CurrentState { get; private set; } = CombatState.Idle;

    [Header("Dungeon Progression")]
    public int currentFloor = 1;
    public int currentWave = 1;

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

    [Header("Enemy Management")]
    public List<EnemyDataSO> normalEnemies;
    public List<EnemyDataSO> bossEnemies;
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
        if (CurrentState == CombatState.BattleEnd || CurrentState == CombatState.FloorCleared || CurrentState == CombatState.WaveTransition) return;

        if (player == null || enemy == null) return;
        
        if (player.health <= 0)
        {
            CurrentState = CombatState.BattleEnd;
            Debug.Log("[Combat] Battle Ended! Player Defeated.");
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
        
        if (enemy != null)
        {
            currentEnemyATB += enemyATBFillRate * Time.deltaTime;
        }

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

    public void EvaluateParryClash(string spellName, SpellType spellType, List<SpellElement> playerElements)
    {
        if (!IsParryWindowOpen) return;
        IsParryWindowOpen = false;

        if (spellType == SpellType.Defense)
        {
            Debug.Log($"<color=cyan>[Combat] 방어 마법({spellName}) 전개! 데미지 80% 경감!</color>");
            enemy.ExecuteAttack(0.2f); // 방어 시 20% 데미지
            EndEnemyTurn();
            return;
        }

        SpellElement enemyElement = enemy.currentAttackElement;
        bool isAdvantage = false;
        SpellElement successfulElement = SpellElement.None;
        
        foreach (var el in playerElements)
        {
            if (CheckAdvantage(el, enemyElement))
            {
                isAdvantage = true;
                successfulElement = el;
                break;
            }
        }

        if (isAdvantage)
        {
            // 패링 성공! 상성 우위
            Debug.Log($"<color=lime>[Combat] 패링 성공! {successfulElement} 속성이 적의 {enemyElement} 속성을 파훼했습니다!</color>");
            enemy.OnParried();
            
            // 적용된 모든 속성으로 피해를 입힘
            foreach (var el in playerElements)
            {
                enemy.TakeDamage(30f, el); // 카운터 데미지
            }
            
            EndEnemyTurn();
        }
        else
        {
            string elementsString = string.Join(", ", playerElements);
            // 상성 열위 또는 무속성 -> 크로스 카운터 (적은 데미지 안입고 유저만 피해 입음 - 수정된 룰 적용)
            Debug.Log($"<color=red>[Combat] 패링 실패! 상성({elementsString} vs {enemyElement})이 불리하여 마법이 상쇄되었습니다!</color>");
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
        if (enemy != null)
        {
            enemy.OnTurnEnd();
        }
        currentEnemyATB = 0f;
        IsParryWindowOpen = false;
        CurrentState = CombatState.Idle;
    }

    public void OnEnemyDefeated()
    {
        if (currentWave < 4) // 1, 2, 3 are normal waves, 4 is boss
        {
            currentWave++;
            StartCoroutine(StartNextWaveCoroutine());
        }
        else
        {
            // Boss defeated!
            CurrentState = CombatState.FloorCleared;
            Debug.Log($"<color=yellow>[Combat] Floor {currentFloor} Cleared!</color>");
        }
    }

    private System.Collections.IEnumerator StartNextWaveCoroutine()
    {
        CurrentState = CombatState.WaveTransition;
        yield return new WaitForSeconds(1.5f);
        
        // Set stats based on floor and wave
        float healthMultiplier = 1f + (currentFloor - 1) * 0.5f + (currentWave - 1) * 0.2f;
        float damageMultiplier = 1f + (currentFloor - 1) * 0.2f + (currentWave - 1) * 0.1f;
        
        if (currentWave == 4)
        {
            if (bossEnemies != null && bossEnemies.Count > 0)
            {
                enemy.enemyData = bossEnemies[Random.Range(0, bossEnemies.Count)];
            }
            healthMultiplier *= 2.0f; // Boss is twice as tanky
            damageMultiplier *= 1.5f;
            Debug.Log($"<color=red>[Combat] Floor {currentFloor} - BOSS BATTLE START!</color>");
        }
        else
        {
            if (normalEnemies != null && normalEnemies.Count > 0)
            {
                enemy.enemyData = normalEnemies[Random.Range(0, normalEnemies.Count)];
            }
            Debug.Log($"<color=cyan>[Combat] Floor {currentFloor} - Wave {currentWave} Start!</color>");
        }

        enemy.Spawn(healthMultiplier, damageMultiplier);
        
        currentPlayerATB = 0f;
        currentEnemyATB = 0f;
        CurrentState = CombatState.Idle;
    }

    public void StartNextFloor()
    {
        currentFloor++;
        currentWave = 1;
        StartCoroutine(StartNextWaveCoroutine());
    }

    private void OnGUI()
    {
        GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
        labelStyle.fontSize = 20;
        labelStyle.alignment = TextAnchor.UpperCenter;
        labelStyle.richText = true;
        GUI.Label(new Rect(0, 20, Screen.width, 30), $"<color=white><b>{currentFloor}F - Wave {currentWave}/4 {(currentWave == 4 ? "(BOSS)" : "")}</b></color>", labelStyle);

        if (CurrentState == CombatState.FloorCleared)
        {
            GUIStyle btnStyle = new GUIStyle(GUI.skin.button);
            btnStyle.fontSize = 24;
            float width = 200f;
            float height = 60f;
            float x = (Screen.width - width) / 2f;
            float y = (Screen.height - height) / 2f;

            if (GUI.Button(new Rect(x, y, width, height), $"Next Floor ({currentFloor + 1}F)", btnStyle))
            {
                StartNextFloor();
            }
        }
    }
}

