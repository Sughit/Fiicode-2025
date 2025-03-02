using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class PlanetDataManager : MonoBehaviour
{
    private const string SavedPlanetKey = "SavedPlanetData";

    [Header("Referințe")]
    [Tooltip("ScriptableObject-ul care conține datele pentru cele 5 planete (asset în Project).")]
    [SerializeField] private PlanetData planetData;
    
    [Tooltip("Referințe la cele 5 obiecte din scenă ce au componenta RandomPlanet.")]
    [SerializeField] private RandomPlanet[] randomPlanets;
    public static PlanetDataManager instance;

    void Start()
    {
        // Verificăm dacă există date salvate în PlayerPrefs
        if (PlayerPrefs.HasKey(SavedPlanetKey))
        {
            Debug.Log("Există date salvate. Le încărcăm și le aplicăm planetelor.");

            // 1. Încărcăm datele din PlayerPrefs
            string json = PlayerPrefs.GetString(SavedPlanetKey);
            // Suprascriem asset-ul PlanetData cu ce era salvat
            JsonUtility.FromJsonOverwrite(json, planetData);

            // 2. Aplicăm datele încărcate la planetele din scenă
            for (int i = 0; i < randomPlanets.Length; i++)
            {
                randomPlanets[i].shapeSettings = planetData.planets[i].shapeSettings;
                randomPlanets[i].colourSettings = planetData.planets[i].colourSettings;

                // Ca să fii sigur că nu mai face random, poți dezactiva randomizarea
                randomPlanets[i].randomizeShape = false;
                randomPlanets[i].randomizeColours = false;

                // Apoi generezi planeta pe baza datelor încărcate
                randomPlanets[i].GeneratePlanet();
            }
        }
        else
        {
            Debug.Log("Nu există date salvate. Generăm planete random și le salvăm.");

            // 1. Generăm random planetele
            for (int i = 0; i < randomPlanets.Length; i++)
            {
                // Asta va face random pentru că randomizeShape / randomizeColours sunt true
                randomPlanets[i].GeneratePlanet();

                // 2. Copiem datele random generate în asset (PlanetData)
                planetData.planets[i].shapeSettings = randomPlanets[i].shapeSettings;
                planetData.planets[i].colourSettings = randomPlanets[i].colourSettings;
            }

            // 3. Salvăm datele în PlayerPrefs
            string json = JsonUtility.ToJson(planetData);
            PlayerPrefs.SetString(SavedPlanetKey, json);
            PlayerPrefs.Save();

            // (Opțional) În Editor, salvăm și asset-ul ca să rămână modificările
#if UNITY_EDITOR
            EditorUtility.SetDirty(planetData);
            AssetDatabase.SaveAssets();
#endif
        }
    }

    // Metodă utilă dacă vrei să accesezi PlanetData din alte scripturi
    public PlanetData GetPlanetData()
    {
        return planetData;
    }
}
