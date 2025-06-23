using UnityEngine;
using System.Collections;

namespace TabletopShop
{
    /// <summary>
    /// State representing customer purchasing behavior.
    /// Handles checkout queue management, item placement, and transaction completion.
    /// Converts existing coroutine logic from CustomerBehavior.HandlePurchasingState.
    /// </summary>
    public class PurchasingState : BaseCustomerState
    {
        public override string StateName => "Purchasing";
        public override CustomerState GetStateType() => CustomerState.Purchasing;
        
        // State-specific data
        private bool hasReachedCheckout = false;
        private bool isWaitingForCheckoutTurn = false;
        private bool hasPlacedItems = false;
        private bool hasCompletedTransaction = false;
        private float purchaseStartTime = 0f;
        private float waitStartTime = 0f;
        private CheckoutCounter targetCheckoutCounter = null;
        
        // Purchase behavior configuration
        private const float MAX_WAIT_TIME = 60f; // Maximum wait time before giving up
        private const float CHECKOUT_TIMEOUT = 120f; // Maximum total time in purchasing state
        private const float QUEUE_CHECK_INTERVAL = 5f; // Check queue status every 5 seconds
        
        public override void OnEnter(CustomerStateContext context)
        {
            base.OnEnter(context);
            
            // Initialize purchasing state
            purchaseStartTime = Time.time;
            hasReachedCheckout = false;
            isWaitingForCheckoutTurn = false;
            hasPlacedItems = false;
            hasCompletedTransaction = false;
            targetCheckoutCounter = null;
            
            context.LogDebug("Started purchasing - looking for checkout counter");
            
            // Start movement to checkout
            StartCheckoutProcess(context);
        }
        
        public override void OnUpdate(CustomerStateContext context)
        {
            base.OnUpdate(context);
            
            float totalPurchaseTime = Time.time - purchaseStartTime;
            
            // Safety timeout - if stuck in purchasing too long
            if (totalPurchaseTime > CHECKOUT_TIMEOUT)
            {
                context.LogWarning($"Purchasing timeout after {CHECKOUT_TIMEOUT}s - transitioning to leaving");
                context.RequestStateTransition(CustomerState.Leaving, "Checkout timeout");
                return;
            }
            
            // Check if store closed while purchasing
            if (!IsStoreOpen())
            {
                context.LogDebug("Store closed while purchasing - transitioning to leaving");
                context.RequestStateTransition(CustomerState.Leaving, "Store closed during purchase");
                return;
            }
            
            // Handle different phases of purchasing
            if (!hasReachedCheckout)
            {
                HandleMovementToCheckout(context);
            }
            else if (!hasPlacedItems)
            {
                HandleItemPlacement(context);
            }
            else if (!hasCompletedTransaction)
            {
                HandleTransactionCompletion(context);
            }
            else
            {
                // Transaction complete - move to leaving
                context.LogDebug("Purchase transaction complete - transitioning to leaving");
                context.RequestStateTransition(CustomerState.Leaving, "Transaction complete");
            }
        }
        
        public override void OnExit(CustomerStateContext context)
        {
            base.OnExit(context);
            
            // Clean up checkout state if we were in a queue
            if (ValidateBehavior(context) && context.Behavior.IsInQueue)
            {
                context.Behavior.ForceLeaveQueue();
                context.LogDebug("Left checkout queue during state exit");
            }
            
            // Log purchase summary
            float totalPurchaseTime = Time.time - purchaseStartTime;
            context.LogDebug($"Purchase complete - Duration: {totalPurchaseTime:F1}s, Transaction completed: {hasCompletedTransaction}");
        }
        
        public override bool CanTransitionTo(CustomerState targetState, CustomerStateContext context)
        {
            // Always allow emergency exit to leaving
            if (targetState == CustomerState.Leaving)
            {
                return true;
            }
            
            // Don't allow transitions back to entering or shopping
            if (targetState == CustomerState.Entering || targetState == CustomerState.Shopping)
            {
                context.LogWarning($"Cannot transition back to {targetState} from Purchasing");
                return false;
            }
            
            return base.CanTransitionTo(targetState, context);
        }
        
        public override string GetDebugInfo(CustomerStateContext context)
        {
            string baseInfo = base.GetDebugInfo(context);
            float totalTime = Time.time - purchaseStartTime;
            float waitTime = isWaitingForCheckoutTurn ? Time.time - waitStartTime : 0f;
            
            string stateSpecificInfo = $"Purchase Progress: {totalTime:F1}s\n" +
                                     $"Has Reached Checkout: {hasReachedCheckout}\n" +
                                     $"Waiting for Turn: {isWaitingForCheckoutTurn}\n" +
                                     $"Wait Time: {waitTime:F1}s\n" +
                                     $"Has Placed Items: {hasPlacedItems}\n" +
                                     $"Transaction Complete: {hasCompletedTransaction}\n" +
                                     $"Target Checkout: {targetCheckoutCounter?.name ?? "None"}";
            
            return $"{baseInfo}\n{stateSpecificInfo}";
        }
        
