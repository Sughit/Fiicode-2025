using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class SpawnableObject
{
    [Tooltip("Referință pentru identificarea obiectului în Inspector (ex: Clădire Mică, Copac, etc.).")]
    public string reference;

    public GameObject prefab;

    // Variante de prefabs
    public bool usePrefabVariations = false;
    public List<GameObject> prefabVariations;

    // Câte obiecte dorim să spawnăm din acest tip
    public int count;

    // *********************
    // Culori
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

    // *********************
    // Distanțe personalizate
    // *********************
    [Tooltip("Distanța minimă personalizată între obiectele spawnate pentru acest tip.")]
    public float customMinDistance = 1.0f;

    [Tooltip("Distanța maximă personalizată față de poziția centrală de spawn pentru acest tip.")]
    public float customMaxDistance = 1.0f;
}

public class Spawner : MonoBehaviour
{
    public List<SpawnableObject> spawnableObjects;
    public float colorThreshold = 0.1f;
    // Nu mai există valoare default pentru distanță; se folosesc exclusiv cele din fiecare obiect.

    private Texture2D gradientTexture;
    private float planetRadius;
    private float minHeight;
    private float maxHeight;

    public void SpawnObjects(ShapeGenerator shapeGenerator)
    {
        // Dacă planeta folosește seed, reinitializează starea random cu seed-ul actual
        RandomPlanet rp = GetComponent<RandomPlanet>();
        if (rp != null && rp.useSeed)
        {
            Random.InitState(rp.currentSeed);
        }

        MeshRenderer renderer = GetPlanetMeshRenderer();
        if (renderer == null || renderer.sharedMaterial == null)
        {
            Debug.LogError("Nu a fost găsit niciun MeshRenderer pe copiii planetei!");
            return;
        }

        // Obținem proprietățile din materialul planetei
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

        // Stocăm pozițiile globale pentru a evita spawnarea prea apropiată a obiectelor
        List<Vector3> globalSpawnedPositions = new List<Vector3>();

        foreach (var spawnable in spawnableObjects)
        {
            int successfulSpawns = 0;

            // Încercăm să spawnăm "count" obiecte pentru tipul curent
            while (successfulSpawns < spawnable.count && attempts < maxAttempts)
            {
                attempts++;
                // Alegem un punct aleator pe sfera planetei (normalizat)
                Vector3 randomDir = Random.onUnitSphere;
                Vector3 spawnPosition = shapeGenerator.CalculatePointOnPlanet(randomDir);

                // Obținem culoarea de la poziția respectivă
                Color surfaceColor = GetColorAtPosition(spawnPosition);

                // Verificăm dacă culoarea se potrivește (folosind fie spawnColor, fie spawnColors)
                bool matchesColor = false;
                if (spawnable.useMultipleColors && spawnable.spawnColors.Count > 0)
                {
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
                    matchesColor = ColorCloseEnough(surfaceColor, spawnable.spawnColor, colorThreshold);
                }

                if (matchesColor)
                {
                    int groupCount = Mathf.Min(spawnable.maxGroupSize, spawnable.count - successfulSpawns);
                    List<Vector3> groupPositions = new List<Vector3>();

                    // Calculăm vectorii pentru offset 2D pe suprafața planetei
                    Vector3 normal   = spawnPosition.normalized;
                    Vector3 tangent  = Vector3.Cross(normal, Vector3.up);
                    if (tangent == Vector3.zero)
                        tangent = Vector3.Cross(normal, Vector3.right);
                    tangent.Normalize();
                    Vector3 bitangent = Vector3.Cross(normal, tangent).normalized;

                    // Folosim valorile din fiecare obiect
                    float minDist = spawnable.customMinDistance;
                    float maxDist = spawnable.customMaxDistance;

                    for (int i = 0; i < groupCount; i++)
                    {
                        for (int inner = 0; inner < 10; inner++)
                        {
                            Vector2 offset2D = Random.insideUnitCircle * spawnable.groupSpawnRadius;
                            Vector3 offset   = tangent * offset2D.x + bitangent * offset2D.y;
                            Vector3 candidatePos = shapeGenerator.CalculatePointOnPlanet((spawnPosition + offset).normalized);

                            // Validăm candidatul: trebuie să fie în interiorul distanței maxime față de spawnPosition
                            // și să respecte distanța minimă față de cele deja spawnate
                            if (IsValidCandidate(candidatePos, globalSpawnedPositions, groupPositions, spawnPosition, maxDist, minDist))
                            {
                                groupPositions.Add(candidatePos);
                                break;
                            }
                        }
                    }

                    // Instanțiem obiectele la pozițiile validate
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
                    totalSpawned     += groupPositions.Count;
                }
            }
        }

        Debug.Log($"Total obiecte spawnate: {totalSpawned}");
    }

    // Metoda de validare a pozițiilor candidate verifică și distanța maximă față de poziția de spawn (origin)
    private bool IsValidCandidate(Vector3 candidate, List<Vector3> globalPositions, List<Vector3> groupPositions, Vector3 origin, float maxDist, float minDist)
    {
        if (Vector3.Distance(candidate, origin) > maxDist)
            return false;

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
        return (Mathf.Abs(a.r - b.r) < threshold &&
                Mathf.Abs(a.g - b.g) < threshold &&
                Mathf.Abs(a.b - b.b) < threshold);
    }
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(SpawnableObject))]
public class SpawnableObjectDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(property, label, true);
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        SerializedProperty referenceProp = property.FindPropertyRelative("reference");
        if (referenceProp != null && !string.IsNullOrEmpty(referenceProp.stringValue))
        {
            label.text = referenceProp.stringValue;
        }
        EditorGUI.PropertyField(position, property, label, true);
    }
}
#endif
