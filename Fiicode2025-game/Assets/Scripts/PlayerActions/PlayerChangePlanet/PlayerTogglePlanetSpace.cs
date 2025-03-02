using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerTogglePlanetSpace : MonoBehaviour
{
    [SerializeField] private bool onPlanet = true;
    void Start()
    {
        if (PlayerController.instance != null)
        {
            PlayerController.instance.OnTogglePlanetSpace += TogglePlanetSpace;
        }
    }

    void TogglePlanetSpace()
    {
        SceneManager.LoadScene("Space");
    }
}
