using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    [Header("References")]
    public Transform planet;   // Centrul planetei
    public Transform gfx;      // Partea vizuală a inamicului

    [Header("Movement Settings")]
    public float gravityStrength = 10f;
    public float moveSpeed = 4f;
    public float rotationSpeed = 10f;

    private Rigidbody rb;
    private EnemyController enemyController;
    private Vector3 latestMoveDirection;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false; // Gravitația va fi calculată manual
        enemyController = GetComponent<EnemyController>();

        planet = GameObject.FindGameObjectWithTag("Planet").transform;
    }

    void FixedUpdate()
    {
        ApplyGravity();
        HandleMovement();
    }

    private void ApplyGravity()
    {
        // Calculăm direcția gravitațională: de la inamic spre centrul planetei
        Vector3 gravityDirection = (planet.position - transform.position).normalized;
        rb.AddForce(gravityDirection * gravityStrength, ForceMode.Acceleration);

        // Calculăm "up-ul" corect (normalul planetei la poziția inamicului)
        Vector3 desiredUp = (transform.position - planet.position).normalized;
        Quaternion targetRotation = Quaternion.FromToRotation(transform.up, desiredUp) * transform.rotation;
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * 5f);
    }

    private void HandleMovement()
    {
        Vector3 targetPos = enemyController.GetTargetPosition();
        // Dacă ținta este la poziția curentă (nu avem informații sau am ajuns la ultima poziție cunoscută),
        // nu mișcăm inamicul.
        if (targetPos == transform.position)
        {
            latestMoveDirection = Vector3.zero;
            return;
        }

        Vector3 directionToTarget = targetPos - transform.position;
        // Proiectăm direcția pe planul tangent (calculăm normalul planetei)
        Vector3 planetNormal = (transform.position - planet.position).normalized;
        Vector3 moveDirection = Vector3.ProjectOnPlane(directionToTarget, planetNormal).normalized;
        latestMoveDirection = moveDirection;

        // Dacă ținta nu este în raza de atac, mișcăm inamicul
        if (!enemyController.IsTargetInAttackRange())
        {
            Vector3 targetPosition = rb.position + moveDirection * moveSpeed * Time.fixedDeltaTime;
            rb.MovePosition(targetPosition);
        }
    }

    void LateUpdate()
    {
        // Actualizează rotația grafică pentru feedback vizual
        if (latestMoveDirection != Vector3.zero)
        {
            Vector3 planetNormal = (transform.position - planet.position).normalized;
            Quaternion moveRotation = Quaternion.LookRotation(latestMoveDirection, planetNormal);
            gfx.rotation = Quaternion.Slerp(gfx.rotation, moveRotation, Time.deltaTime * rotationSpeed);
        }
    }
}
