using UnityEngine;
using UnityEngine.InputSystem;
using System;

[RequireComponent(typeof(PlayerInput))]
public class PlayerController : MonoBehaviour
{
    public static PlayerController instance;

    public event Action<Vector2> OnMove;
    public event Action OnInteract;
    public event Action OnOpenResearchMenu;
    public event Action OnOpenBuildingMenu;

    PlayerInput playerInput;
    InputAction moveAction, interactAction, openResearchMenuAction, openBuildingMenuAction;

    void Awake()
    {
        if(instance == null) instance = this;
        else Destroy(this);
        
        playerInput = GetComponent<PlayerInput>();

        moveAction = playerInput.actions.FindAction("Move");
        interactAction = playerInput.actions.FindAction("Interact");
        openResearchMenuAction = playerInput.actions.FindAction("OpenResearchMenu");
        openBuildingMenuAction = playerInput.actions.FindAction("OpenBuildingMenu");

        moveAction.performed += OnMovePerformed;
        moveAction.canceled += OnMoveCanceled;
        interactAction.performed += ctx => OnInteract?.Invoke();
        openResearchMenuAction.performed += ctx => OnOpenResearchMenu?.Invoke();
        openBuildingMenuAction.performed += ctx => OnOpenBuildingMenu?.Invoke();
    }

    void OnMovePerformed(InputAction.CallbackContext context)
    {
        Vector2 input = context.ReadValue<Vector2>();
        OnMove?.Invoke(input);
    }

    void OnMoveCanceled(InputAction.CallbackContext context)
    {
        OnMove?.Invoke(Vector2.zero);
    }

    void OnEnable()
    {
        moveAction.Enable();
        interactAction.Enable();
        openResearchMenuAction.Enable();
        openBuildingMenuAction.Enable();
    }

    void OnDisable()
    {
        moveAction.Disable();
        interactAction.Disable();
        openResearchMenuAction.Disable();
        openBuildingMenuAction.Disable();
    }
}
