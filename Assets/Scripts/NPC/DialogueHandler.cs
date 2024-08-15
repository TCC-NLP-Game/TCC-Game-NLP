using UnityEngine;
using TMPro;

public class DialogueHandler: MonoBehaviour
{
    [SerializeField] protected TextMeshProUGUI npcTextBox;
    [SerializeField] protected TMP_InputField inputField;

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
        GameManager.Instance.dialogueManager.SetIsClosable(true);
        npcTextBox.text = textData;
    }


    public void FinishResponse()
    {
        inputField.text = "";
        inputField.interactable = true;
        inputField.ActivateInputField();
    }
}
