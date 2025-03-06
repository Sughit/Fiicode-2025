using UnityEngine;
using UnityEngine.UI;       // Sau foloseste TMPro, in functie de ce ai in UI
using UnityEngine.InputSystem;

public class CanvasManager : MonoBehaviour
{
    // Meniul "principal" unde ai categoriile de research (din codul anterior)
    [SerializeField] private GameObject researchMenu;
    [SerializeField] private GameObject tooltip;
    [SerializeField] private GameObject[] researchMenus;

    // Meniuri separate pentru fiecare tip de clădire
    [Header("Building UI")]
    [SerializeField] private GameObject mineMenu;
    [SerializeField] private GameObject plantMenu;
    [SerializeField] private GameObject foodMenu;
    [SerializeField] private GameObject metalMenu;
    [SerializeField] private GameObject miscMenu;
    [SerializeField] private GameObject lifeMenu;
    [SerializeField] private GameObject weaponMenu;
    [SerializeField] private GameObject depotMenu;

    // Un text in care afișăm numele clădirii selectate (opțional)
    [SerializeField] private Text buildingNameText;
    [SerializeField] private GameObject buildingMenu;

    [Header("Mining Icons")]
    [SerializeField] private Image miningIcon;

    [Header("Crafting Icons")]
    [SerializeField] private Image craftingInputIcon;
    [SerializeField] private Image craftingOutputIcon;

    private GameObject interactionGO;

    public static CanvasManager instance;

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(this);
    }

    #region ResearchMenu
    public void ToggleResearchMenu()
    {
        tooltip.SetActive(false);
        researchMenu.SetActive(!researchMenu.activeSelf);
        if(researchMenu.activeSelf)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
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
            case BuildingType.PlantCrafting:
                plantMenu.SetActive(true);
                break;
            case BuildingType.FoodCrafting:
                foodMenu.SetActive(true);
                break;
            case BuildingType.Weapon:
                weaponMenu.SetActive(true);
                break;
            case BuildingType.MetalCrafting:
                metalMenu.SetActive(true);
                break;
            case BuildingType.MiscCrafting:
                miscMenu.SetActive(true);
                break;
            case BuildingType.LifeCrafting:
                lifeMenu.SetActive(true);
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
        plantMenu.SetActive(false);
        foodMenu.SetActive(false);
        weaponMenu.SetActive(false);
        metalMenu.SetActive(false);
        miscMenu.SetActive(false);
        lifeMenu.SetActive(false);
        depotMenu.SetActive(false);
    }

    public void SetInteractionGO(GameObject go)
    {
        interactionGO = go;
    }

    public void CollectResourcesFromMine()
    {
        if(interactionGO != null)
        {
            interactionGO.GetComponent<Miner>().CollectResources();
        }
        else return;
    }
    #endregion

    #region Icons
    public void SetMiningIcons(Sprite icon)
    {
        if(miningIcon != null)
        {
            miningIcon.sprite = icon;
        } 
    }

    public void SetCraftingIcons(Sprite iconInput, Sprite iconOutput)
    {
        if(craftingInputIcon != null && craftingOutputIcon != null)
        {
            craftingInputIcon.sprite = iconInput;
            craftingOutputIcon.sprite = iconOutput;
        }
    }
    #endregion

}
