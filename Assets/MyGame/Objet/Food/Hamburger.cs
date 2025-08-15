using UnityEngine;

public class Hamburger : MonoBehaviour, IUsable, IDropable
{
    [Header("Hamburger Properties")]
    public int HungerRestored = 120;
    public float UseTime = 2.0f;

    [Header("Link Scripts")]
    public InventoryManager inventoryManager;

    void Start()
    {
        
    }
    public void UseItem()
    {
        PlayerHunger playerHunger = FindAnyObjectByType<PlayerHunger>();
        if (playerHunger != null)
        {
            playerHunger.Hunger += HungerRestored;
            playerHunger.Hunger = Mathf.Min(playerHunger.Hunger, 100f);
            Debug.Log("Hamburger used! Hunger restored by " + HungerRestored);
        }
    }

    public void DropItem()
    {
        Debug.Log("Hamburger dropped!");
    }

    
    void Update()
    {
        
    }
}
