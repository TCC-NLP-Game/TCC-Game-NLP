using System.Collections.Generic;
using System.Threading.Tasks;
using Convai.Scripts.Narrative_Design.Models;
using UnityEditor;
using UnityEngine;

namespace Convai.Scripts.Narrative_Design.Editor
{
    [CustomEditor(typeof(NarrativeDesignManager))]
    public class NarrativeDesignManagerEditor : UnityEditor.Editor
    {
        /// Dictionary to keep track of which sections are expanded in the editor
        private readonly Dictionary<string, bool> _sectionIdExpanded = new();

        /// Reference to the NarrativeDesignManager that this editor is modifying
        private NarrativeDesignManager _narrativeDesignManager;

        /// SerializedProperty for the section change events in the NarrativeDesignManager
        private SerializedProperty _sectionChangeEvents;

        /// Whether the section events are expanded in the editor
        private bool _sectionEventsExpanded = true;

        /// SerializedObject for the target object
        private SerializedObject _serializedObject;

        private void OnEnable()
        {
            _serializedObject = new SerializedObject(target);
            _narrativeDesignManager = target as NarrativeDesignManager;

            if (_narrativeDesignManager != null) FindProperties();
        }

        public override void OnInspectorGUI()
        {
            _serializedObject.Update();

            if (GUILayout.Button("Check for Updates")) OnUpdateNarrativeDesignButtonClicked();

            GUILayout.Space(10);

            if (_narrativeDesignManager.sectionDataList.Count > 0)
            {
                _sectionEventsExpanded = EditorGUILayout.Foldout(_sectionEventsExpanded, "Section Events", true, EditorStyles.foldoutHeader);

                if (_sectionEventsExpanded)
                {
                    EditorGUI.indentLevel++;

                    EditorGUI.BeginChangeCheck();

                    for (int i = 0; i < _narrativeDesignManager.sectionDataList.Count; i++)
                    {
                        SectionData sectionData = _narrativeDesignManager.sectionDataList[i];
                        string sectionId = sectionData.sectionId;

                        SectionChangeEventsData sectionChangeEventsData = _narrativeDesignManager.sectionChangeEventsDataList.Find(x => x.id == sectionId);

                        if (sectionChangeEventsData == null)
                        {
                            sectionChangeEventsData = new SectionChangeEventsData { id = sectionId };
                            _narrativeDesignManager.sectionChangeEventsDataList.Add(sectionChangeEventsData);
                        }

                        _sectionIdExpanded.TryAdd(sectionId, false);

                        GUIStyle sectionIdStyle = new(EditorStyles.foldoutHeader)
                        {
                            fontStyle = FontStyle.Bold,
                            fontSize = 14
                        };

                        string sectionIdText = $"{sectionData.sectionName} - {sectionId}";
                        _sectionIdExpanded[sectionId] = EditorGUILayout.Foldout(_sectionIdExpanded[sectionId], sectionIdText, true, sectionIdStyle);

                        if (_sectionIdExpanded[sectionId])
                        {
                            EditorGUI.indentLevel++;

                            SerializedProperty sectionChangeEventsProperty = _sectionChangeEvents.GetArrayElementAtIndex(i);
                            EditorGUILayout.PropertyField(sectionChangeEventsProperty, GUIContent.none, true);

                            EditorGUI.indentLevel--;
                        }
                    }

                    if (EditorGUI.EndChangeCheck())
                    {
                        _serializedObject.ApplyModifiedProperties();
                        _narrativeDesignManager.OnSectionEventListChange();
                    }

                    EditorGUI.indentLevel--;
                }
            }

            _serializedObject.ApplyModifiedProperties();
        }

        private async void OnUpdateNarrativeDesignButtonClicked()
        {
            await Task.WhenAll(_narrativeDesignManager.UpdateSectionListAsync(), _narrativeDesignManager.UpdateTriggerListAsync());
            _serializedObject.ApplyModifiedProperties();
            _narrativeDesignManager.OnSectionEventListChange();
        }

        private void FindProperties()
        {
            _serializedObject.FindProperty(nameof(NarrativeDesignManager.sectionDataList));
            _serializedObject.FindProperty(nameof(NarrativeDesignManager.triggerDataList));
            _sectionChangeEvents = _serializedObject.FindProperty(nameof(NarrativeDesignManager.sectionChangeEventsDataList));
        }
    }
}