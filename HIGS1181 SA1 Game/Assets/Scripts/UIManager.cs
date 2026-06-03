using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [Header("HUD Text Elements")]
    public TMP_Text hpText;
    public TMP_Text enemiesText;
    public TMP_Text turnText;

    [Header("End Screen Panels")]
    public GameObject winPanel;
    public GameObject losePanel;

    private void Start()
    {
        SetPanelActive(winPanel,  false);
        SetPanelActive(losePanel, false);
    }

    public void UpdateHP(int current, int max)
    {
        if (hpText != null)
            hpText.text = "HP: " + current + " / " + max;
    }

    public void UpdateEnemiesRemaining(int count)
    {
        if (enemiesText != null)
            enemiesText.text = "Enemies: " + count;
    }

    public void UpdateTurnIndicator(bool isPlayerTurn)
    {
        if (turnText != null)
            turnText.text = isPlayerTurn ? "Player Turn" : "Enemy Turn";
    }

    public void ShowWinScreen()
    {
        SetPanelActive(winPanel, true);
        Debug.Log("Win Screen displayed.");
    }

    public void ShowLoseScreen()
    {
        SetPanelActive(losePanel, true);
        Debug.Log("Game Over screen displayed.");
    }

    private void SetPanelActive(GameObject panel, bool active)
    {
        if (panel != null)
            panel.SetActive(active);
    }
}
