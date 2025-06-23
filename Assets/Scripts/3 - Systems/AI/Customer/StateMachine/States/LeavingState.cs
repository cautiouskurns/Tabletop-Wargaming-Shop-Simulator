using UnityEngine;

namespace TabletopShop
{
    /// <summary>
    /// State representing customer leaving the shop.
    /// Handles movement to exit point and cleanup.
    /// Converts existing coroutine logic from CustomerBehavior.HandleLeavingState.
    /// </summary>
    public class LeavingState : BaseCustomerState
    {
        public override string StateName => "Leaving";
        public override CustomerState GetStateType() => CustomerState.Leaving;
        
        // State-specific data
        private bool hasFoundExit = false;
        private bool isMovingToExit = false;
        private float leavingStartTime = 0f;
        private const float MAX_LEAVING_TIME = 45f; // Safety timeout for leaving
        private const float CLEANUP_DELAY = 2f; // Wait time after reaching exit before cleanup
        
        public override void OnEnter(CustomerStateContext context)
        {
            base.OnEnter(context);
            
            // Initialize leaving state
            leavingStartTime = Time.time;
            hasFoundExit = false;
            isMovingToExit = false;
            
            context.LogDebug("Customer is leaving the shop");
            
            // Start movement to exit
            StartMovementToExit(context);
        }
        
        public override void OnUpdate(CustomerStateContext context)
        {
            base.OnUpdate(context);
            
            float leavingTime = Time.time - leavingStartTime;
            
            // Safety timeout - force cleanup if taking too long to leave
            if (leavingTime > MAX_LEAVING_TIME)
            {
                context.LogWarning($"Leaving timeout after {MAX_LEAVING_TIME}s - forcing cleanup");
                HandleCustomerCleanup(context);
                return;
            }
            
            // Handle movement to exit
            if (isMovingToExit && ValidateMovement(context))
            {
                // Check if we've reached the exit
                if (context.Movement.HasReachedDestination())
                {
                    context.LogDebug("Reached exit destination");
                    
                    // Wait a moment before cleanup (like original coroutine)
                    if (leavingTime >= CLEANUP_DELAY)
                    {
                        HandleCustomerCleanup(context);
                    }
                }
                // Check if movement failed
                else if (!context.Movement.IsMoving && context.Movement.HasDestination)
                {
                    context.LogWarning("Movement to exit failed - forcing cleanup");
                    HandleCustomerCleanup(context);
                }
            }
            else if (!isMovingToExit && !hasFoundExit)
            {
                // Retry finding exit if we haven't started moving yet
                StartMovementToExit(context);
            }
        }
        
        public override void OnExit(CustomerStateContext context)
        {
            base.OnExit(context);
            
            // Final cleanup
            float totalLeavingTime = Time.time - leavingStartTime;
            context.LogDebug($"Leaving state complete - Duration: {totalLeavingTime:F1}s");
        }
        
        public override bool CanTransitionTo(CustomerState targetState, CustomerStateContext context)
        {
            // Leaving is typically a terminal state - no transitions allowed
            // This prevents customers from re-entering after starting to leave
            context.LogWarning($"Cannot transition from Leaving to {targetState} - Leaving is terminal state");
            return false;
        }
        
        public override string GetDebugInfo(CustomerStateContext context)
        {
            string baseInfo = base.GetDebugInfo(context);
            float leavingTime = Time.time - leavingStartTime;
            
            string stateSpecificInfo = $"Leaving Duration: {leavingTime:F1}s\n" +
                                     $"Has Found Exit: {hasFoundExit}\n" +
                                     $"Is Moving to Exit: {isMovingToExit}\n" +
                                     $"Has Destination: {(ValidateMovement(context) ? context.Movement.HasDestination : false)}\n" +
                                     $"Is Moving: {(ValidateMovement(context) ? context.Movement.IsMoving : false)}";
            
            return $"{baseInfo}\n{stateSpecificInfo}";
        }
        
        /// <summary>
        /// Start movement to exit point (converted from coroutine logic)
        /// </summary>
        /// <param name="context">Customer state context</param>
        private void StartMovementToExit(CustomerStateContext context)
        {
            if (!ValidateMovement(context))
            {
                context.LogWarning("Cannot start exit movement - movement component not available, forcing cleanup");
                HandleCustomerCleanup(context);
                return;
            }
            
            // Try to find and move to exit
            bool foundExit = context.Movement.MoveToExitPoint();
            
            if (foundExit)
            {
                hasFoundExit = true;
                isMovingToExit = true;
                context.LogDebug("Started movement to exit point");
            }
            else
            {
                context.LogWarning("Failed to find exit point - using fallback cleanup");
                
                // Fallback: try to move away from the shop center
                Vector3 shopCenter = Vector3.zero;
                Vector3 exitDirection = (context.Customer.transform.position - shopCenter).normalized;
                Vector3 fallbackExit = context.Customer.transform.position + exitDirection * 10f;
                
                if (context.Movement.SetDestination(fallbackExit))
                {
                    hasFoundExit = true;
                    isMovingToExit = true;
                    context.LogDebug("Using fallback exit destination");
                }
                else
                {
                    context.LogError("Complete exit failure - forcing immediate cleanup");
                    HandleCustomerCleanup(context);
                }
            }
        }
        
        /// <summary>
        /// Handle customer cleanup and removal from scene
        /// </summary>
        /// <param name="context">Customer state context</param>
        private void HandleCustomerCleanup(CustomerStateContext context)
        {
            context.LogDebug("Starting customer cleanup process");
            
            // Clean up any remaining queue state
            if (ValidateBehavior(context) && context.Behavior.IsInQueue)
            {
                context.Behavior.ForceLeaveQueue();
                context.LogDebug("Cleaned up queue state");
            }
            
            // Stop any ongoing movement
            if (ValidateMovement(context))
            {
                context.Movement.StopMovement();
            }
            
            // Update analytics or other systems about customer leaving
            HandleCustomerAnalytics(context);
            
            // Schedule customer destruction
            // Note: In a real implementation, this might be handled by a customer manager
            // or spawning system rather than directly destroying the GameObject
            if (context.Customer != null)
            {
                context.LogDebug("Customer leaving - scheduling cleanup");
                
                // In a production system, you might want to:
                // 1. Return customer to object pool
                // 2. Update customer analytics
                // 3. Notify customer manager
                // 4. Trigger leaving effects
                
                // For now, we'll schedule destruction after a brief delay
                Object.Destroy(context.Customer.gameObject, 1f);
            }
        }
        
        /// <summary>
        /// Handle customer analytics and metrics before leaving
        /// </summary>
        /// <param name="context">Customer state context</param>
        private void HandleCustomerAnalytics(CustomerStateContext context)
        {
            if (!ValidateBehavior(context)) return;
            
            // Calculate visit metrics
            float totalVisitTime = Time.time - (context.StateStartTime - context.TotalTimeInCurrentState);
            int productsSelected = context.Behavior.SelectedProducts?.Count ?? 0;
            float totalSpent = context.Behavior.TotalPurchaseAmount;
            
            context.LogDebug($"Customer analytics - Visit time: {totalVisitTime:F1}s, Products: {productsSelected}, Spent: ${totalSpent:F2}, Shelves visited: {context.ShelvesVisited}");
            
            // Here you could send this data to an analytics system:
            // AnalyticsManager.Instance.RecordCustomerVisit(totalVisitTime, productsSelected, totalSpent, context.ShelvesVisited);
        }
    }
}
