using System;
using System.Text.RegularExpressions;
using Convai.Scripts.Utils.LipSync.Types;
using Service;
using UnityEngine;

namespace Convai.Scripts.Utils.LipSync
{
    public class ConvaiLipSync : MonoBehaviour
    {
        public enum LipSyncBlendshapeType
        {
            None, // Default Value
            OVR, // Oculus 
            ReallusionPlus, // Reallusion Extended
            ARKit, // AR Kit - Translated from Oculus
        }

        [Tooltip(
            "The type of facial blend-shapes in the character. Select OVR for Oculus and ReallusionPlus for Reallusion Extended visemes.")]
        public LipSyncBlendshapeType BlendshapeType = LipSyncBlendshapeType.OVR;
        [Tooltip("Skinned Mesh Renderer Component for the head of the character.")]
        public SkinnedMeshRenderer HeadSkinnedMeshRenderer;
        [Tooltip("Skinned Mesh Renderer Component for the teeth of the character, if available. Leave empty if not.")]
        public SkinnedMeshRenderer TeethSkinnedMeshRenderer;
        [Tooltip("Skinned Mesh Renderer Component for the tongue of the character, if available. Leave empty if not.")]
        public SkinnedMeshRenderer TongueSkinnedMeshRenderer;
        [Tooltip("Game object with the bone of the jaw for the character, if available. Leave empty if not.")]
        public GameObject jawBone;
        [Tooltip("Game object with the bone of the tongue for the character, if available. Leave empty if not.")]
        public GameObject tongueBone; // even though actually tongue doesn't have a bone

        [HideInInspector]
        public FaceModel faceModel = FaceModel.OvrModelName;
        [Tooltip("The index of the first blendshape that will be manipulated.")]
        public int firstIndex;
        [Tooltip("This will multiply the weights of the incoming frames to the lipsync")]
        [field: SerializeField] public float WeightMultiplier { get; private set; } = 1f;

        private ConvaiNPC _convaiNPC;
        public event Action<bool> OnCharacterLipSyncing;
        private ConvaiLipSyncApplicationBase convaiLipSyncApplicationBase;
        public ConvaiLipSyncApplicationBase ConvaiLipSyncApplicationBase { get => convaiLipSyncApplicationBase; private set => convaiLipSyncApplicationBase = value; }

        private void Awake()
        {
            switch (BlendshapeType)
            {
                case LipSyncBlendshapeType.None:
                    break;
                case LipSyncBlendshapeType.OVR:
                    ConvaiLipSyncApplicationBase = gameObject.GetOrAddComponent<ConvaiOVRLipsync>();
                    break;
                case LipSyncBlendshapeType.ReallusionPlus:
                    ConvaiLipSyncApplicationBase = gameObject.GetOrAddComponent<ConvaiReallusionLipSync>();
                    break;
                case LipSyncBlendshapeType.ARKit:
                    ConvaiLipSyncApplicationBase = gameObject.GetOrAddComponent<ConvaiARKitLipSync>();
                    break;
            }
        }

        /// <summary>
        ///     This function will automatically set any of the unassigned skinned mesh renderers
        ///     to appropriate values using regex based functions.
        ///     It also invokes the LipSyncCharacter() function every one hundredth of a second.
        /// </summary>
        private void Start()
        {
            // regex search for SkinnedMeshRenderers: head, teeth, tongue
            if (HeadSkinnedMeshRenderer == null)
                HeadSkinnedMeshRenderer = GetHeadSkinnedMeshRendererWithRegex(transform);
            if (TeethSkinnedMeshRenderer == null)
                TeethSkinnedMeshRenderer = GetTeethSkinnedMeshRendererWithRegex(transform);
            if (TongueSkinnedMeshRenderer == null)
                TongueSkinnedMeshRenderer = GetTongueSkinnedMeshRendererWithRegex(transform);

            _convaiNPC = GetComponent<ConvaiNPC>();
            ConvaiLipSyncApplicationBase.Initialize(this, _convaiNPC);
            SetCharacterLipSyncing(true);
        }
        /// <summary>
        /// Fires an event with update the Character Lip Syncing State
        /// </summary>
        /// <param name="value"></param>
        private void SetCharacterLipSyncing(bool value)
        {
            OnCharacterLipSyncing?.Invoke(value);
        }


        private void OnApplicationQuit()
        {
            StopLipSync();
        }

