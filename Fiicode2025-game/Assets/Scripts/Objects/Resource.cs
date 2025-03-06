using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class ResourceDrop
{
    [Tooltip("Numele item-ului (trebuie să existe în Inventory).")]
    public string itemName;

    [Tooltip("Cantitatea minimă care poate cădea.")]
    public int minAmount;

    [Tooltip("Cantitatea maximă care poate cădea.")]
    public int maxAmount;

    [Range(0f, 1f), Tooltip("Probabilitatea de a pica acest item (0 - 1).")]
    public float dropChance;
}

public class Resource : Interactable
{
    [Tooltip("Dacă este true, resursa va fi distrusă după minare.")]
    public bool destroyOnMine = false;
    
    // -----------------
    // Noul sistem de drops multiple cu probabilități
    // -----------------
    [Tooltip("Lista de iteme care pot fi obținute la minarea acestei resurse, cu probabilități individuale.")]
    public List<ResourceDrop> resourceDrops;
    public ResourceType type;
    [SerializeField] private GameObject scanningObj;
    [SerializeField] private string requiredDiscovery;

    /// <summary>
    /// Metoda de interacțiune a resursei.
    /// Dacă resursa nu a fost descoperită încă, se scanează (se deblochează).
    /// Dacă este deja descoperită, se inițiază minarea.
    /// </summary>
    /// <param name="player">Transformul jucătorului.</param>
    public override void Interact(Transform player)
    {
        Debug.Log("Sunt chemat");
        if (!PlayerScanInventory.instance.IsUnlocked(requiredDiscovery))
        {
            // Resursa nu a fost descoperită: efectuăm scanarea și o deblocăm.
            scanningObj.SetActive(true);
            ScanningManager.instance.StartScan(player, scanningObj.transform);
        }
        else
        {
            // Resursa este deja descoperită: inițiem minarea.
            MiningManager.instance.MineResource(this, player);
        }
    }

    public override void CompletedScanLogic()
    {
        scanningObj.SetActive(false);
        PlayerScanInventory.instance.Unlock(requiredDiscovery);
    }
}
