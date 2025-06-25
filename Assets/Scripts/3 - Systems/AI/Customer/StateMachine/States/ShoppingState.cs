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
            lastProductCheckTime = Time.time - PRODUCT_CHECK_INTERVAL; // Initialize so first check can happen immediately
            lastShelfSwitchTime = Time.time;
            shelfArrivalTime = Time.time;
            hasArrivedAtCurrentShelf = false;
            currentBrowsingShelf = customer.TargetShelf; // Start with current target shelf
            isWaitingAfterShelfSwitch = false;
            shelfSwitchWaitStartTime = 0f;
            
            Debug.Log($"{customer.name} started shopping for {customer.ShoppingTime:F1}s");
            Debug.Log($"[PRODUCT-DEBUG] {customer.name}: Initialized lastProductCheckTime to {lastProductCheckTime:F1}s (allows immediate check after delays)");
        }
        
        public override void OnUpdate(CustomerBehavior customer)
        {
            float shoppedTime = Time.time - shoppingStartTime;
            float targetShoppingTime = customer.ShoppingTime;
            
            // [DEBUG] Frame-by-frame state tracking (every 60 frames = ~1 second)
            if (Time.frameCount % 60 == 0)
            {
                var movement = customer.GetMovement();
                Debug.Log($"[SHOPPING-DEBUG] {customer.name} Frame {Time.frameCount}:");
                Debug.Log($"  Time: {shoppedTime:F1}s / {targetShoppingTime:F1}s | Products: {customer.SelectedProducts.Count}");
                Debug.Log($"  hasArrivedAtShelf: {hasArrivedAtCurrentShelf} | isWaiting: {isWaitingAfterShelfSwitch}");
                Debug.Log($"  currentShelf: {currentBrowsingShelf?.name ?? "None"} | targetShelf: {customer.TargetShelf?.name ?? "None"}");
                Debug.Log($"  movement.IsMoving: {movement?.IsMoving ?? false} | movement.HasReached: {movement?.HasReachedDestination() ?? false}");
                Debug.Log($"  browsingTime: {(hasArrivedAtCurrentShelf ? Time.time - shelfArrivalTime : 0):F1}s | lastProductCheck: {Time.time - lastProductCheckTime:F1}s ago");
            }
            
            // STATE DECIDES: Check if store is closing
            bool storeOpen = IsStoreOpen();
            bool shouldLeave = ShouldLeaveStoreDueToHours();
            if (!storeOpen || shouldLeave)
            {
                Debug.Log($"[SHOPPING-DEBUG] {customer.name}: Store state - Open: {storeOpen}, ShouldLeave: {shouldLeave}");
                RequestTransition(CustomerState.Leaving, "Store closed");
                return;
            }
            
            // STATE DECIDES: Check if should hurry up
            bool storeSoon = IsStoreClosingSoon();
            if (storeSoon)
            {
                float originalTime = targetShoppingTime;
                targetShoppingTime *= 0.7f; // Reduce shopping time
                Debug.Log($"[SHOPPING-DEBUG] {customer.name}: Store closing soon - reducing time from {originalTime:F1}s to {targetShoppingTime:F1}s");
            }
            
            // STATE DECIDES: Check if shopping time is complete
            if (shoppedTime >= targetShoppingTime)
            {
                Debug.Log($"[SHOPPING-DEBUG] {customer.name}: Shopping time complete ({shoppedTime:F1}s >= {targetShoppingTime:F1}s) with {customer.SelectedProducts.Count} products");
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
            if (movement == null) 
            {
                Debug.LogError($"[SHELF-DEBUG] {customer.name}: Movement component is NULL!");
                return;
            }
            
            Debug.Log($"[SHELF-DEBUG] {customer.name}: HandleShelfBrowsing called - hasArrived: {hasArrivedAtCurrentShelf}, isWaiting: {isWaitingAfterShelfSwitch}");
            Debug.Log($"[SHELF-DEBUG] {customer.name}: Movement state - IsMoving: {movement.IsMoving}, HasReached: {movement.HasReachedDestination()}");
            Debug.Log($"[SHELF-DEBUG] {customer.name}: Current destination: {movement.CurrentDestination}, Has destination: {movement.HasDestination}");
            
            // Handle post-switch waiting period (matches legacy "yield return new WaitForSeconds(2f)")
            if (isWaitingAfterShelfSwitch)
            {
                float waitTime = Time.time - shelfSwitchWaitStartTime;
                Debug.Log($"[SHELF-DEBUG] {customer.name}: Post-switch wait - {waitTime:F1}s / {POST_SWITCH_WAIT_TIME:F1}s");
                
                if (waitTime < POST_SWITCH_WAIT_TIME)
                {
                    Debug.Log($"[SHELF-DEBUG] {customer.name}: Still waiting after shelf switch ({waitTime:F1}s < {POST_SWITCH_WAIT_TIME:F1}s)");
                    return; // Still waiting after shelf switch
                }
                else
                {
                    isWaitingAfterShelfSwitch = false;
                    Debug.Log($"[SHELF-DEBUG] {customer.name}: ✓ Finished post-switch wait ({waitTime:F1}s) - resuming movement");
                }
            }
            
            // Check if customer has arrived at current shelf
            bool hasReachedDest = movement.HasReachedDestination();
            Debug.Log($"[SHELF-DEBUG] {customer.name}: Arrival check - hasArrivedAtShelf: {hasArrivedAtCurrentShelf}, HasReachedDestination: {hasReachedDest}");
            
            if (!hasArrivedAtCurrentShelf && hasReachedDest)
            {
                hasArrivedAtCurrentShelf = true;
                shelfArrivalTime = Time.time;
                currentBrowsingShelf = customer.TargetShelf;
                Debug.Log($"[SHELF-DEBUG] {customer.name}: ✓ ARRIVED at shelf: {currentBrowsingShelf?.name ?? "Unknown"} at time {shelfArrivalTime:F1}s");
                Debug.Log($"[SHELF-DEBUG] {customer.name}: Target shelf has product: {currentBrowsingShelf?.CurrentProduct?.ProductData?.ProductName ?? "None"}");
                return; // Don't immediately switch, give time to browse
            }
            
            // If we're browsing at a shelf, enforce minimum browse time
            if (hasArrivedAtCurrentShelf)
            {
                float browsingTime = Time.time - shelfArrivalTime;
                Debug.Log($"[SHELF-DEBUG] {customer.name}: Browsing at shelf for {browsingTime:F1}s (min: {MINIMUM_BROWSE_TIME:F1}s)");
                
                // Must browse for minimum time before considering a switch
                if (browsingTime < MINIMUM_BROWSE_TIME)
                {
                    Debug.Log($"[SHELF-DEBUG] {customer.name}: Continuing to browse ({browsingTime:F1}s < {MINIMUM_BROWSE_TIME:F1}s)");
                    return; // Stay at current shelf, continue browsing
                }
                
                // After minimum browse time, occasionally switch shelves (30% chance)
                float timeSinceLastSwitch = Time.time - lastShelfSwitchTime;
                bool intervalMet = timeSinceLastSwitch > BROWSE_CHECK_INTERVAL;
                float randomValue = UnityEngine.Random.value;
                bool shouldSwitch = randomValue < SHELF_SWITCH_PROBABILITY;
                
                Debug.Log($"[SHELF-DEBUG] {customer.name}: Switch check - interval: {timeSinceLastSwitch:F1}s > {BROWSE_CHECK_INTERVAL:F1}s = {intervalMet}, random: {randomValue:F3} < {SHELF_SWITCH_PROBABILITY:F3} = {shouldSwitch}");
                
                if (intervalMet && shouldSwitch)
                {
                    Debug.Log($"[SHELF-DEBUG] {customer.name}: ✓ Switching shelves after browsing {currentBrowsingShelf?.name ?? "Unknown"} for {browsingTime:F1}s");
                    SwitchToRandomShelf();
                }
                else
                {
                    Debug.Log($"[SHELF-DEBUG] {customer.name}: Staying at current shelf - interval: {intervalMet}, shouldSwitch: {shouldSwitch}");
                }
            }
            else if (!movement.IsMoving && !isWaitingAfterShelfSwitch)
            {
                Debug.Log($"[SHELF-DEBUG] {customer.name}: Not moving and not waiting - checking if need new destination");
                
                bool noTarget = customer.TargetShelf == null;
                float cooldownTime = Time.time - lastShelfSwitchTime;
                bool cooldownMet = cooldownTime > SHELF_SWITCH_COOLDOWN;
                
                Debug.Log($"[SHELF-DEBUG] {customer.name}: No target: {noTarget}, cooldown: {cooldownTime:F1}s > {SHELF_SWITCH_COOLDOWN:F1}s = {cooldownMet}");
                
                if (noTarget || cooldownMet)
                {
                    Debug.Log($"[SHELF-DEBUG] {customer.name}: ✓ Need new shelf destination");
                    SwitchToRandomShelf();
                }
            }
            else
            {
                Debug.Log($"[SHELF-DEBUG] {customer.name}: Currently moving or waiting - IsMoving: {movement.IsMoving}, isWaiting: {isWaitingAfterShelfSwitch}");
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
            Debug.Log($"[PRODUCT-DEBUG] {customer.name}: HandleProductSelection called with shoppedTime: {shoppedTime:F1}s");
            
            // Check all conditions for product selection
            bool arrivedAtShelf = hasArrivedAtCurrentShelf;
            bool hasTargetShelf = customer.TargetShelf != null;
            
            Debug.Log($"[PRODUCT-DEBUG] {customer.name}: Initial checks - arrivedAtShelf: {arrivedAtShelf}, hasTargetShelf: {hasTargetShelf}");
            
            if (!arrivedAtShelf)
            {
                Debug.Log($"[PRODUCT-DEBUG] {customer.name}: ❌ Not arrived at shelf yet - skipping product check");
                return;
            }
            
            if (!hasTargetShelf)
            {
                Debug.Log($"[PRODUCT-DEBUG] {customer.name}: ❌ No target shelf - skipping product check");
                return;
            }
            
            // Calculate timing conditions
            float browsingTime = Time.time - shelfArrivalTime;
            float timeSinceLastCheck = Time.time - lastProductCheckTime;
            
            Debug.Log($"[PRODUCT-DEBUG] {customer.name}: Timing calculations:");
            Debug.Log($"  Current time: {Time.time:F1}s, Arrival time: {shelfArrivalTime:F1}s");
            Debug.Log($"  Browsing time: {browsingTime:F1}s (need >= 2.0s)");
            Debug.Log($"  Last check time: {lastProductCheckTime:F1}s, Time since: {timeSinceLastCheck:F1}s (need >= {PRODUCT_CHECK_INTERVAL:F1}s)");
            
            // Check browsing delay condition
            bool browsingDelayMet = browsingTime >= 2f;
            Debug.Log($"[PRODUCT-DEBUG] {customer.name}: Browsing delay check: {browsingTime:F1}s >= 2.0s = {browsingDelayMet}");
            
            if (!browsingDelayMet)
            {
                Debug.Log($"[PRODUCT-DEBUG] {customer.name}: ❌ Browsing delay not met - need to wait {2f - browsingTime:F1}s more");
                return;
            }
            
            // Check interval condition
            bool intervalMet = timeSinceLastCheck >= PRODUCT_CHECK_INTERVAL;
            Debug.Log($"[PRODUCT-DEBUG] {customer.name}: Interval check: {timeSinceLastCheck:F1}s >= {PRODUCT_CHECK_INTERVAL:F1}s = {intervalMet}");
            
            if (!intervalMet)
            {
                Debug.Log($"[PRODUCT-DEBUG] {customer.name}: ❌ Check interval not met - need to wait {PRODUCT_CHECK_INTERVAL - timeSinceLastCheck:F1}s more");
                return;
            }
            
            // All conditions met - proceed with product check
            lastProductCheckTime = Time.time;
            Debug.Log($"[PRODUCT-DEBUG] {customer.name}: ✓ ALL CONDITIONS MET - proceeding with product check at {customer.TargetShelf.name}");
            Debug.Log($"[PRODUCT-DEBUG] {customer.name}: Updated lastProductCheckTime to {lastProductCheckTime:F1}s (absolute time)");
            
            TrySelectProductsAtShelf(customer.TargetShelf);
        }
        
        /// <summary>
        /// STATE CONTROLS: Try to select products at a shelf
        /// </summary>
        private void TrySelectProductsAtShelf(ShelfSlot shelf)
        {
            Debug.Log($"[SELECT-DEBUG] {customer.name}: ======= TrySelectProductsAtShelf CALLED =======");
            
            // STEP 1: Validate shelf
            if (shelf == null)
            {
                Debug.LogError($"[SELECT-DEBUG] {customer.name}: ❌ FAILED - Shelf is NULL!");
                return;
            }
            
            Debug.Log($"[SELECT-DEBUG] {customer.name}: ✓ Shelf exists: {shelf.name}");
            Debug.Log($"[SELECT-DEBUG] {customer.name}: Shelf.IsEmpty: {shelf.IsEmpty}");
            Debug.Log($"[SELECT-DEBUG] {customer.name}: Shelf.CurrentProduct: {(shelf.CurrentProduct != null ? "EXISTS" : "NULL")}");
            
            if (shelf.IsEmpty)
            {
                Debug.Log($"[SELECT-DEBUG] {customer.name}: ❌ FAILED - Shelf {shelf.name} is EMPTY!");
                return;
            }
            
            if (shelf.CurrentProduct == null)
            {
                Debug.LogError($"[SELECT-DEBUG] {customer.name}: ❌ FAILED - Shelf {shelf.name} reports not empty but CurrentProduct is NULL!");
                return;
            }
            
            // STEP 2: Get product details
            Product availableProduct = shelf.CurrentProduct;
            string productName = availableProduct.ProductData?.ProductName ?? "Unknown Product";
            float productPrice = availableProduct.CurrentPrice;
            
            Debug.Log($"[SELECT-DEBUG] {customer.name}: ✓ Product found: {productName}");
            Debug.Log($"[SELECT-DEBUG] {customer.name}: Product price: ${productPrice:F2}");
            Debug.Log($"[SELECT-DEBUG] {customer.name}: Product.IsPurchased: {availableProduct.IsPurchased}");
            Debug.Log($"[SELECT-DEBUG] {customer.name}: Product.IsOnShelf: {availableProduct.IsOnShelf}");
            
            // STEP 3: Check customer's current state
            float currentTotal = customer.TotalPurchaseAmount;
            float baseBudget = customer.BaseSpendingPower;
            int currentProducts = customer.SelectedProducts.Count;
            float purchaseProb = customer.PurchaseProbability;
            
            Debug.Log($"[SELECT-DEBUG] {customer.name}: === CUSTOMER STATE ===");
            Debug.Log($"[SELECT-DEBUG] {customer.name}: Current total spent: ${currentTotal:F2}");
            Debug.Log($"[SELECT-DEBUG] {customer.name}: Base spending power: ${baseBudget:F2}");
            Debug.Log($"[SELECT-DEBUG] {customer.name}: Current products: {currentProducts}");
            Debug.Log($"[SELECT-DEBUG] {customer.name}: Purchase probability: {purchaseProb:F3}");
            
            // STEP 4: Check affordability
            bool canAfford = CanAffordProduct(availableProduct);
            float remainingBudget = baseBudget - currentTotal;
            float afterPurchaseTotal = currentTotal + productPrice;
            
            Debug.Log($"[SELECT-DEBUG] {customer.name}: === AFFORDABILITY CHECK ===");
            Debug.Log($"[SELECT-DEBUG] {customer.name}: Remaining budget: ${remainingBudget:F2}");
            Debug.Log($"[SELECT-DEBUG] {customer.name}: Product price: ${productPrice:F2}");
            Debug.Log($"[SELECT-DEBUG] {customer.name}: Can afford: {productPrice:F2} <= {remainingBudget:F2} = {canAfford}");
            Debug.Log($"[SELECT-DEBUG] {customer.name}: Total after purchase would be: ${afterPurchaseTotal:F2}");
            
            if (!canAfford)
            {
                Debug.Log($"[SELECT-DEBUG] {customer.name}: ❌ FAILED - Cannot afford product!");
                return;
            }
            
            // STEP 5: Check want logic
            bool wantsProduct = WantsProduct(availableProduct);
            Debug.Log($"[SELECT-DEBUG] {customer.name}: === WANT CHECK RESULT ===");
            Debug.Log($"[SELECT-DEBUG] {customer.name}: Wants product: {wantsProduct}");
            
            if (!wantsProduct)
            {
                Debug.Log($"[SELECT-DEBUG] {customer.name}: ❌ FAILED - Customer doesn't want this product!");
                return;
            }
            
            // STEP 6: Product selection SUCCESS!
            Debug.Log($"[SELECT-DEBUG] {customer.name}: ✅ ✅ ✅ PRODUCT SELECTION SUCCESS! ✅ ✅ ✅");
            Debug.Log($"[SELECT-DEBUG] {customer.name}: Selected: {productName} for ${productPrice:F2}");
            
            // Add to selected products and update total
            int productsBefore = customer.SelectedProducts.Count;
            float totalBefore = customer.TotalPurchaseAmount;
            
            customer.AddSelectedProduct(availableProduct);
            customer.UpdateTotalPurchaseAmount(availableProduct.CurrentPrice);
            
            int productsAfter = customer.SelectedProducts.Count;
            float totalAfter = customer.TotalPurchaseAmount;
            
            Debug.Log($"[SELECT-DEBUG] {customer.name}: Products: {productsBefore} → {productsAfter}");
            Debug.Log($"[SELECT-DEBUG] {customer.name}: Total: ${totalBefore:F2} → ${totalAfter:F2}");
            
            // Remove product from shelf
            Debug.Log($"[SELECT-DEBUG] {customer.name}: Removing product from shelf...");
            Product removedProduct = shelf.RemoveProduct();
            
            if (removedProduct == null)
            {
                Debug.LogError($"[SELECT-DEBUG] {customer.name}: ⚠️ WARNING - RemoveProduct returned NULL!");
            }
            else
            {
                Debug.Log($"[SELECT-DEBUG] {customer.name}: ✓ Product removed from shelf");
                Debug.Log($"[SELECT-DEBUG] {customer.name}: Removed product.IsOnShelf: {removedProduct.IsOnShelf}");
                
                if (removedProduct.IsOnShelf)
                {
                    Debug.Log($"[SELECT-DEBUG] {customer.name}: Product still marked as on shelf - calling RemoveFromShelf()");
                    removedProduct.RemoveFromShelf();
                    Debug.Log($"[SELECT-DEBUG] {customer.name}: After RemoveFromShelf - IsOnShelf: {removedProduct.IsOnShelf}");
                }
            }
            
            // Start following customer
            Debug.Log($"[SELECT-DEBUG] {customer.name}: Starting AttachProductToCustomer coroutine...");
            customer.StartCoroutine(AttachProductToCustomer(availableProduct));
            
            Debug.Log($"[SELECT-DEBUG] {customer.name}: ======= PRODUCT SELECTION COMPLETE =======");
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
            Debug.Log($"[WANT-DEBUG] {customer.name}: === WantsProduct CHECK ===");
            
            // Null check
            if (product == null)
            {
                Debug.Log($"[WANT-DEBUG] {customer.name}: ❌ Product is NULL - returning false");
                return false;
            }
            
            // Purchased check
            if (product.IsPurchased)
            {
                Debug.Log($"[WANT-DEBUG] {customer.name}: ❌ Product already purchased - returning false");
                return false;
            }
            
            // On shelf check
            if (!product.IsOnShelf)
            {
                Debug.Log($"[WANT-DEBUG] {customer.name}: ❌ Product not on shelf - returning false");
                return false;
            }
            
            Debug.Log($"[WANT-DEBUG] {customer.name}: ✓ Product passed basic checks");
            
            // Generate random value and check against probability
            float randomValue = UnityEngine.Random.value;
            float purchaseProb = customer.PurchaseProbability;
            bool wants = randomValue <= purchaseProb;
            
            Debug.Log($"[WANT-DEBUG] {customer.name}: === PROBABILITY CALCULATION ===");
            Debug.Log($"[WANT-DEBUG] {customer.name}: Random value: {randomValue:F3}");
            Debug.Log($"[WANT-DEBUG] {customer.name}: Purchase probability: {purchaseProb:F3}");
            Debug.Log($"[WANT-DEBUG] {customer.name}: Comparison: {randomValue:F3} <= {purchaseProb:F3} = {wants}");
            Debug.Log($"[WANT-DEBUG] {customer.name}: Result: {(wants ? "✅ WANTS" : "❌ DOESN'T WANT")} product");
            
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
