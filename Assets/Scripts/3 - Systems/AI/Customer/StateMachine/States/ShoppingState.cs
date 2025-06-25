using UnityEngine;
using System.Collections;

namespace TabletopShop
{
    /// <summary>
    /// SHOPPING STATE - Controls all shopping behavior
    /// </summary>
    public class ShoppingState : BaseCustomerState
    {
        private float shoppingStartTime;
        private float lastProductCheckTime;
        private float lastShelfSwitchTime;
        private float shelfArrivalTime;
        private bool hasArrivedAtCurrentShelf = false;
        private ShelfSlot currentBrowsingShelf = null;
        private bool isWaitingAfterShelfSwitch = false;
        private float shelfSwitchWaitStartTime;
        
        private const float PRODUCT_CHECK_INTERVAL = 3f;
        private const float SHELF_SWITCH_PROBABILITY = 0.3f;
        private const float SHELF_SWITCH_COOLDOWN = 2f;
        private const float MINIMUM_BROWSE_TIME = 5f; // Minimum time to spend at each shelf
        private const float BROWSE_CHECK_INTERVAL = 1f; // How often to check while browsing
        private const float POST_SWITCH_WAIT_TIME = 2f; // Wait time after shelf switch (matches legacy)
        
        public override void OnEnter(CustomerBehavior customer)
        {
            this.customer = customer;
            shoppingStartTime = Time.time;
            lastProductCheckTime = Time.time;
            lastShelfSwitchTime = Time.time;
            shelfArrivalTime = Time.time;
            hasArrivedAtCurrentShelf = false;
            currentBrowsingShelf = customer.TargetShelf; // Start with current target shelf
            isWaitingAfterShelfSwitch = false;
            shelfSwitchWaitStartTime = 0f;
            
            Debug.Log($"{customer.name} started shopping for {customer.ShoppingTime:F1}s");
        }
        
        public override void OnUpdate(CustomerBehavior customer)
        {
            float shoppedTime = Time.time - shoppingStartTime;
            float targetShoppingTime = customer.ShoppingTime;
            
            // STATE DECIDES: Check if store is closing
            if (!IsStoreOpen() || ShouldLeaveStoreDueToHours())
            {
                Debug.Log($"{customer.name}: Store closed during shopping");
                RequestTransition(CustomerState.Leaving, "Store closed");
                return;
            }
            
            // STATE DECIDES: Check if should hurry up
            if (IsStoreClosingSoon())
            {
                targetShoppingTime *= 0.7f; // Reduce shopping time
                Debug.Log($"{customer.name}: Store closing soon - hurrying up");
            }
            
            // STATE DECIDES: Check if shopping time is complete
            if (shoppedTime >= targetShoppingTime)
            {
                Debug.Log($"{customer.name}: Shopping time complete ({shoppedTime:F1}s)");
                RequestTransition(CustomerState.Purchasing, "Shopping time complete");
                return;
            }
            
            // STATE CONTROLS: Handle shelf browsing and product selection
            // Use proper timing to match legacy "yield return new WaitForSeconds(1f)" behavior
            HandleShoppingBehavior(shoppedTime);
        }
        
        public override void OnExit(CustomerBehavior customer)
        {
            float totalShoppingTime = Time.time - shoppingStartTime;
            Debug.Log($"{customer.name} finished shopping after {totalShoppingTime:F1}s with {customer.SelectedProducts.Count} products");
        }
        
        /// <summary>
        /// STATE CONTROLS: Unified shopping behavior - matches legacy coroutine exactly
        /// </summary>
        private void HandleShoppingBehavior(float shoppedTime)
        {
            // Handle shelf browsing first
            HandleShelfBrowsing(shoppedTime);
            
            // Handle product selection (only when properly positioned at shelf)
            HandleProductSelection(shoppedTime);
        }
        
        /// <summary>
        /// STATE CONTROLS: Handle shelf browsing and movement - matches legacy coroutine behavior
        /// </summary>
        private void HandleShelfBrowsing(float shoppedTime)
        {
            var movement = customer.GetMovement();
            if (movement == null) return;
            
            // Handle post-switch waiting period (matches legacy "yield return new WaitForSeconds(2f)")
            if (isWaitingAfterShelfSwitch)
            {
                float waitTime = Time.time - shelfSwitchWaitStartTime;
                if (waitTime < POST_SWITCH_WAIT_TIME)
                {
                    return; // Still waiting after shelf switch
                }
                else
                {
                    isWaitingAfterShelfSwitch = false;
                    Debug.Log($"{customer.name} finished post-switch wait ({waitTime:F1}s) - resuming movement");
                }
            }
            
            // Check if customer has arrived at current shelf
            if (!hasArrivedAtCurrentShelf && movement.HasReachedDestination())
            {
                hasArrivedAtCurrentShelf = true;
                shelfArrivalTime = Time.time;
                currentBrowsingShelf = customer.TargetShelf;
                Debug.Log($"{customer.name} arrived at shelf: {currentBrowsingShelf?.name ?? "Unknown"} - starting browse period");
                return; // Don't immediately switch, give time to browse
            }
            
            // If we're browsing at a shelf, enforce minimum browse time
            if (hasArrivedAtCurrentShelf)
            {
                float browsingTime = Time.time - shelfArrivalTime;
                
                // Must browse for minimum time before considering a switch
                if (browsingTime < MINIMUM_BROWSE_TIME)
                {
                    return; // Stay at current shelf, continue browsing
                }
                
                // After minimum browse time, occasionally switch shelves (30% chance)
                // This matches the legacy "if (UnityEngine.Random.value < 0.3f)" check
                if (Time.time - lastShelfSwitchTime > BROWSE_CHECK_INTERVAL && 
                    UnityEngine.Random.value < SHELF_SWITCH_PROBABILITY)
                {
                    Debug.Log($"{customer.name} finished browsing {currentBrowsingShelf?.name ?? "Unknown"} after {browsingTime:F1}s - switching shelves");
                    SwitchToRandomShelf();
                }
            }
            else if (!movement.IsMoving && !isWaitingAfterShelfSwitch)
            {
            // If we're not moving and haven't arrived, try to get to a shelf
                if (customer.TargetShelf == null || Time.time - lastShelfSwitchTime > SHELF_SWITCH_COOLDOWN)
                {
                    SwitchToRandomShelf();
                }
            }
        }
        
