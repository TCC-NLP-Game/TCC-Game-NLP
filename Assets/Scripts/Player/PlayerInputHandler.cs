using Inworld;
using TMPro;
using UnityEngine;

public class PlayerInputHandler : MonoBehaviour
{
    [SerializeField] protected TMP_InputField inputField;

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
        if (string.IsNullOrEmpty(inputField.text) || !InworldController.CurrentCharacter)
            return;
        try
        {
            if (InworldController.CurrentCharacter)
            {
                InworldController.CurrentCharacter.SendText(inputField.text);
                inputField.interactable = false;
                inputField.text = "";
                GameManager.Instance.dialogueManager.SetIsClosable(false);
            }
        }
        catch (InworldException error)
        {
            InworldAI.LogWarning($"Failed to send texts: {error}");
        }
    }
}
