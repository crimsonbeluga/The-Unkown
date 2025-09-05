using UnityEngine;

public class PushPullHandler : MonoBehaviour
{
    [Header("Interaction Settings")]
    public Transform interactionPoint;
    public float interactionRadius = 0.5f;
    public LayerMask boxLayer;

    [Header("References")]
    public PlayerPawn playerPawn;
    public PlayerAnimator playerAnimator;

    [Header("Debug Override")]
    public float maxInteractionDistance = 1.2f;

    private Rigidbody2D boxRb;
    private Transform boxTransform;
    private bool isInteracting;

    private bool wasPushing = false;
    private bool wasPulling = false;

    void Update()
    {
        if (isInteracting)
        {
            HandlePushPull();
        }
    }

    public void TryInteract()
    {
        Debug.Log("[PushPullHandler] TryInteract() called.");

        if (isInteracting)
        {
            StopInteraction();
            return;
        }

        Collider2D hit = Physics2D.OverlapCircle(interactionPoint.position, interactionRadius, boxLayer);
        if (hit != null && hit.CompareTag("Box"))
        {
            Debug.Log("[PushPullHandler] Collider hit: " + hit.name);

            boxTransform = hit.transform;
            boxRb = hit.attachedRigidbody;
            isInteracting = true;

            playerPawn.SetPushingOrPulling(true);
            Debug.Log("[PushPullHandler] Started interacting with box: " + boxTransform.name);
        }
        else
        {
            Debug.Log("[PushPullHandler] No box found in range.");
        }
    }

    private void HandlePushPull()
    {
        if (boxTransform == null || boxRb == null)
        {
            StopInteraction();
            return;
        }

        float distanceToBox = Vector2.Distance(interactionPoint.position, boxTransform.position);
        Debug.Log($"[PushPullHandler] Distance to box: {distanceToBox:F2}");

        if (distanceToBox > maxInteractionDistance)
        {
            Debug.Log($"[PushPullHandler] Distance exceeded limit ({maxInteractionDistance}). Stopping.");
            StopInteraction();
            return;
        }

        Vector2 input = playerPawn.GetInput();
        float inputX = input.x;
        float playerSpeed = playerPawn.GetVelocity().x;
        bool isIdle = Mathf.Abs(inputX) < 0.01f;

        if (!isIdle)
        {
            float dirToBox = boxTransform.position.x - playerPawn.transform.position.x;
            float inputDir = Mathf.Sign(inputX);
            bool isPushing = inputDir == Mathf.Sign(dirToBox);
            bool isPulling = !isPushing;

            wasPushing = isPushing;
            wasPulling = isPulling;

            playerAnimator.SetPushing(isPushing);
            playerAnimator.SetPulling(isPulling);
            playerAnimator.animator.speed = 1f;

            if (isPulling)
                playerPawn.spriteRenderer.flipX = (boxTransform.position.x < playerPawn.transform.position.x);
        }
        else
        {
            playerAnimator.animator.speed = 0f;

            if (wasPushing)
            {
                playerAnimator.SetPushing(true);
                playerAnimator.SetPulling(false);
            }
            else if (wasPulling)
            {
                playerAnimator.SetPushing(false);
                playerAnimator.SetPulling(true);
            }
        }

        boxRb.velocity = new Vector2(playerSpeed, boxRb.velocity.y);

        Debug.Log($"[PushPullHandler] Interacting. Idle: {isIdle}, wasPushing: {wasPushing}, wasPulling: {wasPulling}, AnimatorSpeed: {playerAnimator.animator.speed}");
    }

    private void StopInteraction()
    {
        Debug.Log("[PushPullHandler] StopInteraction() called.");

        isInteracting = false;
        playerAnimator.SetPushing(false);
        playerAnimator.SetPulling(false);
        playerAnimator.animator.speed = 1f;

        if (boxRb != null)
            boxRb.velocity = Vector2.zero;

        boxRb = null;
        boxTransform = null;

        wasPushing = false;
        wasPulling = false;

        playerPawn.SetPushingOrPulling(false);
    }

    public void ForceRelease()
    {
        if (!isInteracting) return;

        Debug.Log("[PushPullHandler] Force release triggered.");
        StopInteraction();
    }


    void OnDrawGizmosSelected()
    {
        if (interactionPoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(interactionPoint.position, interactionRadius);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(interactionPoint.position, maxInteractionDistance);
        }
    }
}
