using Inworld;
using Inworld.Assets;
using Inworld.Packet;
using UnityEngine;

 public class AnimationController : InworldAnimation
{
    [SerializeField] Animator animator;
    static readonly int s_Emotion = Animator.StringToHash("Emotion");
    static readonly int s_Gesture = Animator.StringToHash("Gesture");
    static readonly int s_Motion = Animator.StringToHash("MainStatus");
    static readonly int s_RemainSec = Animator.StringToHash("RemainSec");
    static readonly int s_Random = Animator.StringToHash("Random");

    protected override void Awake()
    {
        base.Awake();
    }

    protected virtual void HandleMainStatus(AnimMainStatus status) => animator.SetInteger(s_Motion, (int)status);

    protected override void HandleEmotion(EmotionPacket packet)
    {
        animator.SetFloat(s_Random, Random.Range(0, 1) > 0.5f ? 1 : 0);
        animator.SetFloat(s_RemainSec, m_Interaction.AnimFactor);
        ProcessEmotion(packet.emotion.behavior.ToUpper());
    }

    private void ProcessEmotion(string emotionBehavior)
    {
        EmotionMapData emoMapData = m_EmotionMap[emotionBehavior];
        if (emoMapData == null)
        {
            InworldAI.LogError($"Unhandled emotion {emotionBehavior}");
            return;
        }
        animator.SetInteger(s_Emotion, (int)emoMapData.bodyEmotion);
        animator.SetInteger(s_Gesture, (int)emoMapData.bodyGesture);
    }
}

