using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Crafter : Building
{
    [Header("Crafting lists")]
    [SerializeField] private List<string> options;
    [SerializeField] private List<CraftingInfoSO> craftingRecipes;

    public override void Interact()
    {
        // OBIGATORIU: apelează metoda de bază
        base.Interact();
        // Populează dropdown-ul din CanvasManager cu opțiuni și referințe la CraftingInfoSO
        CanvasManager.instance.SetupCrafterDropdown(options, craftingRecipes);
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
        foreach(Ingredient input in inputs)
        {
            if(!PlayerInventory.instance.CanRemoveItem(input.name, input.ammount))
            {
                Debug.Log("Missing ingredient: " + input.name);
                return;
            }
        }

        foreach(Ingredient input in inputs)
        {
            PlayerInventory.instance.RemoveItem(input.name, input.ammount);
        }

        PlayerInventory.instance.AddItem(outputName, outputAmount);
        NotificationManager.instance.ShowNotification($"Added {outputAmount} {outputName} to inventory.");
        Debug.Log("Crafted: " + outputName);
    }
}
