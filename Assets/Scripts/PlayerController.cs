using Inworld;
using Inworld.Packet;
using TMPro;
using UnityEngine;

public class PlayerController : SingletonBehavior<PlayerController>
{
    [Header("References")]
    [SerializeField] protected TMP_InputField m_InputField;

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
        if (!m_InputField || string.IsNullOrEmpty(m_InputField.text) || !InworldController.CurrentCharacter)
            return;
        try
        {
            if (InworldController.CurrentCharacter)
                InworldController.CurrentCharacter.SendText(m_InputField.text);
            m_InputField.text = "";
        }
        catch (InworldException e)
        {
            InworldAI.LogWarning($"Failed to send texts: {e}");
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
