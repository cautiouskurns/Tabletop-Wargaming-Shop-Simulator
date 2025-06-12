using UnityEngine;
using System.Linq;

namespace TabletopShop
{
    /// <summary>
    /// Development and debugging utility for InventoryManager
    /// Contains all testing and debugging methods extracted from InventoryManager
    /// to keep the main class focused on core inventory operations
    /// </summary>
    public class InventoryManagerDebugger : MonoBehaviour
    {
        [Header("Debug Configuration")]
        [SerializeField] private bool enableDebugLogs = true;
        [SerializeField] private bool enableEconomicTestLogs = true;
        
        private InventoryManager inventoryManager;
        
        /// <summary>
        /// Get reference to InventoryManager instance
        /// </summary>
        private InventoryManager Inventory
        {
            get
            {
                if (inventoryManager == null)
                {
                    inventoryManager = InventoryManager.Instance;
                }
                return inventoryManager;
            }
        }
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            // Ensure we have a reference to InventoryManager
            if (inventoryManager == null)
            {
                inventoryManager = InventoryManager.Instance;
            }
        }
        
        #endregion
        
        #region Economic Testing Methods
        
        /// <summary>
        /// Test economic integration with GameManager
        /// </summary>
        [ContextMenu("Test Economic Integration")]
        public void TestEconomicIntegration()
        {
            if (Inventory == null)
            {
                Debug.LogError("InventoryManager not available for testing");
                return;
            }
            
            Debug.Log("=== INVENTORY ECONOMIC INTEGRATION TEST ===");
            
            // Test economic validator availability - using public method
            bool canAffordTest = Inventory.HasSufficientFundsForRestock(
                Inventory.AvailableProducts.FirstOrDefault(p => p != null), 1);
            Debug.Log($"Economic validation test completed: {canAffordTest}");
            
            // Test economic configuration
            Debug.Log(Inventory.GetEconomicConfiguration());
            
            // Test restock cost calculations
            if (Inventory.AvailableProducts.Count > 0 && Inventory.AvailableProducts[0] != null)
            {
                var testProduct = Inventory.AvailableProducts[0];
                float restockCost = Inventory.CalculateRestockCost(testProduct, 5);
                bool canAfford = Inventory.HasSufficientFundsForRestock(testProduct, 5);
                
                Debug.Log($"Test Product: {testProduct.ProductName}");
                Debug.Log($"Retail Price: ${testProduct.BasePrice}");
                Debug.Log($"Restock Cost (5 units): ${restockCost:F2}");
                Debug.Log($"Can Afford Restock: {canAfford}");
            }
            
            Debug.Log("=== INTEGRATION TEST COMPLETE ===");
        }
        
        /// <summary>
        /// Test adding products with economic constraints
        /// </summary>
        [ContextMenu("Test Economic Product Addition")]
        public void TestEconomicProductAddition()
        {
            if (Inventory == null)
            {
                Debug.LogError("InventoryManager not available for testing");
                return;
            }
            
            Debug.Log("=== TESTING ECONOMIC PRODUCT ADDITION ===");
            
            if (Inventory.AvailableProducts.Count == 0 || Inventory.AvailableProducts[0] == null)
            {
                Debug.LogWarning("No products available for testing");
                return;
            }
            
            var testProduct = Inventory.AvailableProducts[0];
            float restockCost = Inventory.CalculateRestockCost(testProduct, 1);
            
            Debug.Log($"Testing addition of 1 x {testProduct.ProductName}");
            Debug.Log($"Calculated restock cost: ${restockCost:F2}");
            
            // Test with calculated cost
            bool success = Inventory.AddProduct(testProduct, 1, true, restockCost);
            Debug.Log($"Economic addition result: {(success ? "SUCCESS" : "FAILED")}");
            
            // Test without cost (legacy behavior)
            bool legacySuccess = Inventory.AddProduct(testProduct, 1, true, null);
            Debug.Log($"Legacy addition result: {(legacySuccess ? "SUCCESS" : "FAILED")}");
            
            Debug.Log("=== ECONOMIC ADDITION TEST COMPLETE ===");
        }
        
