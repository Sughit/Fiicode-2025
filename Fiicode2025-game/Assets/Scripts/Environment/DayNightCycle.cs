using UnityEngine;

public class DayNightCycle : MonoBehaviour
{
    [Header("Time Settings")]
    public float dayDuration = 120f; // Full day duration in seconds
    private float timeOfDay = 0f; // 0-1 normalized time

    [Header("Lighting")]
    public Light sun;
    public Light moon;
    public Gradient sunColorGradient; // Gradient for sun color
    public Gradient fogColorGradient; // Gradient for fog color

    [Header("Skybox Blending")]
    public Material skyboxMaterial; // Blended skybox material
    private float blendFactor;

    private float sunInitialIntensity;
    private float moonInitialIntensity;

    private void Start()
    {
        sunInitialIntensity = sun.intensity;
        moonInitialIntensity = moon.intensity;
    }

    private void Update()
    {
        // Progress time
        timeOfDay += (Time.deltaTime / dayDuration);
        if (timeOfDay > 1f) timeOfDay = 0f; // Reset at midnight

        UpdateLighting();
        UpdateSkybox();
        UpdateFog();
    }

    void UpdateLighting()
    {
        float sunAngle = timeOfDay * 360f - 90f; // Rotate sun from -90 to 270 degrees
        sun.transform.rotation = Quaternion.Euler(sunAngle, 170, 0);
        moon.transform.rotation = Quaternion.Euler(sunAngle + 180, 170, 0);

        // Adjust light intensity
        float sunIntensityFactor = Mathf.Clamp01(Vector3.Dot(sun.transform.forward, Vector3.down));
        float moonIntensityFactor = Mathf.Clamp01(Vector3.Dot(moon.transform.forward, Vector3.down));

        sun.intensity = sunInitialIntensity * sunIntensityFactor;
        moon.intensity = moonInitialIntensity * moonIntensityFactor;

        // Adjust sun color based on gradient
        sun.color = sunColorGradient.Evaluate(timeOfDay);
    }

    void UpdateSkybox()
    {
        // Calculate blend factor for smooth transition
        blendFactor = Mathf.Sin(timeOfDay * Mathf.PI); // 0 at midnight, 1 at noon

        // Apply to shader
        skyboxMaterial.SetFloat("_Blend", blendFactor);

        // Ensure Unity updates the skybox
        DynamicGI.UpdateEnvironment();
    }

    void UpdateFog()
    {
        // Change fog color based on time of day
        RenderSettings.fogColor = fogColorGradient.Evaluate(timeOfDay);
    }
}
