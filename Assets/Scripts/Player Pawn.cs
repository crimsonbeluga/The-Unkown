// File: PlayerPawn.cs
using UnityEngine;

public class PlayerPawn : MonoBehaviour
{
    private Rigidbody2D rb;
    public PlayerAnimator playerAnimator;
    public SpriteRenderer spriteRenderer;
    public WallInteractionHandler wallController;

    [Header("Movement Settings")]
    public float walkSpeed = 3f;
    public float runSpeed = 6f;
    public float acceleration = 15f;

    [Header("Air Control Settings")]
    public float airControlAcceleration = 5f;
    public float maxAirSpeed = 5f;

    [Header("Jump Settings")]
    public float jumpForce = 12f;
    public float doubleJumpForce = 10f;

    [Header("Crawl Settings")]
    public float crawlSpeed = 1.5f;
    public float crawlDecelerationRate = 5f;

    [Header("Roll Settings")]
    public float rollForce = 10f;
    public float rollDuration = 0.4f;
    public float rollCooldown = 1f;
    public bool rollInvincibility = true;

    [Header("Wall Interaction Settings")]
    public float mantleFallSuppressionDuration = 0.2f;
    public float climbSuppressionDuration = 1.0f;

    [Header("Hitbox Colliders")]
    public BoxCollider2D standingCollider;
    public BoxCollider2D crawlingCollider;

    [Header("Ground Detection")]
    public Transform groundCheckPoint;
    public Vector2 groundCheckSize = new Vector2(0.75f, 0.1f);
    public Vector2 groundCheckOffsetFacingRight = new Vector2(0.4f, 0f);
    public Vector2 groundCheckOffsetFacingLeft = new Vector2(-0.4f, 0f);

    [Header("Ground Detection - Crawl")]
    public Transform crawlGroundCheckPoint;
    public Vector2 crawlGroundCheckSize = new Vector2(0.75f, 0.1f);
    public Vector2 crawlGroundCheckOffset = Vector2.zero;

    [Header("Crouch Head Clearance Check")]
    public Transform crouchCheckPoint;
    public Vector2 crouchCheckSize = new Vector2(0.75f, 0.1f);
    public Vector2 crouchCheckOffset = new Vector2(0f, 1f);

    [Header("Ground Snapping")]
    public bool enableGroundSnap = true;
    [Range(0.001f, 0.2f)]
    public float groundSnapTolerance = 0.05f;

    [Header("Grounded Grace Settings")]
    public float groundedCoyoteTime = 0.1f;

    // Add this field to Sliding Settings
    [Header("Sliding Settings")]
    public float slidingThreshold = 2.5f;
    private float lastRunReleaseTime;
    public float slideInputBufferTime = 0.2f;

    public float slideDuration = 0.75f; // <-- New
    private float slideTimer = 0f;       // <-- New

    [Header("Debug Gizmo Toggles")]
    public bool showGroundCheckGizmo = true;
    public bool showCrawlCheckGizmo = true;
    public bool showHeadClearanceGizmo = true;
    public bool showCrawlColliderGizmo = true;
    public bool showStandingColliderGizmo = true;

    private bool isGrounded;
    private bool isActuallyGrounded;
    private float lastGroundedTime;
    private float lastCrawlGroundedTime;

    private bool hasDoubleJumped;
    private Vector2 currentInput;
    private float currentSpeed;

    private bool crawlHeld;
    private bool runHeld;
    private float runTime;

    private bool isRolling;
    private float rollTimer;
    private float lastRollTime;

    private bool isFrozen;
    private bool isWallJumping;
    private bool suppressFlipUntilWallJumpAnimOver;
    private float wallJumpFlipSuppressTimer;
    public float wallJumpAnimDuration = 0.3f;

    private float lastMantleTime;
    private bool isInPostMantle = false;

    private bool jumpRequestedThisFrame;
    private bool suppressNextAnimationUpdate = false;
    private float suppressFlipUntilTime = 0f;

    private bool suppressFallingFromJump = false;

    private Vector2 crawlColliderDefaultOffset;
    private bool isPushingOrPulling = false;

    private PlatformMover currentPlatform;
    private bool isSliding = false;

    private bool wantsToSlideOnLand = false;

