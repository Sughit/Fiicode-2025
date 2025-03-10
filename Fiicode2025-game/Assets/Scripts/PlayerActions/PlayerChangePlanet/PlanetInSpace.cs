using UnityEngine;
using UnityEngine.UI;  
using UnityEngine.SceneManagement; // Pentru a folosi SceneManager

public class PlanetInSpace : MonoBehaviour
{
    private RandomPlanet randomPlanet;

    [SerializeField] private Text overlayText;
    [SerializeField] private GameObject infoMenu;

    // Indică dacă aceasta este planeta de început
    [SerializeField] bool isStartingPlanet = false;

    // Numele scenei ce va fi încărcată când dai clic pe planetă
    [SerializeField] private string sceneToLoadOnClick;
    private bool mouseOver = false;

    void Start()
    {
        randomPlanet = GetComponent<RandomPlanet>();

        // Ascunde overlay-ul la pornire
        if (infoMenu != null)
        {
            infoMenu.SetActive(false);
        }
    }

    // Când cursorul intră pe collider
    void OnMouseEnter()
    {
        Debug.Log(gameObject.name);

        if (overlayText != null && infoMenu != null)
        {
            string info = "Name: " + gameObject.name + "\n";
            if (isStartingPlanet)
            {
                // Planeta de început (presupunem tip Perfect și mărime Medium)
                info += "Type: Perfect\n";
                info += "Size: Medium";
            }
            else if (randomPlanet != null)
            {
                // Determină dimensiunea în funcție de raza generată
                float radius = randomPlanet.shapeSettings.planetRadius;
                string sizeCategory = "";
                if (radius < 20f)
                    sizeCategory = "Small";
                else if (radius < 25f)
                    sizeCategory = "Medium";
                else
                    sizeCategory = "Large";

                info += "Type: " + randomPlanet.planetType.ToString() + "\n";
                info += "Size: " + sizeCategory;
            }
            overlayText.text = info;
            infoMenu.SetActive(true);
        }

        mouseOver = true;
    }

    // Când cursorul iese de pe collider
    void OnMouseExit()
    {
        if (overlayText != null && infoMenu != null)
        {
            infoMenu.SetActive(false);
            overlayText.text = "";
        }
        mouseOver = false;
    }

    // La clic pe planetă
    void OnMouseDown()
    {
        // Încarcă scena specificată dacă este setată și cursorul este peste planetă
        if (!string.IsNullOrEmpty(sceneToLoadOnClick) && mouseOver)
        {
            SceneManager.LoadScene(sceneToLoadOnClick);
        }
        else
        {
            Debug.LogWarning("Nu a fost setat niciun nume de scenă pentru " + gameObject.name);
        }
    }
}
