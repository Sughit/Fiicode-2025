using UnityEngine;
using System.Collections.Generic;

public class PlayerInteract : MonoBehaviour
{
    private Collider[] interactables = new Collider[10];
    private HashSet<Interactable> previousInteractables = new HashSet<Interactable>();
    private HashSet<Interactable> currentInteractables = new HashSet<Interactable>();
    private int interactableCount = 0;

    [SerializeField] private float interactRange = 5f;
    [SerializeField] private LayerMask interactLayer;

    void Start()
    {
        if (PlayerController.instance != null)
        {
            PlayerController.instance.OnInteract += Interact;
        }
    }

    void Update()
    {
        HashSet<Interactable> temp = previousInteractables;
        previousInteractables = currentInteractables;
        currentInteractables = temp;
        currentInteractables.Clear();

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

        foreach (Interactable interactable in previousInteractables)
        {
            if (!currentInteractables.Contains(interactable))
                interactable.CantInteract();
        }
    }

    void Interact()
    {
        if (interactableCount > 0)
        {
            Interactable interactable = interactables[0].GetComponent<Interactable>();
            if (interactable != null)
            {
                interactable.Interact(transform);
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
