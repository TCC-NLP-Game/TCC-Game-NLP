using Inworld;
using Inworld.Interactions;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CapsuleCollider))]
public class NPCInteractable : InworldInteraction
{
    private DialogueManager dialogueManager;

    private void Start()
    {
       dialogueManager = GameManager.Instance.dialogueManager;
    }

    public void Interact()
    {
        InworldController.CurrentCharacter = GetComponent<InworldCharacter>();
        dialogueManager.OpenChat();
    }

    protected override IEnumerator InteractionCoroutine()
    {
        while (true)
        {
            yield return RemoveExceedItems();
            yield return HandleNextUtterance();
        }
    }
}
