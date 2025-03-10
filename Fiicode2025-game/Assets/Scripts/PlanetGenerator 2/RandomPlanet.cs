using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlanetSize { Small, Medium, Large, Random }
public enum PlanetType { Cold, Hot, Perfect, Random }

public class RandomPlanet : MonoBehaviour
{
    [Header("Planet Settings")]
    public PlanetSize planetSize = PlanetSize.Medium;
    public PlanetType planetType = PlanetType.Random;
    
    [Tooltip("Indicele planetei (0-4). Acesta este folosit pentru a deriva seed-ul, astfel încât cele 5 planete să fie diferite.")]
    public int planetIndex = 0;

    [Header("Randomization Settings")]
    public bool useSeed = true;
    public int baseSeed = 1000;
    [HideInInspector] public int currentSeed;

    [Header("Shape Settings Limits")]
    [Tooltip("Numărul minim de layere permise în shapeSettings")]
    public int minNoiseLayers = 1;
    [Tooltip("Numărul maxim de layere permise în shapeSettings")]
    [Range(1, 10)]
    public int maxNoiseLayers = 4;

    [Header("General Settings")]
    [Range(2, 256)]
    public int resolution = 10;
    public bool autoUpdate = true;
    public enum FaceRenderMask { All, Top, Bottom, Left, Right, Front, Back }
    public FaceRenderMask faceRenderMask = FaceRenderMask.All;

    [Header("References")]
    public ShapeSettings shapeSettings;
    public ColourSettings colourSettings;

    // Date salvate pentru utilizări ulterioare
    [HideInInspector] public float savedPlanetRadius;
    [HideInInspector] public Gradient savedGradient;

    MeshFilter[] meshFilters;
    TerrainFace[] terrainFaces;
    ShapeGenerator shapeGenerator = new ShapeGenerator();
    ColourGenerator colourGenerator = new ColourGenerator();

    void Start()
    {
        GeneratePlanet();
    }

