using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlanetType { Random, Perfect, Cold, Hot }
public enum PlanetSize { Random, Small, Medium, Large }
public enum PlanetDataIndex { Index0, Index1, Index2, Index3, Index4 }

public class RandomPlanet : MonoBehaviour
{
    [Header("Planet Type")]
    public PlanetType planetType = PlanetType.Random;

    [Header("Planet Size")]
    public PlanetSize planetSize = PlanetSize.Medium;

    [Header("Main Menu Settings")]
    public bool useMainMenuPlanetSizes = false;

    [Header("General Settings")]
    [Range(2, 256)] public int resolution = 10;
    public bool autoUpdate = true;
    public enum FaceRenderMask { All, Top, Bottom, Left, Right, Front, Back }
    public FaceRenderMask faceRenderMask;

    [Header("Planet Settings")]
    public ShapeSettings shapeSettings;   // Shape settings
    public ColourSettings colourSettings; // Colour settings

    [Header("Randomization Settings")]
    public bool randomizeShape = true;
    public bool randomizeColours = true;

    [Header("Shape Inspiration (Model)")]
    public ShapeSettings shapeInspiration;
    [Range(0f, 1f)] public float randomFactor = 0.2f;

    [Header("Noise Layer Count")]
    public int minNoiseLayers = 2;
    public int maxNoiseLayers = 4;

    [Header("Foam Color")]
    public Color userFoamColor = Color.white;

    [Header("Mountain Settings")]
    public bool addMountainsAndSnowPeaks = true;
    public float mountainStrengthMultiplier = 1.5f;

    [Header("Saved Data Index (0-4)")]
    public PlanetDataIndex planetDataIndex;

    ShapeGenerator shapeGenerator = new ShapeGenerator();
    ColourGenerator colourGenerator = new ColourGenerator();

    [SerializeField, HideInInspector]
    MeshFilter[] meshFilters;
    TerrainFace[] terrainFaces;

    void Start()
    {
        // Poți apela GeneratePlanet() sau poți lăsa PlanetDataManager să facă totul
        // GeneratePlanet();
    }

