using System;
using Convai.Scripts;
using Convai.Scripts.Utils;
using UnityEngine;

/// <summary>
///     A group of NPCs that are currently conversing with each other.
/// </summary>
[Serializable]
public class NPCGroup
{
    [field: SerializeField] public ConvaiGroupNPCController GroupNPC1 { get; private set; }
    [field: SerializeField] public ConvaiGroupNPCController GroupNPC2 { get; private set; }
    public string topic;
    [ReadOnly] public string messageToRelay;

    public ConvaiGroupNPCController CurrentSpeaker { get; set; }
    public ConvaiGroupNPCController CurrentListener => CurrentSpeaker == GroupNPC1 ? GroupNPC2 : GroupNPC1;

    private bool _isPlayerNearGroup;
    private Action<bool, ConvaiGroupNPCController> _vicinityChangedCallback;

    public void Initialize(Action<bool, ConvaiGroupNPCController> vicinityChangedCallback)
    {
        _vicinityChangedCallback = vicinityChangedCallback;
        GroupNPC1.OnPlayerVicinityChanged += HandleVicinity;
        GroupNPC1.OnPlayerVicinityChanged += HandleVicinity;
    }

    ~NPCGroup()
    {
        GroupNPC1.OnPlayerVicinityChanged -= HandleVicinity;
        GroupNPC1.OnPlayerVicinityChanged -= HandleVicinity;
    }

    private void HandleVicinity(bool isPlayerNear, ConvaiGroupNPCController npc)
    {
        if (isPlayerNear && !_isPlayerNearGroup)
        {
            _isPlayerNearGroup = true;
            _vicinityChangedCallback?.Invoke(true, npc);
        }

        if (!isPlayerNear && _isPlayerNearGroup)
        {
            _isPlayerNearGroup = false;
            _vicinityChangedCallback?.Invoke(false, npc);
        }
    }

    public bool IsAnyoneTalking()
    {
        return GroupNPC1.ConvaiNPC.IsCharacterTalking || GroupNPC2.ConvaiNPC.IsCharacterTalking;
    }

    public bool BelongToGroup(ConvaiGroupNPCController controller)
    {
        return controller.CharacterID == GroupNPC1.CharacterID || controller.CharacterID == GroupNPC2.CharacterID;
    }

    public bool BothNPCAreNotNull()
    {
        return GroupNPC1 != null && GroupNPC2 != null;
    }
    public bool BothNPCAreNotActiveNPC()
    {
        ConvaiNPC activeNPC = ConvaiNPCManager.Instance.activeConvaiNPC;
        string activeNPCId = activeNPC != null ? activeNPC.characterID : string.Empty;
        return !GroupNPC1.CharacterID.Equals(activeNPCId) && !GroupNPC2.CharacterID.Equals(activeNPCId);
    }
}