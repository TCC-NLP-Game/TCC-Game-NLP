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
            switch (_frameType)
            {
                case FrameType.Visemes:
                    npc.convaiLipSync.ConvaiLipSyncApplicationBase.EnqueueQueue(new Queue<VisemesData>(_visemesFrames));
                    break;
                case FrameType.Blendshape:
                    npc.convaiLipSync.ConvaiLipSyncApplicationBase.EnqueueQueue(new Queue<BlendshapeFrame>(_blendShapeFrames));
                    break;
            }

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

            switch (_frameType)
            {
                case FrameType.Visemes:
                    while (_visemesFrames.Count != 0)
                    {
                        npc.convaiLipSync.ConvaiLipSyncApplicationBase.EnqueueFrame(_visemesFrames.Dequeue());
                    }
                    break;
                case FrameType.Blendshape:
                    while (_blendShapeFrames.Count != 0)
                    {
                        npc.convaiLipSync.ConvaiLipSyncApplicationBase.EnqueueFrame(_blendShapeFrames.Dequeue());
                    }
                    break;
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