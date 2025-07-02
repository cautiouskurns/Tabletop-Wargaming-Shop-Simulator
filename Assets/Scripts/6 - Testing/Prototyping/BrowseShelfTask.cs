using UnityEngine;
using Opsive.BehaviorDesigner.Runtime.Tasks.Actions;
using Opsive.BehaviorDesigner.Runtime.Tasks;

namespace TabletopShop
{
    /// <summary>
    /// Behavior Designer task for browsing at a shelf before selecting products
    /// Uses the shelfBrowseTime setting to determine how long to browse
    /// </summary>
    public class BrowseShelfTask : Action
    {
        [Header("Settings Override (Optional)")]
        [Tooltip("Leave null to use global settings from CustomerBehaviorSettingsManager")]
        public CustomerBehaviorSettings settingsOverride;
        
        private float browseStartTime = 0f;
        private bool isBrowsing = false;
        
        /// <summary>
        /// Get the shopping settings to use (either override or global)
        /// </summary>
        private ShoppingSettings GetShoppingSettings()
        {
            if (settingsOverride != null && settingsOverride.shopping != null)
                return settingsOverride.shopping;
            
            return CustomerBehaviorSettingsManager.Shopping;
        }
        
        public override void OnStart()
        {
            Customer customer = GetComponent<Customer>();
            if (customer == null)
            {
                Debug.LogError("[BrowseShelfTask] Customer component not found!");
                return;
            }
            
            browseStartTime = Time.time;
            isBrowsing = true;
            
            if (customer.showDebugLogs)
            {
                var shoppingSettings = GetShoppingSettings();
                float browseTime = shoppingSettings?.shelfBrowseTime ?? 3f;
                Debug.Log($"[BrowseShelfTask] {customer.name}: Started browsing shelf for {browseTime}s");
            }
        }
        
        public override TaskStatus OnUpdate()
        {
            if (!isBrowsing)
                return TaskStatus.Failure;
            
            Customer customer = GetComponent<Customer>();
            if (customer == null)
                return TaskStatus.Failure;
            
            // Get browse time from settings
            var shoppingSettings = GetShoppingSettings();
            float browseTime = shoppingSettings?.shelfBrowseTime ?? 3f;
            
            // Check if browse time has elapsed
            float elapsedTime = Time.time - browseStartTime;
            if (elapsedTime >= browseTime)
            {
                if (customer.showDebugLogs)
                    Debug.Log($"[BrowseShelfTask] ✅ {customer.name}: Finished browsing after {elapsedTime:F1}s");
                return TaskStatus.Success;
            }
            
            // Still browsing
            return TaskStatus.Running;
        }
        
        public override void OnEnd()
        {
            isBrowsing = false;
            
            Customer customer = GetComponent<Customer>();
            if (customer != null && customer.showDebugLogs)
            {
                float totalBrowseTime = Time.time - browseStartTime;
                Debug.Log($"[BrowseShelfTask] {customer.name}: Browse task ended after {totalBrowseTime:F1}s");
            }
        }
    }
}