using UnityEngine;
using System.Collections;

public class MiningManager : MonoBehaviour
{
    public static MiningManager instance;

    [Tooltip("Prefab-ul care conține un LineRenderer pentru efectul laser.")]
    [SerializeField] private GameObject laserPrefab;
    
    [Tooltip("Durata de minare (secunde).")]
    [SerializeField] private float miningDuration = 2.0f;

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
        float timer = 0f;
        while (timer < miningDuration)
        {
            timer += Time.deltaTime;
            // Actualizează pozițiile laserului în cazul în care jucătorul sau resursa se mișcă.
            if (laser != null)
            {
                LineRenderer lr = laser.GetComponent<LineRenderer>();
                if (lr != null)
                {
                    lr.SetPosition(0, player.position);
                    lr.SetPosition(1, resource.transform.position);
                }
            }
            yield return null;
        }
        // Finalul procesului de minare.
        Destroy(laser);
        Debug.Log("Minare finalizată.");
        
        // Dacă resursa trebuie distrusă după minare, o distrugem.
        if (resource.destroyOnMine)
        {
            Destroy(resource.gameObject);
        }
    }
}
