using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Service;
using UnityEngine;


namespace Convai.Scripts.Utils.LipSync
{

    /// <summary>
    /// This Class will serve as a base for any method of Lipsync that Convai will develop or use
    /// </summary>
    public abstract class ConvaiLipSyncApplicationBase : MonoBehaviour
    {
        /// <summary>
        /// This stores a dictionary of blendshape name and index of the Blendweight it will affect
        /// </summary>
        protected Dictionary<string, int> _headMapping;
        /// <summary>
        /// Reference to the Head Skin Mesh Renderer used for lipsync
        /// </summary>
        protected SkinnedMeshRenderer _headSkinMeshRenderer;
        /// <summary>
        /// Reference to the Teeth Skin Mesh Renderer used for lipsync
        /// </summary>
        protected SkinnedMeshRenderer _teethSkinMeshRenderer;
        /// <summary>
        /// Reference to the Jaw bone gameobject used for lipsync
        /// </summary>
        private GameObject _jawBone;
        /// <summary>
        /// Reference to the Tongue bone gameobject used for lipsync
        /// </summary>
        private GameObject _tongueBone;
        /// <summary>
        /// Reference to the NPC on which lipsync will be applied
        /// </summary>
        protected ConvaiNPC _convaiNPC;
        protected float _weightMultiplier { get; private set; }
        #region Null States of References
        protected bool HasHeadSkinnedMeshRenderer { get; private set; }
        protected bool HasTeethSkinnedMeshRenderer { get; private set; }
        protected bool HasJawBone { get; private set; }
        protected bool HasTongueBone { get; private set; }
        #endregion
        /// <summary>
        /// Initializes and setup up of the things necessary for lipsync to work
        /// </summary>
        /// <param name="convaiLipSync"></param>
        /// <param name="convaiNPC"></param>
        public virtual void Initialize(ConvaiLipSync convaiLipSync, ConvaiNPC convaiNPC)
        {
            _headSkinMeshRenderer = convaiLipSync.HeadSkinnedMeshRenderer;
            HasHeadSkinnedMeshRenderer = _headSkinMeshRenderer != null;

            _teethSkinMeshRenderer = convaiLipSync.TeethSkinnedMeshRenderer;
            HasTeethSkinnedMeshRenderer = _teethSkinMeshRenderer != null;

            _jawBone = convaiLipSync.jawBone;
            HasJawBone = _jawBone != null;

            _tongueBone = convaiLipSync.tongueBone;
            HasTongueBone = _tongueBone != null;

            _convaiNPC = convaiNPC;
            _weightMultiplier = convaiLipSync != null ? convaiLipSync.WeightMultiplier : 1;

            if (HasHeadSkinnedMeshRenderer)
                _headMapping = SetupMapping(GetHeadRegexMapping, _headSkinMeshRenderer);
        }
        /// <summary>
        /// Creates the mapping of blendshape and index it affects during lipsync
        /// </summary>
        protected Dictionary<string, int> SetupMapping(Func<Dictionary<string, string>> finder, SkinnedMeshRenderer skinnedMeshRenderer)
        {
            Dictionary<string, int> mapping = new Dictionary<string, int>();
            Dictionary<string, string> regexMapping = finder();

            foreach (KeyValuePair<string, string> pair in regexMapping)
            {
                for (int i = 0; i < skinnedMeshRenderer.sharedMesh.blendShapeCount; i++)
                {
                    string blendShapeName = skinnedMeshRenderer.sharedMesh.GetBlendShapeName(i);
                    Regex regex = new(pair.Value);
                    if (regex.IsMatch(blendShapeName))
                    {
                        mapping.TryAdd(pair.Key, i);
                    }
                }
            }
            return mapping;
        }
        /// <summary>
        /// Returns a dictionary of blendshape name and regex string used to find the index
        /// TODO Modify the override to fit your version of the mapping
        /// </summary>
        /// <returns></returns>
        protected virtual Dictionary<string, string> GetHeadRegexMapping()
        {
            return new Dictionary<string, string>();
        }
        /// <summary>
        /// Updates the tongue bone rotation to the new rotation
        /// </summary>
        /// <param name="newRotation"></param>
        protected void UpdateTongueBoneRotation(Vector3 newRotation)
        {
            if (!HasTongueBone) return;
            _tongueBone.transform.localEulerAngles = newRotation;
        }
        /// <summary>
        /// Updates the jaw bone rotation to the new rotation
        /// </summary>
        /// <param name="newRotation"></param>
        protected void UpdateJawBoneRotation(Vector3 newRotation)
        {
            if (!HasJawBone) return;
            _jawBone.transform.localEulerAngles = newRotation;
        }
        /// <summary>
        /// Updates the current blendshape or visemes frame
        /// </summary>
        protected abstract void UpdateBlendShape();
        /// <summary>
        /// This removes the excess frames in the queue
        /// </summary>
        public abstract void PurgeExcessBlendShapeFrames();
        /// <summary>
        /// This resets the whole queue of the frames
        /// </summary>
        protected bool CanPurge<T>(Queue<T> queue)
        {
            // ? Should I hardcode the limiter for this check
            return queue.Count < 10;
        }
        public abstract void ClearQueue();
        /// <summary>
        /// Adds blendshape frames in the queue
        /// </summary>
        /// <param name="blendshapeFrames"></param>
        public virtual void EnqueueQueue(Queue<BlendshapeFrame> blendshapeFrames) { }
        /// <summary>
        /// Adds Visemes frames in the list
        /// </summary>
        /// <param name="visemesFrames"></param>
        public virtual void EnqueueQueue(Queue<VisemesData> visemesFrames) { }
        /// <summary>
        /// Adds a blendshape frame in the last queue
        /// </summary>
        /// <param name="blendshapeFrame"></param>
        public virtual void EnqueueFrame(BlendshapeFrame blendshapeFrame) { }
        /// <summary>
        /// Adds a viseme frame to the last element of the list
        /// </summary>
        /// <param name="viseme"></param>
        public virtual void EnqueueFrame(VisemesData viseme) { }
    }
}
