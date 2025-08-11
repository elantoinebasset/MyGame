using UnityEngine;

public class PlayerHunger : MonoBehaviour
{
    [Header("Hunger Settings")]
    public float Hunger = 100f;
    public float HungerDrainRate = 0.1f;
    public float healthLostWhenStarving = 2f;
    void Start()
    {
        Hunger = 100f;
    }


    void Update()
    {

        if (Input.GetKeyDown(KeyCode.H))
        {
            Hunger -= 10f;
            Hunger = Mathf.Max(Hunger, 0f);
        }

        if (Hunger <= 0f)
        {
            PlayerHealth playerHealth = FindAnyObjectByType<PlayerHealth>();
            Debug.Log("You are starving!");
            playerHealth.CurrentHealth -= healthLostWhenStarving * Time.deltaTime;
            if (playerHealth.CurrentHealth <= 0f)
            {
                playerHealth.CurrentHealth = 0f;
            }

        }
        else
        {
            Hunger -= HungerDrainRate * Time.deltaTime;
            if (Hunger < 0f)
            {
                Hunger = 0f;
            }
        }
    }
}
