using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("UI Elements")]
    public Slider healthBar;
    public Slider staminaBar;
    public Slider hungerBar;
    
    [Header("Player Reference")]
    public PlayerMouvement playerMovement;
    public PlayerHealth playerHealth;
    public PlayerHunger playerHunger;

    void Start()
    {
        UpdateUI();
    }

    void Update()
    {
        UpdateUI();
    }

    void UpdateUI()
    {

        if (staminaBar != null && playerMovement != null)
        {
            staminaBar.value = playerMovement.Stamina;
        }


        if (healthBar != null)
        {
            healthBar.value = playerHealth.CurrentHealth;
        }
        
        if (hungerBar != null && playerHunger != null)
        {
            hungerBar.value = playerHunger.Hunger;
        }
    }
}