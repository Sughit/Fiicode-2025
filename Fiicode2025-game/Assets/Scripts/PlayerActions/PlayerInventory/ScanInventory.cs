using UnityEngine;

[CreateAssetMenu(fileName = "Scan Inventory", menuName = "Custom/Scan Inventory")]
public class ScanInventory : ScriptableObject
{
    // Resurse naturale
    public bool iron;
    public bool gold;
    public bool coal;
    public bool wood;
    public bool stone;
    public bool water;
    public bool petrolium;
    public bool clay;
    public bool brick;
    public bool copper;

    // Cladiri
    public bool sMine1;
    public bool sMine2;
    public bool fMine1;
    public bool greenHouse;
    public bool sStorage1;
    public bool sStorage2;
    public bool sStorage3;
    public bool sStorage4;
    public bool fStorage1;
    public bool fStorage2;
    public bool weapon1;
    public bool weapon2;
    public bool comm1;
    public bool comm2;
    public bool comm3;
    public bool comm4;
}
