using Inworld.Interactions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class NPCInteractable : InworldInteraction
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


    protected override IEnumerator InteractionCoroutine()
    {
        while (true)
        {
            yield return HandleNextUtterance();
        }
    }
}
