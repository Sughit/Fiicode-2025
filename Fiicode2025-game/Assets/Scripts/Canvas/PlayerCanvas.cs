using UnityEngine;

public class PlayerCanvas : MonoBehaviour
{
    void Awake()
    {
        if (PlayerController.instance != null)
        {
            PlayerController.instance.OnOpenResearchMenu += OpenResearchMenu;
        }
    }

    void OpenResearchMenu()
    {
        CanvasManager.instance.ToggleResearchMenu();
    }
}
