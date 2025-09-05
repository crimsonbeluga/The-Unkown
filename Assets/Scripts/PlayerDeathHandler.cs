using System.Collections;
using UnityEngine;

public class PlayerDeathHandler : MonoBehaviour
{
   [SerializeField] public bool isDead = false;
    private PlayerController controller;
    private PushPullHandler pushPullHandler;
    

    private void Awake()
    {
        controller = GetComponent<PlayerController>();
        pushPullHandler = GetComponent<PushPullHandler>(); // Get push/pull component
        
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;
         

        pushPullHandler?.ForceRelease(); // ✅ auto-release box on death

        if (controller != null)
        {
            controller.DisableInput();
        }

        Debug.Log("Die() called. Starting coroutine...");
        StartCoroutine(HandleDeath());
    }

    private IEnumerator HandleDeath()
    {
        yield return new WaitForSeconds(0f);

        if (RespawnManager.Instance == null)
        {
            Debug.LogWarning("RespawnManager is null!");
            yield break;
        }

        RespawnManager.Instance.Respawn(gameObject);

        isDead = false;
    }

}
