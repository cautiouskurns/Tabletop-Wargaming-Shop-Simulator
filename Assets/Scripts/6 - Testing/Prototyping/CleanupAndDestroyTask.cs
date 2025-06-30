using UnityEngine;
using Opsive.BehaviorDesigner.Runtime.Tasks.Actions;
using Opsive.BehaviorDesigner.Runtime.Tasks;

namespace TabletopShop
{
    public class CleanupAndDestroyTask : Action
    {
        [Header("Settings Override (Optional)")]
        [Tooltip("Leave null to use global settings from CustomerBehaviorSettingsManager")]
        public CustomerBehaviorSettings settingsOverride;
        
        /// <summary>
        /// Get the checkout settings to use (either override or global)
        /// </summary>
        private CheckoutSettings GetCheckoutSettings()
        {
            if (settingsOverride != null && settingsOverride.checkout != null)
                return settingsOverride.checkout;
            
            return CustomerBehaviorSettingsManager.Checkout;
        }
        
        public override TaskStatus OnUpdate()
        {
            Customer customer = GetComponent<Customer>();
            if (customer == null)
                return TaskStatus.Failure;
            
            // Cleanup logic
            customer.CleanupOnDestroy();
            
            // Destroy after delay using settings
            var checkoutSettings = GetCheckoutSettings();
            float destroyDelay = checkoutSettings?.destroyDelay ?? 1f;
            Object.Destroy(customer.gameObject, destroyDelay);
            
            if (customer.showDebugLogs)
                Debug.Log("[CleanupAndDestroyTask] ✅ Customer cleanup complete");
            
            return TaskStatus.Success;
        }
    }
}