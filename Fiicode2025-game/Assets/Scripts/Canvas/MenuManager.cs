using UnityEngine;
using UnityEngine.SceneManagement;
public class MenuManager : MonoBehaviour
{
    public void Play()
    {
        SceneManager.LoadScene("Main");
    }

    public void Settings()
    {

    }

    public void Quit()
    {
        Application.Quit();
    }
}
