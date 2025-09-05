using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerPawn))]
public class ClimbHandler : MonoBehaviour
{
    [Header("Climb Settings")]
    public float climbSpeed = 2f;
    public float jumpForceX = 8f;
    public float jumpForceY = 10f;
    public float climbCheckDistance = 0.5f;

    [Header("Climbable Surface Box Size")]
    public Vector2 climbCheckSize = new Vector2(0.5f, 2f);
    public Vector2 climbCheckOffset = Vector2.zero;

    [Header("Cooldown Settings")]
    public float reGrabCooldown = 0.5f;

    private Rigidbody2D rb;
    private PlayerPawn pawn;
    private bool isClimbing = false;
    private bool canJumpOffWall = true;
    private bool isJumpingOffWall = false;
    private bool canRegrabWall = true;

    private float climbableSurfaceTop = 0f;
    private float climbableSurfaceBottom = 0f;
    private bool enteredClimbThisFrame = false;
    private bool lastClimbDirectionUp = true;
    private bool hasPlayedClimbAnimFrame = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        pawn = GetComponent<PlayerPawn>();
    }

    void Update()
    {
        enteredClimbThisFrame = false;

        if (isClimbing)
        {
            HandleClimbing();
            if (pawn.WasJumpPressedThisFrame() && canJumpOffWall)
            {
                JumpOffWall();
            }
        }
        else
        {
            CheckForClimbableSurface();
        }
    }

    void HandleClimbing()
    {
        Vector2 input = pawn.GetInput();
        float yVelocity = rb.velocity.y;
        bool isMovingUp = input.y > 0.1f;
        bool isMovingDown = input.y < -0.1f;
        bool isIdle = !isMovingUp && !isMovingDown;

        if (isMovingUp)
        {
            if (transform.position.y < climbableSurfaceTop)
                rb.velocity = new Vector2(rb.velocity.x, climbSpeed);
            else
                rb.velocity = Vector2.zero;
        }
        else if (isMovingDown)
        {
            if (transform.position.y > climbableSurfaceBottom)
                rb.velocity = new Vector2(rb.velocity.x, -climbSpeed);
            else
                rb.velocity = Vector2.zero;
        }
        else
        {
            rb.velocity = new Vector2(rb.velocity.x, 0f);
        }

        pawn.playerAnimator.SetClimbing(true);

        if (!isIdle)
        {
            lastClimbDirectionUp = isMovingUp;
            pawn.playerAnimator.SetClimbingDirection(isMovingUp);
            hasPlayedClimbAnimFrame = true;
            pawn.playerAnimator.animator.speed = 1f;
        }
        else if (hasPlayedClimbAnimFrame)
        {
            pawn.playerAnimator.animator.speed = 0f;
        }
    }

    void JumpOffWall()
    {
        StopClimbing();
        float jumpDirection = pawn.IsFacingLeft() ? 1f : -1f;
        rb.velocity = new Vector2(jumpDirection * jumpForceX, jumpForceY);
        pawn.TriggerJump();
        StartCoroutine(JumpCooldown());
        canRegrabWall = false;
        StartCoroutine(RegrabCooldown());
    }

    void CheckForClimbableSurface()
    {
        if (canRegrabWall)
        {
            Vector2 boxOrigin = (Vector2)transform.position + climbCheckOffset;
            RaycastHit2D hit = Physics2D.BoxCast(boxOrigin, climbCheckSize, 0f, Vector2.up, climbCheckDistance, LayerMask.GetMask("Climbable"));

            if (hit.collider != null && !isClimbing && !isJumpingOffWall)
            {
                climbableSurfaceTop = hit.collider.bounds.max.y;
                climbableSurfaceBottom = hit.collider.bounds.min.y;
                StartClimbing();
            }
            else if (hit.collider == null && isClimbing)
            {
                StopClimbing();
            }
        }
    }

    void StartClimbing()
    {
        enteredClimbThisFrame = true;
        hasPlayedClimbAnimFrame = false;
        StartCoroutine(DelaySetPlayedFrame());
        pawn.playerAnimator.SetBool("Falling From Jump", false);

        isClimbing = true;
        rb.gravityScale = 0f;
        rb.velocity = Vector2.zero;
        pawn.FreezeMovement(true);
        pawn.playerAnimator.SetClimbing(true);
        pawn.playerAnimator.animator.speed = 1f;
    }

    IEnumerator DelaySetPlayedFrame()
    {
        yield return null;
        hasPlayedClimbAnimFrame = true;
    }

    void StopClimbing()
    {
        isClimbing = false;
        rb.gravityScale = 4f;
        pawn.FreezeMovement(false);
        pawn.playerAnimator.SetClimbing(false);
        pawn.playerAnimator.animator.speed = 1f;
        isJumpingOffWall = false;
    }

    IEnumerator JumpCooldown()
    {
        canJumpOffWall = false;
        yield return new WaitForSeconds(0.1f);
        canJumpOffWall = true;
    }

    IEnumerator RegrabCooldown()
    {
        yield return new WaitForSeconds(reGrabCooldown);
        canRegrabWall = true;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube((Vector2)transform.position + climbCheckOffset, climbCheckSize);
    }
}