    public void GeneratePlanet()
    {
        // DACĂ randomizeShape e false, nu mai apelăm RandomizeShapeSettings()
        // DACĂ randomizeColours e false, nu mai apelăm RandomizeColourSettings()
        if (randomizeShape)
        {
            RandomizeShapeSettings();
        }
        if (randomizeColours)
        {
            RandomizeColourSettings();
        }

        // Construim planeta (fie cu date random, fie cu date deja existente)
        Initialize();
        GenerateMesh();
        GenerateColours();

        colourSettings.planetMaterial.SetFloat("_PlanetRadius", shapeSettings.planetRadius);

        // Setăm radius la collider dacă nu e meniu principal
        if (!useMainMenuPlanetSizes)
        {
            SphereCollider sphereCollider = GetComponent<SphereCollider>();
            if (sphereCollider == null)
            {
                sphereCollider = gameObject.AddComponent<SphereCollider>();
            }
            sphereCollider.radius = shapeSettings.planetRadius;
        }
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

        Vector3[] directions = {
            Vector3.up, Vector3.down, 
            Vector3.left, Vector3.right, 
            Vector3.forward, Vector3.back
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

    // ----------------------
    // Metode de randomizare
    // ----------------------
    void RandomizeShapeSettings()
    {
        float minRadius, maxRadius;
        if (useMainMenuPlanetSizes)
        {
            switch (planetSize)
            {
                case PlanetSize.Small:
                    minRadius = 4f;  maxRadius = 5.5f;  break;
                case PlanetSize.Medium:
                    minRadius = 5.5f;  maxRadius = 7f;  break;
                case PlanetSize.Large:
                    minRadius = 7f;  maxRadius = 9f;  break;
                case PlanetSize.Random:
                    minRadius = 4f;  maxRadius = 9f;   break;
                default:
                    minRadius = 5.5f; maxRadius = 7f;  break;
            }
        }
        else
        {
            switch (planetSize)
            {
                case PlanetSize.Small:
                    minRadius = 15f; maxRadius = 20f; break;
                case PlanetSize.Medium:
                    minRadius = 20f; maxRadius = 25f; break;
                case PlanetSize.Large:
                    minRadius = 25f; maxRadius = 30f; break;
                case PlanetSize.Random:
                    minRadius = 15f; maxRadius = 30f; break;
                default:
                    minRadius = 20f; maxRadius = 25f; break;
            }
        }
        shapeSettings.planetRadius = Random.Range(minRadius, maxRadius);

        if (shapeInspiration != null)
        {
            shapeSettings.orientation = shapeInspiration.orientation;
        }

        if (shapeInspiration == null || shapeInspiration.noiseLayers == null || shapeInspiration.noiseLayers.Length == 0)
        {
            Debug.LogWarning("No shape inspiration found. Using default settings (planet might look spiky).");
            return;
        }

        List<ShapeSettings.NoiseLayer> simpleSourceLayers = new List<ShapeSettings.NoiseLayer>();
        List<ShapeSettings.NoiseLayer> ridgidSourceLayers = new List<ShapeSettings.NoiseLayer>();

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

        if (simpleSourceLayers.Count == 0 || ridgidSourceLayers.Count == 0)
        {
            Debug.LogWarning("ShapeInspiration must contain at least one Simple and one Ridgid layer.");
            return;
        }

        int randomLayerCount = Random.Range(minNoiseLayers, maxNoiseLayers + 1);
        shapeSettings.noiseLayers = new ShapeSettings.NoiseLayer[randomLayerCount];

        // Primul strat: Simple
        ShapeSettings.NoiseLayer forcedSimple = CloneAndRandomizeLayer(
            simpleSourceLayers[Random.Range(0, simpleSourceLayers.Count)]
        );
        shapeSettings.noiseLayers[0] = forcedSimple;

        // Al doilea strat: Ridgid (dacă există loc)
        if (randomLayerCount >= 2)
        {
            ShapeSettings.NoiseLayer forcedRidgid = CloneAndRandomizeLayer(
                ridgidSourceLayers[Random.Range(0, ridgidSourceLayers.Count)]
            );
            shapeSettings.noiseLayers[1] = forcedRidgid;
        }

        // Restul straturilor
        for (int i = 2; i < randomLayerCount; i++)
        {
            var srcLayer = shapeInspiration.noiseLayers[Random.Range(0, shapeInspiration.noiseLayers.Length)];
            shapeSettings.noiseLayers[i] = CloneAndRandomizeLayer(srcLayer);
        }

        // Dacă avem munți, creștem strength-ul straturilor Ridgid
        if (addMountainsAndSnowPeaks)
        {
            foreach (var layer in shapeSettings.noiseLayers)
            {
                if (layer.noiseSettings.filterType == NoiseSettings.FilterType.Ridgid)
                {
                    layer.noiseSettings.ridgidNoiseSettings.strength *= mountainStrengthMultiplier;
                }
            }
        }
    }

    void RandomizeColourSettings()
    {
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

        // Assign gradient
        colourSettings.gradient = randomGradient;

        // Determinăm cea mai întunecată culoare din gradient
        Color darkestGradientColor = FindDarkestColorInGradient(randomGradient);
        // Și forțăm userFoamColor să fie mai întunecată, dacă e cazul
        Color finalFoamColor = EnsureDarkerThan(userFoamColor, darkestGradientColor);
        colourSettings.planetMaterial.SetColor("_FoamColor", finalFoamColor);
    }

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

            if (srcSimple != null)
            {
                dstSimple.strength        = RandomizeAround(srcSimple.strength);
                dstSimple.baseRoughness   = RandomizeAround(srcSimple.baseRoughness);
                dstSimple.roughness       = RandomizeAround(srcSimple.roughness);
                dstSimple.persistence     = RandomizeAround(srcSimple.persistence);
                dstSimple.centre          = srcSimple.centre + Random.insideUnitSphere * randomFactor;
                dstSimple.minValue        = RandomizeAround(srcSimple.minValue);
                dstSimple.numLayers       = srcSimple.numLayers;
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
}
