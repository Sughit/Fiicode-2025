using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovementOnPlanet : MonoBehaviour
{
    [SerializeField] private Transform planet;
    [SerializeField] private Transform gfx; 
    [SerializeField] private float gravityStrength = 10f;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private LayerMask planetLayer; // Set this to the layer of planets in the Inspector

    private Rigidbody rb;
    private PlayerInput playerInput;
    private InputAction moveAction;
    private Vector3 currentVelocity = Vector3.zero;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false; 

        playerInput = GetComponent<PlayerInput>();
        moveAction = playerInput.actions.FindAction("Move");
    }

    private void OnEnable() => moveAction.Enable();
    private void OnDisable() => moveAction.Disable();

    void Update()
    {
        ApplyGravity();
        HandleMovement();
    }

    void ApplyGravity()
    {
        Vector3 gravityDirection = (planet.position - transform.position).normalized;
        rb.AddForce(gravityDirection * gravityStrength, ForceMode.Acceleration);

        // Rotate player to align with planet surface
        Quaternion targetRotation = Quaternion.FromToRotation(transform.up, -gravityDirection) * transform.rotation;
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
    }

    void HandleMovement()
    {
        Vector2 inputDirection = moveAction.ReadValue<Vector2>();
        
        if (inputDirection != Vector2.zero)
        {
            // Convert input direction to world direction relative to the planet
            Vector3 moveDirection = (transform.right * inputDirection.x + transform.forward * inputDirection.y).normalized;

            // Smooth movement using Lerp
            Vector3 targetPosition = rb.position + moveDirection * moveSpeed * Time.deltaTime;
            rb.MovePosition(Vector3.Lerp(rb.position, targetPosition, 0.5f));

            // Smoothly rotate the player in the movement direction
            Quaternion moveRotation = Quaternion.LookRotation(moveDirection, transform.up);
            gfx.rotation = Quaternion.Slerp(gfx.rotation, moveRotation, Time.deltaTime * rotationSpeed);
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        // Check if the object we are colliding with is in the Planet Layer
        if (((1 << collision.gameObject.layer) & planetLayer) != 0)
        {
            rb.linearVelocity = Vector3.zero; // Prevent sliding when on the surface
        }
    }
}
