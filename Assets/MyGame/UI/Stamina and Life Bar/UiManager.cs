using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("UI Elements")]
    public Slider healthBar;
    public Slider staminaBar;
    
    [Header("Player Reference")]
    public PlayerMouvement playerMovement;
    
    public float maxHealth = 100f;
    public float currentHealth = 100f;

    void Start()
    {
        currentHealth = maxHealth;
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
            healthBar.value = currentHealth;
        }
    }
}