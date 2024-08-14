using Convai.Scripts;
using Convai.Scripts.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class NPCInteractable : MonoBehaviour
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
        ConvaiNPC currentNPC = GetComponent<ConvaiNPC>();
        ConvaiNPCManager.Instance.SetActiveConvaiNPC(currentNPC);
        ConvaiNPC charData = ConvaiNPCManager.Instance.GetActiveConvaiNPC();
        npcNameTextBox.text = charData.characterName ?? "Character";
        GameManager.Instance.dialogueManager.OpenChat();
        targetWeight = 1f;
        npcTarget.position = player.playerHead.transform.position;
    }

    public void EndInteraction()
    {
        targetWeight = 0f;
    }
}
