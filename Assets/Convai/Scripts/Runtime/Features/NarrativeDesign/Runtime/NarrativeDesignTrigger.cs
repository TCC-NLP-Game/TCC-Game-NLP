using System.Collections.Generic;
using System.Linq;
using Convai.Scripts.Utils;
using UnityEngine;
using UnityEngine.Events;

namespace Convai.Scripts.Narrative_Design
{
    public class NarrativeDesignTrigger : MonoBehaviour
    {
        public ConvaiNPC convaiNPC;
        [HideInInspector] public int selectedTriggerIndex;
        [HideInInspector] public List<string> availableTriggers;
        public UnityEvent onTriggerEvent;

        private void Awake()
        {
            onTriggerEvent.AddListener(InvokeSelectedTrigger);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player")) InvokeSelectedTrigger();
        }


        private void OnValidate()
        {
            availableTriggers = null;

            if (convaiNPC != null)
            {
                NarrativeDesignManager narrativeDesignManager = convaiNPC.GetComponent<NarrativeDesignManager>();
                if (narrativeDesignManager != null) availableTriggers = narrativeDesignManager.triggerDataList.Select(trigger => trigger.triggerName).ToList();
            }
        }

        /// <summary>
        ///   Invokes the selected trigger.
        /// </summary>
        public void InvokeSelectedTrigger()
        {
            if (convaiNPC != null && availableTriggers != null && selectedTriggerIndex >= 0 && selectedTriggerIndex < availableTriggers.Count)
            {
                string selectedTriggerName = availableTriggers[selectedTriggerIndex];
                ConvaiNPCManager.Instance.SetActiveConvaiNPC(convaiNPC);
                convaiNPC.TriggerEvent(selectedTriggerName);
            }
        }
    }
}