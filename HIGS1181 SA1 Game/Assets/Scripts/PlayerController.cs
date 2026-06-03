using UnityEngine;

/// <summary>
/// Controls the player character on a tile-based grid.
/// Reads W/A/S/D input and moves one tile per turn.
/// Detects walls and enemies before committing to movement.
/// Uses OnTriggerEnter2D to receive enemy attack notifications.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
public class PlayerController : MonoBehaviour
{
    // ── Global variables (Inspector-configurable) ────────────
    [Header("Combat")]
    public int attackDamage = 3;

    [Header("Layer Masks")]
    public LayerMask blockingLayer; // Assign the Wall layer here
    public LayerMask enemyLayer;    // Assign the Enemy layer here

    // ── Cached components (global to this class) ─────────────
    private Rigidbody2D rb2D;
    private BoxCollider2D col2D;

    // ── Local state ──────────────────────────────────────────
    private bool inputEnabled = false;

    /// <summary>
    /// The player's logical grid position.
    /// Updated immediately when the player moves – before Unity's physics
    /// system processes the MovePosition call – so other scripts (e.g.
    /// EnemyAI) can read the correct destination in the same frame.
    /// </summary>
    public Vector2 GridPosition { get; private set; }

    // ── Unity Lifecycle ──────────────────────────────────────

    private void Awake()
    {
        // GetComponent<> – cache references once on startup
        rb2D  = GetComponent<Rigidbody2D>();
        col2D = GetComponent<BoxCollider2D>();

        // Seed GridPosition from the starting transform position
        GridPosition = transform.position;
    }

    private void Update()
    {
        if (!inputEnabled) return;
        HandleInput();
    }

    // ── Input ────────────────────────────────────────────────

    // Converts WASD keys into a grid direction vector
    private void HandleInput()
    {
        // Local variables – only needed inside this method
        int h = 0, v = 0;

        if      (Input.GetKeyDown(KeyCode.W)) v =  1;
        else if (Input.GetKeyDown(KeyCode.S)) v = -1;
        else if (Input.GetKeyDown(KeyCode.A)) h = -1;
        else if (Input.GetKeyDown(KeyCode.D)) h =  1;

        if (h != 0 || v != 0)
            AttemptMove(new Vector2(h, v));
    }

    // ── Movement / Combat ────────────────────────────────────

    /// <summary>
    /// Tries to move one tile in the given direction.
    /// Returns true when an action (move or attack) was performed.
    /// Checks for walls and enemies before moving.
    /// </summary>
    private bool AttemptMove(Vector2 direction)
    {
        Vector2 targetPos = rb2D.position + direction;

        // Check for a wall at the target tile
        Collider2D wallHit = Physics2D.OverlapBox(
            targetPos, Vector2.one * 0.8f, 0f, blockingLayer);

        if (wallHit != null)
        {
            Debug.Log("Player movement blocked by wall.");
            return false; // Movement failed – tile is occupied by a wall
        }

        // Check for an enemy at the target tile
        Collider2D enemyHit = Physics2D.OverlapBox(
            targetPos, Vector2.one * 0.8f, 0f, enemyLayer);

        if (enemyHit != null)
        {
            // GetComponent<> – retrieve EnemyAI from the hit collider
            EnemyAI enemy = enemyHit.GetComponent<EnemyAI>();
            if (enemy != null)
            {
                AttackEnemy(enemy);
                EnableInput(false);
                GameManager.instance.StartEnemyTurn();
                return true;
            }
        }

        // Tile is clear – commit the logical position immediately so that
        // EnemyAI can read GridPosition correctly this same frame, then
        // hand the visual move off to the physics system.
        GridPosition = targetPos;
        rb2D.MovePosition(targetPos);
        Debug.Log("Player moved to " + targetPos);

        EnableInput(false);
        GameManager.instance.StartEnemyTurn();
        return true;
    }

    /// <summary>Deals attackDamage to the specified enemy.</summary>
    private void AttackEnemy(EnemyAI enemy)
    {
        Debug.Log("Player Defeated Enemy attempt – dealt " + attackDamage + " damage.");
        enemy.TakeDamage(attackDamage);
    }

    // ── Trigger Detection ────────────────────────────────────

    /// <summary>
    /// Fires when an enemy's trigger collider overlaps the player.
    /// Confirms the physical overlap that combat logic acts on.
    /// </summary>
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            Debug.Log("Enemy entered player trigger zone – attack registered.");
        }
    }

    // ── Public API (called by GameManager) ───────────────────

    /// <summary>Enables or disables player keyboard input for this turn.</summary>
    public void EnableInput(bool enable)
    {
        inputEnabled = enable;
    }
}
