using UnityEngine;

namespace TabletopShop
{
    /// <summary>
    /// Handles all economic operations and GameManager integration for products
    /// Manages purchase processing, price validation, and transaction logging
    /// </summary>
    public class ProductEconomics : MonoBehaviour
    {
        // Events for component communication
        public System.Action OnPurchaseProcessed;
        public System.Action<float> OnPriceChanged;
        
        // Cached component reference
        private Product productComponent;
        
        #region Initialization
        
        private void Start()
        {
            // Get Product component reference after all components are initialized
            productComponent = GetComponent<Product>();
            if (productComponent == null)
            {
                Debug.LogError($"ProductEconomics on {name} requires a Product component!", this);
            }
        }
        
        #endregion
        
        #region Public Economic Methods
        
        /// <summary>
        /// Process a product purchase with full economic validation and GameManager integration
        /// </summary>
        /// <returns>True if purchase was successfully processed</returns>
        public bool ProcessPurchase()
        {
            if (productComponent == null)
            {
                Debug.LogError("Cannot process purchase - Product component not found!");
                return false;
            }
            
            if (productComponent.IsPurchased)
            {
                Debug.LogWarning($"Product {productComponent.ProductData?.ProductName ?? productComponent.name} is already purchased!");
                return false;
            }
            
            if (!productComponent.IsOnShelf)
            {
                Debug.LogWarning($"Cannot purchase {productComponent.ProductData?.ProductName ?? productComponent.name} - not on shelf!");
                return false;
            }
            
            // Validate economic transaction
            if (!ValidateEconomicTransaction())
            {
                Debug.LogWarning($"Economic validation failed for {productComponent.ProductData?.ProductName ?? productComponent.name}");
                return false;
            }
            
            // Process through GameManager if available
            bool gameManagerSuccess = ProcessGameManagerTransaction();
            
            // Fire purchase processed event
            OnPurchaseProcessed?.Invoke();
            
            Debug.Log($"PURCHASED: {productComponent.ProductData?.ProductName ?? productComponent.name} for ${productComponent.CurrentPrice}!");
            
            return gameManagerSuccess;
        }
        
        /// <summary>
        /// Validate if a price is acceptable for this product
        /// </summary>
        /// <param name="newPrice">The price to validate</param>
        /// <returns>True if price is valid</returns>
        public bool ValidatePrice(float newPrice)
        {
            if (newPrice < 0)
            {
                Debug.LogWarning($"Cannot set negative price for {productComponent?.ProductData?.ProductName ?? productComponent?.name}");
                return false;
            }
            
            // Additional economic validation could go here
            // e.g., maximum price limits, profit margin validation, etc.
            
            return true;
        }
        
        /// <summary>
        /// Update the product price with validation
        /// </summary>
        /// <param name="newPrice">The new price to set</param>
        /// <returns>True if price was successfully updated</returns>
        public bool UpdatePrice(float newPrice)
        {
            if (!ValidatePrice(newPrice))
            {
                return false;
            }
            
            float oldPrice = productComponent.CurrentPrice;
            
            // The price update will be handled by the main Product component
            // This method provides economic validation and event firing
            
            Debug.Log($"Price changed for {productComponent.ProductData?.ProductName ?? productComponent.name}: ${oldPrice} → ${newPrice}");
            
            // Fire price changed event
            OnPriceChanged?.Invoke(newPrice);
            
            return true;
        }
        
        /// <summary>
        /// Calculate the profit margin for this product
        /// </summary>
        /// <returns>Profit margin as a percentage (0-100)</returns>
        public float CalculateProfitMargin()
        {
            if (productComponent?.ProductData == null)
                return 0f;
            
            float costPrice = productComponent.ProductData.CostPrice;
            float sellPrice = productComponent.CurrentPrice;
            
            if (costPrice <= 0)
                return 100f; // No cost data available
            
            return ((sellPrice - costPrice) / sellPrice) * 100f;
        }
        
        /// <summary>
        /// Get economic status information for this product
        /// </summary>
        /// <returns>Economic status string</returns>
        public string GetEconomicStatus()
        {
            if (productComponent?.ProductData == null)
                return "No economic data available";
            
            float profitMargin = CalculateProfitMargin();
            float basePrice = productComponent.ProductData.BasePrice;
            float currentPrice = productComponent.CurrentPrice;
            
            string priceComparison = currentPrice > basePrice ? "Above base" : 
                                   currentPrice < basePrice ? "Below base" : "At base price";
            
            return $"Price: ${currentPrice:F2} ({priceComparison}), Profit: {profitMargin:F1}%";
        }
        
