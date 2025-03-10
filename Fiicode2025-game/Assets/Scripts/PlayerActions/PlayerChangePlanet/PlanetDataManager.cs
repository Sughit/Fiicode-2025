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
        // Asignăm un planetIndex unic pentru fiecare planetă (0, 1, 2, 3, 4)
        for (int i = 0; i < randomPlanets.Length; i++)
        {
            randomPlanets[i].planetIndex = i;
        }

        // Verificăm dacă există date salvate în PlayerPrefs
        if (PlayerPrefs.HasKey(SavedPlanetKey))
        {
            Debug.Log("Există date salvate. Le încărcăm și le aplicăm planetelor.");

            string json = PlayerPrefs.GetString(SavedPlanetKey);
            JsonUtility.FromJsonOverwrite(json, planetData);

            for (int i = 0; i < randomPlanets.Length; i++)
            {
                randomPlanets[i].shapeSettings = planetData.planets[i].shapeSettings;
                randomPlanets[i].colourSettings = planetData.planets[i].colourSettings;
                randomPlanets[i].GeneratePlanet();
            }
        }
        else
        {
            Debug.Log("Nu există date salvate. Generăm planete random și le salvăm.");

            for (int i = 0; i < randomPlanets.Length; i++)
            {
                randomPlanets[i].GeneratePlanet();
                planetData.planets[i].shapeSettings = randomPlanets[i].shapeSettings;
                planetData.planets[i].colourSettings = randomPlanets[i].colourSettings;
            }

            string json = JsonUtility.ToJson(planetData);
            PlayerPrefs.SetString(SavedPlanetKey, json);
            PlayerPrefs.Save();

#if UNITY_EDITOR
            EditorUtility.SetDirty(planetData);
            AssetDatabase.SaveAssets();
#endif
        }
    }

    public PlanetData GetPlanetData()
    {
        return planetData;
    }
}
