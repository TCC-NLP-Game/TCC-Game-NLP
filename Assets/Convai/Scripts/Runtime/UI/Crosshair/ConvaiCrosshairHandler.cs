using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Convai.Scripts.Utils
{
    /// <summary>
    ///     Handles the crosshair behavior for the Convai application.
    ///     It can detect which Convai game object the player's crosshair is currently looking at.
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Convai/Crosshair Handler")]
    [HelpURL("https://docs.convai.com/api-docs/plugins-and-integrations/unity-plugin/scripts-overview/convaicrosshairhandler.cs")]
    public class ConvaiCrosshairHandler : MonoBehaviour
    {
        private Camera _camera;
        private Dictionary<GameObject, string> _interactableReferences;
        private ConvaiInteractablesData _interactablesData;

        private void Awake()
        {
            _camera = Camera.main;

            _interactablesData = FindObjectOfType<ConvaiInteractablesData>();
            if (_interactablesData == null) return;
            // Build the interactable references dictionary
            _interactableReferences = new Dictionary<GameObject, string>();
            foreach (ConvaiInteractablesData.Object eachObject in _interactablesData.Objects)
                _interactableReferences[eachObject.gameObject] = eachObject.Name;
            foreach (ConvaiInteractablesData.Character eachCharacter in _interactablesData.Characters)
                _interactableReferences[eachCharacter.gameObject] = eachCharacter.Name;
        }

        /// <summary>
        ///     Finds the reference object that the player's crosshair is currently looking at.
        /// </summary>
        /// <returns>A reference string of the interactable object or character, "None" if no valid hit.</returns>
        public string FindPlayerReferenceObject()
        {
            if (_interactablesData == null || _camera == null) return "None";

            Vector3 centerOfScreen = new(0.5f, 0.5f, 0);
            if (Physics.Raycast(_camera.ViewportPointToRay(centerOfScreen), out RaycastHit hit))
            {
                _interactablesData.DynamicMoveTargetIndicator.position = hit.point;
                string reference = FindInteractableReference(hit.transform.gameObject);
                Logger.DebugLog($"Player is looking at: {reference}", Logger.LogCategory.Actions);
                return reference;
            }

            return "None";
        }

        /// <summary>
        ///     Finds the reference object that the player's crosshair is currently looking at.
        /// </summary>
        /// <param name="hitGameObject"> The game object that the player's crosshair is currently looking at.</param>
        /// <returns> A reference string(name) of the interactable object or character, "None" if no valid hit.</returns>
        private string FindInteractableReference(GameObject hitGameObject)
        {
            foreach (KeyValuePair<GameObject, string> kvp in _interactableReferences.Where(kvp => hitGameObject.GetComponentInParent<Transform>() == kvp.Key.transform))
                return kvp.Value;

            return "None";
        }
    }
}