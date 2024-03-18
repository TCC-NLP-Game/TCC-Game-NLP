using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class NPCInteractable : MonoBehaviour
{
    private DialogueManager dialogueManager;

    private void Start()
    {
       dialogueManager = GameManager.Instance.dialogueManager;
    }

    public void Interact()
    {
        GameObject tempLookAt = Instantiate(new GameObject("TempLookAt"));
        tempLookAt.transform.position = transform.position;
        tempLookAt.transform.position += new Vector3(0f, 1f, 0f);
        dialogueManager.OpenChat(tempLookAt.transform);
    }
}
