using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteract : MonoBehaviour
{
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.E))
        {
            float interactRange = 2f;
            Collider[] collidedItems = Physics.OverlapSphere(transform.position, interactRange);
            foreach  (Collider collider in collidedItems)
            {
                if (collider.TryGetComponent(out NPCInteractable npcInteractable))
                {
                    npcInteractable.Interact();
                }

            }
        }
    }
}
