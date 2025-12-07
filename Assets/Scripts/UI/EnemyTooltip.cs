using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class EnemyTooltip : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI typeText;
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI damageText;
    public TextMeshProUGUI attackSpeedText;
    
    [Header("Traits")]
    public GameObject traitContainer;
    public GameObject traitEntryPrefab;
    
    public RectTransform rectTransform;

    public void SetInfo(Enemy enemy)
    {
        if (enemy == null || enemy.enemyData == null) return;

        EnemyData data = enemy.enemyData;

        // Basic Info
        if (nameText != null)
        {
            nameText.text = data.enemyName;
            nameText.color = GetTypeColor(data.enemyType);
        }

        if (typeText != null)
        {
            typeText.text = $"[{data.enemyType}]";
            typeText.color = GetTypeColor(data.enemyType);
        }

        // Stats
        if (healthText != null)
        {
            healthText.text = $"HP: {Mathf.CeilToInt(enemy.health)} / {Mathf.CeilToInt(data.maxHealth)}";
        }

        if (damageText != null)
        {
            damageText.text = $"Damage: {data.damage}";
        }

        if (attackSpeedText != null)
        {
            attackSpeedText.text = $"Attack Speed: {data.attackInterval}s";
        }

        // Traits
        DisplayTraits(data.traits);
    }

    void DisplayTraits(List<EnemyTrait> traits)
    {
        if (traitContainer == null) return;

        // Clear existing traits
        foreach (Transform child in traitContainer.transform)
        {
            Destroy(child.gameObject);
        }

        // Add trait entries
        foreach (var trait in traits)
        {
            if (trait == null) continue;

            GameObject entry = CreateTraitEntry(trait);
            if (entry != null)
            {
                entry.transform.SetParent(traitContainer.transform, false);
            }
        }
    }

    GameObject CreateTraitEntry(EnemyTrait trait)
    {
        if (traitEntryPrefab != null)
        {
            GameObject entry = Instantiate(traitEntryPrefab);
            
            // Assuming the prefab has TextMeshProUGUI components
            TextMeshProUGUI[] texts = entry.GetComponentsInChildren<TextMeshProUGUI>();
            if (texts.Length >= 2)
            {
                texts[0].text = trait.traitName;
                texts[1].text = trait.description;
            }
            
            return entry;
        }
        else
        {
            // Fallback: Create simple text entry
            GameObject entry = new GameObject("TraitEntry");
            TextMeshProUGUI text = entry.AddComponent<TextMeshProUGUI>();
            text.text = $"• {trait.traitName}: {trait.description}";
            text.fontSize = 14;
            text.color = Color.yellow;
            
            RectTransform rt = entry.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(300, 40);
            
            return entry;
        }
    }

    Color GetTypeColor(EnemyType type)
    {
        return type switch
        {
            EnemyType.Normal => Color.white,
            EnemyType.Elite => new Color(1f, 0.84f, 0f), // Gold
            EnemyType.Boss => new Color(1f, 0.2f, 0.2f), // Red
            _ => Color.white
        };
    }
}
