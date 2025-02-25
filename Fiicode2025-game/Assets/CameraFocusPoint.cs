using UnityEngine;
using UnityEngine.InputSystem;

public class RotateObjectWithMouse : MonoBehaviour
{
    public float xSpeed = 120.0f;
    public float ySpeed = 120.0f;

    float x = 0.0f;
    float y = 0.0f;
    [SerializeField] private PlayerInput playerInput;
    private InputAction lookAction;

    void Start()
    {
        Vector3 angles = transform.eulerAngles;
        x = angles.y;
        y = angles.x;

        lookAction = playerInput.actions.FindAction("Look");
    }

    void OnEnable()
    {
        lookAction.Enable();
    }

    void OnDisable()
    {
        lookAction.Disable();
    }

    void Update()
    {
        Vector2 dir = lookAction.ReadValue<Vector2>();

        x += dir.x * xSpeed * Time.deltaTime;
        y -= dir.y * ySpeed * Time.deltaTime;

        Quaternion rotation = Quaternion.Euler(y, x, 0);
        transform.rotation = rotation;
    }
}