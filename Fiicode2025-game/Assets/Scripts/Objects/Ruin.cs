using UnityEngine;

public class Ruin : Interactable
{
    [SerializeField] private GameObject scanningObj;
    public override void Interact(Transform player)
    {
        scanningObj.SetActive(true);
        ScanningManager.instance.StartScan(player, scanningObj.transform);
    }
}
