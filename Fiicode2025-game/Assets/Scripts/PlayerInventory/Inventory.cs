using UnityEngine;

[CreateAssetMenu(fileName = "Inventory", menuName = "Custom/Inventory")]
public class Inventory : ScriptableObject
{
    public int wood;
    public int stone;
    public int gold;
    public int silver;
}
