using UnityEngine;

public class ScanningManager : MonoBehaviour
{
    [Header("References")]
    public Transform player;             // Apex of the triangle.
    public Transform scannedObject;      // Used to compute bounds.
    public Material scanningMaterial;    // Material for the scanning triangle.
    public Material scannedObjectMaterial; // Material for the scanned object band.

    [Header("Scan Settings")]
    public float scanSpeed = 1.0f;         // Oscillation speed.
    public float minBaseWidth = 1.0f;      // Minimum full width of the base.

    private Mesh scanningMesh;
    private MeshFilter meshFilter;

    void Start()
    {
        // Create the scanning triangle.
        GameObject scanningPlane = new GameObject("ScanningPlane");
        scanningPlane.transform.parent = transform;

        meshFilter = scanningPlane.AddComponent<MeshFilter>();
        MeshRenderer renderer = scanningPlane.AddComponent<MeshRenderer>();
        renderer.material.SetColor("_BaseColor", scannedObjectMaterial.GetColor("_ScanColor"));

        scanningMesh = new Mesh();
        meshFilter.mesh = scanningMesh;
    }

    void Update()
    {
        if (player == null || scannedObject == null)
            return;

        Renderer rend = scannedObject.GetComponent<Renderer>();
        if (rend == null)
            return;
        Bounds bounds = rend.bounds;
        Vector3 center = bounds.center;

        // Compute dynamic base height (Y) between object bottom and top.
        float baseY = Mathf.Lerp(bounds.min.y, bounds.max.y, Mathf.Abs(Mathf.Sin(Time.time * scanSpeed)));

        // Compute the object's XZ center and the player's XZ.
        Vector3 centerXZ = new Vector3(center.x, 0, center.z);
        Vector3 playerXZ = new Vector3(player.position.x, 0, player.position.z);

        // Direction from object center toward player.
        Vector3 dir = playerXZ - centerXZ;
        if (dir.sqrMagnitude < 0.0001f)
            dir = Vector3.forward;
        else
            dir.Normalize();

        // Perpendicular direction in the XZ plane.
        Vector3 perp = new Vector3(-dir.z, 0, dir.x);

        // Determine half width using object bounds (or a minimum value).
        float computedHalfWidth = Mathf.Abs(bounds.extents.x * perp.x) + Mathf.Abs(bounds.extents.z * perp.z);
        float halfWidth = Mathf.Max(computedHalfWidth, minBaseWidth * 0.5f);

        // Compute base vertices at Y = baseY.
        Vector3 baseCenter = new Vector3(center.x, baseY, center.z);
        Vector3 leftBase = baseCenter + perp * halfWidth;
        Vector3 rightBase = baseCenter - perp * halfWidth;
        Vector3 apex = player.position;

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

        // Set up UV coordinates for the triangle.
        Vector2[] uvs = new Vector2[3];
        uvs[0] = new Vector2(0.5f, 0.0f); // Apex.
        uvs[1] = new Vector2(0.0f, 1.0f); // Left base.
        uvs[2] = new Vector2(1.0f, 1.0f); // Right base.

        scanningMesh.Clear();
        scanningMesh.vertices = vertices;
        scanningMesh.triangles = triangles;
        scanningMesh.uv = uvs;
        scanningMesh.RecalculateNormals();

        //Update the _ScanBaseHeight uniform on both materials so they are in sync.
        scanningMaterial.SetFloat("_ScanBaseHeight", baseY);
        if (scannedObjectMaterial != null)
            scannedObjectMaterial.SetFloat("_ScanBaseHeight", baseY);
    }
}
