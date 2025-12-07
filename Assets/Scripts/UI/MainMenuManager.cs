using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public void StartGame()
    {
        // Load the main game scene (assuming it's called "Game" or index 1)
        SceneManager.LoadScene("Game");
    }

    public void OpenSettings()
    {
        // Show settings panel
        SettingsManager settingsManager = FindFirstObjectByType<SettingsManager>();
        if (settingsManager != null)
        {
            settingsManager.ShowSettings();
        }
    }

    public void OpenAchievements()
    {
        // Placeholder for achievements
        Debug.Log("Achievements panel opened (not yet implemented)");
    }

    public void QuitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}
