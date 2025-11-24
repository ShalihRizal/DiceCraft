using UnityEngine;
using UnityEngine.UI;

public class CombatUI : MonoBehaviour
{
    public Button startCombatButton;

    void Start()
    {
        if (startCombatButton != null)
        {
            startCombatButton.onClick.AddListener(OnStartCombatClicked);
            startCombatButton.gameObject.SetActive(true); // Ensure it's visible at start if not in combat
        }

        GameEvents.OnCombatStarted += OnCombatStarted;
        GameEvents.OnPreparationPhaseStarted += OnPreparationPhaseStarted;
    }

    void OnDestroy()
    {
        GameEvents.OnCombatStarted -= OnCombatStarted;
        GameEvents.OnPreparationPhaseStarted -= OnPreparationPhaseStarted;
    }

    void OnStartCombatClicked()
    {
        if (GameManager.Instance != null && !GameManager.Instance.IsCombatActive)
        {
            GameManager.Instance.StartCombat();
        }
    }

    void OnCombatStarted()
    {
        if (startCombatButton != null)
            startCombatButton.gameObject.SetActive(false);
    }

    void OnPreparationPhaseStarted()
    {
        if (startCombatButton != null)
            startCombatButton.gameObject.SetActive(true);
    }
}
