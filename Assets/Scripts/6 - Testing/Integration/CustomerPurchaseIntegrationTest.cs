using UnityEngine;
using System.Collections;

namespace TabletopShop
{
    /// <summary>
    /// Integration test for customer purchase flow with GameManager
    /// Tests the complete flow: Customer shopping → Product selection → GameManager purchase processing
    /// </summary>
    public class CustomerPurchaseIntegrationTest : MonoBehaviour
    {
        [Header("Test Configuration")]
        [SerializeField] private bool runTestOnStart = false;
        [SerializeField] private bool enableDebugLogs = true;
        
        [Header("References")]
        [SerializeField] private CustomerBehavior testCustomer;
        [SerializeField] private ShelfSlot[] testShelves;
        [SerializeField] private Product[] testProducts;
        
        private void Start()
        {
            if (runTestOnStart)
            {
                StartCoroutine(RunPurchaseIntegrationTest());
            }
        }
        
        /// <summary>
        /// Run the complete customer purchase integration test
        /// </summary>
        [ContextMenu("Run Purchase Integration Test")]
        public void RunPurchaseIntegrationTestManual()
        {
            StartCoroutine(RunPurchaseIntegrationTest());
        }
        
        /// <summary>
        /// Complete integration test for customer purchase workflow
        /// </summary>
        private IEnumerator RunPurchaseIntegrationTest()
        {
            LogTest("=== CUSTOMER PURCHASE INTEGRATION TEST STARTED ===");
            
            // Test 1: Verify GameManager exists and is initialized
            yield return StartCoroutine(TestGameManagerSetup());
            
            // Test 2: Create test customer if needed
            yield return StartCoroutine(TestCustomerSetup());
            
            // Test 3: Set up products on shelves
            yield return StartCoroutine(TestProductSetup());
            
            // Test 4: Run customer shopping behavior
            yield return StartCoroutine(TestCustomerShopping());
            
            // Test 5: Test purchase processing
            yield return StartCoroutine(TestPurchaseProcessing());
            
            LogTest("=== CUSTOMER PURCHASE INTEGRATION TEST COMPLETED ===");
        }
        
        /// <summary>
        /// Test GameManager initialization and economic state
        /// </summary>
        private IEnumerator TestGameManagerSetup()
        {
            LogTest("Testing GameManager setup...");
            
            // Verify GameManager exists
            if (GameManager.Instance == null)
            {
                LogError("GameManager.Instance is null! GameManager not found in scene.");
                yield break;
            }
            
            // Log initial economic state
            var economicStatus = GameManager.Instance.GetEconomicStatus();
            LogTest($"Initial GameManager State:");
            LogTest($"  Money: ${economicStatus.money:F2}");
            LogTest($"  Day: {economicStatus.day}");
            LogTest($"  Reputation: {economicStatus.reputation:F1}");
            LogTest($"  Customers Served Today: {economicStatus.customers}");
            LogTest($"  Daily Revenue: ${economicStatus.revenue:F2}");
            
            yield return new WaitForSeconds(0.5f);
        }
        
        /// <summary>
        /// Test customer behavior setup
        /// </summary>
        private IEnumerator TestCustomerSetup()
        {
            LogTest("Testing Customer setup...");
            
            // Find or create test customer
            if (testCustomer == null)
            {
                GameObject customerObj = GameObject.FindAnyObjectByType<Customer>()?.gameObject;
                if (customerObj != null)
                {
                    testCustomer = customerObj.GetComponent<CustomerBehavior>();
                }
            }
            
            if (testCustomer == null)
            {
                LogError("No CustomerBehavior found for testing!");
                yield break;
            }
            
            LogTest($"Found test customer: {testCustomer.name}");
            LogTest($"  Base Spending Power: ${testCustomer.BaseSpendingPower:F2}");
            LogTest($"  Shopping Time: {testCustomer.ShoppingTime:F1}s");
            
            // Reset customer state for clean test
            testCustomer.ResetShoppingState();
            
            yield return new WaitForSeconds(0.5f);
        }
        
        /// <summary>
        /// Test product setup on shelves
        /// </summary>
        private IEnumerator TestProductSetup()
        {
            LogTest("Testing Product setup...");
            
            // Find test shelves if not assigned
            if (testShelves == null || testShelves.Length == 0)
            {
                testShelves = FindObjectsByType<ShelfSlot>(FindObjectsSortMode.None);
            }
            
            // Find test products if not assigned
            if (testProducts == null || testProducts.Length == 0)
            {
                testProducts = FindObjectsByType<Product>(FindObjectsSortMode.None);
            }
            
            LogTest($"Found {testShelves.Length} shelves and {testProducts.Length} products");
            
            // Log product availability
            int availableProducts = 0;
            foreach (var product in testProducts)
            {
                if (product != null && product.IsOnShelf && !product.IsPurchased)
                {
                    availableProducts++;
                    LogTest($"  Available: {product.ProductData?.ProductName ?? "Unknown"} - ${product.CurrentPrice}");
                }
            }
            
            LogTest($"Total available products for purchase: {availableProducts}");
            
            yield return new WaitForSeconds(0.5f);
        }
        
