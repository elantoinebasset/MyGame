using UnityEngine;

public class Bandage : MonoBehaviour, IUsable, IDropable
{
    [Header("Bandage Properties")]
    public int healAmount = 20;
    public float useTime = 2.0f;

    [Header("Link Scripts")]
    public InventoryManager inventoryManager;

    void Start()
    {
        
    }

    public void UseItem()
    {
        PlayerHealth playerHealth = FindAnyObjectByType<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.CurrentHealth += healAmount;
            playerHealth.CurrentHealth = Mathf.Min(playerHealth.CurrentHealth, 100f);
            Debug.Log("Bandage used! Health restored by " + healAmount);
        }
    }
    public void DropItem()
    {
        Debug.Log("Bandage dropped!");
    }

    void Update()
    {

    }
}
