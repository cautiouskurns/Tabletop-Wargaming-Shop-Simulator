using UnityEngine;
using TabletopShop;

namespace TabletopShop
{
    /// <summary>
    /// Quick manual test for ProcessCustomerPurchase
    /// </summary>
    public class QuickPurchaseTest : MonoBehaviour
    {
        [Header("Test Settings")]
        [SerializeField] private float testPurchaseAmount = 50.0f;
        [SerializeField] private float testSatisfaction = 0.8f;
        
        private void Start()
        {
            Debug.Log("QuickPurchaseTest: Starting manual test");
            TestPurchaseProcessing();
        }
        
        [ContextMenu("Run Purchase Test")]
        public void TestPurchaseProcessing()
        {
            if (GameManager.Instance == null)
            {
                Debug.LogError("GameManager instance not found!");
                return;
            }
            
            // Get initial state
            var initialState = GameManager.Instance.GetEconomicStatus();
            Debug.Log($"INITIAL STATE: Money=${initialState.money:F2}, Customers={initialState.customers}, Revenue=${initialState.revenue:F2}");
            Debug.Log($"INITIAL CustomersServedToday property = {GameManager.Instance.CustomersServedToday}");
            
            // Call ProcessCustomerPurchase
            Debug.Log($"Calling ProcessCustomerPurchase with amount=${testPurchaseAmount:F2}, satisfaction={testSatisfaction:F2}");
            GameManager.Instance.ProcessCustomerPurchase(testPurchaseAmount, testSatisfaction);
            
            // Get final state
            var finalState = GameManager.Instance.GetEconomicStatus();
            Debug.Log($"FINAL STATE: Money=${finalState.money:F2}, Customers={finalState.customers}, Revenue=${finalState.revenue:F2}");
            Debug.Log($"FINAL CustomersServedToday property = {GameManager.Instance.CustomersServedToday}");
            
            // Calculate changes
            float moneyChange = finalState.money - initialState.money;
            int customerChange = finalState.customers - initialState.customers;
            float revenueChange = finalState.revenue - initialState.revenue;
            
            Debug.Log($"CHANGES: Money=+${moneyChange:F2}, Customers=+{customerChange}, Revenue=+${revenueChange:F2}");
            
            // Verify results
            if (moneyChange > 0 && customerChange > 0 && revenueChange > 0)
            {
                Debug.Log("✅ TEST PASSED - All metrics increased correctly!");
            }
            else
            {
                Debug.LogError("❌ TEST FAILED - Not all metrics increased as expected");
            }
        }
    }
}
