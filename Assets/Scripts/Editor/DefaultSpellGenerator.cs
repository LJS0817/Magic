using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

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
        CreateSpell(folderPath, "1_Orb", "Fireball", SpellType.Attack, SpellElement.Fire, 25f,
            "가장 기초적인 화염 구체. 대기의 마나를 뜨겁게 응축하여 적에게 날려보냅니다.",
            "원 1개", "타오르는 화염 구체를 형상화하여 단순한 원을 그리세요.",
            new string[] { "Circle" },
            new RuleData[0]);

        CreateSpell(folderPath, "2_Trinity", "Ice Lance", SpellType.Attack, SpellElement.Ice, 50f,
            "세 개의 날카로운 얼음 창을 나란히 정렬하여 적을 꿰뚫는 빙결 마법입니다.",
            "세모 3개 (일렬 배치)", "세 개의 날카로운 얼음 조각(삼각형)을 한 줄로 나란히 세우세요.",
            new string[] { "Triangle", "Triangle", "Triangle" },
            new RuleData[] { new RuleData { RuleType = RuleType.Aligned, TargetIndices = new int[] { 0, 1, 2 } } });

        CreateSpell(folderPath, "3_DivineShield", "Earth Aegis", SpellType.Defense, SpellElement.Earth, 20f,
            "대지의 기운으로 전신을 감싸 물리적인 공격을 완벽히 차단하는 암석 방패입니다.",
            "세모 1개, 원 1개 (세모 안에 원)", "견고한 바위산(세모) 중심에 대지의 핵(원)을 보호하듯 그리세요.",
            new string[] { "Triangle", "Circle" },
            new RuleData[] { new RuleData { RuleType = RuleType.Inside, TargetIndices = new int[] { 1, 0 } } });

        CreateSpell(folderPath, "4_DoubleStrike", "Lightning Spark", SpellType.Attack, SpellElement.Lightning, 50f,
            "두 번의 번개가 내리치듯 찰나의 순간에 적을 두 번 가격하는 전격 마법입니다.",
            "세모 2개 (일렬 배치)", "번개가 내리치는 궤적(세모) 두 개를 일직선상에 그리세요.",
            new string[] { "Triangle", "Triangle" },
            new RuleData[] { new RuleData { RuleType = RuleType.Aligned, TargetIndices = new int[] { 0, 1 } } });

        CreateSpell(folderPath, "5_MagicMissile", "Flame Missiles", SpellType.Attack, SpellElement.Fire, 50f,
            "작고 맹렬한 불꽃 유도탄 세 발을 연속으로 발사합니다.",
            "원 3개 (일렬 배치)", "마력이 응집된 세 개의 불꽃(원)을 한 줄로 띄우세요.",
            new string[] { "Circle", "Circle", "Circle" },
            new RuleData[] { new RuleData { RuleType = RuleType.Aligned, TargetIndices = new int[] { 0, 1, 2 } } });

        CreateSpell(folderPath, "6_Heal", "Nature's Touch", SpellType.Utility, SpellElement.Earth, 40f,
            "대지의 따스한 생명력으로 대상의 상처를 치유하고 활력을 되찾아줍니다.",
            "원 2개 (원 안에 원)", "작은 생명의 씨앗(원)을 더 큰 대지의 결계(원)로 감싸세요.",
            new string[] { "Circle", "Circle" },
            new RuleData[] { new RuleData { RuleType = RuleType.Inside, TargetIndices = new int[] { 1, 0 } } });

        CreateSpell(folderPath, "7_Barrier", "Ice Barrier", SpellType.Defense, SpellElement.Ice, 60f,
            "모든 공격을 얼려버리는 단단한 만년빙의 육면체 장벽을 전개합니다.",
            "네모 1개, 원 1개 (네모 안에 원)", "단단한 얼음벽(네모) 안에 생명(원)을 지키는 형상을 그리세요.",
            new string[] { "Square", "Circle" },
            new RuleData[] { new RuleData { RuleType = RuleType.Inside, TargetIndices = new int[] { 1, 0 } } });

        CreateSpell(folderPath, "8_Haste", "Lightning Speed", SpellType.Utility, SpellElement.Lightning, 40f,
            "전류로 대상의 신경계를 가속시켜 움직임을 벼락처럼 민첩하게 만듭니다.",
            "네모 1개, 세모 1개 (네모 안에 세모)", "안정적인 공간(네모)에 벼락의 속도(세모)를 불어넣으세요.",
            new string[] { "Square", "Triangle" },
            new RuleData[] { new RuleData { RuleType = RuleType.Inside, TargetIndices = new int[] { 1, 0 } } });

        CreateSpell(folderPath, "9_Teleport", "Thunder Blink", SpellType.Utility, SpellElement.Lightning, 70f,
            "자신을 전자로 변환해 번개가 치는 속도로 지정한 위치로 순간이동합니다.",
            "원 1개, 네모 1개 (원 안에 네모)", "번개의 문(원) 안에 도착점(네모)을 지정하세요.",
            new string[] { "Circle", "Square" },
            new RuleData[] { new RuleData { RuleType = RuleType.Inside, TargetIndices = new int[] { 1, 0 } } });

        CreateSpell(folderPath, "10_Quake", "Earthquake", SpellType.Attack, SpellElement.Earth, 60f,
            "강력한 지진 파동을 일으켜 반경 내의 모든 적들을 넘어뜨리고 바위로 짓누릅니다.",
            "네모 2개 (네모 안에 네모)", "단단한 대지를 상징하는 사각형 안에 진동(사각형)을 중첩하세요.",
            new string[] { "Square", "Square" },
            new RuleData[] { new RuleData { RuleType = RuleType.Inside, TargetIndices = new int[] { 1, 0 } } });

        CreateSpell(folderPath, "11_Nova", "Fire Nova", SpellType.Attack, SpellElement.Fire, 30f,
            "응축된 거대한 불꽃을 사방으로 폭발시켜 주변을 재로 만들어버립니다.",
            "원 1개, 세모 3개 (원이 중심, 세모가 밖을 둘러쌈)", "화염의 중심(원) 주변으로 타오르는 불길(세모) 3개를 둘러서 그리세요.",
            new string[] { "Circle", "Triangle", "Triangle", "Triangle" },
            new RuleData[] { new RuleData { RuleType = RuleType.Surround, TargetIndices = new int[] { 0, 1, 2, 3 } } },
            5f);


        // --- [12~16: 기본 마름모 스펠] ---
        CreateSpell(folderPath, "12_MagicSpear", "Ice Spear", SpellType.Attack, SpellElement.Ice, 30f,
            "대기 중의 수분을 얼려내어 꿰뚫는 위력을 지닌 얼음 창을 쏘아냅니다.",
            "마름모 3개 (일렬 배치)", "날카로운 얼음 조각(마름모) 3개를 나란히 엮어 창을 완성하세요.",
            new string[] { "Rhombus", "Rhombus", "Rhombus" },
            new RuleData[] { new RuleData { RuleType = RuleType.Aligned, TargetIndices = new int[] { 0, 1, 2 } } });

        CreateSpell(folderPath, "13_ForceAegis", "Earth Bulwark", SpellType.Defense, SpellElement.Earth, 45f,
            "단단한 암석으로 둘러싸인 절대 깨지지 않는 대지의 결계를 몸에 두릅니다.",
            "원 1개, 마름모 1개 (원이 마름모를 둘러쌈)", "커다란 방어선(원) 안에 단단한 바위 핵(마름모)을 수놓으세요.",
            new string[] { "Circle", "Rhombus" },
            new RuleData[] { new RuleData { RuleType = RuleType.Surround, TargetIndices = new int[] { 0, 1 } } });

        CreateSpell(folderPath, "14_ManaTrap", "Lightning Trap", SpellType.Utility, SpellElement.Lightning, 25f,
            "땅에 닿는 순간 감전되는 보이지 않는 벼락 지뢰를 매설합니다.",
            "마름모 2개 (겹치지 않게)", "두 개의 응축된 전격(마름모)을 떨어뜨려 놓으세요.",
            new string[] { "Rhombus", "Rhombus" },
            new RuleData[0]);

        CreateSpell(folderPath, "15_EnergyRay", "Fire Beam", SpellType.Attack, SpellElement.Fire, 50f,
            "불꽃을 극도로 압축시켜 강력한 화염 광선을 일직선으로 뿜어냅니다.",
            "세모 1개, 마름모 1개 (세모 안에 마름모)", "세모난 집중진 안에 화염의 핵(마름모)을 가두어 열을 모으세요.",
            new string[] { "Rhombus", "Triangle" },
            new RuleData[] { new RuleData { RuleType = RuleType.Inside, TargetIndices = new int[] { 0, 1 } } });

        CreateSpell(folderPath, "16_ForceSpike", "Earth Spike", SpellType.Attack, SpellElement.Earth, 35f,
            "대지의 밑바닥에서 거대한 암석 가시를 솟구치게 합니다.",
            "네모 1개, 마름모 1개 (네모가 마름모를 둘러쌈)", "단단한 대지(네모) 속에 날카로운 바위 가시(마름모)를 품으세요.",
            new string[] { "Square", "Rhombus" },
            new RuleData[] { new RuleData { RuleType = RuleType.Surround, TargetIndices = new int[] { 0, 1 } } });


        // --- [17~25: 새롭게 추가된 마법 9종 (25개 완성)] ---
        CreateSpell(folderPath, "17_ManaBlade", "Flame Blade", SpellType.Attack, SpellElement.Fire, 35f,
            "모든 것을 불태우며 베어내는 거대한 화염의 검기를 생성합니다.",
            "마름모 1개, 세모 1개 (마름모 안에 세모)", "불씨(마름모) 속에 예리한 화염 칼날(세모)을 심으세요.",
            new string[] { "Rhombus", "Triangle" },
            new RuleData[] { new RuleData { RuleType = RuleType.Inside, TargetIndices = new int[] { 1, 0 } } });

        CreateSpell(folderPath, "18_Silence", "Frost Silence", SpellType.Utility, SpellElement.Ice, 45f,
            "지정된 영역의 마나 흐름마저 꽁꽁 얼려버려 마법을 봉인합니다.",
            "네모 1개, 세모 1개 (네모가 세모를 둘러쌈)", "얼어붙은 구역(네모)으로 시전자의 힘(세모)을 감싸세요.",
            new string[] { "Square", "Triangle" },
            new RuleData[] { new RuleData { RuleType = RuleType.Surround, TargetIndices = new int[] { 0, 1 } } });

        CreateSpell(folderPath, "19_Reflect", "Ice Mirror", SpellType.Defense, SpellElement.Ice, 60f,
            "자신을 향한 마법 공격을 매끄러운 얼음 거울로 튕겨냅니다.",
            "세모 1개, 원 1개, 세모 1개 (일렬 배치)", "얼음 반사경(원) 양 옆에 튕겨낼 방향(세모)을 그리세요.",
            new string[] { "Triangle", "Circle", "Triangle" },
            new RuleData[] { new RuleData { RuleType = RuleType.Aligned, TargetIndices = new int[] { 0, 1, 2 } } });

        CreateSpell(folderPath, "20_ArcaneRain", "Meteor Shower", SpellType.Attack, SpellElement.Fire, 80f,
            "하늘에서 불타는 운석들을 비처럼 쏟아내어 넓은 범위를 초토화시킵니다.",
            "마름모 4개 (일렬 배치)", "4개의 운석(마름모)을 일직선으로 정렬해 폭격을 유도하세요.",
            new string[] { "Rhombus", "Rhombus", "Rhombus", "Rhombus" },
            new RuleData[] { new RuleData { RuleType = RuleType.Aligned, TargetIndices = new int[] { 0, 1, 2, 3 } } });

        CreateSpell(folderPath, "21_Ensnare", "Earth Bind", SpellType.Utility, SpellElement.Earth, 50f,
            "질긴 덩굴과 흙으로 적의 발을 단단히 묶어 이동하지 못하게 만듭니다.",
            "네모 3개 (하나의 네모가 나머지 두 개를 둘러쌈)", "큰 감옥(네모) 안에 대지의 족쇄(네모 2개)를 채우세요.",
            new string[] { "Square", "Square", "Square" },
            new RuleData[] { new RuleData { RuleType = RuleType.Surround, TargetIndices = new int[] { 0, 1, 2 } } },
            3f);

        CreateSpell(folderPath, "22_Purify", "Water Cleansing", SpellType.Utility, SpellElement.Ice, 40f,
            "깨끗한 물줄기로 대상에게 걸린 모든 해로운 마법 효과를 씻어냅니다.",
            "원 1개, 마름모 1개, 원 1개 (일렬 배치)", "정화의 고리(원) 사이에 맑은 물방울(마름모)을 두세요.",
            new string[] { "Circle", "Rhombus", "Circle" },
            new RuleData[] { new RuleData { RuleType = RuleType.Aligned, TargetIndices = new int[] { 0, 1, 2 } } });

        CreateSpell(folderPath, "23_ForcePush", "Thunder Wave", SpellType.Attack, SpellElement.Lightning, 40f,
            "귀를 찢는 천둥의 파동을 발사하여 적들을 멀리 튕겨냅니다.",
            "네모 1개, 원 1개, 네모 1개 (일렬 배치)", "장벽(네모) 사이에 천둥의 파동(원)을 압축시키세요.",
            new string[] { "Square", "Circle", "Square" },
            new RuleData[] { new RuleData { RuleType = RuleType.Aligned, TargetIndices = new int[] { 0, 1, 2 } } },
            2f);

        CreateSpell(folderPath, "24_MindControl", "Shocking Mind", SpellType.Utility, SpellElement.Lightning, 90f,
            "전두엽에 강한 전기 자극을 주어 일시적으로 정신을 붕괴시키고 혼란에 빠뜨립니다.",
            "마름모 1개, 원 1개 (마름모 안에 원)", "대상의 정신(마름모) 속에 혼란의 벼락(원)을 주입하세요.",
            new string[] { "Rhombus", "Circle" },
            new RuleData[] { new RuleData { RuleType = RuleType.Inside, TargetIndices = new int[] { 1, 0 } } },
            5f);

        CreateSpell(folderPath, "25_Cataclysm", "Volcano", SpellType.Attack, SpellElement.Fire, 120f,
            "대지를 가르고 용암을 분출시켜 화면 전체에 궁극의 파괴를 선사하는 화산 폭발입니다.",
            "원 4개 (하나의 원이 세 개의 원을 둘러쌈)", "거대한 분화구(원) 안에 마그마 구체 3개를 가두어 연쇄 폭발을 일으키세요.",
            new string[] { "Circle", "Circle", "Circle", "Circle" },
            new RuleData[] { new RuleData { RuleType = RuleType.Surround, TargetIndices = new int[] { 0, 1, 2, 3 } } },
            10f);


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

        Debug.Log("<color=lime>🎉 총 25종의 속성 마법 스펠(1~25)이 성공적으로 생성 및 갱신되었습니다!</color>");
    }

    private static void CreateSpell(string folderPath, string fileName, string spellName, SpellType type, SpellElement element, float mana, string desc, string condition, string hint, string[] shapes, RuleData[] rules, float effectDuration = 0f)
    {
        SpellRecipeAsset asset = ScriptableObject.CreateInstance<SpellRecipeAsset>();
        asset.SpellName = spellName;
        asset.Type = type;
        asset.Element = element;
        asset.manaCost = mana;
        asset.Description = desc;
        asset.ActivationCondition = condition;
        asset.ActivationHint = hint;
        asset.RequiredShapes = shapes;
        asset.SpatialRules = rules;

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
