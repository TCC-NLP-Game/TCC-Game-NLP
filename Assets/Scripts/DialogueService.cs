using Inworld.Packet;
using Inworld;
using UnityEngine;
using Inworld.Entities;
using TMPro;

public class DialogueService : MonoBehaviour
{
    [SerializeField] protected TextMeshProUGUI npcTextBox;
    [SerializeField] protected TextMeshProUGUI npcNameTextBox;
    [SerializeField] protected TextMeshProUGUI playerTextBox;
    [SerializeField] protected TMP_InputField inputField;

    void Update()
    {
        // todo: move to a function to be called on interact trigger, instead frame update
        InworldCharacterData charData = InworldController.CurrentCharacter.Data;
        npcNameTextBox.text = charData.givenName ?? "Character";
    }

    private void OnEnable()
    {
        inputField.interactable = true;
        InworldController.Instance.OnCharacterInteraction += OnInteraction;
    }

    private void OnDisable()
    {
        npcTextBox.text = "";
        playerTextBox.text = "";
        if (!InworldController.Instance)
            return;
        InworldController.Instance.OnCharacterInteraction -= OnInteraction;
    }

    private void OnInteraction(InworldPacket incomingPacket)
    {
        GameManager.Instance.dialogueManager.SetIsClosable(true);
        switch (incomingPacket)
        {
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
            case "PLAYER":
                playerTextBox.text = content;
                break;
        }
    }

    private void HandleControl()
    {
        inputField.interactable = true;
    }
}
