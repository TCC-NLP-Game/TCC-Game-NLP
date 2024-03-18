using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    public CinemachineFreeLook freeLook;
    public bool isDialogueOpen = false;
    public float rotationSpeed = 2f;
    private DialogueManager chatParent;
    private Transform playerCamera; 

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

            // Calculate the rotation needed to look at the NPC's head
            Quaternion targetRotation = Quaternion.LookRotation(direction);

            // Smoothly interpolate the camera rotation
            playerCamera.rotation = Quaternion.Slerp(playerCamera.rotation, targetRotation, Time.deltaTime * rotationSpeed);  // Set camera rotation directly
            yield return null;
        }
    }
}
