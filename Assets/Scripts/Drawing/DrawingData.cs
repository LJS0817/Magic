using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Stroke
{
    public List<Vector2> points = new List<Vector2>();

    public void AddPoint(Vector2 point)
    {
        points.Add(point);
    }
}

[System.Serializable]
public class DrawingData
{
    public List<Stroke> strokes = new List<Stroke>();

    public void AddStroke(Stroke stroke)
    {
        strokes.Add(stroke);
    }

    public void Clear()
    {
        strokes.Clear();
    }
}

