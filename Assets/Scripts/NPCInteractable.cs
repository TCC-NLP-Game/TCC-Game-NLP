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
        GameObject tempLookAt = Instantiate(new GameObject("TempLookAt"));
        tempLookAt.transform.position = transform.position;
        tempLookAt.transform.position += new Vector3(0f, 1f, 0f);
        InworldController.CurrentCharacter = GetComponent<InworldCharacter>();
        dialogueManager.OpenChat(tempLookAt.transform);
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
