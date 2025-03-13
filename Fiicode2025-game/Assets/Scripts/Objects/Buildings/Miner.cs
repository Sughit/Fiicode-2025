using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class Miner : Building
{
    [Header("Parameters")]
    [Tooltip("Intervalul de timp (secunde) între acțiunile de minare.")]
    public float miningInterval = 2f;
    [Tooltip("Raza în care se caută resurse.")]
    public float miningRange = 1f;
    [Tooltip("Capacitatea maximă a inventarului (numărul de iteme).")]
    public int maxInventoryCapacity = 100;
    [Tooltip("Numele resursei ce va fi adăugată în inventarul playerului. (Opțional: dacă nu e setat, se va folosi numele primului drop al resursei țintă)")]
    public string resourceItemName;
    public Sprite icon;
    public bool solidMine = true;

    public int currentInventoryCount = 0; // numărul curent de iteme în inventar
    private Resource targetResource;        // resursa țintă pe care o minăm
    private Coroutine miningCoroutine;      // referință la rutina de minare

    private void Start()
    {
        base.Start(); 
        targetResource = FindResourceInRange();
        if (targetResource == null)
        {
            Debug.LogWarning("Nu s-a găsit nicio resursă în raza de căutare.");
        }
        miningCoroutine = StartCoroutine(MiningRoutine());
        icon = IconManager.instance.GetResourceIcon(targetResource.type.ToString());
    }

    public override void Interact()
    {
        // OBLIGATORIU
        base.Interact();
        CanvasManager.instance.SetMiningIcons(icon);
        if (miningCoroutine == null)
        {
            miningCoroutine = StartCoroutine(MiningRoutine());
        }
    }

    public override void EndInteraction()
    {
        // OBLIGATORIU
        base.EndInteraction();
        CanvasManager.instance.SetMiningIcons(null);
    }

    // Rutină care la fiecare interval minează resursa găsită, dacă inventarul nu e plin
    private IEnumerator MiningRoutine()
    {
        while (true)
        {
            if (currentInventoryCount >= maxInventoryCapacity)
            {
                // Inventarul e plin; așteptăm până se golește
                yield return null;
            }
            else
            {
                yield return new WaitForSeconds(miningInterval);
                if (targetResource != null)
                {
                    int minedAmount = Mine(targetResource);
                    currentInventoryCount += minedAmount;
                    Debug.Log("S-au minat " + minedAmount + " iteme. Inventar: " + currentInventoryCount + "/" + maxInventoryCapacity);

                    // Dacă resursa trebuie distrusă după minare, o distrugem și oprim minarea
                    if (targetResource.destroyOnMine)
                    {
                        Destroy(targetResource.gameObject);
                        targetResource = null;
                        Debug.Log("Resursa a fost distrusă după minare.");
                        StopCoroutine(miningCoroutine);
                        miningCoroutine = null;
                    }
                }
            }
            yield return null;
        }
    }

    // Caută o resursă în raza definită folosind un OverlapSphere
    private Resource FindResourceInRange()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, miningRange);
        foreach (Collider col in colliders)
        {
            Resource res = col.GetComponent<Resource>();
            if (res != null)
            {
                if(solidMine)
                {
                    if(res.gameObject.tag == "SolidResource")
                    {
                        return res;
                    }
                }
                else
                {
                    if(res.gameObject.tag == "FluidResource")
                    {
                        return res;
                    }
                }
            }
        }
        return null;
    }

    // Simulează procesul de minare, folosind lista de drop-uri din resursă
    private int Mine(Resource resource)
    {
        int totalMined = 0;
        foreach (ResourceDrop drop in resource.resourceDrops)
        {
            if (Random.value <= drop.dropChance)
            {
                // Random.Range are capătul superior exclus, de aceea adăugăm 1
                int amount = Random.Range(drop.minAmount, drop.maxAmount + 1);
                totalMined += amount;
            }
        }
        if (totalMined <= 0)
            totalMined = 1;

        int availableSpace = maxInventoryCapacity - currentInventoryCount;
        int mined = Mathf.Min(totalMined, availableSpace);
        return mined;
    }

    // Noua funcție: Colectează resursele din mina curentă, adăugându-le în inventarul jucătorului
    // Această funcție poate fi apelată de un buton din UI
    public void CollectResources()
    {
        // Determinăm numele item-ului care va fi adăugat în inventarul playerului
        string itemNameToAdd = resourceItemName;
        if (string.IsNullOrEmpty(itemNameToAdd))
        {
            if (targetResource != null && targetResource.resourceDrops != null && targetResource.resourceDrops.Count > 0)
            {
                itemNameToAdd = targetResource.resourceDrops[0].itemName;
            }
            else
            {
                Debug.LogWarning("Nu s-a putut determina numele item-ului pentru colectare.");
                return;
            }
        }

        PlayerInventory.instance.AddItem(itemNameToAdd, currentInventoryCount);

        Debug.Log("S-au colectat " + currentInventoryCount + " " + itemNameToAdd + " din mina în inventarul playerului.");

        // Golește inventarul minei
        EmptyInventory();
    }

    // Metodă existentă pentru a goli inventarul minei (poate fi folosită și independent)
    public void EmptyInventory()
    {
        currentInventoryCount = 0;
        Debug.Log("Inventarul a fost golit.");
    }

    // Pentru vizualizarea razei de minare în editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, miningRange);
    }
}
