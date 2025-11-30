using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public void StartGame()
    {
        // Load the main game scene
        SceneManager.LoadScene("SampleScene");
    }

    public SettingsManager settingsManager;

    public void OpenSettings()
    {
        // Show settings panel
        if (settingsManager != null)
        {
            settingsManager.ShowSettings();
        }
        else
        {
            // Fallback if not assigned
            var sm = FindFirstObjectByType<SettingsManager>(FindObjectsInactive.Include);
            if (sm != null) sm.ShowSettings();
        }
    }

    public AchievementListUI achievementListUI;

    public void OpenAchievements()
    {
        if (achievementListUI != null)
        {
            achievementListUI.gameObject.SetActive(true);
            achievementListUI.RefreshList();
        }
        else
        {
            Debug.LogWarning("AchievementListUI not assigned in MainMenuManager");
        }
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
