using UnityEngine;

public class InteractableObject : MonoBehaviour, IInteractable
{
    [Header("Interaction")]
    public string itemName = "Objet";
    public string interactionMessage = "Vous avez ramassé un objet !";


//Fonction d'interaction pour cet item, cela pourra changer en fonction de l'objet (pour l'instant c'est un médic)
    [Header("Effects")]
    public float HealingAmount = 20f;
    
    public void Interact()
    {
        Debug.Log(interactionMessage);

        PlayerHealth playerHealth = FindAnyObjectByType<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.CurrentHealth += HealingAmount;
            if (playerHealth.CurrentHealth > 100f)
            {
                playerHealth.CurrentHealth = 100f;
            }
        }


        Destroy(gameObject);

    }
}