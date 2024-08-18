using UnityEngine;
using TMPro;

public class DialogueHandler: MonoBehaviour
{
    [SerializeField] protected TextMeshProUGUI npcTextBox;
    [SerializeField] protected TMP_InputField inputField;
    private string text = "";

    public static DialogueHandler Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void OnEnable()
    {
        inputField.interactable = true;
        inputField.ActivateInputField();
    }


    private void OnDisable()
    {
        npcTextBox.text = "";
    }

    public void SendText(string textData)
    {
        text += textData + " ";
        npcTextBox.text = text;
    }


    public void FinishResponse()
    {
        GameManager.Instance.dialogueManager.SetIsClosable(true);
        text = "";
        inputField.text = "";
        inputField.interactable = true;
        inputField.ActivateInputField();
    }
}
