using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class Ingredient
{
    public string name;
    public int ammount;
}

[CreateAssetMenu(fileName = "CraftingRecipe", menuName = "Custom/Crafting Recipe")]
public class CraftingInfoSO : ScriptableObject
{
    public string outputName;
    public int outputAmount;
    public Sprite outputIcon;
    public Ingredient[] inputs;
}
