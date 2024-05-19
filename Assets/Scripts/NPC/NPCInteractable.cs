using Inworld;
using Inworld.Entities;
using Inworld.Interactions;
using System.Collections;
using TMPro;
using UnityEngine;

public class NPCInteractable : InworldInteraction
{
    [SerializeField] protected TextMeshProUGUI npcNameTextBox;

    public void Interact()
    {
        InworldController.CurrentCharacter = GetComponent<InworldCharacter>();
        InworldCharacterData charData = InworldController.CurrentCharacter.Data;
        npcNameTextBox.text = charData.givenName ?? "Character";
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
