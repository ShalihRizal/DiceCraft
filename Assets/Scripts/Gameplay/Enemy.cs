using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Runtime Data")]
    public EnemyData enemyData;
    
    public float health;

    public float MaxHealth { get; private set; } // Renamed to property
    private EnemyHealthUI healthUI;

    public GameObject projectilePrefab;
    public GameObject healthBarPrefab; // Prefab for health bar
    public float fireInterval = 1f;
    public float projectileDamage = 1f;

    private bool isDead = false;
    public bool IsDead => isDead;

    private float timer;
    
    // Trait trigger flags
    private bool triggered50Percent = false;
    private bool triggered25Percent = false;
    private bool triggered1Percent = false;
    
    // Immunity Logic
    private List<Enemy> linkedMinions = new List<Enemy>();
    public bool immuneWhileMinionsAlive = false;

    public void RegisterLinkedMinion(Enemy minion)
    {
        if (minion != null)
        {
            linkedMinions.Add(minion);
            immuneWhileMinionsAlive = true;
            Debug.Log($"{name} is now protected by {minion.name}!");
        }
    }

    void Start()
    {
        InitializeFromData();
        
        Debug.Log($"[Enemy] Before scaling: HP={health}, Damage={projectileDamage}");
        
        // Apply difficulty scaling FIRST
        if (DifficultyManager.Instance != null)
        {
            DifficultyManager.Instance.ScaleEnemy(this);
            Debug.Log($"[Enemy] After scaling: HP={health}, Damage={projectileDamage}");
        }
        else
        {
            Debug.LogWarning("[Enemy] DifficultyManager.Instance is NULL!");
        }
        
        // THEN set maxHealth (after scaling)
        MaxHealth = health;
        
        // Initialize health UI
        if (enemyData.enemyType == EnemyType.Boss)
        {
            // Find Boss UI in scene
            BossHealthBarUI bossUI = FindFirstObjectByType<BossHealthBarUI>(FindObjectsInactive.Include); // Unity 2023+
            if (bossUI == null) bossUI = FindObjectOfType<BossHealthBarUI>(true); // Fallback to includeInactive=true
            
            if (bossUI != null)
            {
                bossUI.Setup(this);
                // Don't add local health UI
            }
            else
            {
                Debug.LogWarning("⚠️ BossHealthBarUI not found in scene!");
                // Fallback to normal UI if prefab exists, otherwise AddComponent (legacy)
                InitializeHealthUI();
            }
        }
        else
        {
            // Standard enemy UI
            InitializeHealthUI();
        }

        if (ObjectPooler.Instance != null && projectilePrefab != null)
        {
            string poolTag = projectilePrefab.name;
            
            if (!ObjectPooler.Instance.poolDictionary.ContainsKey(poolTag))
            {
                ObjectPooler.Instance.CreatePool(poolTag, projectilePrefab, 10);
            }
        }

        // Trigger OnSpawned traits
        TriggerTraits(EnemyEventTrigger.OnSpawned);
        
        // Ensure collider for mouse events
        if (GetComponent<Collider2D>() == null)
        {
            CircleCollider2D col = gameObject.AddComponent<CircleCollider2D>();
            col.radius = 0.5f;
        }
    }

    void InitializeHealthUI()
    {
        if (healthBarPrefab != null)
        {
            GameObject hpObj = Instantiate(healthBarPrefab, transform);
            // Default to zero, then adjust if component exists
            hpObj.transform.localPosition = Vector3.zero; 
            
            healthUI = hpObj.GetComponent<EnemyHealthUI>();
            if (healthUI == null) healthUI = hpObj.GetComponentInChildren<EnemyHealthUI>();
            
            if (healthUI != null)
            {
                // Apply the offset defined in the UI script
                hpObj.transform.localPosition = healthUI.offset;
                healthUI.Setup(MaxHealth);
            }
            else
            {
                 Debug.LogWarning("EnemyHealthUI component missing on healthBarPrefab!");
            }
        }
        else
        {
             // Legacy fallback if no prefab assigned
             healthUI = gameObject.AddComponent<EnemyHealthUI>();
             healthUI.Setup(MaxHealth);
        }
    }

    void OnMouseEnter()
    {
        if (TooltipManager.Instance != null)
        {
            TooltipManager.Instance.ShowTooltip(this, transform.position);
        }
    }

    void OnMouseExit()
    {
        if (TooltipManager.Instance != null)
        {
            TooltipManager.Instance.HideTooltip();
        }
    }

    void InitializeFromData()
    {
        if (enemyData != null)
        {
            health = enemyData.maxHealth;
            projectileDamage = enemyData.damage;
            fireInterval = enemyData.attackInterval;
            
            // Apply all traits
            foreach (var trait in enemyData.traits)
            {
                if (trait != null)
                {
                    trait.OnApplied(this);
                }
            }
        }
    }

    public void Heal(float amount)
    {
        health = Mathf.Min(health + amount, MaxHealth);
        
        if (healthUI != null)
        {
            healthUI.UpdateHealth(health);
        }

        // Update Boss UI
        if (enemyData.enemyType == EnemyType.Boss)
        {
             BossHealthBarUI bossUI = FindFirstObjectByType<BossHealthBarUI>(FindObjectsInactive.Include);
             if (bossUI == null) bossUI = FindObjectOfType<BossHealthBarUI>(true);
             
             if (bossUI != null)
             {
                 bossUI.UpdateHealth(health);
             }
        }
    }

    public void TakeDamage(float amount)
    {
        // Immunity Check
        if (immuneWhileMinionsAlive)
        {
            // Clean up dead minions from list
            linkedMinions.RemoveAll(m => m == null || m.IsDead);
            
            if (linkedMinions.Count > 0)
            {
                // Is Immune!
                if (healthUI != null) healthUI.ShowImmune();
                
                // Update Boss UI if applicable
                if (enemyData != null && enemyData.enemyType == EnemyType.Boss)
                {
                    BossHealthBarUI bossUI = FindFirstObjectByType<BossHealthBarUI>();
                    if (bossUI == null) bossUI = FindObjectOfType<BossHealthBarUI>();
                    if (bossUI != null) bossUI.ShowImmune();
                }
                
                return; // Block damage
            }
            else
            {
                // All minions dead, immunity lost
                immuneWhileMinionsAlive = false;
                // Update UI back to numbers? handled by next UpdateHealth call or we can force it:
                if (healthUI != null) healthUI.UpdateHealth(health);
            }
        }

        health -= amount;

        if (healthUI != null) 
        {
            healthUI.UpdateHealth(health);
        }
        
        // Also update Boss UI if this is a boss
        if (enemyData != null && enemyData.enemyType == EnemyType.Boss)
        {
             BossHealthBarUI bossUI = FindFirstObjectByType<BossHealthBarUI>();
             if (bossUI == null) bossUI = FindObjectOfType<BossHealthBarUI>();
             
             if (bossUI != null)
             {
                 bossUI.UpdateHealth(health);
             }
        }

        // Trigger OnDamaged traits
        TriggerTraits(EnemyEventTrigger.OnDamaged);

        // Check HP thresholds
        float healthPercent = (health / MaxHealth) * 100f;
        
        if (!triggered50Percent && healthPercent <= 50f)
        {
            triggered50Percent = true;
            TriggerTraits(EnemyEventTrigger.On50Percent);
        }
        
        if (!triggered25Percent && healthPercent <= 25f)
        {
            triggered25Percent = true;
            TriggerTraits(EnemyEventTrigger.On25Percent);
        }
        
        if (!triggered1Percent && healthPercent <= 1f)
        {
            triggered1Percent = true;
            TriggerTraits(EnemyEventTrigger.On1Percent);
        }

        if (health <= 0f)
        {
            Die();
        }
    }

    public void AddShield(float amount)
    {
        // Shield logic here (future implementation)
        TriggerTraits(EnemyEventTrigger.OnShielded);
    }

    public void AddBuff()
    {
        // Buff logic here (future implementation)
        TriggerTraits(EnemyEventTrigger.OnBuffed);
    }

    public int coinDropAmount = 10;

    public void Die()
    {
        isDead = true;
        
        // Trigger OnDeath traits BEFORE cleanup
        TriggerTraits(EnemyEventTrigger.OnDeath);

        if (enemyData != null && enemyData.enemyType == EnemyType.Boss)
        {
             BossHealthBarUI bossUI = FindFirstObjectByType<BossHealthBarUI>(FindObjectsInactive.Include);
             if (bossUI == null) bossUI = FindObjectOfType<BossHealthBarUI>(true);
             
             if (bossUI != null) bossUI.Hide();
        }
        
        GameEvents.RaiseEnemyKilled();
        
        // Drop loot
        DropLoot();
        
        // Drop legacy coins if no loot table
        if (enemyData == null || enemyData.lootTable == null)
        {
            if (PlayerCurrency.Instance != null)
            {
                PlayerCurrency.Instance.AddGold(coinDropAmount);
            }
        }
        
        // Notify spawner for wave progress
        EnemySpawner spawner = FindFirstObjectByType<EnemySpawner>();
        if (spawner != null)
        {
            spawner.OnEnemyKilled();
        }
        
        EnemySpawner.UnregisterEnemy(this);
        Destroy(gameObject);
    }

    void DropLoot()
    {
        if (enemyData == null || enemyData.lootTable == null) return;

        List<LootDrop> drops = enemyData.lootTable.RollLoot();
        
        foreach (var drop in drops)
        {
            SpawnLootPickup(drop);
        }
    }

    void SpawnLootPickup(LootDrop drop)
    {
        // Spawn loot at enemy position with slight randomization
        Vector3 spawnPos = transform.position + new Vector3(
            Random.Range(-0.5f, 0.5f),
            Random.Range(-0.5f, 0.5f),
            0f
        );

        GameObject lootObj;
        LootPickup pickup;

        if (drop.prefab != null)
        {
            // Use specific prefab
            lootObj = Instantiate(drop.prefab, spawnPos, Quaternion.identity);
            pickup = lootObj.GetComponent<LootPickup>();
            if (pickup == null) pickup = lootObj.AddComponent<LootPickup>();
        }
        else
        {
            // Create default placeholder
            lootObj = new GameObject($"Loot_{drop.type}");
            lootObj.transform.position = spawnPos;
            pickup = lootObj.AddComponent<LootPickup>();
        }
        
        pickup.lootDrop = drop;
    }

    void TriggerTraits(EnemyEventTrigger trigger)
    {
        if (enemyData == null) return;

        var traits = enemyData.GetTraitsForEvent(trigger);
        foreach (var trait in traits)
        {
            trait.ExecuteEffect(this);
        }
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= fireInterval)
        {
            ShootAtPlayer();
            timer = 0f;
        }
    }

    void ShootAtPlayer()
    {
        if (isDead || projectilePrefab == null) return;

        if (ObjectPooler.Instance == null) return;

        // Trigger OnAttack traits
        TriggerTraits(EnemyEventTrigger.OnAttack);

        Vector3 spawnPos = transform.position + Vector3.down * 1.0f; 
        
        string poolTag = projectilePrefab.name;
        GameObject projectile = ObjectPooler.Instance.SpawnFromPool(poolTag, spawnPos, Quaternion.identity);

        if (projectile != null)
        {
            Projectile proj = projectile.GetComponent<Projectile>();
            if (proj == null)
            {
                proj = projectile.AddComponent<Projectile>();
            }

            if (proj != null)
            {
                proj.owner = ProjectileOwner.Enemy;
                proj.damage = projectileDamage;
                proj.direction = Vector3.down;
                proj.validToDamage = true;
            }
        }
    }
}
