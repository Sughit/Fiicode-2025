using UnityEngine;
using System.Collections;

public class ScanningManager : MonoBehaviour
{
    [Header("References")]
    public Material scanningMaterial;       // Material for the scanning triangle.
    public Material scannedObjectMaterial;  // Material for the scanned object band.

    [Header("Scan Settings")]
    public float scanSpeed = 1.0f;            // Oscillation speed.
    public float minBaseWidth = 1.0f;         // Minimum full width of the base.
    public float maxScanDistance = 10.0f;     // Maximum allowed distance between player and object.

    private Mesh scanningMesh;
    private MeshFilter meshFilter;
    private Coroutine scanningCoroutine;

    private Transform player;
    private Transform scannedObject;

    public static ScanningManager instance;

    void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this);
    }

    void Start()
    {
        GameObject scanningPlane = new GameObject("ScanningPlane");
        scanningPlane.transform.parent = transform;

        meshFilter = scanningPlane.AddComponent<MeshFilter>();
        MeshRenderer renderer = scanningPlane.AddComponent<MeshRenderer>();
        renderer.material.SetColor("_BaseColor", scannedObjectMaterial.GetColor("_ScanColor"));

        scanningMesh = new Mesh();
        meshFilter.mesh = scanningMesh;
        
        scanningPlane.SetActive(false);
    }

    public void StartScan(Transform player, Transform scannedObject, float scanDuration = 5f)
    {
        this.player = player;
        this.scannedObject = scannedObject;

        if (!scannedObject.gameObject.activeSelf)
            scannedObject.gameObject.SetActive(true);

        if (meshFilter != null && meshFilter.gameObject != null)
        {
            meshFilter.gameObject.SetActive(true);
            MeshRenderer renderer = meshFilter.gameObject.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                renderer.material.SetColor("_BaseColor", scannedObjectMaterial.GetColor("_ScanColor"));
            }
        }

        if (scanningCoroutine != null)
        {
            StopCoroutine(scanningCoroutine);
        }
        scanningCoroutine = StartCoroutine(ScanCoroutine(scanDuration));
    }

    private IEnumerator ScanCoroutine(float scanDuration)
    {
        float elapsedTime = 0f;
        while (elapsedTime < scanDuration)
        {
            if (Vector3.Distance(player.position, scannedObject.position) > maxScanDistance)
            {
                CancelScan();
                yield break;
            }

            UpdateScanningMesh();

            elapsedTime += Time.deltaTime;
            yield return null;
        }
        FinishScan();
    }

    private void UpdateScanningMesh()
    {
        if (player == null || scannedObject == null)
            return;

        Renderer rend = scannedObject.GetComponent<Renderer>();
        if (rend == null)
            return;

        Bounds bounds = rend.bounds;
        Vector3 center = bounds.center;

        // Compute a dynamic base height using a sine oscillation.
        float baseY = Mathf.Lerp(bounds.min.y, bounds.max.y, Mathf.Abs(Mathf.Sin(Time.time * scanSpeed)));

        // Calculate the center on the XZ plane.
        Vector3 centerXZ = new Vector3(center.x, 0, center.z);
        Vector3 playerXZ = new Vector3(player.position.x, 0, player.position.z);

        // Get the direction from the object to the player.
        Vector3 dir = playerXZ - centerXZ;
        if (dir.sqrMagnitude < 0.0001f)
            dir = Vector3.forward;
        else
            dir.Normalize();

        // Compute a perpendicular direction in the XZ plane.
        Vector3 perp = new Vector3(-dir.z, 0, dir.x);

        // Determine the half-width of the scan based on object bounds (or use a minimum value).
        float computedHalfWidth = Mathf.Abs(bounds.extents.x * perp.x) + Mathf.Abs(bounds.extents.z * perp.z);
        float halfWidth = Mathf.Max(computedHalfWidth, minBaseWidth * 0.5f);

        // Calculate base vertices for the scanning triangle.
        Vector3 baseCenter = new Vector3(center.x, baseY, center.z);
        Vector3 leftBase = baseCenter + perp * halfWidth;
        Vector3 rightBase = baseCenter - perp * halfWidth;
        Vector3 apex = player.position;

        // Build the triangle vertices.
        Vector3[] vertices = new Vector3[] { apex, leftBase, rightBase };

        // Ensure the triangle's normal points upward.
        Vector3 normal = Vector3.Cross(vertices[1] - apex, vertices[2] - apex);
        if (normal.y < 0)
        {
            Vector3 temp = vertices[1];
            vertices[1] = vertices[2];
            vertices[2] = temp;
        }

        int[] triangles = new int[] { 0, 1, 2 };

        // Define UV coordinates.
        Vector2[] uvs = new Vector2[3];
        uvs[0] = new Vector2(0.5f, 0.0f); // Apex.
        uvs[1] = new Vector2(0.0f, 1.0f); // Left base.
        uvs[2] = new Vector2(1.0f, 1.0f); // Right base.

        // Update the mesh.
        scanningMesh.Clear();
        scanningMesh.vertices = vertices;
        scanningMesh.triangles = triangles;
        scanningMesh.uv = uvs;
        scanningMesh.RecalculateNormals();

        // Sync the base height uniform on both materials.
        scanningMaterial.SetFloat("_ScanBaseHeight", baseY);
        if (scannedObjectMaterial != null)
            scannedObjectMaterial.SetFloat("_ScanBaseHeight", baseY);
    }
    public void CancelScan()
    {
        if (scanningCoroutine != null)
        {
            StopCoroutine(scanningCoroutine);
            scanningCoroutine = null;
        }
        Debug.Log("Scan cancelled: Player is too far from the scanned object.");
        scanningMesh.Clear();
        if (meshFilter != null && meshFilter.gameObject != null)
        {
            meshFilter.gameObject.SetActive(false);
            scannedObject.gameObject.SetActive(false);
        }
    }

    private void FinishScan()
    {
        scanningCoroutine = null;
        Debug.Log("Scan finished successfully.");
        scannedObject.parent.gameObject.GetComponent<Interactable>().CompletedScanLogic();
        if (meshFilter != null && meshFilter.gameObject != null)
        {
            meshFilter.gameObject.SetActive(false);
            scannedObject.gameObject.SetActive(false);
        }
        // Insert any additional logic for when a scan completes here.

    }
}
