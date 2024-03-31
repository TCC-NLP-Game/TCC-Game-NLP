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

    protected virtual void OnEnable()
    {
        InworldController.Instance.OnCharacterInteraction += OnInteraction;
    }

    protected virtual void OnDisable()
    {
        npcTextBox.text = "";
        playerTextBox.text = "";
        if (!InworldController.Instance)
            return;
        InworldController.Instance.OnCharacterInteraction -= OnInteraction;
    }

    protected virtual void OnInteraction(InworldPacket incomingPacket)
    {
        switch (incomingPacket)
        {
            case TextPacket textPacket:
                HandleText(textPacket);
                break;
            case ControlPacket controlPacket:
                HandleControl(controlPacket);
                break;
            default:
                InworldAI.LogWarning($"Received unknown {incomingPacket.type}");
                break;
        }
    }

    protected virtual void HandleText(TextPacket textPacket)
    {
        InworldCharacterData charData = InworldController.CharacterHandler.GetCharacterDataByID(textPacket.routing.source.name);
        string whoIsTalking = textPacket.routing.source.type.ToUpper();
        string content = textPacket.text.text;
        switch (whoIsTalking)
        {
            case "AGENT":
                if (charData != null)
                {
                    npcTextBox.text = content;
                    npcNameTextBox.text = charData.givenName ?? "Character";
                }
                break;
            case "PLAYER":
                playerTextBox.text = content;
                break;
        }
    }

    protected virtual void HandleControl(ControlPacket controlPacket)
    {
        inputField.interactable = true;
    }
}
