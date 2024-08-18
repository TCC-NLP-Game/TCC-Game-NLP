using System.Collections.Generic;
using Service;
using UnityEngine;

namespace Convai.Scripts.Utils.LipSync
{
    public class LipSyncBlendFrameData
    {
        public enum FrameType
        {
            Visemes,
            Blendshape
        }
        private readonly int _totalFrames;
        private readonly Queue<BlendshapeFrame> _blendShapeFrames = new Queue<BlendshapeFrame>();
        private readonly Queue<VisemesData> _visemesFrames = new Queue<VisemesData>();
        private readonly GetResponseResponse _getResponseResponse;
        private readonly FrameType _frameType;

        private int _framesCaptured;
        private bool _partiallyProcessed;

        public LipSyncBlendFrameData(int totalFrames, GetResponseResponse response, FrameType frameType)
        {
            _totalFrames = totalFrames;
            _framesCaptured = 0;
            _getResponseResponse = response;
            _frameType = frameType;
            //Logger.DebugLog($"Total Frames: {_totalFrames} | {response.AudioResponse.TextData}", Logger.LogCategory.LipSync);
        }

        public void Enqueue(BlendshapeFrame blendShapeFrame)
        {
            _blendShapeFrames.Enqueue(blendShapeFrame);
            _framesCaptured++;
        }

        public void Enqueue(VisemesData visemesData)
        {
            _visemesFrames.Enqueue(visemesData);
        }

        public void Process(ConvaiNPC npc)
        {
            if (!_partiallyProcessed)
                npc.EnqueueResponse(_getResponseResponse);
            npc.AudioManager.SetWaitForCharacterLipSync(false);
        }

        public void ProcessPartially(ConvaiNPC npc)
        {
            if (!_partiallyProcessed)
            {
                _partiallyProcessed = true;
                npc.EnqueueResponse(_getResponseResponse);
                npc.AudioManager.SetWaitForCharacterLipSync(false);
            }
        }

        public bool CanPartiallyProcess()
        {
            return _framesCaptured > Mathf.Min(21, _totalFrames * 0.7f);
        }

        public bool CanProcess()
        {
            return _framesCaptured == _totalFrames;
        }
    }
}