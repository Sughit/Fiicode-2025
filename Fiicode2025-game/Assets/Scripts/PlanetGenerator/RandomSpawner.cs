using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SpawnGradientData
{
    public string reference;

    // Lista de prefabs (variații)
    public List<GameObject> prefabVariations;

    public int count = 10;

    [Header("Range pe gradient (0..1)")]
    [Range(0, 1)]
    public float minGradientPercent = 0f;
    [Range(0, 1)]
    public float maxGradientPercent = 1f;

    [Header("Setări de grup")]
    public int maxGroupSize = 1;
    public float groupSpawnRadius = 1f;

    [Header("Distanțe")]
    public float customMinDistance = 2f;
    public float customMaxDistance = 5f;
}

public class RandomSpawner : MonoBehaviour
{
    [Header("Listă Cold")]
    public List<SpawnGradientData> coldPrefabs;

    [Header("Listă Hot")]
    public List<SpawnGradientData> hotPrefabs;

    [Header("Listă Paradise")]
    public List<SpawnGradientData> paradisePrefabs;

    [Header("Listă Random")]
    public List<SpawnGradientData> randomPrefabs;

    [Header("Alte setări de spawn global")]
    public int maxAttempts = 2000;

    // Referințe
    private Planet planet;
    private float planetRadius;
    private float minHeight;
    private float maxHeight;

    // Pozițiile globale deja plasate
    private List<Vector3> globalSpawnedPositions = new List<Vector3>();

    public void SpawnNow()
    {
        // 1) Găsim Planet
        planet = GetComponentInChildren<Planet>();
        if (planet == null)
        {
            Debug.LogError("Nu s-a găsit niciun Planet în ierarhie!");
            return;
        }

        planetRadius = planet.shapeSettings.planetRadius;

        // 2) Luăm minHeight, maxHeight din material
        MeshRenderer mr = GetPlanetMeshRenderer(planet);
        if (mr && mr.sharedMaterial)
        {
            if (mr.sharedMaterial.HasProperty("_MinHeight"))
                minHeight = mr.sharedMaterial.GetFloat("_MinHeight");
            if (mr.sharedMaterial.HasProperty("_MaxHeight"))
                maxHeight = mr.sharedMaterial.GetFloat("_MaxHeight");
        }
        else
        {
            Debug.LogError("Materialul planetei nu are _MinHeight/_MaxHeight!");
            return;
        }

        // 3) Determinăm tipul planetei
        RandomPlanet randomPlanet = planet.GetComponent<RandomPlanet>();
        if (randomPlanet == null)
        {
            Debug.LogError("Nu s-a găsit RandomPlanet pe obiectul Planet!");
            return;
        }
        RandomPlanet.PlanetType planetType = randomPlanet.planetType;

        // 4) Alegem lista corespunzătoare
        switch (planetType)
        {
            case RandomPlanet.PlanetType.Cold:
                SpawnForList(coldPrefabs);
                break;
            case RandomPlanet.PlanetType.Hot:
                SpawnForList(hotPrefabs);
                break;
            case RandomPlanet.PlanetType.Paradise:
                SpawnForList(paradisePrefabs);
                break;
            case RandomPlanet.PlanetType.Random:
            default:
                if(randomPlanet.aproxType == RandomPlanet.PlanetType.Cold)
                    SpawnForList(coldPrefabs);
                else if(randomPlanet.aproxType == RandomPlanet.PlanetType.Hot)
                    SpawnForList(hotPrefabs);
                else if(randomPlanet.aproxType == RandomPlanet.PlanetType.Paradise)
                    SpawnForList(paradisePrefabs);
                else
                    SpawnForList(randomPrefabs);
                break;
        }
    }

    private void Start()
    {
        // Gol, dacă vrem să fie apelat doar din RandomPlanet
    }

