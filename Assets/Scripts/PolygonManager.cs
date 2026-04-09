using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;  

public class PolygonManager : MonoBehaviour
{
    [Header("Settings")]
    public GameObject pointPrefab; // Küçük küre prefab'ýn
    public float wallHeight = 3.0f;

    [Header("Data")]
    public List<Vector3> pointPositions = new List<Vector3>();
    private List<GameObject> pointObjects = new List<GameObject>();

    void Update()
    {
        // Mouse.current.leftButton.wasPressedThisFrame -> Eski Input.GetMouseButtonDown(0) yerine
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            AddPoint();
        }

        // Keyboard.current.enterKey.wasPressedThisFrame -> Eski Input.GetKeyDown(KeyCode.Return) yerine
        if (Keyboard.current != null && Keyboard.current.enterKey.wasPressedThisFrame && pointPositions.Count >= 3)
        {
            GenerateGallery();
        }
    }

    void AddPoint()
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();
        Ray ray = Camera.main.ScreenPointToRay(mousePos);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            // Noktayý zemin düzleminde (y=0) oluþturuyoruz
            Vector3 newPos = new Vector3(hit.point.x, 0, hit.point.z);

            pointPositions.Add(newPos);
            GameObject newPoint = Instantiate(pointPrefab, newPos, Quaternion.identity);
            pointObjects.Add(newPoint);

            Debug.Log($"Nokta eklendi: {newPos}");
        }
    }

    void GenerateGallery()
    {
        Debug.Log("Galeri oluþturuluyor... (Ear Clipping burada devreye girecek)");
        // Bir sonraki adýmda burayý dolduracaðýz.
        GetComponent<MeshGenerator>().CreateMesh(pointPositions, wallHeight);

    }
}