    public void GeneratePlanet()
    {
        // Folosim un seed unic pentru fiecare planetă, bazat pe baseSeed și planetIndex,
        // astfel încât, chiar dacă baseSeed este același, cele 5 planete vor arăta diferit.
        if (useSeed)
        {
            currentSeed = baseSeed + planetIndex; 
            Random.InitState(currentSeed);
        }

        // Setăm raza planetei conform mărimii alese
        float minRadius, maxRadius;
        switch (planetSize)
        {
            case PlanetSize.Small:
                minRadius = 15f; maxRadius = 20f; break;
            case PlanetSize.Medium:
                minRadius = 20f; maxRadius = 25f; break;
            case PlanetSize.Large:
                minRadius = 25f; maxRadius = 30f; break;
            case PlanetSize.Random:
            default:
                minRadius = 15f; maxRadius = 30f; break;
        }
        shapeSettings.planetRadius = Random.Range(minRadius, maxRadius);
        savedPlanetRadius = shapeSettings.planetRadius;  // Salvăm raza generată

        // Setăm gradientul în funcție de tipul de planetă
        Gradient gradient = new Gradient();
        GradientColorKey[] colorKeys;
        GradientAlphaKey[] alphaKeys;

        if (planetType == PlanetType.Cold)
        {
            colorKeys = new GradientColorKey[5];
            colorKeys[0] = new GradientColorKey(Color.green, 0f);
            colorKeys[1] = new GradientColorKey(new Color(0f, 0f, 0.5f), 0.25f);
            colorKeys[2] = new GradientColorKey(Color.cyan, 0.5f);
            colorKeys[3] = new GradientColorKey(new Color(0.5f, 0f, 0.5f), 0.75f);
            colorKeys[4] = new GradientColorKey(Color.white, 1f);

            alphaKeys = new GradientAlphaKey[5];
            for (int i = 0; i < 5; i++)
            {
                alphaKeys[i] = new GradientAlphaKey(1f, colorKeys[i].time);
            }
        }
        else if (planetType == PlanetType.Hot)
        {
            colorKeys = new GradientColorKey[4];
            colorKeys[0] = new GradientColorKey(Color.yellow, 0f);
            colorKeys[1] = new GradientColorKey(Color.red, 0.33f);
            colorKeys[2] = new GradientColorKey(new Color(1f, 0.5f, 0f), 0.66f);
            colorKeys[3] = new GradientColorKey(new Color(1f, 0.5f, 1f), 1f);

            alphaKeys = new GradientAlphaKey[4];
            for (int i = 0; i < 4; i++)
            {
                alphaKeys[i] = new GradientAlphaKey(1f, colorKeys[i].time);
            }
        }
        else if (planetType == PlanetType.Perfect)
        {
            // Gradient similar cu cel al Pământului
            colorKeys = new GradientColorKey[4];
            colorKeys[0] = new GradientColorKey(Color.blue, 0f);    // apă
            colorKeys[1] = new GradientColorKey(Color.green, 0.5f);   // teren
            colorKeys[2] = new GradientColorKey(new Color(0.5f, 0.25f, 0f), 0.75f); // munți
            colorKeys[3] = new GradientColorKey(Color.white, 1f);     // vârfuri de zăpadă

            alphaKeys = new GradientAlphaKey[4];
            for (int i = 0; i < 4; i++)
            {
                alphaKeys[i] = new GradientAlphaKey(1f, colorKeys[i].time);
            }
        }
        else // Random
        {
            int keyCount = Random.Range(3, 6);
            colorKeys = new GradientColorKey[keyCount];
            alphaKeys = new GradientAlphaKey[keyCount];
            for (int i = 0; i < keyCount; i++)
            {
                float t = i / (float)(keyCount - 1);
                colorKeys[i] = new GradientColorKey(new Color(Random.value, Random.value, Random.value), t);
                alphaKeys[i] = new GradientAlphaKey(1f, t);
            }
        }
        gradient.SetKeys(colorKeys, alphaKeys);
        colourSettings.gradient = gradient;
        savedGradient = gradient;  // Salvăm gradientul generat

        // Asigură-te că shapeSettings.noiseLayers nu este null sau gol
        if (shapeSettings.noiseLayers == null || shapeSettings.noiseLayers.Length == 0)
        {
            shapeSettings.noiseLayers = new ShapeSettings.NoiseLayer[1];
            shapeSettings.noiseLayers[0] = new ShapeSettings.NoiseLayer();
        }

        // Limităm numărul de layere din shapeSettings la valorile minime și maxime specificate
        int currentLayers = shapeSettings.noiseLayers.Length;
        if (currentLayers < minNoiseLayers)
        {
            // Completează cu clone ale ultimului layer pentru a atinge numărul minim
            ShapeSettings.NoiseLayer[] newLayers = new ShapeSettings.NoiseLayer[minNoiseLayers];
            for (int i = 0; i < currentLayers; i++)
            {
                newLayers[i] = shapeSettings.noiseLayers[i];
            }
            for (int i = currentLayers; i < minNoiseLayers; i++)
            {
                newLayers[i] = CloneNoiseLayer(shapeSettings.noiseLayers[currentLayers - 1]);
            }
            shapeSettings.noiseLayers = newLayers;
        }
        else if (currentLayers > maxNoiseLayers)
        {
            ShapeSettings.NoiseLayer[] limitedLayers = new ShapeSettings.NoiseLayer[maxNoiseLayers];
            for (int i = 0; i < maxNoiseLayers; i++)
            {
                limitedLayers[i] = shapeSettings.noiseLayers[i];
            }
            shapeSettings.noiseLayers = limitedLayers;
        }

        // Inițializăm și generăm mesh-ul și culorile
        Initialize();
        GenerateMesh();
        GenerateColours();

        // Setăm raza planetei pe materialul folosit
        if (colourSettings.planetMaterial != null)
        {
            colourSettings.planetMaterial.SetFloat("_PlanetRadius", shapeSettings.planetRadius);
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

        Vector3[] directions = new Vector3[]
        {
            Vector3.up, Vector3.down, Vector3.left,
            Vector3.right, Vector3.forward, Vector3.back
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

    void GenerateColours()
    {
        colourGenerator.UpdateColours();
    }

    // Metodă utilitară pentru a clona un layer (folosit la completarea numărului minim de layere)
    private ShapeSettings.NoiseLayer CloneNoiseLayer(ShapeSettings.NoiseLayer original)
    {
        ShapeSettings.NoiseLayer clone = new ShapeSettings.NoiseLayer();
        clone.enabled = original.enabled;
        clone.useFirstLayerAsMask = original.useFirstLayerAsMask;
        clone.noiseSettings = original.noiseSettings; // shallow copy
        return clone;
    }
}
