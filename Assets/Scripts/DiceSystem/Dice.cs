using System.Collections;
using DG.Tweening;
using UnityEngine;
using System.Collections.Generic;


public enum VFXType { Idle, Spawn, Destroy, Sold, Bought, Drag, Drop, Merge, Passive }

public class Dice : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject projectilePrefab;
    public GameObject floatingTextPrefab_Normal;
    public GameObject floatingTextPrefab_Crit;
    public GameObject floatingTextPrefab_Multicast;
    public GameObject tooltipPrefab;

    public GameObject vfxPassive;
    public GameObject vfxHighlight; // 💡 Visual for adjacency feedback

    [Header("Data")]
    [SerializeField] private DiceData _diceData;
    public DiceData diceData
    {
        get => _diceData;
        set
        {
            _diceData = value;
            if (_diceData != null)
            {
                // ✅ Ensure runtimeStats is always initialized when diceData is assigned
                runtimeStats = new RuntimeDiceData(_diceData);
            }
        }
    }

    public RuntimeDiceData runtimeStats;

    private DiceTooltip tooltipInstance;
    private Coroutine fireRoutine;
    private SpriteRenderer spriteRenderer;
    private Vector3 originalScale;
    private Tween highlightTween;

    void Awake()
    {
        // ✅ Initialize runtimeStats if diceData already exists
        if (diceData != null && runtimeStats == null)
            runtimeStats = new RuntimeDiceData(diceData);

        // ✅ Ensure Collider for Mouse Events
        if (GetComponent<Collider2D>() == null)
        {
            BoxCollider2D col = gameObject.AddComponent<BoxCollider2D>();
            col.isTrigger = true; // Trigger to avoid physics collisions
            col.size = new Vector2(1f, 1f); // Default size
        }
    }

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalScale = transform.localScale;

        // Apply passive if exists — trigger OnDiceSpawn
        if (diceData?.passive != null)
        {
            diceData.passive.DebugTrigger(this, "OnDiceSpawn");
            diceData.passive.OnDiceSpawn(this, runtimeStats);
        }

        if (diceData != null) PlayVFX(VFXType.Spawn);
        StartCoroutine(IdleVFXLoop());

        // Apply default sprite
        if (diceData != null && diceData.upgradeSprites != null && diceData.upgradeSprites.Length > 0 && spriteRenderer != null)
        {
            int level = runtimeStats != null ? runtimeStats.upgradeLevel : 1;
            int spriteIndex = level - 1; // Convert to 0-based index
            if (spriteIndex >= 0 && spriteIndex < diceData.upgradeSprites.Length)
                spriteRenderer.sprite = diceData.upgradeSprites[spriteIndex];
            else
                spriteRenderer.sprite = diceData.upgradeSprites[0];
        }

        // Notify neighbors of spawn
        foreach (var neighbor in GetNeighbors())
        {
            neighbor.diceData?.passive?.OnNeighborSpawn(neighbor, this);
        }
    }

    private void OnEnable()
    {
        GameEvents.OnCombatStarted += HandleCombatStart;
        GameEvents.OnCombatEnded += HandleCombatEnd;

        if (GameManager.Instance != null && GameManager.Instance.IsCombatActive)
            HandleCombatStart();
    }

    private void OnDisable()
    {
        GameEvents.OnCombatStarted -= HandleCombatStart;
        GameEvents.OnCombatEnded -= HandleCombatEnd;

        StopFiring();
    }

    private void HandleCombatStart()
    {
        // Trigger OnCombatStart passives first
        diceData?.passive?.OnCombatStart(this);

        // Start firing AFTER stats are modified
        StartFiring();
    }

    private void HandleCombatEnd()
    {
        // Trigger OnCombatEnd passives to reset stats
        diceData?.passive?.OnCombatEnd(this);

        StopFiring();
    }

    private bool isRemoved = false;

    public void NotifyRemoval(Vector3 atPosition)
    {
        if (isRemoved) return;
        isRemoved = true;

        // Temporarily move to the correct position so GetNeighbors() works for passives
        Vector3 originalPos = transform.position;
        transform.position = atPosition;

        var neighbors = GetNeighbors(atPosition);

        // Notify neighbors of removal at the specific position
        foreach (var neighbor in neighbors)
        {
            neighbor.diceData?.passive?.OnNeighborRemoved(neighbor, this);
        }

        diceData?.passive?.OnDiceRemoved(this);
        
        // Restore position (though it will be destroyed soon)
        transform.position = originalPos;

        if (diceData != null) PlayVFX(VFXType.Destroy);
    }

    void OnDestroy()
    {
        if (isRemoved) return; // Already handled manually

        // Notify neighbors of removal (fallback to current position)
        foreach (var neighbor in GetNeighbors())
        {
            neighbor.diceData?.passive?.OnNeighborRemoved(neighbor, this);
        }

        diceData?.passive?.OnDiceRemoved(this);
        if (diceData != null) PlayVFX(VFXType.Destroy);
    }

    public void StartFiring()
    {
        if (runtimeStats == null || fireRoutine != null) return;
        if (!diceData.canAttack) return;

        fireRoutine = StartCoroutine(FireLoop());
    }

    // ...

    IEnumerator FireLoop()
    {
        while (true)
        {
            // 🛑 Wait if no enemies are present
            if (EnemySpawner.activeEnemies.Count == 0)
            {
                // Debug.Log($"zzz {name} waiting for enemies..."); // Too spammy
                yield return null;
                continue;
            }

            float fireRate = runtimeStats.fireInterval;
            if (GameManager.Instance != null)
            {
                fireRate /= (1f + GameManager.Instance.globalFireRateMultiplier); // Higher multiplier = lower interval = faster fire
            }
            yield return new WaitForSeconds(Mathf.Max(0.001f, fireRate));
            
            // Double check before firing
            if (EnemySpawner.activeEnemies.Count > 0)
                FireProjectile();
        }
    }

    public void PlayVFX(VFXType type)
    {
        if (diceData == null) return;
        GameObject prefab = type switch
        {
            VFXType.Idle => diceData.vfxIdle,
            VFXType.Spawn => diceData.vfxSpawn,
            VFXType.Destroy => diceData.vfxDestroy,
            VFXType.Sold => diceData.vfxSold,
            VFXType.Bought => diceData.vfxBought,
            VFXType.Drag => diceData.vfxDrag,
            VFXType.Drop => diceData.vfxDrop,
            VFXType.Merge => diceData.vfxMerge,
            VFXType.Passive => diceData.vfxPassive,
            _ => null
        };

        if (prefab != null)
        {
            Instantiate(prefab, transform.position, Quaternion.identity);
        }
    }

    private IEnumerator IdleVFXLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(3f, 5f));
            PlayVFX(VFXType.Idle);
        }
    }

    public List<Dice> GetNeighbors(float radius = 1.5f)
    {
        return GetNeighbors(transform.position, radius);
    }

    public List<Dice> GetNeighbors(Vector3 center, float radius = 1.5f)
    {
        List<Dice> neighbors = new List<Dice>();
        Collider2D[] hits = Physics2D.OverlapCircleAll(center, radius);
        foreach (var hit in hits)
        {
            Dice d = hit.GetComponent<Dice>();
            if (d != null && d != this)
            {
                neighbors.Add(d);
            }
        }
        return neighbors;
    }

    public void OnMove(Vector3 oldPosition)
    {
        // 1. Handle Old Neighbors (Leaving)
        foreach (var neighbor in GetNeighbors(oldPosition))
        {
            // Neighbor reacts to me leaving
            neighbor.diceData?.passive?.OnNeighborRemoved(neighbor, this);
            
            // I react to neighbor leaving
            diceData?.passive?.OnNeighborRemoved(this, neighbor);
        }

        // 2. Handle New Neighbors (Arriving)
        foreach (var neighbor in GetNeighbors(transform.position))
        {
            // Neighbor reacts to me arriving
            neighbor.diceData?.passive?.OnNeighborSpawn(neighbor, this);
            
            // I react to neighbor arriving
            diceData?.passive?.OnNeighborSpawn(this, neighbor);
        }
    }

    // =========================
    // 🔹 Adjacency Visualization
    // =========================
    private void OnMouseEnter()
    {
        // 🛑 Block interaction if mouse is over UI (e.g. Reward Screen, Inventory)
        if (UnityEngine.EventSystems.EventSystem.current != null)
        {
            // Don't show tooltips during Reward Phase (focus on rewards)
            if (GameManager.Instance != null && GameManager.Instance.IsRewardPhaseActive)
            {
                return;
            }

            UnityEngine.EventSystems.PointerEventData pointerData = new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current)
            {
                position = UnityEngine.Input.mousePosition
            };

            System.Collections.Generic.List<UnityEngine.EventSystems.RaycastResult> results = new System.Collections.Generic.List<UnityEngine.EventSystems.RaycastResult>();
            UnityEngine.EventSystems.EventSystem.current.RaycastAll(pointerData, results);

            if (results.Count > 0)
            {
                // Check if any result is a blocking UI element (Layer 5 is usually UI)
                foreach (var result in results)
                {
                    if (result.gameObject.layer == 5) // UI Layer
                    {
                        // 🛑 Ignore specific UI elements that shouldn't block tooltips
                        if (!result.gameObject.activeInHierarchy) continue;

                        bool isIgnored = false;
                        Transform current = result.gameObject.transform;
                        
                        // Traverse up the hierarchy to check if any parent is in the ignore list
                        while (current != null)
                        {
                            string currentName = current.name;
                            
                            if (currentName == "StartCombatButton" || 
                                currentName == "CombatUI" || 
                                currentName == "WaveProgressUI" || 
                                currentName == "HealthBar" || 
                                currentName == "ShieldBar" || 
                                currentName == "CurrencyUI" || 
                                currentName == "DayHourUI" ||
                                currentName == "RerollButton" || 
                                currentName == "RewardPanel" ||
                                currentName.Contains("FloatingText")) // Ignore floating damage numbers
                            {
                                isIgnored = true;
                                break;
                            }
                            // Stop if we hit the Canvas to avoid going too far up
                            if (current.GetComponent<Canvas>() != null) break;
                            current = current.parent;
                        }

                        if (isIgnored) continue;

                        return;
                    }
                }
            }
        }

        // Highlight neighbors
        if (diceData?.passive != null)
        {
            var affected = diceData.passive.GetAffectedNeighbors(this);
            foreach (var d in affected)
            {
                d.ShowHighlight(true);
            }
        }

        // Show Tooltip
        if (TooltipManager.Instance != null)
        {
            TooltipManager.Instance.ShowTooltip(this, transform.position);
        }
    }

    private void OnMouseExit()
    {
        // Clear all highlights (inefficient but safe)
        var allDice = FindObjectsByType<Dice>(FindObjectsSortMode.None);
        foreach (var d in allDice)
        {
            d.ShowHighlight(false);
        }

        // Hide Tooltip
        if (TooltipManager.Instance != null)
        {
            TooltipManager.Instance.HideTooltip();
        }
    }

    public void ShowHighlight(bool show)
    {
        if (vfxHighlight != null)
        {
            vfxHighlight.SetActive(show);
        }
        else
        {
            // Fallback: Scale Pulse (Breathing)
            if (show)
            {
                if (highlightTween == null || !highlightTween.IsActive())
                {
                    highlightTween = transform.DOScale(originalScale * 1.1f, 0.6f)
                        .SetLoops(-1, LoopType.Yoyo)
                        .SetEase(Ease.InOutSine);
                }
            }
            else
            {
                if (highlightTween != null)
                {
                    highlightTween.Kill();
                    transform.localScale = originalScale;
                }
            }
        }
    }

    public void PlayEffect(GameObject effectPrefab)
    {
        if (effectPrefab != null)
        {
            Instantiate(effectPrefab, transform.position, Quaternion.identity);
        }
    }

    public void StopFiring()
    {
        if (fireRoutine != null)
        {
            StopCoroutine(fireRoutine);
            fireRoutine = null;
        }
    }



    void FireProjectile()
    {
        if (runtimeStats == null) return;

        int rolledValue = Random.Range(1, runtimeStats.diceSides + 1);
        float amount = runtimeStats.baseDamage * rolledValue;

        // Apply Global Damage Multiplier
        if (GameManager.Instance != null)
        {
            amount *= (1f + GameManager.Instance.globalDamageMultiplier);
        }

        // Apply Relic Damage Multiplier
        if (RelicManager.Instance != null)
        {
            amount *= RelicManager.Instance.GetDamageMultiplier();
        }

        bool isCrit = Random.value < (runtimeStats.critChance + (GameManager.Instance != null ? GameManager.Instance.globalCritChance : 0f));
        if (isCrit) amount *= 2f;

        int multicastCount = 1;
        while (Random.value < runtimeStats.multicastChance)
        {
            multicastCount++;
        }

        for (int i = 0; i < multicastCount; i++)
        {
            bool skipProjectile = false;
            float finalDamage = amount;

            diceData?.passive?.OnDiceFire(this, ref finalDamage, ref skipProjectile);

            // Trigger neighbor passives
            foreach (var neighbor in GetNeighbors())
            {
                neighbor.diceData?.passive?.OnNeighborFire(neighbor, this, ref finalDamage, ref skipProjectile);
            }

            // Trigger Global Passives
            if (GameManager.Instance != null)
            {
                foreach (var gp in GameManager.Instance.globalPassives)
                {
                    gp.OnDiceFire(this, ref finalDamage, ref skipProjectile);
                }
            }

            if (skipProjectile) continue;

            if (projectilePrefab == null) return;

            Vector3 spawnPos = transform.position;
            GameObject projObj = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);

            Projectile proj = projObj.GetComponent<Projectile>();
            if (proj != null)
            {
                proj.owner = ProjectileOwner.Player;
                proj.damage = finalDamage;
                proj.isHoming = true; // Dice projectiles are homing
                proj.target = EnemySpawner.GetRandomEnemy(); // Get target
                proj.validToDamage = true;
            }
            else
            {
                // Fallback for old Bullet prefabs if they haven't been updated yet?
                // Or maybe we should add Projectile component if missing?
                // But Bullet.cs is being deleted.
                // Let's assume user will update prefabs or we add it dynamically.
                // For now, let's try to add it if missing, just in case.
                proj = projObj.AddComponent<Projectile>();
                proj.owner = ProjectileOwner.Player;
                proj.damage = finalDamage;
                proj.isHoming = true;
                proj.target = EnemySpawner.GetRandomEnemy();
                proj.validToDamage = true;
            }

            // ✅ Assign Source Dice for Passives (e.g. Electro)
            if (proj != null)
            {
                proj.sourceDice = this;
            }

            SpawnFloatingText(finalDamage, isCrit, multicastCount > 1);
        }
    }

    public void SpawnFloatingText(float amount, bool isCrit, bool isMulticast, bool isHeal = false, bool isShield = false)
    {
        if (floatingTextPrefab_Normal == null) return;

        Vector3 spawnPos = transform.position + Vector3.up * 0.1f;
        GameObject textGO = Instantiate(floatingTextPrefab_Normal, spawnPos, Quaternion.identity);
        FloatingText text = textGO.GetComponent<FloatingText>();

        if (text != null)
        {
            string symbol = isHeal || isShield ? "+" : "";
            string displayText = symbol + Mathf.RoundToInt(amount);

            Color color = Color.white;
            int fontSize = 30;

            if (isShield && isCrit) { color = Color.blue; fontSize = 36; }
            else if (isShield) { color = new Color(0.4f, 0.6f, 1f); fontSize = 30; }
            else if (isHeal && isCrit) { color = Color.yellow; fontSize = 36; }
            else if (isHeal) { color = Color.green; fontSize = 30; }
            else if (isCrit) { color = Color.red; fontSize = 36; }
            else if (isMulticast) { color = Color.cyan; fontSize = 30; }

            text.Show(displayText, color, fontSize);
        }
    }
}