        /// <summary>
        ///     This function finds the Head skinned mesh renderer components, if present,
        ///     in the children of the parentTransform using regex.
        /// </summary>
        /// <param name="parentTransform">The parent transform whose children are searched.</param>
        /// <returns>The SkinnedMeshRenderer component of the Head, if found; otherwise, null.</returns>
        private SkinnedMeshRenderer GetHeadSkinnedMeshRendererWithRegex(Transform parentTransform)
        {
            // Initialize a variable to store the found SkinnedMeshRenderer.
            SkinnedMeshRenderer findFaceSkinnedMeshRenderer = null;

            // Define a regular expression pattern for matching child object names.
            Regex regexPattern = new("(.*_Head|CC_Base_Body)");

            // Iterate through each child of the parentTransform.
            foreach (Transform child in parentTransform)
                // Check if the child's name matches the regex pattern.
                if (regexPattern.IsMatch(child.name))
                {
                    // If a match is found, get the SkinnedMeshRenderer component of the child.
                    findFaceSkinnedMeshRenderer = child.GetComponent<SkinnedMeshRenderer>();

                    // If a SkinnedMeshRenderer is found, break out of the loop.
                    if (findFaceSkinnedMeshRenderer != null) break;
                }

            // Return the found SkinnedMeshRenderer (or null if none is found).
            return findFaceSkinnedMeshRenderer;
        }


        /// <summary>
        ///     This function finds the Teeth skinned mesh renderer components, if present,
        ///     in the children of the parentTransform using regex.
        /// </summary>
        /// <param name="parentTransform">The parent transform whose children are searched.</param>
        /// <returns>The SkinnedMeshRenderer component of the Teeth, if found; otherwise, null.</returns>
        private SkinnedMeshRenderer GetTeethSkinnedMeshRendererWithRegex(Transform parentTransform)
        {
            // Initialize a variable to store the found SkinnedMeshRenderer for teeth.
            SkinnedMeshRenderer findTeethSkinnedMeshRenderer = null;

            // Define a regular expression pattern for matching child object names.
            Regex regexPattern = new("(.*_Teeth|CC_Base_Body)");

            // Iterate through each child of the parentTransform.
            foreach (Transform child in parentTransform)
                // Check if the child's name matches the regex pattern.
                if (regexPattern.IsMatch(child.name))
                {
                    // If a match is found, get the SkinnedMeshRenderer component of the child.
                    findTeethSkinnedMeshRenderer = child.GetComponent<SkinnedMeshRenderer>();

                    // If a SkinnedMeshRenderer is found, break out of the loop.
                    if (findTeethSkinnedMeshRenderer != null) break;
                }

            // Return the found SkinnedMeshRenderer for teeth (or null if none is found).
            return findTeethSkinnedMeshRenderer;
        }


        /// <summary>
        ///     This function finds the Tongue skinned mesh renderer components, if present,
        ///     in the children of the parentTransform using regex.
        /// </summary>
        /// <param name="parentTransform">The parent transform whose children are searched.</param>
        /// <returns>The SkinnedMeshRenderer component of the Tongue, if found; otherwise, null.</returns>
        private SkinnedMeshRenderer GetTongueSkinnedMeshRendererWithRegex(Transform parentTransform)
        {
            // Initialize a variable to store the found SkinnedMeshRenderer for the tongue.
            SkinnedMeshRenderer findTongueSkinnedMeshRenderer = null;

            // Define a regular expression pattern for matching child object names.
            Regex regexPattern = new("(.*_Tongue|CC_Base_Body)");

            // Iterate through each child of the parentTransform.
            foreach (Transform child in parentTransform)
                // Check if the child's name matches the regex pattern.
                if (regexPattern.IsMatch(child.name))
                {
                    // If a match is found, get the SkinnedMeshRenderer component of the child.
                    findTongueSkinnedMeshRenderer = child.GetComponent<SkinnedMeshRenderer>();

                    // If a SkinnedMeshRenderer is found, break out of the loop.
                    if (findTongueSkinnedMeshRenderer != null) break;
                }

            // Return the found SkinnedMeshRenderer for the tongue (or null if none is found).
            return findTongueSkinnedMeshRenderer;
        }
        /// <summary>
        /// Purges the latest chuck of lipsync frames
        /// </summary>
        public void PurgeExcessFrames()
        {
            ConvaiLipSyncApplicationBase?.PurgeExcessBlendShapeFrames();
        }
        /// <summary>
        /// Stops the Lipsync by clearing the frames queue
        /// </summary>
        public void StopLipSync()
        {
            ConvaiLipSyncApplicationBase?.ClearQueue();
        }
    }
}