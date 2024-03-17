using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCInteractable : MonoBehaviour
{
    private DialogueManager dialogueManager;

    private void Start()
    {
       dialogueManager = GameManager.Instance.dialogueManager;
    }

    public void Interact()
    {
        dialogueManager.OpenChat();
        Debug.Log("Interact!");
    }
}
