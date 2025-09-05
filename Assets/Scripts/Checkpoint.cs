using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [SerializeField] private string checkpointId;
    [SerializeField] private Transform respawnPoint;

    private void Start()
    {
        RespawnManager.Instance?.RegisterCheckpoint(checkpointId, respawnPoint.position);
    }


    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.collider.CompareTag("Player"))
        {
            Debug.Log("Player collided with checkpoint: " + checkpointId);
            RespawnManager.Instance?.SetActiveCheckpoint(checkpointId);
        }
    }
}
