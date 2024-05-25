using Inworld;
using Inworld.Entities;
using Inworld.Interactions;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class NPCInteractable : InworldInteraction
{
    [SerializeField] protected TextMeshProUGUI npcNameTextBox;
    private PlayerInteract player;
    public Transform npcHead;
    public Transform npcTarget;
    private Rig rig;
    private float targetWeight;

    private void Awake()
    {
        rig = GetComponentInChildren<Rig>();
    }

    private void Start()
    {
        player = FindObjectOfType<PlayerInteract>();
    }

    private void Update()
    {
        rig.weight = Mathf.Lerp(rig.weight, targetWeight, Time.deltaTime * 10f);
    }

    public void Interact()
    {
        InworldController.CurrentCharacter = GetComponent<InworldCharacter>();
        InworldCharacterData charData = InworldController.CurrentCharacter.Data;
        npcNameTextBox.text = charData.givenName ?? "Character";
        GameManager.Instance.dialogueManager.OpenChat();
        targetWeight = 1f;
        npcTarget.position = player.playerHead.transform.position;
    }

    public void EndInteraction()
    {
        targetWeight = 0f;
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
