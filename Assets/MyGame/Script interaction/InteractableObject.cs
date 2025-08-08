using UnityEngine;

public class InteractableObject : MonoBehaviour, IInteractable
{
    [Header("Interaction")]
    public string itemName = "Objet";
    public string interactionMessage = "Vous avez ramassé un objet !";

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