    private bool CanStandUp => !Physics2D.OverlapBox((Vector2)crouchCheckPoint.position + crouchCheckOffset, crouchCheckSize, 0f);

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerAnimator = GetComponent<PlayerAnimator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        wallController = GetComponent<WallInteractionHandler>();
        crawlColliderDefaultOffset = crawlingCollider.offset;
    }

    void Update()
    {
        if (suppressFlipUntilWallJumpAnimOver)
        {
            wallJumpFlipSuppressTimer -= Time.deltaTime;
            if (wallJumpFlipSuppressTimer <= 0f)
                suppressFlipUntilWallJumpAnimOver = false;
        }

        if (suppressFlipUntilTime > 0f)
        {
            suppressFlipUntilTime -= Time.deltaTime;
        }
    }

    void FixedUpdate()
    {
        if (currentPlatform != null)
        {
            rb.position += (Vector2)currentPlatform.DeltaMovement;
        }

        CheckGrounded();

        if (isRolling || isFrozen)
        {
            if (isRolling)
            {
                rollTimer -= Time.fixedDeltaTime;
                if (rollTimer <= 0)
                    isRolling = false;
            }
            return;
        }

        ApplyMovement();

        bool isSuppressed = Time.time - lastMantleTime < mantleFallSuppressionDuration || isInPostMantle;
        bool climbingOverride = wallController.IsClimbing() || wallController.IsMantling();
        bool isFalling = !isGrounded && rb.velocity.y < -0.1f && !isSuppressed && !suppressFallingFromJump && !climbingOverride;

        if (!wallController.IsMantling() && !suppressNextAnimationUpdate)
        {
            playerAnimator.UpdateAnimation(
                Mathf.Abs(rb.velocity.x),
                isGrounded,
                crawlHeld,
                isSliding,
                isFalling
            );
        }

        suppressNextAnimationUpdate = false;
        jumpRequestedThisFrame = false;
    }

    private void ApplyMovement()
    {
        if (isSliding && isGrounded)
        {
            slideTimer -= Time.fixedDeltaTime;

            // Gradually slow down from runSpeed to crawlSpeed
            float currentX = rb.velocity.x;
            float targetX = Mathf.Sign(currentX) * crawlSpeed;
            float decelerationStep = crawlDecelerationRate * Time.fixedDeltaTime;
            float newX = Mathf.MoveTowards(currentX, targetX, decelerationStep);
            rb.velocity = new Vector2(newX, rb.velocity.y);

            playerAnimator.SetCrawlSpeed(Mathf.Abs(newX));
            playerAnimator.SetBool("isSliding", true);

            // End slide if speed reaches crawlSpeed or timer ends
            if (Mathf.Abs(newX) <= crawlSpeed || slideTimer <= 0f)
            {
                isSliding = false;
                playerAnimator.SetBool("isSliding", false);

                if (crawlHeld && isGrounded)
                {
                    // Transition to crawl state
                    currentSpeed = crawlSpeed;
                    float crawlDirection = Mathf.Sign(newX);
                    rb.velocity = new Vector2(crawlDirection * crawlSpeed, rb.velocity.y);
                    playerAnimator.SetCrawlSpeed(Mathf.Abs(rb.velocity.x));

                    // Enable correct collider
                    if (standingCollider && crawlingCollider)
                    {
                        standingCollider.enabled = false;
                        crawlingCollider.enabled = true;
                    }

                    playerAnimator.SetBool("isSliding", false);
                }
                else
                {
                    // Stop if crawl not held
                    rb.velocity = new Vector2(0f, rb.velocity.y);
                    currentSpeed = 0f;
                }
            }

            return; // Don't allow other movement logic while sliding
        }

        // CRAWLING
        if (crawlHeld && isGrounded)
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, crawlSpeed, crawlDecelerationRate * Time.fixedDeltaTime);
            rb.velocity = new Vector2(currentInput.x * currentSpeed, rb.velocity.y);

            playerAnimator.SetCrawlSpeed(Mathf.Abs(rb.velocity.x));
            playerAnimator.SetBool("isSliding", false);
        }
        // RUNNING/WALKING
        else if (isGrounded)
        {
            float targetSpeed = (runHeld && !crawlingCollider.enabled) ? runSpeed : walkSpeed;
            currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, acceleration * Time.fixedDeltaTime);
            rb.velocity = new Vector2(currentInput.x * currentSpeed, rb.velocity.y);

            playerAnimator.SetBool("isSliding", false);
        }
        // AIR MOVEMENT
        else
        {
            float airAccel = airControlAcceleration * Time.fixedDeltaTime;
            float targetVelocityX = rb.velocity.x + currentInput.x * airAccel;
            targetVelocityX = Mathf.Clamp(targetVelocityX, -maxAirSpeed, maxAirSpeed);
            rb.velocity = new Vector2(targetVelocityX, rb.velocity.y);

            playerAnimator.SetBool("isSliding", false);
        }

        // Flip sprite
        if (!isSliding && !suppressFlipUntilWallJumpAnimOver && suppressFlipUntilTime <= 0f && !wallController.IsMantling() && currentInput.x != 0)
        {
            spriteRenderer.flipX = currentInput.x < 0;
        }

        // Crawl collider offset (for sprite flipping)
        float xFlip = spriteRenderer.flipX ? -1f : 1f;
        Vector2 offset = crawlColliderDefaultOffset;
        offset.x *= xFlip;
        crawlingCollider.offset = offset;

        // Collider switching
        if (standingCollider && crawlingCollider)
        {
            if (crawlHeld && isGrounded)
            {
                standingCollider.enabled = false;
                crawlingCollider.enabled = true;
            }
            else
            {
                if (CanStandUp)
                {
                    crawlingCollider.enabled = false;
                    standingCollider.enabled = true;
                }
                else
                {
                    crawlHeld = true;
                }
            }
        }
    }





    private void CheckGrounded()
    {
        Vector2 checkPosition;
        Vector2 checkSize;

        if (crawlHeld && crawlingCollider.enabled)
        {
            checkPosition = (Vector2)crawlGroundCheckPoint.position + crawlGroundCheckOffset;
            checkSize = crawlGroundCheckSize;
        }
        else
        {
            Vector2 offset = spriteRenderer.flipX ? groundCheckOffsetFacingLeft : groundCheckOffsetFacingRight;
            checkPosition = (Vector2)groundCheckPoint.position + offset;
            checkSize = new Vector2(1.25f, groundCheckSize.y);
        }

        Collider2D[] hits = Physics2D.OverlapBoxAll(checkPosition, checkSize, 0f);
        isActuallyGrounded = false;

        foreach (var col in hits)
        {
            if (col == null || col == GetComponent<Collider2D>()) continue;

            WallInteractionSurface surface = col.GetComponent<WallInteractionSurface>();
            if (surface != null && surface.HasInteraction(SurfaceSide.Top, i => i.isWalkable))
            {
                Vector2 contactPoint = col.ClosestPoint(transform.position);
                Vector2 normal = (Vector2)transform.position - contactPoint;
                normal.Normalize();

                if (normal.y >= 0.25f && rb.velocity.y <= 0.1f)
                {
                    isActuallyGrounded = true;
                    lastGroundedTime = Time.time;
                    if (crawlHeld && crawlingCollider.enabled)
                        lastCrawlGroundedTime = Time.time;

                    float desiredY = contactPoint.y + standingCollider.bounds.extents.y;
                    float yDiff = Mathf.Abs(transform.position.y - desiredY);

                    if (enableGroundSnap && yDiff < groundSnapTolerance)
                    {
                        transform.position = new Vector3(transform.position.x, desiredY, transform.position.z);
                        rb.velocity = new Vector2(rb.velocity.x, 0f);
                    }

                    break;
                }
            }
        }

        isGrounded = crawlHeld && crawlingCollider.enabled
            ? isActuallyGrounded || (Time.time - lastCrawlGroundedTime < groundedCoyoteTime)
            : isActuallyGrounded;

        playerAnimator.SetBool("isGrounded", isGrounded);

        if (isGrounded)
        {
            if (wantsToSlideOnLand && runHeld && Mathf.Abs(rb.velocity.x) > slidingThreshold)
            {
                isSliding = true;
                crawlHeld = true;
                playerAnimator.SetBool("isSliding", true);
            }

            wantsToSlideOnLand = false;
            isInPostMantle = false;
            hasDoubleJumped = false;
            SetWallJumping(false);
            playerAnimator.SetBool("Falling From Jump", false);
            suppressFallingFromJump = false;
        }
    }


    public void ReceiveInput(Vector2 moveInput, bool jumpRequested, bool crouch, bool run, bool crawl)
    {
        currentInput = moveInput;

        // Don't allow crawl input to interfere while sliding
        if (!isSliding && !isPushingOrPulling)
        {
            crawlHeld = crawl;
        }

        if (runHeld && !run)
            lastRunReleaseTime = Time.time;

        runHeld = isPushingOrPulling || suppressRunInputFlag ? false : run;

        if (jumpRequested)
            jumpRequestedThisFrame = true;

        runTime = run && Mathf.Abs(moveInput.x) > 0.1f && isGrounded ? runTime + Time.deltaTime : 0f;

        // Store if player wants to slide while airborne and lands soon
        if (!isGrounded && crawl && runHeld && Mathf.Abs(rb.velocity.x) > slidingThreshold)
        {
            wantsToSlideOnLand = true;
        }

        // Check if we can start sliding now
        bool canSlideNow =
            !isRolling &&
            crawl &&
            isGrounded &&
            Mathf.Abs(rb.velocity.x) > slidingThreshold &&
            (runHeld || Time.time - lastRunReleaseTime < slideInputBufferTime);

        if (canSlideNow)
        {
            isSliding = true;
            slideTimer = slideDuration;
            crawlHeld = false; // prevent crawl from immediately overriding slide
            wantsToSlideOnLand = false;
            playerAnimator.SetBool("isSliding", true);
        }

        // Exit sliding if crawl released during slide
        if (isSliding && !crawl)
        {
            isSliding = false;
            playerAnimator.SetBool("isSliding", false);
        }

        // Jump logic
        if (jumpRequested && !crawlHeld && !isRolling && !isPushingOrPulling)
        {
            if (!isGrounded)
            {
                if (wallController.TryWallJump(currentInput))
                {
                    SetWallJumping(true);
                    suppressFlipUntilWallJumpAnimOver = true;
                    wallJumpFlipSuppressTimer = wallJumpAnimDuration;
                    playerAnimator.TriggerWallJump();
                    return;
                }

                if (!hasDoubleJumped && !IsWallJumping())
                {
                    rb.velocity = new Vector2(rb.velocity.x, doubleJumpForce);
                    hasDoubleJumped = true;
                    playerAnimator.TriggerDoubleJump();
                }
            }
            else
            {
                rb.velocity = new Vector2(rb.velocity.x, jumpForce);
                playerAnimator.TriggerJump();
                hasDoubleJumped = false;
            }
        }
    }


    public void TriggerRoll()
    {
        if (Time.time - lastRollTime < rollCooldown || isRolling || !isGrounded || crawlHeld || isPushingOrPulling)
            return;

        isRolling = true;
        rollTimer = rollDuration;
        lastRollTime = Time.time;
        float direction = spriteRenderer.flipX ? -1f : 1f;
        rb.velocity = new Vector2(direction * rollForce, rb.velocity.y);
        playerAnimator.TriggerRoll();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("MovingPlatform"))
        {
            transform.SetParent(collision.transform);
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("MovingPlatform"))
        {
            transform.SetParent(null);
        }
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (showGroundCheckGizmo && groundCheckPoint != null)
        {
            Vector2 offset = spriteRenderer != null && spriteRenderer.flipX ? groundCheckOffsetFacingLeft : groundCheckOffsetFacingRight;
            Vector2 pos = (Vector2)groundCheckPoint.position + offset;
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(pos, new Vector2(1.25f, groundCheckSize.y));
        }

        if (showCrawlCheckGizmo && crawlGroundCheckPoint != null)
        {
            Vector2 pos = (Vector2)crawlGroundCheckPoint.position + crawlGroundCheckOffset;
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(pos, crawlGroundCheckSize);
        }

        if (showHeadClearanceGizmo && crouchCheckPoint != null)
        {
            Vector2 pos = (Vector2)crouchCheckPoint.position + crouchCheckOffset;
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireCube(pos, crouchCheckSize);
        }

        if (showCrawlColliderGizmo && crawlingCollider != null)
        {
            Gizmos.color = Color.cyan;
            Vector2 crawlOffset = crawlColliderDefaultOffset;
            float xFlip = spriteRenderer != null && spriteRenderer.flipX ? -1f : 1f;
            crawlOffset.x *= xFlip;
            Vector2 center = (Vector2)transform.position + crawlOffset;
            Gizmos.DrawWireCube(center, crawlingCollider.size);
        }

        if (showStandingColliderGizmo && standingCollider != null)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawWireCube(standingCollider.bounds.center, standingCollider.bounds.size);
        }
    }
