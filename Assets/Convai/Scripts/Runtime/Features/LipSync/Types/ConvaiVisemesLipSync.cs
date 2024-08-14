using System.Collections.Generic;
using Service;
using UnityEngine;
namespace Convai.Scripts.Utils.LipSync.Types
{
    public abstract class ConvaiVisemesLipSync : ConvaiLipSyncApplicationBase
    {
        private const float FRAMERATE = 1f / 100.0f;
        protected Queue<Queue<VisemesData>> _visemesDataQueue = new Queue<Queue<VisemesData>>();
        protected Viseme _currentViseme;
        protected List<string> _possibleCombinations = new List<string>();

        public override void Initialize(ConvaiLipSync convaiLipSync, ConvaiNPC convaiNPC)
        {
            base.Initialize(convaiLipSync, convaiNPC);
            _possibleCombinations = new List<string> { "", "R", "U", "L", "O" };
            InvokeRepeating(nameof(UpdateBlendShape), 0, FRAMERATE);
        }

        public override void ClearQueue()
        {
            _visemesDataQueue = new Queue<Queue<VisemesData>>();
            _currentViseme = new Viseme();
        }

        public override void PurgeExcessBlendShapeFrames()
        {
            if (_visemesDataQueue.Count == 0) return;
            if (!CanPurge<VisemesData>(_visemesDataQueue.Peek())) return;
            Logger.Info($"Purging {_visemesDataQueue.Peek().Count} Frames", Logger.LogCategory.LipSync);
            _visemesDataQueue.Dequeue();
        }



        protected override void UpdateBlendShape()
        {
            if (_visemesDataQueue == null || _visemesDataQueue.Count <= 0)
            {
                _currentViseme = new Viseme();
                return;
            }
            // Dequeue the next frame of visemes data from the faceDataList.
            if (_visemesDataQueue.Peek().Count <= 0 || _visemesDataQueue.Peek() == null)
            {
                _visemesDataQueue.Dequeue();
                return;
            }
            if (!_convaiNPC.IsCharacterTalking) return;

            _currentViseme = _visemesDataQueue.Peek().Dequeue().Visemes;
        }

        protected void FindAndUpdateBlendWeight(SkinnedMeshRenderer renderer, string fieldName, float value, List<int> knownIndexs, Dictionary<string, int> mapping)
        {
            if (mapping.TryGetValue(fieldName, out int index))
            {
                if (!knownIndexs.Contains(index))
                {
                    knownIndexs.Add(index);
                    UpdateWeight(renderer, index, value, true);
                }
                else
                {
                    UpdateWeight(_headSkinMeshRenderer, index, value, false);
                }
            }
        }

        protected virtual void UpdateWeight(SkinnedMeshRenderer renderer, int index, float value, bool firstTime)
        {
            if (value == 0f)
            {
                renderer.SetBlendShapeWeight(index, 0);
                return;
            }

            if (FRAMERATE > Time.deltaTime)
            {
                renderer.SetBlendShapeWeight(index, (firstTime ? 0 : renderer.GetBlendShapeWeight(index)) + value);
            }
            else
            {
                renderer.SetBlendShapeWeightInterpolate(index, (firstTime ? 0 : renderer.GetBlendShapeWeight(index)) + value, Time.deltaTime);
            }
        }

        public override void EnqueueQueue(Queue<VisemesData> visemesFrames)
        {
            _visemesDataQueue.Enqueue(visemesFrames);
        }

        public override void EnqueueFrame(VisemesData viseme)
        {
            if (_visemesDataQueue.Count == 0)
            {
                EnqueueQueue(new Queue<VisemesData>());
            }
            _visemesDataQueue.Peek().Enqueue(viseme);
        }
    }
}