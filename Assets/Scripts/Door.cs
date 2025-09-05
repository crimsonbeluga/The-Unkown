using UnityEngine;

public class DoorScript : MonoBehaviour
{
    public Transform MovePositionObject;
    public float MoveSpeed = 2f;

    private Vector3 StartingPosition;
    private Vector3 CurrentTargetPosition;
    private bool isOpen = false;

    private void Awake()
    {
        StartingPosition = transform.position;
        CurrentTargetPosition = StartingPosition;
    }

    private void Update()
    {
        Debug.DrawLine(transform.position, CurrentTargetPosition, Color.red);
        Debug.Log($"[Door] Position: {transform.position} | Target: {CurrentTargetPosition}");

        if (Vector3.Distance(transform.position, CurrentTargetPosition) < 0.01f)
        {
            transform.position = CurrentTargetPosition;
            return;
        }

        transform.position = Vector3.MoveTowards(transform.position, CurrentTargetPosition, MoveSpeed * Time.deltaTime);
    }

    public void MoveDoor()
    {
        if (!isOpen)
        {
            isOpen = true;
            CurrentTargetPosition = MovePositionObject.position; // Cache current world pos
        }
    }

    public void ResetDoor()
    {
        if (isOpen)
        {
            isOpen = false;
            CurrentTargetPosition = StartingPosition;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(Application.isPlaying ? StartingPosition : transform.position, 0.1f);

        if (MovePositionObject != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(MovePositionObject.position, 0.1f);
        }

        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, CurrentTargetPosition);
    }
}
