using UnityEngine;
using TMPro;
using Inworld.Packet;
using Inworld;

public class DialogueHandler: MonoBehaviour
{
    [SerializeField] protected TextMeshProUGUI npcTextBox;
    [SerializeField] protected TMP_InputField inputField;

    private void OnEnable()
    {
        inputField.interactable = true;
        inputField.ActivateInputField();
        InworldController.Instance.OnCharacterInteraction += OnInteraction;
    }

    private void OnDisable()
    {
        npcTextBox.text = "";
        if (!InworldController.Instance)
            return;
        InworldController.Instance.OnCharacterInteraction -= OnInteraction;
    }

    private void OnInteraction(InworldPacket incomingPacket)
    {
        GameManager.Instance.dialogueManager.SetIsClosable(true);
        switch (incomingPacket)
        {
            case EmotionPacket:
                HandleEmotion();
                break;
            case TextPacket textPacket:
                HandleText(textPacket);
                break;
            case ControlPacket:
                HandleControl();
                break;
            case AudioPacket:
                break;
            default:
                InworldAI.LogWarning($"Received unknown {incomingPacket.type}");
                break;
        }
    }

    private void HandleText(TextPacket textPacket)
    {
        string whoIsTalking = textPacket.routing.source.type.ToUpper();
        string content = textPacket.text.text;
        switch (whoIsTalking)
        {
            case "AGENT":
                npcTextBox.text = content;
                break;
        }
    }

    private void HandleEmotion()
    {
        return; // Handled by children
    }

    private void HandleControl()
    {
        inputField.text = "";
        inputField.interactable = true;
        inputField.ActivateInputField();
    }
}
