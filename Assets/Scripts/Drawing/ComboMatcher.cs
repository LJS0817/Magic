using UnityEngine;
using System.Collections.Generic;

public static class ComboMatcher
{
    public static System.Tuple<string, float> Match(List<DrawnShape> drawnShapes, SpellRecipeAsset[] recipes)
    {
        if (drawnShapes == null || drawnShapes.Count == 0) return new System.Tuple<string, float>("아무것도 그리지 않음", 0f);

        // 평균 정확도(Score) 계산
        float averageScore = 0f;
        foreach (var shape in drawnShapes)
        {
            averageScore += shape.Accuracy;
        }
        averageScore /= drawnShapes.Count;

        // 1. 글로벌 규칙 검사 (상호 배타성 검사: 꼼수 방지)
        if (CheckGlobalOverlap(drawnShapes))
        {
            return new System.Tuple<string, float>("조합 실패 (같은 도형이 너무 겹쳐 있습니다)", averageScore);
        }

        // 2. 레시피 매칭
        foreach (var recipe in recipes)
        {
            if (recipe.RequiredShapes.Length != drawnShapes.Count)
                continue; // 갯수가 다르면 무조건 탈락

            int[] mapping = new int[recipe.RequiredShapes.Length];
            bool[] used = new bool[drawnShapes.Count];

            // 재귀적 순열(Permutation) 탐색을 통해 유효한 짝을 찾고 규칙을 검사함
            if (TryMatchRecursive(0, mapping, used, drawnShapes, recipe))
            {
                return new System.Tuple<string, float>(recipe.SpellName, averageScore);
            }
        }

        return new System.Tuple<string, float>("조합 실패 (일치하는 레시피가 없습니다)", averageScore);
    }

    private static bool CheckGlobalOverlap(List<DrawnShape> shapes)
    {
        for (int i = 0; i < shapes.Count; i++)
        {
            for (int j = i + 1; j < shapes.Count; j++)
            {
                // 종류가 같은 도형끼리만 겹침 검사 (예: 별 위에 별을 여러 개 그리는 꼼수 방지)
                if (shapes[i].Name == shapes[j].Name)
                {
                    if (SpatialAnalyzer.CheckOverlap(shapes[i], shapes[j]))
                        return true; // 너무 겹치면 true 반환 (꼼수 발견)
                }
            }
        }
        return false;
    }

    private static bool TryMatchRecursive(int recipeIdx, int[] mapping, bool[] used, List<DrawnShape> drawnShapes, SpellRecipeAsset recipe)
    {
        if (recipeIdx == recipe.RequiredShapes.Length)
        {
            // 모든 도형 짝짓기 완료. 이제 해당 짝에 대해 공간 규칙(Spatial Rules)을 평가합니다.
            return EvaluateRules(mapping, drawnShapes, recipe.SpatialRules);
        }

        string requiredShape = recipe.RequiredShapes[recipeIdx];
        for (int i = 0; i < drawnShapes.Count; i++)
        {
            if (!used[i] && drawnShapes[i].Name == requiredShape)
            {
                used[i] = true;
                mapping[recipeIdx] = i;
                
                if (TryMatchRecursive(recipeIdx + 1, mapping, used, drawnShapes, recipe))
                    return true; // 하나라도 성공하는 짝짓기가 있으면 통과
                    
                used[i] = false;
            }
        }
        return false;
    }

    private static bool EvaluateRules(int[] mapping, List<DrawnShape> drawnShapes, RuleData[] rules)
    {
        if (rules == null) return true; // 규칙이 없으면 통과

        foreach (var rule in rules)
        {
            if (rule.TargetIndices == null || rule.TargetIndices.Length == 0) continue;

            if (rule.RuleType == RuleType.Inside)
            {
                if (rule.TargetIndices.Length < 2) return false;
                DrawnShape a = drawnShapes[mapping[rule.TargetIndices[0]]]; // 들어갈 놈
                DrawnShape b = drawnShapes[mapping[rule.TargetIndices[1]]]; // 바탕이 될 놈
                if (!SpatialAnalyzer.CheckInside(a, b)) return false;
            }
            else if (rule.RuleType == RuleType.Surround)
            {
                if (rule.TargetIndices.Length < 3) return false; // 적어도 2개가 둘러싸야 함
                DrawnShape center = drawnShapes[mapping[rule.TargetIndices[0]]];
                List<DrawnShape> others = new List<DrawnShape>();
                for (int i = 1; i < rule.TargetIndices.Length; i++)
                {
                    others.Add(drawnShapes[mapping[rule.TargetIndices[i]]]);
                }
                if (!SpatialAnalyzer.CheckSurround(center, others)) return false;
            }
            else if (rule.RuleType == RuleType.Aligned)
            {
                if (rule.TargetIndices.Length < 3) continue; // 2개는 무조건 일직선이므로 규칙 의미가 적음
                List<DrawnShape> shapes = new List<DrawnShape>();
                foreach (var idx in rule.TargetIndices)
                {
                    shapes.Add(drawnShapes[mapping[idx]]);
                }
                if (!SpatialAnalyzer.CheckAligned(shapes)) return false;
            }
        }
        return true;
    }
}

