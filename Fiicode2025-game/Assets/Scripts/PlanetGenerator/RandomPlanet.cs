using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode] // Permite rularea metodei GeneratePlanet() în modul Edit
public class RandomPlanet : Planet {

    [Header("Base Settings")]
    [Tooltip("ShapeSettings-ul sursă din care se va inspira această planetă. Acest asset nu se va modifica.")]
    public ShapeSettings baseShapeSettings;

    // Setările specifice pentru randomizare
    public enum PlanetSize { Small, Medium, Large, Random }
    public PlanetSize planetSize = PlanetSize.Medium;

    [HideInInspector]
    public enum PlanetType { Cold, Hot, Paradise, Random }
    public PlanetType planetType = PlanetType.Random; // Parametru ascuns care influențează culorile planetei
    public PlanetType aproxType; // Tipul aproximativ al planetei, folosit pentru a afișa un mesaj

    [Header("Shape Settings Limits")]
    [Tooltip("Numărul minim de layere permise în ShapeSettings")]
    public int minNoiseLayers = 2;
    [Tooltip("Numărul maxim de layere permise în ShapeSettings")]
    [Range(1, 10)]
    public int maxNoiseLayers = 4;

    void Start() {
        GeneratePlanet();
    }

    // Clonăm baseShapeSettings în shapeSettings (câmpul moștenit din Planet) și aplicăm variațiile doar pe copie.
    public new void GeneratePlanet() {
        // Dacă avem un ShapeSettings sursă, clonăm pentru a nu modifica asset-ul original
        if (baseShapeSettings != null) {
            shapeSettings = CloneShapeSettings(baseShapeSettings);
        }
        
        // Randomizează raza planetei în funcție de mărime
        float minRadius, maxRadius;
        switch (planetSize) {
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

        // Generare gradient de culori în funcție de tipul planetei
        Gradient gradient = new Gradient();
        GradientColorKey[] colorKeys;
        GradientAlphaKey[] alphaKeys;

        if (planetType == PlanetType.Cold) {
            colorKeys = new GradientColorKey[5];

            // Înlocuim culoarea green de la index 0 cu un albastru deschis (0.5f, 0.75f, 1f)
            colorKeys[0] = new GradientColorKey(new Color(0.5f, 0.75f, 1f), 0f); // albastru deschis
            colorKeys[1] = new GradientColorKey(new Color(0f, 0f, 0.5f), 0.25f);
            colorKeys[2] = new GradientColorKey(Color.cyan, 0.5f);
            colorKeys[3] = new GradientColorKey(new Color(0.5f, 0f, 0.5f), 0.75f);
            colorKeys[4] = new GradientColorKey(Color.white, 1f);

            // Setăm alpha la 1 pentru toate punctele
            alphaKeys = new GradientAlphaKey[5];
            for (int i = 0; i < 5; i++) {
                alphaKeys[i] = new GradientAlphaKey(1f, colorKeys[i].time);
            }
        }
        else if (planetType == PlanetType.Hot) {
            colorKeys = new GradientColorKey[4];
            colorKeys[0] = new GradientColorKey(Color.yellow, 0f);
            colorKeys[1] = new GradientColorKey(Color.red, 0.33f);
            colorKeys[2] = new GradientColorKey(new Color(1f, 0.5f, 0f), 0.66f);
            colorKeys[3] = new GradientColorKey(new Color(1f, 0.5f, 1f), 1f);

            alphaKeys = new GradientAlphaKey[4];
            for (int i = 0; i < 4; i++) {
                alphaKeys[i] = new GradientAlphaKey(1f, colorKeys[i].time);
            }
        }
        else if (planetType == PlanetType.Paradise)
        {
            // Setăm un tablou cu 6 culori cheie
            colorKeys = new GradientColorKey[6];
            colorKeys[0] = new GradientColorKey(new Color(0.0f, 0.05f, 0.17f), 0.0f);    // albastru foarte închis
            colorKeys[1] = new GradientColorKey(new Color(0.20f, 0.34f, 0.50f), 0.15f);  // nuanță intermediară de albastru
            colorKeys[2] = new GradientColorKey(new Color(0.75f, 0.80f, 0.82f), 0.30f);  // zonă mai deschisă/gri deschis
            colorKeys[3] = new GradientColorKey(new Color(0.33f, 0.73f, 0.16f), 0.55f);  // verde
            colorKeys[4] = new GradientColorKey(new Color(0.53f, 0.32f, 0.09f), 0.80f);  // maro
            colorKeys[5] = new GradientColorKey(new Color(0.94f, 0.94f, 0.94f), 1.0f);   // alb

            // Toate aceste culori vor avea alpha = 1
            alphaKeys = new GradientAlphaKey[colorKeys.Length];
            for (int i = 0; i < colorKeys.Length; i++)
            {
                alphaKeys[i] = new GradientAlphaKey(1f, colorKeys[i].time);
            }
        }
        else { // Random
            int keyCount = Random.Range(3, 6);
            colorKeys = new GradientColorKey[keyCount];
            alphaKeys = new GradientAlphaKey[keyCount];

            Vector3 sumColor = Vector3.zero; // Vector3(r, g, b) pentru a ține suma culorilor

            for (int i = 0; i < keyCount; i++) {
                float t = i / (float)(keyCount - 1);

                // Generăm o culoare random
                Color randomColor = new Color(Random.value, Random.value, Random.value);

                // O adăugăm la suma totală, pentru statistici
                sumColor += new Vector3(randomColor.r, randomColor.g, randomColor.b);

                colorKeys[i] = new GradientColorKey(randomColor, t);
                alphaKeys[i] = new GradientAlphaKey(1f, t);
            }

            gradient.SetKeys(colorKeys, alphaKeys);
            colourSettings.gradient = gradient;

            // Calculează media culorilor (R, G, B)
            Vector3 avgColor = sumColor / keyCount;

            // Verificăm care canal e dominant
            float maxValue = Mathf.Max(avgColor.x, Mathf.Max(avgColor.y, avgColor.z));

            if (Mathf.Approximately(maxValue, avgColor.x)) {
                Debug.Log("Planeta Random: Culoarea medie are mult roșu; ar putea semăna cu o planetă de tip 'Hot'.");
                aproxType = PlanetType.Hot;
            }
            else if (Mathf.Approximately(maxValue, avgColor.y)) {
                Debug.Log("Planeta Random: Culoarea medie are mult verde; ar putea semăna cu o planetă de tip 'Paradise'.");
                aproxType = PlanetType.Paradise;
            }
            else {
                Debug.Log("Planeta Random: Culoarea medie are mult albastru; ar putea semăna cu o planetă de tip 'Cold'.");
                aproxType = PlanetType.Cold;
            }
        }
        gradient.SetKeys(colorKeys, alphaKeys);
        colourSettings.gradient = gradient;

        // Randomizează numărul de layere (straturi) de zgomot între minNoiseLayers și maxNoiseLayers
        int targetLayerCount = Random.Range(minNoiseLayers, maxNoiseLayers + 1);
        int currentLayers = (shapeSettings.noiseLayers != null) ? shapeSettings.noiseLayers.Length : 0;
        if (currentLayers < targetLayerCount) {
            // Adaugă layere clonând ultimul layer existent (sau creând unul nou dacă nu există niciunul)
            ShapeSettings.NoiseLayer[] newLayers = new ShapeSettings.NoiseLayer[targetLayerCount];
            for (int i = 0; i < currentLayers; i++) {
                newLayers[i] = shapeSettings.noiseLayers[i];
            }
            ShapeSettings.NoiseLayer baseLayer = (currentLayers > 0) ? shapeSettings.noiseLayers[currentLayers - 1] : new ShapeSettings.NoiseLayer();
            for (int i = currentLayers; i < targetLayerCount; i++) {
                newLayers[i] = CloneNoiseLayer(baseLayer);
            }
            shapeSettings.noiseLayers = newLayers;
        }
        else if (currentLayers > targetLayerCount) {
            // Taie layerele în exces
            ShapeSettings.NoiseLayer[] limitedLayers = new ShapeSettings.NoiseLayer[targetLayerCount];
            for (int i = 0; i < targetLayerCount; i++) {
                limitedLayers[i] = shapeSettings.noiseLayers[i];
            }
            shapeSettings.noiseLayers = limitedLayers;
        }

        // Pentru fiecare layer, aplică variații pentru toate variabilele zgomotului, în limite moderate
        if (shapeSettings.noiseLayers != null) {
            foreach (var layer in shapeSettings.noiseLayers) {
                if (layer.noiseSettings != null) {
                    // Pentru tipul Simple
                    if (layer.noiseSettings.filterType == NoiseSettings.FilterType.Simple && layer.noiseSettings.simpleNoiseSettings != null) {
                        var settings = layer.noiseSettings.simpleNoiseSettings;
                        settings.strength = Random.Range(settings.strength * 0.9f, settings.strength * 1.1f);
                        settings.baseRoughness = Random.Range(settings.baseRoughness * 0.9f, settings.baseRoughness * 1.1f);
                        settings.roughness = Random.Range(settings.roughness * 0.9f, settings.roughness * 1.1f);
                        settings.persistence = Random.Range(settings.persistence * 0.9f, settings.persistence * 1.1f);
                        settings.minValue = Random.Range(settings.minValue * 0.9f, settings.minValue * 1.1f);
                        settings.centre += new Vector3(Random.Range(-0.1f, 0.1f), Random.Range(-0.1f, 0.1f), Random.Range(-0.1f, 0.1f));
                        int origLayers = settings.numLayers;
                        settings.numLayers = Random.Range(Mathf.Max(1, origLayers - 1), Mathf.Min(8, origLayers + 1) + 1);
                    }
                    // Pentru tipul Ridgid
                    else if (layer.noiseSettings.filterType == NoiseSettings.FilterType.Ridgid && layer.noiseSettings.ridgidNoiseSettings != null) {
                        var settings = layer.noiseSettings.ridgidNoiseSettings;
                        settings.strength = Random.Range(settings.strength * 0.9f, settings.strength * 1.1f);
                        settings.baseRoughness = Random.Range(settings.baseRoughness * 0.9f, settings.baseRoughness * 1.1f);
                        settings.roughness = Random.Range(settings.roughness * 0.9f, settings.roughness * 1.1f);
                        settings.persistence = Random.Range(settings.persistence * 0.9f, settings.persistence * 1.1f);
                        settings.minValue = Random.Range(settings.minValue * 0.9f, settings.minValue * 1.1f);
                        settings.centre += new Vector3(Random.Range(-0.1f, 0.1f), Random.Range(-0.1f, 0.1f), Random.Range(-0.1f, 0.1f));
                        int origLayers = settings.numLayers;
                        settings.numLayers = Random.Range(Mathf.Max(1, origLayers - 1), Mathf.Min(8, origLayers + 1) + 1);
                        settings.weightMultiplier = Random.Range(settings.weightMultiplier * 0.9f, settings.weightMultiplier * 1.1f);
                    }
                }
            }
        }

        // Apelează metoda de generare din clasa de bază pentru a crea mesh-ul și culorile
        base.GeneratePlanet();

        // Actualizează uniforma pentru raza planetei pe material, dacă este setat
        if (colourSettings.planetMaterial != null) {
            colourSettings.planetMaterial.SetFloat("_PlanetRadius", shapeSettings.planetRadius);
        }

        RandomSpawner spawner = GetComponent<RandomSpawner>();
        if (spawner != null)
        {
            spawner.SpawnNow(); 
        }
        else
        {
            Debug.LogWarning("Nu s-a găsit un RandomSpawner pe acest GameObject!");
        }
    }

    // Metodă pentru clonarea unui ShapeSettings (realizează o clonare superficială a valorilor),
    // însă noiseLayers și noiseSettings vor fi clonate profund în CloneNoiseLayer.
    private ShapeSettings CloneShapeSettings(ShapeSettings original) {
        ShapeSettings clone = new ShapeSettings();
        clone.planetRadius = original.planetRadius;

        if (original.noiseLayers != null) {
            clone.noiseLayers = new ShapeSettings.NoiseLayer[original.noiseLayers.Length];
            for (int i = 0; i < original.noiseLayers.Length; i++) {
                clone.noiseLayers[i] = CloneNoiseLayer(original.noiseLayers[i]);
            }
        }

        // Alte câmpuri din ShapeSettings pot fi clonate aici (dacă există)

        return clone;
    }

    // Metodă utilitară pentru clonarea unui layer de zgomot (copie profundă a noiseSettings).
    private ShapeSettings.NoiseLayer CloneNoiseLayer(ShapeSettings.NoiseLayer original) {
        ShapeSettings.NoiseLayer clone = new ShapeSettings.NoiseLayer();
        clone.enabled = original.enabled;
        clone.useFirstLayerAsMask = original.useFirstLayerAsMask;
        clone.noiseSettings = CloneNoiseSettings(original.noiseSettings); 
        return clone;
    }

    // Metodă de clonare completă a unui NoiseSettings
    private NoiseSettings CloneNoiseSettings(NoiseSettings original) {
        NoiseSettings clone = new NoiseSettings();
        clone.filterType = original.filterType;

        // Clonăm setările Simple
        if (original.simpleNoiseSettings != null) {
            clone.simpleNoiseSettings = new NoiseSettings.SimpleNoiseSettings();
            clone.simpleNoiseSettings.strength       = original.simpleNoiseSettings.strength;
            clone.simpleNoiseSettings.numLayers      = original.simpleNoiseSettings.numLayers;
            clone.simpleNoiseSettings.baseRoughness  = original.simpleNoiseSettings.baseRoughness;
            clone.simpleNoiseSettings.roughness      = original.simpleNoiseSettings.roughness;
            clone.simpleNoiseSettings.persistence    = original.simpleNoiseSettings.persistence;
            clone.simpleNoiseSettings.centre        = original.simpleNoiseSettings.centre;
            clone.simpleNoiseSettings.minValue       = original.simpleNoiseSettings.minValue;
        }

        // Clonăm setările Ridgid
        if (original.ridgidNoiseSettings != null) {
            clone.ridgidNoiseSettings = new NoiseSettings.RidgidNoiseSettings();
            clone.ridgidNoiseSettings.strength        = original.ridgidNoiseSettings.strength;
            clone.ridgidNoiseSettings.numLayers       = original.ridgidNoiseSettings.numLayers;
            clone.ridgidNoiseSettings.baseRoughness   = original.ridgidNoiseSettings.baseRoughness;
            clone.ridgidNoiseSettings.roughness       = original.ridgidNoiseSettings.roughness;
            clone.ridgidNoiseSettings.persistence     = original.ridgidNoiseSettings.persistence;
            clone.ridgidNoiseSettings.centre          = original.ridgidNoiseSettings.centre;
            clone.ridgidNoiseSettings.minValue        = original.ridgidNoiseSettings.minValue;
            clone.ridgidNoiseSettings.weightMultiplier= original.ridgidNoiseSettings.weightMultiplier;
        }

        return clone;
    }
}
