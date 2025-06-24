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
            
            // STATE DECIDES: Store closing check
            if (!IsStoreOpen())
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
            var movement = customer.GetComponent<CustomerMovement>();
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
            var movement = customer.GetComponent<CustomerMovement>();
            if (movement == null) return;
            
            if (movement.HasReachedDestination())
            {
                hasReachedCheckout = true;
                Debug.Log($"{customer.name} reached checkout area");
                
                // Join queue if we have a target checkout
                if (targetCheckoutCounter != null)
                {
                    // This will trigger the queue system
                    targetCheckoutCounter.OnCustomerArrival(customer.GetComponent<Customer>());
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
        }
        
        /// <summary>
        /// STATE CONTROLS: Handle transaction completion
        /// </summary>
        private void HandleTransactionCompletion()
        {
            // Wait for checkout to complete
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
                    }
                }
                
                // Notify checkout counter
                if (targetCheckoutCounter != null)
                {
                    targetCheckoutCounter.OnCustomerDeparture();
                }
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
            
            Debug.Log($"{customer.name} placing {customer.SelectedProducts.Count} items on counter");
            
            for (int i = 0; i < customer.SelectedProducts.Count; i++)
            {
                Product product = customer.SelectedProducts[i];
                if (product != null)
                {
                    targetCheckoutCounter.PlaceProduct(product, customer.GetComponent<Customer>());
                    yield return new WaitForSeconds(0.5f);
                }
            }
            
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
