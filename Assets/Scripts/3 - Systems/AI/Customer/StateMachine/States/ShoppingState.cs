using UnityEngine;

namespace TabletopShop
{
    /// <summary>
    /// State representing customer shopping behavior.
    /// Handles shelf browsing, product selection, and shopping time management.
    /// Converts existing coroutine logic from CustomerBehavior.HandleShoppingState.
    /// </summary>
    public class ShoppingState : BaseCustomerState
    {
        public override string StateName => "Shopping";
        public override CustomerState GetStateType() => CustomerState.Shopping;
        
        // State-specific data
        private float shoppingStartTime = 0f;
        private float lastProductCheckTime = 0f;
        private float lastShelfSwitchTime = 0f;
        private bool hasSelectedProducts = false;
        private int shelvesVisited = 0;
        
        // Shopping behavior configuration
        private const float PRODUCT_CHECK_INTERVAL = 3f; // Check for products every 3 seconds
        private const float SHELF_SWITCH_PROBABILITY = 0.3f; // 30% chance to switch shelves
        private const float SHELF_SWITCH_COOLDOWN = 2f; // Wait 2 seconds between shelf switches
        private const float HURRY_UP_TIME_MULTIPLIER = 0.7f; // Reduce shopping time when hurrying
        
        public override void OnEnter(CustomerStateContext context)
        {
            base.OnEnter(context);
            
            // Initialize shopping state
            shoppingStartTime = Time.time;
            lastProductCheckTime = Time.time;
            lastShelfSwitchTime = Time.time;
            hasSelectedProducts = false;
            shelvesVisited = 0;
            
            // Update context with shopping start time
            context.ShoppingStartTime = shoppingStartTime;
            context.ShelvesVisited = 0;
            context.HasSelectedProducts = false;
            
            context.LogDebug($"Started shopping - target duration: {GetShoppingDuration(context):F1}s");
        }
        
        public override void OnUpdate(CustomerStateContext context)
        {
            base.OnUpdate(context);
            
            float shoppedTime = Time.time - shoppingStartTime;
            float targetShoppingTime = GetShoppingDuration(context);
            
            // Check if store is closing
            if (!IsStoreOpen())
            {
                context.LogDebug("Store closed while shopping - transitioning to leaving");
                context.RequestStateTransition(CustomerState.Leaving, "Store closed");
                return;
            }
            
            // Check if we should hurry up due to store closing soon
            if (IsStoreClosingSoon())
            {
                targetShoppingTime *= HURRY_UP_TIME_MULTIPLIER;
                context.LogDebug("Store closing soon - hurrying up shopping");
            }
            
            // Handle shelf switching and movement
            HandleShelfBrowsing(context, shoppedTime);
            
            // Handle product selection
            HandleProductSelection(context, shoppedTime);
            
            // Check if shopping time is complete
            if (shoppedTime >= targetShoppingTime)
            {
                context.LogDebug($"Shopping time complete ({shoppedTime:F1}s) - transitioning to purchasing");
                context.RequestStateTransition(CustomerState.Purchasing, "Shopping time complete");
                return;
            }
        }
        
        public override void OnExit(CustomerStateContext context)
        {
            base.OnExit(context);
            
            // Update context with final shopping data
            context.ShelvesVisited = shelvesVisited;
            context.HasSelectedProducts = hasSelectedProducts;
            
            // Log shopping summary
            float totalShoppingTime = Time.time - shoppingStartTime;
            context.LogDebug($"Shopping complete - Duration: {totalShoppingTime:F1}s, Shelves visited: {shelvesVisited}, Products selected: {hasSelectedProducts}");
        }
        
