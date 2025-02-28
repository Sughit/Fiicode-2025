using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomPlanet : MonoBehaviour
{
    [Header("General Settings")]
    [Range(2, 256)]
    public int resolution = 10;
    public bool autoUpdate = true;
    public enum FaceRenderMask { All, Top, Bottom, Left, Right, Front, Back };
    public FaceRenderMask faceRenderMask;

    [Header("Planet Settings")]
    public ShapeSettings shapeSettings;        // The shapeSettings to be generated/modified
    public ColourSettings colourSettings;      // The colourSettings to be generated/modified

    [Header("Randomization Settings")]
    public bool randomizeShape = true;
    public bool randomizeColours = true;

    [Header("Shape Inspiration (Model)")]
    public ShapeSettings shapeInspiration;
    [Range(0f, 1f)]
    public float randomFactor = 0.2f;

    [Header("Noise Layer Count")]
    public int minNoiseLayers = 2;
    public int maxNoiseLayers = 4;

    [Header("Foam Color")]
    public Color userFoamColor = Color.white;

    ShapeGenerator shapeGenerator = new ShapeGenerator();
    ColourGenerator colourGenerator = new ColourGenerator();

    [SerializeField, HideInInspector]
    MeshFilter[] meshFilters;
    TerrainFace[] terrainFaces;

    void Start()
    {
        if (randomizeShape)
        {
            RandomizeShapeSettings();
        }
        if (randomizeColours)
        {
            RandomizeColourSettings();
        }
        GeneratePlanet();
    }

    void RandomizeShapeSettings()
    {
        // 1) Asigurăm un radius peste 5
        shapeSettings.planetRadius = Random.Range(5.1f, 10f);

        // 2) Copiem orientarea din shapeInspiration, dacă există
        if (shapeInspiration != null)
        {
            shapeSettings.orientation = shapeInspiration.orientation;
        }

        // 3) Verificăm shapeInspiration
        if (shapeInspiration == null || shapeInspiration.noiseLayers == null || shapeInspiration.noiseLayers.Length == 0)
        {
            Debug.LogWarning("No shapeInspiration found. Using default shapeSettings (planet might look spiky).");
            return;
        }

        // -- CREĂM LISTE pentru layere Simple și Ridgid din shapeInspiration
        List<ShapeSettings.NoiseLayer> simpleSourceLayers = new List<ShapeSettings.NoiseLayer>();
        List<ShapeSettings.NoiseLayer> ridgidSourceLayers = new List<ShapeSettings.NoiseLayer>();

        // Parcurgem layerele din shapeInspiration și le separăm după tip.
        foreach (var srcLayer in shapeInspiration.noiseLayers)
        {
            if (srcLayer.noiseSettings != null)
            {
                if (srcLayer.noiseSettings.filterType == NoiseSettings.FilterType.Simple &&
                    srcLayer.noiseSettings.simpleNoiseSettings != null)
                {
                    simpleSourceLayers.Add(srcLayer);
                }
                else if (srcLayer.noiseSettings.filterType == NoiseSettings.FilterType.Ridgid &&
                         srcLayer.noiseSettings.ridgidNoiseSettings != null)
                {
                    ridgidSourceLayers.Add(srcLayer);
                }
            }
        }

        // Dacă lipsesc complet layere de tip Simple sau Ridgid, nu putem garanta cerința
        if (simpleSourceLayers.Count == 0 || ridgidSourceLayers.Count == 0)
        {
            Debug.LogWarning("ShapeInspiration must have at least one Simple and one Ridgid layer.");
            return;
        }

        // Aflăm numărul total de layere pe care îl vom genera
        int randomLayerCount = Random.Range(minNoiseLayers, maxNoiseLayers + 1);
        shapeSettings.noiseLayers = new ShapeSettings.NoiseLayer[randomLayerCount];

        // -- PASUL 1: Cream obligatoriu 1 layer de tip Simple
        ShapeSettings.NoiseLayer forcedSimple = CloneAndRandomizeLayer(
            simpleSourceLayers[Random.Range(0, simpleSourceLayers.Count)]
        );
        shapeSettings.noiseLayers[0] = forcedSimple;

        // -- PASUL 2: Cream obligatoriu 1 layer de tip Ridgid (dacă randomLayerCount >= 2)
        if (randomLayerCount >= 2)
        {
            ShapeSettings.NoiseLayer forcedRidgid = CloneAndRandomizeLayer(
                ridgidSourceLayers[Random.Range(0, ridgidSourceLayers.Count)]
            );
            shapeSettings.noiseLayers[1] = forcedRidgid;
        }

        // -- PASUL 3: Restul layere-lor (de la 2 până la randomLayerCount - 1)
        // pot fi alese la întâmplare din TOATE layerele sursă.
        // (fie simpleSourceLayers, fie ridgidSourceLayers, fie *toate* shapeInspiration.noiseLayers)
        // Aici, pentru exemplu, îi lăsăm să fie aleși din TOT shapeInspiration (Simple + Ridgid).
        for (int i = 2; i < randomLayerCount; i++)
        {
            var srcLayer = shapeInspiration.noiseLayers[Random.Range(0, shapeInspiration.noiseLayers.Length)];
            shapeSettings.noiseLayers[i] = CloneAndRandomizeLayer(srcLayer);
        }
    }

    /// <summary>
    /// Clonează un NoiseLayer (Simple sau Ridgid) și randomizează parametrii.
    /// </summary>
    ShapeSettings.NoiseLayer CloneAndRandomizeLayer(ShapeSettings.NoiseLayer srcLayer)
    {
        ShapeSettings.NoiseLayer newLayer = new ShapeSettings.NoiseLayer();
        newLayer.enabled = srcLayer.enabled;
        newLayer.useFirstLayerAsMask = srcLayer.useFirstLayerAsMask;

        newLayer.noiseSettings = new NoiseSettings();
        newLayer.noiseSettings.filterType = srcLayer.noiseSettings.filterType;

        if (newLayer.noiseSettings.filterType == NoiseSettings.FilterType.Simple)
        {
            newLayer.noiseSettings.simpleNoiseSettings = new NoiseSettings.SimpleNoiseSettings();
            var srcSimple = srcLayer.noiseSettings.simpleNoiseSettings;
            var dstSimple = newLayer.noiseSettings.simpleNoiseSettings;

            // Evităm NullRef dacă inspirația lipsește
            if (srcSimple != null)
            {
                dstSimple.strength      = RandomizeAround(srcSimple.strength);
                dstSimple.baseRoughness = RandomizeAround(srcSimple.baseRoughness);
                dstSimple.roughness     = RandomizeAround(srcSimple.roughness);
                dstSimple.persistence   = RandomizeAround(srcSimple.persistence);
                dstSimple.centre        = srcSimple.centre + Random.insideUnitSphere * randomFactor;
                dstSimple.minValue      = RandomizeAround(srcSimple.minValue);
                dstSimple.numLayers     = srcSimple.numLayers;
            }
        }
        else // Ridgid
        {
            newLayer.noiseSettings.ridgidNoiseSettings = new NoiseSettings.RidgidNoiseSettings();
            var srcRidgid = srcLayer.noiseSettings.ridgidNoiseSettings;
            var dstRidgid = newLayer.noiseSettings.ridgidNoiseSettings;

            if (srcRidgid != null)
            {
                dstRidgid.strength         = RandomizeAround(srcRidgid.strength);
                dstRidgid.baseRoughness    = RandomizeAround(srcRidgid.baseRoughness);
                dstRidgid.roughness        = RandomizeAround(srcRidgid.roughness);
                dstRidgid.persistence      = RandomizeAround(srcRidgid.persistence);
                dstRidgid.centre           = srcRidgid.centre + Random.insideUnitSphere * randomFactor;
                dstRidgid.minValue         = RandomizeAround(srcRidgid.minValue);
                dstRidgid.weightMultiplier = RandomizeAround(srcRidgid.weightMultiplier);
                dstRidgid.numLayers        = srcRidgid.numLayers;
            }
        }

        return newLayer;
    }

    float RandomizeAround(float sourceValue)
    {
        float offset = sourceValue * randomFactor;
        float minVal = sourceValue - offset;
        float maxVal = sourceValue + offset;

        if (Mathf.Abs(sourceValue) < 0.0001f)
        {
            minVal = -0.1f * randomFactor;
            maxVal =  0.1f * randomFactor;
        }
        return Random.Range(minVal, maxVal);
    }

    /// <summary>
    /// Returnează cea mai întunecată culoare din gradient, căutând pe câteva eșantioane între 0 și 1.
    /// </summary>
    Color FindDarkestColorInGradient(Gradient gradient)
    {
        int sampleCount = 10;
        Color darkestColor = Color.white;
        float darkestValue = 999f;

        for (int i = 0; i <= sampleCount; i++)
        {
            float t = i / (float)sampleCount;
            Color c = gradient.Evaluate(t);
            float brightness = (c.r + c.g + c.b) / 3f;
            if (brightness < darkestValue)
            {
                darkestValue = brightness;
                darkestColor = c;
            }
        }
        return darkestColor;
    }

    /// <summary>
    /// Forțează `colorA` să fie mai întunecată (sau egal de întunecată) decât `referenceColor`.
    /// </summary>
    Color EnsureDarkerThan(Color colorA, Color referenceColor)
    {
        float brightnessA = (colorA.r + colorA.g + colorA.b) / 3f;
        float brightnessRef = (referenceColor.r + referenceColor.g + referenceColor.b) / 3f;

        if (brightnessA > brightnessRef)
        {
            float factor = brightnessRef / brightnessA;
            return colorA * factor; 
        }
        return colorA;
    }

    void RandomizeColourSettings()
    {
        // Creează un gradient random cu 4 puncte de culoare.
        Gradient randomGradient = new Gradient();

        float time1 = Random.Range(0.1f, 0.4f);
        float time2 = Random.Range(0.6f, 0.9f);

        GradientColorKey[] colorKeys = new GradientColorKey[4];
        colorKeys[0].color = new Color(Random.value, Random.value, Random.value);
        colorKeys[0].time = 0f;
        colorKeys[1].color = new Color(Random.value, Random.value, Random.value);
        colorKeys[1].time = time1;
        colorKeys[2].color = new Color(Random.value, Random.value, Random.value);
        colorKeys[2].time = time2;
        colorKeys[3].color = new Color(Random.value, Random.value, Random.value);
        colorKeys[3].time = 1f;

        GradientAlphaKey[] alphaKeys = new GradientAlphaKey[4];
        for (int i = 0; i < alphaKeys.Length; i++)
        {
            alphaKeys[i].alpha = 1f;
            alphaKeys[i].time = colorKeys[i].time;
        }
        randomGradient.SetKeys(colorKeys, alphaKeys);

        // Îl setăm pe planetă
        colourSettings.gradient = randomGradient;

        // Determinăm cea mai întunecată culoare
        Color darkestGradientColor = FindDarkestColorInGradient(randomGradient);

        // Apoi forțăm userFoamColor să fie (dacă e cazul) mai întunecată
        Color finalFoamColor = EnsureDarkerThan(userFoamColor, darkestGradientColor);

        // Setăm culoarea spumei în material.
        colourSettings.planetMaterial.SetColor("_FoamColor", finalFoamColor);
    }

    void Initialize()
    {
        shapeGenerator.UpdateSettings(shapeSettings);
        colourGenerator.UpdateSettings(colourSettings);

        if (meshFilters == null || meshFilters.Length == 0)
        {
            meshFilters = new MeshFilter[6];
        }
        terrainFaces = new TerrainFace[6];

        Vector3[] directions = 
        { 
            Vector3.up, 
            Vector3.down, 
            Vector3.left, 
            Vector3.right, 
            Vector3.forward, 
            Vector3.back 
        };

        for (int i = 0; i < 6; i++)
        {
            if (meshFilters[i] == null)
            {
                GameObject meshObj = new GameObject("Mesh_" + directions[i]);
                meshObj.transform.SetParent(transform, false);
                meshObj.transform.localPosition = Vector3.zero;

                meshObj.AddComponent<MeshRenderer>();
                meshFilters[i] = meshObj.AddComponent<MeshFilter>();
                meshFilters[i].sharedMesh = new Mesh();
            }
            meshFilters[i].GetComponent<MeshRenderer>().sharedMaterial = colourSettings.planetMaterial;

            terrainFaces[i] = new TerrainFace(shapeGenerator, meshFilters[i].sharedMesh, resolution, directions[i]);
            bool renderFace = faceRenderMask == FaceRenderMask.All || (int)faceRenderMask - 1 == i;
            meshFilters[i].gameObject.SetActive(renderFace);
        }
    }

    public void GeneratePlanet()
    {
        Initialize();
        GenerateMesh();
        GenerateColours();
        colourSettings.planetMaterial.SetFloat("_PlanetRadius", shapeSettings.planetRadius);
    }

    void GenerateMesh()
    {
        for (int i = 0; i < 6; i++)
        {
            if (meshFilters[i].gameObject.activeSelf)
            {
                terrainFaces[i].ConstructMesh();
            }
        }
        colourGenerator.UpdateElevation(shapeGenerator.elevationMinMax);
    }

    public void GenerateColours()
    {
        colourGenerator.UpdateColours();
    }
}
