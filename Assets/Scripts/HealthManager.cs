using UnityEngine;

public class HealthManager : MonoBehaviour
{
    private PlayerDeathHandler playerDeathHandlerScript;
    public int maxHealth = 100;
    public int currentHealth { get; private set; }


    private void Awake()
    {
        currentHealth = maxHealth;
        playerDeathHandlerScript = GetComponent<PlayerDeathHandler>();
    }
   

    public void TakeDamage(int amount)
    {
        if (currentHealth <= 0)
        {
            return;
        }

        currentHealth = Mathf.Max(currentHealth - amount, 0);

        if (currentHealth <= 0)
        {
            playerDeathHandlerScript.Die();
            Debug.Log("Player died, calling Die()");
        }
    }

    public void Heal(int amount)
    {
        if (currentHealth <= 0)
        {
            return;
        }

        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);

    }

    public void ResetHealth()
    {

        currentHealth = maxHealth;
    }
}
