using UnityEngine;
using System.Collections;

public class MiningManager : MonoBehaviour
{
    public static MiningManager instance;

    [Tooltip("Prefab-ul care conține un LineRenderer pentru efectul laser.")]
    [SerializeField] private GameObject laserPrefab;

    [Tooltip("Prefab-ul particulelor care se afișează în timpul minării (opțional).")]
    [SerializeField] private GameObject miningParticlePrefab;

    [Tooltip("Prefab-ul particulelor care se afișează la finalul minării (opțional).")]
    [SerializeField] private GameObject finalParticlePrefab;

    [Tooltip("Durata de minare (secunde).")]
    [SerializeField] private float miningDuration = 2.0f;

    [Tooltip("Intensitatea efectului de shake aplicat transformului minat în timpul minării.")]
    [SerializeField] private float shakeIntensity = 0.01f;

    [Tooltip("Timpul (secunde) înainte de distrugerea resursei, în care apare efectul final de particule.")]
    [SerializeField] private float finalEffectDelay = 0.15f;

    [Tooltip("Distanța maximă la care jucătorul poate fi de resursa minată pentru a continua minatul.")]
    [SerializeField] private float maxMiningDistance = 5f;

    // Flag pentru a evita minarea simultană a mai multor resurse
    private bool isMining = false;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(this);
    }

    public void MineResource(Resource resource, Transform player)
    {
        if (isMining)
        {
            Debug.Log("Deja se minează o resursă. Așteaptă finalizarea acesteia.");
            return;
        }

        isMining = true;
        StartCoroutine(MineLoop(resource, player));
    }

    private IEnumerator MineLoop(Resource resource, Transform player)
    {
        if (resource == null)
        {
            Debug.LogWarning("Resursa de minat este null!");
            isMining = false;
            yield break;
        }

        yield return StartCoroutine(MineCoroutine(resource, player));

        isMining = false;
    }

    private IEnumerator MineCoroutine(Resource resource, Transform player)
    {
        // Creăm efectul laser.
        GameObject laser = Instantiate(laserPrefab);
        LineRenderer lr = laser.GetComponent<LineRenderer>();
        if (lr != null)
        {
            lr.SetPosition(0, player.position);
            lr.SetPosition(1, resource.transform.position);
        }

        // Instanțiem particulele din timpul minării, dacă există.
        GameObject miningParticleEffect = null;
        if (miningParticlePrefab != null)
        {
            miningParticleEffect = Instantiate(miningParticlePrefab, resource.transform.position, Quaternion.identity, resource.transform);
        }

        Transform minedTransform = resource.transform;
        Vector3 originalPos = minedTransform.position;

        float timer = 0f;
        while (timer < miningDuration)
        {
            timer += Time.deltaTime;

            // Actualizează pozițiile laserului
            if (laser != null && lr != null)
            {
                lr.SetPosition(0, player.position);
                lr.SetPosition(1, minedTransform.position);
            }

            // Aplicăm efectul de shake
            Vector3 shakeOffset = Random.insideUnitSphere * shakeIntensity;
            minedTransform.position = originalPos + shakeOffset;

            // Verificăm distanța
            if (Vector3.Distance(player.position, minedTransform.position) > maxMiningDistance)
            {
                Debug.Log("Minare întreruptă: jucătorul s-a îndepărtat prea mult.");
                Destroy(laser);
                if (miningParticleEffect != null) Destroy(miningParticleEffect);
                yield break;
            }

            yield return null;
        }

        // Restaurăm poziția
        minedTransform.position = originalPos;

        // Distrugem efectul laser.
        Destroy(laser);

        // Adăugăm itemele în inventar (cu probabilități)
        AddResourceToInventory(resource);

        // Dacă resursa trebuie distrusă
        if (resource.destroyOnMine)
        {
            if (finalParticlePrefab != null)
            {
                Instantiate(finalParticlePrefab, minedTransform.position, Quaternion.identity);
            }
            yield return new WaitForSeconds(finalEffectDelay);
            Destroy(resource.gameObject);
        }
        else
        {
            // Dacă nu se distruge resursa, distrugem particulele de minare (dacă există)
            if (miningParticleEffect != null)
            {
                Destroy(miningParticleEffect);
            }
        }
    }

    /// <summary>
    /// Adaugă itemele din resource.resourceDrops în inventarul jucătorului cu probabilitățile configurate.
    /// </summary>
    /// <param name="resource">Resursa minată.</param>
    private void AddResourceToInventory(Resource resource)
    {
        if (PlayerInventory.instance == null)
        {
            Debug.LogError("Inventory system not found!");
            return;
        }

        // Parcurgem toate drop-urile definite în Resource
        if (resource.resourceDrops != null && resource.resourceDrops.Count > 0)
        {
            foreach (var drop in resource.resourceDrops)
            {
                // Verificăm probabilitatea
                float randVal = Random.value; // un număr între 0 și 1
                if (randVal <= drop.dropChance)
                {
                    int amount = Random.Range(drop.minAmount, drop.maxAmount + 1);
                    // Adăugăm în inventar
                    PlayerInventory.instance.AddItem(drop.itemName, amount);

                    Debug.Log($"Added {amount} {drop.itemName} to inventory. (Probability {drop.dropChance})");
                }
            }
        }
        else
        {
            Debug.LogWarning("resourceDrops este gol. Nicio resursă nu a fost definită pentru acest obiect.");
        }
    }
}
