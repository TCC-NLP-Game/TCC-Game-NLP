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
    private float rotationSpeed = 2f;

    void Start()
    {
        chatParent = GetComponent<DialogueManager>();
        chatParent.gameObject.SetActive(false);
        playerCamera = Camera.main.transform;
    }

    public void OpenChat(Transform npc)
    {
        isDialogueOpen = true;
        freeLook.enabled = false;
        chatParent.gameObject.SetActive(true);
        StartCoroutine(TurnCameraTowardsNPC(npc));
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void CloseChat()
    {
        isDialogueOpen = false;
        chatParent.gameObject.SetActive(false);
        freeLook.enabled = true;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
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
