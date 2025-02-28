using UnityEngine;
using System.Collections;

public class ScanningManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Material scanningMaterial;       // Material for the scanning triangle.
    [SerializeField] private Material scannedObjectMaterial;  // Material for the scanned object band.

    [Header("Scan Settings")]
    [SerializeField] private float scanSpeed = 1.0f;            // Oscillation speed.
    [SerializeField] private float minBaseWidth = 1.0f;         // Minimum full width of the base.
    [SerializeField] private float maxScanDistance = 10.0f;     // Maximum allowed distance between player and object.

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

    // Calculăm valorile de bază din bounds-ul planetei.
    Bounds bounds = rend.bounds;
    Vector3 center = bounds.center;
    float baseY = Mathf.Lerp(bounds.min.y, bounds.max.y, Mathf.Abs(Mathf.Sin(Time.time * scanSpeed)));

    // Calculăm direcția pe planul XZ de la centrul planetei către jucător.
    Vector3 centerXZ = new Vector3(center.x, 0, center.z);
    Vector3 playerXZ = new Vector3(player.position.x, 0, player.position.z);
    Vector3 dir = playerXZ - centerXZ;
    if (dir.sqrMagnitude < 0.0001f)
        dir = Vector3.forward;
    else
        dir.Normalize();

    // Calculăm vectorul perpendicular în planul XZ.
    Vector3 perp = new Vector3(-dir.z, 0, dir.x);

    // Determinăm jumătatea lățimii bazei.
    float computedHalfWidth = Mathf.Abs(bounds.extents.x * perp.x) + Mathf.Abs(bounds.extents.z * perp.z);
    float halfWidth = Mathf.Max(computedHalfWidth, minBaseWidth * 0.5f);

    // Punctele de bază ale triunghiului.
    Vector3 baseCenter = new Vector3(center.x, baseY, center.z);
    Vector3 leftBase = baseCenter + perp * halfWidth;
    Vector3 rightBase = baseCenter - perp * halfWidth;
    Vector3 apex = player.position;

    // Creăm mesh-ul dublu fațat:
    // Prima față: (apex, leftBase, rightBase)
    // A doua față: (apex, rightBase, leftBase)
    Vector3[] vertices = new Vector3[6]
    {
        apex, leftBase, rightBase,    // fața frontală
        apex, rightBase, leftBase       // fața din spate (winding invers)
    };

    int[] triangles = new int[6]
    {
        0, 1, 2,
        3, 4, 5
    };

    // Setăm coordonatele UV (duplicat pentru ambele fețe).
    Vector2[] uvs = new Vector2[6]
    {
        new Vector2(0.5f, 0.0f), new Vector2(0.0f, 1.0f), new Vector2(1.0f, 1.0f),
        new Vector2(0.5f, 0.0f), new Vector2(1.0f, 1.0f), new Vector2(0.0f, 1.0f)
    };

    // Calculăm normală dorită folosind poziția jucătorului, presupunând că acesta este orientat corect pe planetă.
    Vector3 planetCenter = scannedObject.position;
    Vector3 desiredNormal = (player.position - planetCenter).normalized;
    Vector3[] normals = new Vector3[6]
    {
        desiredNormal, desiredNormal, desiredNormal,
        desiredNormal, desiredNormal, desiredNormal
    };

    // Actualizăm mesh-ul.
    scanningMesh.Clear();
    scanningMesh.vertices = vertices;
    scanningMesh.triangles = triangles;
    scanningMesh.uv = uvs;
    scanningMesh.normals = normals;

    // Sincronizare cu shader-ul, dacă este necesar.
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
