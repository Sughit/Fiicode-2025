using UnityEngine;
using UnityEngine.UI;       // Sau foloseste TMPro, in functie de ce ai in UI
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class CanvasManager : MonoBehaviour
{
    // Meniul "principal" unde ai categoriile de research (din codul anterior)
    [SerializeField] private GameObject researchMenu;
    [SerializeField] private GameObject tooltip;
    [SerializeField] private GameObject[] researchMenus;

    // Meniuri separate pentru fiecare tip de clădire
    [Header("Building UI")]
    [SerializeField] private GameObject mineMenu;
    [SerializeField] private GameObject craftingMenu;
    [SerializeField] private GameObject weaponMenu;
    [SerializeField] private GameObject depotMenu;

    // Un text in care afișăm numele clădirii selectate (opțional)
    [SerializeField] private Text buildingNameText;
    [SerializeField] private GameObject buildingMenu;

    [Header("Mining Icons")]
    [SerializeField] private Image miningIcon;

    [Header("Crafting Info")]
    [SerializeField] private Image craftingIcon;
    [SerializeField] private CraftingInfoSO currentRecipe;
    [SerializeField] private Dropdown craftingDropdown;

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
                craftingMenu.SetActive(true);
                break;
            case BuildingType.FoodCrafting:
                craftingMenu.SetActive(true);
                break;
            case BuildingType.Weapon:
                weaponMenu.SetActive(true);
                break;
            case BuildingType.MetalCrafting:
                craftingMenu.SetActive(true);
                break;
            case BuildingType.MiscCrafting:
                craftingMenu.SetActive(true);
                break;
            case BuildingType.LifeCrafting:
                craftingMenu.SetActive(true);
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
        craftingMenu.SetActive(false);
        weaponMenu.SetActive(false);
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

    public void MakeItemFromCrafter()
    {
        if(interactionGO != null)
        {
            interactionGO.GetComponent<Crafter>().MakeItem(currentRecipe.inputs, currentRecipe.outputName, currentRecipe.outputAmount);
        }
        else return;
    }
    #endregion

    #region Crafting Dropdown Management

    // Populează dropdown-ul cu opțiuni și cu referințele la CraftingInfoSO
    public void SetupCrafterDropdown(List<string> options, List<CraftingInfoSO> recipes)
    {
        if (craftingDropdown == null) return;
        craftingDropdown.ClearOptions();
        craftingDropdown.AddOptions(options);
        // Eliminăm eventualii listener anteriori
        craftingDropdown.onValueChanged.RemoveAllListeners();
        // Adăugăm listener-ul care actualizează iconița din CraftingInfoSO și reține ultima rețetă selectată
        craftingDropdown.onValueChanged.AddListener((index) =>
        {
            if (index < recipes.Count)
            {
                SetCraftingIcons(recipes[index].outputIcon);
                currentRecipe = recipes[index];
                // Dacă obiectul interacționat este un Crafter, actualizăm indexul ultimei rețete
                if (interactionGO != null)
                {
                    Crafter crafter = interactionGO.GetComponent<Crafter>();
                    if (crafter != null)
                    {
                        crafter.SetLastSelectedRecipe(index);
                    }
                }
            }
        });
        // Setăm iconița și rețeta inițială, dacă există opțiuni
        if (options.Count > 0 && recipes.Count > 0)
        {
            SetCraftingIcons(recipes[craftingDropdown.value].outputIcon);
            currentRecipe = recipes[craftingDropdown.value];
        }
    }


    public void SetDropdownSelectedOption(int index)
    {
        if (craftingDropdown != null)
        {
            craftingDropdown.value = index;
            craftingDropdown.RefreshShownValue();
        }
    }

    // Curăță dropdown-ul la finalul interacțiunii
    public void ClearCraftingDropdown()
    {
        if(craftingDropdown != null)
        {
            craftingDropdown.onValueChanged.RemoveAllListeners();
            craftingDropdown.ClearOptions();
        }
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

    public void SetCraftingIcons(Sprite icon)
    {
        if(craftingIcon != null)
        {
            craftingIcon.sprite = icon;
        }
    }
    #endregion

}
