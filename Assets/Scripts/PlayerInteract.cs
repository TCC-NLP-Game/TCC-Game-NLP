using Inworld;
using UnityEngine;

public class PlayerInteract : MonoBehaviour
{
    [SerializeField] GameObject interactHint;

    private void Start()
    {
        interactHint.SetActive(false);
    }

    void Update()
    {
        HandleInteract();
        HandleCloseChat();
    }
    private bool GetIsDialogueOpen()
    {
        return GameManager.Instance.dialogueManager.isDialogueOpen;
    }

    private void HandleInteract()
    {
        float interactRange = 4f;
        Ray ray = new(Camera.main.transform.position, Camera.main.transform.forward);
        interactHint.SetActive(false);
        if (Physics.Raycast(ray, out RaycastHit hit, interactRange) && !GetIsDialogueOpen())
        {
            if (hit.collider.TryGetComponent(out NPCInteractable npcInteractable))
            {
                interactHint.SetActive(true);
                if (Input.GetKeyDown(KeyCode.E))
                {
                    npcInteractable.Interact();
                }
            }
        }
    }

    private void HandleCloseChat()
    {
        bool canBeClosed = GetIsDialogueOpen() && GameManager.Instance.dialogueManager.canBeClosed;
        if (canBeClosed && Input.GetKeyDown(KeyCode.Escape))
        {
            InworldController.CurrentCharacter.CancelResponse();
            GameManager.Instance.dialogueManager.CloseChat();
        }
    }
}