using UnityEngine;
using UnityEngine.InputSystem;
using System;
using Cinemachine;

[RequireComponent(typeof(PlayerInput))]
public class PlayerController : MonoBehaviour
{
    public static PlayerController instance;

    public event Action<Vector2> OnMove;
    public event Action OnInteract, OnCancelInteract;
    public event Action OnOpenResearchMenu;
    public event Action OnOpenBuildingMenu;
    public event Action OnAttack;

    PlayerInput playerInput;
    InputAction moveAction, interactAction, openResearchMenuAction, openBuildingMenuAction, attackAction;

    public CinemachineVirtualCamera interactionCam;

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(this);

        playerInput = GetComponent<PlayerInput>();

        moveAction = playerInput.actions.FindAction("Move");
        interactAction = playerInput.actions.FindAction("Interact");
        openResearchMenuAction = playerInput.actions.FindAction("OpenResearchMenu");
        openBuildingMenuAction = playerInput.actions.FindAction("OpenBuildingMenu");
        attackAction = playerInput.actions.FindAction("Attack");

        moveAction.performed += OnMovePerformed;
        moveAction.canceled += OnMoveCanceled;
        interactAction.performed += ctx => OnInteract?.Invoke();
        openResearchMenuAction.performed += ctx => OnOpenResearchMenu?.Invoke();
        openBuildingMenuAction.performed += ctx => OnOpenBuildingMenu?.Invoke();
        // Check interaction state before firing an attack.
        attackAction.performed += ctx =>
        {
            if (interactionCam != null && !interactionCam.gameObject.activeSelf) OnAttack?.Invoke();
        };
    }

    void OnMovePerformed(InputAction.CallbackContext context)
    {
        Vector2 input = context.ReadValue<Vector2>();
        OnMove?.Invoke(input);

        // If the player is moving and the interaction camera is active, disable it.
        if (input.sqrMagnitude > 0.01f && interactionCam != null && interactionCam.gameObject.activeSelf)
        {
            OnCancelInteract?.Invoke();
        }
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
        attackAction.Enable();
    }

    void OnDisable()
    {
        moveAction.Disable();
        interactAction.Disable();
        openResearchMenuAction.Disable();
        openBuildingMenuAction.Disable();
        attackAction.Disable();
    }
}
