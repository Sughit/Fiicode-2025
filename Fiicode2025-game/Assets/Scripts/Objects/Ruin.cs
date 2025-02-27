using UnityEngine;

public class Ruin : Interactable
{
    [SerializeField] private GameObject scanningObj;

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
    }
}
