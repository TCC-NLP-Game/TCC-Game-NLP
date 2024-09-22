using TMPro;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class PlayerInteract : MonoBehaviour
{
    [SerializeField] GameObject interactHint;
    [SerializeField] TextMeshProUGUI interactText;

    private float targetWeight;
    private Rig rig;
    public Transform playerHead;
    public Transform playerTarget;
    private NPCInteractable NPCInteracting;

    private void Awake()
    {
        rig = GetComponentInChildren<Rig>();
    }


    private void Start()
    {
        interactHint.SetActive(false);
    }

    void Update()
    {
        HandlePauseGame();
        HandleInteract();
        HandleCloseChat();
        rig.weight = Mathf.Lerp(rig.weight, targetWeight, Time.deltaTime * 10f);
    }

    private bool GetIsDialogueOpen()
    {
        return GameManager.Instance.dialogueManager.isDialogueOpen;
    }

    private bool GetIsGamePaused()
    {
        return GameManager.Instance.pauseMenu.isPaused;
    }

    private bool CanDialogueBeClosed ()
    {
        return GetIsDialogueOpen() && GameManager.Instance.dialogueManager.canBeClosed;
    }

    private void HandleInteract()
    {
        float interactRange = 4f;
        Ray ray = new(Camera.main.transform.position, Camera.main.transform.forward);
        interactHint.SetActive(false);
        if (Physics.Raycast(ray, out RaycastHit hit, interactRange) && !GetIsDialogueOpen() && !GetIsGamePaused())
        {
            if (hit.collider.TryGetComponent(out InteractableObject interactableObject))
            {
                if (!interactableObject.CanInteract()) return;
                interactText.text = "USE";
                interactHint.SetActive(true);
                if (Input.GetKeyDown(KeyCode.E))
                {
                    interactableObject.Interact();
                }
            }
            if (hit.collider.TryGetComponent(out NPCInteractable npcInteractable))
            {
                bool hasLetter = PlayerInventory.Instance.PlayerHasLetter();
                interactText.text = hasLetter ? "GIVE" : "TALK";
                interactHint.SetActive(true);
                if (Input.GetKeyDown(KeyCode.E))
                {
                    targetWeight = 1f;
                    playerTarget.position = npcInteractable.npcHead.transform.position;
                    npcInteractable.Interact();
                    NPCInteracting = npcInteractable;
                }
            }
        }
    }

    private void HandleCloseChat()
    {
        if (CanDialogueBeClosed() && Input.GetKeyDown(KeyCode.Escape))
        {
            targetWeight = 0;
            GameManager.Instance.dialogueManager.CloseChat();
            NPCInteracting.EndInteraction();
        }
    }


    private void HandlePauseGame()
    {
        if (!GetIsDialogueOpen() && Input.GetKeyDown(KeyCode.Escape))
        {
            GameManager.Instance.pauseMenu.PauseGame();
        }
    }
}