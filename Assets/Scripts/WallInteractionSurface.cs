using UnityEngine;

public enum SurfaceSide { Top, Left, Right, Bottom }

[System.Serializable]
public struct SurfaceInteraction
{
    public SurfaceSide side;
    public bool isWalkable;
    public bool isClimbable;
    public bool isMantleable;
    public bool isWallJumpable;
}

public class WallInteractionSurface : MonoBehaviour
{
    public SurfaceInteraction[] interactions;

    [Header("Mantle Corner Bounds Override")]
    public Vector2 cornerBoxSize = new Vector2(0.5f, 0.5f);
    public Vector2 cornerBoxOffset = Vector2.zero;

    public bool HasInteraction(SurfaceSide side, System.Func<SurfaceInteraction, bool> condition)
    {
        foreach (var i in interactions)
        {
            if (i.side == side && condition(i))
                return true;
        }
        return false;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        var bounds = GetComponent<Collider2D>()?.bounds ?? new Bounds(transform.position, Vector3.one);

        Vector3 topLeft = new Vector3(bounds.min.x + cornerBoxOffset.x, bounds.max.y + cornerBoxOffset.y, 0f);
        Vector3 topRight = new Vector3(bounds.max.x - cornerBoxOffset.x, bounds.max.y + cornerBoxOffset.y, 0f);

        Gizmos.DrawWireCube(topLeft, cornerBoxSize);
        Gizmos.DrawWireCube(topRight, cornerBoxSize);
    }

    public static bool TryGetSurfaceSide(Vector2 normal, out SurfaceSide side)
    {
        if (Vector2.Dot(normal, Vector2.left) > 0.9f)
            side = SurfaceSide.Right; // Corrected: hit from left means right side
        else if (Vector2.Dot(normal, Vector2.right) > 0.9f)
            side = SurfaceSide.Left;  // Corrected: hit from right means left side
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


