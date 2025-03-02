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
        instance = this;
    }

    /// <summary>
    /// Inițiază procesul de minare asupra resursei, creând un laser de la jucător la resursă.
    /// </summary>
    /// <param name="resource">Resursa de minat.</param>
    /// <param name="player">Transformul jucătorului.</param>
    public void MineResource(Resource resource, Transform player)
    {
        // Creăm efectul laser.
        GameObject laser = Instantiate(laserPrefab);
        LineRenderer lr = laser.GetComponent<LineRenderer>();
        if (lr != null)
        {
            lr.SetPosition(0, player.position);
            lr.SetPosition(1, resource.transform.position);
        }
        StartCoroutine(MineCoroutine(resource, player, laser));
    }

    private IEnumerator MineCoroutine(Resource resource, Transform player, GameObject laser)
    {
        // Instanțiem particulele din timpul minării, dacă resursa se va distruge și avem prefab-ul setat.
        GameObject miningParticleEffect = null;
        if (resource.destroyOnMine && miningParticlePrefab != null)
        {
            miningParticleEffect = Instantiate(miningParticlePrefab, resource.transform.position, Quaternion.identity, resource.transform);
        }

        // Folosim transformul resursei pentru a aplica efectul de shake.
        Transform minedTransform = resource.transform;
        Vector3 originalPos = minedTransform.position;

        float timer = 0f;
        bool interrupted = false;
        while (timer < miningDuration)
        {
            timer += Time.deltaTime;
            
            // Actualizează pozițiile laserului dacă jucătorul sau obiectul minat se mișcă.
            if (laser != null)
            {
                LineRenderer lr = laser.GetComponent<LineRenderer>();
                if (lr != null)
                {
                    lr.SetPosition(0, player.position);
                    lr.SetPosition(1, minedTransform.position);
                }
            }

            // Aplicăm efectul de shake pe transformul minat.
            if (resource.destroyOnMine)
            {
                Vector3 shakeOffset = Random.insideUnitSphere * shakeIntensity;
                minedTransform.position = originalPos + shakeOffset;
            }

            // Verificăm dacă jucătorul s-a îndepărtat prea mult (doar maxim, fără nicio distanță minimă).
            if (Vector3.Distance(player.position, minedTransform.position) > maxMiningDistance)
            {
                Debug.Log("Minare întreruptă: jucătorul s-a îndepărtat prea mult.");
                interrupted = true;
                break;
            }
            
            yield return null;
        }
        
        // Dacă minatul a fost întrerupt, restaurăm poziția și distrugem efectele.
        if (interrupted)
        {
            minedTransform.position = originalPos;
            Destroy(laser);
            if (miningParticleEffect != null)
            {
                Destroy(miningParticleEffect);
            }
            yield break;
        }
        
        // Restaurăm poziția originală dacă obiectul nu e distrus.
        if (!resource.destroyOnMine)
        {
            minedTransform.position = originalPos;
        }

        // Distrugem efectul laser.
        Destroy(laser);
        Debug.Log("Minare finalizată.");
        
        // Dacă resursa trebuie distrusă:
        if (resource.destroyOnMine)
        {
            // Spawnează efectul final de particule, dacă este setat.
            if (finalParticlePrefab != null)
            {
                Instantiate(finalParticlePrefab, minedTransform.position, Quaternion.identity);
            }
            // Așteptăm puțin pentru a permite efectului final să fie vizibil.
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
}
