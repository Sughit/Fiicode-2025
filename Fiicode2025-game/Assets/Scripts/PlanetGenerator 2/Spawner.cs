using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SpawnableObject
{
    public GameObject prefab;

    // Variante de prefabs
    public bool usePrefabVariations = false;
    public List<GameObject> prefabVariations;

    // Câte obiecte dorim să spawnăm din acest tip
    public int count;                 

    // *********************
    // 1) Culori
    // *********************
    [Tooltip("O singură culoare, dacă useMultipleColors este false.")]
    public Color spawnColor;

    [Tooltip("Dacă e true, ignoră spawnColor și folosește spawnColors.")]
    public bool useMultipleColors = false;

    [Tooltip("Lista de culori în care vrei să spawnezi acest obiect.")]
    public List<Color> spawnColors = new List<Color>();

    // *********************
    // Grupare
    // *********************
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
        planetRadius    = renderer.sharedMaterial.GetFloat("_PlanetRadius");
        minHeight       = renderer.sharedMaterial.GetFloat("_MinHeight");
        maxHeight       = renderer.sharedMaterial.GetFloat("_MaxHeight");

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
        int maxAttempts  = 1000;
        int attempts     = 0;

        // Stocăm pozițiile globale pentru a nu spawna obiecte foarte apropiate
        List<Vector3> globalSpawnedPositions = new List<Vector3>();

        foreach (var spawnable in spawnableObjects)
        {
            int successfulSpawns = 0;

            // Începem să încercăm să spawnăm "count" obiecte (în grupuri).
            while (successfulSpawns < spawnable.count && attempts < maxAttempts)
            {
                attempts++;
                // Alegem un punct random pe sfera (normalizat)
                Vector3 randomDir = Random.onUnitSphere;
                Vector3 spawnPosition = shapeGenerator.CalculatePointOnPlanet(randomDir);

                // Culoarea la acea poziție
                Color surfaceColor = GetColorAtPosition(spawnPosition);

                // Verificăm dacă se potrivește cu oricare dintre culorile (1 sau multiple)
                bool matchesColor = false;
                if (spawnable.useMultipleColors && spawnable.spawnColors.Count > 0)
                {
                    // Se potrivește dacă oricare din culorile listate e suficient de apropiată
                    foreach (Color c in spawnable.spawnColors)
                    {
                        if (ColorCloseEnough(surfaceColor, c, colorThreshold))
                        {
                            matchesColor = true;
                            break;
                        }
                    }
                }
                else
                {
                    // Folosim culoarea unică (spawnColor)
                    matchesColor = ColorCloseEnough(surfaceColor, spawnable.spawnColor, colorThreshold);
                }

                // Dacă culoarea se potrivește, creăm un grup
                if (matchesColor)
                {
                    int groupCount = Mathf.Min(spawnable.maxGroupSize, spawnable.count - successfulSpawns);
                    List<Vector3> groupPositions = new List<Vector3>();

                    // Calculăm normal, tangent și bitangent pentru a obține random offset 2D pe suprafață
                    Vector3 normal   = spawnPosition.normalized;
                    Vector3 tangent  = Vector3.Cross(normal, Vector3.up);
                    if (tangent == Vector3.zero)
                        tangent = Vector3.Cross(normal, Vector3.right);
                    tangent.Normalize();
                    Vector3 bitangent = Vector3.Cross(normal, tangent).normalized;

                    for (int i = 0; i < groupCount; i++)
                    {
                        for (int inner = 0; inner < 10; inner++)
                        {
                            // Random offset în jurul spawnPosition, pe suprafață
                            Vector2 offset2D = Random.insideUnitCircle * spawnable.groupSpawnRadius;
                            Vector3 offset   = tangent * offset2D.x + bitangent * offset2D.y;
                            Vector3 candidatePos = shapeGenerator.CalculatePointOnPlanet((spawnPosition + offset).normalized);

                            if (IsValidCandidate(candidatePos, globalSpawnedPositions, groupPositions, minDistanceBetweenObjects))
                            {
                                groupPositions.Add(candidatePos);
                                break; // Am găsit o poziție validă pentru acest obiect
                            }
                        }
                    }

                    // Instanțiem obiectele
                    foreach (Vector3 pos in groupPositions)
                    {
                        GameObject prefabToSpawn = spawnable.prefab;

                        // Dacă avem variații de prefab, alegem aleator
                        if (spawnable.usePrefabVariations && 
                            spawnable.prefabVariations != null && spawnable.prefabVariations.Count > 0)
                        {
                            prefabToSpawn = spawnable.prefabVariations[Random.Range(0, spawnable.prefabVariations.Count)];
                        }

                        GameObject spawnedObject = Instantiate(prefabToSpawn, pos, Quaternion.identity);
                        // Așezăm obiectul astfel încât "în sus" să fie normal la suprafață
                        spawnedObject.transform.up = pos.normalized;

                        // Adăugăm această poziție în globalSpawnedPositions
                        globalSpawnedPositions.Add(pos);
                    }

                    // Avansăm cu numărul de spawnuri reușite din acest tip
                    successfulSpawns += groupPositions.Count;
                    totalSpawned     += groupPositions.Count;
                }
            }
        }

        Debug.Log($"Total obiecte spawnate: {totalSpawned}");
    }

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
        // Cautăm un MeshRenderer pe un copil (cum era înainte)
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

        // height map între minHeight și maxHeight
        float t = Mathf.InverseLerp(minHeight, maxHeight, height);
        t = Mathf.Clamp01(t);

        // Luăm culoarea din textură la poziția t
        return gradientTexture.GetPixelBilinear(t, 0.5f);
    }

    bool ColorCloseEnough(Color a, Color b, float threshold)
    {
        return (Mathf.Abs(a.r - b.r) < threshold &&
                Mathf.Abs(a.g - b.g) < threshold &&
                Mathf.Abs(a.b - b.b) < threshold);
    }
}
