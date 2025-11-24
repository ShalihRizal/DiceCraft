using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class WaveProgressUI : MonoBehaviour
{
    public Slider progressSlider;
    public TextMeshProUGUI waveText;
    public TextMeshProUGUI enemyCountText;

    private void Start()
    {
        GameEvents.OnWaveProgressChanged += UpdateProgress;
        GameEvents.OnWaveStarted += OnWaveStarted;
        
        if (progressSlider != null)
        {
            progressSlider.value = 0;
        }
        
        // Hide initially
        if (waveText != null) waveText.alpha = 0;
        if (enemyCountText != null) enemyCountText.alpha = 0;
    }

    private void OnDestroy()
    {
        GameEvents.OnWaveProgressChanged -= UpdateProgress;
        GameEvents.OnWaveStarted -= OnWaveStarted;
    }

    private void OnWaveStarted(int waveNumber, int totalEnemies)
    {
        // Update wave text with animation
        if (waveText != null)
        {
            waveText.text = $"Wave {waveNumber}";
            
            // Fade in and scale animation
            waveText.alpha = 0;
            waveText.transform.localScale = Vector3.one * 0.5f;
            
            waveText.DOFade(1f, 0.5f).SetEase(Ease.OutQuad);
            waveText.transform.DOScale(1f, 0.5f).SetEase(Ease.OutBack);
        }
        
        // Show enemy count
        if (enemyCountText != null)
        {
            enemyCountText.alpha = 0;
            enemyCountText.DOFade(1f, 0.5f).SetDelay(0.2f);
        }
        
        // Reset progress
        UpdateProgress(0, totalEnemies);
    }

    private void UpdateProgress(int enemiesKilled, int totalEnemies)
    {
        // Update slider with smooth animation
        if (progressSlider != null)
        {
            float targetValue = totalEnemies > 0 ? (float)enemiesKilled / totalEnemies : 0;
            progressSlider.DOValue(targetValue, 0.3f).SetEase(Ease.OutQuad);
        }

        // Update count text
        if (enemyCountText != null)
        {
            enemyCountText.text = $"{enemiesKilled}/{totalEnemies}";
            
            // Pulse animation when enemy killed
            if (enemiesKilled > 0)
            {
                enemyCountText.transform.DOPunchScale(Vector3.one * 0.2f, 0.3f, 5, 0.5f);
            }
        }
        
        // Celebrate when wave complete
        if (enemiesKilled >= totalEnemies && totalEnemies > 0)
        {
            CelebrateWaveComplete();
        }
    }

    private void CelebrateWaveComplete()
    {
        // Flash the progress bar
        if (progressSlider != null && progressSlider.fillRect != null)
        {
            Image fillImage = progressSlider.fillRect.GetComponent<Image>();
            if (fillImage != null)
            {
                Color originalColor = fillImage.color;
                fillImage.DOColor(Color.yellow, 0.2f).SetLoops(3, LoopType.Yoyo).OnComplete(() =>
                {
                    fillImage.color = originalColor;
                });
            }
        }
        
        // Bounce the wave text
        if (waveText != null)
        {
            waveText.transform.DOPunchScale(Vector3.one * 0.3f, 0.5f, 5, 0.5f);
        }
    }
}
