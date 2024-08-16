using UnityEngine;

public class ConvaiSpeechBubbleController : MonoBehaviour
{
    ConvaiGroupNPCController _convaiGroupNPC;
    NPCSpeechBubble _speechBubble;
    public void Initialize(NPCSpeechBubble speechBubbleDisplay, ConvaiGroupNPCController convaiGroupNPC)
    {
        if (_speechBubble != null) return;
        _speechBubble = Instantiate(speechBubbleDisplay, transform);
        _convaiGroupNPC = convaiGroupNPC;
        _convaiGroupNPC.ShowSpeechBubble += ConvaiNPC_ShowSpeechBubble;
        _convaiGroupNPC.HideSpeechBubble += ConvaiNPC_HideSpeechBubble;
    }

    private void ConvaiNPC_HideSpeechBubble()
    {
        _speechBubble.HideSpeechBubble();
    }

    private void ConvaiNPC_ShowSpeechBubble(string text)
    {
        _speechBubble.ShowSpeechBubble(text);
    }

    void OnDestroy()
    {
        _convaiGroupNPC.ShowSpeechBubble -= ConvaiNPC_ShowSpeechBubble;
        _convaiGroupNPC.HideSpeechBubble -= ConvaiNPC_HideSpeechBubble;
        Destroy(_speechBubble.gameObject);
        _speechBubble = null;
    }
}