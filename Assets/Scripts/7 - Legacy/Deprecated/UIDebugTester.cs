using UnityEngine;

namespace TabletopShop
{
    /// <summary>
    /// Quick debug script to test if ShopStatusUI is working and if GameManager events are firing
    /// </summary>
    public class UIDebugTester : MonoBehaviour
    {
        [Header("Debug Controls")]
        [SerializeField] private bool testOnStart = true;
        [SerializeField] private float testMoney = 500f;
        
        private void Start()
        {
            if (testOnStart)
            {
                // Wait a frame to ensure everything is initialized
                Invoke(nameof(RunDebugTests), 0.1f);
            }
        }
        
        [ContextMenu("Run Debug Tests")]
        private void RunDebugTests()
        {
            Debug.Log("=== UI Debug Test Starting ===");
            
            // Check if GameManager exists
            if (GameManager.Instance == null)
            {
                Debug.LogError("UIDebugTester: GameManager.Instance is null!");
                return;
            }
            
            Debug.Log($"GameManager found: {GameManager.Instance.name}");
            Debug.Log($"GameManager position: {GameManager.Instance.transform.position}");
            Debug.Log($"GameManager is active: {GameManager.Instance.gameObject.activeInHierarchy}");
            
            // Check if ShopStatusUI exists
            var shopStatusUI = FindAnyObjectByType<ShopStatusUI>();
            if (shopStatusUI == null)
            {
                Debug.LogError("UIDebugTester: ShopStatusUI not found in scene!");
                return;
            }
            
            Debug.Log($"ShopStatusUI found: {shopStatusUI.name}");
            Debug.Log($"ShopStatusUI position: {shopStatusUI.transform.position}");
            Debug.Log($"ShopStatusUI is active: {shopStatusUI.gameObject.activeInHierarchy}");
            Debug.Log($"ShopStatusUI enabled: {shopStatusUI.enabled}");
            
            // Test GameManager data access
            var economicStatus = GameManager.Instance.GetEconomicStatus();
            Debug.Log($"Economic Status - Money: ${economicStatus.money:F2}, Day: {economicStatus.day}, IsDay: {economicStatus.isDay}");
            
            // Test triggering events manually
            Debug.Log("Testing GameManager events...");
            
            // Add some money to trigger event
            GameManager.Instance.AddMoney(testMoney, "Debug Test");
            
            // Try to force refresh
            if (shopStatusUI != null)
            {
                shopStatusUI.ForceRefresh();
            }
            
            Debug.Log("=== UI Debug Test Complete ===");
        }
        
        [ContextMenu("Test Money Change")]
        private void TestMoneyChange()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddMoney(100f, "Debug Test");
                Debug.Log("Added $100 to test money change event");
            }
        }
        
        [ContextMenu("Force Next Day")]
        private void TestDayChange()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ForceNextDay();
                Debug.Log("Forced next day to test day change event");
            }
        }
    }
}