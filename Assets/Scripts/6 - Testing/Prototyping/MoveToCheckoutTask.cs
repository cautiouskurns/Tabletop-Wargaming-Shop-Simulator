using UnityEngine;
using Opsive.BehaviorDesigner.Runtime.Tasks.Actions;
using Opsive.BehaviorDesigner.Runtime.Tasks;

namespace TabletopShop
{
    /// <summary>
    /// Behavior Designer task for moving customer to checkout area
    /// Handles finding nearest checkout counter and navigation
    /// </summary>
    public class MoveToCheckoutTask : Action
    {
        private bool isMoving = false;
        private CheckoutCounter targetCheckout = null;
        
        public override void OnStart()
        {
            Customer customer = GetComponent<Customer>();
            if (customer == null)
            {
                Debug.LogError("[MoveToCheckoutTask] Customer component not found!");
                return;
            }
            
            if (customer.Movement == null)
            {
                Debug.LogError("[MoveToCheckoutTask] Movement component not available!");
                return;
            }
            
            // Find nearest checkout counter
            targetCheckout = FindNearestCheckoutCounter();
            if (targetCheckout == null)
            {
                Debug.LogError($"[MoveToCheckoutTask] {customer.name}: No checkout counter found in scene!");
                return;
            }
            
            // Use the existing CustomerMovement method that handles NavMesh properly
            bool moveStarted = customer.Movement.MoveToCheckoutPoint();
            isMoving = moveStarted;
            
            if (moveStarted)
            {
                if (customer.showDebugLogs)
                    Debug.Log($"[MoveToCheckoutTask] ✅ {customer.name}: Started moving to checkout");
            }
            else
            {
                Debug.LogError($"[MoveToCheckoutTask] {customer.name}: Failed to start checkout movement!");
            }
        }
        
        public override TaskStatus OnUpdate()
        {
            if (!isMoving)
                return TaskStatus.Failure;
            
            Customer customer = GetComponent<Customer>();
            if (customer == null)
                return TaskStatus.Failure;
            
            // Check if reached checkout area
            if (customer.Movement != null && customer.Movement.HasReachedDestination())
            {
                if (customer.showDebugLogs)
                    Debug.Log($"[MoveToCheckoutTask] ✅ {customer.name}: Reached checkout area");
                return TaskStatus.Success;
            }
            
            return TaskStatus.Running;
        }
        
        public override void OnEnd()
        {
            isMoving = false;
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
                Debug.LogWarning("[MoveToCheckoutTask] No checkout counters found in scene");
                return null;
            }
            
            // For now, return the first one. Could be enhanced to find truly nearest one.
            CheckoutCounter nearest = checkoutCounters[0];
            Debug.Log($"[MoveToCheckoutTask] Found checkout counter: {nearest.name}");
            return nearest;
        }
    }
}