using UnityEngine;
using System.Collections;

namespace TabletopShop
{
   
    /// <summary>
    /// PURCHASING STATE - Controls all checkout behavior
    /// </summary>
    public class PurchasingState : BaseCustomerState
    {
        private float purchaseStartTime;
        private bool hasReachedCheckout = false;
        private bool hasPlacedItems = false;
        private bool hasCompletedTransaction = false;
        private CheckoutCounter targetCheckoutCounter = null;
        private const float CHECKOUT_TIMEOUT = 120f;
        private const float MAX_WAIT_TIME = 60f;
        
        public override void OnEnter(CustomerBehavior customer)
        {
            this.customer = customer;
            purchaseStartTime = Time.time;
            hasReachedCheckout = false;
            hasPlacedItems = false;
            hasCompletedTransaction = false;
            targetCheckoutCounter = null;
            
            Debug.Log($"{customer.name} started purchasing with {customer.SelectedProducts.Count} products");
            
            // STATE CONTROLS: Start movement to checkout
            StartCheckoutProcess();
        }
        
        public override void OnUpdate(CustomerBehavior customer)
        {
            float purchaseTime = Time.time - purchaseStartTime;
            
            // STATE DECIDES: Timeout check
            if (purchaseTime > CHECKOUT_TIMEOUT)
            {
                Debug.LogWarning($"{customer.name}: Purchase timeout");
                RequestTransition(CustomerState.Leaving, "Purchase timeout");
                return;
            }
            
            // STATE DECIDES: Store closing check (allow purchase completion even if store is closing)
            if (!IsStoreOpen() && ShouldLeaveStoreDueToHours())
            {
                Debug.Log($"{customer.name}: Store closed during purchase");
                RequestTransition(CustomerState.Leaving, "Store closed");
                return;
            }
            
            // STATE CONTROLS: Handle different phases of purchasing
            if (!hasReachedCheckout)
            {
                HandleMovementToCheckout();
            }
            else if (!hasPlacedItems)
            {
                HandleItemPlacement();
            }
            else if (!hasCompletedTransaction)
            {
                HandleTransactionCompletion();
            }
            else
            {
                // Transaction complete
                RequestTransition(CustomerState.Leaving, "Transaction complete");
            }
        }
        
        public override void OnExit(CustomerBehavior customer)
        {
            float totalPurchaseTime = Time.time - purchaseStartTime;
            Debug.Log($"{customer.name} finished purchasing after {totalPurchaseTime:F1}s");
            
            // Clean up checkout state
            if (customer.IsInQueue)
            {
                customer.ForceLeaveQueue();
            }
        }
        
        /// <summary>
        /// STATE CONTROLS: Start checkout process
        /// </summary>
        private void StartCheckoutProcess()
        {
            var movement = customer.GetMovement();
            if (movement == null)
            {
                Debug.LogError($"{customer.name} cannot move to checkout - no movement component");
                RequestTransition(CustomerState.Leaving, "No movement component");
                return;
            }
            
            // Move to checkout area
            bool foundCheckout = movement.MoveToCheckoutPoint();
            if (foundCheckout)
            {
                // Find specific checkout counter
                targetCheckoutCounter = FindNearestCheckoutCounter();
                Debug.Log($"{customer.name} moving to checkout area");
            }
            else
            {
                Debug.LogError($"{customer.name} failed to find checkout area");
                RequestTransition(CustomerState.Leaving, "No checkout available");
            }
        }
        
        /// <summary>
        /// STATE CONTROLS: Handle movement to checkout
        /// </summary>
        private void HandleMovementToCheckout()
        {
            var movement = customer.GetMovement();
            if (movement == null) return;
            
            if (movement.HasReachedDestination())
            {
                hasReachedCheckout = true;
                Debug.Log($"{customer.name} reached checkout area");
                
                // Join queue if we have a target checkout
                if (targetCheckoutCounter != null)
                {
                    // This will trigger the queue system
                    targetCheckoutCounter.OnCustomerArrival(customer.GetMainCustomer());
                }
            }
            else if (!movement.IsMoving)
            {
                Debug.LogWarning($"{customer.name} movement to checkout failed");
                RequestTransition(CustomerState.Leaving, "Checkout movement failed");
            }
        }
        
        /// <summary>
        /// STATE CONTROLS: Handle item placement
        /// </summary>
        private void HandleItemPlacement()
        {
            // Check if it's our turn (not waiting for checkout turn)
            if (!customer.WaitingForCheckoutTurn && !customer.IsInQueue)
            {
                Debug.Log($"{customer.name} placing items on counter");
                customer.StartCoroutine(PlaceItemsOnCounter());
                hasPlacedItems = true;
            }
            else
            {
                // Safety timeout for waiting
                float waitTime = Time.time - purchaseStartTime;
                if (waitTime > MAX_WAIT_TIME)
                {
                    Debug.LogWarning($"{customer.name} has been waiting for {waitTime:F1}s - CheckoutCounter may not support queue callbacks. Proceeding anyway.");
                    customer.SetWaitingForCheckout(false);
                    // Force clear queue state
                    if (customer.IsInQueue && customer.QueuedCheckout != null)
                    {
                        customer.ForceLeaveQueue();
                    }
                    hasPlacedItems = false; // Retry placement next frame
                }
            }
        }
        
