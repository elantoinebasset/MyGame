using UnityEngine;

public class Hamburger : MonoBehaviour, IUsable
{
    [Header("Hamburger Properties")]
    public int HungerRestored = 20;
    public float UseTime = 1.0f;

    [Header("Link Scripts")]
    public InventoryManager inventoryManager;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
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

    // Update is called once per frame
    void Update()
    {
        
    }
}