        /// <summary>
        /// Simulate inventory restocking with economic validation
        /// </summary>
        [ContextMenu("Simulate Inventory Restocking")]
        public void SimulateInventoryRestocking()
        {
            if (Inventory == null)
            {
                Debug.LogError("InventoryManager not available for testing");
                return;
            }
            
            Debug.Log("=== SIMULATING INVENTORY RESTOCKING ===");
            
            // Enable economic constraints for this test
            Inventory.SetEconomicConstraints(true);
            Debug.Log("Economic constraints enabled for restocking simulation");
            
            foreach (var product in Inventory.AvailableProducts.Where(p => p != null))
            {
                int currentStock = Inventory.GetProductCount(product);
                int restockAmount = 10 - currentStock; // Restock to 10 units
                
                if (restockAmount > 0)
                {
                    float cost = Inventory.CalculateRestockCost(product, restockAmount);
                    bool canAfford = Inventory.HasSufficientFundsForRestock(product, restockAmount);
                    
                    Debug.Log($"{product.ProductName}: Stock={currentStock}, Need={restockAmount}, Cost=${cost:F2}, CanAfford={canAfford}");
                    
                    if (canAfford)
                    {
                        bool success = Inventory.AddProduct(product, restockAmount, true, cost);
                        Debug.Log($"  Restocking result: {(success ? "SUCCESS" : "FAILED")}");
                    }
                    else
                    {
                        Debug.Log($"  Insufficient funds for restocking");
                    }
                }
                else
                {
                    Debug.Log($"{product.ProductName}: Stock={currentStock} - No restocking needed");
                }
            }
            
            Debug.Log("=== RESTOCKING SIMULATION COMPLETE ===");
        }
        
        #endregion
        
        #region Inventory State Testing Methods
        
        /// <summary>
        /// Reset inventory to initial state
        /// </summary>
        [ContextMenu("Reset Inventory")]
        public void ResetInventory()
        {
            if (Inventory == null)
            {
                Debug.LogError("InventoryManager not available for reset");
                return;
            }
            
            // Clear inventory through available public methods
            var productsToRemove = Inventory.AvailableProducts.Where(p => p != null && Inventory.HasProduct(p)).ToList();
            
            foreach (var product in productsToRemove)
            {
                int currentCount = Inventory.GetProductCount(product);
                if (currentCount > 0)
                {
                    Inventory.RemoveProduct(product, currentCount);
                }
            }
            
            // Clear selection
            Inventory.ClearSelection();
            
            // Reinitialize with starting inventory - simulate this by adding starting quantities
            foreach (var product in Inventory.AvailableProducts.Where(p => p != null))
            {
                Inventory.AddProduct(product, 5, true); // Default starting quantity
            }
            
            // Select first available product
            if (Inventory.AvailableProducts.Count > 0 && Inventory.AvailableProducts[0] != null)
            {
                Inventory.SelectProduct(Inventory.AvailableProducts[0]);
            }
            
            Debug.Log("Inventory has been reset to initial state.");
        }
        
        /// <summary>
        /// Add test products for development
        /// </summary>
        [ContextMenu("Add Test Products")]
        public void AddTestProducts()
        {
            if (Inventory == null)
            {
                Debug.LogError("InventoryManager not available for adding test products");
                return;
            }
            
            foreach (var product in Inventory.AvailableProducts.Where(p => p != null))
            {
                // Use legacy method without economic constraints for testing
                Inventory.AddProduct(product, 10, true);
            }
            
            Debug.Log("Added 10 of each available product for testing.");
        }
        
        /// <summary>
        /// Force reload and reinitialize the inventory system
        /// </summary>
        [ContextMenu("Force Reload Inventory")]
        public void ForceReloadInventory()
        {
            if (Inventory == null)
            {
                Debug.LogError("InventoryManager not available for force reload");
                return;
            }
            
            Debug.Log("=== FORCE RELOADING INVENTORY ===");
            
            // Clear current inventory
            ResetInventory();
            
            Debug.Log("=== INVENTORY RELOAD COMPLETE ===");
        }
        
        #endregion
        
        #region Debug Information Methods
        
