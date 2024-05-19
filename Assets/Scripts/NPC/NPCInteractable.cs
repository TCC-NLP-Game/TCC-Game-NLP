using Inworld;
using Inworld.Entities;
using Inworld.Interactions;
using System.Collections;
using TMPro;
using UnityEngine;

public class NPCInteractable : InworldInteraction
{
    [SerializeField] protected TextMeshProUGUI npcNameTextBox;
    private PlayerInteract player;
    public Transform npcHead;
    public Transform npcTarget;

    private void Start()
    {
        player = FindObjectOfType<PlayerInteract>();
    }

    public void Interact()
    {
        InworldController.CurrentCharacter = GetComponent<InworldCharacter>();
        InworldCharacterData charData = InworldController.CurrentCharacter.Data;
        npcNameTextBox.text = charData.givenName ?? "Character";
        npcTarget.position = player.playerHead.transform.position;
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
