using UnityEngine;

namespace TabletopShop
{
    /// <summary>
    /// Main container for all customer behavior settings
    /// Provides centralized configuration for AI behavior across all Behavior Designer tasks
    /// </summary>
    [CreateAssetMenu(fileName = "CustomerBehaviorSettings", menuName = "TabletopShop/Customer Behavior Settings")]
    public class CustomerBehaviorSettings : ScriptableObject
    {
        [Header("Shopping Behavior")]
        [Tooltip("Settings related to customer shopping behavior")]
        public ShoppingSettings shopping = new ShoppingSettings();
        
        [Header("Checkout Behavior")]
        [Tooltip("Settings related to customer checkout behavior")]
        public CheckoutSettings checkout = new CheckoutSettings();
        
        [Header("Global Settings")]
        [Tooltip("Enable debug logging for all customer tasks")]
        public bool enableDebugLogging = true;
        
        [Tooltip("Global speed multiplier for all customer actions")]
        [Range(0.1f, 5f)]
        public float globalSpeedMultiplier = 1f;
        
        [Header("Store Integration")]
        [Tooltip("How customers react to store closing time")]
        public float storeClosingReactionTime = 30f; // minutes before close
        
        [Tooltip("Speed multiplier when store is closing soon")]
        [Range(1f, 3f)]
        public float closingTimeSpeedMultiplier = 1.5f;
        
        /// <summary>
        /// Validate settings and provide warnings for invalid configurations
        /// </summary>
        private void OnValidate()
        {
            // Ensure reasonable values
            shopping.buyProbability = Mathf.Clamp01(shopping.buyProbability);
            shopping.shelfSwitchProbability = Mathf.Clamp01(shopping.shelfSwitchProbability);
            globalSpeedMultiplier = Mathf.Max(0.1f, globalSpeedMultiplier);
            closingTimeSpeedMultiplier = Mathf.Max(1f, closingTimeSpeedMultiplier);
            
            // Ensure positive time values
            shopping.shoppingDuration = Mathf.Max(1f, shopping.shoppingDuration);
            checkout.maxQueueWaitTime = Mathf.Max(1f, checkout.maxQueueWaitTime);
            checkout.maxScanWaitTime = Mathf.Max(1f, checkout.maxScanWaitTime);
            checkout.progressCheckInterval = Mathf.Max(0.1f, checkout.progressCheckInterval);
            
            // Ensure reasonable product limits
            shopping.maxProducts = Mathf.Max(1, shopping.maxProducts);
        }
        
        /// <summary>
        /// Get a debug-friendly string representation of current settings
        /// </summary>
        /// <returns>Formatted settings summary</returns>
        public string GetSettingsSummary()
        {
            return $"CustomerBehaviorSettings Summary:\n" +
                   $"Shopping: Buy Probability={shopping.buyProbability:F2}, Duration={shopping.shoppingDuration}s, Max Products={shopping.maxProducts}\n" +
                   $"Checkout: Queue Wait={checkout.maxQueueWaitTime}s, Scan Wait={checkout.maxScanWaitTime}s\n" +
                   $"Global: Speed Multiplier={globalSpeedMultiplier:F1}, Debug Logging={enableDebugLogging}";
        }
    }
}