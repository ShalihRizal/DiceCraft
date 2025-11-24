using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class GameOverUI : MonoBehaviour
{
    public GameObject gameOverPanel;
    public Button restartButton;
    public TextMeshProUGUI gameOverText;

    void Start()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        if (restartButton != null)
            restartButton.onClick.AddListener(OnRestartClicked);

        GameEvents.OnGameOver += ShowGameOver;
    }

    void OnDestroy()
    {
        GameEvents.OnGameOver -= ShowGameOver;
    }

    void ShowGameOver()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);
    }

    void OnRestartClicked()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.Restart();
        }
    }
}
