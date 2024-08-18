using Convai.Scripts;
using Convai.Scripts.Utils;
using UnityEngine;

public class AmyTriggers : MonoBehaviour
{
    [SerializeField] private ConvaiNPC npc;

    public void WarnStartAway()
    {
        ConvaiNPCManager.Instance.SetActiveConvaiNPC(npc);
        npc.TriggerEvent("warn_stay_away");
    }

}
