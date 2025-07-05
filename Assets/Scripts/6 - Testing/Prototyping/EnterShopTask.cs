using UnityEngine;
using Opsive.BehaviorDesigner.Runtime.Tasks.Actions;
using Opsive.BehaviorDesigner.Runtime.Tasks;

namespace TabletopShop
{
    /// <summary>
    /// Behavior Designer task for ensuring customer enters the shop
    /// Moves customer to shop center if they're outside the boundary
    /// Prevents customers from leaving the shop until they complete shopping
    /// </summary>
    public class EnterShopTask : Action
    {
        [Header("Settings Override (Optional)")]
        [Tooltip("Leave null to use global settings from CustomerBehaviorSettingsManager")]
        public CustomerBehaviorSettings settingsOverride;
        
        private bool isMovingToShop = false;
        private bool hasEnteredShop = false;
        
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
                Debug.LogError("[EnterShopTask] Customer component not found!");
                return;
            }
            
            // If no shop boundary is defined, assume customer is already "in shop"
            if (ShopBoundary.Instance == null)
            {
                hasEnteredShop = true;
                if (customer.showDebugLogs)
                    Debug.Log($"[EnterShopTask] {customer.name}: No shop boundary - assuming already in shop");
                return;
            }
            
            // Check if customer is already in the shop
            if (IsCustomerInShop(customer))
            {
                hasEnteredShop = true;
                if (customer.showDebugLogs)
                    Debug.Log($"[EnterShopTask] {customer.name}: Already inside shop");
            }
            else
            {
                // Customer is outside, move them to shop center
                MoveCustomerToShop(customer);
                if (customer.showDebugLogs)
                    Debug.Log($"[EnterShopTask] {customer.name}: Moving to enter shop");
            }
        }
        
        public override TaskStatus OnUpdate()
        {
            Customer customer = GetComponent<Customer>();
            if (customer == null)
                return TaskStatus.Failure;
            
            // If already entered, we're done
            if (hasEnteredShop)
                return TaskStatus.Success;
            
            // Check if customer has entered the shop
            bool inShop = IsCustomerInShop(customer);
            if (customer.showDebugLogs)
//                Debug.Log($"[EnterShopTask] {customer.name}: In shop check: {inShop}, Position: {customer.transform.position}");
            
            if (inShop)
            {
                hasEnteredShop = true;
                if (customer.showDebugLogs)
                    Debug.Log($"[EnterShopTask] ✅ {customer.name}: Successfully entered shop");
                return TaskStatus.Success;
            }
            
            // Still moving to shop
            if (isMovingToShop)
            {
                // Check if movement is complete
                if (customer.Movement != null && customer.Movement.HasReachedDestination())
                {
                    if (customer.showDebugLogs)
                        Debug.Log($"[EnterShopTask] {customer.name}: Reached destination, forcing success");
                    
                    // Force success if we reached the destination (assume we're close enough to shop)
                    hasEnteredShop = true;
                    return TaskStatus.Success;
                }
                return TaskStatus.Running;
            }
            
            // If not moving and not in shop, something went wrong - force success to avoid blocking
            if (customer.showDebugLogs)
                Debug.Log($"[EnterShopTask] {customer.name}: Not moving and not detected in shop - forcing success to continue");
            
            hasEnteredShop = true;
            return TaskStatus.Success;
        }
        
        public override void OnEnd()
        {
            Customer customer = GetComponent<Customer>();
            if (customer != null && customer.showDebugLogs)
            {
                Debug.Log($"[EnterShopTask] {customer.name}: Enter shop task ended - In shop: {hasEnteredShop}");
            }
        }
        
        /// <summary>
        /// Check if customer is currently inside the shop boundary
        /// </summary>
        /// <param name="customer">Customer to check</param>
        /// <returns>True if customer is inside shop</returns>
        private bool IsCustomerInShop(Customer customer)
        {
            if (ShopBoundary.Instance == null)
            {
                // No shop boundary defined, assume customer is "in shop"
                return true;
            }
            
            return ShopBoundary.Instance.IsObjectInShop(customer.gameObject);
        }
        
        /// <summary>
        /// Move customer to the shop center
        /// </summary>
        /// <param name="customer">Customer to move</param>
        private void MoveCustomerToShop(Customer customer)
        {
            if (customer.Movement == null)
            {
                Debug.LogError($"[EnterShopTask] {customer.name}: No movement component found!");
                return;
            }
            
            Vector3 shopCenter;
            if (ShopBoundary.Instance != null)
            {
                shopCenter = ShopBoundary.Instance.GetShopCenter();
            }
            else
            {
                // Fallback: use origin or find a reasonable shop center
                shopCenter = Vector3.zero;
                Debug.LogWarning("[EnterShopTask] No ShopBoundary found, using origin as shop center");
            }

            // Move to shop center
            bool moveStarted = customer.Movement.SetDestination(shopCenter);
            isMovingToShop = moveStarted;
            
            if (!moveStarted)
            {
                Debug.LogError($"[EnterShopTask] {customer.name}: Failed to start movement to shop!");
            }
            else if (customer.showDebugLogs)
            {
                Debug.Log($"[EnterShopTask] {customer.name}: Moving to shop center at {shopCenter}");
            }
        }
    }
}