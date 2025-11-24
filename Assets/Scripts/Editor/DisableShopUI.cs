using UnityEngine;
using UnityEditor;

public class DisableShopUI : EditorWindow
{
    [MenuItem("Tools/Disable Shop UI")]
    public static void DisableShop()
    {
        // Find ShopItemContainer
        GameObject shopContainer = GameObject.Find("ShopItemContainer");
        if (shopContainer != null)
        {
            shopContainer.SetActive(false);
            Debug.Log("✅ Disabled ShopItemContainer");
        }
        else
        {
            Debug.LogWarning("ShopItemContainer not found");
        }

        // Find Shopmanager
        GameObject shopManager = GameObject.Find("Shopmanager");
        if (shopManager != null)
        {
            shopManager.SetActive(false);
            Debug.Log("✅ Disabled Shopmanager");
        }
        else
        {
            Debug.LogWarning("Shopmanager not found");
        }

        // Find any other shop-related UI
        foreach (GameObject go in GameObject.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
        {
            if (go.name.ToLower().Contains("shop") && go.activeInHierarchy)
            {
                Debug.Log($"Found active shop-related object: {go.name}");
            }
        }

        EditorUtility.DisplayDialog("Shop UI Disabled", "Shop UI elements have been disabled.\n\nThe reward screen should now display properly!", "OK");
    }
}
