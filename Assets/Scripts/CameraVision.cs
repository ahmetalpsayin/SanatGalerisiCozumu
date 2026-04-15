using UnityEngine;

public class CameraVision : MonoBehaviour
{
    [Header("Detection Settings")]
    public float viewDistance = 15f;
    public float viewAngle = 60f;
    public LayerMask targetMask;   // Hýrsýzýn katmaný (Target)
    public LayerMask obstacleMask; // Duvarlarýn katmaný (Obstacle)

    private Light myLight;

    void Start()
    {
        myLight = GetComponentInChildren<Light>();
        // Iþýðýn açýsýný kodla senkronize edelim
        if (myLight != null) myLight.spotAngle = viewAngle;
    }

    void Update()
    {
        CheckForTargets();
    }

    void CheckForTargets()
    {
        // "Player" etiketli objeyi bul
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        Vector3 dirToTarget = (player.transform.position - transform.position).normalized;

        // Görüþ açýsý ve mesafe kontrolü
        if (Vector3.Angle(transform.forward, dirToTarget) < viewAngle / 2)
        {
            float dstToTarget = Vector3.Distance(transform.position, player.transform.position);

            if (dstToTarget <= viewDistance)
            {
                // Raycast ile arada duvar var mý bak
                if (!Physics.Raycast(transform.position, dirToTarget, dstToTarget, obstacleMask))
                {
                    // HIRSIZ GÖRÜLDÜ!
                    if (myLight != null) myLight.color = Color.red;
                    Debug.DrawLine(transform.position, player.transform.position, Color.red);
                }
                else
                {
                    if (myLight != null) myLight.color = Color.green;
                }
            }
            else
            {
                if (myLight != null) myLight.color = Color.green;
            }
        }
        else
        {
            if (myLight != null) myLight.color = Color.green;
        }
    }
}