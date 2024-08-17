public class LetterInteract : InteractableObject
{
    public override void Interact()
    {
        Destroy(gameObject);
        PlayerInventory.Instance.AddToInventory();
    }
}