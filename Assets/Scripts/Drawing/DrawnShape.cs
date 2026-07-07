using UnityEngine;

namespace Magic.Drawing
{
    public struct DrawnShape
    {
        public string Name;
        public Rect Bounds;
        public Vector2 Center;
        public float Accuracy;

        public DrawnShape(string name, Rect bounds, Vector2 center, float accuracy = 0f)
        {
            Name = name;
            Bounds = bounds;
            Center = center;
            Accuracy = accuracy;
        }
    }
}
