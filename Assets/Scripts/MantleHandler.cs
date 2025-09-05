using UnityEngine;

//File: MantleHandler.cs
using UnityEngine;

public class MantleHandler : MonoBehaviour
{
    [Header("Ledge Detection")]
    public Transform ledgeCheck;
    public LayerMask mantleableLayer;
    public Vector2 checkBoxSize = new Vector2(0.6f, 0.4f);
    public Vector3 checkOffset = Vector3.zero;
    public float checkDistance = 0.5f;

    [Header("Mantle Settings")]
    public float mantleHeight = 1f;
    public float mantleDuration = 0.25f;
    public AnimationCurve mantleCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

    [Header("Mantle Direction")]
    public Vector2 inputDirection = Vector2.right;
    public float minHorizontalOffset = 0.3f;
    public float topMargin = 0.1f;

    [Header("Fall Mantle Guard")]
    public float airborneTimeThreshold = 0.2f;

    private PlayerPawn pawn;
    private bool isMantling;
    private Vector3 start;
    private Vector3 target;
    private float timer;

    [Header("Mantle Cooldown")]
    public float mantleCooldownDuration = 0.3f;
    private float lastMantleTime = -999f;
    private Collider2D lastMantledLedge;

    void Awake() => pawn = GetComponent<PlayerPawn>();

    void Update()
    {
        if (isMantling)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / mantleDuration);
            float height = mantleCurve.Evaluate(t);
            transform.position = Vector3.Lerp(start, target, t) + Vector3.up * height;

            if (t >= 1f)
                EndMantle();
        }
        else
        {
            AutoMantleCheck();
        }
    }

    private void AutoMantleCheck()
    {
        if (Time.time - lastMantleTime < mantleCooldownDuration) return;
        if (pawn.WasRecentlyGrounded(airborneTimeThreshold)) return;

        RaycastHit2D hit = Physics2D.BoxCast(
            ledgeCheck.position + checkOffset,
            checkBoxSize,
            0f,
            Vector2.down,
            checkDistance,
            mantleableLayer
        );

        if (hit.collider != null && hit.collider.CompareTag("Mantleable"))
        {
            if (hit.collider == lastMantledLedge) return;

            Bounds b = hit.collider.bounds;
            if (!IsAboveLedge(b)) return;

            float horizontalOffset = Mathf.Abs(transform.position.x - b.center.x);
            if (horizontalOffset < minHorizontalOffset) return;

            Vector2 corner = GetMantleCorner(b);
            StartMantle(corner, hit.collider);
        }
    }

    public void TryMantle()
    {
        if (isMantling || Time.time - lastMantleTime < mantleCooldownDuration) return;
        if (pawn.WasRecentlyGrounded(airborneTimeThreshold)) return;

        RaycastHit2D hit = Physics2D.BoxCast(
            ledgeCheck.position + checkOffset,
            checkBoxSize,
            0f,
            Vector2.down,
            checkDistance,
            mantleableLayer
        );

        if (hit.collider != null && hit.collider.CompareTag("Mantleable"))
        {
            if (hit.collider == lastMantledLedge) return;

            Bounds b = hit.collider.bounds;
            if (!IsAboveLedge(b)) return;

            float horizontalOffset = Mathf.Abs(transform.position.x - b.center.x);
            if (horizontalOffset < minHorizontalOffset) return;

            Vector2 corner = GetMantleCorner(b);
            StartMantle(corner, hit.collider);
        }
    }

    public bool CanMantle()
    {
        if (Time.time - lastMantleTime < mantleCooldownDuration) return false;
        if (pawn.WasRecentlyGrounded(airborneTimeThreshold)) return false;

        RaycastHit2D hit = Physics2D.BoxCast(
            ledgeCheck.position + checkOffset,
            checkBoxSize,
            0f,
            Vector2.down,
            checkDistance,
            mantleableLayer
        );

        if (hit.collider == null || !hit.collider.CompareTag("Mantleable") || hit.collider == lastMantledLedge) return false;

        Bounds b = hit.collider.bounds;
        if (!IsAboveLedge(b)) return false;

        float horizontalOffset = Mathf.Abs(transform.position.x - b.center.x);
        if (horizontalOffset < minHorizontalOffset) return false;

        return true;
    }

    public bool IsNearLedgeFromBelow()
    {
        RaycastHit2D hit = Physics2D.BoxCast(
            ledgeCheck.position + checkOffset,
            checkBoxSize,
            0f,
            Vector2.down,
            checkDistance,
            mantleableLayer
        );

        if (hit.collider == null || hit.collider == lastMantledLedge || !hit.collider.CompareTag("Mantleable")) return false;

        Bounds b = hit.collider.bounds;
        return transform.position.y < b.max.y - topMargin;
    }

    public void ForceMantle(Vector2 topCorner, Collider2D ledge)
    {
        StartMantle(topCorner, ledge);
    }

    private bool IsAboveLedge(Bounds b) => transform.position.y > b.max.y - topMargin;

    private Vector2 GetMantleCorner(Bounds bounds)
    {
        float side = transform.position.x < bounds.center.x ? bounds.min.x : bounds.max.x;
        return new Vector2(side, bounds.max.y);
    }

    private void StartMantle(Vector2 topCorner, Collider2D ledge)
    {
        isMantling = true;
        start = transform.position;
        target = new Vector3(topCorner.x, topCorner.y + mantleHeight, transform.position.z);
        timer = 0f;
        lastMantleTime = Time.time;
        lastMantledLedge = ledge;
        pawn.FreezeMovement(true);
        pawn.NotifyMantleStart();
        pawn.TriggerMantleAnim();
    }

    private void EndMantle()
    {
        isMantling = false;
        pawn.FreezeMovement(false);
        lastMantledLedge = null;
    }

    public bool IsMantling() => isMantling;
}
