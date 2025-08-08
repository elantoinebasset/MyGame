using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("UI Elements")]
    public Slider healthBar;
    public Slider staminaBar;
    
    [Header("Player Reference")]
    public PlayerMouvement playerMovement;
    public PlayerHealth playerHealth;

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
    }
}