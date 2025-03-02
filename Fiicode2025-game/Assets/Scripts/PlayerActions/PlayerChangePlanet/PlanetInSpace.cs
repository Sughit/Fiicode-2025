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

    // Cursorul intră pe collider
    void OnMouseEnter()
    {
        Debug.Log(gameObject.name);

        if (overlayText != null && infoMenu != null)
        {
            string info = "Name: " + gameObject.name + "\n";
            if (isStartingPlanet)
            {
                // Planeta de început
                info += "Type: Perfect\n";
                info += "Size: Medium";
            }
            else if (randomPlanet != null)
            {
                // Determină dimensiunea în funcție de radius
                float radius = randomPlanet.shapeSettings.planetRadius;
                string sizeCategory = "";
                if (randomPlanet.useMainMenuPlanetSizes)
                {
                    if (radius < 5.5f) sizeCategory = "Small";
                    else if (radius < 7f) sizeCategory = "Medium";
                    else sizeCategory = "Large";
                }
                else
                {
                    if (radius < 20f) sizeCategory = "Small";
                    else if (radius < 25f) sizeCategory = "Medium";
                    else sizeCategory = "Large";
                }

                info += "Type: " + randomPlanet.planetType.ToString() + "\n";
                info += "Size: " + sizeCategory;
            }
            overlayText.text = info;
            infoMenu.SetActive(true);
        }

        mouseOver = true;
    }

    // Cursorul iese de pe collider
    void OnMouseExit()
    {
        if (overlayText != null && infoMenu != null)
        {
            infoMenu.SetActive(false);
            overlayText.text = "";
        }
        mouseOver = false;
    }

    // Când dai clic pe collider
    void OnMouseDown()
    {
        // Încarcă scena specificată dacă e setată
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
