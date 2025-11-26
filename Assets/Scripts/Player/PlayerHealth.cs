using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using DG.Tweening;


public class PlayerHealth : MonoBehaviour
{
    public float maxHealth = 100f;
    public float currentHealth;

    public Slider healthSlider;
    public TextMeshProUGUI percentageText;

    public float regenAmount = 0f;
    public float regenInterval = 1f;
    private Coroutine regenRoutine;

    public Slider shieldSlider;

    
    public static PlayerHealth Instance;

    private float currentShield = 0f;

    void Awake()
    {
        Debug.Log("PlayerHealth Awake called!");
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        currentHealth = maxHealth;
        currentShield = 0f;
        
        if (shieldSlider != null)
        {
            shieldSlider.minValue = 0f;
            shieldSlider.maxValue = 1f;
            shieldSlider.value = 0f;
        }

        UpdateUI();
        GameEvents.OnCombatStarted += ResetShield;
    }
    
    void ResetShield()
{
    if (currentShield > 0)
    {
        GameEvents.RaiseLostShield(currentShield);
        currentShield = 0f;
        UpdateUI();
    }
}


    public void TakeDamage(float amount)
    {
        float damage = amount;

        if (currentShield > 0)
        {
            float absorbed = Mathf.Min(currentShield, damage);
            currentShield -= absorbed;
            damage -= absorbed;

            GameEvents.RaiseLostShield(absorbed);
        }


        if (damage > 0)
        {
            currentHealth -= damage;
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        }

        UpdateUI();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void AddShield(float amount)
{
    float previous = currentShield;
    currentShield += amount;
    float gained = currentShield - previous;

    if (gained > 0f)
        GameEvents.RaiseGainShield(gained);

    UpdateUI();
}


    public void Heal(float amount)
    {
        float previous = currentHealth;
        currentHealth += amount;

        if (currentHealth > maxHealth)
        {
            float overheal = currentHealth - maxHealth;
            currentHealth = maxHealth;

            // ✅ Only triggered from Heal
            GameEvents.RaiseOverHeal(overheal);
        }

        UpdateUI();
    }

    public void Regen(float amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        // ❌ No overheal logic here
        UpdateUI();
    }

    void UpdateUI()
    {
        float healthRatio = currentHealth / maxHealth;
        float shieldRatio = maxHealth > 0 ? currentShield / maxHealth : 0;

        Debug.Log($"UpdateUI: Health: {currentHealth}/{maxHealth} ({healthRatio}), Shield: {currentShield} ({shieldRatio}), Slider Value: {shieldSlider?.value}");

        healthSlider?.DOValue(healthRatio, 0.3f).SetEase(Ease.OutQuad);
        shieldSlider?.DOValue(shieldRatio, 0.3f).SetEase(Ease.OutQuad);

        if (percentageText != null)
        {
            int endValue = Mathf.RoundToInt(healthRatio * 100f);
            DOTween.To(() => int.Parse(percentageText.text.Replace("%", "")), x =>
            {
                percentageText.text = $"{x}%";
            }, endValue, 0.3f).SetEase(Ease.OutQuad);
        }
    }

    void Die()
    {
        Debug.Log("💀 Player died!");
        StartCoroutine(HandleDeathSlowdown());
    }

    IEnumerator HandleDeathSlowdown()
    {
        GameEvents.RaisePlayerDied();
        Time.timeScale = 0.2f;
        yield return new WaitForSecondsRealtime(2f);
        GameManager.Instance.GameOver();
    }
    
    // Add inside the class:
IEnumerator RegenLoop()
{
        while (true)
        {
            yield return new WaitForSeconds(regenInterval);
            Regen(regenAmount);
    }
}

public void StartRegen()
{
    if (regenAmount > 0f && regenRoutine == null)
        regenRoutine = StartCoroutine(RegenLoop());
}

public void StopRegen()
{
    if (regenRoutine != null)
    {
        StopCoroutine(regenRoutine);
        regenRoutine = null;
    }
}

void OnEnable()
{
    GameEvents.OnCombatStarted += StartRegen;
    GameEvents.OnCombatEnded += StopRegen;
}

void OnDisable()
{
    GameEvents.OnCombatStarted -= StartRegen;
    GameEvents.OnCombatEnded -= StopRegen;
}
}
