using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthUI : MonoBehaviour
{
    private GameObject canvasGO;
    private Image fillImage;
    private float maxHealth;

    public void Setup(float maxHealth)
    {
        this.maxHealth = maxHealth;

        // Create a simple white sprite
        Texture2D texture = new Texture2D(2, 2);
        Color[] colors = new Color[4];
        for (int i = 0; i < colors.Length; i++) colors[i] = Color.white;
        texture.SetPixels(colors);
        texture.Apply();
        Sprite whiteSprite = Sprite.Create(texture, new Rect(0, 0, 2, 2), Vector2.zero);

        // Create World Space Canvas
        canvasGO = new GameObject("HealthCanvas");
        canvasGO.transform.SetParent(transform);
        canvasGO.transform.localPosition = new Vector3(0, 0.8f, 0); // Above head
        
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = Camera.main;
        
        // Scale down canvas for world space
        RectTransform canvasRect = canvasGO.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(1, 0.2f);
        canvasGO.transform.localScale = new Vector3(0.01f, 0.01f, 1f); // Tiny scale for world space

        // Create Background
        GameObject bgGO = new GameObject("Background");
        bgGO.transform.SetParent(canvasGO.transform);
        Image bgImage = bgGO.AddComponent<Image>();
        bgImage.sprite = whiteSprite; // Assign sprite
        bgImage.color = Color.black;
        RectTransform bgRect = bgGO.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;
        bgRect.anchoredPosition = Vector2.zero;

        // Create Fill
        GameObject fillGO = new GameObject("Fill");
        fillGO.transform.SetParent(bgGO.transform);
        fillImage = fillGO.AddComponent<Image>();
        fillImage.sprite = whiteSprite; // Assign sprite
        fillImage.color = Color.red;
        fillImage.type = Image.Type.Filled;
        fillImage.fillMethod = Image.FillMethod.Horizontal;
        
        RectTransform fillRect = fillGO.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.sizeDelta = new Vector2(-0.1f, -0.1f); // Padding
        fillRect.anchoredPosition = Vector2.zero;
    }

    public void UpdateHealth(float currentHealth)
    {
        if (fillImage != null)
        {
            float fill = currentHealth / maxHealth;
            fillImage.fillAmount = fill;
            Debug.Log($"Health Update: {currentHealth}/{maxHealth} = {fill}");
        }
        else
        {
            Debug.LogWarning("EnemyHealthUI: Fill Image is null!");
        }
    }
}
