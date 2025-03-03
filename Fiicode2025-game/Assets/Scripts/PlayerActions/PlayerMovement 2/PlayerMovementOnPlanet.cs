using UnityEngine;

public class PlayerMovementOnPlanet : MonoBehaviour
{
    [SerializeField] private Transform planet;
    [SerializeField] private Transform gfx; 
    [SerializeField] private float gravityStrength = 10f;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 10f;

    private Rigidbody rb;
    private Vector2 moveInput;
    
    // Salvăm direcția de mișcare aici, pentru rotația din LateUpdate
    private Vector3 latestMoveDirection;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        // Asigură-te că, în Inspector, ai:
        // - Rb.interpolation = Interpolate
        // - Collision detection = Continuous (dacă ai suprafețe neregulate)
    }

    void Start()
    {
        if (PlayerController.instance != null)
        {
            PlayerController.instance.OnMove += HandleMoveInput;
        }
    }

    void HandleMoveInput(Vector2 input)
    {
        moveInput = input;
    }

    void FixedUpdate()
    {
        ApplyGravity();
        HandleMovement();
    }

    private void ApplyGravity()
    {
        Vector3 gravityDirection = (planet.position - transform.position).normalized;
        rb.AddForce(gravityDirection * gravityStrength, ForceMode.Acceleration);

        // Rotește corpul jucătorului spre planetă
        Quaternion targetRotation = Quaternion.FromToRotation(transform.up, -gravityDirection) * transform.rotation;
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * 5f);
    }

    private void HandleMovement()
    {
        if (moveInput != Vector2.zero)
        {
            // Calculăm direcția de deplasare
            Vector3 moveDirection = (transform.right * moveInput.x + transform.forward * moveInput.y).normalized;
            Vector3 targetPosition = rb.position + moveDirection * moveSpeed * Time.fixedDeltaTime;

            // Mișcăm rigidbody-ul
            rb.MovePosition(targetPosition);

            // Salvezi direcția pentru rotația ulterioară
            latestMoveDirection = moveDirection;
        }
        else
        {
            // Dacă player-ul nu se mișcă, nu vrem să "aruncăm" un moveDirection vechi
            latestMoveDirection = Vector3.zero;
        }
    }

    // În LateUpdate, facem rotația graficii la fiecare frame
    void LateUpdate()
    {
        // Dacă nu ai moveDirection, nu roti gfx
        if (latestMoveDirection != Vector3.zero)
        {
            Quaternion moveRotation = Quaternion.LookRotation(latestMoveDirection, transform.up);
            gfx.rotation = Quaternion.Slerp(gfx.rotation, moveRotation, Time.deltaTime * rotationSpeed);
        }
    }
}
