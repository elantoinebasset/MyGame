using UnityEngine;

public class InteractableObject : MonoBehaviour, IInteractable
{
    [Header("Interaction")]
    public string itemName = "Objet";
    public string interactionMessage = "Vous avez ramassé un objet !";
    
    public void Interact()
    {
        Debug.Log(interactionMessage);
        

        Destroy(gameObject);
        
    }
}