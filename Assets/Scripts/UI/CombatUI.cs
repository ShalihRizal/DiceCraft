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
        }

        GameEvents.OnCombatStarted += OnCombatStarted;
        GameEvents.OnCombatEnded += OnCombatEnded;
    }

    void OnDestroy()
    {
        GameEvents.OnCombatStarted -= OnCombatStarted;
        GameEvents.OnCombatEnded -= OnCombatEnded;
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

    void OnCombatEnded()
    {
        if (startCombatButton != null)
            startCombatButton.gameObject.SetActive(true);
    }
}
