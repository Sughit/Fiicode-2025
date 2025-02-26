using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class PlayerInteract : MonoBehaviour
{
    private Collider[] interactables = new Collider[10]; // Preallocated array
    private HashSet<Interactable> previousInteractables = new HashSet<Interactable>(); // Use HashSet for fast lookup
    private HashSet<Interactable> currentInteractables = new HashSet<Interactable>(); // Track current interactables
    private int interactableCount = 0;

    [SerializeField] private float interactRange = 5f;
    [SerializeField] private LayerMask interactLayer; 

    private PlayerInput playerInput;
    private InputAction interactAction;

    void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        interactAction = playerInput.actions.FindAction("Interact");
        interactAction.performed += Interact;
    }

    void OnEnable() => interactAction.Enable();
    void OnDisable() => interactAction.Disable();

    void Update()
    {
        // Swap sets to avoid garbage collection
        HashSet<Interactable> temp = previousInteractables;
        previousInteractables = currentInteractables;
        currentInteractables = temp;
        currentInteractables.Clear(); // Reuse the set instead of allocating a new one

        interactableCount = Physics.OverlapSphereNonAlloc(transform.position, interactRange, interactables, interactLayer);

        for (int i = 0; i < interactableCount; i++)
        {
            Interactable interactable = interactables[i].GetComponent<Interactable>();
            if (interactable != null)
            {
                interactable.CanInteract();
                currentInteractables.Add(interactable);
            }
        }

        // Disable interaction for objects that are no longer in range
        foreach (Interactable interactable in previousInteractables)
        {
            if (!currentInteractables.Contains(interactable))
            {
                interactable.CantInteract();
            }
        }
    }

    void Interact(InputAction.CallbackContext context)
    {
        if (interactableCount > 0)
        {
            Interactable interactable = interactables[0].GetComponent<Interactable>();
            if (interactable != null)
            {
                interactable.Interact();
                Debug.Log($"Interacted with {interactables[0].name}");
            }
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.blue; 
        Gizmos.DrawWireSphere(transform.position, interactRange);
    }
}
