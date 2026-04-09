using System.Collections.Generic;
using UnityEngine;

public static class GeometryUtils
{
    // --- BÖLÜM 1: ÜÇGENLEME (EAR CLIPPING) ---
    public static class EarClipping
    {
        public static int[] Triangulate(List<Vector3> points)
        {
            // Poligon yönü kontrolü ve düzeltme
            if (!IsClockwise(points))
            {
                points.Reverse();
                Debug.Log("Poligon yönü ters (CCW) algýlandý, otomatik olarak CW yapýldý.");
            }

            List<int> indices = new List<int>();
            int n = points.Count;
            if (n < 3) return indices.ToArray();

            List<int> indexList = new List<int>();
            for (int i = 0; i < n; i++) indexList.Add(i);

            int iterations = 0;
            while (indexList.Count > 3 && iterations < 500)
            {
                iterations++;
                bool earFound = false;
                for (int i = 0; i < indexList.Count; i++)
                {
                    int prev = indexList[(i + indexList.Count - 1) % indexList.Count];
                    int curr = indexList[i];
                    int next = indexList[(i + 1) % indexList.Count];

                    if (IsEar(prev, curr, next, points, indexList))
                    {
                        indices.Add(prev);
                        indices.Add(curr);
                        indices.Add(next);
                        indexList.RemoveAt(i);
                        earFound = true;
                        break;
                    }
                }
                if (!earFound) break;
            }

            if (indexList.Count == 3)
            {
                indices.Add(indexList[0]);
                indices.Add(indexList[1]);
                indices.Add(indexList[2]);
            }

            return indices.ToArray();
        }

        private static bool IsEar(int p, int c, int n, List<Vector3> points, List<int> indexList)
        {
            Vector2 a = new Vector2(points[p].x, points[p].z);
            Vector2 b = new Vector2(points[c].x, points[c].z);
            Vector2 g = new Vector2(points[n].x, points[n].z);

            if (CrossProduct(a, b, g) <= 0) return false;

            for (int i = 0; i < indexList.Count; i++)
            {
                int vi = indexList[i];
                if (vi == p || vi == c || vi == n) continue;
                if (IsPointInTriangle(new Vector2(points[vi].x, points[vi].z), a, b, g)) return false;
            }
            return true;
        }

        private static float CrossProduct(Vector2 a, Vector2 b, Vector2 c)
        {
            return (b.x - a.x) * (c.y - a.y) - (b.y - a.y) * (c.x - a.x);
        }

        private static bool IsPointInTriangle(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
        {
            float d1 = CrossProduct(p, a, b);
            float d2 = CrossProduct(p, b, c);
            float d3 = CrossProduct(p, c, a);
            bool hasNeg = (d1 < -0.001f) || (d2 < -0.001f) || (d3 < -0.001f);
            bool hasPos = (d1 > 0.001f) || (d2 > 0.001f) || (d3 > 0.001f);
            return !(hasNeg && hasPos);
        }

        private static bool IsClockwise(List<Vector3> points)
        {
            float area = 0;
            for (int i = 0; i < points.Count; i++)
            {
                Vector3 p1 = points[i];
                Vector3 p2 = points[(i + 1) % points.Count];
                area += (p2.x - p1.x) * (p2.z + p1.z);
            }
            return area < 0;
        }
    }

    // --- BÖLÜM 2: 3-RENKLENDÝRME (3-COLORING) ---
    public static class TriColoring
    {
        public static int[] ColorMesh(Vector3[] vertices, int[] triangles)
        {
            int vertexCount = vertices.Length;
            int[] colors = new int[vertexCount];
            for (int i = 0; i < vertexCount; i++) colors[i] = -1; // -1: Henüz boyanmamýþ

            // Ýlk üçgeni boya
            if (triangles.Length >= 3)
            {
                colors[triangles[0]] = 0;
                colors[triangles[1]] = 1;
                colors[triangles[2]] = 2;
            }

            bool coloredAny;
            int safetyNet = 0;
            do
            {
                coloredAny = false;
                safetyNet++;
                for (int i = 0; i < triangles.Length; i += 3)
                {
                    int[] t = { triangles[i], triangles[i + 1], triangles[i + 2] };

                    for (int j = 0; j < 3; j++)
                    {
                        int current = t[j];
                        int next = t[(j + 1) % 3];
                        int prev = t[(j + 2) % 3];

                        if (colors[current] == -1 && colors[next] != -1 && colors[prev] != -1)
                        {
                            colors[current] = 3 - (colors[next] + colors[prev]);
                            coloredAny = true;
                        }
                    }
                }
            } while (coloredAny && safetyNet < 1000);

            return colors;
        }
    }
}