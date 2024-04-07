using Cinemachine;
using System;
using System.Collections;
using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    [NonSerialized] public bool isDialogueOpen = false;
    [SerializeField] private CinemachineFreeLook freeLook;
    private DialogueManager chatParent;
    private Transform playerCamera;
    private readonly float rotationSpeed = 2f;

    void Start()
    {
        chatParent = GetComponent<DialogueManager>();
        chatParent.gameObject.SetActive(false);
        playerCamera = Camera.main.transform;
    }

    public void OpenChat(Transform npc)
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        isDialogueOpen = true;
        freeLook.enabled = false;
        chatParent.gameObject.SetActive(true);
        StartCoroutine(TurnCameraTowardsNPC(npc));
    }

    public void CloseChat()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        isDialogueOpen = false;
        chatParent.gameObject.SetActive(false);
        freeLook.enabled = true;
    }

    IEnumerator TurnCameraTowardsNPC(Transform npc)
    {
        while (isDialogueOpen)
        {
            Vector3 direction = npc.position - playerCamera.position;
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            playerCamera.rotation = Quaternion.Slerp(playerCamera.rotation, targetRotation, Time.deltaTime * rotationSpeed);  // Set camera rotation directly
            yield return null;
        }
    }
}
