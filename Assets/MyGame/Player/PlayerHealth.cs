using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public float CurrentHealth = 100f;
    public float RegenHealthRate = 0.1f;
    void Start()
    {
        CurrentHealth = 100f; 
    }

    // Update is called once per frame
    void Update()
    {
        //Code to make some test on the health
        if (Input.GetKeyDown(KeyCode.R))
        {
            CurrentHealth -= 10f;
            CurrentHealth = Mathf.Max(CurrentHealth, 0f);
        }

        if (CurrentHealth <= 0f)
        {
            Debug.Log("You are dead!");
        }

        else
        {
            CurrentHealth += RegenHealthRate * Time.deltaTime;
            if (CurrentHealth > 100f)
            {
                CurrentHealth = 100f;
            }
        }


    }
}
