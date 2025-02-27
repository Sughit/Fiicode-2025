using UnityEngine;

public class Ruin : Interactable
{
    [SerializeField] private GameObject scanningObj;
    [SerializeField] private string discovery;

    public override void Interact(Transform player)
    {
        scanningObj.SetActive(true);
        ScanningManager.instance.StartScan(player, scanningObj.transform);
    }

    public override void CompletedScanLogic()
    {
        Debug.Log($"Ruin scanned {gameObject.name}");
        gameObject.layer = LayerMask.NameToLayer("Default");
        GetComponent<Outline>().enabled = false;
        if(discovery != null && discovery != "") PlayerScanInventory.instance.Unlock(discovery);
        else Debug.Log("There is no discovery!");
    }
}
