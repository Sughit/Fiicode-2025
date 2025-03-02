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

    private void Awake()
    {
        if(instance == null) instance = this;
        else Destroy(this);
    }

    /// <summary>
    /// Inițiază procesul de minare asupra resursei, creând un laser de la jucător la resursă.
    /// </summary>
    /// <param name="resource">Resursa de minat.</param>
    /// <param name="player">Transformul jucătorului.</param>
    public void MineResource(Resource resource, Transform player)
    {
        StartCoroutine(MineLoop(resource, player));
    }

    private IEnumerator MineLoop(Resource resource, Transform player)
    {
        while (true) // Buclă infinită până când resursa este distrusă sau jucătorul pleacă
        {
            // Dacă jucătorul se îndepărtează, minarea se întrerupe
            if (Vector3.Distance(player.position, resource.transform.position) > maxMiningDistance)
            {
                Debug.Log("Minare întreruptă: jucătorul s-a îndepărtat prea mult.");
                yield break;
            }

            yield return StartCoroutine(MineCoroutine(resource, player));

            // Dacă resursa este distrusă după minare, oprim bucla
            if (resource == null || resource.destroyOnMine)
            {
                yield break;
            }
        }
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

            // Actualizează pozițiile laserului dacă jucătorul sau obiectul minat se mișcă.
            if (laser != null)
            {
                if (lr != null)
                {
                    lr.SetPosition(0, player.position);
                    lr.SetPosition(1, minedTransform.position);
                }
            }

            // Aplicăm efectul de shake pe transformul minat.
            Vector3 shakeOffset = Random.insideUnitSphere * shakeIntensity;
            minedTransform.position = originalPos + shakeOffset;

            // Verificăm dacă jucătorul s-a îndepărtat prea mult.
            if (Vector3.Distance(player.position, minedTransform.position) > maxMiningDistance)
            {
                Debug.Log("Minare întreruptă: jucătorul s-a îndepărtat prea mult.");
                Destroy(laser);
                if (miningParticleEffect != null) Destroy(miningParticleEffect);
                yield break;
            }

            yield return null;
        }

        // Restaurăm poziția obiectului
        minedTransform.position = originalPos;

        // Distrugem efectul laser.
        Destroy(laser);

        // Adăugăm resursa în inventar.
        AddResourceToInventory(resource);

        // Dacă resursa trebuie distrusă:
        if (resource.destroyOnMine)
        {
            // Spawnează efectul final de particule, dacă este setat.
            if (finalParticlePrefab != null)
            {
                Instantiate(finalParticlePrefab, minedTransform.position, Quaternion.identity);
            }

            // Așteptăm puțin pentru efectul final
            yield return new WaitForSeconds(finalEffectDelay);

            Destroy(resource.gameObject);
        }
        else
        {
            // Dacă nu se distruge resursa, distrugem particulele din timpul minării (dacă există).
            if (miningParticleEffect != null)
            {
                Destroy(miningParticleEffect);
            }
        }
    }

    /// <summary>
    /// Adaugă resursa minată în inventarul jucătorului.
    /// </summary>
    /// <param name="resource">Obiectul de resursă minat.</param>
    private void AddResourceToInventory(Resource resource)
    {
        if (PlayerInventory.instance == null)
        {
            Debug.LogError("Inventory system not found!");
            return;
        }

        if (int.TryParse(resource.resourceAmount, out int amount))
        {
            PlayerInventory.instance.AddItem(resource.resourceName, amount);
            Debug.Log($"Added {amount} {resource.resourceName} to inventory.");
        }
        else
        {
            Debug.LogError($"Invalid resource amount: {resource.resourceAmount}");
        }
    }
}
