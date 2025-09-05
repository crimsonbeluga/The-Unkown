using Unity.VisualScripting;
using UnityEngine;

public class DeathTrigger : MonoBehaviour
{
    private PlayerDeathHandler playerDeathHandlerScript;


    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Something entered DeathTrigger: " + other.name);

        if (other.CompareTag("Player"))
        {
            Debug.Log("Player hit death trigger!");
            HealthManager playerHealthManagerScript = other.GetComponent<HealthManager>();
            if (playerHealthManagerScript != null)
            {
                playerHealthManagerScript.TakeDamage(100);
            }

        }
    }

}
