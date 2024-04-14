using Inworld;
using Inworld.Interactions;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CapsuleCollider))]
public class NPCInteractable : InworldInteraction
{
    public void Interact()
    {
        InworldController.CurrentCharacter = GetComponent<InworldCharacter>();
        GameManager.Instance.dialogueManager.OpenChat();
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
