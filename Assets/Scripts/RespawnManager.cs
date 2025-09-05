// File: RespawnManager.cs
using UnityEngine;
using System.Collections.Generic;

public class RespawnManager : MonoBehaviour
{
    public static RespawnManager Instance { get; private set; }

    private Dictionary<string, Vector3> checkpoints = new();
    private string activeCheckpointId = null;
    private CameraZoneTrigger trigger;
    private Box[] allBoxes;

    public string GetActiveCheckpointId()
    {
        return activeCheckpointId;
    }

    private void Awake()
    {
        Instance = this;
        trigger = FindObjectOfType<CameraZoneTrigger>();
        allBoxes = FindObjectsOfType<Box>();
    }

    public void RegisterCheckpoint(string id, Vector3 position)
    {
        Debug.Log($"Registering checkpoint ID: {id} at position {position}");

        if (!checkpoints.ContainsKey(id))
            checkpoints.Add(id, position);
    }

    public void SetActiveCheckpoint(string id)
    {
        Debug.Log($"SetActiveCheckpoint CALLED with ID: {id}");

        if (checkpoints.ContainsKey(id))
        {
            activeCheckpointId = id;
            Debug.Log("Activated checkpoint: " + id);
        }
        else
        {
            Debug.LogWarning("Checkpoint ID not found: " + id);
        }
    }

    public void Respawn(GameObject player)
    {
        Debug.Log("RESPAWNING player...");

        if (activeCheckpointId == null)
        {
            Debug.LogWarning("No active checkpoint set!");
            return;
        }

        Debug.Log("Using checkpoint ID: " + activeCheckpointId);
        Vector3 checkpoint = checkpoints[activeCheckpointId];
        Debug.Log("Moving player to: " + checkpoint);

        player.transform.position = checkpoint;
        trigger.PlatformReset();

        foreach (var box in allBoxes)
            box.ResetBoxPosition();

        var health = player.GetComponent<HealthManager>();
        if (health != null) health.ResetHealth();

        var controller = player.GetComponent<PlayerController>();
        if (controller != null) controller.EnableInput();
    }
}