        /// <summary>
        /// STATE CONTROLS: Handle transaction completion
        /// </summary>
        private void HandleTransactionCompletion()
        {
            // Set flag to indicate we're waiting for checkout if not already set
            if (!customer.IsWaitingForCheckout)
            {
                customer.SetWaitingForCheckout(true);
                Debug.Log($"{customer.name} waiting for checkout completion");
            }
            
            // Wait for checkout to complete (OnCheckoutCompleted will be called)
            // This mirrors the legacy coroutine behavior exactly
            if (!customer.IsWaitingForCheckout)
            {
                hasCompletedTransaction = true;
                Debug.Log($"{customer.name} transaction completed");
                
                // Mark products as purchased
                foreach (Product product in customer.SelectedProducts)
                {
                    if (product != null)
                    {
                        product.Purchase();
                        Debug.Log($"{customer.name} purchased {product.ProductData?.ProductName ?? "Product"} successfully");
                    }
                }
                
                // Notify checkout counter
                if (targetCheckoutCounter != null)
                {
                    targetCheckoutCounter.OnCustomerDeparture();
                }
            }
            
            // Safety check - if checkout counter becomes null or inactive, stop waiting
            if (targetCheckoutCounter == null || !targetCheckoutCounter.gameObject.activeInHierarchy)
            {
                Debug.LogWarning($"{customer.name} checkout counter became invalid, completing transaction anyway");
                hasCompletedTransaction = true;
                customer.SetWaitingForCheckout(false);
            }
        }
        
        /// <summary>
        /// STATE CONTROLS: Place items on checkout counter
        /// </summary>
        private IEnumerator PlaceItemsOnCounter()
        {
            if (targetCheckoutCounter == null)
            {
                Debug.LogWarning($"{customer.name} no checkout counter to place items on");
                yield break;
            }
            
            // Safety check: Only place items if we're not waiting for our turn and not in queue
            if (customer.WaitingForCheckoutTurn)
            {
                Debug.LogWarning($"{customer.name} attempted to place items while waiting for checkout turn - blocking!");
                yield break;
            }
            
            if (customer.IsInQueue)
            {
                Debug.LogWarning($"{customer.name} attempted to place items while still in queue - blocking!");
                yield break;
            }
            
            Debug.Log($"{customer.name} placing {customer.SelectedProducts.Count} items on counter");
            
            for (int i = 0; i < customer.SelectedProducts.Count; i++)
            {
                Product product = customer.SelectedProducts[i];
                if (product != null)
                {
                    Debug.Log($"{customer.name} placing product {i + 1}/{customer.SelectedProducts.Count}: {product.ProductData?.ProductName ?? product.name}");
                    
                    // Place the product at checkout with customer association
                    targetCheckoutCounter.PlaceProduct(product, customer.GetMainCustomer());
                    
                    // Ensure product retains its IInteractable interface
                    ProductInteraction productInteraction = product.GetComponent<ProductInteraction>();
                    if (productInteraction != null)
                    {
                        productInteraction.UpdateInteractionState();
                        Debug.Log($"Updated interaction state for {product.ProductData?.ProductName ?? product.name} - CanInteract: {product.CanInteract}");
                    }
                    else
                    {
                        Debug.LogWarning($"ProductInteraction component missing on {product.ProductData?.ProductName ?? product.name}");
                    }
                    
                    // Add to placed products list for tracking
                    customer.AddPlacedProduct(product);
                    
                    // Small delay between placements for natural look
                    yield return new WaitForSeconds(0.5f);
                }
            }
            
            Debug.Log($"{customer.name} finished placing {customer.PlacedOnCounterProducts.Count} items on counter");
            
            // Force UI refresh to ensure all products are visible
            targetCheckoutCounter.RefreshUI();
        }
        
        /// <summary>
        /// STATE HELPER: Find nearest checkout counter
        /// </summary>
        private CheckoutCounter FindNearestCheckoutCounter()
        {
            CheckoutCounter[] checkoutCounters = Object.FindObjectsByType<CheckoutCounter>(FindObjectsSortMode.None);
            CheckoutCounter nearest = null;
            float closestDistance = float.MaxValue;
            
            foreach (CheckoutCounter counter in checkoutCounters)
            {
                if (counter != null)
                {
                    float distance = Vector3.Distance(customer.transform.position, counter.transform.position);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        nearest = counter;
                    }
                }
            }
            
            return nearest;
        }
    }
}
