using System.Collections.Generic;
using JetBrains.Annotations;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Playables;

[RequireComponent(typeof(Collider2D))]
public class CameraZoneTrigger : MonoBehaviour
{
    [SerializeField]
    public CameraMode cameraMode;
    private PlatformMover platformMover;
    public PlayableDirector director;
    [System.Serializable]
    public class SpawnEntry
    {
        public GameObject prefab;
        public Vector3 spawnPosition;
    }

    [Header("Prefabs and Spawn Points")]
    public List<SpawnEntry> spawnEntries = new List<SpawnEntry>();

    private void OnTriggerEnter2D(Collider2D other)
    {


        spawnObjectWhenCameraZoneIsTriggered();
        if (!other.CompareTag("Player")) return;
        CameraManager.Instance.SetCameraMode(cameraMode, gameObject);

        if (director != null)
        {
            director.Play();
        }


    }

    public void spawnObjectWhenCameraZoneIsTriggered()
    {
       

        foreach (var entry in spawnEntries)
        {
            if (entry.prefab != null)
            {
                Vector3 worldPosition = transform.position + entry.spawnPosition;
                Instantiate(entry.prefab, worldPosition, Quaternion.identity);
            }
            
        }
    }

    public void PlatformReset()
    {
        foreach (var entry in spawnEntries)
        {
            if (entry.prefab != null)
            {
                PlatformMover mover = entry.prefab.GetComponent<PlatformMover>();
                if (mover != null && mover.DeathReset == true)
                {
                    Vector3 worldPosition = transform.position + entry.spawnPosition;
                    Instantiate(entry.prefab, worldPosition, Quaternion.identity);
                }
            }
        }
    }
    

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (cameraMode == null) return;

        // Determine camera position
        Vector3 camPos = cameraMode.modeType == CameraModeType.FollowPlayer && cameraMode.target != null
            ? cameraMode.target.position + cameraMode.offset
            : cameraMode.position;

        camPos.z = 0f; // flatten

        // Orthographic camera size
        float size = cameraMode.zoom;
        float aspect = 16f / 9f; // default editor assumption (adjust if needed)
        float height = size * 2f;
        float width = height * aspect;

        // Draw camera area
        Gizmos.color = new Color(1f, 1f, 0f, 0.5f); // yellowish
        Gizmos.DrawWireCube(camPos, new Vector3(width, height, 0.1f));

        // Optional: draw target and offset line
        if (cameraMode.target != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(cameraMode.target.position, camPos);
            Gizmos.DrawSphere(cameraMode.target.position, 0.1f);
        }

        Gizmos.color = Color.green;
        foreach (var entry in spawnEntries)
        {
            Vector3 worldPos = transform.position + entry.spawnPosition;
            Gizmos.DrawWireSphere(worldPos, 0.25f);
            
        }
    }

#endif
}