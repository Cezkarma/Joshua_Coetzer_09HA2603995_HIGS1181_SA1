using TMPro;
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

    [Header("UI")]
    [Tooltip("Drag a child GameObject's TextMeshPro (non-UI) component here to show remaining HP.")]
    public TextMeshPro hpLabel;

    // Cached component references (global to this class)
    private Rigidbody2D rb2D;
    private BoxCollider2D col2D;

    // Cached player references – found once in Start
    private Transform playerTransform;
    private PlayerController playerController;

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
        {
            playerTransform  = playerObj.transform;
            // Cache PlayerController so we can read GridPosition – the
            // logically-correct position that updates before physics does.
            playerController = playerObj.GetComponent<PlayerController>();
        }
        else
        {
            Debug.LogWarning("EnemyAI: No GameObject tagged 'Player' found in scene.");
        }

        // Show full HP on the label as soon as the enemy spawns
        UpdateHPLabel();
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

        // Don't step onto the player's tile – attack from the adjacent tile instead.
        // This prevents the enemy and player from ever sharing the same position.
        Collider2D playerAtTarget = Physics2D.OverlapBox(
            targetPos, Vector2.one * 0.8f, 0f, playerLayer);

        if (playerAtTarget != null)
        {
            Debug.Log("Enemy target tile occupied by player – attacking instead of moving.");
            AttackPlayer();
            return;
        }

        // Tile is clear of both walls and the player – move one step closer
        rb2D.MovePosition(targetPos);
        Debug.Log("Enemy moved toward player to " + targetPos);
    }

    // ── Custom Methods ───────────────────────────────────────

    /// <summary>
    /// Returns the player's current logical grid position.
    /// Reads PlayerController.GridPosition when available – this value is
    /// updated the moment the player commits a move, before Unity's physics
    /// step runs, so it is always correct even within the same frame.
    /// Falls back to transform.position if the component is missing.
    /// </summary>
    private Vector2 GetPlayerPosition()
    {
        if (playerController != null)
            return playerController.GridPosition;

        return playerTransform.position;
    }

    /// <summary>
    /// Returns the single-tile direction (horizontal OR vertical) that
    /// closes the gap between this enemy and the player most efficiently.
    /// Prioritises whichever axis has the larger distance.
    /// </summary>
    private Vector2 GetDirectionToPlayer()
    {
        // Local variables – distance on each axis
        Vector2 playerPos = GetPlayerPosition();
        float dx = playerPos.x - rb2D.position.x;
        float dy = playerPos.y - rb2D.position.y;

        // Move along the axis with the greater gap (no diagonal movement)
        if (Mathf.Abs(dx) >= Mathf.Abs(dy))
            return new Vector2(Mathf.Sign(dx), 0f);
        else
            return new Vector2(0f, Mathf.Sign(dy));
    }

    /// <summary>
    /// Returns true when the player is exactly one tile away (Manhattan
    /// distance ≤ 1), indicating the enemy should attack rather than move.
    /// Uses GridPosition so the check is accurate in the same frame the
    /// player moves.
    /// </summary>
    private bool IsAdjacentToPlayer()
    {
        // Local distance variable – Manhattan distance on a grid
        float dist = Vector2.Distance(rb2D.position, GetPlayerPosition());
        return dist <= 1.1f; // 1.0 = one tile, small tolerance for float precision
    }

    /// <summary>
    /// Returns the perpendicular direction to the one supplied.
    /// Used to navigate around walls when the primary path is blocked.
    /// </summary>
    private Vector2 GetPerpendicularDirection(Vector2 primary)
    {
        // Local variables for axis-flip calculation
        Vector2 playerPos = GetPlayerPosition();
        float dx = playerPos.x - rb2D.position.x;
        float dy = playerPos.y - rb2D.position.y;

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
    /// Reduces this enemy's HP by the given amount and refreshes the HP label.
    /// Destroys the enemy and notifies the GameManager when HP reaches zero.
    /// Called by PlayerController.AttackEnemy().
    /// </summary>
    public void TakeDamage(int damage)
    {
        enemyHP -= damage;
        Debug.Log("Enemy took " + damage + " damage. HP remaining: " + enemyHP);

        UpdateHPLabel();

        if (enemyHP <= 0)
        {
            Debug.Log("Player Defeated Enemy – enemy destroyed.");
            GameManager.instance.EnemyDefeated();
            Destroy(gameObject);
        }
    }

    // Writes the current HP value to the world-space label on this prefab.
    // Does nothing if no label has been assigned in the Inspector.
    private void UpdateHPLabel()
    {
        if (hpLabel != null)
            hpLabel.text = enemyHP.ToString();
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
