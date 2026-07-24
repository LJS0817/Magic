using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewGestureTemplate", menuName = "Magic/Gesture Template")]
public class GestureTemplateAsset : ScriptableObject
{
    public string shapeName;
    public Point[] points;
}

