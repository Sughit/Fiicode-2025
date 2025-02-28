using UnityEngine;
using UnityEngine.InputSystem;
public class CanvasManager : MonoBehaviour
{
    [SerializeField] private GameObject researchMenu;

    [SerializeField] private GameObject[] researchMenus;

    public static CanvasManager instance;

    void Awake()
    {
        if(instance == null) instance = this;
        else Destroy(this);
    }

    public void ToggleResearchMenu()
    {
        researchMenu.SetActive(!researchMenu.activeSelf);
    }

    public void OpenResearchMenuCategory(GameObject menu)
    {
        for(int i=0; i<researchMenus.Length; i++)
        {
            if(researchMenus[i] != menu) researchMenus[i].SetActive(false);
        }
        menu.SetActive(true);
    }
}
