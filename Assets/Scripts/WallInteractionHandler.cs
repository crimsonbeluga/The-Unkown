


using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerPawn))]
public class WallInteractionHandler : MonoBehaviour
{
    [Header("Settings")]
    public float climbSpeed = 2f;
    public float jumpForceX = 8f;
    public float jumpForceY = 10f;
    public float wallJumpDuration = 0.2f;
    public float reGrabCooldown = 0.5f;

    [Header("Detection")]
    public float sideCheckDistance = 0.5f;
    public Vector2 climbCheckSize = new Vector2(0.5f, 2f);
    public Vector2 climbCheckOffset = Vector2.zero;

    [Header("Mantle Settings")]
    public float mantleUpOffset = 1.5f;
    public float mantleForwardOffset = 0.5f;
    public float mantleDuration = 0.25f;
    public float mantleTopMargin = 0.1f;

    [Header("Mantle Curve Tuning")]
    public AnimationCurve mantleVerticalCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public AnimationCurve mantleHorizontalCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Mantle Corner Detection")]
    public float mantleCornerRadius = 0.5f;

    [Header("Mantle Lockout")]
    public float mantleLockoutAfterJump = 0.3f;

    [Header("Climb Suppression")]
    public float climbSuppressionDuration = 1.0f;

    [Header("Climb Grace Settings")]
    public float climbEntryLockTime = 0.2f;

    [Header("Climb Offset")]
    public float climbEntryLift = 0.05f;

    [Header("Climb Exit Offset")]
    public float climbExitNudge = 0.05f;

    [Header("Debug Gizmo Toggles")]
    public bool showClimbCheckGizmo = true;
    public bool showMantleCornerRadiusGizmo = true;
    public bool showMantleTargetGizmo = true;


    private float climbStartTime = -999f;
    private float climbEntryGraceTime = 0.1f;


    private Rigidbody2D rb;
    private PlayerPawn pawn;

    private bool isClimbing = false;
    private float climbEntryTimestamp;
    private bool canJumpOffWall = true;
    private bool isWallJumping = false;
    private float wallJumpTimer;
    private bool canRegrabWall = true;
    private bool isMantling = false;
    private bool hasPlayedClimbAnimFrame;
    private float lastWallJumpTime = -999f;

    private WallInteractionSurface currentSurface;
    private float climbableSurfaceTop = 0f;
    private float climbableSurfaceBottom = 0f;

