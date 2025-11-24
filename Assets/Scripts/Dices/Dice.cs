using System.Collections;
using UnityEngine;

public class Dice : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject projectilePrefab;
    public GameObject floatingTextPrefab_Normal;
    public GameObject floatingTextPrefab_Crit;
    public GameObject floatingTextPrefab_Multicast;
    public GameObject tooltipPrefab;

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

    void Awake()
    {
        // ✅ Initialize runtimeStats if diceData already exists
        if (diceData != null && runtimeStats == null)
            runtimeStats = new RuntimeDiceData(diceData);
    }

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Apply passive if exists — trigger OnDiceSpawn
        if (diceData?.passive != null)
        {
            diceData.passive.DebugTrigger(this, "OnDiceSpawn");
            diceData.passive.OnDiceSpawn(this, runtimeStats);
        }

        if (diceData != null) PlayEffect(diceData.effectOnDrop);

        // Apply default sprite
        if (diceData != null && diceData.upgradeSprites != null && diceData.upgradeSprites.Length > 0 && spriteRenderer != null)
        {
            spriteRenderer.sprite = diceData.upgradeSprites[0];
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
        Debug.Log($"⚔️ Combat started — calling passives for {name}");

        // Trigger OnCombatStart passives first
        diceData?.passive?.OnCombatStart(this);

        // Start firing AFTER stats are modified
        StartFiring();
    }

    private void HandleCombatEnd()
    {
        Debug.Log($"🏁 Combat ended — calling passives for {name}");

        // Trigger OnCombatEnd passives to reset stats
        diceData?.passive?.OnCombatEnd(this);

        StopFiring();
    }

    void OnDestroy()
    {
        diceData?.passive?.OnDiceRemoved(this);
        if (diceData != null) PlayEffect(diceData.effectOnDestroy);
    }

    public void StartFiring()
    {
        if (runtimeStats == null || fireRoutine != null) return;
        if (!diceData.canAttack) return; // 🛑 Check if dice can attack

        fireRoutine = StartCoroutine(FireLoop());
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

        diceData?.passive?.OnDiceRemoved(this);
    }

    IEnumerator FireLoop()
    {
        while (true)
        {
            // 🛑 Wait if no enemies are present
            if (EnemySpawner.activeEnemies.Count == 0)
            {
                yield return null;
                continue;
            }

            yield return new WaitForSeconds(Mathf.Max(0.001f, runtimeStats.fireInterval));
            
            // Double check before firing
            if (EnemySpawner.activeEnemies.Count > 0)
                FireProjectile();
        }
    }

    void FireProjectile()
    {
        if (runtimeStats == null) return;

        int rolledValue = Random.Range(1, runtimeStats.diceSides + 1);
        float amount = runtimeStats.baseDamage * rolledValue;

        bool isCrit = Random.value < runtimeStats.critChance;
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

            if (skipProjectile) continue;

            if (projectilePrefab == null) return;

            Vector3 spawnPos = transform.position;
            GameObject proj = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);

            Bullet b = proj.GetComponent<Bullet>();
            if (b != null)
                b.damage = finalDamage;

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
