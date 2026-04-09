using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem; // Unity 6 için gerekli yeni giriþ sistemi

public class PolygonManager : MonoBehaviour
{
    [Header("Settings")]
    public GameObject pointPrefab; // Köþeleri temsil eden küre prefab'ý
    public float wallHeight = 3.0f; // Duvar yüksekliði

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
        Debug.Log("Galeri inþa ediliyor... Nokta sayýsý: " + pointPositions.Count);

        // MeshGenerator bileþenini al
        MeshGenerator generator = GetComponent<MeshGenerator>();

        if (generator != null)
        {
            // Matematiksel ve görsel inþa iþlemini baþlat
            generator.CreateMesh(pointPositions, wallHeight);

            // Giriþ noktalarýný (küreleri) gizle
            foreach (var obj in pointObjects)
            {
                obj.SetActive(false);
            }

            isGalleryGenerated = true;
            Debug.Log("Galeri baþarýyla oluþturuldu.");
        }
        else
        {
            Debug.LogError("HATA: PolygonManager objesi üzerinde 'MeshGenerator' script'i bulunamadý!");
        }
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