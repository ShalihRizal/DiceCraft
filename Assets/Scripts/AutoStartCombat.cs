using UnityEngine;
using System.Collections;

public class AutoStartCombat : MonoBehaviour
{
    IEnumerator Start()
    {
        yield return new WaitForSeconds(0.5f); // Wait for initialization
        if (GameManager.Instance != null)
        {
            Debug.Log("🚀 Auto-starting combat...");
            GameManager.Instance.StartCombat();
        }
    }
}
