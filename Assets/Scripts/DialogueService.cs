using Inworld.Packet;
using Inworld;
using UnityEngine;
using Inworld.Entities;
using TMPro;

public class DialogueService : MonoBehaviour
{
    [SerializeField] protected TextMeshProUGUI chatBox;

    protected virtual void OnEnable()
    {
        InworldController.Instance.OnCharacterInteraction += OnInteraction;
    }

    protected virtual void OnDisable()
    {
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
        if (charData != null)
        {
            string charName = charData.givenName ?? "Character";
            string content = textPacket.text.text;
            chatBox.text = $"{charName}: {content}";
        }
    }
}
