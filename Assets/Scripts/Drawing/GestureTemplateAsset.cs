using UnityEngine;
using System.Collections.Generic;

namespace Magic.Drawing
{
    [CreateAssetMenu(fileName = "NewGestureTemplate", menuName = "Magic/Gesture Template")]
    public class GestureTemplateAsset : ScriptableObject
    {
        public string shapeName;
        public Point[] points;
    }
}
