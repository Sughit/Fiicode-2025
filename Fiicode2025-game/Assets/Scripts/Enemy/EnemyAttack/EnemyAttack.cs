using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    public Transform[] cannonOrigins;      // Punctele de lansare a proiectilelor
    public Transform enemyGfx;             // Folosit pentru orientarea proiectilului
    public GameObject projectilePrefab;    // Prefab-ul proiectilului
    public float projectileSpeed = 10f;      // Viteza proiectilului
    public float fireRate = 2f;            // Intervalul minim între atacuri (cooldown)

    private float nextFireTime = 0f;
    private EnemyController enemyController;

    void Awake()
    {
        enemyController = GetComponent<EnemyController>();
    }

    void Update()
    {
        // Atacă doar dacă player-ul este activ detectat și se află în raza de atac,
        // evitând astfel atacul atunci când inamicul stă pe loc la ultima poziție cunoscută.
        if (enemyController.player != null &&
            enemyController.IsTargetInAttackRange() &&
            Time.time >= nextFireTime)
        {
            nextFireTime = Time.time + fireRate;
            PerformAttack();
        }
    }

    // Instanțiază proiectile din fiecare cannonOrigin și setează viteza lor
    public void PerformAttack()
    {
        foreach (Transform cannonOrigin in cannonOrigins)
        {
            if (cannonOrigin == null)
                continue;

            GameObject projectile = Instantiate(projectilePrefab, cannonOrigin.position, enemyGfx.rotation);
            projectile.GetComponent<Projectile>().senderGO = gameObject;
            Rigidbody rbProjectile = projectile.GetComponent<Rigidbody>();
            if (rbProjectile != null)
            {
                rbProjectile.linearVelocity = enemyGfx.forward * projectileSpeed;
            }
        }
    }

    void OnDrawGizmos()
    {
        if (cannonOrigins == null)
            return;

        Gizmos.color = Color.red;
        foreach (Transform cannon in cannonOrigins)
        {
            if (cannon != null)
                Gizmos.DrawWireSphere(cannon.position, 0.5f);
        }
    }
}
