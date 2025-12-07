using UnityEngine;

public class LootPickup : MonoBehaviour
{
    public LootDrop lootDrop;
    public float collectRadius = 1.5f;
    public float autoCollectDelay = 0.5f;

    private float spawnTime;
    private bool collected = false;

    void Start()
    {
        spawnTime = Time.time;
        
        // Add visual based on loot type
        CreateVisual();
    }

    void CreateVisual()
    {
        // Only create sprite if one doesn't exist (e.g. from prefab)
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr == null)
        {
            sr = gameObject.AddComponent<SpriteRenderer>();
            
            // Set color based on loot type
            Color color = lootDrop.type switch
            {
                LootType.Gold => Color.yellow,
                LootType.DicePips => Color.cyan,
                LootType.Relic => Color.magenta,
                LootType.Dice => Color.white,
                LootType.HealthOrb => Color.green,
                _ => Color.white
            };
            
            sr.color = color;
            
            // Create a simple circle sprite
            Texture2D texture = new Texture2D(32, 32);
            for (int y = 0; y < 32; y++)
            {
                for (int x = 0; x < 32; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), new Vector2(16, 16));
                    texture.SetPixel(x, y, dist < 12 ? color : Color.clear);
                }
            }
            texture.Apply();
            
            sr.sprite = Sprite.Create(texture, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f));
        }
        
        // Ensure collider for mouse interaction
        if (gameObject.GetComponent<Collider2D>() == null)
        {
            CircleCollider2D col = gameObject.AddComponent<CircleCollider2D>();
            col.radius = 0.5f; 
            col.isTrigger = true; 
        }
    }

    void Update()
    {
        // Auto-collect after delay
        if (!collected && Time.time - spawnTime > autoCollectDelay)
        {
            // Check distance to player (assuming player is at a fixed position or find PlayerHealth)
            PlayerHealth player = FindFirstObjectByType<PlayerHealth>();
            if (player != null)
            {
                float dist = Vector3.Distance(transform.position, player.transform.position);
                if (dist < collectRadius)
                {
                    Collect();
                }
            }
        }
    }

    void OnDisable()
    {
        if (TooltipManager.Instance != null)
        {
            TooltipManager.Instance.HideTooltip();
        }
    }

    void OnMouseDown()
    {
        if (!collected)
        {
            Collect();
        }
    }

    void OnMouseEnter()
    {
        if (TooltipManager.Instance != null && !collected)
        {
            TooltipManager.Instance.ShowTooltip(lootDrop, transform.position);
        }
    }

    void OnMouseExit()
    {
        if (TooltipManager.Instance != null)
        {
            TooltipManager.Instance.HideTooltip();
        }
    }

    void Collect()
    {
        collected = true;

        switch (lootDrop.type)
        {
            case LootType.Gold:
                if (PlayerCurrency.Instance != null)
                {
                    PlayerCurrency.Instance.AddGold(lootDrop.amount);
                }
                break;

            case LootType.DicePips:
                // TODO: Implement Dice Pips currency
                Debug.Log($"Collected {lootDrop.amount} Dice Pips!");
                break;

            case LootType.Relic:
                // TODO: Grant random relic of specified rarity
                Debug.Log($"Collected Relic ({lootDrop.relicRarity})!");
                break;

            case LootType.Dice:
                // TODO: Grant random dice
                Debug.Log($"Collected Dice!");
                break;

            case LootType.HealthOrb:
                if (PlayerHealth.Instance != null)
                {
                    PlayerHealth.Instance.Heal(lootDrop.amount);
                }
                break;
        }

        Destroy(gameObject);
    }
}
