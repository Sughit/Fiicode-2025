using UnityEngine;

public class NewPlayerAttack : MonoBehaviour
{
    [Header("Cannons")]
    [SerializeField] private Transform[] cannonOrigins;

    [Header("GFX")]
    [SerializeField] private Transform gfx; // Partea vizuală a jucătorului

    [Header("Projectile Settings")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float projectileSpeed = 10f;
    [SerializeField] private float fireRate = 0.5f;

    [Header("Attack Settings")]
    [SerializeField] private float attackRadius = 5f; // Raza de atac

    private float nextFireTime;
    public Transform currentTarget;
    private bool isAttacking = false;

    void Update()
    {
        // Verificăm dacă cooldown-ul a expirat
        if (Time.time >= nextFireTime)
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, attackRadius);
            Transform nearestEnemy = null;
            float minDistance = Mathf.Infinity;

            // Căutăm cel mai apropiat inamic care este și vizibil
            foreach (Collider col in colliders)
            {
                if (col.CompareTag("Enemy"))
                {
                    if (IsEnemyVisible(col.transform))
                    {
                        float distance = Vector3.Distance(transform.position, col.transform.position);
                        if (distance < minDistance)
                        {
                            minDistance = distance;
                            nearestEnemy = col.transform;
                        }
                    }
                }
            }

            if (nearestEnemy != null)
            {
                currentTarget = nearestEnemy;
                Attack();
                nextFireTime = Time.time + fireRate;
                isAttacking = true;
            }
            else
            {
                currentTarget = null;
                isAttacking = false;
            }
        }

        // Dacă atacul este activ, forțăm gfx-ul să se uite către inamic
        if (currentTarget != null)
        {
            Vector3 directionToEnemy = (currentTarget.position - gfx.position).normalized;
            Quaternion targetRotation = Quaternion.LookRotation(directionToEnemy, transform.up);
            gfx.rotation = Quaternion.Slerp(gfx.rotation, targetRotation, Time.deltaTime * 10f);
        }
    }

    // Instanțiază proiectilele de la fiecare cannon
    void Attack()
    {
        foreach (Transform cannonOrigin in cannonOrigins)
        {
            if (cannonOrigin == null)
                continue;

            GameObject projectile = Instantiate(projectilePrefab, cannonOrigin.position, gfx.rotation);
            Projectile projScript = projectile.GetComponent<Projectile>();
            if (projScript != null)
            {
                projScript.senderGO = gameObject;
            }

            Rigidbody rb = projectile.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = gfx.forward * projectileSpeed;
            }
        }
    }

    // Verifică dacă inamicul este vizibil (nu este blocat de obstacole)
    bool IsEnemyVisible(Transform enemy)
    {
        Vector3 direction = (enemy.position - transform.position).normalized;
        RaycastHit hit;
        if (Physics.Raycast(transform.position, direction, out hit, attackRadius))
        {
            if (hit.transform == enemy)
            {
                return true;
            }
        }
        return false;
    }

    // Desenăm raza de atac în editor pentru vizualizare
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRadius);
    }
}
