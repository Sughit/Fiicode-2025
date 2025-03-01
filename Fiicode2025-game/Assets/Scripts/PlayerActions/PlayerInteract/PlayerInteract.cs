using UnityEngine;
using System.Collections.Generic;

public class PlayerInteract : MonoBehaviour
{
    private Collider[] interactablesBuffer = new Collider[20];
    private HashSet<Interactable> previousInteractables = new HashSet<Interactable>();
    private HashSet<Interactable> currentInteractables = new HashSet<Interactable>();
    private int interactableCount = 0;

    [SerializeField] private float interactRange = 5f;
    [SerializeField] private LayerMask interactLayer;

    [SerializeField] private float checkInterval = 0.1f;
    private float nextCheckTime;

    void Start()
    {
        if (PlayerController.instance != null)
        {
            PlayerController.instance.OnInteract += Interact;
        }
    }

    void Update()
    {
        if (Time.time >= nextCheckTime)
        {
            nextCheckTime = Time.time + checkInterval;
            RefreshInteractables();
        }
    }

    void RefreshInteractables()
    {
        // Pregătim seturile
        var temp = previousInteractables;
        previousInteractables = currentInteractables;
        currentInteractables = temp;
        currentInteractables.Clear();

        // Căutăm obiectele din jur
        interactableCount = Physics.OverlapSphereNonAlloc(
            transform.position,
            interactRange,
            interactablesBuffer,
            interactLayer
        );

        Interactable firstInteractable = null;
        if (interactableCount > 0)
        {
            firstInteractable = interactablesBuffer[0].GetComponent<Interactable>();
        }

        // Activează doar primul interactiv dacă există
        if (firstInteractable != null)
        {
            firstInteractable.CanInteract();
            currentInteractables.Add(firstInteractable);
        }

        // Orice alt obiect care fusese "interactabil" înainte,
        // dar nu e primul acum, devine neinteractabil
        foreach (Interactable prev in previousInteractables)
        {
            if (prev != firstInteractable)
                prev.CantInteract();
        }
    }

    void Interact()
    {
        // Interactăm doar cu primul obiect din buffer (dacă există)
        if (interactableCount > 0)
        {
            Interactable interactable = interactablesBuffer[0].GetComponent<Interactable>();
            if (interactable != null)
            {
                if (interactable.CompareTag("Ruin") || interactable.CompareTag("Resource"))
                {
                    interactable.Interact(transform);
                }
                else
                {
                    interactable.Interact();
                }
            }
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, interactRange);
    }
}
