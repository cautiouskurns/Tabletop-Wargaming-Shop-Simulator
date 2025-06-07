using UnityEngine;

namespace TabletopShop
{
    /// <summary>
    /// Integration test script to validate ShopStatusUI and CustomerSpawner work with GameManager
    /// This script demonstrates the complete economic integration system
    /// </summary>
    public class EconomicIntegrationTest : MonoBehaviour
    {
        [Header("Test Configuration")]
        [SerializeField] private bool autoRunTests = true;
        [SerializeField] private float testInterval = 5.0f;
        [SerializeField] private bool enableDetailedLogging = true;
        
        // Component references for testing
        private GameManager gameManager;
        private CustomerSpawner customerSpawner;
        private ShopStatusUI shopStatusUI;
        
        // Test state
        private float nextTestTime;
        private int testIterations = 0;
        
        private void Start()
        {
            // Find all required components for integration testing
            FindComponents();
            
            // Schedule first test
            nextTestTime = Time.time + testInterval;
            
            Debug.Log("=== ECONOMIC INTEGRATION TEST STARTED ===");
            LogInitialState();
        }
        
        private void Update()
        {
            if (autoRunTests && Time.time >= nextTestTime)
            {
                RunIntegrationTests();
                nextTestTime = Time.time + testInterval;
                testIterations++;
            }
        }
        
        /// <summary>
        /// Find and cache references to all integration components
        /// </summary>
        private void FindComponents()
        {
            gameManager = GameManager.Instance;
            customerSpawner = FindFirstObjectByType<CustomerSpawner>();
            shopStatusUI = FindFirstObjectByType<ShopStatusUI>();
            
            Debug.Log($"Integration Test - Found Components:");
            Debug.Log($"  GameManager: {gameManager != null}");
            Debug.Log($"  CustomerSpawner: {customerSpawner != null}");
            Debug.Log($"  ShopStatusUI: {shopStatusUI != null}");
        }
        
        /// <summary>
        /// Log the initial state of all systems
        /// </summary>
        private void LogInitialState()
        {
            if (gameManager != null)
            {
                var (money, day, isDay, reputation, customers, revenue, expenses) = gameManager.GetEconomicStatus();
                
                Debug.Log("=== INITIAL ECONOMIC STATE ===");
                Debug.Log($"Money: ${money:F2}");
                Debug.Log($"Day: {day}");
                Debug.Log($"Time of Day: {(isDay ? "Day" : "Night")}");
                Debug.Log($"Reputation: {reputation:F1}/100");
                Debug.Log($"Customers Served: {customers}/{gameManager.MaxDailyCustomers}");
                Debug.Log($"Revenue: ${revenue:F2}");
                Debug.Log($"Expenses: ${expenses:F2}");
                Debug.Log("===============================");
            }
        }
        
        /// <summary>
        /// Run comprehensive integration tests
        /// </summary>
        private void RunIntegrationTests()
        {
            Debug.Log($"\n=== INTEGRATION TEST #{testIterations + 1} ===");
            
            // Test 1: GameManager State
            TestGameManagerState();
            
            // Test 2: CustomerSpawner Integration
            TestCustomerSpawnerIntegration();
            
            // Test 3: ShopStatusUI Integration
            TestShopStatusUIIntegration();
            
            // Test 4: Event System Integration
            TestEventSystemIntegration();
            
            Debug.Log("=== INTEGRATION TEST COMPLETE ===\n");
        }
        
        /// <summary>
        /// Test GameManager economic state and functionality
        /// </summary>
        private void TestGameManagerState()
        {
            if (gameManager == null)
            {
                Debug.LogError("TEST FAILED: GameManager is null!");
                return;
            }
            
            var (money, day, isDay, reputation, customers, revenue, expenses) = gameManager.GetEconomicStatus();
            
            Debug.Log($"GameManager State Test:");
            Debug.Log($"  Money: ${money:F2} (Valid: {money >= 0})");
            Debug.Log($"  Day: {day} (Valid: {day >= 1})");
            Debug.Log($"  Time: {(isDay ? "Day" : "Night")}");
            Debug.Log($"  Reputation: {reputation:F1}/100 (Valid: {reputation >= 0 && reputation <= 100})");
            Debug.Log($"  Customers: {customers}/{gameManager.MaxDailyCustomers}");
            
            // Validation
            bool valid = money >= 0 && day >= 1 && reputation >= 0 && reputation <= 100;
            Debug.Log($"  GameManager State: {(valid ? "PASS" : "FAIL")}");
        }
        