    private bool isClimbSuppressed = false;
    private float climbSuppressionTimer = 0f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        pawn = GetComponent<PlayerPawn>();
    }

    void Update()
    {
        if (isWallJumping)
        {
            wallJumpTimer -= Time.deltaTime;
            if (wallJumpTimer <= 0f)
            {
                isWallJumping = false;
                pawn.SetWallJumping(false);
            }
        }

        if (climbSuppressionTimer > 0f)
        {
            climbSuppressionTimer -= Time.deltaTime;
            isClimbSuppressed = true;
        }
        else
        {
            isClimbSuppressed = false;
        }

        if (isMantling) return;

        if (!isClimbing && !isClimbSuppressed && canRegrabWall)
            TrySetInteractionSurface(i => i.isClimbable, isClimbCheck: true);

        if (!isMantling && currentSurface == null)
            TrySetInteractionSurface(i => i.isMantleable);

        if (isClimbing)
        {
            HandleClimbing();
            if (pawn.WasJumpPressedThisFrame() && canJumpOffWall)
                JumpOffWall();
        }

        if (CanMantle())
            TryMantle();
    }

    public bool TryWallJump(Vector2 input)
    {
        if (pawn.IsGrounded() || isWallJumping || !pawn.IsRunHeld())
            return false;

        float direction = pawn.IsFacingLeft() ? -1f : 1f;
        Vector2 rayDirection = Vector2.right * direction;

        int mask = ~(1 << gameObject.layer);
        RaycastHit2D hit = Physics2D.Raycast(transform.position, rayDirection, sideCheckDistance, mask);

        if (hit.collider != null && hit.collider != GetComponent<Collider2D>())
        {
            WallInteractionSurface surface = hit.collider.GetComponent<WallInteractionSurface>();
            if (surface != null && TryGetSurfaceSide(hit.normal, out SurfaceSide side))
            {
                bool valid = surface.HasInteraction(side, i => i.isWallJumpable);
                bool rightDir = (direction > 0 && input.x > 0.1f) || (direction < 0 && input.x < -0.1f);

                if (valid && rightDir)
                {
                    float xForce = -direction * jumpForceX;
                    rb.velocity = new Vector2(xForce, jumpForceY);

                    isWallJumping = true;
                    wallJumpTimer = wallJumpDuration;
                    lastWallJumpTime = Time.time;

                    pawn.SetWallJumping(true);
                    pawn.TriggerWallJump();

                    return true;
                }
            }
        }

        return false;
    }

    void HandleClimbing()
    {
        Vector2 input = pawn.GetInput();
        bool up = input.y > 0.1f;
        bool down = input.y < -0.1f;

        Debug.Log($"[Climb] Input Y: {input.y} | Up: {up} | Down: {down}");

        float playerTop = pawn.standingCollider.bounds.max.y;
        float surfaceTop = climbableSurfaceTop;
        bool exceededTop = playerTop >= surfaceTop;
        bool isJumping = pawn.WasJumpPressedThisFrame();
        bool allowJumpVelocity = rb.velocity.y > jumpForceY * 0.5f;

        Debug.Log($"[Climb] PlayerTop: {playerTop} | SurfaceTop: {surfaceTop} | ExceededTop: {exceededTop}");
        Debug.Log($"[Climb] IsJumping: {isJumping} | AllowJumpVelocity: {allowJumpVelocity}");

        if (pawn.IsGrounded() && (Time.time - climbStartTime > climbEntryGraceTime))
        {
            Debug.Log("[Climb] Forced exit due to ground contact");
            StopClimbing(applyNudge: true);
            climbSuppressionTimer = climbSuppressionDuration;
            return;
        }

        if (up && !exceededTop)
        {
            rb.velocity = new Vector2(rb.velocity.x, climbSpeed);
            Debug.Log("[Climb] Moving up");
        }
        else if (up && exceededTop && !isJumping && !allowJumpVelocity)
        {
            rb.velocity = new Vector2(rb.velocity.x, 0f);
            Debug.Log("[Climb] Holding at top");
        }
        else if (down && transform.position.y > climbableSurfaceBottom)
        {
            rb.velocity = new Vector2(rb.velocity.x, -climbSpeed);
            Debug.Log("[Climb] Moving down");

            Vector2 bottomCenter = pawn.standingCollider.bounds.center;
            bottomCenter.y = pawn.standingCollider.bounds.min.y - 0.05f;

            Vector2 checkSize = new Vector2(pawn.standingCollider.bounds.size.x * 0.9f, 0.1f);
            int mask = ~(1 << gameObject.layer);
            Collider2D[] hits = Physics2D.OverlapBoxAll(bottomCenter, checkSize, 0f, mask);

            foreach (var hit in hits)
            {
                if (hit == null || hit == GetComponent<Collider2D>())
                    continue;

                WallInteractionSurface surface = hit.GetComponent<WallInteractionSurface>();
                if (surface != null && surface.HasInteraction(SurfaceSide.Top, i => i.isWalkable))
                {
                    Debug.Log("[Climb] Drop-to-surface complete");
                    pawn.SuppressFallingFromJump();
                    StopClimbing(applyNudge: true);
                    climbSuppressionTimer = climbSuppressionDuration;
                    return;
                }
            }
        }
        else
        {
            rb.velocity = new Vector2(rb.velocity.x, 0f);
            Debug.Log("[Climb] No vertical movement");
        }

        pawn.playerAnimator.SetClimbing(true);

        if (up || down)
        {
            pawn.playerAnimator.SetClimbingDirection(up);
            hasPlayedClimbAnimFrame = true;
            pawn.playerAnimator.animator.speed = 1f;
        }
        else if (hasPlayedClimbAnimFrame)
        {
            pawn.playerAnimator.animator.speed = 0f;
            Debug.Log("[Climb] No input - Anim Paused");
        }

        if (pawn.WasJumpPressedThisFrame() && canJumpOffWall)
            JumpOffWall();
    }




    void JumpOffWall()
    {
        StopClimbing();
        float dir = pawn.IsFacingLeft() ? -1f : 1f;
        rb.velocity = new Vector2(-dir * jumpForceX, jumpForceY);
        pawn.TriggerJump();

        lastWallJumpTime = Time.time;
        pawn.SuppressNextAnimationUpdate();

        StartCoroutine(JumpCooldown());
        canRegrabWall = false;
        StartCoroutine(RegrabCooldown());
    }

    void StopClimbing(bool applyNudge = true)
    {
        if (!isClimbing) return;

        isClimbing = false;
        currentSurface = null;

        Debug.Log("[Climb] StopClimbing() triggered");

        if (applyNudge)
        {
            float direction = pawn.IsFacingLeft() ? 1f : -1f;
            transform.position += Vector3.right * climbExitNudge * direction;
            Debug.Log($"[Climb] Applied climb exit nudge: {climbExitNudge * direction}");

            climbSuppressionTimer = climbSuppressionDuration;
            Debug.Log("[Climb] Climb suppression activated");
        }

        rb.gravityScale = 4f;
        rb.velocity = new Vector2(rb.velocity.x, 0f);

        pawn.FreezeMovement(false);
        pawn.playerAnimator.SetClimbing(false);
        pawn.playerAnimator.SetClimbingDirection(false);
        pawn.playerAnimator.SetBool("Falling From Jump", false);
        pawn.playerAnimator.SetBool("ClimbingDown", false);
        pawn.playerAnimator.animator.speed = 1f;

        pawn.ResetDoubleJump();
    }




    public void SuppressClimbing()
    {
        if (Time.time - climbEntryTimestamp < climbEntryLockTime)
            return;

        isClimbSuppressed = true;
        climbSuppressionTimer = climbSuppressionDuration;

        if (isClimbing) StopClimbing();
    }

    public bool IsClimbing() => isClimbing;
    public bool IsMantling() => isMantling;

    public bool TrySetInteractionSurface(System.Func<SurfaceInteraction, bool> predicate, bool isClimbCheck = false)
    {
        if (isClimbSuppressed && isClimbCheck)
        {
            Debug.Log("[Climb] Suppressed — climb blocked");
            return false;
        }

        Vector2 origin = (Vector2)transform.position + climbCheckOffset;
        int mask = ~(1 << gameObject.layer);
        RaycastHit2D hit = Physics2D.BoxCast(origin, climbCheckSize, 0f, Vector2.zero, 0f, mask);

        if (hit.collider == null || hit.collider == GetComponent<Collider2D>())
        {
            Debug.Log("[Climb] No valid collider hit");
            return false;
        }

        WallInteractionSurface surface = hit.collider.GetComponent<WallInteractionSurface>();
        if (surface == null)
        {
            Debug.Log("[Climb] Hit object has no WallInteractionSurface");
            return false;
        }

        if (!TryGetSurfaceSide(hit.normal, out SurfaceSide sideHit))
        {
            Debug.Log("[Climb] Could not determine surface side");
            return false;
        }

        if (!surface.HasInteraction(sideHit, predicate))
        {
            Debug.Log("[Climb] Surface does not match predicate for side: " + sideHit);
            return false;
        }

        if (isClimbCheck)
        {
            float wallX = hit.collider.bounds.center.x;
            float playerX = transform.position.x;
            bool playerFacingLeft = pawn.IsFacingLeft();
            bool facingCorrectly = (playerFacingLeft && wallX < playerX) || (!playerFacingLeft && wallX > playerX);

            if (!facingCorrectly)
            {
                Debug.Log("[Climb] Facing wrong direction for climb");
                return false;
            }

            float surfaceTop = hit.collider.bounds.max.y;
            float playerTop = pawn.standingCollider.bounds.max.y;

            if (playerTop > surfaceTop + 0.05f) // ✅ allow climb if player is on ground
            {
                Debug.Log("[Climb] Player is clearly above wall top — climb blocked");
                return false;
            }
        }

        currentSurface = surface;
        climbableSurfaceTop = hit.collider.bounds.max.y;
        climbableSurfaceBottom = hit.collider.bounds.min.y;

        if (isClimbCheck && predicate.Invoke(new SurfaceInteraction { side = sideHit, isClimbable = true }))
        {
            Debug.Log("[Climb] Immediate StartClimbing triggered");
            StartClimbing();
            return true;
        }

        return true;
    }



    void StartClimbing()
    {
        hasPlayedClimbAnimFrame = false;
        StartCoroutine(DelaySetPlayedFrame());
        pawn.playerAnimator.SetBool("Falling From Jump", false);

        transform.position += Vector3.up * climbEntryLift;

        isClimbing = true;
        isMantling = false;
        rb.gravityScale = 0f;
        rb.velocity = Vector2.zero;

        pawn.FreezeMovement(true);
        pawn.playerAnimator.SetClimbing(true);
        pawn.playerAnimator.animator.speed = 1f;

        climbEntryTimestamp = Time.time;
        climbStartTime = Time.time; // ✅ Start climb grace timer
    }


    public bool CanMantle()
    {
        if (Time.time - lastWallJumpTime < mantleLockoutAfterJump)
            return false;

        WallInteractionSurface[] surfaces = GameObject.FindObjectsOfType<WallInteractionSurface>();
        foreach (var surface in surfaces)
        {
            if (!surface.HasInteraction(SurfaceSide.Top, i => i.isMantleable)) continue;

            Collider2D col = surface.GetComponent<Collider2D>();
            if (col == null) continue;

            Bounds b = col.bounds;
            Vector2 playerPos = transform.position;
            Vector2 topLeft = new Vector2(b.min.x, b.max.y);
            Vector2 topRight = new Vector2(b.max.x, b.max.y);
            float playerTopY = GetComponent<Collider2D>().bounds.max.y;

            bool nearCorner = Vector2.Distance(playerPos, topLeft) <= mantleCornerRadius ||
                              Vector2.Distance(playerPos, topRight) <= mantleCornerRadius;

            if (playerTopY < b.max.y - 0.01f && nearCorner)
            {
                currentSurface = surface;
                return true;
            }
        }

        return false;
    }

    public void TryMantle()
    {
        if (!CanMantle()) return;

        Vector3 surfacePos = currentSurface.transform.position;
        float playerX = transform.position.x;
        float surfaceX = surfacePos.x;

        bool facingLeft = pawn.IsFacingLeft();
        bool facingCorrectDirection = (facingLeft && surfaceX < playerX) || (!facingLeft && surfaceX > playerX);
        if (!facingCorrectDirection) return;

        bool shouldFaceLeft = surfaceX < playerX;
        pawn.spriteRenderer.flipX = shouldFaceLeft;

        isMantling = true;
        StopClimbing(false);
        pawn.TriggerMantleAnim();
        pawn.NotifyMantleStart();

        Vector3 target = transform.position + new Vector3(shouldFaceLeft ? -mantleForwardOffset : mantleForwardOffset, mantleUpOffset, 0f);
        StartCoroutine(MantleLerp(target));
    }

    IEnumerator MantleLerp(Vector3 target)
    {
        Vector3 start = transform.position;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / mantleDuration;
            float x = mantleHorizontalCurve.Evaluate(t);
            float y = mantleVerticalCurve.Evaluate(t);
            transform.position = start + new Vector3((target - start).x * x, (target - start).y * y, 0f);
            yield return null;
        }

        isMantling = false;
        pawn.FreezeMovement(false);
    }
