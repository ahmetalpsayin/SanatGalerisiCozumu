using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem; // Unity 6 için gerekli yeni giriþ sistemi

public class PolygonManager : MonoBehaviour
{
    [Header("Settings")]
    public GameObject pointPrefab; // Köþeleri temsil eden küre prefab'ý
    public float wallHeight = 3.0f; // Duvar yüksekliði

    public GameObject cameraPrefab; // Köþeleri temsil eden küre prefab'ý

    [Header("Data")]
    public List<Vector3> pointPositions = new List<Vector3>();
    private List<GameObject> pointObjects = new List<GameObject>();
    private bool isGalleryGenerated = false;

    void Update()
    {
        // 1. Sol Týk ile Nokta Ekle (Sadece galeri henüz oluþturulmadýysa)
        if (!isGalleryGenerated && Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            AddPoint();
        }

        // 2. 'Enter' Tuþu ile Galeriyi Ýnþa Et
        if (!isGalleryGenerated && Keyboard.current != null && Keyboard.current.enterKey.wasPressedThisFrame)
        {
            if (pointPositions.Count >= 3)
            {
                GenerateGallery();
            }
            else
            {
                Debug.LogWarning("Galeri oluþturmak için en az 3 nokta gereklidir!");
            }
        }

        // 3. 'R' Tuþu ile Sahneyi Sýfýrla (Yeni çizim için)
        if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
        {
            ResetScene();
        }
    }

    void AddPoint()
    {
        // Mouse pozisyonunu ekrandan dünyaya (world space) çevir
        Vector2 mousePos = Mouse.current.position.ReadValue();
        Ray ray = Camera.main.ScreenPointToRay(mousePos);

        // Zemine (Plane) çarpýp çarpmadýðýný kontrol et
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            // Noktayý tam zemin yüzeyinde (y=0) tutuyoruz
            Vector3 newPos = new Vector3(hit.point.x, 0, hit.point.z);

            pointPositions.Add(newPos);

            // Görselleþtirme için küre oluþtur
            if (pointPrefab != null)
            {
                GameObject newPoint = Instantiate(pointPrefab, newPos, Quaternion.identity);
                pointObjects.Add(newPoint);
            }

            Debug.Log($"Nokta {pointPositions.Count} eklendi: {newPos}");
        }
    }

    void GenerateGallery()
    {
        Debug.Log("Galeri inþa ediliyor...");
        MeshGenerator generator = GetComponent<MeshGenerator>();

        if (generator != null)
        {
            // 1. Mesh'i oluþtur (Zemin ve Duvarlar)
            generator.CreateMesh(pointPositions, wallHeight);

            // 2. Oluþturulan Mesh verilerini al
            Mesh mesh = GetComponent<MeshFilter>().mesh;
            Vector3[] vertices = mesh.vertices;
            int[] triangles = mesh.triangles;

            // 3. 3-Coloring (Üç-Renklendirme) Algoritmasýný çalýþtýr
            // Not: Sadece zemin köþelerini (ilk noktalarý) boyamak yeterlidir
            int[] vertexColors = GeometryUtils.TriColoring.ColorMesh(vertices, triangles);

            // 4. Hangi renkten (0, 1, 2) kaç tane olduðunu say
            int[] colorCounts = new int[3];
            for (int i = 0; i < pointPositions.Count; i++) // Sadece ana köþeleri sayýyoruz
            {
                if (vertexColors[i] != -1)
                    colorCounts[vertexColors[i]]++;
            }

            // 5. En az kullanýlan rengi (ID) tespit et
            int minColorID = 0;
            if (colorCounts[1] < colorCounts[minColorID]) minColorID = 1;
            if (colorCounts[2] < colorCounts[minColorID]) minColorID = 2;

            Debug.Log($"Renk Daðýlýmý -> Kýrmýzý: {colorCounts[0]}, Yeþil: {colorCounts[1]}, Mavi: {colorCounts[2]}");
            Debug.Log($"Seçilen Minimum Renk ID: {minColorID}. Bu noktalara kamera yerleþtiriliyor...");

            // 6. Kameralarý Yerleþtir
            if (cameraPrefab != null)
            {
                for (int i = 0; i < pointPositions.Count; i++)
                {
                    if (vertexColors[i] == minColorID)
                    {
                        // Kamerayý duvarýn üst hizasýna koyuyoruz
                        Vector3 camPos = vertices[i] + Vector3.up * wallHeight;

                        // Kamerayý oluþtur ve PolygonManager'ýn altýna baðla
                        GameObject cam = Instantiate(cameraPrefab, camPos, Quaternion.identity);
                        cam.transform.SetParent(this.transform);

                        // Opsiyonel: Kamerayý poligonun merkezine doðru döndürebilirsin
                        cam.transform.LookAt(GetPolygonCenter() + Vector3.up * (wallHeight / 2));
                    }
                }
            }

            // 7. Giriþ noktalarýný (küreleri) gizle
            foreach (var obj in pointObjects)
            {
                obj.SetActive(false);
            }

            isGalleryGenerated = true;
        }
    }

    // Yardýmcý fonksiyon: Kameralarýn bakacaðý merkezi bulur
    Vector3 GetPolygonCenter()
    {
        Vector3 center = Vector3.zero;
        foreach (var pos in pointPositions) center += pos;
        return center / pointPositions.Count;
    }

    void ResetScene()
    {
        // Her þeyi temizle ve yeniden baþla
        pointPositions.Clear();
        foreach (var obj in pointObjects)
        {
            Destroy(obj);
        }
        pointObjects.Clear();

        // Mesh'i temizle
        MeshFilter mf = GetComponent<MeshFilter>();
        if (mf != null && mf.mesh != null)
        {
            mf.mesh.Clear();
        }

        isGalleryGenerated = false;
        Debug.Log("Sahne sýfýrlandý. Yeni noktalar koyabilirsiniz.");
    }
}