#endif

    public void SetPushingOrPulling(bool state) => isPushingOrPulling = state;
    public void SuppressFallingFromJump() => suppressFallingFromJump = true;
    public void FreezeMovement(bool freeze) { isFrozen = freeze; rb.velocity = Vector2.zero; }
    public void TriggerMantleAnim() => playerAnimator.TriggerMantle();
    public void TriggerJump() => playerAnimator.TriggerJump();
    public void TriggerWallJump() => playerAnimator.TriggerWallJump();
    public void SuppressNextAnimationUpdate() => suppressNextAnimationUpdate = true;
    public void NotifyMantleStart() { lastMantleTime = Time.time; suppressFlipUntilTime = 0.3f; }

    public bool WasJumpPressedThisFrame() => jumpRequestedThisFrame;
    public void SetWallJumping(bool state) => isWallJumping = state;
    public bool IsWallJumping() => isWallJumping;
    public bool IsRunHeld() => runHeld;

    public void ResetDoubleJump() => hasDoubleJumped = false;
    public bool IsFacingLeft() => spriteRenderer != null && spriteRenderer.flipX;
    public bool IsGrounded() => isGrounded;
    public Vector2 GetInput() => currentInput;
    public Vector2 GetVelocity() => rb.velocity;
    public bool WasRecentlyGrounded(float gracePeriod = 0.1f) => Time.time - lastGroundedTime < gracePeriod;

    private bool suppressRunInputFlag = false;
    public void SuppressRunInput() => suppressRunInputFlag = true;
    public void ReleaseRunSuppression() => suppressRunInputFlag = false;
    public bool IsRunSuppressed() => suppressRunInputFlag;
}
