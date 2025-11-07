using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public GameObject gameOverPanel;

    void OnEnable()
    {
        GameEvents.OnPlayerDied += ShowGameOver;
    }

    void OnDisable()
    {
        GameEvents.OnPlayerDied -= ShowGameOver;
    }

    void ShowGameOver()
    {
        gameOverPanel.SetActive(true);
    }

    public void RestartGame()
    {
        GameManager.Instance?.Restart();
        gameOverPanel.SetActive(false);
    }
}