        #endregion
        
        #region Private Economic Logic
        
        /// <summary>
        /// Validate economic transaction through GameManager
        /// Performs null-safe checks and basic economic validation
        /// </summary>
        /// <returns>True if economic validation passes</returns>
        private bool ValidateEconomicTransaction()
        {
            // Null-safe GameManager access with graceful degradation
            if (GameManager.Instance == null)
            {
                Debug.LogWarning($"GameManager not available for economic validation of {productComponent?.ProductData?.ProductName ?? productComponent?.name} purchase");
                return false; // Validation failed, will trigger fallback behavior
            }
            
            // Basic transaction validation
            if (productComponent.CurrentPrice <= 0)
            {
                Debug.LogWarning($"Invalid price for {productComponent.ProductData?.ProductName ?? productComponent.name}: ${productComponent.CurrentPrice}");
                return false;
            }
            
            // Log transaction attempt for economic tracking
            Debug.Log($"Economic validation: Player attempting to purchase {productComponent.ProductData?.ProductName ?? productComponent.name} for ${productComponent.CurrentPrice}");
            
            // Note: For player purchases, we don't check shop funds since this is player-to-shop transaction
            // Future expansion: Could add inventory purchasing costs or other economic constraints here
            
            return true; // Basic validation passed
        }
        
        /// <summary>
        /// Process the purchase transaction through GameManager
        /// Handles the complete purchase flow with transaction logging
        /// </summary>
        /// <returns>True if GameManager integration was successful</returns>
        private bool ProcessGameManagerTransaction()
        {
            // Integrate with GameManager for economic tracking if available
            if (GameManager.Instance != null)
            {
                // Process player purchase through economic system
                // This tracks the transaction as revenue for the shop
                GameManager.Instance.ProcessCustomerPurchase(productComponent.CurrentPrice, 1.0f); // Perfect satisfaction for player purchases
                
                Debug.Log($"Economic integration: Processed player purchase of ${productComponent.CurrentPrice} through GameManager");
                return true;
            }
            else
            {
                Debug.LogWarning("GameManager unavailable - purchase processed without economic integration");
                return false;
            }
        }
        
        #endregion
        
        #region Testing & Validation
        
        /// <summary>
        /// Test the GameManager economic integration (for development/testing)
        /// </summary>
        [ContextMenu("Test Economic Integration")]
        public void TestEconomicIntegration()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("Economic integration test requires Play mode");
                return;
            }
            
            Debug.Log("=== TESTING PRODUCT ECONOMIC INTEGRATION ===");
            Debug.Log($"Product: {productComponent?.ProductData?.ProductName ?? productComponent?.name} (${productComponent?.CurrentPrice})");
            Debug.Log($"Is on shelf: {productComponent?.IsOnShelf}, Is purchased: {productComponent?.IsPurchased}");
            Debug.Log($"Economic Status: {GetEconomicStatus()}");
            
            if (GameManager.Instance != null)
            {
                var economicStatus = GameManager.Instance.GetEconomicStatus();
                Debug.Log($"GameManager state - Money: ${economicStatus.money:F2}, Customers: {economicStatus.customers}");
                
                // Test economic validation
                bool validationResult = ValidateEconomicTransaction();
                Debug.Log($"Economic validation result: {validationResult}");
                
                if (validationResult && productComponent != null && !productComponent.IsPurchased && productComponent.IsOnShelf)
                {
                    Debug.Log("Simulating purchase...");
                    
                    // Store initial state for comparison
                    float initialMoney = economicStatus.money;
                    int initialCustomers = economicStatus.customers;
                    
                    // Simulate purchase
                    bool purchaseResult = ProcessPurchase();
                    
                    // Check results
                    var newEconomicStatus = GameManager.Instance.GetEconomicStatus();
                    Debug.Log($"Purchase result: {purchaseResult}");
                    Debug.Log($"After purchase - Money: ${newEconomicStatus.money:F2} (+${newEconomicStatus.money - initialMoney:F2})");
                    Debug.Log($"After purchase - Customers: {newEconomicStatus.customers} (+{newEconomicStatus.customers - initialCustomers})");
                    
                    Debug.Log("✅ Economic integration test completed!");
                }
                else
                {
                    Debug.LogWarning("Cannot test purchase - validation failed or product already purchased/not on shelf");
                }
            }
            else
            {
                Debug.LogError("GameManager.Instance is null - integration test failed");
            }
            
            Debug.Log("=== END ECONOMIC INTEGRATION TEST ===");
        }
        
        #endregion
    }
}
