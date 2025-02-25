using UnityEngine;
using UnityEngine.InputSystem;

public class PlanetaryMovement : MonoBehaviour
{
    public Transform planet;
    public float speed = 200f;
    public float gravityStrength = 2f;

    private Vector2 moveInput;
    private Rigidbody rb;
    [SerializeField] private PlayerInput playerInput;
    private InputAction moveAction;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;

        playerInput = GetComponent<PlayerInput>();
        moveAction = playerInput.actions.FindAction("Move");
    }
    void OnEnabled()
    {
        moveAction.Enable();
    }

    void OnDisabled()
    {
        moveAction.Disable();
    }

    void FixedUpdate()
    {
        ApplyGravity();
        ApplyPlanetaryMovement();

        Vector2 dir = moveAction.ReadValue<Vector2>();

        if (dir != Vector2.zero)
        {
            // Calculăm direcția de mișcare relativ la suprafața planetei
            Vector3 moveDirection = new Vector3(dir.x, 0, dir.y);
            Vector3 planetUp = (transform.position - planet.position).normalized;
            Vector3 planetRight = Vector3.Cross(planetUp, transform.forward).normalized;
            Vector3 planetForward = Vector3.Cross(planetRight, planetUp).normalized;

            Vector3 projectedMoveDirection = (planetRight * moveDirection.x + planetForward * moveDirection.z).normalized;

            transform.position += projectedMoveDirection * speed * Time.deltaTime;
        }
    }

    void ApplyGravity()
    {
        Vector3 gravityDirection = (planet.position - transform.position).normalized;
        rb.AddForce(gravityDirection * gravityStrength, ForceMode.Acceleration);
    }

    void ApplyPlanetaryMovement()
    {
        transform.rotation = Quaternion.FromToRotation(transform.up, (transform.position - planet.position).normalized) * transform.rotation;
    }
}