using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Convai.Scripts.Runtime.Utils;
using Convai.Scripts.Utils;
using Grpc.Core;
using Service;
using UnityEngine;
using Logger = Convai.Scripts.Utils.Logger;
using Random = UnityEngine.Random;

/// <summary>
///     Manages the conversation between two Convai powered NPC groups
/// </summary>
public class NPC2NPCConversationManager : MonoBehaviour
{
    private const string GRPC_API_ENDPOINT = "stream.convai.com";
    public static NPC2NPCConversationManager Instance;
    public List<NPCGroup> npcGroups;
    private readonly List<NPCGroup> _groupsWhereConversationEnded = new();
    private string _apiKey = string.Empty;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
        LoadApiKey();
    }

    private void Start()
    {
        foreach (NPCGroup group in npcGroups)
            group.Initialize(HandlePlayerVicinityChanged);

        StartConversationWithAllNPCs();
    }

    /// <summary>
    ///     Handles the event of player vicinity change.
    /// </summary>
    /// <param name="isPlayerNear">Indicates if the player is near.</param>
    /// <param name="npc">The NPC for which the vicinity changed.</param>
    private void HandlePlayerVicinityChanged(bool isPlayerNear, ConvaiGroupNPCController npc)
    {
        if (isPlayerNear)
            ResumeConversation(npc);
    }


    /// <summary>
    ///     Processes the message from the sender NPC.
    /// </summary>
    /// <param name="sender">The NPC sending the message.</param>
    /// <param name="topic">The topic of the conversation.</param>
    /// <param name="message">The message to be processed.</param>
    /// <returns>The processed message.</returns>
    private string ProcessMessage(ConvaiGroupNPCController sender, string topic, string message)
    {
        string processedMessage = $"{sender.CharacterName} said \"{message}\" to you. Reply to it. ";

        processedMessage += Random.Range(0, 2) == 0
            ? $"Talk about something related to {message}. "
            : $"Talk about something other than \"{message}\" but related to {topic}. Gently change the conversation topic. ";

        return processedMessage + "Definitely, reply to the message. Dont address speaker. Keep the reply short. Do not repeat the same message, or keep asking same question.";
    }

    /// <summary>
    ///     Relays the message from the sender NPC.
    /// </summary>
    /// <param name="message">The message to be relayed.</param>
    /// <param name="sender">The NPC sending the message.</param>
    /// <param name="performSwitch"></param>
    public void RelayMessage(string message, ConvaiGroupNPCController sender, bool performSwitch = true)
    {
        NPCGroup npcGroup = npcGroups.Find(c => c.BelongToGroup(sender));
        if (npcGroup == null)
        {
            Debug.LogError("Conversation not found for the sender.");
            return;
        }

        npcGroup.messageToRelay = message;
        if (performSwitch) SwitchSpeaker(npcGroup.CurrentSpeaker);
        if (!npcGroup.CurrentSpeaker.IsPlayerNearMe()) return;
        StartCoroutine(RelayMessageCoroutine(message, npcGroup));
    }

    /// <summary>
    ///     Coroutine to relay the message from the sender NPC.
    /// </summary>
    /// <param name="message">The message to be relayed.</param>
    /// <param name="npcGroup"> The NPC group to relay the message to. </param>
    /// <returns>An IEnumerator to be used in a coroutine.</returns>
    private IEnumerator RelayMessageCoroutine(string message, NPCGroup npcGroup)
    {
        yield return new WaitForSeconds(0.5f);

        try
        {
            ConvaiGroupNPCController receiver = npcGroup.CurrentSpeaker;
            if (!receiver.CanRelayMessage) yield break;

            Logger.DebugLog($"Relaying message from {npcGroup.CurrentListener.CharacterName} to {receiver.CharacterName}: {message}", Logger.LogCategory.Character);

            string processedMessage = ProcessMessage(npcGroup.CurrentListener, npcGroup.topic, message);
            receiver.SendTextDataNPC2NPC(processedMessage);
        }
        catch (Exception e)
        {
            Logger.Warn($"Failed to relay message: {e.Message}", Logger.LogCategory.Character);
        }
    }

    /// <summary>
    ///     Switches the speaker in the conversation.
    /// </summary>
    /// <param name="currentSpeaker">The current speaker NPC.</param>
    /// <returns>The new speaker NPC.</returns>
    private void SwitchSpeaker(ConvaiGroupNPCController currentSpeaker)
    {
        NPCGroup group = npcGroups.Find(g => g.CurrentSpeaker == currentSpeaker);
        if (group != null)
        {
            group.CurrentSpeaker = currentSpeaker == group.GroupNPC1 ? group.GroupNPC2 : group.GroupNPC1;
            Logger.DebugLog($"Switching NPC2NPC Speaker to {group.CurrentSpeaker}", Logger.LogCategory.Character);
            return;
        }

        Logger.Warn("Failed to switch speaker. Current speaker not found in any group.", Logger.LogCategory.Character);
    }

    private void LoadApiKey()
    {
        ConvaiAPIKeySetup.GetAPIKey(out _apiKey);
    }

    /// <summary>
    ///     Initializes a single NPC for conversation.
    /// </summary>
    /// <param name="npc">The NPC to initialize.</param>
    /// <param name="grpcClient">The GRPC client to use for the NPC.</param>
    private void InitializeNPC(ConvaiGroupNPCController npc, NPC2NPCGRPCClient grpcClient)
    {
        if (npc == null)
        {
            Debug.LogWarning("The given NPC is null.");
            return;
        }

        npc.ConversationManager = this;
        npc.InitializeNpc2NpcGrpcClient(grpcClient);
        npc.AttachSpeechBubble();
        npc.IsInConversationWithAnotherNPC = true;
        npc.ConvaiNPC.isCharacterActive = false;
    }

    /// <summary>
    ///     Starts the conversation for the given NPC.
    /// </summary>
    /// <param name="npcGroup">The NPC to start the conversation for.</param>
    private void InitializeNPCGroup(NPCGroup npcGroup)
    {
        if (npcGroup == null)
        {
            Debug.LogWarning("The given NPC is not part of any group.");
            return;
        }

        ConvaiGroupNPCController npc1 = npcGroup.GroupNPC1;
        ConvaiGroupNPCController npc2 = npcGroup.GroupNPC2;

        if (npc1.IsInConversationWithAnotherNPC || npc2.IsInConversationWithAnotherNPC)
        {
            Debug.LogWarning($"{npc1.CharacterName} or {npc2.CharacterName} is already in a conversation.");
            return;
        }

        NPC2NPCGRPCClient grpcClient = CreateAndInitializeGRPCClient(npcGroup);

        // Initialize both NPCs
        InitializeNPC(npc1, grpcClient);
        InitializeNPC(npc2, grpcClient);

        npcGroup.CurrentSpeaker = Random.Range(0, 10) % 2 == 0 ? npc1 : npc2;
    }

    /// <summary>
    ///     Creates and initializes a new GRPC client for the given NPC group.
    /// </summary>
    /// <param name="group">The NPC group to create the GRPC client for.</param>
    /// <returns>The initialized GRPC client.</returns>
    private NPC2NPCGRPCClient CreateAndInitializeGRPCClient(NPCGroup group)
    {
        GameObject grpcClientGameObject = new($"GRPCClient_{group.GroupNPC1.CharacterID}_{group.GroupNPC2.CharacterID}")
        {
            transform = { parent = transform }
        };

        NPC2NPCGRPCClient grpcClient = grpcClientGameObject.AddComponent<NPC2NPCGRPCClient>();

        ConvaiService.ConvaiServiceClient serviceClient = CreateNewConvaiServiceClient();
        grpcClient.Initialize(_apiKey, serviceClient, group);

        return grpcClient;
    }

    /// <summary>
    ///     Creates a new ConvaiServiceClient.
    /// </summary>
    /// <returns> The new ConvaiServiceClient. </returns>
    private ConvaiService.ConvaiServiceClient CreateNewConvaiServiceClient()
    {
        try
        {
            SslCredentials credentials = new();
            Channel channel = new(GRPC_API_ENDPOINT, credentials);
            return new ConvaiService.ConvaiServiceClient(channel);
        }
        catch (Exception ex)
        {
            Logger.Error($"Failed to create ConvaiServiceClient: {ex.Message}", Logger.LogCategory.Character);
            throw;
        }
    }

    /// <summary>
    ///     Resumes the conversation for the given NPC.
    /// </summary>
    /// <param name="sender"> The NPC to resume the conversation for. </param>
    private void ResumeConversation(ConvaiGroupNPCController sender)
    {
        NPCGroup npcGroup = npcGroups.Find(g => g.BelongToGroup(sender));
        if (npcGroup.IsAnyoneTalking()) return;

        if (_groupsWhereConversationEnded.Contains(npcGroup))
        {
            InitializeNPCGroup(npcGroup);
            _groupsWhereConversationEnded.Remove(npcGroup);
        }

        if (string.IsNullOrEmpty(npcGroup.messageToRelay))
        {
            string message = $"Talk about {npcGroup.topic}.";
            npcGroup.CurrentSpeaker.SendTextDataNPC2NPC(message);
            npcGroup.messageToRelay = message;

            Debug.Log($"Starting conversation for the first time between {npcGroup.GroupNPC1.CharacterName} and {npcGroup.GroupNPC2.CharacterName}");
        }
        else
        {
            RelayMessage(npcGroup.messageToRelay, npcGroup.CurrentSpeaker, false);
            Debug.Log($"Resuming conversation between {npcGroup.GroupNPC1.CharacterName} and {npcGroup.GroupNPC2.CharacterName}");
        }
    }

    /// <summary>
    ///     Ends the conversation for the given NPC.
    /// </summary>
    /// <param name="npc"> The NPC to end the conversation for. </param>
    public void EndConversation(ConvaiGroupNPCController npc)
    {
        NPCGroup group = npcGroups.Find(g => g.BelongToGroup(npc));
        Debug.Log($"Ending conversation between {group.GroupNPC1.CharacterName} and {group.GroupNPC2.CharacterName}");

        void EndConversationForNPC(ConvaiGroupNPCController groupNPC)
        {
            groupNPC.IsInConversationWithAnotherNPC = false;
            groupNPC.ConvaiNPC.InterruptCharacterSpeech();
            groupNPC.GetComponent<ConvaiGroupNPCController>().DetachSpeechBubble();
        }

        EndConversationForNPC(group.GroupNPC1);
        EndConversationForNPC(group.GroupNPC2);
        _groupsWhereConversationEnded.Add(group);
        ConvaiNPCManager.Instance.SetActiveConvaiNPC(npc.ConvaiNPC);

        Destroy(transform.Find($"GRPCClient_{group.GroupNPC1.CharacterID}_{group.GroupNPC2.CharacterID}").gameObject);
    }


    /// <summary>
    ///     Starts the conversation with all NPCs.
    /// </summary>
    private void StartConversationWithAllNPCs()
    {
        IEnumerable filteredList = npcGroups
            .Where(npcGroup => npcGroup.BothNPCAreNotNull())
            .Where(npcGroup => npcGroup.BothNPCAreNotActiveNPC());

        foreach (NPCGroup npcGroup in filteredList)
            InitializeNPCGroup(npcGroup);
    }
}