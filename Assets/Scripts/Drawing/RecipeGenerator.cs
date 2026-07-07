using UnityEngine;
using System.Collections.Generic;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Magic.Drawing
{
    public static class RecipeGenerator
    {
        public static void GenerateAndSaveRecipe(List<DrawnShape> shapes, string recipeName)
        {
            if (shapes == null || shapes.Count == 0)
            {
                Debug.LogWarning("[RecipeGenerator] 도형이 없어 레시피를 생성할 수 없습니다.");
                return;
            }

            // 1. 필요한 도형 목록 추출
            string[] requiredShapes = new string[shapes.Count];
            for (int i = 0; i < shapes.Count; i++)
            {
                requiredShapes[i] = shapes[i].Name;
            }

            List<RuleData> generatedRules = new List<RuleData>();

            // 2. 일렬 배치(Aligned) 판별
            // (도형이 3개 이상일 때 전체가 일렬인지 확인)
            if (shapes.Count >= 3)
            {
                if (SpatialAnalyzer.CheckAligned(shapes))
                {
                    int[] allIndices = new int[shapes.Count];
                    for (int i = 0; i < shapes.Count; i++) allIndices[i] = i;

                    generatedRules.Add(new RuleData
                    {
                        RuleType = RuleType.Aligned,
                        TargetIndices = allIndices
                    });
                    Debug.Log("[RecipeGenerator] 'Aligned' (일렬 배치) 규칙이 자동 감지되었습니다!");
                }
            }

            // 3. 포함 관계(Inside) 판별
            for (int i = 0; i < shapes.Count; i++)
            {
                for (int j = 0; j < shapes.Count; j++)
                {
                    if (i == j) continue;

                    // i번 도형이 j번 도형 안에 있는가?
                    if (SpatialAnalyzer.CheckInside(shapes[i], shapes[j]))
                    {
                        generatedRules.Add(new RuleData
                        {
                            RuleType = RuleType.Inside,
                            TargetIndices = new int[] { i, j } // [0] is inside [1]
                        });
                        Debug.Log($"[RecipeGenerator] 'Inside' 규칙 감지: {i}번({shapes[i].Name}) 도형이 {j}번({shapes[j].Name}) 안에 있습니다.");
                    }
                }
            }

            // 4. 둘러싸기(Surround) 판별
            if (shapes.Count >= 3)
            {
                for (int centerIdx = 0; centerIdx < shapes.Count; centerIdx++)
                {
                    List<DrawnShape> others = new List<DrawnShape>();
                    List<int> otherIndices = new List<int>();

                    for (int j = 0; j < shapes.Count; j++)
                    {
                        if (centerIdx == j) continue;
                        others.Add(shapes[j]);
                        otherIndices.Add(j);
                    }

                    if (SpatialAnalyzer.CheckSurround(shapes[centerIdx], others))
                    {
                        List<int> indices = new List<int> { centerIdx };
                        indices.AddRange(otherIndices);

                        generatedRules.Add(new RuleData
                        {
                            RuleType = RuleType.Surround,
                            TargetIndices = indices.ToArray()
                        });
                        Debug.Log($"[RecipeGenerator] 'Surround' 규칙 감지: {centerIdx}번 도형을 나머지 도형들이 둘러싸고 있습니다.");
                    }
                }
            }

            // 5. 에셋 저장 (Editor 전용)
#if UNITY_EDITOR
            string folderPath = "Assets/Resources/SpellRecipes";
            if (!AssetDatabase.IsValidFolder("Assets/Resources"))
                AssetDatabase.CreateFolder("Assets", "Resources");
            if (!AssetDatabase.IsValidFolder(folderPath))
                AssetDatabase.CreateFolder("Assets/Resources", "SpellRecipes");

            SpellRecipeAsset newAsset = ScriptableObject.CreateInstance<SpellRecipeAsset>();
            newAsset.SpellName = recipeName;
            newAsset.RequiredShapes = requiredShapes;
            newAsset.SpatialRules = generatedRules.ToArray();

            string assetPath = $"{folderPath}/{recipeName}.asset";
            // 동일한 이름이 있으면 유니티가 자동으로 덮어쓰거나 번호를 붙여주도록 고유 이름 생성
            assetPath = AssetDatabase.GenerateUniqueAssetPath(assetPath);

            AssetDatabase.CreateAsset(newAsset, assetPath);
            AssetDatabase.SaveAssets();

            // 씬에 있는 DrawingDatabase 찾아서 자동으로 배열에 넣어주기
            DrawingDatabase db = Object.FindObjectOfType<DrawingDatabase>();
            if (db != null)
            {
                SpellRecipeAsset[] allRecipes = Resources.LoadAll<SpellRecipeAsset>("SpellRecipes");
                db.recipes = allRecipes;
                EditorUtility.SetDirty(db);
            }

            Debug.Log($"<color=lime>🎉 [자동 생성 완료] 새로운 마법 레시피 '{recipeName}'이(가) 저장되었습니다!</color>\n경로: {assetPath}");
#else
            Debug.LogWarning("[RecipeGenerator] 레시피 자동 생성 기능은 유니티 에디터에서만 작동합니다.");
#endif
        }
    }
}
