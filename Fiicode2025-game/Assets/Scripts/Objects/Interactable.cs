using UnityEngine;

[RequireComponent(typeof(Outline))]
public abstract class Interactable : MonoBehaviour
{
    private Outline outline;

    void Awake()
    {
        outline = GetComponent<Outline>();
        outline.enabled = false;
    }

    public virtual void CanInteract()
    {
        if(outline != null) outline.enabled = true;
    }

    public virtual void CantInteract()
    {
        if(outline != null) outline.enabled = false;
    }

    public virtual void Interact()
    {
        
    }

    public virtual void Interact(Transform player)
    {
        
    }

    public virtual void CompletedScanLogic()
    {

    }
}
