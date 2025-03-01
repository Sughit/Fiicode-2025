using UnityEngine;

public class DayNightCycle : MonoBehaviour
{
    public float dayDuration = 120f;

    public Material skyboxMaterial;

    // Normalizat 0..1 (0 = miezul nopții, 0.5 = amiază, 1 = următor miez de noapte)
    private float timeOfDay = 0f;

    private void Start()
    {
        // Opțional: asigură-te că materialul acesta e setat ca Skybox
        RenderSettings.skybox = skyboxMaterial;
    }

    private void Update()
    {
        // Incrementăm timpul din zi (0..1)
        timeOfDay += (Time.deltaTime / dayDuration);
        if (timeOfDay > 1f) 
            timeOfDay = 0f; // Resetează la miezul nopții

        // Calculează factorul de tranziție
        // Poți folosi direct timeOfDay (tranziție liniară) sau un sine (tranziție mai domoală).
        // Mai jos exemplu cu sin(π * t):
        float transitionFactor = Mathf.Sin(timeOfDay * Mathf.PI);

        // Actualizează proprietatea shader-ului
        skyboxMaterial.SetFloat("_CubemapTransition", transitionFactor);

        // Dacă vrei să actualizezi reflexiile globale în timp real:
        DynamicGI.UpdateEnvironment();
    }
}
