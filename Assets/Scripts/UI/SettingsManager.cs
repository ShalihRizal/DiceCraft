using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class SettingsManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject settingsPanel;
    public TMP_Dropdown resolutionDropdown;
    public TMP_Dropdown qualityDropdown;
    public Toggle fullscreenToggle;
    public Slider volumeSlider;
    public TextMeshProUGUI volumeText;

    private Resolution[] resolutions;

    void OnEnable()
    {
        SetupResolutionDropdown();
        SetupQualityDropdown();
        LoadSettings();
    }

    void SetupResolutionDropdown()
    {
        if (resolutionDropdown == null) return;

        // Hardcoded resolutions as requested
        resolutions = new Resolution[2];
        
        resolutions[0] = new Resolution();
        resolutions[0].width = 2560;
        resolutions[0].height = 1440;
        
        resolutions[1] = new Resolution();
        resolutions[1].width = 1920;
        resolutions[1].height = 1080;

        resolutionDropdown.ClearOptions();

        List<string> options = new List<string>();
        int currentResolutionIndex = 0; // Default to 2560x1440

        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + " x " + resolutions[i].height;
            options.Add(option);

            // Check if this matches current screen resolution
            if (resolutions[i].width == Screen.width &&
                resolutions[i].height == Screen.height)
            {
                currentResolutionIndex = i;
            }
        }

        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();
    }

    void SetupQualityDropdown()
    {
        if (qualityDropdown == null) return;

        qualityDropdown.ClearOptions();
        qualityDropdown.AddOptions(new List<string>(QualitySettings.names));
        qualityDropdown.value = QualitySettings.GetQualityLevel();
        qualityDropdown.RefreshShownValue();
    }

    void LoadSettings()
    {
        // Load fullscreen
        if (fullscreenToggle != null)
        {
            fullscreenToggle.isOn = Screen.fullScreen;
        }

        // Load volume
        if (volumeSlider != null)
        {
            float volume = PlayerPrefs.GetFloat("MasterVolume", 1f);
            volumeSlider.value = volume;
            UpdateVolumeText(volume);
            AudioListener.volume = volume;
        }
    }

    public void ShowSettings()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(true);
    }

    public void HideSettings()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }

    public void SetResolution(int resolutionIndex)
    {
        if (resolutions == null || resolutionIndex >= resolutions.Length) return;

        Resolution resolution = resolutions[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
    }

    public void SetQuality(int qualityIndex)
    {
        QualitySettings.SetQualityLevel(qualityIndex);
        PlayerPrefs.SetInt("QualityLevel", qualityIndex);
    }

    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
        PlayerPrefs.SetInt("Fullscreen", isFullscreen ? 1 : 0);
    }

    public void SetVolume(float volume)
    {
        AudioListener.volume = volume;
        PlayerPrefs.SetFloat("MasterVolume", volume);
        UpdateVolumeText(volume);
    }

    void UpdateVolumeText(float volume)
    {
        if (volumeText != null)
        {
            volumeText.text = Mathf.RoundToInt(volume * 100) + "%";
        }
    }
}