    private void SpawnForList(List<SpawnGradientData> spawnDataList)
    {
        if (spawnDataList == null || spawnDataList.Count == 0)
        {
            Debug.LogWarning("Lista de SpawnGradientData este goală!");
            return;
        }

        int totalSpawned = 0;
        int attempts = 0;

        foreach (var data in spawnDataList)
        {
            // Să avem ceva prefabs
            if (data.prefabVariations == null || data.prefabVariations.Count == 0)
            {
                Debug.LogWarning($"Elementul '{data.reference}' nu are niciun prefab în prefabVariations!");
                continue;
            }

            int successfulSpawns = 0;

            // Încercăm să plasăm data.count obiecte
            while (successfulSpawns < data.count && attempts < maxAttempts)
            {
                attempts++;

                // 1) Alegem o direcție random
                Vector3 randomDir = Random.onUnitSphere.normalized;

                // 2) Plecăm dintr-un punct "sus" (planetRadius + maxHeight + un offset)
                float bigRadius = planetRadius + maxHeight + 100f;
                Vector3 rayStart = planet.transform.position + randomDir * bigRadius;

                // 3) Raycast înapoi spre centrul planetei
                RaycastHit hit;
                if (Physics.Raycast(rayStart, -randomDir, out hit, bigRadius + 200f))
                {
                    // Avem un punct pe mesh
                    Vector3 spawnOrigin = hit.point;

                    // 4) Calculăm altitudinea reală
                    float realAlt = (spawnOrigin - planet.transform.position).magnitude - planetRadius;

                    // 5) Convertim la un T între 0..1
                    // T = (realAlt - minHeight) / (maxHeight - minHeight)
                    float t = Mathf.InverseLerp(minHeight, maxHeight, realAlt);

                    // 6) Verificăm dacă se încadrează în [data.minGradientPercent..data.maxGradientPercent]
                    if (t < data.minGradientPercent || t > data.maxGradientPercent)
                    {
                        // Nu e în interval => skip
                        continue;
                    }

                    // OK, suntem în banda corectă de gradient => formăm grup
                    int groupCount = Mathf.Min(data.maxGroupSize, data.count - successfulSpawns);

                    List<Vector3> groupPositions = new List<Vector3>();

                    // Vectori de offset 2D
                    Vector3 normal = (spawnOrigin - planet.transform.position).normalized;
                    Vector3 tangent = Vector3.Cross(normal, Vector3.up);
                    if (tangent == Vector3.zero)
                        tangent = Vector3.Cross(normal, Vector3.right);
                    tangent.Normalize();
                    Vector3 bitangent = Vector3.Cross(normal, tangent).normalized;

                    float minDist = data.customMinDistance;
                    float maxDist = data.customMaxDistance;

                    for (int i = 0; i < groupCount; i++)
                    {
                        bool foundPosition = false;
                        for (int attemptGroup = 0; attemptGroup < 10; attemptGroup++)
                        {
                            // offset 2D
                            Vector2 offset2D = Random.insideUnitCircle * data.groupSpawnRadius;
                            Vector3 offset = tangent * offset2D.x + bitangent * offset2D.y;

                            // Pornim un ray mic "de sus" de la spawnOrigin + offset + normal * ceva
                            Vector3 candidateStart = spawnOrigin + offset + normal * 50f;
                            Vector3 downDir = -normal;

                            RaycastHit candidateHit;
                            if (Physics.Raycast(candidateStart, downDir, out candidateHit, 200f))
                            {
                                // Pozitie finala
                                Vector3 candidatePos = candidateHit.point;

                                // Verificare distanțe
                                if (IsValidCandidate(candidatePos, groupPositions, minDist, maxDist, spawnOrigin))
                                {
                                    // Verificăm gradient și pentru acest punct, dacă vrei:
                                    float cAlt = (candidatePos - planet.transform.position).magnitude - planetRadius;
                                    float cT = Mathf.InverseLerp(minHeight, maxHeight, cAlt);
                                    if (cT >= data.minGradientPercent && cT <= data.maxGradientPercent)
                                    {
                                        groupPositions.Add(candidatePos);
                                        foundPosition = true;
                                        break;
                                    }
                                }
                            }
                        }
                        if (!foundPosition)
                            break;
                    }

                    // Instanțiem
                    if (groupPositions.Count > 0)
                    {
                        foreach (var pos in groupPositions)
                        {
                            GameObject randomPrefab = data.prefabVariations[Random.Range(0, data.prefabVariations.Count)];
                            GameObject spawned = Instantiate(randomPrefab, pos, Quaternion.identity, transform);
                            spawned.transform.up = (pos - planet.transform.position).normalized;

                            globalSpawnedPositions.Add(pos);
                        }

                        successfulSpawns += groupPositions.Count;
                        totalSpawned += groupPositions.Count;
                    }
                }
            }
        }

        Debug.Log($"S-au spawnat {totalSpawned} obiecte (Gradient + Raycast).");
    }

    /// <summary>
    /// Validarea distanței față de centrul grupului și față de alte obiecte.
    /// </summary>
    private bool IsValidCandidate(Vector3 candidate, List<Vector3> groupPositions, float minDist, float maxDist, Vector3 origin)
    {
        if (Vector3.Distance(candidate, origin) > maxDist)
            return false;

        foreach (var globalPos in globalSpawnedPositions)
            if (Vector3.Distance(candidate, globalPos) < minDist)
                return false;

        foreach (var grpPos in groupPositions)
            if (Vector3.Distance(candidate, grpPos) < minDist)
                return false;

        return true;
    }

    private MeshRenderer GetPlanetMeshRenderer(Planet planet)
    {
        foreach (Transform child in planet.transform)
        {
            MeshRenderer mr = child.GetComponent<MeshRenderer>();
            if (mr != null) return mr;
        }
        return null;
    }
}
