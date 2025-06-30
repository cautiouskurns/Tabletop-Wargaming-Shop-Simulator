using UnityEngine;
using Opsive.BehaviorDesigner.Runtime.Tasks.Actions;
using Opsive.BehaviorDesigner.Runtime.Tasks;

namespace TabletopShop
{
    /// <summary>
    /// Behavior Designer task for joining checkout queue and waiting for turn
    /// Handles queue positioning and customer arrival at checkout
    /// </summary>
    public class JoinQueueTask : Action
    {
        [Header("Settings Override (Optional)")]
        [Tooltip("Leave null to use global settings from CustomerBehaviorSettingsManager")]
        public CustomerBehaviorSettings settingsOverride;
        
        private CheckoutCounter checkoutCounter = null;
        private float queueStartTime = 0f;
        private bool hasJoinedQueue = false;
        
        /// <summary>
        /// Get the checkout settings to use (either override or global)
        /// </summary>
        private CheckoutSettings GetCheckoutSettings()
        {
            if (settingsOverride != null && settingsOverride.checkout != null)
                return settingsOverride.checkout;
            
            return CustomerBehaviorSettingsManager.Checkout;
        }
        
        public override void OnStart()
        {
            Customer customer = GetComponent<Customer>();
            if (customer == null)
            {
                Debug.LogError("[JoinQueueTask] Customer component not found!");
                return;
            }
            
            // Find nearest checkout counter
            checkoutCounter = FindNearestCheckoutCounter();
            if (checkoutCounter == null)
            {
                Debug.LogError($"[JoinQueueTask] {customer.name}: No checkout counter found!");
                return;
            }
            
            // Join the checkout queue
            checkoutCounter.OnCustomerArrival(customer);
            hasJoinedQueue = true;
            queueStartTime = Time.time;
            
            if (customer.showDebugLogs)
                Debug.Log($"[JoinQueueTask] {customer.name}: Joined checkout queue");
        }
        
        public override TaskStatus OnUpdate()
        {
            if (!hasJoinedQueue || checkoutCounter == null)
                return TaskStatus.Failure;
                
            Customer customer = GetComponent<Customer>();
            if (customer == null)
                return TaskStatus.Failure;
            
            // Check timeout using settings
            var checkoutSettings = GetCheckoutSettings();
            if (checkoutSettings != null && Time.time - queueStartTime > checkoutSettings.maxQueueWaitTime)
            {
                Debug.LogWarning($"[JoinQueueTask] {customer.name}: Queue wait timeout ({checkoutSettings.maxQueueWaitTime}s) - giving up");
                return TaskStatus.Failure;
            }
            
            // Check if customer can proceed to checkout (is current customer)
            if (checkoutCounter.HasCustomer && IsCurrentCustomer(customer))
            {
                if (customer.showDebugLogs)
                    Debug.Log($"[JoinQueueTask] ✅ {customer.name}: Ready for checkout - is current customer");
                return TaskStatus.Success;
            }
            
            // Still waiting in queue - log based on settings interval
            var logInterval = checkoutSettings?.queueStatusLogInterval ?? 3f;
            if (customer.showDebugLogs && Time.frameCount % Mathf.RoundToInt(logInterval * 60f) == 0)
            {
                int queuePosition = GetQueuePosition(customer);
                Debug.Log($"[JoinQueueTask] {customer.name}: Waiting in queue (position: {queuePosition})");
            }
            
            return TaskStatus.Running;
        }
        
        public override void OnEnd()
        {
            hasJoinedQueue = false;
        }
        
        /// <summary>
        /// Find the nearest checkout counter in the scene
        /// </summary>
        /// <returns>Nearest CheckoutCounter or null if none found</returns>
        private CheckoutCounter FindNearestCheckoutCounter()
        {
            CheckoutCounter[] checkoutCounters = Object.FindObjectsByType<CheckoutCounter>(FindObjectsSortMode.None);
            
            if (checkoutCounters.Length == 0)
            {
                return null;
            }
            
            // Return the first one for now
            return checkoutCounters[0];
        }
        
        /// <summary>
        /// Check if the customer is the current customer at checkout
        /// </summary>
        /// <param name="customer">Customer to check</param>
        /// <returns>True if customer is being served</returns>
        private bool IsCurrentCustomer(Customer customer)
        {
            // Use reflection to access private currentCustomer field or add public property
            // For now, assume customer is current if they can place items
            return checkoutCounter.CanCustomerPlaceItems(customer);
        }
        
        /// <summary>
        /// Get the customer's position in queue (rough estimate)
        /// </summary>
        /// <param name="customer">Customer to check</param>
        /// <returns>Queue position (0-based)</returns>
        private int GetQueuePosition(Customer customer)
        {
            // Since QueueLength is public, we can use it as an estimate
            return checkoutCounter.QueueLength;
        }
    }
}