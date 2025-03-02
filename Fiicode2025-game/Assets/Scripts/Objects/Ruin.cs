using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class RuinDiscoveryChance
{
    [Tooltip("Numele câmpului bool din ScanInventory (ex: 'iron').")]
    public string itemName;

    [Tooltip("Probabilitatea (greutatea) de bază, ex: 10, 50, etc.")]
    public float chance;
}

public class Ruin : Interactable
{
    [SerializeField] private GameObject scanningObj;

    // Aici listezi descoperirile posibile pentru această Ruină,
    // fiecare având un 'itemName' (trebuie să corespundă cu câmpul bool din ScanInventory)
    // și o 'chance' (șansă de bază).
    [Header("Posibile descoperiri pentru această ruină")]
    public List<RuinDiscoveryChance> possibleDiscoveries = new List<RuinDiscoveryChance>();

    public override void Interact(Transform player)
    {
        if (scanningObj != null)
            scanningObj.SetActive(true);

        ScanningManager.instance.StartScan(player, scanningObj != null ? scanningObj.transform : null);
    }

    public override void CompletedScanLogic()
    {
        Debug.Log($"Ruin scanned {gameObject.name}");

        // Dezactivăm interactivitatea, Outline, etc.
        gameObject.layer = LayerMask.NameToLayer("Default");
        var outline = GetComponent<Outline>();
        if (outline != null) outline.enabled = false;

        // Încercăm să obținem O descoperire nouă (cu Weighted Random) din 'possibleDiscoveries'
        string discovered = GetNewDiscoveryFromRuin();

        if (!string.IsNullOrEmpty(discovered))
        {
            // Marchez în Inventory
            PlayerScanInventory.instance.Unlock(discovered);
            Debug.Log($"Ruin => discovered: {discovered}");
        }
        else
        {
            Debug.Log("Ruin => nimic de descoperit sau toate erau deja descoperite.");
        }
    }

    /// <summary>
    /// Alege UN item (itemName) din possibleDiscoveries care nu e descoperit încă,
    /// cu Weighted Random pe 'chance'. 
    /// - Dacă rămâne unul singur nedescoperit, îl iei cu 100%.
    /// - Dacă niciunul nu e disponibil, returnează string gol/null.
    /// - Altfel, face Weighted Random.
    /// </summary>
    private string GetNewDiscoveryFromRuin()
    {
        // Construim o listă locală cu itemele NEDESCOPERITE
        List<RuinDiscoveryChance> undiscovered = new List<RuinDiscoveryChance>();

        // Verificăm în inventory dacă e deja descoperit
        foreach (var dc in possibleDiscoveries)
        {
            if (!PlayerScanInventory.instance.IsUnlocked(dc.itemName))
            {
                undiscovered.Add(dc);
            }
        }

        // Dacă nu e nimic nedescoperit, returnăm null
        if (undiscovered.Count == 0)
        {
            return null;
        }

        // Dacă e doar unul, îl returnăm direct (100%)
        if (undiscovered.Count == 1)
        {
            return undiscovered[0].itemName;
        }

        // Altfel Weighted Random
        float totalChance = 0f;
        foreach (var dc in undiscovered)
        {
            totalChance += dc.chance;
        }

        float randValue = Random.Range(0f, totalChance);
        float cumulative = 0f;

        foreach (var dc in undiscovered)
        {
            cumulative += dc.chance;
            if (randValue <= cumulative)
            {
                return dc.itemName; 
            }
        }

        // fallback (nu ar trebui să ajungem aici)
        return undiscovered[undiscovered.Count - 1].itemName;
    }
}
