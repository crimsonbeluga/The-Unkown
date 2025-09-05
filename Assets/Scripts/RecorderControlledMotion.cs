using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Rigidbody2D))]
public class FreezePositionDuringIdleRecording : MonoBehaviour
{
    public bool isRecording = true;
    public string speedParam = "Speed";
    public float velocityThreshold = 0.1f;

    private Animator animator;
    private Rigidbody2D rb;
    private Vector3 frozenPosition;

    void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        if (isRecording)
            animator.updateMode = AnimatorUpdateMode.UnscaledTime;
    }

    void FixedUpdate()
    {
        if (!isRecording) return;

        float speed = rb.velocity.magnitude;

        if (speed < velocityThreshold)
        {
            // Force stop motion
            rb.velocity = Vector2.zero;

            // Freeze transform during idle
            transform.position = frozenPosition;
            speed = 0f;
        }
        else
        {
            // Cache position only while moving
            frozenPosition = transform.position;
        }

        animator.SetFloat(speedParam, speed);
    }

    void OnDisable()
    {
        if (isRecording)
            animator.updateMode = AnimatorUpdateMode.Normal;
    }
}