        /// <summary>
        /// STATE CONTROLS: Switch to a random shelf - reset browsing state
        /// </summary>
        private void SwitchToRandomShelf()
        {
            ShelfSlot[] availableShelves = Object.FindObjectsByType<ShelfSlot>(FindObjectsSortMode.None);
            
            if (availableShelves.Length == 0) return;
            
            ShelfSlot randomShelf = availableShelves[UnityEngine.Random.Range(0, availableShelves.Length)];
            customer.SetTargetShelf(randomShelf);
            
            var movement = customer.GetMovement();
            if (movement != null && movement.MoveToShelfPosition(randomShelf))
            {
                // Reset browsing state for new shelf
                lastShelfSwitchTime = Time.time;
                hasArrivedAtCurrentShelf = false;
                shelfArrivalTime = Time.time;
                currentBrowsingShelf = null;
                
                // Start post-switch waiting period (matches legacy 2-second wait)
                isWaitingAfterShelfSwitch = true;
                shelfSwitchWaitStartTime = Time.time;
                
                Debug.Log($"{customer.name} switching to shelf: {randomShelf.name} - waiting {POST_SWITCH_WAIT_TIME}s before movement");
            }
        }
        
        /// <summary>
        /// STATE CONTROLS: Handle product selection - matches legacy timing exactly
        /// </summary>
        private void HandleProductSelection(float shoppedTime)
        {
            // Only check for products if we've arrived at a shelf and spent some time there
            // This matches the legacy behavior where products are checked "every few seconds" during browsing
            if (hasArrivedAtCurrentShelf && customer.TargetShelf != null)
            {
                float browsingTime = Time.time - shelfArrivalTime;
                
                // Start checking for products after a brief browsing delay (like legacy 2-second wait)
                if (browsingTime >= 2f && shoppedTime - lastProductCheckTime >= PRODUCT_CHECK_INTERVAL)
                {
                    lastProductCheckTime = shoppedTime;
                    
                    Debug.Log($"{customer.name} checking for products at {customer.TargetShelf.name} (browsed for {browsingTime:F1}s)");
                    TrySelectProductsAtShelf(customer.TargetShelf);
                }
            }
        }
        
        /// <summary>
        /// STATE CONTROLS: Try to select products at a shelf
        /// </summary>
        private void TrySelectProductsAtShelf(ShelfSlot shelf)
        {
            if (shelf == null || shelf.IsEmpty || shelf.CurrentProduct == null) return;
            
            Product availableProduct = shelf.CurrentProduct;
            
            // Check if customer can afford and wants this product
            bool canAfford = CanAffordProduct(availableProduct);
            bool wantsProduct = WantsProduct(availableProduct);
            
            if (canAfford && wantsProduct)
            {
                // Add to selected products and update total
                customer.AddSelectedProduct(availableProduct);
                customer.UpdateTotalPurchaseAmount(availableProduct.CurrentPrice);
                
                // Remove product from shelf
                Product removedProduct = shelf.RemoveProduct();
                if (removedProduct != null && removedProduct.IsOnShelf)
                {
                    removedProduct.RemoveFromShelf();
                }
                
                Debug.Log($"{customer.name} selected {availableProduct.ProductData?.ProductName ?? "Product"} for ${availableProduct.CurrentPrice} (Total: ${customer.TotalPurchaseAmount:F2})");
                
                // Start following customer
                customer.StartCoroutine(AttachProductToCustomer(availableProduct));
            }
        }
        
        /// <summary>
        /// STATE CHECKS: Can customer afford product
        /// </summary>
        private bool CanAffordProduct(Product product)
        {
            if (product == null) return false;
            
            float remainingBudget = customer.BaseSpendingPower - customer.TotalPurchaseAmount;
            return product.CurrentPrice <= remainingBudget;
        }
        
        /// <summary>
        /// STATE CHECKS: Does customer want this product
        /// </summary>
        private bool WantsProduct(Product product)
        {
            if (product == null || product.IsPurchased || !product.IsOnShelf) return false;
            
            // Use the same probability as legacy implementation
            float randomValue = UnityEngine.Random.value;
            bool wants = randomValue <= customer.PurchaseProbability;
            
            Debug.Log($"{customer.name} - Random value: {randomValue:F3}, Purchase probability: {customer.PurchaseProbability:F3}, Wants product: {wants}");
            
            return wants;
        }
        
        /// <summary>
        /// STATE CONTROLS: Make product follow customer
        /// </summary>
        private IEnumerator AttachProductToCustomer(Product product)
        {
            if (product == null) yield break;
            
            Vector3 offset = new Vector3(0.3f, 1.2f, 0.2f);
            
            while (customer != null && product != null && 
                   customer.SelectedProducts.Contains(product) && 
                   !customer.PlacedOnCounterProducts.Contains(product))
            {
                if (product.transform != null && customer.transform != null)
                {
                    product.transform.position = Vector3.Lerp(
                        product.transform.position,
                        customer.transform.position + customer.transform.rotation * offset,
                        Time.deltaTime * 5f);
                }
                
                yield return null;
            }
        }
    }
}
