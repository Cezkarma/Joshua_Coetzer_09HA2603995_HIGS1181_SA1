using UnityEngine;

/// <summary>
/// Enemy AI for a turn-based grid game.
///
/// ── MODIFICATIONS FROM ORIGINAL SCRIPT ──────────────────────
/// The original script moved the enemy in a random direction each
/// turn and contained TODO comments instructing students to replace
/// that logic. The following changes have been made:
///
///  1. Added enemyHP and attackDamage fields.
///  2. Added blockingLayer and playerLayer references.
///  3. Replaced the random movement in TakeTurn() with player-
///     detection and directional chasing logic.
///  4. Added GetDirectionToPlayer() – returns the best Vector2
///     direction to step closer to the player (custom method).
///  5. Added IsAdjacentToPlayer() – returns true when the enemy
///     is one tile away and should attack instead of move (custom
///     method with return value).
///  6. Added GetPerpendicularDirection() to handle wall detours.
///  7. Added AttackPlayer() and TakeDamage() for combat.
///  8. Added OnTriggerEnter2D() for trigger-based overlap detection.
///  9. Added comprehensive Debug.Log() statements throughout.
/// ─────────────────────────────────────────────────────────────
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
public class EnemyAI : MonoBehaviour
{
    // ── Global variables ──────────────────────────────────────
    [Header("Movement Settings")]
    [Tooltip("Time (in seconds) it takes to move one tile.")]
    public float moveTime = 0.2f;

    [Header("Combat Settings")]
    public int enemyHP     = 5;
    public int attackDamage = 2;

    [Header("Detection Layers")]
    public LayerMask blockingLayer; // Assign the Wall layer
    public LayerMask playerLayer;   // Assign the Player layer

    // Cached component references (global to this class)
    private Rigidbody2D rb2D;
    private BoxCollider2D col2D;

    // Cached player transform – found once in Start
    private Transform playerTransform;

    // ── Unity Lifecycle ──────────────────────────────────────

    private void Awake()
    {
        // GetComponent<> – cache Rigidbody2D and BoxCollider2D
        rb2D  = GetComponent<Rigidbody2D>();
        col2D = GetComponent<BoxCollider2D>();
    }

    private void Start()
    {
        // Locate the player once by tag (set player GameObject tag to "Player")
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
            playerTransform = playerObj.transform;
        else
            Debug.LogWarning("EnemyAI: No GameObject tagged 'Player' found in scene.");
    }

    // ── Turn Logic (MODIFIED) ────────────────────────────────

    /// <summary>
    /// Called by GameManager each time it is the enemy's turn.
    /// MODIFIED: replaces random movement with player-chasing logic.
    /// The enemy acts exactly once per call (one tile move OR attack).
    /// </summary>
    public void TakeTurn()
    {
        if (playerTransform == null) return;

        // MODIFIED: attack the player if already adjacent (1 tile)
        if (IsAdjacentToPlayer())
        {
            AttackPlayer();
            return; // one action per turn
        }

        // MODIFIED: calculate which direction brings us closer
        Vector2 moveDir   = GetDirectionToPlayer();
        Vector2 targetPos = rb2D.position + moveDir;

        // MODIFIED: check for walls before committing to the move
        Collider2D wallHit = Physics2D.OverlapBox(
            targetPos, Vector2.one * 0.8f, 0f, blockingLayer);

        if (wallHit != null)
        {
            // Primary direction blocked – try the perpendicular axis
            Vector2 altDir    = GetPerpendicularDirection(moveDir);
            Vector2 altTarget = rb2D.position + altDir;

            Collider2D altWallHit = Physics2D.OverlapBox(
                altTarget, Vector2.one * 0.8f, 0f, blockingLayer);

            if (altWallHit != null)
            {
                Debug.Log("Enemy movement blocked on both axes – waiting.");
                return; // completely blocked this turn
            }

            targetPos = altTarget;
        }

        // Move one tile toward the player, respecting the grid
        rb2D.MovePosition(targetPos);
        Debug.Log("Enemy moved toward player to " + targetPos);
    }

    // ── Custom Methods ───────────────────────────────────────

    /// <summary>
    /// Returns the single-tile direction (horizontal OR vertical) that
    /// closes the gap between this enemy and the player most efficiently.
    /// Prioritises whichever axis has the larger distance.
    /// </summary>
    private Vector2 GetDirectionToPlayer()
    {
        // Local variables – distance on each axis
        float dx = playerTransform.position.x - rb2D.position.x;
        float dy = playerTransform.position.y - rb2D.position.y;

        // Move along the axis with the greater gap (no diagonal movement)
        if (Mathf.Abs(dx) >= Mathf.Abs(dy))
            return new Vector2(Mathf.Sign(dx), 0f);
        else
            return new Vector2(0f, Mathf.Sign(dy));
    }

    /// <summary>
    /// Returns true when the player is exactly one tile away (Manhattan
    /// distance ≤ 1), indicating the enemy should attack rather than move.
    /// </summary>
    private bool IsAdjacentToPlayer()
    {
        // Local distance variable – Manhattan distance on a grid
        float dist = Vector2.Distance(rb2D.position, playerTransform.position);
        return dist <= 1.1f; // 1.0 = one tile, small tolerance for float precision
    }

    /// <summary>
    /// Returns the perpendicular direction to the one supplied.
    /// Used to navigate around walls when the primary path is blocked.
    /// </summary>
    private Vector2 GetPerpendicularDirection(Vector2 primary)
    {
        // Local variables for axis-flip calculation
        float dx = playerTransform.position.x - rb2D.position.x;
        float dy = playerTransform.position.y - rb2D.position.y;

        if (primary.x != 0)
            return new Vector2(0f, Mathf.Sign(dy)); // was horizontal → try vertical
        else
            return new Vector2(Mathf.Sign(dx), 0f); // was vertical → try horizontal
    }

    // ── Combat ───────────────────────────────────────────────

    // Sends attack damage to the player via GameManager
    private void AttackPlayer()
    {
        Debug.Log("Enemy Attacked Player for " + attackDamage + " damage.");
        GameManager.instance.PlayerTakeDamage(attackDamage);
    }

    /// <summary>
    /// Reduces this enemy's HP by the given amount.
    /// Destroys the enemy and notifies the GameManager when HP reaches zero.
    /// Called by PlayerController.AttackEnemy().
    /// </summary>
    public void TakeDamage(int damage)
    {
        enemyHP -= damage;
        Debug.Log("Enemy took " + damage + " damage. HP remaining: " + enemyHP);

        if (enemyHP <= 0)
        {
            Debug.Log("Player Defeated Enemy – enemy destroyed.");
            GameManager.instance.EnemyDefeated();
            Destroy(gameObject);
        }
    }

    // ── Trigger Detection ────────────────────────────────────

    /// <summary>
    /// Fires when the player's collider overlaps this enemy's trigger zone.
    /// Confirms physical overlap used by the combat system.
    /// </summary>
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player entered enemy trigger zone.");
        }
    }
}
