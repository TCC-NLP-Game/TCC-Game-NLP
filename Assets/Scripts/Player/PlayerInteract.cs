using Inworld;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class PlayerInteract : MonoBehaviour
{
    [SerializeField] GameObject interactHint;

    private float targetWeight;
    private Rig rig;
    public Transform playerHead;
    public Transform playerTarget;

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
        HandleInteract();
        HandleCloseChat();
        rig.weight = Mathf.Lerp(rig.weight, targetWeight, Time.deltaTime * 10f);
    }

    private bool GetIsDialogueOpen()
    {
        return GameManager.Instance.dialogueManager.isDialogueOpen;
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
        if (Physics.Raycast(ray, out RaycastHit hit, interactRange) && !GetIsDialogueOpen())
        {
            if (hit.collider.TryGetComponent(out NPCInteractable npcInteractable))
            {
                interactHint.SetActive(true);
                if (Input.GetKeyDown(KeyCode.E))
                {
                    targetWeight = 1f;
                    playerTarget.position = npcInteractable.npcHead.transform.position;
                    npcInteractable.Interact();
                }
            }
        }
    }

    private void HandleCloseChat()
    {
        if (CanDialogueBeClosed() && Input.GetKeyDown(KeyCode.Escape))
        {
            targetWeight = 0;
            InworldController.CurrentCharacter.CancelResponse();
            GameManager.Instance.dialogueManager.CloseChat();
        }
    }
}