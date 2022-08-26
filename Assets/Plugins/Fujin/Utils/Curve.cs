
using UnityEngine;

namespace Fujin.Utils
{
    [System.Serializable]
    public class Curve
    {
        public Vector3[] points = new Vector3[] {
            new Vector3(0, 0f, 0f),
            new Vector3(0, 0f, 1f),
            new Vector3(0, 0f, 2f)
        };

        public void Straighten()
        {
            points[1] = points[0] + (points[2] - points[0]) / 2;
        }

        public Vector3 GetPoint(float t)
        {
            t = Mathf.Clamp01(t);
            float oneMinusT = 1f - t;
            Vector3 point =
                Mathf.Pow(oneMinusT, 2) * points[0] +
                2f * oneMinusT * t * points[1] +
                Mathf.Pow(t, 2) * points[2];

            return point;
        }
    }
}
