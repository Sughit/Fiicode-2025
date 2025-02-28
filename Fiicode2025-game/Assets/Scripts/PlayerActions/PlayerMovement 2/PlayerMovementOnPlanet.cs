using UnityEngine;

public class PlayerMovementOnPlanet : MonoBehaviour
{
    [SerializeField] private Transform planet;
    [SerializeField] private Transform gfx; 
    [SerializeField] private float gravityStrength = 10f;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private LayerMask planetLayer;

    private Rigidbody rb;
    private Vector2 moveInput;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false; 

        if (PlayerController.instance != null)
        {
            PlayerController.instance.OnMove += HandleMoveInput;
        }
    }

    void HandleMoveInput(Vector2 input)
    {
        moveInput = input;
    }

    void Update()
    {
        ApplyGravity();
        HandleMovement();
    }

    void ApplyGravity()
    {
        Vector3 gravityDirection = (planet.position - transform.position).normalized;
        rb.AddForce(gravityDirection * gravityStrength, ForceMode.Acceleration);

        Quaternion targetRotation = Quaternion.FromToRotation(transform.up, -gravityDirection) * transform.rotation;
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
    }

    void HandleMovement()
    {
        if (moveInput != Vector2.zero)
        {
            Vector3 moveDirection = (transform.right * moveInput.x + transform.forward * moveInput.y).normalized;
            Vector3 targetPosition = rb.position + moveDirection * moveSpeed * Time.deltaTime;
            rb.MovePosition(Vector3.Lerp(rb.position, targetPosition, 0.5f));

            Quaternion moveRotation = Quaternion.LookRotation(moveDirection, transform.up);
            gfx.rotation = Quaternion.Slerp(gfx.rotation, moveRotation, Time.deltaTime * rotationSpeed);
        }
    }
}
