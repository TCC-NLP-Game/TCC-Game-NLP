using Inworld.Packet;
using Inworld;
using UnityEngine;
using Inworld.Entities;
using TMPro;

public class DialogueService : MonoBehaviour
{
    [SerializeField] protected TextMeshProUGUI npcTextBox;
    [SerializeField] protected TextMeshProUGUI playerTextBox;

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
                    string charName = charData.givenName ?? "Character";
                    npcTextBox.text = $"{charName}: {content}";
                }
                break;
            case "PLAYER":
                playerTextBox.text = $"You: {content}";
                break;

        }


    }
}
