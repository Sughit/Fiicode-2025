using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Crafter : Building
{
    public enum CrafterType
    {
        PlantCrafting,
        FoodCrafting,
        MetalCrafting,
        MiscCrafting,
        LifeCrafting
    }

    [Header("Crafting lists")]
    [SerializeField] private List<string> plantCraftingList;
    [SerializeField] private List<string> foodCraftingList;
    [SerializeField] private List<string> metalCraftingList;
    [SerializeField] private List<string> miscCraftingList;
    [SerializeField] private List<string> lifeCraftingList;
    public Sprite iconInput, iconOutput;

    public override void Interact()
    {
        //OBLIGATORIU
        base.Interact();
        SetCraftingIcons(iconInput, iconOutput);
    }

    public override void EndInteraction()
    {
        //OBLIGATORIU
        base.EndInteraction();
        SetCraftingIcons(null, null);
    }

    void Start()
    {
        base.Start();
    }
}
