using UnityEngine;

public class PlayerMovementOnPlanet : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform planet;
    [SerializeField] private Transform gfx; 
    [SerializeField] private Transform cameraTransform;  // <--- Asignează camera în Inspector (ex: Camera.main)

    [Header("Movement Settings")]
    [SerializeField] private float gravityStrength = 10f;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 10f;

    private Rigidbody rb;
    private Vector2 moveInput;
    
    // Salvăm direcția de mișcare aici, pentru rotația din LateUpdate
    private Vector3 latestMoveDirection;

    private NewPlayerAttack playerAttack;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        // Asigură-te că, în Inspector, ai:
        // - Rb.interpolation = Interpolate
        // - Collision Detection = Continuous (dacă ai suprafețe neregulate)

        playerAttack = GetComponent<NewPlayerAttack>();
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
        // 1. Atras de planetă
        Vector3 gravityDirection = (planet.position - transform.position).normalized;
        rb.AddForce(gravityDirection * gravityStrength, ForceMode.Acceleration);

        // 2. Orientează "susul" jucătorului contra direcției gravitației
        Quaternion targetRotation = Quaternion.FromToRotation(transform.up, -gravityDirection) * transform.rotation;
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * 5f);
    }

    private void HandleMovement()
    {
        if (moveInput != Vector2.zero && cameraTransform != null)
        {
            // Proiectăm forward și right ale camerei pe planul tangent la planetă (perpendicular pe transform.up)
            Vector3 cameraForward = Vector3.ProjectOnPlane(cameraTransform.forward, transform.up).normalized;
            Vector3 cameraRight   = Vector3.ProjectOnPlane(cameraTransform.right, transform.up).normalized;

            // Construiești direcția de mișcare în planul tangent
            Vector3 moveDirection = (cameraRight * moveInput.x + cameraForward * moveInput.y).normalized;

            Vector3 targetPosition = rb.position + moveDirection * moveSpeed * Time.fixedDeltaTime;
            rb.MovePosition(targetPosition);

            latestMoveDirection = moveDirection;
        }
        else
        {
            // Dacă nu există input, nu păstrăm ultimul moveDirection
            latestMoveDirection = Vector3.zero;
        }
    }

    // În LateUpdate rotim doar partea vizuală (gfx) pentru animație / feedback
    void LateUpdate()
    {
        if (latestMoveDirection != Vector3.zero && playerAttack.currentTarget == null)
        {
            Quaternion moveRotation = Quaternion.LookRotation(latestMoveDirection, transform.up);
            gfx.rotation = Quaternion.Slerp(gfx.rotation, moveRotation, Time.deltaTime * rotationSpeed);
        }
    }
}
