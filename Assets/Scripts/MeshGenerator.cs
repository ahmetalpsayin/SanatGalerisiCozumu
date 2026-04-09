using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class MeshGenerator : MonoBehaviour
{
    public Material galleryMaterial; // Unity'den bir materyal ata

    public void CreateMesh(List<Vector3> points, float wallHeight)
    {
        Mesh mesh = new Mesh();
        mesh.name = "GalleryMesh";

        List<Vector3> allVertices = new List<Vector3>();
        List<int> allTriangles = new List<int>();

        // --- 1. ZEMÝN OLUÞTURMA ---
        int[] floorIndices = GeometryUtils.EarClipping.Triangulate(points);

        // Zemin vertexleri direkt eklenir
        allVertices.AddRange(points);
        allTriangles.AddRange(floorIndices);

        // --- 2. DUVARLARI OLUÞTURMA ---
        int floorVertexCount = points.Count;

        for (int i = 0; i < floorVertexCount; i++)
        {
            Vector3 p1 = points[i];
            Vector3 p2 = points[(i + 1) % floorVertexCount]; // Bir sonraki nokta (son noktada baþa döner)

            // Duvar için 4 yeni vertex (A_alt, A_üst, B_alt, B_üst)
            int vIdx = allVertices.Count;

            allVertices.Add(p1);                        // vIdx
            allVertices.Add(p1 + Vector3.up * wallHeight); // vIdx + 1
            allVertices.Add(p2);                        // vIdx + 2
            allVertices.Add(p2 + Vector3.up * wallHeight); // vIdx + 3

            // Duvar panelini 2 üçgenle oluþtur (Quad)
            // Ýlk üçgen
            allTriangles.Add(vIdx);
            allTriangles.Add(vIdx + 1);
            allTriangles.Add(vIdx + 2);
            // Ýkinci üçgen
            allTriangles.Add(vIdx + 1);
            allTriangles.Add(vIdx + 3);
            allTriangles.Add(vIdx + 2);
        }

        // Mesh verilerini ata
        mesh.vertices = allVertices.ToArray();
        mesh.triangles = allTriangles.ToArray();

        mesh.RecalculateNormals(); // Iþýklandýrma için þart
        mesh.RecalculateBounds();

        GetComponent<MeshFilter>().mesh = mesh;
        GetComponent<MeshRenderer>().material = galleryMaterial;
    }
}