using Convai.Scripts;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public static PlayerInventory Instance { get; private set; }
    protected bool hasLetter = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            return;
        }
        Destroy(gameObject);
    }

    public void AddToInventory()
    {
        hasLetter = true;
    }

    public bool PlayerHasLetter ()
    {
        return hasLetter;
    }

    public void GiveLetter(ConvaiNPC currentNPC)
    {
        currentNPC.TriggerEvent("receive_letter");
    }
}