        public override bool CanTransitionTo(CustomerState targetState, CustomerStateContext context)
        {
            // Allow emergency exit to leaving
            if (targetState == CustomerState.Leaving)
            {
                return true;
            }
            
            // Allow transition to purchasing if we have products or shopping time is complete
            if (targetState == CustomerState.Purchasing)
            {
                float shoppedTime = Time.time - shoppingStartTime;
                float targetTime = GetShoppingDuration(context);
                
                return hasSelectedProducts || shoppedTime >= targetTime;
            }
            
            // Don't allow going back to entering
            if (targetState == CustomerState.Entering)
            {
                context.LogWarning("Cannot transition back to Entering from Shopping");
                return false;
            }
            
            return base.CanTransitionTo(targetState, context);
        }
        
        public override string GetDebugInfo(CustomerStateContext context)
        {
            string baseInfo = base.GetDebugInfo(context);
            float shoppedTime = Time.time - shoppingStartTime;
            float targetTime = GetShoppingDuration(context);
            
            string stateSpecificInfo = $"Shopping Progress: {shoppedTime:F1}s / {targetTime:F1}s\n" +
                                     $"Shelves Visited: {shelvesVisited}\n" +
                                     $"Has Selected Products: {hasSelectedProducts}\n" +
                                     $"Store Open: {IsStoreOpen()}\n" +
                                     $"Store Closing Soon: {IsStoreClosingSoon()}";
            
            return $"{baseInfo}\n{stateSpecificInfo}";
        }
        
        /// <summary>
        /// Handle shelf browsing and movement between shelves
        /// </summary>
        /// <param name="context">Customer state context</param>
        /// <param name="shoppedTime">Time spent shopping so far</param>
        private void HandleShelfBrowsing(CustomerStateContext context, float shoppedTime)
        {
            if (!ValidateMovement(context)) return;
            
            // Check if we should switch to a new shelf
            bool shouldSwitchShelf = false;
            
            // Switch if we've reached current destination
            if (context.Movement.HasReachedDestination())
            {
                shouldSwitchShelf = true;
            }
            // Random shelf switching (like original coroutine logic)
            else if (Time.time - lastShelfSwitchTime > SHELF_SWITCH_COOLDOWN && 
                     Random.value < SHELF_SWITCH_PROBABILITY)
            {
                shouldSwitchShelf = true;
            }
            
            if (shouldSwitchShelf)
            {
                // Move to a new random shelf
                if (context.Movement.SetRandomShelfDestination())
                {
                    shelvesVisited++;
                    lastShelfSwitchTime = Time.time;
                    context.LogDebug($"Switched to new shelf (total visited: {shelvesVisited})");
                    
                    // Update context
                    context.ShelvesVisited = shelvesVisited;
                }
            }
        }
        
        /// <summary>
        /// Handle product selection at shelves
        /// </summary>
        /// <param name="context">Customer state context</param>
        /// <param name="shoppedTime">Time spent shopping so far</param>
        private void HandleProductSelection(CustomerStateContext context, float shoppedTime)
        {
            // Check for products periodically (like original coroutine logic)
            if (shoppedTime - lastProductCheckTime >= PRODUCT_CHECK_INTERVAL)
            {
                lastProductCheckTime = shoppedTime;
                
                // Try to select products at current shelf if we have one
                if (ValidateBehavior(context) && context.Behavior.TargetShelf != null)
                {
                    // Call the existing product selection logic
                    context.Behavior.TrySelectProductsAtShelf(context.Behavior.TargetShelf);
                    
                    // Check if we now have products
                    if (context.Behavior.SelectedProducts != null && context.Behavior.SelectedProducts.Count > 0)
                    {
                        hasSelectedProducts = true;
                        context.HasSelectedProducts = true;
                        context.LogDebug($"Selected {context.Behavior.SelectedProducts.Count} products");
                    }
                }
            }
        }
        
        /// <summary>
        /// Get the target shopping duration for this customer
        /// </summary>
        /// <param name="context">Customer state context</param>
        /// <returns>Shopping duration in seconds</returns>
        private float GetShoppingDuration(CustomerStateContext context)
        {
            if (ValidateBehavior(context))
            {
                return context.Behavior.ShoppingTime;
            }
            
            // Fallback default shopping time
            return 15f;
        }
    }
}
