using UnityEditor;

namespace Convai.Scripts.Narrative_Design.Editor
{
    [CustomEditor(typeof(NarrativeDesignTrigger))]
    public class NarrativeDesignTriggerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            NarrativeDesignTrigger narrativeDesignTrigger = (NarrativeDesignTrigger)target;

            DrawDefaultInspector();

            if (narrativeDesignTrigger.availableTriggers is { Count: > 0 })
                narrativeDesignTrigger.selectedTriggerIndex =
                    EditorGUILayout.Popup("Trigger", narrativeDesignTrigger.selectedTriggerIndex, narrativeDesignTrigger.availableTriggers.ToArray());
        }
    }
}