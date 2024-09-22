using UnityEngine;

public class InteractableObject : MonoBehaviour
{
    [SerializeField] private bool canInteract = false;

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