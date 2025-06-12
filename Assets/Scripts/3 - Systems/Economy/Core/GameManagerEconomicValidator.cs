using UnityEngine;

namespace TabletopShop
{
    /// <summary>
    /// GameManager-based implementation of IEconomicValidator.
    /// Delegates economic operations to GameManager.Instance while providing
    /// graceful fallback behavior when GameManager is unavailable.
    /// 
    /// This implementation maintains the exact same behavior as the original
    /// direct GameManager.Instance calls in InventoryManager.
    /// </summary>
    public class GameManagerEconomicValidator : IEconomicValidator
    {
        private bool logTransactions;
        
        /// <summary>
        /// Initialize the validator with optional transaction logging
        /// </summary>
        /// <param name="enableLogging">Whether to enable transaction logging</param>
        public GameManagerEconomicValidator(bool enableLogging = true)
        {
            logTransactions = enableLogging;
        }
        
        /// <summary>
        /// Check if the player has sufficient funds to afford a given cost
        /// </summary>
        /// <param name="cost">The cost to validate</param>
        /// <returns>True if the player can afford the cost, false otherwise</returns>
        public bool CanAffordCost(float cost)
        {
            // Null-safe GameManager access with graceful degradation
            if (GameManager.Instance == null)
            {
                if (logTransactions)
                {
                    Debug.LogWarning("GameManager not available for cost validation. Allowing operation.");
                }
                return true; // Graceful fallback when GameManager unavailable
            }
            
            return GameManager.Instance.HasSufficientFunds(cost);
        }
        
        /// <summary>
        /// Process an economic transaction by deducting the cost from available funds
        /// </summary>
        /// <param name="cost">The cost to deduct</param>
        /// <param name="description">Description of the transaction for logging/tracking</param>
        /// <returns>True if the transaction was successful, false if insufficient funds or other error</returns>
        public bool ProcessTransaction(float cost, string description)
        {
            // Null-safe GameManager access with graceful degradation
            if (GameManager.Instance == null)
            {
                if (logTransactions)
                {
                    Debug.LogWarning($"GameManager not available for transaction processing: {description}. Allowing operation without economic impact.");
                }
                return true; // Graceful fallback
            }
            
            // Process payment through GameManager
            bool success = GameManager.Instance.SubtractMoney(cost, description);
            
            if (success && logTransactions)
            {
                Debug.Log($"Processed transaction: {description} for ${cost:F2}");
            }
            else if (!success && logTransactions)
            {
                Debug.LogError($"Failed to process transaction: {description} for ${cost:F2}");
            }
            
            return success;
        }
        
        /// <summary>
        /// Calculate the total cost for restocking items
        /// </summary>
        /// <param name="quantity">Number of items to restock</param>
        /// <param name="basePrice">Base price per item</param>
        /// <param name="multiplier">Cost multiplier (e.g., 0.7 for 70% of retail price)</param>
        /// <returns>Total calculated restock cost</returns>
        public float CalculateRestockCost(int quantity, float basePrice, float multiplier)
        {
            if (quantity <= 0 || basePrice < 0 || multiplier < 0)
            {
                if (logTransactions)
                {
                    Debug.LogWarning($"Invalid restock cost calculation parameters: quantity={quantity}, basePrice={basePrice}, multiplier={multiplier}");
                }
                return 0f;
            }
            
            return basePrice * multiplier * quantity;
        }
        
        /// <summary>
        /// Get current available funds for display or validation purposes
        /// </summary>
        /// <returns>Current available money/funds</returns>
        public float GetAvailableFunds()
        {
            if (GameManager.Instance == null)
            {
                if (logTransactions)
                {
                    Debug.LogWarning("GameManager not available for funds query. Returning 0.");
                }
                return 0f;
            }
            
            return GameManager.Instance.CurrentMoney;
        }
        
        /// <summary>
        /// Check if the economic validator is available and functional
        /// </summary>
        /// <returns>True if validator is ready to process transactions</returns>
        public bool IsAvailable()
        {
            return GameManager.Instance != null;
        }
        
        /// <summary>
        /// Enable or disable transaction logging
        /// </summary>
        /// <param name="enabled">Whether to enable transaction logging</param>
        public void SetLogging(bool enabled)
        {
            logTransactions = enabled;
            if (logTransactions)
            {
                Debug.Log("GameManagerEconomicValidator transaction logging enabled");
            }
        }
        
        /// <summary>
        /// Get current economic status for debugging
        /// </summary>
        /// <returns>String representation of economic status</returns>
        public string GetEconomicStatus()
        {
            if (GameManager.Instance == null)
            {
                return "Economic Status: GameManager unavailable";
            }
            
            return $"Economic Status: Available Funds=${GetAvailableFunds():F2}, GameManager Available={IsAvailable()}";
        }
    }
}
