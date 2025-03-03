using UnityEngine;

public class Weapon : Building
{
    [Header("Attack Settings")]
    [Tooltip("Prefab-ul proiectilului care va fi lansat spre inamic.")]
    [SerializeField] private GameObject projectilePrefab;

    [Tooltip("Punctele din care ies proiectilele (multiple).")]
    [SerializeField] private Transform[] firePoints;

    [Tooltip("Timpul (secunde) dintre două atacuri consecutive.")]
    [SerializeField] private float attackRate = 1f;
    private float nextAttackTime = 0f;

    [Header("Detection Settings")]
    [Tooltip("Raza de detecție a inamicilor (folosită de OverlapSphere).")]
    [SerializeField] private float detectionRadius = 5f;

    [Tooltip("LayerMask pentru a filtra obiectele considerate inamici.")]
    [SerializeField] private LayerMask enemyLayer;

    [Header("Rotation")]
    [Tooltip("Viteza cu care arma se rotește spre inamic (Slerp).")]
    [SerializeField] private float rotationSpeed = 5f;

    // Buffer pentru OverlapSphereNonAlloc
    private Collider[] overlapResults = new Collider[5];

    public override void Interact()
    {
        // OBLIGATORIU: logica din clasa părinte (Building)
        base.Interact();
    }

    private void Update()
    {
        // 1) Verificăm dacă există inamici în raza de detectare (la fiecare frame)
        int count = Physics.OverlapSphereNonAlloc(
            transform.position,
            detectionRadius,
            overlapResults,
            enemyLayer
        );

        if (count > 0)
        {
            // 2) Aflăm inamicul cel mai apropiat
            GameObject closestEnemy = GetClosestEnemy(count);
            if (closestEnemy != null)
            {
                // 3) Rotim arma spre inamic, dar doar pe axa Y
                RotateTowardsTargetOnY(closestEnemy);

                // 4) Dacă e timpul să tragem, lansăm proiectilele
                if (Time.time >= nextAttackTime)
                {
                    FireAllProjectilesAtEnemy(closestEnemy);
                    nextAttackTime = Time.time + attackRate;
                }
            }
        }
        // Dacă count == 0, nu facem nimic special (rămâne la ultima rotație)
    }

    private GameObject GetClosestEnemy(int detectedCount)
    {
        float closestDist = Mathf.Infinity;
        GameObject closestEnemy = null;

        for (int i = 0; i < detectedCount; i++)
        {
            if (overlapResults[i] == null) 
                continue;

            float dist = Vector3.Distance(transform.position, overlapResults[i].transform.position);

            if (dist < closestDist)
            {
                closestDist = dist;
                closestEnemy = overlapResults[i].gameObject;
            }
        }

        return closestEnemy;
    }

    /// <summary>
    /// Rotește arma doar pe axa Y, pentru a privi spre inamic.
    /// </summary>
    private void RotateTowardsTargetOnY(GameObject enemy)
    {
        // Direcția din arma curentă spre inamic
        Vector3 direction = enemy.transform.position - transform.position;

        // Anulăm componenta pe Y, ca să rotim doar în plan orizontal
        direction.y = 0f;

        // Verificăm să nu fie zero vector (dacă suntem exact deasupra sau sub inamic)
        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                Time.deltaTime * rotationSpeed
            );
        }
    }

    private void FireAllProjectilesAtEnemy(GameObject enemy)
    {
        if (projectilePrefab == null || firePoints == null || enemy == null) return;

        foreach (Transform fp in firePoints)
        {
            if (fp == null) continue; 
            GameObject projectile = Instantiate(projectilePrefab, fp.position, fp.rotation);

            // Dacă proiectilul are un script "Projectile" cu un SetTarget(GameObject)
            // projectile.GetComponent<Projectile>()?.SetTarget(enemy);

            // Efecte suplimentare, particule, etc.
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
