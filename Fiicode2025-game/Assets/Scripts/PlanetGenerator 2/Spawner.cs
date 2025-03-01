using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SpawnableObject
{
    public GameObject prefab;
    public bool usePrefabVariations = false;
    public List<GameObject> prefabVariations;
    public int count;                 
    public Color spawnColor;          
    public int maxGroupSize = 1;      
    public float groupSpawnRadius = 1.0f; 
}

public class Spawner : MonoBehaviour
{
    public List<SpawnableObject> spawnableObjects;
    public float colorThreshold = 0.1f;
    public float minDistanceBetweenObjects = 1.0f;

    private Texture2D gradientTexture;
    private float planetRadius;
    private float minHeight;
    private float maxHeight;

    public void SpawnObjects(ShapeGenerator shapeGenerator)
    {
        MeshRenderer renderer = GetPlanetMeshRenderer();
        if (renderer == null || renderer.sharedMaterial == null)
        {
            Debug.LogError("Nu a fost găsit niciun MeshRenderer pe copiii planetei!");
            return;
        }

        // Obținem proprietățile shader-ului
        gradientTexture = (Texture2D)renderer.sharedMaterial.GetTexture("_GradientTex");
        planetRadius = renderer.sharedMaterial.GetFloat("_PlanetRadius");
        minHeight = renderer.sharedMaterial.GetFloat("_MinHeight");
        maxHeight = renderer.sharedMaterial.GetFloat("_MaxHeight");

        if (gradientTexture == null)
        {
            Debug.LogError("Gradient texture nu a fost găsit în shader!");
            return;
        }
        if (spawnableObjects.Count == 0)
        {
            Debug.LogError("Nu sunt definite obiecte spawnabile!");
            return;
        }

        int totalSpawned = 0;
        int maxAttempts = 1000, attempts = 0;
        List<Vector3> globalSpawnedPositions = new List<Vector3>();

        foreach (var spawnable in spawnableObjects)
        {
            int successfulSpawns = 0;
            while (successfulSpawns < spawnable.count && attempts < maxAttempts)
            {
                attempts++;
                Vector3 randomPoint = Random.onUnitSphere;
                Vector3 spawnPosition = shapeGenerator.CalculatePointOnPlanet(randomPoint);
                Color surfaceColor = GetColorAtPosition(spawnPosition);

                if (ColorCloseEnough(surfaceColor, spawnable.spawnColor, colorThreshold))
                {
                    int groupCount = Mathf.Min(spawnable.maxGroupSize, spawnable.count - successfulSpawns);
                    List<Vector3> groupPositions = new List<Vector3>();

                    // Calculează tangentul și bitangentul o singură dată
                    Vector3 normal = spawnPosition.normalized;
                    Vector3 tangent = Vector3.Cross(normal, Vector3.up);
                    if (tangent == Vector3.zero)
                        tangent = Vector3.Cross(normal, Vector3.right);
                    tangent.Normalize();
                    Vector3 bitangent = Vector3.Cross(normal, tangent).normalized;

                    // Pentru fiecare obiect din grup, încercăm să găsim o poziție validă
                    for (int i = 0; i < groupCount; i++)
                    {
                        bool foundValid = false;
                        for (int inner = 0; inner < 10; inner++)
                        {
                            Vector2 offset2D = Random.insideUnitCircle * spawnable.groupSpawnRadius;
                            Vector3 offset = tangent * offset2D.x + bitangent * offset2D.y;
                            Vector3 candidate = shapeGenerator.CalculatePointOnPlanet((spawnPosition + offset).normalized);
                            if (IsValidCandidate(candidate, globalSpawnedPositions, groupPositions, minDistanceBetweenObjects))
                            {
                                groupPositions.Add(candidate);
                                foundValid = true;
                                break;
                            }
                        }
                    }

                    // Instanțiază obiectele pentru care s-a găsit o poziție validă
                    foreach (Vector3 pos in groupPositions)
                    {
                        GameObject prefabToSpawn = spawnable.prefab;
                        if (spawnable.usePrefabVariations && spawnable.prefabVariations != null && spawnable.prefabVariations.Count > 0)
                        {
                            prefabToSpawn = spawnable.prefabVariations[Random.Range(0, spawnable.prefabVariations.Count)];
                        }
                        GameObject spawnedObject = Instantiate(prefabToSpawn, pos, Quaternion.identity);
                        spawnedObject.transform.up = pos.normalized;
                        globalSpawnedPositions.Add(pos);
                    }

                    successfulSpawns += groupPositions.Count;
                    totalSpawned += groupPositions.Count;
                }
            }
        }
        Debug.Log($"Total obiecte spawnate: {totalSpawned}");
    }

    // Verifică dacă poziția candidat nu este prea aproape de pozițiile deja spawnate
    private bool IsValidCandidate(Vector3 candidate, List<Vector3> globalPositions, List<Vector3> groupPositions, float minDist)
    {
        foreach (Vector3 pos in globalPositions)
            if (Vector3.Distance(candidate, pos) < minDist)
                return false;
        foreach (Vector3 pos in groupPositions)
            if (Vector3.Distance(candidate, pos) < minDist)
                return false;
        return true;
    }

    MeshRenderer GetPlanetMeshRenderer()
    {
        foreach (Transform child in transform)
        {
            MeshRenderer mr = child.GetComponent<MeshRenderer>();
            if (mr != null)
                return mr;
        }
        return null;
    }

    Color GetColorAtPosition(Vector3 worldPosition)
    {
        if (gradientTexture == null) return Color.black;
        float height = worldPosition.magnitude - planetRadius;
        float t = Mathf.InverseLerp(minHeight, maxHeight, height);
        t = Mathf.Clamp01(t);
        return gradientTexture.GetPixelBilinear(t, 0.5f);
    }

    bool ColorCloseEnough(Color a, Color b, float threshold)
    {
        return Mathf.Abs(a.r - b.r) < threshold &&
               Mathf.Abs(a.g - b.g) < threshold &&
               Mathf.Abs(a.b - b.b) < threshold;
    }
}
