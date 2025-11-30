using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    public GameObject gameOverPanel;

    void Start()
    {
        // Hide game over panel by default
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        // Subscribe to player death event
        GameEvents.OnPlayerDied += ShowGameOver;
    }

    void OnDestroy()
    {
        GameEvents.OnPlayerDied -= ShowGameOver;
    }

    void ShowGameOver()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            
            // Pause the game
            Time.timeScale = 0f;
        }
    }

    public void ReturnToMainMenu()
    {
        // Unpause the game before loading
        Time.timeScale = 1f;
        
        // Load main menu scene
        SceneManager.LoadScene("MainMenuScene");
    }

    public void RestartGame()
    {
        // Unpause the game before loading
        Time.timeScale = 1f;
        
        // Reload current scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
