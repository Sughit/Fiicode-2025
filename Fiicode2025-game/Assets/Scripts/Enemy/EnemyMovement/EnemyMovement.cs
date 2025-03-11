using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform planet;    // Centrul planetei
    [SerializeField] private Transform player;    // Referința la jucător (poate fi null inițial)
    [SerializeField] private Transform gfx;       // Partea vizuală a inamicului

    [Header("Movement Settings")]
    [SerializeField] private float gravityStrength = 10f;
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private float rotationSpeed = 10f;

    [Header("AI Settings")]
    [SerializeField] private float detectionRange = 3f;  // Raza de detecție
    [SerializeField] private float attackRange = 2f;        // Raza de atac
    [SerializeField] private float attackCooldown = 2f;     // Cooldown între atacuri
    [SerializeField] private LayerMask playerLayer;         // Layer-ul pentru player

    private Rigidbody rb;
    private float lastAttackTime;
    private Vector3 latestMoveDirection;
    
    // Ultima poziție cunoscută a jucătorului
    private Vector3 lastKnownPlayerPosition;
    private bool hasLastKnownPosition = false;

    // Buffer pentru OverlapSphereNonAlloc
    private Collider[] detectionResults = new Collider[5];

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false; // Gravitația este gestionată manual
    }

    void FixedUpdate()
    {
        ApplyGravity();
        DetectPlayer();
        HandleAIMovement();
    }

    // Folosim OverlapSphereNonAlloc pentru a detecta jucătorul în jurul inamicului, filtrând după layer
    private void DetectPlayer()
    {
        int count = Physics.OverlapSphereNonAlloc(transform.position, detectionRange, detectionResults, playerLayer);
        if (count > 0)
        {
            // Luăm primul collider găsit din layer-ul player
            player = detectionResults[0].transform;
        }
        else
        {
            player = null;
        }
    }

    private void ApplyGravity()
    {
        // Calculăm direcția gravitațională (de la inamic spre centrul planetei)
        Vector3 gravityDirection = (planet.position - transform.position).normalized;
        rb.AddForce(gravityDirection * gravityStrength, ForceMode.Acceleration);

        // Calculăm normalul planetei la poziția inamicului
        Vector3 desiredUp = (transform.position - planet.position).normalized;
        // Ajustăm rotația pentru a avea "up-ul" corect
        Quaternion targetRotation = Quaternion.FromToRotation(transform.up, desiredUp) * transform.rotation;
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * 5f);
    }

    private void HandleAIMovement()
    {
        // Dacă player-ul a fost detectat, actualizăm ultima poziție cunoscută și urmărim jucătorul
        if (player != null)
        {
            lastKnownPlayerPosition = player.position;
            hasLastKnownPosition = true;

            float distanceToPlayer = Vector3.Distance(transform.position, player.position);
            Vector3 directionToPlayer = player.position - transform.position;
            Vector3 planetNormal = (transform.position - planet.position).normalized;
            Vector3 moveDirection = Vector3.ProjectOnPlane(directionToPlayer, planetNormal).normalized;
            latestMoveDirection = moveDirection;

            // Dacă jucătorul este în afara razei de atac, inamicul se deplasează spre el
            if (distanceToPlayer > attackRange)
            {
                Vector3 targetPosition = rb.position + moveDirection * moveSpeed * Time.fixedDeltaTime;
                rb.MovePosition(targetPosition);
            }
            else
            {
                // Atac dacă jucătorul este în raza de atac și cooldown-ul a expirat
                if (Time.time - lastAttackTime >= attackCooldown)
                {
                    Attack();
                    lastAttackTime = Time.time;
                }
            }
        }
        // Dacă jucătorul nu este detectat, dar avem ultima poziție cunoscută, inamicul merge spre acea poziție
        else if (hasLastKnownPosition)
        {
            float distanceToLast = Vector3.Distance(transform.position, lastKnownPlayerPosition);
            if (distanceToLast > 0.5f)
            {
                Vector3 directionToLast = lastKnownPlayerPosition - transform.position;
                Vector3 planetNormal = (transform.position - planet.position).normalized;
                Vector3 moveDirection = Vector3.ProjectOnPlane(directionToLast, planetNormal).normalized;
                latestMoveDirection = moveDirection;

                Vector3 targetPosition = rb.position + moveDirection * moveSpeed * Time.fixedDeltaTime;
                rb.MovePosition(targetPosition);
            }
            else
            {
                latestMoveDirection = Vector3.zero;
            }
        }
        else
        {
            latestMoveDirection = Vector3.zero;
        }
    }

    private void Attack()
    {
        // Implementați logica de atac (de exemplu, proiectile, animații, etc.)
        Debug.Log("Enemy attacks the player!");
    }

    void LateUpdate()
    {
        // Actualizează rotația grafică pentru feedback vizual, folosind normalul planetei
        if (latestMoveDirection != Vector3.zero)
        {
            Vector3 planetNormal = (transform.position - planet.position).normalized;
            Quaternion moveRotation = Quaternion.LookRotation(latestMoveDirection, planetNormal);
            gfx.rotation = Quaternion.Slerp(gfx.rotation, moveRotation, Time.deltaTime * rotationSpeed);
        }
    }

    void OnDrawGizmos()
    {
        // Desenăm o sferă pentru raza de detecție
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Desenăm o sferă pentru raza de atac
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
