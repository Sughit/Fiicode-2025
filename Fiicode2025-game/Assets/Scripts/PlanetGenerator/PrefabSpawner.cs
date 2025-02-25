using System.Collections;
using UnityEngine;

public class PrefabSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject[] prefabs;
    public int spawnCount = 100;
    public float minScale = 0.5f;
    public float maxScale = 2.0f;
    public float surfaceOffset = 0.1f;

    [Header("Planet Reference")]
    public GroundGenerator planet;  // Reference to the planet

    void Start()
    {
        StartCoroutine(WaitForPlanetAndSpawn());
    }

    IEnumerator WaitForPlanetAndSpawn()
    {
        // Wait until the planet generates a valid mesh
        Mesh mesh = null;
        while (mesh == null || mesh.vertices.Length == 0)
        {
            if (planet != null)
            {
                mesh = planet.GetComponent<MeshFilter>().mesh;
            }
            yield return null; // Wait for the next frame
        }

        SpawnPrefabs();
    }

    void SpawnPrefabs()
    {
        Mesh mesh = planet.GetComponent<MeshFilter>().mesh;
        if (mesh == null || mesh.vertices.Length == 0)
        {
            Debug.LogError("PrefabSpawner: Planet mesh is still invalid after waiting.");
            return;
        }

        Vector3[] vertices = mesh.vertices;
        Vector3 planetCenter = planet.transform.position;

        for (int i = 0; i < spawnCount; i++)
        {
            int randomIndex = Random.Range(0, vertices.Length);
            Vector3 spawnPosition = planet.transform.TransformPoint(vertices[randomIndex]);

            Vector3 normal = (spawnPosition - planetCenter).normalized;
            spawnPosition += normal * surfaceOffset;

            if (prefabs.Length == 0)
            {
                Debug.LogError("PrefabSpawner: Prefab array is empty!");
                return;
            }

            GameObject prefab = prefabs[Random.Range(0, prefabs.Length)];
            if (prefab == null)
            {
                Debug.LogError($"PrefabSpawner: Null prefab at index {randomIndex}.");
                continue;
            }

            GameObject spawnedObject = Instantiate(prefab, spawnPosition, Quaternion.identity);
            spawnedObject.transform.up = normal;
            spawnedObject.transform.localScale = Vector3.one * Random.Range(minScale, maxScale);
            spawnedObject.transform.parent = planet.transform;
        }
    }
}
