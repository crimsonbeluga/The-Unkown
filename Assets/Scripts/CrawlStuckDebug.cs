using UnityEngine;

public class CrawlStuckDebug : MonoBehaviour
{
    public BoxCollider2D crawlingCollider;
    public LayerMask crawlStuckLayerMask;
    public float castDistance = 0.1f;

    [Header("Debug Adjustments")]
    public Vector2 boxSizeOverride = Vector2.zero;
    public Vector2 boxOffset = Vector2.zero;

    void OnDrawGizmosSelected()
    {
        if (!crawlingCollider) return;

        Vector2 origin = (Vector2)crawlingCollider.bounds.center + boxOffset;
        Vector2 size = boxSizeOverride != Vector2.zero ? boxSizeOverride : crawlingCollider.size;
        Vector2 direction = Vector2.right * Mathf.Sign(transform.localScale.x);

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(origin + direction * castDistance, size);
    }
}
