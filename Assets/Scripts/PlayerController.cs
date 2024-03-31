using Inworld;
using TMPro;
using UnityEngine;

public class PlayerController : SingletonBehavior<PlayerController>
{
    [Header("References")]
    [SerializeField] protected TMP_InputField inputField;

    protected virtual void Update()
    {
        HandleInput();
    }

    protected virtual void HandleInput()
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
            }
        }
        catch (InworldException error)
        {
            InworldAI.LogWarning($"Failed to send texts: {error}");
        }
    }

    protected virtual void OnEnable()
    {
        InworldController.CharacterHandler.OnCharacterChanged += OnCharacterChanged;
    }

    protected virtual void OnDisable()
    {
        InworldController.CharacterHandler.OnCharacterChanged -= OnCharacterChanged;
    }

    protected virtual void OnCharacterChanged(InworldCharacter oldChar, InworldCharacter newChar)
    {
        if (newChar == null)
        {
            InworldAI.Log($"No longer talking to anyone.");
            return;
        }
        InworldAI.Log($"Now Talking to: {newChar.Name}");
    }
}
