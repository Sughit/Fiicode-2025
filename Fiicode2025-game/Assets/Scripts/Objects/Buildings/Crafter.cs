using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Crafter : Building
{
    [Header("Crafting lists")]
    [SerializeField] private List<string> options;
    [SerializeField] private List<CraftingInfoSO> craftingRecipes;

    // Variabilă pentru a păstra ultima rețetă selectată
    private int lastSelectedRecipeIndex = -1;

    public override void Interact()
    {
        // OBIGATORIU: apelează metoda de bază
        base.Interact();

        // Populează dropdown-ul cu opțiunile și rețetele disponibile
        CanvasManager.instance.SetupCrafterDropdown(options, craftingRecipes);

        // Dacă există o rețetă selectată anterior, setează dropdown-ul corespunzător
        if (lastSelectedRecipeIndex >= 0 && lastSelectedRecipeIndex < options.Count)
        {
            CanvasManager.instance.SetDropdownSelectedOption(lastSelectedRecipeIndex);
        }
    }

    // Metodă pentru actualizarea ultimei rețete selectate
    public void SetLastSelectedRecipe(int index)
    {
        if (index >= 0 && index < options.Count)
        {
            lastSelectedRecipeIndex = index;
        }
    }

    public override void EndInteraction()
    {
        base.EndInteraction();
        CanvasManager.instance.SetCraftingIcons(null);
        CanvasManager.instance.ClearCraftingDropdown();
    }

    void Start()
    {
        base.Start();
    }

    public void MakeItem(Ingredient[] inputs, string outputName, int outputAmount)
    {
        foreach (Ingredient input in inputs)
        {
            if (!PlayerInventory.instance.CanRemoveItem(input.name, input.ammount))
            {
                Debug.Log("Missing ingredient: " + input.name);
                return;
            }
        }

        foreach (Ingredient input in inputs)
        {
            PlayerInventory.instance.RemoveItem(input.name, input.ammount);
        }

        PlayerInventory.instance.AddItem(outputName, outputAmount);
        Debug.Log("Crafted: " + outputName);
    }
}
