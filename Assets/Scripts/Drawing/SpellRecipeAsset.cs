using UnityEngine;
using System.Collections.Generic;

namespace Magic.Drawing
{
    public enum RuleType
    {
        Inside,     // targetIndices[0] is Inside targetIndices[1]
        Surround,   // targetIndices[1..N] surrounds targetIndices[0]
        Aligned     // targetIndices[0..N] are aligned in a straight line
    }

    public enum SpellType
    {
        Attack,
        Defense,
        Utility
    }

    [System.Serializable]
    public struct RuleData
    {
        public RuleType RuleType;
        public int[] TargetIndices;
    }

    [CreateAssetMenu(fileName = "NewSpellRecipe", menuName = "Magic/Spell Recipe")]
    public class SpellRecipeAsset : ScriptableObject
    {
        public string SpellName;
        public SpellType Type = SpellType.Attack;
        public Sprite icon;
        
        [Tooltip("이 마법을 사용할 때 총 필요한 마나량 (그릴 때 50%, 발동 시 50% 소모)")]
        public float manaCost = 30f;
        
        [Header("Lore & Information")]
        [TextArea(3, 5)]
        [Tooltip("마법서에 기록될 판타지 느낌의 설명")]
        public string Description;

        [Tooltip("시스템적인 발동 조건 (예: 원 1개, 별 1개)")]
        public string ActivationCondition;

        [Tooltip("유저에게 보여줄 그리기 힌트 (예: 커다란 둥근 결계 속에 별을 새겨 넣으세요.)")]
        public string ActivationHint;

        [Header("Matching Rules")]
        [Tooltip("필요한 도형들의 이름 (예: Circle, Star, Star). 인덱스 0, 1, 2가 됨")]
        public string[] RequiredShapes;

        [Tooltip("공간 규칙들 (예: RuleType.Inside, TargetIndices=[1, 0] -> 1번(Star)이 0번(Circle) 안에 있어야 함)")]
        public RuleData[] SpatialRules;
    }
}
