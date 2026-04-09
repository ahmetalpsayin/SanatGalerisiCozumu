using System.Collections.Generic;
using UnityEngine;

// En dýþtaki sýnýfý static yapýyoruz ki kolay eriþelim
public static class GeometryUtils
{
    public static class EarClipping
    {
        public static int[] Triangulate(List<Vector3> points)
        {
            // ---YENÝ EKLENEN KONTROL: SAAT YÖNÜNÜ DÜZELT-- -
            // Eðer noktalar saat yönünün tersiyse, listeyi tersine çevir
            if (!IsClockwise(points))
            {
                points.Reverse();
                Debug.Log("Poligon yönü ters (CCW) algýlandý, otomatik olarak CW yapýldý.");
            }
            // -------------------------------------------------

            List<int> indices = new List<int>();
            int n = points.Count;
            if (n < 3) return indices.ToArray();

            List<int> indexList = new List<int>();
            for (int i = 0; i < n; i++) indexList.Add(i);

            // Sonsuz döngü korumasý
            int iterations = 0;
            while (indexList.Count > 3 && iterations < 100)
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

            indices.Add(indexList[0]);
            indices.Add(indexList[1]);
            indices.Add(indexList[2]);

            return indices.ToArray();
        }

        private static bool IsEar(int p, int c, int n, List<Vector3> points, List<int> indexList)
        {
            Vector2 a = new Vector2(points[p].x, points[p].z);
            Vector2 b = new Vector2(points[c].x, points[c].z);
            Vector2 g = new Vector2(points[n].x, points[n].z);

            // UNITY SAAT YÖNÜ (Clockwise) KONTROLÜ
            // Eðer zemin oluþmazsa buradaki <= 0 kýsmýný >= 0 yapmayý deneyeceðiz.
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

            // Daha hassas kontrol: Sadece kesinlikle içerideyse true döner
            bool hasNeg = (d1 < -0.001f) || (d2 < -0.001f) || (d3 < -0.001f);
            bool hasPos = (d1 > 0.001f) || (d2 > 0.001f) || (d3 > 0.001f);

            return !(hasNeg && hasPos);
        }

        // Poligonun saat yönünde olup olmadýðýný kontrol eder (Schoenhardt Algoritmasý basiti)
        private static bool IsClockwise(List<Vector3> points)
        {
            float area = 0;
            for (int i = 0; i < points.Count; i++)
            {
                Vector3 p1 = points[i];
                Vector3 p2 = points[(i + 1) % points.Count];
                // 2D düzlemde (XZ) alan hesabý
                area += (p2.x - p1.x) * (p2.z + p1.z);
            }
            // Alan negatifse Saat Yönünün Tersidir (CCW), pozitifse Saat Yönüdür (CW)
            // Unity'nin XZ düzlemindeki koordinat sistemine göre bu kontrolü yapýyoruz.
            return area < 0;
        }
    }
}
