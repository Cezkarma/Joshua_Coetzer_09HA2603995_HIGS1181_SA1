using UnityEngine;

/// <summary>
/// Marker component for wall tiles.
/// Ensures this GameObject has a solid (non-trigger) BoxCollider2D
/// so it correctly blocks player and enemy movement.
/// Place this on every wall/boundary prefab in the scene.
/// </summary>
[RequireComponent(typeof(BoxCollider2D))]
public class WallTile : MonoBehaviour
{
    // Cached component reference
    private BoxCollider2D col;

    private void Awake()
    {
        // GetComponent<> – retrieve and configure the collider
        col = GetComponent<BoxCollider2D>();
        col.isTrigger = false; // walls must be solid, never triggers
    }
}
