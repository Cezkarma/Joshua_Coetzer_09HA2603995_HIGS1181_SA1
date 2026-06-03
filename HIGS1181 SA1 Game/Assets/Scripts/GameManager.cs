using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages the overall game state: turn order, player HP,
/// enemy count, and win/lose conditions.
/// Singleton – one instance accessible from any script.
/// </summary>
public class GameManager : MonoBehaviour
{
    // ── Singleton ────────────────────────────────────────────
    public static GameManager instance;

    // ── Global variables (tracked across the whole session) ──
    [Header("Player Health")]
    public int playerMaxHP = 10;
    public int playerCurrentHP;

    [Header("Turn Management")]
    public bool isPlayerTurn = true;

    [Header("Scene References")]
    public UIManager uiManager;
    public PlayerController player;

    // Global enemy counter – decremented each time an enemy dies
    private int enemiesRemaining;

    // ── Unity Lifecycle ──────────────────────────────────────

    private void Awake()
    {
        // Enforce singleton: destroy any duplicate
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        playerCurrentHP = playerMaxHP;
        CountEnemies();
        RefreshUI();

        Debug.Log("Game started. Enemies: " + enemiesRemaining +
                  " | Player HP: " + playerCurrentHP);

        StartPlayerTurn();
    }

    // ── Turn Management ──────────────────────────────────────

    /// <summary>Activates the player's input for this turn.</summary>
    public void StartPlayerTurn()
    {
        isPlayerTurn = true;
        player.EnableInput(true);
        uiManager.UpdateTurnIndicator(true);
        Debug.Log("Player Turn started.");
    }

    /// <summary>
    /// Ends the player's turn and lets every living enemy act.
    /// Called by PlayerController after a successful move or attack.
    /// </summary>
    public void StartEnemyTurn()
    {
        isPlayerTurn = false;
        player.EnableInput(false);
        uiManager.UpdateTurnIndicator(false);
        Debug.Log("Enemy Turn started.");
        StartCoroutine(RunEnemyTurns());
    }

    // Coroutine: iterate enemies with a small delay for visual clarity
    private IEnumerator RunEnemyTurns()
    {
        EnemyAI[] enemies = FindObjectsOfType<EnemyAI>();

        foreach (EnemyAI enemy in enemies)
        {
            if (enemy == null) continue;
            enemy.TakeTurn();
            yield return new WaitForSeconds(0.15f);

            // Stop immediately if the player died mid-turn
            if (IsPlayerDead()) yield break;
        }

        // Only resume player turn if the game is still running
        if (!IsPlayerDead() && !AllEnemiesDefeated())
            StartPlayerTurn();
    }

    // ── Enemy / Win Condition ────────────────────────────────

    /// <summary>
    /// Called by EnemyAI when an enemy is destroyed.
    /// Decrements the counter and checks the win condition.
    /// </summary>
    public void EnemyDefeated()
    {
        enemiesRemaining--;
        uiManager.UpdateEnemiesRemaining(enemiesRemaining);
        Debug.Log("Player Defeated Enemy. Enemies remaining: " + enemiesRemaining);

        if (AllEnemiesDefeated())
        {
            Debug.Log("All enemies defeated! Player wins!");
            uiManager.ShowWinScreen();
        }
    }

    /// <summary>Returns true when no enemies are left (win condition).</summary>
    public bool AllEnemiesDefeated()
    {
        return enemiesRemaining <= 0;
    }

    // ── Player / Lose Condition ──────────────────────────────

    /// <summary>
    /// Reduces player HP and checks the lose condition.
    /// Called by EnemyAI.AttackPlayer().
    /// </summary>
    public void PlayerTakeDamage(int damage)
    {
        playerCurrentHP -= damage;
        playerCurrentHP = Mathf.Max(playerCurrentHP, 0); // clamp at zero
        uiManager.UpdateHP(playerCurrentHP, playerMaxHP);
        Debug.Log("Enemy Attacked Player. Player HP: " + playerCurrentHP);

        if (IsPlayerDead())
        {
            Debug.Log("Game Over. Player HP reached zero.");
            uiManager.ShowLoseScreen();
        }
    }

    /// <summary>Returns true when the player HP is zero or less (lose condition).</summary>
    public bool IsPlayerDead()
    {
        return playerCurrentHP <= 0;
    }

    // ── Helpers ──────────────────────────────────────────────

    // Scans the scene and initialises the enemy counter
    private void CountEnemies()
    {
        enemiesRemaining = FindObjectsOfType<EnemyAI>().Length;
    }

    // Pushes current state to all UI elements at once
    private void RefreshUI()
    {
        uiManager.UpdateHP(playerCurrentHP, playerMaxHP);
        uiManager.UpdateEnemiesRemaining(enemiesRemaining);
        uiManager.UpdateTurnIndicator(isPlayerTurn);
    }

    /// <summary>Reloads the active scene (wired to Restart buttons).</summary>
    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
