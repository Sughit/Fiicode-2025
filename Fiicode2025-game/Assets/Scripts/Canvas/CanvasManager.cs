using UnityEngine;
using UnityEngine.UI;       // Sau foloseste TMPro, in functie de ce ai in UI
using UnityEngine.InputSystem;

public class CanvasManager : MonoBehaviour
{
    // Meniul "principal" unde ai categoriile de research (din codul anterior)
    [SerializeField] private GameObject researchMenu;
    [SerializeField] private GameObject[] researchMenus;

    // Meniuri separate pentru fiecare tip de clădire
    [Header("Building UI")]
    [SerializeField] private GameObject mineMenu;
    [SerializeField] private GameObject houseMenu;
    [SerializeField] private GameObject foodSourceMenu;
    [SerializeField] private GameObject weaponMenu;
    [SerializeField] private GameObject metalProcessingMenu;
    [SerializeField] private GameObject woodProcessingMenu;
    [SerializeField] private GameObject stoneProcessingMenu;
    [SerializeField] private GameObject depotMenu;

    // Un text in care afișăm numele clădirii selectate (opțional)
    [SerializeField] private Text buildingNameText;
    [SerializeField] private GameObject buildingMenu;

    public static CanvasManager instance;

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(this);
    }

    #region ResearchMenu
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
    #endregion

    #region BuildingUI
    public void OpenBuildingUI(BuildingType type, string buildingName)
    {
        // Închide eventual toate meniurile altor clădiri (să nu fie deschis altceva)
        CloseAllBuildingMenus();
        buildingMenu.SetActive(true);

        // Actualizează un text care să arate numele clădirii (dacă ai nevoie)
        if (buildingNameText != null)
            buildingNameText.text = buildingName;

        // Activează meniul corect, în funcție de tip
        switch (type)
        {
            case BuildingType.Mine:
                mineMenu.SetActive(true);
                break;
            case BuildingType.House:
                houseMenu.SetActive(true);
                break;
            case BuildingType.FoodSource:
                foodSourceMenu.SetActive(true);
                break;
            case BuildingType.Weapon:
                weaponMenu.SetActive(true);
                break;
            case BuildingType.MetalProcessing:
                metalProcessingMenu.SetActive(true);
                break;
            case BuildingType.WoodProcessing:
                woodProcessingMenu.SetActive(true);
                break;
            case BuildingType.StoneProcessing:
                stoneProcessingMenu.SetActive(true);
                break;
            case BuildingType.Depot:
                depotMenu.SetActive(true);
                break;
        }
    }

    public void CloseBuildingUI()
    {
        // Închide tot ce ține de building
        CloseAllBuildingMenus();

        // Golește textul (opțional)
        if (buildingNameText != null)
            buildingNameText.text = "";

        buildingMenu.SetActive(false);
    }

    private void CloseAllBuildingMenus()
    {
        mineMenu.SetActive(false);
        houseMenu.SetActive(false);
        foodSourceMenu.SetActive(false);
        weaponMenu.SetActive(false);
        metalProcessingMenu.SetActive(false);
        woodProcessingMenu.SetActive(false);
        stoneProcessingMenu.SetActive(false);
        depotMenu.SetActive(false);
    }
    #endregion
}
