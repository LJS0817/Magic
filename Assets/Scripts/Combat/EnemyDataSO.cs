using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemyData", menuName = "Magic/Enemy Data")]
public class EnemyDataSO : ScriptableObject
{
    public string enemyName = "Slime";
    public float maxHealth = 100f;
    public float attackDamage = 15f;
    public Sprite enemySprite;
    public SpellElement defaultAttackElement = SpellElement.None;
}
