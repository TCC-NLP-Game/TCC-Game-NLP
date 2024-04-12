using Cinemachine;
using System;
using UnityEngine;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    [NonSerialized] public bool isDialogueOpen = false;
    [SerializeField] private CinemachineFreeLook freeLook;
    private DialogueManager chatParent;
    [NonSerialized] public bool canBeClosed = true;
    [SerializeField] protected Image closableTextBox;

    void Start()
    {
        chatParent = GetComponent<DialogueManager>();
        chatParent.gameObject.SetActive(false);
    }

    public void OpenChat()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        isDialogueOpen = true;
        freeLook.enabled = false;
        chatParent.gameObject.SetActive(true);
    }

    public void CloseChat()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        isDialogueOpen = false;
        chatParent.gameObject.SetActive(false);
        freeLook.enabled = true;
    }

    public void SetIsClosable(bool isClosable = true) { 
        canBeClosed = isClosable;
        closableTextBox.gameObject.SetActive(isClosable);
    }
}
