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
        outline.enabled = true;
    }

    public virtual void CantInteract()
    {
        outline.enabled = false;
    }

    public virtual void Interact(Transform player)
    {
        
    }
}
