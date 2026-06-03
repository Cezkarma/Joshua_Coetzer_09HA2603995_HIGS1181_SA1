using TMPro;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Combat")]
    public int attackDamage = 3;

    [Header("Layer Masks")]
    public LayerMask blockingLayer;
    public LayerMask enemyLayer;

    [Header("UI")]
    [Tooltip("Drag a child GameObject's TextMeshPro (non-UI) component here to show remaining HP.")]
    public TextMeshPro hpLabel;

    private Rigidbody2D rb2D;
    private BoxCollider2D col2D;

    private bool inputEnabled = false;

    public Vector2 GridPosition { get; private set; }

    private void Awake()
    {
        rb2D  = GetComponent<Rigidbody2D>();
        col2D = GetComponent<BoxCollider2D>();

        GridPosition = transform.position;
    }

    private void Update()
    {
        if (!inputEnabled) return;
        HandleInput();
    }

    private void HandleInput()
    {
        int h = 0, v = 0;

        if      (Input.GetKeyDown(KeyCode.W)) v =  1;
        else if (Input.GetKeyDown(KeyCode.S)) v = -1;
        else if (Input.GetKeyDown(KeyCode.A)) h = -1;
        else if (Input.GetKeyDown(KeyCode.D)) h =  1;

        if (h != 0 || v != 0)
            AttemptMove(new Vector2(h, v));
    }

    private bool AttemptMove(Vector2 direction)
    {
        Vector2 targetPos = rb2D.position + direction;

        Collider2D wallHit = Physics2D.OverlapBox(
            targetPos, Vector2.one * 0.8f, 0f, blockingLayer);

        if (wallHit != null)
        {
            Debug.Log("Player movement blocked by wall.");
            return false;
        }

        Collider2D enemyHit = Physics2D.OverlapBox(
            targetPos, Vector2.one * 0.8f, 0f, enemyLayer);

        if (enemyHit != null)
        {
            EnemyAI enemy = enemyHit.GetComponent<EnemyAI>();
            if (enemy != null)
            {
                AttackEnemy(enemy);
                EnableInput(false);
                GameManager.instance.StartEnemyTurn();
                return true;
            }
        }

        GridPosition = targetPos;
        rb2D.MovePosition(targetPos);
        Debug.Log("Player moved to " + targetPos);

        EnableInput(false);
        GameManager.instance.StartEnemyTurn();
        return true;
    }

    private void AttackEnemy(EnemyAI enemy)
    {
        Debug.Log("Player Defeated Enemy attempt – dealt " + attackDamage + " damage.");
        enemy.TakeDamage(attackDamage);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            Debug.Log("Enemy entered player trigger zone – attack registered.");
        }
    }

    public void EnableInput(bool enable)
    {
        inputEnabled = enable;
    }

    public void UpdateHPLabel(int current, int max)
    {
        if (hpLabel != null)
            hpLabel.text = current.ToString();
    }
}
