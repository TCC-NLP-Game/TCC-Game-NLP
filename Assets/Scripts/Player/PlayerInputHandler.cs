using Convai.Scripts;
using Convai.Scripts.Utils;
using TMPro;
using UnityEngine;

public class PlayerInputHandler : MonoBehaviour
{
    [SerializeField] protected TMP_InputField inputField;
    private Animator animator;  

    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        HandleInput();
    }

    private void HandleInput()
    {
        if (!GameManager.Instance.dialogueManager.isDialogueOpen) return;
        if (Input.GetKeyUp(KeyCode.Return) || Input.GetKeyUp(KeyCode.KeypadEnter))
            SendText();
    }

    private void SendText()
    {
        ConvaiNPC currentCharacter = ConvaiNPCManager.Instance.GetActiveConvaiNPC();
        if (string.IsNullOrEmpty(inputField.text) || !currentCharacter)
            return;
        animator.Play("Talk");
        currentCharacter.SendTextDataAsync(inputField.text);
        inputField.interactable = false;
        GameManager.Instance.dialogueManager.SetIsClosable(false);
    }
}
