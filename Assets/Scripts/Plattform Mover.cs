using UnityEngine;

public class PlatformMover : MonoBehaviour
{
    public Transform startPoint;                 // Starting point in world space
    public Vector3 maxDistance = new Vector3(5f, 0f, 0f); // How far from start to move
    public float speed = 2f;                     // Movement speed

    private Vector3 startPosition;              // Cached initial position
    private Vector3 endPosition;                // Computed as start + maxDistance
    private bool movingToEnd = true;            // Are we moving toward the end?
    private Vector3 lastPosition;               // Previous frame position
    public bool DeathReset;
    private HealthManager PlayerHealthManager;



    public Vector3 DeltaMovement { get; private set; } // How far the platform moved this frame

    void Awake()
    {
        startPosition = startPoint.position;
        endPosition = startPosition + maxDistance;
        transform.position = startPosition;
        lastPosition = transform.position;
    }

    void Update()
    {
        HandlePlatformMovement();
        DeltaMovement = transform.position - lastPosition;
        lastPosition = transform.position;
    }

    void HandlePlatformMovement()
    {
        Vector3 target = movingToEnd ? endPosition : startPosition;

        transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, target) < 0.01f)
        {
            movingToEnd = !movingToEnd;

            if (DeathReset == true)
            {
                Destroy(gameObject);

                

               
                
            }
        }
    }

    void OnDrawGizmos()
    {
        if (!startPoint) return;

        Vector3 startPos = startPoint.position;
        Vector3 endPos = startPos + maxDistance;

        Gizmos.color = Color.green;
        Gizmos.DrawLine(startPos, endPos);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(startPos, Vector3.one * 0.3f);

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(endPos, Vector3.one * 0.3f);
    }
}