        /// <summary>
        /// Test customer shopping behavior
        /// </summary>
        private IEnumerator TestCustomerShopping()
        {
            LogTest("Testing Customer shopping behavior...");
            
            if (testCustomer == null)
            {
                LogError("No test customer available!");
                yield break;
            }
            
            // Record initial state
            int initialSelectedProducts = testCustomer.SelectedProducts.Count;
            float initialPurchaseAmount = testCustomer.TotalPurchaseAmount;
            
            LogTest($"Initial customer state: {initialSelectedProducts} products, ${initialPurchaseAmount:F2}");
            
            // Test product selection at current shelf
            if (testCustomer.TargetShelf != null)
            {
                LogTest($"Customer is at shelf: {testCustomer.TargetShelf.name}");
                
                // Simulate some shopping time
                yield return new WaitForSeconds(2f);
                
                // Check if customer selected any products
                LogTest($"After shopping: {testCustomer.SelectedProducts.Count} products, ${testCustomer.TotalPurchaseAmount:F2}");
            }
            else
            {
                LogTest("Customer has no target shelf - setting random destination");
                testCustomer.SetRandomShelfDestination();
                yield return new WaitForSeconds(1f);
            }
            
            yield return new WaitForSeconds(0.5f);
        }
        
        /// <summary>
        /// Test purchase processing through GameManager
        /// </summary>
        private IEnumerator TestPurchaseProcessing()
        {
            LogTest("Testing Purchase processing...");
            
            if (testCustomer == null)
            {
                LogError("No test customer available!");
                yield break;
            }
            
            // Record initial GameManager state
            var initialEconomicState = GameManager.Instance.GetEconomicStatus();
            
            LogTest($"Pre-purchase GameManager state:");
            LogTest($"  Money: ${initialEconomicState.money:F2}");
            LogTest($"  Customers Served: {initialEconomicState.customers}");
            LogTest($"  Daily Revenue: ${initialEconomicState.revenue:F2}");
            LogTest($"DEBUG: Initial state tuple = {initialEconomicState}");
            
            // Also directly check the property
            LogTest($"DEBUG: Direct property check - CustomersServedToday = {GameManager.Instance.CustomersServedToday}");
            
            // Simulate a manual purchase to test the integration
            if (testCustomer.SelectedProducts.Count == 0)
            {
                LogTest("Customer has no selected products - manually adding test purchase");
                
                // Find an available product and simulate purchase
                Product testProduct = FindAvailableTestProduct();
                if (testProduct != null)
                {
                    float testAmount = testProduct.CurrentPrice;
                    float testSatisfaction = 0.8f;
                    
                    LogTest($"Simulating purchase: ${testAmount:F2} with satisfaction {testSatisfaction:F2}");
                    
                    // Process purchase through GameManager
                    GameManager.Instance.ProcessCustomerPurchase(testAmount, testSatisfaction);
                    
                    // Mark product as purchased
                    testProduct.Purchase();
                }
            }
            else
            {
                LogTest($"Processing actual customer selections: {testCustomer.SelectedProducts.Count} products");
                
                // Process customer's actual selections
                float totalAmount = testCustomer.TotalPurchaseAmount;
                
                // Calculate satisfaction (simple version for testing)
                float satisfaction = testCustomer.SelectedProducts.Count > 0 ? 0.8f : 0.5f;
                
                LogTest($"Processing purchase: ${totalAmount:F2} with satisfaction {satisfaction:F2}");
                
                // Process through GameManager
                GameManager.Instance.ProcessCustomerPurchase(totalAmount, satisfaction);
                
                // Mark products as purchased
                foreach (var product in testCustomer.SelectedProducts)
                {
                    if (product != null)
                    {
                        product.Purchase();
                    }
                }
            }
            
            yield return new WaitForSeconds(1f);
            
            // Check final GameManager state
            var finalEconomicState = GameManager.Instance.GetEconomicStatus();
            
            LogTest($"Post-purchase GameManager state:");
            LogTest($"  Money: ${finalEconomicState.money:F2} (Change: +${finalEconomicState.money - initialEconomicState.money:F2})");
            LogTest($"  Customers Served: {finalEconomicState.customers} (Change: +{finalEconomicState.customers - initialEconomicState.customers})");
            LogTest($"  Daily Revenue: ${finalEconomicState.revenue:F2} (Change: +${finalEconomicState.revenue - initialEconomicState.revenue:F2})");
            LogTest($"  Reputation: {finalEconomicState.reputation:F1}");
            LogTest($"DEBUG: Final state tuple = {finalEconomicState}");
            
            // Also directly check the property
            LogTest($"DEBUG: Direct property check - CustomersServedToday = {GameManager.Instance.CustomersServedToday}");
            
            // Verify the integration worked
            bool moneyIncreased = finalEconomicState.money > initialEconomicState.money;
            bool customersIncreased = finalEconomicState.customers > initialEconomicState.customers;
            bool revenueIncreased = finalEconomicState.revenue > initialEconomicState.revenue;
            
            if (moneyIncreased && customersIncreased && revenueIncreased)
            {
                LogTest("✅ PURCHASE INTEGRATION TEST PASSED - All metrics increased correctly!");
            }
            else
            {
                LogError("❌ PURCHASE INTEGRATION TEST FAILED - Metrics did not increase as expected");
            }
        }
        
        /// <summary>
        /// Find an available product for testing
        /// </summary>
        private Product FindAvailableTestProduct()
        {
            foreach (var product in testProducts)
            {
                if (product != null && product.IsOnShelf && !product.IsPurchased)
                {
                    return product;
                }
            }
            
            // If no products found in array, search scene
            Product[] allProducts = FindObjectsByType<Product>(FindObjectsSortMode.None);
            foreach (var product in allProducts)
            {
                if (product != null && product.IsOnShelf && !product.IsPurchased)
                {
                    return product;
                }
            }
            
            return null;
        }
        
        /// <summary>
        /// Log test message if debug logging is enabled
        /// </summary>
        private void LogTest(string message)
        {
            if (enableDebugLogs)
            {
                Debug.Log($"[PurchaseIntegrationTest] {message}");
            }
        }
        
        /// <summary>
        /// Log error message
        /// </summary>
        private void LogError(string message)
        {
            Debug.LogError($"[PurchaseIntegrationTest] {message}");
        }
    }
}
