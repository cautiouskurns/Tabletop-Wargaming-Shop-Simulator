using UnityEngine;

namespace TabletopShop
{
    /// <summary>
    /// State representing customer entering the shop.
    /// Handles initial movement to a random shelf and store validation.
    /// Converts existing coroutine logic from CustomerBehavior.HandleEnteringState.
    /// </summary>
    public class EnteringState : BaseCustomerState
    {
        public override string StateName => "Entering";
        public override CustomerState GetStateType() => CustomerState.Entering;
        
        // State-specific data
        private bool hasFoundShelf = false;
        private bool isMovingToShelf = false;
        private float entryStartTime = 0f;
        private const float MAX_ENTRY_TIME = 30f; // Safety timeout
        
        public override void OnEnter(CustomerStateContext context)
        {
            base.OnEnter(context);
            
            // Reset state-specific data
            hasFoundShelf = false;
            isMovingToShelf = false;
            entryStartTime = Time.time;
            
            // Check if store is open before proceeding
            if (!IsStoreOpen())
            {
                context.LogDebug("Store is closed - transitioning to leaving");
                context.RequestStateTransition(CustomerState.Leaving, "Store closed");
                return;
            }
            
            // Start movement to random shelf (converted from coroutine logic)
            StartMovementToShelf(context);
        }
        
        public override void OnUpdate(CustomerStateContext context)
        {
            base.OnUpdate(context);
            
            // Safety timeout - if stuck in entering state too long
            if (Time.time - entryStartTime > MAX_ENTRY_TIME)
            {
                context.LogWarning($"Entering state timeout after {MAX_ENTRY_TIME}s - forcing transition to leaving");
                context.RequestStateTransition(CustomerState.Leaving, "Entry timeout");
                return;
            }
            
            // Check if store closed while entering
            if (!IsStoreOpen())
            {
                context.LogDebug("Store closed while entering - transitioning to leaving");
                context.RequestStateTransition(CustomerState.Leaving, "Store closed during entry");
                return;
            }
            
            // Check if we're currently moving to a shelf
            if (isMovingToShelf && ValidateMovement(context))
            {
                // Check if we've reached our destination
                if (context.Movement.HasReachedDestination())
                {
                    context.LogDebug("Reached shelf destination - transitioning to shopping");
                    context.RequestStateTransition(CustomerState.Shopping, "Reached initial shelf");
                    return;
                }
                
                // Check if movement is stuck or failed
                if (!context.Movement.IsMoving && context.Movement.HasDestination)
                {
                    // Try to recover by finding a new shelf
                    context.LogWarning("Movement stuck during entry - trying new shelf");
                    StartMovementToShelf(context);
                }
            }
            else if (!isMovingToShelf && !hasFoundShelf)
            {
                // If we haven't started moving yet, try again
                StartMovementToShelf(context);
            }
        }
        
        public override void OnExit(CustomerStateContext context)
        {
            base.OnExit(context);
            
            // Clean up state-specific data
            hasFoundShelf = false;
            isMovingToShelf = false;
        }
        
        public override bool CanTransitionTo(CustomerState targetState, CustomerStateContext context)
        {
            // Allow emergency exit to leaving
            if (targetState == CustomerState.Leaving)
            {
                return true;
            }
            
            // Allow transition to shopping if we've found a shelf
            if (targetState == CustomerState.Shopping)
            {
                return hasFoundShelf || context.Movement?.HasReachedDestination() == true;
            }
            
            // Don't allow direct transition to purchasing from entering
            if (targetState == CustomerState.Purchasing)
            {
                context.LogWarning("Cannot transition directly from Entering to Purchasing");
                return false;
            }
            
            return base.CanTransitionTo(targetState, context);
        }
        
        public override string GetDebugInfo(CustomerStateContext context)
        {
            string baseInfo = base.GetDebugInfo(context);
            string stateSpecificInfo = $"Has Found Shelf: {hasFoundShelf}\n" +
                                     $"Is Moving to Shelf: {isMovingToShelf}\n" +
                                     $"Entry Duration: {Time.time - entryStartTime:F1}s\n" +
                                     $"Store Open: {IsStoreOpen()}";
            
            return $"{baseInfo}\n{stateSpecificInfo}";
        }
        
        /// <summary>
        /// Start movement to a random shelf (converted from coroutine logic)
        /// </summary>
        /// <param name="context">Customer state context</param>
        private void StartMovementToShelf(CustomerStateContext context)
        {
            if (!ValidateMovement(context))
            {
                context.LogError("Cannot start movement - movement component not available");
                context.RequestStateTransition(CustomerState.Leaving, "Movement component unavailable");
                return;
            }
            
            // Try to find and move to a random shelf
            bool foundShelf = context.Movement.SetRandomShelfDestination();
            
            if (foundShelf)
            {
                hasFoundShelf = true;
                isMovingToShelf = true;
                context.LogDebug("Started movement to random shelf");
                
                // Update context with shelf information if available
                if (ValidateBehavior(context) && context.Behavior.TargetShelf != null)
                {
                    context.LogDebug($"Target shelf: {context.Behavior.TargetShelf.name}");
                }
            }
            else
            {
                context.LogError("Failed to find available shelf - transitioning to leaving");
                context.RequestStateTransition(CustomerState.Leaving, "No available shelves");
            }
        }
    }
}
