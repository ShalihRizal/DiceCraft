using UnityEngine;
using UnityEngine.UI;

public class CombatUI : MonoBehaviour
{
    public Button startCombatButton;
    public GameObject trashZone;
    public Button rerollButton;

    void OnEnable()
    {
        GameEvents.OnCombatStarted += DisableButton;
        GameEvents.OnCombatEnded += EnableButton;
    }

    void OnDisable()
    {
        GameEvents.OnCombatStarted -= DisableButton;
        GameEvents.OnCombatEnded -= EnableButton;
    }

    public void OnStartCombatClicked()
    {
        GameManager.Instance?.StartCombat();
    }

    void DisableButton()
    {
        if (startCombatButton != null)
            startCombatButton.gameObject.SetActive(false);
            trashZone.gameObject.SetActive(false);
        if (rerollButton != null)
            rerollButton.gameObject.SetActive(false);
    }

    void EnableButton()
    {
        if (startCombatButton != null)
            startCombatButton.gameObject.SetActive(true);
            trashZone.gameObject.SetActive(true);
        if (rerollButton != null)
            rerollButton.gameObject.SetActive(true);
    }
}
