using UnityEngine;

public class InteractableObject : MonoBehaviour
{
    protected bool canInteract = true;

    public void EnableInteraction()
    {
        canInteract = true;
    }

    public bool CanInteract()
    {
        return canInteract;
    }

    public virtual void Interact() { }
}