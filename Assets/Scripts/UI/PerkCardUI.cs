using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PerkCardUI : MonoBehaviour
{
    public Image iconImage;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descText;
    public Button selectButton;

    private PerkData perkData;

    public void Setup(PerkData data)
    {
        perkData = data;
        if (perkData == null) return;

        if (nameText != null) nameText.text = perkData.perkName;
        if (descText != null) descText.text = perkData.description;
        if (iconImage != null)
        {
            iconImage.sprite = perkData.icon;
            iconImage.enabled = perkData.icon != null;
        }

        if (selectButton != null)
        {
            selectButton.onClick.RemoveAllListeners();
            selectButton.onClick.AddListener(OnSelect);
        }
    }

    void OnSelect()
    {
        if (RewardManager.Instance != null)
        {
            RewardManager.Instance.SelectReward(perkData);
        }
    }
}
