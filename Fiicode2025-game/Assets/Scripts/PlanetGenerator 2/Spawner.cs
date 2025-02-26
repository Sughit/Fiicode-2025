using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SpawnableObject
{
    public GameObject prefab; // Object to spawn
    public int count; // Number of times to spawn
    public Color spawnColor; // Color on which to spawn
}

public class Spawner : MonoBehaviour
{
    public List<SpawnableObject> spawnableObjects; // List of spawnable objects
    public float colorThreshold = 0.1f; // Color match threshold

    private Texture2D gradientTexture;
    private float planetRadius;
    private float minHeight;
    private float maxHeight;

    public void SpawnObjects(ShapeGenerator shapeGenerator)
    {
        MeshRenderer renderer = GetPlanetMeshRenderer();
        if (renderer == null || renderer.sharedMaterial == null)
        {
            Debug.LogError("No MeshRenderer found on the planet's children!");
            return;
        }

        // Get shader properties
        gradientTexture = (Texture2D)renderer.sharedMaterial.GetTexture("_GradientTex");
        planetRadius = renderer.sharedMaterial.GetFloat("_PlanetRadius");
        minHeight = renderer.sharedMaterial.GetFloat("_MinHeight");
        maxHeight = renderer.sharedMaterial.GetFloat("_MaxHeight");

        if (gradientTexture == null)
        {
            Debug.LogError("Gradient texture not found in shader!");
            return;
        }

        if (spawnableObjects.Count == 0)
        {
            Debug.LogError("No spawnable objects defined!");
            return;
        }

        int totalSpawned = 0;
        int maxAttempts = 1000; // Prevent infinite loops
        int attempts = 0;

        // Iterate through each spawnable object
        foreach (var spawnable in spawnableObjects)
        {
            int successfulSpawns = 0;
            while (successfulSpawns < spawnable.count && attempts < maxAttempts)
            {
                attempts++;

                // Generate a random point on a unit sphere
                Vector3 randomPointOnUnitSphere = Random.onUnitSphere;

                // Get actual surface position using ShapeGenerator
                Vector3 spawnPosition = shapeGenerator.CalculatePointOnPlanet(randomPointOnUnitSphere);

                // Get the terrain color at this position
                Color surfaceColor = GetColorAtPosition(spawnPosition);

                // Check if the color matches the spawn color
                if (ColorCloseEnough(surfaceColor, spawnable.spawnColor, colorThreshold))
                {
                    // Instantiate the object
                    GameObject spawnedObject = Instantiate(spawnable.prefab, spawnPosition, Quaternion.identity);

                    // Align object to planet surface
                    spawnedObject.transform.up = spawnPosition.normalized;

                    successfulSpawns++;
                    totalSpawned++;
                }
            }
        }

        Debug.Log($"Total objects spawned: {totalSpawned}");
    }

    MeshRenderer GetPlanetMeshRenderer()
    {
        // Loop through all children of the Planet GameObject
        foreach (Transform child in transform)
        {
            MeshRenderer meshRenderer = child.GetComponent<MeshRenderer>();
            if (meshRenderer != null)
            {
                return meshRenderer; // Return the first MeshRenderer found
            }
        }
        return null;
    }

    Color GetColorAtPosition(Vector3 worldPosition)
    {
        if (gradientTexture == null) return Color.black;

        // Compute height the same way as the shader does
        float height = worldPosition.magnitude - planetRadius;
        float t = Mathf.InverseLerp(minHeight, maxHeight, height);

        // Clamp t to ensure it's in the 0-1 range
        t = Mathf.Clamp01(t);

        // Sample color from the gradient texture
        return gradientTexture.GetPixelBilinear(t, 0.5f);
    }

    bool ColorCloseEnough(Color a, Color b, float threshold)
    {
        return Mathf.Abs(a.r - b.r) < threshold &&
               Mathf.Abs(a.g - b.g) < threshold &&
               Mathf.Abs(a.b - b.b) < threshold;
    }
}
