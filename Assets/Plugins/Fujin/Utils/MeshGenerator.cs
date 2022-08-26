using System.Collections.Generic;
using UnityEngine;

namespace Fujin.Utils
{
    public class MeshGenerator : MonoBehaviour
    {
        static Vector3 GetDirectionVector(Vector3 to, Vector3 from)
        {
            return (to - from).normalized;
        }

        public static void GenerateRope(int sides, float thickness, Vector3[] vertices, Mesh mesh)
        {
            GenerateRope(sides, thickness, vertices, mesh, Vector2.one);
        }

        public static void GenerateRope(int sides, float thickness, Vector3[] vertices, Mesh mesh, Vector2 tiling)
        {
            mesh.Clear();
            List<Vector3> newVertices = new List<Vector3>();
            List<int> triangles = new List<int>();
            List<Vector2> uvs = new List<Vector2>();

            int ringSize = sides * 4;
            float angle = 360f / sides;

            for (int i = 0; i < vertices.Length - 1; i++)
            {
                // Grab Vertices
                Vector3 start = vertices[i];
                Vector3 end = vertices[i + 1];

                // Calculate Direction Vectors
                Vector3 direction = GetDirectionVector(end, start);
                Vector3 forward = i > 0 ?
                    GetDirectionVector(start, vertices[i - 1]) + direction // midpoint between current and last point
                    : direction;
                Vector3 endForward = i < vertices.Length - 2 ?
                    direction + GetDirectionVector(vertices[i + 2], end) // midpoint between current and next point
                    : direction;

                // Thickness
                Vector3 cross = Vector3.Cross(Vector3.up, forward).normalized;
                Vector3 outward = thickness * (cross == Vector3.zero ? Vector3.right : cross);

                for (int j = 0; j < sides; j++)
                {
                    // Build Quad
                    Quaternion startRotation = Quaternion.AngleAxis(angle * j, forward);
                    Quaternion endRotation = Quaternion.AngleAxis(angle * j, endForward);
                    Vector3 startOffset = startRotation * outward;
                    Vector3 endOffset = endRotation * outward;
                    Vector3[] points = new Vector3[4];

                    // Top Left, Top Right
                    points[0] = start + startOffset;
                    points[1] = end + endOffset;

                    // Flip to Next Edge
                    startRotation = Quaternion.AngleAxis(angle * (j + 1), forward);
                    endRotation = Quaternion.AngleAxis(angle * (j + 1), endForward);
                    startOffset = startRotation * outward;
                    endOffset = endRotation * outward;

                    // Bottom Left, Bottom Right
                    points[2] = start + startOffset;
                    points[3] = end + endOffset;

                    newVertices.Add(points[0]);
                    newVertices.Add(points[1]);
                    newVertices.Add(points[2]);
                    newVertices.Add(points[3]);

                    // Build Triangles
                    int v = (i * ringSize) + (j * 4);
                    triangles.Add(v);
                    triangles.Add(v + 2);
                    triangles.Add(v + 1);
                    triangles.Add(v + 2);
                    triangles.Add(v + 3);
                    triangles.Add(v + 1);

                    // Set UVs
                    uvs.Add(new Vector2(points[0].x, points[0].y));
                    uvs.Add(new Vector2(points[1].x, points[1].y));
                    uvs.Add(new Vector2(points[2].x, points[2].y));
                    uvs.Add(new Vector2(points[3].x, points[3].y));
                }
            }

            mesh.SetVertices(newVertices);
            mesh.SetTriangles(triangles, 0);
            mesh.SetUVs(0, uvs);
            mesh.RecalculateNormals();
        }
    }
}
