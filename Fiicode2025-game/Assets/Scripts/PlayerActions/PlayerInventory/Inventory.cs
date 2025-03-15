using UnityEngine;

[CreateAssetMenu(fileName = "Inventory", menuName = "Custom/Inventory")]
public class Inventory : ScriptableObject
{
    [Header("Resources")]
    public int wood;
    public int stone;
    public int iron;
    public int gold;
    public int silver;
    public int coal;
    public int clay;
    public int copper;

    [Header("Seeds")]
    public int sappling;
    public int vegetableSeed;
    public int fruitSeed;

    [Header("Processed Resources")]
    public int ironBar;
    public int steel;
    public int goldBar;
    public int silverBar;
    public int copperWire;
    public int cuttedStone;
    public int brick;
    public int oilBarrel;
    
}