        /// <summary>
        /// Test CustomerSpawner integration with GameManager day/night cycle
        /// </summary>
        private void TestCustomerSpawnerIntegration()
        {
            if (customerSpawner == null)
            {
                Debug.LogWarning("CustomerSpawner not found - skipping test");
                return;
            }
            
            if (gameManager == null)
            {
                Debug.LogError("GameManager required for CustomerSpawner test");
                return;
            }
            
            Debug.Log($"CustomerSpawner Integration Test:");
            Debug.Log($"  Active Customers: {customerSpawner.ActiveCustomerCount}");
            Debug.Log($"  Is At Max Capacity: {customerSpawner.IsAtMaxCapacity}");
            Debug.Log($"  Can Spawn Customer: {customerSpawner.CanSpawnCustomer}");
            Debug.Log($"  Is Spawning: {customerSpawner.IsSpawning}");
            
            // Test day/night integration
            bool isDayTime = gameManager.IsDayTime;
            bool canSpawn = customerSpawner.CanSpawnCustomer;
            
            Debug.Log($"  Day/Night Integration:");
            Debug.Log($"    Current Time: {(isDayTime ? "Day" : "Night")}");
            Debug.Log($"    Can Spawn During {(isDayTime ? "Day" : "Night")}: {canSpawn}");
            
            // Validate that spawning respects day/night cycle
            if (!isDayTime && canSpawn && customerSpawner.ActiveCustomerCount < gameManager.MaxDailyCustomers)
            {
                Debug.LogWarning("  WARNING: CustomerSpawner allows spawning during night time!");
            }
            else
            {
                Debug.Log($"  Day/Night Integration: PASS");
            }
        }
        
        /// <summary>
        /// Test ShopStatusUI integration with GameManager
        /// </summary>
        private void TestShopStatusUIIntegration()
        {
            if (shopStatusUI == null)
            {
                Debug.LogWarning("ShopStatusUI not found - skipping test");
                return;
            }
            
            Debug.Log($"ShopStatusUI Integration Test:");
            Debug.Log($"  Component Active: {shopStatusUI.gameObject.activeInHierarchy}");
            Debug.Log($"  Component Enabled: {shopStatusUI.enabled}");
            
            // Test if ShopStatusUI can access GameManager data
            if (gameManager != null)
            {
                Debug.Log($"  GameManager Access: Available");
                Debug.Log($"  Event Integration: Should be receiving real-time updates");
            }
            else
            {
                Debug.LogError("  GameManager Access: FAILED");
            }
        }
        
        /// <summary>
        /// Test event system integration between components
        /// </summary>
        private void TestEventSystemIntegration()
        {
            if (gameManager == null)
            {
                Debug.LogError("GameManager required for event system test");
                return;
            }
            
            Debug.Log($"Event System Integration Test:");
            
            // Test adding a small amount of money to trigger events
            float originalMoney = gameManager.CurrentMoney;
            gameManager.AddMoney(1.0f, "Integration Test");
            
            // Check if money was updated
            float newMoney = gameManager.CurrentMoney;
            bool moneyUpdated = newMoney > originalMoney;
            
            Debug.Log($"  Money Event Test: {(moneyUpdated ? "PASS" : "FAIL")}");
            Debug.Log($"    Before: ${originalMoney:F2}");
            Debug.Log($"    After: ${newMoney:F2}");
            
            // Note: We can't easily test UI updates in this script since it's not visual,
            // but the event system should trigger ShopStatusUI updates automatically
        }
        
        /// <summary>
        /// Force advance to next day for testing day/night cycle integration
        /// </summary>
        [ContextMenu("Test Force Next Day")]
        public void TestForceNextDay()
        {
            if (gameManager != null)
            {
                Debug.Log("=== TESTING FORCE NEXT DAY ===");
                LogInitialState();
                
                gameManager.ForceNextDay();
                
                // Wait a frame and log new state
                StartCoroutine(LogStateAfterDelay());
            }
        }
        
        private System.Collections.IEnumerator LogStateAfterDelay()
        {
            yield return null; // Wait one frame
            
            Debug.Log("=== STATE AFTER FORCE NEXT DAY ===");
            LogInitialState();
        }
        
        /// <summary>
        /// Test customer purchase processing
        /// </summary>
        [ContextMenu("Test Customer Purchase")]
        public void TestCustomerPurchase()
        {
            if (gameManager != null)
            {
                Debug.Log("=== TESTING CUSTOMER PURCHASE ===");
                LogInitialState();
                
                // Simulate a customer purchase
                gameManager.ProcessCustomerPurchase(25.50f, 0.9f);
                
                Debug.Log("=== STATE AFTER CUSTOMER PURCHASE ===");
                LogInitialState();
            }
        }
    }
}
