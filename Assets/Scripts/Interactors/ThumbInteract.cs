using UnityEngine.SceneManagement;

public class ThumbInteract : InteractableObject
{
    public override void Interact()
    {
        SceneManager.LoadScene("EndScene");
    }
}