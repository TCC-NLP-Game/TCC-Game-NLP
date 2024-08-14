using System;
using UnityEngine;

namespace Convai.Scripts.Utils
{
    [DefaultExecutionOrder(-101)]
    public class ConvaiNPCManager : MonoBehaviour
    {
        [Tooltip("Reference to the currently active NPC.")] [ReadOnly]
        public ConvaiNPC activeConvaiNPC;

        [Tooltip("Reference to the NPC that is currently near the player.")] [ReadOnly]
        public ConvaiNPC nearbyNPC;

        // Singleton instance of the NPC manager.
        public static ConvaiNPCManager Instance { get; private set; }

        private void Awake()
        {
            // Singleton pattern to ensure only one instance exists
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);
        }

        /// <summary>
        ///     Checks if the specified NPC is in conversation with another NPC.
        /// </summary>
        /// <param name="npc">The NPC to check.</param>
        /// <returns>True if the NPC is in conversation with another NPC; otherwise, false.</returns>
        public bool CheckForNPCToNPCConversation(ConvaiNPC npc)
        {
            return npc.TryGetComponent(out ConvaiGroupNPCController convaiGroupNPC) && convaiGroupNPC.IsInConversationWithAnotherNPC;
        }


        /// <summary>
        ///     Sets the active NPC to the specified NPC.
        /// </summary>
        /// <param name="newActiveNPC">The NPC to set as active.</param>
        public void SetActiveConvaiNPC(ConvaiNPC newActiveNPC)
        {
            if (activeConvaiNPC != newActiveNPC)
            {
                if (activeConvaiNPC != null)
                    // Deactivate the previous NPC
                    activeConvaiNPC.isCharacterActive = false;

                activeConvaiNPC = newActiveNPC;

                if (newActiveNPC != null)
                {
                    // Activate the new NPC
                    newActiveNPC.isCharacterActive = true;
                    Debug.Log($"Active NPC changed to {newActiveNPC.gameObject.name}");
                }

                OnActiveNPCChanged?.Invoke(newActiveNPC);
            }
        }

        /// <summary>
        ///     Event that's triggered when the active NPC changes.
        /// </summary>
        public event Action<ConvaiNPC> OnActiveNPCChanged;

        /// <summary>
        ///     Gets the currently active ConvaiNPC.
        /// </summary>
        /// <returns>The currently active ConvaiNPC.</returns>
        public ConvaiNPC GetActiveConvaiNPC()
        {
            return activeConvaiNPC;
        }
    }
}