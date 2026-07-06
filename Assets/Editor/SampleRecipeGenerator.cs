using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using Magic.Drawing;

namespace Magic.Editor
{
    public class SampleRecipeGenerator
    {
        [MenuItem("Magic/Generate 20 Sample Recipes")]
        public static void GenerateRecipes()
        {
            string folderPath = "Assets/Resources/SpellRecipes";
            
            if (!AssetDatabase.IsValidFolder("Assets/Resources"))
                AssetDatabase.CreateFolder("Assets", "Resources");
            if (!AssetDatabase.IsValidFolder(folderPath))
                AssetDatabase.CreateFolder("Assets/Resources", "SpellRecipes");

            int count = 0;
            
            count += CreateRecipe("1_Fireball", 
                "가장 기초적인 화염 마법. 대기의 마나를 응축하여 적에게 날려보냅니다.",
                "원 1개", "둥근 불꽃을 형상화하여 단순한 원을 그리세요.",
                new string[] { "Circle" });
                
            count += CreateRecipe("2_Meteor", 
                "우주의 별을 소환하여 지상으로 추락시킵니다. 대지를 뒤흔드는 파괴력을 자랑합니다.",
                "원 1개 + 별 1개 (원이 별을 감싸야 함)", "별자리 주위로 거대한 구속의 결계(원)를 그리세요.",
                new string[] { "Circle", "Star" }, 
                new RuleData { RuleType = RuleType.Inside, TargetIndices = new int[] { 1, 0 } });
            
            count += CreateRecipe("3_Trinity", 
                "세 가지의 절대적인 원리가 일렬로 정렬할 때 발동되는 심판의 마법입니다.",
                "세모 3개 (일렬 배치)", "세 개의 성스러운 삼각형을 한 줄로 나란히 세우세요.",
                new string[] { "Triangle", "Triangle", "Triangle" },
                new RuleData { RuleType = RuleType.Aligned, TargetIndices = new int[] { 0, 1, 2 } });
            
            count += CreateRecipe("4_Starfall", 
                "하늘을 가로지르는 유성우를 소환합니다. 아름답지만 치명적입니다.",
                "별 3개 (일렬 배치)", "밤하늘을 가르는 세 개의 유성(별)을 일직선으로 그려내세요.",
                new string[] { "Star", "Star", "Star" },
                new RuleData { RuleType = RuleType.Aligned, TargetIndices = new int[] { 0, 1, 2 } });
                
            count += CreateRecipe("5_DivineShield", 
                "신의 가호가 담긴 절대적인 방어막을 전개합니다. 모든 사악한 것을 튕겨냅니다.",
                "세모 1개 + 원 1개 (세모 안에 원)", "견고한 성벽(세모) 중심에 순수한 마나(원)를 보호하듯 그리세요.",
                new string[] { "Triangle", "Circle" },
                new RuleData { RuleType = RuleType.Inside, TargetIndices = new int[] { 1, 0 } });
                
            count += CreateRecipe("6_SummonFamiliar", 
                "차원의 문을 열어 영혼을 공유할 마수를 소환합니다.",
                "네모 1개 + 별 1개 (네모 안에 별)", "차원의 문(네모)을 그리고 그 안에 계약의 증표(별)를 새기세요.",
                new string[] { "Square", "Star" },
                new RuleData { RuleType = RuleType.Inside, TargetIndices = new int[] { 1, 0 } });
                
            count += CreateRecipe("7_DoubleStrike", 
                "시간의 틈새를 베어내는 두 번의 연속된 물리 타격을 가합니다.",
                "세모 2개 (일렬 배치)", "무기의 궤적을 형상화한 두 개의 날(세모)을 일직선상에 그리세요.",
                new string[] { "Triangle", "Triangle" },
                new RuleData { RuleType = RuleType.Aligned, TargetIndices = new int[] { 0, 1 } });
                
            count += CreateRecipe("8_MagicMissile", 
                "마나로 이루어진 유도탄 세 발을 연속으로 발사합니다.",
                "원 3개 (일렬 배치)", "마력이 응집된 세 개의 둥근 구체를 한 줄로 띄우세요.",
                new string[] { "Circle", "Circle", "Circle" },
                new RuleData { RuleType = RuleType.Aligned, TargetIndices = new int[] { 0, 1, 2 } });
                
            count += CreateRecipe("9_Heal", 
                "따스한 생명력으로 대상의 상처를 치유하고 활력을 되찾아줍니다.",
                "원 2개 (원 안에 원)", "작은 생명의 씨앗(원)을 더 큰 보호의 결계(원)로 감싸세요.",
                new string[] { "Circle", "Circle" },
                new RuleData { RuleType = RuleType.Inside, TargetIndices = new int[] { 1, 0 } });
                
            count += CreateRecipe("10_ManaRestore", 
                "심연의 지혜와 연결하여 고갈된 마력을 급속도로 회복합니다.",
                "별 2개 (별 안에 별)", "빛나는 별빛 속에 또 다른 작은 별빛을 중첩시켜 그리세요.",
                new string[] { "Star", "Star" },
                new RuleData { RuleType = RuleType.Inside, TargetIndices = new int[] { 1, 0 } });
                
            count += CreateRecipe("11_Barrier", 
                "전후좌우를 굳건히 틀어막는 물리적인 마력 장벽을 세웁니다.",
                "네모 1개 + 원 1개 (네모 안에 원)", "견고한 큐브(네모) 모양 안에 마나의 핵(원)을 고정시키세요.",
                new string[] { "Square", "Circle" },
                new RuleData { RuleType = RuleType.Inside, TargetIndices = new int[] { 1, 0 } });
                
            count += CreateRecipe("12_Haste", 
                "바람의 정령을 빌려 대상의 움직임을 번개처럼 가속시킵니다.",
                "세모 2개 (세모 안에 세모)", "앞을 향해 뻗어나가는 두 겹의 날렵한 화살촉(세모)을 그리세요.",
                new string[] { "Triangle", "Triangle" },
                new RuleData { RuleType = RuleType.Inside, TargetIndices = new int[] { 1, 0 } });
                
            count += CreateRecipe("13_Teleport", 
                "공간을 비틀어 지정된 다른 차원의 위치로 순식간에 이동합니다.",
                "네모 2개 (일렬 배치)", "공간을 잇는 두 개의 출입구(네모)를 나란히 배치하세요.",
                new string[] { "Square", "Square" },
                new RuleData { RuleType = RuleType.Aligned, TargetIndices = new int[] { 0, 1 } });
                
            count += CreateRecipe("14_BlackHole", 
                "모든 빛과 마력을 집어삼키는 심연의 소용돌이를 생성합니다.",
                "원 1개 + 별 4개 (별들이 원을 둘러쌈)", "중력의 중심(원)을 그리고, 빨려 들어가는 4개의 조각(별)을 주변에 흩뿌리세요.",
                new string[] { "Circle", "Star", "Star", "Star", "Star" },
                new RuleData { RuleType = RuleType.Surround, TargetIndices = new int[] { 0, 1, 2, 3, 4 } });
                
            count += CreateRecipe("15_EarthQuake", 
                "대지의 분노를 일깨워 주변 반경의 모든 적들을 넘어뜨리고 타격을 줍니다.",
                "네모 2개 (네모 안에 네모)", "단단한 대지를 상징하는 사각형 안에 지진의 파동(사각형)을 중첩하세요.",
                new string[] { "Square", "Square" },
                new RuleData { RuleType = RuleType.Inside, TargetIndices = new int[] { 1, 0 } });
                
            count += CreateRecipe("16_IceNova", 
                "응축된 극한의 냉기를 사방으로 폭발시켜 모든 것을 얼어붙게 만듭니다.",
                "원 1개 + 세모 3개 (세모들이 원을 둘러쌈)", "얼음 결정의 중심(원) 주변으로 날카로운 고드름(세모) 3개를 둘러서 그리세요.",
                new string[] { "Circle", "Triangle", "Triangle", "Triangle" },
                new RuleData { RuleType = RuleType.Surround, TargetIndices = new int[] { 0, 1, 2, 3 } });
                
            count += CreateRecipe("17_LightningStorm", 
                "하늘에 번개를 부르는 거대한 뇌운을 형성하여 벼락을 내리칩니다.",
                "별 3개 + 원 1개 (원 1개가 별 1개 안에 있음)", "세 개의 번개(별) 중 하나에 뇌운의 눈(원)을 새겨 넣으세요.",
                new string[] { "Star", "Star", "Star", "Circle" },
                new RuleData { RuleType = RuleType.Inside, TargetIndices = new int[] { 3, 0 } });

            count += CreateRecipe("18_MeteorShower", 
                "셀 수 없이 많은 운석들을 대지에 퍼부어 종말을 고합니다.",
                "별 5개 (일렬 배치)", "하늘을 뒤덮는 5개의 거대한 유성(별)을 일직선으로 그려내세요.",
                new string[] { "Star", "Star", "Star", "Star", "Star" },
                new RuleData { RuleType = RuleType.Aligned, TargetIndices = new int[] { 0, 1, 2, 3, 4 } });
                
            count += CreateRecipe("19_GrandCross", 
                "마력의 십자성좌를 그려 악을 정화하는 거대한 십자가 빛을 내립니다.",
                "별 4개 (무규칙 조합)", "아무 공간에나 성스러운 빛의 파편(별) 4개를 그려넣어 십자성을 완성하세요.",
                new string[] { "Star", "Star", "Star", "Star" });
            
            count += CreateRecipe("20_UltimateMagic", 
                "고대의 네 가지 원소를 모두 융합하여 세계를 창조하거나 파괴하는 궁극기입니다.",
                "원, 세모, 별, 네모 (별이 원 안에 존재)", "모든 원소를 그려내되, 생명(원)의 중심에 우주(별)의 섭리를 가두세요.",
                new string[] { "Circle", "Triangle", "Star", "Square" },
                new RuleData { RuleType = RuleType.Inside, TargetIndices = new int[] { 2, 0 } });

            AssetDatabase.SaveAssets();
            Debug.Log($"<color=lime>🎉 총 {count}개의 샘플 레시피가 성공적으로 생성되었습니다!</color>");

            // 씬에 있는 DrawingManager 찾아서 자동으로 배열에 넣어주기
            DrawingManager manager = Object.FindObjectOfType<DrawingManager>();
            if (manager != null)
            {
                SpellRecipeAsset[] allRecipes = Resources.LoadAll<SpellRecipeAsset>("SpellRecipes");
                manager.recipes = allRecipes;
                EditorUtility.SetDirty(manager);
                Debug.Log($"<color=cyan>✨ DrawingManager의 Recipes 배열에 {allRecipes.Length}개의 마법이 자동 등록되었습니다!</color>");
            }
        }

        private static int CreateRecipe(string spellName, string desc, string cond, string hint, string[] shapes, params RuleData[] rules)
        {
            string path = $"Assets/Resources/SpellRecipes/{spellName}.asset";
            
            SpellRecipeAsset existing = AssetDatabase.LoadAssetAtPath<SpellRecipeAsset>(path);
            if (existing != null)
            {
                // 이미 있으면 내용만 업데이트
                existing.Description = desc;
                existing.ActivationCondition = cond;
                existing.ActivationHint = hint;
                existing.RequiredShapes = shapes;
                existing.SpatialRules = rules;
                EditorUtility.SetDirty(existing);
                return 0;
            }

            SpellRecipeAsset newAsset = ScriptableObject.CreateInstance<SpellRecipeAsset>();
            newAsset.SpellName = spellName.Split('_')[1]; // 번호 제거
            newAsset.Description = desc;
            newAsset.ActivationCondition = cond;
            newAsset.ActivationHint = hint;
            newAsset.RequiredShapes = shapes;
            newAsset.SpatialRules = rules;

            AssetDatabase.CreateAsset(newAsset, path);
            return 1;
        }
    }
}
