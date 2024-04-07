using Inworld;
using TMPro;
using UnityEngine;

public class PlayerController : SingletonBehavior<PlayerController>
{
    [SerializeField] protected TMP_InputField inputField;

    void Update()
    {
        HandleInput();
    }

    private void HandleInput()
    {
        if (Input.GetKeyUp(KeyCode.Return) || Input.GetKeyUp(KeyCode.KeypadEnter))
            SendText();
    }

    public void SendText()
    {
        if (!inputField || string.IsNullOrEmpty(inputField.text) || !InworldController.CurrentCharacter)
            return;
        try
        {
            if (InworldController.CurrentCharacter) {
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
