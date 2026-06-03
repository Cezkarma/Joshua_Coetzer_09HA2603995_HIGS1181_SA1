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
    public static GameManager instance;

    [Header("Player Health")]
    public int playerMaxHP = 10;
    public int playerCurrentHP;

    [Header("Turn Management")]
    public bool isPlayerTurn = true;

    [Header("Scene References")]
    public UIManager uiManager;
    public PlayerController player;

    private int enemiesRemaining;

    private void Awake()
    {
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
        player.UpdateHPLabel(playerCurrentHP, playerMaxHP);

        Debug.Log("Game started. Enemies: " + enemiesRemaining + " | Player HP: " + playerCurrentHP);

        StartPlayerTurn();
    }

    public void StartPlayerTurn()
    {
        isPlayerTurn = true;
        player.EnableInput(true);
        uiManager.UpdateTurnIndicator(true);
        Debug.Log("Player Turn started.");
    }

    public void StartEnemyTurn()
    {
        isPlayerTurn = false;
        player.EnableInput(false);
        uiManager.UpdateTurnIndicator(false);
        Debug.Log("Enemy Turn started.");
        StartCoroutine(RunEnemyTurns());
    }

    private IEnumerator RunEnemyTurns()
    {
        EnemyAI[] enemies = FindObjectsOfType<EnemyAI>();

        foreach (EnemyAI enemy in enemies)
        {
            if (enemy == null) continue;
            enemy.TakeTurn();
            yield return new WaitForSeconds(0.15f);

            if (IsPlayerDead()) yield break;
        }

        if (!IsPlayerDead() && !AllEnemiesDefeated())
            StartPlayerTurn();
    }

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

    public bool AllEnemiesDefeated()
    {
        return enemiesRemaining <= 0;
    }

    public void PlayerTakeDamage(int damage)
    {
        playerCurrentHP -= damage;
        playerCurrentHP = Mathf.Max(playerCurrentHP, 0);
        uiManager.UpdateHP(playerCurrentHP, playerMaxHP);
        player.UpdateHPLabel(playerCurrentHP, playerMaxHP);
        Debug.Log("Enemy Attacked Player. Player HP: " + playerCurrentHP);

        if (IsPlayerDead())
        {
            Debug.Log("Game Over. Player HP reached zero.");
            uiManager.ShowLoseScreen();
        }
    }

    public bool IsPlayerDead()
    {
        return playerCurrentHP <= 0;
    }

    private void CountEnemies()
    {
        enemiesRemaining = FindObjectsOfType<EnemyAI>().Length;
    }

    private void RefreshUI()
    {
        uiManager.UpdateHP(playerCurrentHP, playerMaxHP);
        uiManager.UpdateEnemiesRemaining(enemiesRemaining);
        uiManager.UpdateTurnIndicator(isPlayerTurn);
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
