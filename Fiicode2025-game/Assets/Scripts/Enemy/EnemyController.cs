using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [Header("Detection Settings")]
    public float detectionRange = 3f;       // Raza de detecție
    public float attackRange = 2f;          // Raza de atac (folosit pentru decizii)
    public LayerMask playerLayer;           // Layer-ul pentru jucător

    // Starea curentă a detectării
    public Transform player { get; private set; }
    public Vector3 lastKnownPlayerPosition { get; private set; }
    public bool HasLastKnown { get; private set; } = false;

    // Buffer pentru OverlapSphereNonAlloc
    private Collider[] detectionResults = new Collider[1];

    void FixedUpdate()
    {
        DetectPlayer();
    }

    private void DetectPlayer()
    {
        int count = Physics.OverlapSphereNonAlloc(transform.position, detectionRange, detectionResults, playerLayer);
        if (count > 0)
        {
            // Dacă găsim cel puțin un collider, luăm referința primului obiect detectat
            player = detectionResults[0].transform;
            lastKnownPlayerPosition = player.position;
            HasLastKnown = true;
        }
        else
        {
            // Dacă nu este detectat jucătorul, resetați referința (dar păstrăm ultima poziție cunoscută)
            player = null;
        }
    }

    // Metodă utilă pentru a obține poziția țintă: poziția actuală a jucătorului (dacă este detectat)
    // sau ultima poziție cunoscută, altfel poziția inamicului.
    public Vector3 GetTargetPosition()
    {
        if (player != null)
            return player.position;
        else if (HasLastKnown)
            return lastKnownPlayerPosition;
        else
            return transform.position;
    }

    // Verifică dacă ținta (jucătorul sau ultima poziție cunoscută) se află în raza de atac
    public bool IsTargetInAttackRange()
    {
        Vector3 targetPos = GetTargetPosition();
        return (Vector3.Distance(transform.position, targetPos) <= attackRange && targetPos != transform.position);
    }
}
