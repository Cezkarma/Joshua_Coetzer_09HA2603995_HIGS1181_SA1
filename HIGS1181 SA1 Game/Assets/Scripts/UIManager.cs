using TMPro;
using UnityEngine;

/// <summary>
/// Manages all HUD and end-screen UI elements.
/// Updated by GameManager whenever game state changes.
/// Requires TextMeshPro (included with Unity 6 by default).
/// </summary>
public class UIManager : MonoBehaviour
{
    // ── Global UI References (assigned in Inspector) ─────────
    [Header("HUD Text Elements")]
    public TMP_Text hpText;
    public TMP_Text enemiesText;
    public TMP_Text turnText;

    [Header("End Screen Panels")]
    public GameObject winPanel;
    public GameObject losePanel;

    // ── Unity Lifecycle ──────────────────────────────────────

    private void Start()
    {
        // Hide end-screen panels at game start
        SetPanelActive(winPanel,  false);
        SetPanelActive(losePanel, false);
    }

    // ── Public Update Methods ────────────────────────────────

    /// <summary>Refreshes the player HP display (e.g. "HP: 7 / 10").</summary>
    public void UpdateHP(int current, int max)
    {
        if (hpText != null)
            hpText.text = "HP: " + current + " / " + max;
    }

    /// <summary>Refreshes the enemy count display (e.g. "Enemies: 2").</summary>
    public void UpdateEnemiesRemaining(int count)
    {
        if (enemiesText != null)
            enemiesText.text = "Enemies: " + count;
    }

    /// <summary>
    /// Updates the turn indicator label.
    /// Shows "Player Turn" or "Enemy Turn" so the player always knows
    /// whose action phase is active.
    /// </summary>
    public void UpdateTurnIndicator(bool isPlayerTurn)
    {
        if (turnText != null)
            turnText.text = isPlayerTurn ? "Player Turn" : "Enemy Turn";
    }

    /// <summary>Displays the win panel when all enemies are defeated.</summary>
    public void ShowWinScreen()
    {
        SetPanelActive(winPanel, true);
        Debug.Log("Win Screen displayed.");
    }

    /// <summary>Displays the lose panel when player HP reaches zero.</summary>
    public void ShowLoseScreen()
    {
        SetPanelActive(losePanel, true);
        Debug.Log("Game Over screen displayed.");
    }

    // ── Helper ───────────────────────────────────────────────

    // Null-safe panel toggle
    private void SetPanelActive(GameObject panel, bool active)
    {
        if (panel != null)
            panel.SetActive(active);
    }
}
