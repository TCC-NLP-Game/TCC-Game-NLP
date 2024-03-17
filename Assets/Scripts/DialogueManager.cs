using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    private DialogueManager chatParent;

    // Start is called before the first frame update
    void Start()
    {
        chatParent = GetComponent<DialogueManager>();
        chatParent.gameObject.SetActive(false);
    }

    public void OpenChat() { 
        chatParent.gameObject.SetActive(true); 
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
