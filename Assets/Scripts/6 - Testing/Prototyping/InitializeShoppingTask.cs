using UnityEngine;
using Opsive.BehaviorDesigner.Runtime.Tasks.Actions;
using Opsive.BehaviorDesigner.Runtime.Tasks;

namespace TabletopShop
{
    public class InitializeShoppingTask : Action
    {
        [Header("Settings Override (Optional)")]
        [Tooltip("Leave null to use global settings from CustomerBehaviorSettingsManager")]
        public CustomerBehaviorSettings settingsOverride;
        
        /// <summary>
        /// Get the shopping settings to use (either override or global)
        /// </summary>
        private ShoppingSettings GetShoppingSettings()
        {
            if (settingsOverride != null && settingsOverride.shopping != null)
                return settingsOverride.shopping;
            
            return CustomerBehaviorSettingsManager.Shopping;
        }
        
        public override TaskStatus OnUpdate()
        {
            Customer customer = GetComponent<Customer>();
            if (customer == null)
            {
                Debug.LogError("[InitializeShoppingTask] No CustomerData component found!");
                return TaskStatus.Failure;
            }
            
            // Start the shopping timer with settings
            var shoppingSettings = GetShoppingSettings();
            customer.StartShoppingTimer();
            
            if (customer.showDebugLogs)
                Debug.Log("[InitializeShoppingTask] ✅ Shopping timer started");
            
            return TaskStatus.Success;
        }
    }
}