        /// <summary>
        /// Start the checkout process by finding and moving to a checkout counter
        /// </summary>
        /// <param name="context">Customer state context</param>
        private void StartCheckoutProcess(CustomerStateContext context)
        {
            if (!ValidateMovement(context))
            {
                context.LogError("Cannot start checkout - movement component unavailable");
                context.RequestStateTransition(CustomerState.Leaving, "Movement unavailable");
                return;
            }
            
            // Try to move to checkout point
            bool foundCheckout = context.Movement.MoveToCheckoutPoint();
            
            if (foundCheckout)
            {
                context.LogDebug("Started movement to checkout area");
                
                // Try to find a specific checkout counter
                var checkoutCounters = Object.FindObjectsByType<CheckoutCounter>(FindObjectsSortMode.None);
                if (checkoutCounters.Length > 0)
                {
                    // Find the closest available checkout counter
                    targetCheckoutCounter = FindBestCheckoutCounter(context, checkoutCounters);
                    if (targetCheckoutCounter != null)
                    {
                        context.LogDebug($"Target checkout counter: {targetCheckoutCounter.name}");
                    }
                }
            }
            else
            {
                context.LogError("Failed to find checkout area - transitioning to leaving");
                context.RequestStateTransition(CustomerState.Leaving, "No checkout available");
            }
        }
        
        /// <summary>
        /// Handle movement to checkout counter
        /// </summary>
        /// <param name="context">Customer state context</param>
        private void HandleMovementToCheckout(CustomerStateContext context)
        {
            if (!ValidateMovement(context)) return;
            
            // Check if we've reached the checkout area
            if (context.Movement.HasReachedDestination())
            {
                hasReachedCheckout = true;
                context.LogDebug("Reached checkout area");
                
                // Try to join a queue if we have a target checkout counter
                if (targetCheckoutCounter != null && ValidateBehavior(context))
                {
                    // Join the queue (this will be handled by the existing queue system)
                    if (!context.Behavior.IsInQueue)
                    {
                        isWaitingForCheckoutTurn = true;
                        waitStartTime = Time.time;
                        context.LogDebug("Waiting for checkout turn");
                    }
                }
            }
            
            // Check for movement issues
            if (!context.Movement.IsMoving && !context.Movement.HasReachedDestination())
            {
                context.LogWarning("Movement to checkout failed - trying to leave");
                context.RequestStateTransition(CustomerState.Leaving, "Checkout movement failed");
            }
        }
        
        /// <summary>
        /// Handle item placement on checkout counter
        /// </summary>
        /// <param name="context">Customer state context</param>
        private void HandleItemPlacement(CustomerStateContext context)
        {
            if (!ValidateBehavior(context)) return;
            
            // Check if we're still waiting for our turn
            if (isWaitingForCheckoutTurn)
            {
                float waitTime = Time.time - waitStartTime;
                
                // Check wait timeout
                if (waitTime > MAX_WAIT_TIME)
                {
                    context.LogWarning($"Checkout wait timeout after {waitTime:F1}s - leaving");
                    context.RequestStateTransition(CustomerState.Leaving, "Checkout wait timeout");
                    return;
                }
                
                // Check if it's our turn (this would be set by the queue system)
                if (!context.Behavior.WaitingForCheckoutTurn)
                {
                    isWaitingForCheckoutTurn = false;
                    context.LogDebug("Checkout turn ready - placing items");
                    
                    // Place items on counter (simulate the coroutine behavior)
                    PlaceItemsOnCounter(context);
                }
            }
        }
        
        /// <summary>
        /// Handle transaction completion
        /// </summary>
        /// <param name="context">Customer state context</param>
        private void HandleTransactionCompletion(CustomerStateContext context)
        {
            if (!ValidateBehavior(context)) return;
            
            // Check if checkout is complete (this would be signaled by the checkout system)
            if (!context.Behavior.IsWaitingForCheckout)
            {
                hasCompletedTransaction = true;
                context.LogDebug("Transaction completed");
                
                // The transition to leaving will be handled in OnUpdate
            }
        }
        
        /// <summary>
        /// Place items on the checkout counter (simulates coroutine behavior)
        /// </summary>
        /// <param name="context">Customer state context</param>
        private void PlaceItemsOnCounter(CustomerStateContext context)
        {
            if (targetCheckoutCounter != null && ValidateBehavior(context))
            {
                // This would interface with the existing checkout system
                // For now, simulate the behavior
                hasPlacedItems = true;
                context.LogDebug($"Placed {context.Behavior.SelectedProducts?.Count ?? 0} items on checkout counter");
                
                // Start waiting for transaction completion
                // This would be handled by the existing checkout counter system
            }
            else
            {
                context.LogWarning("Cannot place items - no checkout counter available");
                context.RequestStateTransition(CustomerState.Leaving, "No checkout counter");
            }
        }
        
        /// <summary>
        /// Find the best checkout counter for this customer
        /// </summary>
        /// <param name="context">Customer state context</param>
        /// <param name="checkoutCounters">Available checkout counters</param>
        /// <returns>Best checkout counter or null</returns>
        private CheckoutCounter FindBestCheckoutCounter(CustomerStateContext context, CheckoutCounter[] checkoutCounters)
        {
            CheckoutCounter bestCounter = null;
            float bestScore = float.MaxValue;
            
            foreach (var counter in checkoutCounters)
            {
                if (counter == null || !counter.gameObject.activeInHierarchy) continue;
                
                // Simple scoring: prefer counters with shorter queues
                float distance = Vector3.Distance(context.Customer.transform.position, counter.transform.position);
                int queueLength = 0; // This would come from the checkout counter queue system
                
                float score = distance + (queueLength * 10f); // Weight queue length more than distance
                
                if (score < bestScore)
                {
                    bestScore = score;
                    bestCounter = counter;
                }
            }
            
            return bestCounter;
        }
    }
}
