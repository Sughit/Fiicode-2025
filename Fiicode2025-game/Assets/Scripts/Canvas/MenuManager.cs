using UnityEngine;
using UnityEngine.SceneManagement;
public class MenuManager : MonoBehaviour
{
    public GameObject main, settings;
    public void Play()
    {
        SceneManager.LoadScene("Main");
    }

    public void Settings()
    {
        settings.SetActive(true);
        main.SetActive(false);
    }

    public void Quit()
    {
        Application.Quit();
    }
    public void BackSettings()
    {
        settings.SetActive(false);
        main.SetActive(true);
    }
}
