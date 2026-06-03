using TMPro;
using UnityEngine;

/// <summary>
/// Basic Enemy AI for a turn-based game.
/// Currently moves randomly. Students must modify this
/// script so the enemy moves toward the player.
/// 
/// MODIFICATIONS FROM ORIGINAL SCRIPT:
///  1. Added enemyHP and attackDamage fields.
///  2. Added blockingLayer and playerLayer references.
///  3. Replaced the random movement in TakeTurn() with player-
///     detection and directional chasing logic.
///  4. Added GetDirectionToPlayer() – returns the best Vector2
///     direction to step closer to the player (custom method).
///  5. Added IsAdjacentToPlayer() – returns true when the enemy
///     is one tile away and should attack instead of move.
///  6. Added GetPerpendicularDirection() to handle wall detours.
///  7. Added AttackPlayer() and TakeDamage() for combat.
///  8. Added OnTriggerEnter2D() for overlap detection.
///  9. Added Debug.Log() statements.
///  10. Added UpdateHPLabel() to make it more obvious to the player when enemy is taking damage.
/// 
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
public class EnemyAI : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("Time (in seconds) it takes to move one tile.")]
    public float moveTime = 0.2f;

    [Header("Combat Settings")]
    public int enemyHP     = 5;
    public int attackDamage = 2;

    [Header("Detection Layers")]
    public LayerMask blockingLayer;
    public LayerMask playerLayer;

    [Header("UI")]
    [Tooltip("Drag a child GameObject's TextMeshPro (non-UI) component here to show remaining HP.")]
    public TextMeshPro hpLabel;

    private Rigidbody2D rb2D;
    private BoxCollider2D col2D;

    private Transform playerTransform;
    private PlayerController playerController;

    private void Awake()
    {
        // Cache the Rigidbody2D and BoxCollider2D
        rb2D  = GetComponent<Rigidbody2D>();
        col2D = GetComponent<BoxCollider2D>();
    }

    private void Start()
    {
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
        {
            playerTransform  = playerObj.transform;
            playerController = playerObj.GetComponent<PlayerController>();
        }
        else
        {
            Debug.LogWarning("EnemyAI: No GameObject tagged 'Player' found in scene.");
        }

        UpdateHPLabel();
    }

    /// <summary>
    /// Called by GameManager each time it is the enemy's turn.
    /// WHAT CHANGED: replaces random movement with player-chasing logic.
    /// The enemy acts exactly once per call (one tile move OR attack).
    /// </summary>
    public void TakeTurn()
    {
        if (playerTransform == null) return;

        if (IsAdjacentToPlayer())
        {
            AttackPlayer();
            return;
        }

        Vector2 moveDir   = GetDirectionToPlayer();
        Vector2 targetPos = rb2D.position + moveDir;

        Collider2D wallHit = Physics2D.OverlapBox(
            targetPos, Vector2.one * 0.8f, 0f, blockingLayer);

        if (wallHit != null)
        {
            Vector2 altDir    = GetPerpendicularDirection(moveDir);
            Vector2 altTarget = rb2D.position + altDir;

            Collider2D altWallHit = Physics2D.OverlapBox(
                altTarget, Vector2.one * 0.8f, 0f, blockingLayer);

            if (altWallHit != null)
            {
                Debug.Log("Enemy movement blocked on both axes, si waiting.");
                return;
            }

            targetPos = altTarget;
        }

        Collider2D playerAtTarget = Physics2D.OverlapBox(
            targetPos, Vector2.one * 0.8f, 0f, playerLayer);

        if (playerAtTarget != null)
        {
            Debug.Log("Enemy target tile occupied by player – attacking instead of moving.");
            AttackPlayer();
            return;
        }

        rb2D.MovePosition(targetPos);
        Debug.Log("Enemy moved toward player to " + targetPos);
    }

    private Vector2 GetPlayerPosition()
    {
        if (playerController != null)
            return playerController.GridPosition;

        return playerTransform.position;
    }

    private Vector2 GetDirectionToPlayer()
    {
        Vector2 playerPos = GetPlayerPosition();
        float dx = playerPos.x - rb2D.position.x;
        float dy = playerPos.y - rb2D.position.y;

        if (Mathf.Abs(dx) >= Mathf.Abs(dy))
            return new Vector2(Mathf.Sign(dx), 0f);
        else
            return new Vector2(0f, Mathf.Sign(dy));
    }

    private bool IsAdjacentToPlayer()
    {
        float dist = Vector2.Distance(rb2D.position, GetPlayerPosition());
        return dist <= 1.1f;
    }

    private Vector2 GetPerpendicularDirection(Vector2 primary)
    {
        Vector2 playerPos = GetPlayerPosition();
        float dx = playerPos.x - rb2D.position.x;
        float dy = playerPos.y - rb2D.position.y;

        if (primary.x != 0)
            return new Vector2(0f, Mathf.Sign(dy));
        else
            return new Vector2(Mathf.Sign(dx), 0f);
    }

    private void AttackPlayer()
    {
        Debug.Log("Enemy Attacked Player for " + attackDamage + " damage.");
        GameManager.instance.PlayerTakeDamage(attackDamage);
    }

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

    private void UpdateHPLabel()
    {
        if (hpLabel != null)
            hpLabel.text = enemyHP.ToString();
    }


    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player entered enemy trigger zone.");
        }
    }
}
