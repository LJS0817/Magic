using UnityEngine;
using System.Collections.Generic;

namespace Magic.Drawing
{
    public static class SpatialAnalyzer
    {
        // A가 B 안에 들어있는지 확인 (너그러운 판정: A의 중심점이 B 안에 있고, A 면적이 B 면적의 80% 이하)
        public static bool CheckInside(DrawnShape a, DrawnShape b)
        {
            if (!b.Bounds.Contains(a.Center)) return false;
            
            // 너그러운 판정: A의 중심점이 B 안에만 있으면 무조건 Inside로 간주! (크기 비교 완화)
            return true;
        }

        // 꼼수 방지: A와 B가 동일한 종류의 도형일 때 너무 심하게 겹쳐 있는지 확인 (80% 이상 겹치면 true)
        public static bool CheckOverlap(DrawnShape a, DrawnShape b)
        {
            if (!a.Bounds.Overlaps(b.Bounds)) return false;

            float minX = Mathf.Max(a.Bounds.xMin, b.Bounds.xMin);
            float maxX = Mathf.Min(a.Bounds.xMax, b.Bounds.xMax);
            float minY = Mathf.Max(a.Bounds.yMin, b.Bounds.yMin);
            float maxY = Mathf.Min(a.Bounds.yMax, b.Bounds.yMax);

            float overlapArea = Mathf.Max(0, maxX - minX) * Mathf.Max(0, maxY - minY);
            float areaA = a.Bounds.width * a.Bounds.height;
            float areaB = b.Bounds.width * b.Bounds.height;

            // 꼼수 방지: 둘 다 크기가 엇비슷하고(같은 자리에 덧그림) 거의 완전히 겹쳤을 때만 방어합니다.
            // 작은 별이 큰 별 안에 들어있는 정상적인 'Inside' 상황을 막지 않으려면 minArea가 아닌 maxArea(또는 두 면적 모두와 비교)를 써야 합니다.
            float maxArea = Mathf.Max(areaA, areaB);
            if (maxArea == 0) return false;

            // 두 도형 중 큰 도형의 면적 대비 겹친 면적이 80% 이상이라면, 두 도형은 크기가 거의 똑같고 같은 자리에 그려진 꼼수입니다.
            return (overlapArea / maxArea) >= 0.8f;
        }

        // Center 주변을 Others가 빙 둘러싸고 있는지 확인 (방사형 분포 검사)
        public static bool CheckSurround(DrawnShape center, List<DrawnShape> others)
        {
            if (others.Count < 2) return false;

            List<float> angles = new List<float>();
            foreach (var shape in others)
            {
                Vector2 dir = shape.Center - center.Center;
                float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                if (angle < 0) angle += 360f;
                angles.Add(angle);
            }

            angles.Sort();

            // 최대 틈(Gap) 각도 찾기
            float maxGap = 0f;
            for (int i = 0; i < angles.Count; i++)
            {
                float gap = (i == angles.Count - 1) 
                    ? (360f - angles[i] + angles[0]) 
                    : (angles[i + 1] - angles[i]);
                
                if (gap > maxGap) maxGap = gap;
            }

            // 둘러쌌다고 판단하려면 한쪽으로 크게 비어있는 공간이 없어야 함.
            // 예를 들어 180도 이상 비어있다면 둘러싼 것이 아니라 한쪽에 몰려있는 것.
            return maxGap <= 180f; 
        }

        // 여러 도형이 하나의 직선 상에 놓여 있는지 확인 (선형 회귀 알고리즘)
        public static bool CheckAligned(List<DrawnShape> shapes)
        {
            if (shapes.Count < 3) return true; // 2개는 무조건 일직선이므로 참

            float sumX = 0, sumY = 0, sumXY = 0, sumX2 = 0, sumY2 = 0;
            int n = shapes.Count;

            foreach (var shape in shapes)
            {
                sumX += shape.Center.x;
                sumY += shape.Center.y;
                sumXY += shape.Center.x * shape.Center.y;
                sumX2 += shape.Center.x * shape.Center.x;
                sumY2 += shape.Center.y * shape.Center.y;
            }

            // 분모가 0인지 체크 (완벽한 수직선)
            float denominator = n * sumX2 - sumX * sumX;
            if (denominator == 0)
            {
                // 수직선이라면, 모든 x좌표가 거의 같아야 함
                float avgX = sumX / n;
                foreach (var shape in shapes)
                {
                    if (Mathf.Abs(shape.Center.x - avgX) > 2.0f) // 오차 허용 범위 완화
                        return false;
                }
                return true;
            }

            // 회귀선 방정식 y = mx + c 의 m과 c 구하기
            float m = (n * sumXY - sumX * sumY) / denominator;
            float c = (sumY - m * sumX) / n;

            // 각 점이 회귀선에서 얼마나 떨어져 있는지(수직 거리) 계산하여 허용 오차 이내인지 확인
            float maxAllowedError = 2.0f; // 오차 허용치 완화
            
            foreach (var shape in shapes)
            {
                float distance = Mathf.Abs(m * shape.Center.x - shape.Center.y + c) / Mathf.Sqrt(m * m + 1);
                if (distance > maxAllowedError)
                    return false;
            }

            return true;
        }
    }
}
