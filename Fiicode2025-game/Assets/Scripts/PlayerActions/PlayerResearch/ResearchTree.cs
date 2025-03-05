using UnityEngine;

[System.Serializable]
public class BuildingButton
{
    public string buildingName;
    public GameObject buildingButton;
}

public class ResearchTree : MonoBehaviour
{
    [SerializeField] private BuildingButton[] buildings;
    public static ResearchTree instance;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(this);
    }

    public void CheckAll()
    {
        foreach(BuildingButton building in buildings)
        {
            CheckIfUnlocked(building);
        }
    }

    void CheckIfUnlocked(BuildingButton building)
    {   
        if(PlayerScanInventory.instance.IsUnlocked(building.buildingName))
        {
            building.buildingButton.SetActive(true);
        }
    }
}
