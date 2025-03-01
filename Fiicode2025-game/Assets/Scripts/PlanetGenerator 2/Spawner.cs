using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SpawnableObject
{
    // Dacă se folosește o singură variantă, se va folosi acest prefab
    public GameObject prefab;
    // Flag pentru a decide dacă se folosesc variații de prefab
    public bool usePrefabVariations = false;
    // Lista de variații pentru prefab, folosită dacă usePrefabVariations este true
    public List<GameObject> prefabVariations;

    public int count;                 // Numărul total de spawn-uri pentru acest obiect
    public Color spawnColor;          // Culoarea pe care trebuie să apară obiectul
    public int maxGroupSize = 1;      // Numărul maxim de obiecte ce pot fi spawnate în grup (1 = spawn individual)
    public float groupSpawnRadius = 1.0f; // Raza în jurul poziției de bază unde se distribuie obiectele în grup
}

public class Spawner : MonoBehaviour
{
    public List<SpawnableObject> spawnableObjects; // Lista de obiecte spawnabile
    public float colorThreshold = 0.1f;            // Toleranța pentru potrivirea culorii
    public float minDistanceBetweenObjects = 1.0f; // Distanța minimă permisă între obiecte

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

        // Obține proprietățile shader-ului
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
        int maxAttempts = 1000; // Previne bucle infinite
        int attempts = 0;

        // Lista globală cu pozițiile spawnate, pentru a preveni suprapunerea
        List<Vector3> globalSpawnedPositions = new List<Vector3>();

        // Iterează prin fiecare tip de obiect spawnabil
        foreach (var spawnable in spawnableObjects)
        {
            int successfulSpawns = 0;
            while (successfulSpawns < spawnable.count && attempts < maxAttempts)
            {
                attempts++;

                // Generează un punct aleatoriu pe sfera unitate
                Vector3 randomPointOnUnitSphere = Random.onUnitSphere;
                // Obține poziția reală pe suprafața planetei folosind ShapeGenerator
                Vector3 spawnPosition = shapeGenerator.CalculatePointOnPlanet(randomPointOnUnitSphere);

                // Obține culoarea terenului la poziția respectivă
                Color surfaceColor = GetColorAtPosition(spawnPosition);

                // Verifică dacă culoarea se potrivește
                if (ColorCloseEnough(surfaceColor, spawnable.spawnColor, colorThreshold))
                {
                    // Determină câte obiecte să spawnăm în grup, fără a depăși maxGroupSize sau numărul rămas
                    int groupCount = Mathf.Min(spawnable.maxGroupSize, spawnable.count - successfulSpawns);
                    List<Vector3> groupPositions = new List<Vector3>();

                    // Calculăm baza pentru planul tangent la poziția de spawn
                    Vector3 normal = spawnPosition.normalized;
                    Vector3 tangent = Vector3.Cross(normal, Vector3.up);
                    if (tangent == Vector3.zero)
                        tangent = Vector3.Cross(normal, Vector3.right);
                    tangent.Normalize();
                    Vector3 bitangent = Vector3.Cross(normal, tangent).normalized;

                    // Pentru fiecare obiect din grup, căutăm o poziție valabilă
                    for (int i = 0; i < groupCount; i++)
                    {
                        bool foundValidPosition = false;
                        int innerAttempts = 0;
                        int maxInnerAttempts = 10; // Numărul maxim de încercări pentru poziția unui obiect din grup

                        while (!foundValidPosition && innerAttempts < maxInnerAttempts)
                        {
                            innerAttempts++;
                            // Calculează un offset aleatoriu în planul tangent
                            Vector2 randomOffset = Random.insideUnitCircle * spawnable.groupSpawnRadius;
                            Vector3 offset = tangent * randomOffset.x + bitangent * randomOffset.y;
                            Vector3 candidatePosition = spawnPosition + offset;
                            // Proiectează poziția astfel încât să rămână pe suprafața planetei
                            candidatePosition = candidatePosition.normalized * spawnPosition.magnitude;

                            // Verifică dacă această poziție este suficient de departe de cele spawnate anterior
                            bool collides = false;
                            foreach (Vector3 pos in globalSpawnedPositions)
                            {
                                if (Vector3.Distance(candidatePosition, pos) < minDistanceBetweenObjects)
                                {
                                    collides = true;
                                    break;
                                }
                            }
                            // Verifică și cu obiectele din cadrul aceluiași grup
                            if (!collides)
                            {
                                foreach (Vector3 pos in groupPositions)
                                {
                                    if (Vector3.Distance(candidatePosition, pos) < minDistanceBetweenObjects)
                                    {
                                        collides = true;
                                        break;
                                    }
                                }
                            }

                            if (!collides)
                            {
                                groupPositions.Add(candidatePosition);
                                foundValidPosition = true;
                            }
                        }
                    }

                    // Instanțiază obiectele pentru care s-au găsit poziții valide
                    foreach (Vector3 pos in groupPositions)
                    {
                        // Alege prefab-ul: dacă se folosesc variații, se alege aleator din lista de variații
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

    MeshRenderer GetPlanetMeshRenderer()
    {
        // Caută prin toți copiii GameObject-ului Planet
        foreach (Transform child in transform)
        {
            MeshRenderer meshRenderer = child.GetComponent<MeshRenderer>();
            if (meshRenderer != null)
            {
                return meshRenderer; // Returnează primul MeshRenderer găsit
            }
        }
        return null;
    }

    Color GetColorAtPosition(Vector3 worldPosition)
    {
        if (gradientTexture == null) return Color.black;

        // Calculează înălțimea similar cu shader-ul
        float height = worldPosition.magnitude - planetRadius;
        float t = Mathf.InverseLerp(minHeight, maxHeight, height);
        t = Mathf.Clamp01(t);

        // Preia culoarea din textura gradient
        return gradientTexture.GetPixelBilinear(t, 0.5f);
    }

    bool ColorCloseEnough(Color a, Color b, float threshold)
    {
        return Mathf.Abs(a.r - b.r) < threshold &&
               Mathf.Abs(a.g - b.g) < threshold &&
               Mathf.Abs(a.b - b.b) < threshold;
    }
}
