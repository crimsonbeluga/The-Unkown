using UnityEngine;

public class Box : MonoBehaviour
{
    private Vector3 startPosition;
    private bool hasSavedStart = false;

    void Awake()
    {
        if (!hasSavedStart)
        {
            startPosition = transform.position;
            hasSavedStart = true;
            Debug.Log($"[Box] Start position set to: {startPosition}");
        }
    }

    public void ResetBoxPosition()
    {
        transform.position = startPosition;
    }
}