        /// <summary>
        /// Debug current inventory state
        /// </summary>
        [ContextMenu("Debug Inventory State")]
        public void DebugInventoryState()
        {
            if (Inventory == null)
            {
                Debug.LogError("InventoryManager not available for debugging");
                return;
            }
            
            Debug.Log("=== INVENTORY DEBUG INFO ===");
            Debug.Log($"Available Products List Count: {Inventory.AvailableProducts.Count}");
            Debug.Log($"Total Product Count: {Inventory.TotalProductCount}");
            Debug.Log($"Unique Product Count: {Inventory.UniqueProductCount}");
            Debug.Log($"Selected Product: {Inventory.SelectedProduct?.ProductName ?? "None"}");
            
            Debug.Log("Available Products Details:");
            for (int i = 0; i < Inventory.AvailableProducts.Count; i++)
            {
                if (Inventory.AvailableProducts[i] != null)
                {
                    Debug.Log($"  [{i}] {Inventory.AvailableProducts[i].ProductName} - Count: {Inventory.GetProductCount(Inventory.AvailableProducts[i])}");
                }
                else
                {
                    Debug.Log($"  [{i}] NULL PRODUCT");
                }
            }
            
            // Show inventory status
            Debug.Log("\n" + Inventory.GetInventoryStatus());
            
            Debug.Log("=== END DEBUG INFO ===");
        }
        
        #endregion
        
        #region Advanced Testing Methods
        
        /// <summary>
        /// Test the refactored AddProduct method with various scenarios
        /// </summary>
        [ContextMenu("Test Refactored AddProduct")]
        public void TestRefactoredAddProduct()
        {
            if (Inventory == null)
            {
                Debug.LogError("InventoryManager not available for testing");
                return;
            }
            
            Debug.Log("=== TESTING REFACTORED ADDPRODUCT METHOD ===");
            
            if (Inventory.AvailableProducts.Count == 0 || Inventory.AvailableProducts[0] == null)
            {
                Debug.LogWarning("No products available for testing");
                return;
            }
            
            var testProduct = Inventory.AvailableProducts[0];
            int initialCount = Inventory.GetProductCount(testProduct);
            
            // Test 1: Valid addition without cost
            Debug.Log("Test 1: Valid addition without cost");
            bool result1 = Inventory.AddProduct(testProduct, 5, true, null);
            Debug.Log($"Result: {result1}, New Count: {Inventory.GetProductCount(testProduct)}");
            
            // Test 2: Valid addition with cost
            Debug.Log("Test 2: Valid addition with cost");
            float testCost = Inventory.CalculateRestockCost(testProduct, 1);
            bool result2 = Inventory.AddProduct(testProduct, 1, true, testCost);
            Debug.Log($"Result: {result2}, New Count: {Inventory.GetProductCount(testProduct)}");
            
            // Test 3: Invalid addition (null product)
            Debug.Log("Test 3: Invalid addition (null product)");
            bool result3 = Inventory.AddProduct(null, 1, true, null);
            Debug.Log($"Result: {result3} (should be false)");
            
            // Test 4: Invalid addition (negative amount)
            Debug.Log("Test 4: Invalid addition (negative amount)");
            bool result4 = Inventory.AddProduct(testProduct, -1, true, null);
            Debug.Log($"Result: {result4} (should be false)");
            
            // Test 5: Addition without triggering events
            Debug.Log("Test 5: Addition without triggering events");
            int countBeforeEvents = Inventory.GetProductCount(testProduct);
            bool result5 = Inventory.AddProduct(testProduct, 2, false, null);
            Debug.Log($"Result: {result5}, Count change: {countBeforeEvents} -> {Inventory.GetProductCount(testProduct)}");
            
            Debug.Log("=== REFACTORED ADDPRODUCT TESTING COMPLETE ===");
        }
        
