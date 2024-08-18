using Convai.Scripts;
using Convai.Scripts.Utils;
using UnityEngine;

public class MouseyTriggers : MonoBehaviour
{
    [SerializeField] private ConvaiNPC npc;

    public void WarnStartAway()
    {
        ConvaiNPCManager.Instance.SetActiveConvaiNPC(npc);
        npc.TriggerEvent("warn_stay_away");
    }

    public void SecretWord()
    {
        ConvaiNPCManager.Instance.SetActiveConvaiNPC(npc);
        npc.TriggerEvent("secret_word");
    }

}
