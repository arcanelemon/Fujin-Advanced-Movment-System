
using UnityEngine;

namespace Fujin.Utils.Math
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

        public void AddPoint(Vector3 point)
        {
            AddPoint(points.Length, point);
        }

        public void AddPoint(int index, Vector3 point)
        {
            int i = 0;
            Vector3[] temp = new Vector3[points.Length + 1];
            foreach (Vector3 w in temp)
            {
                if (i == index)
                    temp[i] = point;
                i++;
            }

            points = temp;
        }

        public void RemovePoint()
        {
            RemovePoint(points.Length - 1);
        }

        public void RemovePoint(int index)
        {
            int i = 0, j = 0;
            Vector3[] temp = new Vector3[points.Length - 1];
            foreach (Vector3 w in points)
            {
                if (i != index)
                {
                    temp[j] = w;
                    j++;
                }

                i++;
            }

            points = temp;
        }

        public void SetPoint(int index, Vector3 point)
        {
            points[index] = point;
        }
    }
}