        /// <summary>
        /// Test event publisher integration
        /// </summary>
        [ContextMenu("Test Event Publisher Integration")]
        public void TestEventPublisherIntegration()
        {
            if (Inventory == null)
            {
                Debug.LogError("InventoryManager not available for testing");
                return;
            }
            
            Debug.Log("=== TESTING EVENT PUBLISHER INTEGRATION ===");
            
            // Test event publisher configuration
            Debug.Log(Inventory.GetEventPublisherConfiguration());
            
            // Test events through normal operations
            Debug.Log("Testing events through normal operations...");
            if (Inventory.AvailableProducts.Count > 0 && Inventory.AvailableProducts[0] != null)
            {
                var testProduct = Inventory.AvailableProducts[0];
                int originalCount = Inventory.GetProductCount(testProduct);
                
                // Test adding product (should trigger events)
                Inventory.AddProduct(testProduct, 1, true, null);
                
                // Test selecting product (should trigger events)
                Inventory.SelectProduct(testProduct);
                
                Debug.Log($"Event publisher integration test completed. Product count: {originalCount} -> {Inventory.GetProductCount(testProduct)}");
            }
            
            Debug.Log("=== EVENT PUBLISHER INTEGRATION TEST COMPLETE ===");
        }
        
        /// <summary>
        /// Comprehensive inventory validation test
        /// </summary>
        [ContextMenu("Validate Inventory System")]
        public void ValidateInventorySystem()
        {
            if (Inventory == null)
            {
                Debug.LogError("InventoryManager not available for validation");
                return;
            }
            
            Debug.Log("=== COMPREHENSIVE INVENTORY VALIDATION ===");
            
            // Test inventory state validation
            bool isValid = Inventory.ValidateInventory();
            Debug.Log($"Inventory Validation Result: {(isValid ? "PASSED" : "FAILED")}");
            
            // Test economic configuration
            Debug.Log("\nEconomic Configuration:");
            Debug.Log(Inventory.GetEconomicConfiguration());
            
            // Test event publisher configuration
            Debug.Log("\nEvent Publisher Configuration:");
            Debug.Log(Inventory.GetEventPublisherConfiguration());
            
            // Test basic operations
            Debug.Log("\nTesting Basic Operations:");
            if (Inventory.AvailableProducts.Count > 0 && Inventory.AvailableProducts[0] != null)
            {
                var testProduct = Inventory.AvailableProducts[0];
                int currentCount = Inventory.GetProductCount(testProduct);
                
                Debug.Log($"Test Product: {testProduct.ProductName}");
                Debug.Log($"Current Count: {currentCount}");
                Debug.Log($"Has Product: {Inventory.HasProduct(testProduct)}");
                
                // Test selection
                bool selected = Inventory.SelectProduct(testProduct);
                Debug.Log($"Selection Test: {(selected ? "SUCCESS" : "FAILED")}");
                Debug.Log($"Selected Product: {Inventory.SelectedProduct?.ProductName ?? "None"}");
            }
            
            Debug.Log("=== VALIDATION COMPLETE ===");
        }
        
        #endregion
        
        #region Utility Methods
        
        /// <summary>
        /// Get debugging status information
        /// </summary>
        [ContextMenu("Show Debug Configuration")]
        public void ShowDebugConfiguration()
        {
            Debug.Log("=== INVENTORY DEBUGGER CONFIGURATION ===");
            Debug.Log($"Debug Logs Enabled: {enableDebugLogs}");
            Debug.Log($"Economic Test Logs Enabled: {enableEconomicTestLogs}");
            Debug.Log($"InventoryManager Reference: {(Inventory != null ? "Available" : "Missing")}");
            
            if (Inventory != null)
            {
                Debug.Log($"InventoryManager Instance: {Inventory.name}");
                Debug.Log($"Total Products: {Inventory.TotalProductCount}");
                Debug.Log($"Available Product Types: {Inventory.AvailableProducts.Count}");
            }
            
            Debug.Log("=== CONFIGURATION COMPLETE ===");
        }
        
        /// <summary>
        /// Quick test of all major functionality
        /// </summary>
        [ContextMenu("Run All Tests")]
        public void RunAllTests()
        {
            Debug.Log("=== RUNNING ALL INVENTORY TESTS ===");
            
            // Run validation first
            ValidateInventorySystem();
            
            // Test economic integration
            TestEconomicIntegration();
            
            // Test event publisher
            TestEventPublisherIntegration();
            
            // Test AddProduct variations
            TestRefactoredAddProduct();
            
            Debug.Log("=== ALL TESTS COMPLETE ===");
        }
        
        #endregion
    }
}