#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (showClimbCheckGizmo)
        {
            Gizmos.color = Color.blue;
            Vector2 origin = (Vector2)transform.position + climbCheckOffset;
            Gizmos.DrawWireCube(origin, climbCheckSize);
        }

        if (showMantleCornerRadiusGizmo && currentSurface != null)
        {
            Collider2D col = currentSurface.GetComponent<Collider2D>();
            if (col != null)
            {
                Bounds b = col.bounds;
                Vector2 topLeft = new Vector2(b.min.x, b.max.y);
                Vector2 topRight = new Vector2(b.max.x, b.max.y);

                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(topLeft, mantleCornerRadius);
                Gizmos.DrawWireSphere(topRight, mantleCornerRadius);
            }
        }

        if (showMantleTargetGizmo)
        {
            float surfaceX = currentSurface != null ? currentSurface.transform.position.x : transform.position.x;
            float playerX = transform.position.x;

            bool shouldFaceLeft = surfaceX < playerX;
            Vector3 target = transform.position + new Vector3(shouldFaceLeft ? -mantleForwardOffset : mantleForwardOffset, mantleUpOffset, 0f);

            Gizmos.color = Color.green;
            Gizmos.DrawSphere(target, 0.1f);
        }
    }
#endif



    IEnumerator DelaySetPlayedFrame() { yield return null; hasPlayedClimbAnimFrame = true; }
    IEnumerator JumpCooldown() { canJumpOffWall = false; yield return new WaitForSeconds(0.1f); canJumpOffWall = true; }
    IEnumerator RegrabCooldown() { yield return new WaitForSeconds(reGrabCooldown); canRegrabWall = true; }

    private bool TryGetSurfaceSide(Vector2 normal, out SurfaceSide side)
    {
        if (Vector2.Dot(normal, Vector2.left) > 0.9f)
            side = SurfaceSide.Left;
        else if (Vector2.Dot(normal, Vector2.right) > 0.9f)
            side = SurfaceSide.Right;
        else if (Vector2.Dot(normal, Vector2.down) > 0.9f)
            side = SurfaceSide.Top;
        else if (Vector2.Dot(normal, Vector2.up) > 0.9f)
            side = SurfaceSide.Bottom;
        else
        {
            side = SurfaceSide.Top;
            return false;
        }

        return true;
    }
}







