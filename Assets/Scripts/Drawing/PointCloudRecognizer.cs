using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct Point
{
    public float X, Y;
    public int StrokeID;

    public Point(float x, float y, int strokeId)
    {
        X = x;
        Y = y;
        StrokeID = strokeId;
    }

    public Point(Vector2 pos, int strokeId)
    {
        X = pos.x;
        Y = pos.y;
        StrokeID = strokeId;
    }
}

public class GestureTemplate
{
    public string Name;
    public Point[] Points;

    public GestureTemplate(string name, Point[] points)
    {
        Name = name;
        Points = PointCloudRecognizer.Normalize(points);
    }
}

public class RecognizerResult
{
    public string Name;
    public float Score;
}

public static class PointCloudRecognizer
{
    private const int NumPoints = 64;

    public static RecognizerResult Classify(Point[] points, List<GestureTemplate> templates)
    {
        if (points.Length == 0)
            return new RecognizerResult { Name = "None", Score = 0f };

        Point[] normalizedPoints = Normalize(points);

        float minDistance = float.MaxValue;
        string bestMatch = "Unknown";

        foreach (var template in templates)
        {
            float dist = GreedyCloudMatch(normalizedPoints, template.Points, minDistance);
            if (dist < minDistance)
            {
                minDistance = dist;
                bestMatch = template.Name;
            }
        }

        // 점수 변환 (거리값을 0~1 사이의 확률 점수로 변환, 거리가 0이면 1.0)
        float score = Mathf.Max((2.0f - minDistance) / 2.0f, 0.0f);
        return new RecognizerResult { Name = bestMatch, Score = score };
    }

    public static Point[] Normalize(Point[] points)
    {
        points = Resample(points, NumPoints);
        points = Scale(points);
        points = TranslateTo(points, new Point(0, 0, 0));
        return points;
    }

    private static Point[] Resample(Point[] points, int n)
    {
        float pathLen = PathLength(points);
        if (pathLen <= 0.0001f)
        {
            List<Point> padded = new List<Point>();
            for (int i = 0; i < n; i++) padded.Add(points[0]);
            return padded.ToArray();
        }

        float interval = pathLen / (n - 1);
        float D = 0.0f;
        List<Point> newPoints = new List<Point> { points[0] };

        int idx = 1;
        Point prevPoint = points[0];

        while (idx < points.Length)
        {
            // 무한루프 완벽 방지: 이미 필요한 점 개수를 모두 채웠다면 루프 탈출
            if (newPoints.Count >= n)
                break;

            if (points[idx].StrokeID == prevPoint.StrokeID)
            {
                float d = Distance(prevPoint, points[idx]);
                if ((D + d) >= interval)
                {
                    float qx = prevPoint.X;
                    float qy = prevPoint.Y;
                    if (d > 0.0001f) // 0 나누기 및 부동소수점 오류 방지
                    {
                        qx += ((interval - D) / d) * (points[idx].X - prevPoint.X);
                        qy += ((interval - D) / d) * (points[idx].Y - prevPoint.Y);
                    }
                    
                    Point q = new Point(qx, qy, points[idx].StrokeID);
                    newPoints.Add(q);

                    prevPoint = q;
                    D = 0.0f;
                    // idx는 증가시키지 않고, 새 기준점(q)부터 원래의 points[idx]까지의 거리를 다음 루프에서 마저 계산함.
                }
                else
                {
                    D += d;
                    prevPoint = points[idx];
                    idx++;
                }
            }
            else
            {
                prevPoint = points[idx];
                idx++;
            }
        }

        // 부동소수점 오차로 인해 점 개수가 모자란 경우를 대비해 패딩 처리
        while (newPoints.Count < n)
        {
            newPoints.Add(new Point(points[points.Length - 1].X, points[points.Length - 1].Y, points[points.Length - 1].StrokeID));
        }
        if (newPoints.Count > n)
        {
            newPoints.RemoveRange(n, newPoints.Count - n);
        }

        return newPoints.ToArray();
    }

    private static Point[] Scale(Point[] points)
    {
        float minX = float.MaxValue, maxX = float.MinValue;
        float minY = float.MaxValue, maxY = float.MinValue;
        
        foreach (var p in points)
        {
            minX = Math.Min(minX, p.X);
            maxX = Math.Max(maxX, p.X);
            minY = Math.Min(minY, p.Y);
            maxY = Math.Max(maxY, p.Y);
        }

        float size = Math.Max(maxX - minX, maxY - minY);
        if (size == 0) size = 1.0f; // Prevent Division by Zero (NaN coordinates)

        Point[] newPoints = new Point[points.Length];
        for (int i = 0; i < points.Length; i++)
        {
            newPoints[i] = new Point(
                (points[i].X - minX) / size,
                (points[i].Y - minY) / size,
                points[i].StrokeID
            );
        }
        return newPoints;
    }

    private static Point[] TranslateTo(Point[] points, Point pt)
    {
        Point c = Centroid(points);
        Point[] newPoints = new Point[points.Length];
        for (int i = 0; i < points.Length; i++)
        {
            newPoints[i] = new Point(
                points[i].X + pt.X - c.X,
                points[i].Y + pt.Y - c.Y,
                points[i].StrokeID
            );
        }
        return newPoints;
    }

    private static Point Centroid(Point[] points)
    {
        float x = 0.0f, y = 0.0f;
        foreach (var p in points)
        {
            x += p.X;
            y += p.Y;
        }
        x /= points.Length;
        y /= points.Length;
        return new Point(x, y, 0);
    }

    private static float PathLength(Point[] points)
    {
        float d = 0.0f;
        for (int i = 1; i < points.Length; i++)
        {
            if (points[i].StrokeID == points[i - 1].StrokeID)
                d += Distance(points[i - 1], points[i]);
        }
        return d;
    }

    private static float Distance(Point p1, Point p2)
    {
        float dx = p2.X - p1.X;
        float dy = p2.Y - p1.Y;
        return (float)Math.Sqrt(dx * dx + dy * dy);
    }

    private static float SqDistance(Point p1, Point p2)
    {
        float dx = p2.X - p1.X;
        float dy = p2.Y - p1.Y;
        return dx * dx + dy * dy;
    }

    private static float GreedyCloudMatch(Point[] points1, Point[] points2, float minSoFar)
    {
        int e = points1.Length;
        int step = (int)Math.Floor(Math.Pow(e, 0.5));
        float min = float.MaxValue;
        for (int i = 0; i < e; i += step)
        {
            float currentMinBound = Math.Min(min, minSoFar);
            float d1 = CloudDistance(points1, points2, i, currentMinBound);
            float d2 = CloudDistance(points2, points1, i, Math.Min(currentMinBound, d1));
            min = Math.Min(min, Math.Min(d1, d2));
        }
        return min;
    }

    private static float CloudDistance(Point[] pts1, Point[] pts2, int start, float minSoFar)
    {
        int n = pts1.Length;
        bool[] matched = new bool[n];
        float sum = 0;
        int i = start;
        do
        {
            int index = -1;
            float minSq = float.MaxValue;
            for (int j = 0; j < n; j++)
            {
                if (!matched[j])
                {
                    float dSq = SqDistance(pts1[i], pts2[j]);
                    if (dSq < minSq)
                    {
                        minSq = dSq;
                        index = j;
                    }
                }
            }
            matched[index] = true;
            float weight = 1.0f - ((i - start + n) % n) / (float)n;
            sum += weight * (float)Math.Sqrt(minSq);

            if (sum >= minSoFar)
            {
                return sum; // Early abandoning
            }

            i = (i + 1) % n;
        } while (i != start);
        return sum;
    }
}

