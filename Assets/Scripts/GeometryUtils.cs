using System.Collections.Generic;
using UnityEngine;

public static class GeometryUtils
{ 

    public static class EarClipping
{
    public static int[] Triangulate(List<Vector3> points)
    {
        List<int> indices = new List<int>();
        int n = points.Count;
        if (n < 3) return indices.ToArray();

        // Köþe indexlerini tutan bir liste (0, 1, 2, ..., n-1)
        List<int> indexList = new List<int>();
        for (int i = 0; i < n; i++) indexList.Add(i);

        while (indexList.Count > 3)
        {
            bool earFound = false;
            for (int i = 0; i < indexList.Count; i++)
            {
                int prev = indexList[(i + indexList.Count - 1) % indexList.Count];
                int curr = indexList[i];
                int next = indexList[(i + 1) % indexList.Count];

                if (IsEar(prev, curr, next, points, indexList))
                {
                    // Kulaðý bulduk! Üçgeni kaydet
                    indices.Add(prev);
                    indices.Add(curr);
                    indices.Add(next);

                    // Kulaðýn orta noktasýný listeden çýkar
                    indexList.RemoveAt(i);
                    earFound = true;
                    break;
                }
            }

            if (!earFound) break; // Hatalý poligon giriþi durumunda sonsuz döngüyü önle
        }

        // Son kalan 3 noktayý ekle
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

        // 1. Üçgenin saat yönü tersine (Counter-Clockwise) olup olmadýðýný kontrol et (Ýçbükeylik testi)
        if (CrossProduct(a, b, g) <= 0) return false;

        // 2. Diðer noktalarýn bu üçgenin içinde olup olmadýðýný kontrol et
        for (int i = 0; i < indexList.Count; i++)
        {
            int vi = indexList[i];
            if (vi == p || vi == c || vi == n) continue;

            Vector2 point = new Vector2(points[vi].x, points[vi].z);
            if (IsPointInTriangle(point, a, b, g)) return false;
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

        bool hasNeg = (d1 < 0) || (d2 < 0) || (d3 < 0);
        bool hasPos = (d1 > 0) || (d2 > 0) || (d3 > 0);

        return !(hasNeg && hasPos);
    }
}
}
