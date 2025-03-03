using UnityEngine;

public class PlayerMovementOnPlanet : MonoBehaviour
{
    [SerializeField] private Transform planet;
    [SerializeField] private Transform gfx; 
    [SerializeField] private Transform cameraFollow;  // Referință la obiectul CameraFollow
    [SerializeField] private float gravityStrength = 10f;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 10f;

    private Rigidbody rb;
    private Vector2 moveInput;
    
    // Salvăm direcția de mișcare pentru rotația din LateUpdate
    private Vector3 latestMoveDirection;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        // Asigură-te că, în Inspector, ai:
        // - Rb.interpolation = Interpolate
        // - Collision detection = Continuous (dacă ai suprafețe neregulate)
        cameraFollow = GameObject.Find("CameraFollowTarget").transform;
    }

    void Start()
    {
        if (PlayerController.instance != null)
        {
            PlayerController.instance.OnMove += HandleMoveInput;
        }
    }

    // Manevrează input-ul de mișcare
    void HandleMoveInput(Vector2 input)
    {
        moveInput = input;
    }

    void FixedUpdate()
    {
        ApplyGravity();
        HandleMovement();
    }

    // Aplica gravitația planetei
    private void ApplyGravity()
    {
        Vector3 gravityDirection = (planet.position - transform.position).normalized;
        rb.AddForce(gravityDirection * gravityStrength, ForceMode.Acceleration);

        // Rotește corpul jucătorului spre planetă
        Quaternion targetRotation = Quaternion.FromToRotation(transform.up, -gravityDirection) * transform.rotation;
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * 5f);
    }

    // Manevrează mișcarea jucătorului
    private void HandleMovement()
    {
        if (moveInput != Vector2.zero)
        {
            // Obținem direcțiile relative ale camerei
            Vector3 cameraForward = cameraFollow.forward;  // Direcția înainte a camerei
            Vector3 cameraRight = cameraFollow.right;      // Direcția dreapta a camerei

            // Anulăm componenta pe axa Y (nu vrem mișcare verticală)
            cameraForward.y = 0f;
            cameraRight.y = 0f;

            // Normalizăm direcțiile pentru a evita mișcări rapide (diagonale)
            cameraForward.Normalize();
            cameraRight.Normalize();

            // Calculăm direcția de mișcare
            Vector3 moveDirection = cameraForward * moveInput.y + cameraRight * moveInput.x;

            // Mișcăm jucătorul în direcția calculată
            Vector3 targetPosition = rb.position + moveDirection * moveSpeed * Time.fixedDeltaTime;
            rb.MovePosition(targetPosition);

            // Salvezi direcția pentru rotația ulterioară
            latestMoveDirection = moveDirection;
        }
        else
        {
            // Dacă player-ul nu se mișcă, nu vrem să păstrăm o direcție anterioară
            latestMoveDirection = Vector3.zero;
        }
    }

    // În LateUpdate, facem rotația graficii la fiecare frame
    void LateUpdate()
    {
        // Dacă există o mișcare, actualizăm rotația graficii
        if (latestMoveDirection != Vector3.zero)
        {
            // Obținem doar rotația pe axa Y a obiectului cameraFollow
            float cameraYaw = cameraFollow.eulerAngles.y;

            // Calculăm rotația dorită pentru gfx pe baza unghiului de rotație Y al camerei
            Quaternion moveRotation = Quaternion.Euler(0f, cameraYaw, 0f);
            gfx.rotation = Quaternion.Slerp(gfx.rotation, moveRotation, Time.deltaTime * rotationSpeed);
        }
    }
}
