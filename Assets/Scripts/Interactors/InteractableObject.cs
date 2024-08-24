using UnityEngine;

public class InteractableObject : MonoBehaviour
{
    protected bool canInteract = false;

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