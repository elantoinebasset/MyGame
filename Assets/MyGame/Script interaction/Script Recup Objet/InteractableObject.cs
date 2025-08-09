using UnityEngine;

public class InteractableObject : MonoBehaviour, IInteractable
{
    [Header("Interaction")]
    public string itemName = "Objet";
    public string interactionMessage = "Vous avez ramassé un objet !";
    
    [Header("Inventory")]
    public Sprite itemSprite; 
    public bool addToInventory = true; 
    public void Interact()
    {
        Debug.Log(interactionMessage);
        
        if (addToInventory && itemSprite != null)
        {
            
            InventoryManager inventory = FindAnyObjectByType<InventoryManager>();
            if (inventory != null)
            {
                bool itemAdded = inventory.AddItem(itemSprite);
                if (itemAdded)
                {
                    Debug.Log($"{itemName} ajouté à l'inventaire !");
                    Destroy(gameObject);
                }
                else
                {
                    Debug.Log("Inventaire plein ! Impossible d'ajouter " + itemName);
                    
                }
            }
        }
    }
}