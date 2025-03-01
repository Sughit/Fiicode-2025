using UnityEngine;
using System.Reflection;

public class Resource : Interactable
{
    [Tooltip("Dacă este true, resursa va fi distrusă după minare.")]
    public bool destroyOnMine = false;
    
    [Tooltip("Numele resursei, care trebuie să corespundă cu numele câmpului din ScanInventory (ex: \"ironIngot\").")]
    [SerializeField] private string resourceName;
    [SerializeField] private GameObject scanningObj;

    /// <summary>
    /// Metoda de interacțiune a resursei.
    /// Dacă resursa nu a fost descoperită încă, se scanează (se deblochează).
    /// Dacă este deja descoperită, se inițiază minarea.
    /// </summary>
    /// <param name="player">Transformul jucătorului.</param>
    public override void Interact(Transform player)
    {
        Debug.Log("Sunt chemat");
        if (!IsResourceDiscovered())
        {
            // Resursa nu a fost descoperită: efectuăm scanarea și o deblocăm.
            scanningObj.SetActive(true);
            ScanningManager.instance.StartScan(player, scanningObj.transform);
        }
        else
        {
            // Resursa este deja descoperită: inițiem minarea.
            MiningManager.instance.MineResource(this, player);
            Debug.Log($"Resursa '{resourceName}' a fost scanată. Acum o poți mina.");
        }
    }

    public override void CompletedScanLogic()
    {
        scanningObj.SetActive(false);
        PlayerScanInventory.instance.Unlock(resourceName);
    }

    /// <summary>
    /// Verifică dacă resursa a fost descoperită în inventarul de scanare al jucătorului.
    /// Se folosește reflection pentru a accesa câmpul din ScanInventory.
    /// </summary>
    /// <returns>True dacă resursa este descoperită, altfel false.</returns>
    private bool IsResourceDiscovered()
    {
        var inventory = PlayerScanInventory.instance.inventory;
        System.Type type = inventory.GetType();
        FieldInfo field = type.GetField(resourceName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        if (field != null && field.FieldType == typeof(bool))
        {
            return (bool)field.GetValue(inventory);
        }
        return false;
    }
}
