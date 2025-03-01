using UnityEngine;

[CreateAssetMenu(fileName = "Scan Inventory", menuName = "Custom/Scan Inventory")]
public class ScanInventory : ScriptableObject
{
    public bool iron;
    public bool gold;
    public bool coal;
    public bool wood;
    public bool stone;
    public bool water;
    public bool petrolium;
    public bool clay;
    public bool copper;
}
