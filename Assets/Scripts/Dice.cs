using System.Collections;
using UnityEngine;

public class Dice : MonoBehaviour
{
    public GameObject projectilePrefab;
    public GameObject floatingTextPrefab_Normal;
    public GameObject floatingTextPrefab_Crit;
    public GameObject floatingTextPrefab_Multicast;

    public DiceData diceData; // assigned from prefab
    public RuntimeDiceData runtimeStats;
    private DiceTooltip tooltipInstance;

    public GameObject tooltipPrefab;


    public enum PassiveAbility { None, FasterFire, BiggerProjectile, DoubleShot, Heal, Shield}
    public PassiveAbility passive = PassiveAbility.None;

    private Coroutine fireRoutine;

    void Awake()
    {
        if (diceData != null)
            runtimeStats = new RuntimeDiceData(diceData); // create unique instance per dice
    }

    void Start()
    {
        ApplyPassive();
        if (diceData != null && diceData.upgradeSprites != null && diceData.upgradeSprites.Length > 0)
        {
            GetComponent<SpriteRenderer>().sprite = diceData.upgradeSprites[0];
        }

    }

    void OnEnable()
    {
        GameEvents.OnCombatStarted += StartFiring;
        GameEvents.OnCombatEnded += StopFiring;

        if (GameManager.Instance != null && GameManager.Instance.IsCombatActive)
            StartFiring();
    }

    void OnDisable()
    {
        GameEvents.OnCombatStarted -= StartFiring;
        GameEvents.OnCombatEnded -= StopFiring;

        StopFiring();
    }

    void ApplyPassive()
    {


        if (runtimeStats == null) return;

        if (passive == PassiveAbility.FasterFire)
            runtimeStats.fireInterval *= 0.5f;

        Debug.Log($"{diceData.diceName} fireInterval: {runtimeStats.fireInterval}");
    }

    void StartFiring()
    {
        if (runtimeStats == null) return;

        if (fireRoutine == null)
            fireRoutine = StartCoroutine(FireLoop());
    }

    void StopFiring()
    {
        if (fireRoutine != null)
        {
            StopCoroutine(fireRoutine);
            fireRoutine = null;
        }
    }

    IEnumerator FireLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(runtimeStats.fireInterval);
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

        // Multicast check
        int multicastCount = 1;
        while (Random.value < runtimeStats.multicastChance)
        {
            multicastCount++;
        }

        for (int i = 0; i < multicastCount; i++)
        {
            if (diceData.passive == PassiveAbility.Heal)
                {
                    var player = FindObjectOfType<PlayerHealth>();
                    if (player != null)
                    {
                        player.Heal(amount);
                        SpawnFloatingText(amount, isCrit, multicastCount > 1, isHeal: true);
                    }
                    continue;
                }else if (diceData.passive == PassiveAbility.Shield)
                    {
                        var player = FindObjectOfType<PlayerHealth>();
                        if (player != null)
                        {
                            player.AddShield(amount);
                            SpawnFloatingText(amount, isCrit, multicastCount > 1, isShield: true);
                        }
                        continue;
                    }


            // Normal projectile
            if (projectilePrefab == null) return;

            Vector3 spawnPos = transform.position;
            GameObject proj = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);

            Bullet b = proj.GetComponent<Bullet>();
            if (b != null)
                b.damage = amount;


            SpawnFloatingText(amount, isCrit, multicastCount > 1);
        }
    }
    
    void SpawnFloatingText(float amount, bool isCrit, bool isMulticast, bool isHeal = false, bool isShield = false)
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

        if (isShield && isCrit)  { color = Color.blue;    fontSize = 36; }
        else if (isShield)       { color = new Color(0.4f, 0.6f, 1f); fontSize = 30; }
        else if (isHeal && isCrit) { color = Color.yellow; fontSize = 36; }
        else if (isHeal)         { color = Color.green;  fontSize = 30; }
        else if (isCrit)         { color = Color.red;    fontSize = 36; }
        else if (isMulticast)    { color = Color.cyan;   fontSize = 30; }

        text.Show(displayText, color, fontSize);
    }
}





}
