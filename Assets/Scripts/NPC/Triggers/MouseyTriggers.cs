using Convai.Scripts;
using Convai.Scripts.Utils;
using UnityEngine;

public class MouseyTriggers : MonoBehaviour
{
    [SerializeField] private ConvaiNPC npc;

    public void WarnStartAway()
    {
        ConvaiNPCManager.Instance.SetActiveConvaiNPC(npc);
        npc.TriggerEvent("Warn Stay Away");
    }

    public void SecretWord()
    {
        ConvaiNPCManager.Instance.SetActiveConvaiNPC(npc);
        npc.TriggerEvent("Secret Word");
    }

}
