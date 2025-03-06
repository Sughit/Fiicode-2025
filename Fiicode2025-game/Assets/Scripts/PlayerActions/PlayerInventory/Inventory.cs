using UnityEngine;

[CreateAssetMenu(fileName = "Inventory", menuName = "Custom/Inventory")]
public class Inventory : ScriptableObject
{
    // Raw resources
    public int wood;
    public int stone;
    public int iron;
    public int gold;
    public int silver;
    public int coal;
    public int clay;
    public int copper;

    // Seeds
    public int sappling;
    public int vegetableSeed;
    public int fruitSeed;

    // Processed resources
    public int brick;
}
