using UnityEngine;
using TMPro;
using DG.Tweening;

public class FloatingText : MonoBehaviour
{
    public TextMeshProUGUI text;
    public float floatDistance = .1f;
    public float duration = .5f;

    private CanvasGroup canvasGroup;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public void Show(string content, Color color, int fontSize = 30)
    {
        if (text != null)
        {
            text.text = content;
            text.color = color;
            text.fontSize = fontSize;

            AnimateText(color);
        }
    }

    public void ShowGold(int goldAmount)
    {
        if (text != null)
        {
            text.text = $"+{goldAmount}g";
            text.color = new Color(1f, 0.84f, 0f);
            text.fontSize = 30;

            AnimateText(text.color);
        }
    }

    private void AnimateText(Color color)
    {
        if (canvasGroup == null || text == null) return;

        // Kill existing tweens to prevent ghost references
        transform.DOKill();
        text.transform.DOKill();

        // Move upward
        transform.DOMoveY(transform.position.y + floatDistance, duration).SetEase(Ease.OutQuad);

        // Fade out
        canvasGroup.DOFade(0f, duration).SetEase(Ease.InQuad);

        // Punch if crit/heal-crit
        if (color == Color.red || color == Color.yellow)
            text.transform.DOPunchScale(Vector3.one * 0.2f, 0.3f, 5, 0.5f);

        // Delay destruction to after tweens
        Destroy(gameObject, duration + 0.05f);
    }
}
