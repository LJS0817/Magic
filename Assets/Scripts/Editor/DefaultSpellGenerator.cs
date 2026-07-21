using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using Magic.Drawing;
using System.Collections.Generic;

namespace Magic.EditorTools
{
    public class DefaultSpellGenerator
    {
        [MenuItem("Magic/Generate Default Spells (All Shapes)")]
        public static void GenerateSpells()
        {
            string folderPath = "Assets/Resources/SpellRecipes";
            if (!AssetDatabase.IsValidFolder("Assets/Resources"))
                AssetDatabase.CreateFolder("Assets", "Resources");
            if (!AssetDatabase.IsValidFolder(folderPath))
                AssetDatabase.CreateFolder("Assets/Resources", "SpellRecipes");

            // --- [1~11: 기존 도형 스펠] ---
            CreateSpell(folderPath, "1_Orb", "Orb", SpellType.Attack, 50f,
                "가장 기초적인 마법 구체. 대기의 마나를 응축하여 적에게 날려보냅니다.",
                "원 1개", "둥근 구체를 형상화하여 단순한 원을 그리세요.",
                new string[] { "Circle" },
                new RuleData[0]);

            CreateSpell(folderPath, "2_Trinity", "Trinity", SpellType.Attack, 50f,
                "세 가지의 절대적인 원리가 일렬로 정렬할 때 발동되는 심판의 마법입니다.",
                "세모 3개 (일렬 배치)", "세 개의 성스러운 삼각형을 한 줄로 나란히 세우세요.",
                new string[] { "Triangle", "Triangle", "Triangle" },
                new RuleData[] { new RuleData { RuleType = RuleType.Aligned, TargetIndices = new int[] { 0, 1, 2 } } });

            CreateSpell(folderPath, "3_DivineShield", "DivineShield", SpellType.Defense, 20f,
                "신성한 빛으로 전신을 감싸 모든 사악한 공격을 완벽히 차단합니다.",
                "세모 1개, 원 1개 (세모 안에 원)", "견고한 성벽(세모) 중심에 순수한 마나(원)를 보호하듯 그리세요.",
                new string[] { "Triangle", "Circle" },
                new RuleData[] { new RuleData { RuleType = RuleType.Inside, TargetIndices = new int[] { 1, 0 } } });

            CreateSpell(folderPath, "4_DoubleStrike", "DoubleStrike", SpellType.Attack, 50f,
                "시간의 틈새를 베어내는 두 번의 연속된 물리 타격을 가합니다.",
                "세모 2개 (일렬 배치)", "무기의 궤적을 형상화한 두 개의 날(세모)을 일직선상에 그리세요.",
                new string[] { "Triangle", "Triangle" },
                new RuleData[] { new RuleData { RuleType = RuleType.Aligned, TargetIndices = new int[] { 0, 1 } } });

            CreateSpell(folderPath, "5_MagicMissile", "MagicMissile", SpellType.Attack, 50f,
                "마나로 이루어진 유도탄 세 발을 연속으로 발사합니다.",
                "원 3개 (일렬 배치)", "마력이 응집된 세 개의 둥근 구체를 한 줄로 띄우세요.",
                new string[] { "Circle", "Circle", "Circle" },
                new RuleData[] { new RuleData { RuleType = RuleType.Aligned, TargetIndices = new int[] { 0, 1, 2 } } });

            CreateSpell(folderPath, "6_Heal", "Heal", SpellType.Utility, 40f,
                "따스한 생명력으로 대상의 상처를 치유하고 활력을 되찾아줍니다.",
                "원 2개 (원 안에 원)", "작은 생명의 씨앗(원)을 더 큰 보호의 결계(원)로 감싸세요.",
                new string[] { "Circle", "Circle" },
                new RuleData[] { new RuleData { RuleType = RuleType.Inside, TargetIndices = new int[] { 1, 0 } } });

            CreateSpell(folderPath, "7_Barrier", "Barrier", SpellType.Defense, 60f,
                "물리적 타격을 막아내는 단단한 육면체 장벽을 전개합니다.",
                "네모 1개, 원 1개 (네모 안에 원)", "안전한 구역(네모) 안에 생명(원)을 지키는 형상을 그리세요.",
                new string[] { "Square", "Circle" },
                new RuleData[] { new RuleData { RuleType = RuleType.Inside, TargetIndices = new int[] { 1, 0 } } });

            CreateSpell(folderPath, "8_Haste", "Haste", SpellType.Utility, 40f,
                "대상의 시간 흐름을 가속시켜 움직임을 민첩하게 만듭니다.",
                "네모 1개, 세모 1개 (네모 안에 세모)", "안정적인 공간(네모)에 가속의 힘(세모)을 불어넣으세요.",
                new string[] { "Square", "Triangle" },
                new RuleData[] { new RuleData { RuleType = RuleType.Inside, TargetIndices = new int[] { 1, 0 } } });

            CreateSpell(folderPath, "9_Teleport", "Teleport", SpellType.Utility, 70f,
                "공간을 왜곡하여 지정한 위치로 순간이동합니다.",
                "원 1개, 네모 1개 (원 안에 네모)", "공간의 문(원) 안에 도착점(네모)을 지정하세요.",
                new string[] { "Circle", "Square" },
                new RuleData[] { new RuleData { RuleType = RuleType.Inside, TargetIndices = new int[] { 1, 0 } } });

            CreateSpell(folderPath, "10_Quake", "Quake", SpellType.Attack, 60f,
                "강력한 마력 파동을 일으켜 주변 반경의 모든 적들을 넘어뜨리고 타격을 줍니다.",
                "네모 2개 (네모 안에 네모)", "단단한 대지를 상징하는 사각형 안에 파동(사각형)을 중첩하세요.",
                new string[] { "Square", "Square" },
                new RuleData[] { new RuleData { RuleType = RuleType.Inside, TargetIndices = new int[] { 1, 0 } } });

            CreateSpell(folderPath, "11_Nova", "Nova", SpellType.Attack, 30f,
                "응축된 마나를 사방으로 폭발시켜 주변을 휩쓸어버립니다.",
                "원 1개, 세모 3개 (원이 중심, 세모가 밖을 둘러쌈)", "마나 결정의 중심(원) 주변으로 날카로운 파편(세모) 3개를 둘러서 그리세요.",
                new string[] { "Circle", "Triangle", "Triangle", "Triangle" },
                new RuleData[] { new RuleData { RuleType = RuleType.Surround, TargetIndices = new int[] { 0, 1, 2, 3 } } },
                Magic.Combat.StatusEffectType.Burn, 5f);


            // --- [12~16: 기본 마름모 스펠] ---
            CreateSpell(folderPath, "12_MagicSpear", "Magic Spear", SpellType.Attack, 30f,
                "대기 중의 마나를 응축하여 날카로운 창을 쏘아냅니다.",
                "마름모 3개 (일렬 배치)", "날카로운 마력(마름모) 3개를 나란히 엮어 창을 완성하세요.",
                new string[] { "Rhombus", "Rhombus", "Rhombus" },
                new RuleData[] { new RuleData { RuleType = RuleType.Aligned, TargetIndices = new int[] { 0, 1, 2 } } });

            CreateSpell(folderPath, "13_ForceAegis", "Force Aegis", SpellType.Defense, 45f,
                "절대 깨지지 않는 마력 결계를 몸에 두릅니다.",
                "원 1개, 마름모 1개 (원이 마름모를 둘러쌈)", "커다란 마력의 원 안에 단단한 핵(마름모)을 수놓으세요.",
                new string[] { "Circle", "Rhombus" },
                new RuleData[] { new RuleData { RuleType = RuleType.Surround, TargetIndices = new int[] { 0, 1 } } });

            CreateSpell(folderPath, "14_ManaTrap", "Mana Trap", SpellType.Utility, 25f,
                "땅에 보이지 않는 마나 지뢰를 매설합니다.",
                "마름모 2개 (겹치지 않게)", "두 개의 응축된 마력(마름모)을 떨어뜨려 놓으세요.",
                new string[] { "Rhombus", "Rhombus" },
                new RuleData[0]);

            CreateSpell(folderPath, "15_EnergyRay", "Energy Ray", SpellType.Attack, 50f,
                "마력을 모아 강력한 광선을 일직선으로 발사합니다.",
                "세모 1개, 마름모 1개 (세모 안에 마름모)", "세모난 집중진 안에 마력의 핵(마름모)을 가두어 빛을 모으세요.",
                new string[] { "Rhombus", "Triangle" },
                new RuleData[] { new RuleData { RuleType = RuleType.Inside, TargetIndices = new int[] { 0, 1 } } });

            CreateSpell(folderPath, "16_ForceSpike", "Force Spike", SpellType.Attack, 35f,
                "거대한 마력 가시를 바닥에서 솟구치게 합니다.",
                "네모 1개, 마름모 1개 (네모가 마름모를 둘러쌈)", "단단한 기반(네모) 속에 날카로운 가시(마름모)를 품으세요.",
                new string[] { "Square", "Rhombus" },
                new RuleData[] { new RuleData { RuleType = RuleType.Surround, TargetIndices = new int[] { 0, 1 } } });


            // --- [17~25: 새롭게 추가된 마법 9종 (25개 완성)] ---
            CreateSpell(folderPath, "17_ManaBlade", "Mana Blade", SpellType.Attack, 35f,
                "적을 베어내는 거대한 마력의 검기를 생성합니다.",
                "마름모 1개, 세모 1개 (마름모 안에 세모)", "마력의 핵(마름모) 속에 예리한 칼날(세모)을 심으세요.",
                new string[] { "Rhombus", "Triangle" },
                new RuleData[] { new RuleData { RuleType = RuleType.Inside, TargetIndices = new int[] { 1, 0 } } });

            CreateSpell(folderPath, "18_Silence", "Silence", SpellType.Utility, 45f,
                "지정된 영역의 마나 흐름을 차단하여 마법을 봉인합니다.",
                "네모 1개, 세모 1개 (네모가 세모를 둘러쌈)", "봉인의 구역(네모)으로 시전자의 힘(세모)을 감싸세요.",
                new string[] { "Square", "Triangle" },
                new RuleData[] { new RuleData { RuleType = RuleType.Surround, TargetIndices = new int[] { 0, 1 } } });

            CreateSpell(folderPath, "19_Reflect", "Reflect", SpellType.Defense, 60f,
                "자신을 향한 마법 공격을 튕겨내는 거울을 소환합니다.",
                "세모 1개, 원 1개, 세모 1개 (일렬 배치)", "반사경(원) 양 옆에 튕겨낼 방향(세모)을 그리세요.",
                new string[] { "Triangle", "Circle", "Triangle" },
                new RuleData[] { new RuleData { RuleType = RuleType.Aligned, TargetIndices = new int[] { 0, 1, 2 } } });

            CreateSpell(folderPath, "20_ArcaneRain", "Arcane Rain", SpellType.Attack, 80f,
                "수많은 마력의 결정을 비처럼 쏟아내어 넓은 범위를 타격합니다.",
                "마름모 4개 (일렬 배치)", "4개의 마나 결정(마름모)을 일직선으로 정렬해 폭격을 유도하세요.",
                new string[] { "Rhombus", "Rhombus", "Rhombus", "Rhombus" },
                new RuleData[] { new RuleData { RuleType = RuleType.Aligned, TargetIndices = new int[] { 0, 1, 2, 3 } } });

            CreateSpell(folderPath, "21_Ensnare", "Ensnare", SpellType.Utility, 50f,
                "마력의 사슬로 적의 발을 묶어 이동하지 못하게 만듭니다.",
                "네모 3개 (하나의 네모가 나머지 두 개를 둘러쌈)", "큰 감옥(네모) 안에 족쇄(네모 2개)를 채우세요.",
                new string[] { "Square", "Square", "Square" },
                new RuleData[] { new RuleData { RuleType = RuleType.Surround, TargetIndices = new int[] { 0, 1, 2 } } },
                Magic.Combat.StatusEffectType.Stun, 3f);

            CreateSpell(folderPath, "22_Purify", "Purify", SpellType.Utility, 40f,
                "대상에게 걸린 모든 해로운 마법 효과를 정화합니다.",
                "원 1개, 마름모 1개, 원 1개 (일렬 배치)", "정화의 고리(원) 사이에 맑은 마나(마름모)를 두세요.",
                new string[] { "Circle", "Rhombus", "Circle" },
                new RuleData[] { new RuleData { RuleType = RuleType.Aligned, TargetIndices = new int[] { 0, 1, 2 } } });

            CreateSpell(folderPath, "23_ForcePush", "Force Push", SpellType.Attack, 40f,
                "강력한 충격파를 발사하여 적들을 멀리 밀쳐냅니다.",
                "네모 1개, 원 1개, 네모 1개 (일렬 배치)", "장벽(네모) 사이에 충격파(원)를 압축시키세요.",
                new string[] { "Square", "Circle", "Square" },
                new RuleData[] { new RuleData { RuleType = RuleType.Aligned, TargetIndices = new int[] { 0, 1, 2 } } },
                Magic.Combat.StatusEffectType.Stun, 2f);

            CreateSpell(folderPath, "24_MindControl", "Mind Control", SpellType.Utility, 90f,
                "대상의 정신을 붕괴시키고 일시적으로 혼란에 빠뜨립니다.",
                "마름모 1개, 원 1개 (마름모 안에 원)", "대상의 정신(마름모) 속에 환상(원)을 주입하세요.",
                new string[] { "Rhombus", "Circle" },
                new RuleData[] { new RuleData { RuleType = RuleType.Inside, TargetIndices = new int[] { 1, 0 } } },
                Magic.Combat.StatusEffectType.Confusion, 5f);

            CreateSpell(folderPath, "25_Cataclysm", "Cataclysm", SpellType.Attack, 120f,
                "엄청난 마나를 폭주시켜 화면 전체에 궁극의 파괴를 선사합니다.",
                "원 4개 (하나의 원이 세 개의 원을 둘러쌈)", "거대한 세계(원) 안에 파멸의 구체 3개를 가두어 연쇄 폭발을 일으키세요.",
                new string[] { "Circle", "Circle", "Circle", "Circle" },
                new RuleData[] { new RuleData { RuleType = RuleType.Surround, TargetIndices = new int[] { 0, 1, 2, 3 } } },
                Magic.Combat.StatusEffectType.Burn, 10f);


            // Automatically register to Database
            DrawingDatabase db = Object.FindObjectOfType<DrawingDatabase>();
            if (db != null)
            {
                SpellRecipeAsset[] allRecipes = Resources.LoadAll<SpellRecipeAsset>("SpellRecipes");
                db.recipes = allRecipes;
                EditorUtility.SetDirty(db);
                EditorSceneManager.MarkSceneDirty(db.gameObject.scene);
                Debug.Log($"<color=cyan>[자동 등록] DrawingDatabase의 recipes 리스트에 {allRecipes.Length}개의 스펠이 자동으로 부착되었습니다!</color>");
            }

            Debug.Log("<color=lime>🎉 총 25종의 마법 스펠(1~25)이 성공적으로 생성 및 갱신되었습니다!</color>");
        }

        private static void CreateSpell(string folderPath, string fileName, string spellName, SpellType type, float mana, string desc, string condition, string hint, string[] shapes, RuleData[] rules, Magic.Combat.StatusEffectType effectType = Magic.Combat.StatusEffectType.None, float effectDuration = 0f)
        {
            SpellRecipeAsset asset = ScriptableObject.CreateInstance<SpellRecipeAsset>();
            asset.SpellName = spellName;
            asset.Type = type;
            asset.manaCost = mana;
            asset.Description = desc;
            asset.ActivationCondition = condition;
            asset.ActivationHint = hint;
            asset.RequiredShapes = shapes;
            asset.SpatialRules = rules;
            asset.statusEffect = effectType;
            asset.statusEffectDuration = effectDuration;

            string assetPath = $"{folderPath}/{fileName}.asset";
            SpellRecipeAsset existing = AssetDatabase.LoadAssetAtPath<SpellRecipeAsset>(assetPath);
            if (existing != null)
            {
                EditorUtility.CopySerialized(asset, existing);
                EditorUtility.SetDirty(existing);
            }
            else
            {
                AssetDatabase.CreateAsset(asset, assetPath);
            }
            AssetDatabase.SaveAssets();
        }
    }
}
