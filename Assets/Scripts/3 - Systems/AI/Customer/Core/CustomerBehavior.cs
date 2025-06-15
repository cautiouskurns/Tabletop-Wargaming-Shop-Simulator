using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace TabletopShop
{
    /// <summary>
    /// Handles customer AI behavior, lifecycle state machine, and shopping patterns.
    /// Manages the complete customer experience from entering to leaving the shop.
    /// </summary>
    public class CustomerBehavior : MonoBehaviour
    {
    [Header("State Management")]
    [SerializeField] private CustomerState currentState = CustomerState.Entering;
    
    [Header("Shopping Configuration")]
    [SerializeField] private float shoppingTime;
    [SerializeField] private ShelfSlot targetShelf;
    
    [Header("Purchase Configuration")]
    [SerializeField] private float baseSpendingPower = 100f;
    [SerializeField] private float purchaseProbability = 0.8f;
    
    // Component references
    private CustomerMovement customerMovement;
    private Customer mainCustomer; // Reference to main customer for state changes
    
    // State tracking
    private Coroutine lifecycleCoroutine;
    
    // Purchase tracking
    private List<Product> selectedProducts = new List<Product>();
    private float totalPurchaseAmount = 0f;
    
    // Checkout state tracking
    private bool isWaitingForCheckout = false;
    private List<Product> placedOnCounterProducts = new List<Product>();

        // Events
        public event Action<CustomerState, CustomerState> OnStateChangeRequested;
        public event Action<ShelfSlot> OnTargetShelfChanged;
        
        // Properties
        public CustomerState CurrentState => currentState;
        public float ShoppingTime => shoppingTime;
        public ShelfSlot TargetShelf => targetShelf;
        public List<Product> SelectedProducts => selectedProducts;
        public float TotalPurchaseAmount => totalPurchaseAmount;
        public float BaseSpendingPower => baseSpendingPower;
        
        #region Initialization
        
        private void Awake()
        {
            InitializeShoppingTime();
        }
        
        /// <summary>
        /// Initialize with component references
        /// </summary>
        public void Initialize(CustomerMovement movement, Customer customer)
        {
            customerMovement = movement;
            mainCustomer = customer;
        }
        
        /// <summary>
        /// Initialize random shopping time between 10-30 seconds
        /// </summary>
        private void InitializeShoppingTime()
        {
            shoppingTime = UnityEngine.Random.Range(10f, 30f);
        }
        
        /// <summary>
        /// Reset customer's shopping state (for reuse or testing)
        /// </summary>
        public void ResetShoppingState()
        {
            selectedProducts.Clear();
            placedOnCounterProducts.Clear();
            totalPurchaseAmount = 0f;
            targetShelf = null;
            InitializeShoppingTime();
            
            Debug.Log($"CustomerBehavior {name} shopping state reset");
        }
        
        #endregion
        
        #region State Management
        
        /// <summary>
        /// Change the customer state and notify listeners
        /// </summary>
        /// <param name="newState">The new state to transition to</param>
        public void ChangeState(CustomerState newState)
        {
            CustomerState previousState = currentState;
            currentState = newState;
            
            Debug.Log($"CustomerBehavior {name} state changed: {previousState} -> {currentState}");
            
            // Notify listeners of state change
            OnStateChangeRequested?.Invoke(previousState, currentState);
        }
        
        /// <summary>
        /// Get the current customer state
        /// </summary>
        /// <returns>Current CustomerState</returns>
        public CustomerState GetCurrentState()
        {
            return currentState;
        }
        
        /// <summary>
        /// Check if customer is currently in a specific state
        /// </summary>
        /// <param name="state">State to check</param>
        /// <returns>True if customer is in the specified state</returns>
        public bool IsInState(CustomerState state)
        {
            return currentState == state;
        }
        
        #endregion
        
        #region Customer Lifecycle State Machine
        
        /// <summary>
        /// Start the complete customer lifecycle automatically
        /// Progresses through: Entering → Shopping → Purchasing → Leaving
        /// ✅ MINIMAL VERSION TO TEST CRASH FIX
        /// </summary>
        public void StartCustomerLifecycle(CustomerState startingState)
        {
            Debug.Log($"CustomerBehavior {name}: Starting lifecycle in state {startingState}");
            
            // ✅ VALIDATE DEPENDENCIES FIRST
            if (customerMovement == null)
            {
                Debug.LogError($"CustomerBehavior {name}: Cannot start lifecycle - CustomerMovement not initialized!");
                return;
            }
            
            // ✅ JUST SET STATE - DON'T START COROUTINES YET TO TEST CRASH FIX
            ChangeState(startingState);
            
            Debug.Log($"CustomerBehavior {name}: State set successfully to {currentState}");
            
            // ✅ TEMPORARY: Start with minimal lifecycle to test crash fix
            if (lifecycleCoroutine != null)
            {
                StopCoroutine(lifecycleCoroutine);
            }
            
            // Start the lifecycle coroutine
            lifecycleCoroutine = StartCoroutine(CustomerLifecycleCoroutine(startingState));
            
            Debug.Log($"CustomerBehavior {name}: Lifecycle coroutine started successfully");
        }
        
        /// <summary>
        /// Stop the customer lifecycle
        /// </summary>
        public void StopCustomerLifecycle()
        {
            if (lifecycleCoroutine != null)
            {
                StopCoroutine(lifecycleCoroutine);
                lifecycleCoroutine = null;
            }
        }
        
        /// <summary>
        /// Coroutine to handle the complete customer lifecycle automatically
        /// Progresses through: Entering → Shopping → Purchasing → Leaving
        /// </summary>
        private IEnumerator CustomerLifecycleCoroutine(CustomerState initialState)
        {
            // Set the initial state
            ChangeState(initialState);
            
            Debug.Log($"CustomerBehavior {name} starting lifecycle in state: {currentState}");
            
            // Phase 1: Entering the shop
            if (currentState == CustomerState.Entering)
            {
                yield return StartCoroutine(HandleEnteringState());
                ChangeState(CustomerState.Shopping);
            }
            
            // Phase 2: Shopping behavior
            if (currentState == CustomerState.Shopping)
            {
                yield return StartCoroutine(HandleShoppingState());
                ChangeState(CustomerState.Purchasing);
            }
            
            // Phase 3: Purchasing (move to checkout area and make purchase)
            if (currentState == CustomerState.Purchasing)
            {
                yield return StartCoroutine(HandlePurchasingState());
                ChangeState(CustomerState.Leaving);
            }
            
            // Phase 4: Leaving the shop
            if (currentState == CustomerState.Leaving)
            {
                yield return StartCoroutine(HandleLeavingState());
            }
            
            Debug.Log($"CustomerBehavior {name} lifecycle completed");
        }
        
        /// <summary>
        /// Handle entering state behavior
        /// </summary>
        private IEnumerator HandleEnteringState()
        {
            Debug.Log($"CustomerBehavior {name} entering shop - looking for shelves");
            
            // Move to a random shelf to start shopping
            bool foundShelf = SetRandomShelfDestination();
            if (foundShelf)
            {
                // ✅ ADD NULL CHECK HERE
                while (customerMovement != null && !customerMovement.HasReachedDestination())
                {
                    yield return new WaitForSeconds(0.5f);
                }
            }
            else
            {
                Debug.LogWarning($"CustomerBehavior {name} couldn't find any shelves - skipping to leaving");
                // Force transition to leaving if no shelves found
                OnStateChangeRequested?.Invoke(CustomerState.Entering, CustomerState.Leaving);
                yield break;
            }
        }
        
        /// <summary>
        /// Handle shopping state behavior
        /// </summary>
        private IEnumerator HandleShoppingState()
        {
            Debug.Log($"CustomerBehavior {name} browsing products for {shoppingTime:F1} seconds");
            
            float shoppedTime = 0f;
            float lastProductCheckTime = 0f;
            
            while (shoppedTime < shoppingTime)
            {
                // Occasionally move to different shelves while shopping
                if (UnityEngine.Random.value < 0.3f) // 30% chance every check
                {
                    SetRandomShelfDestination();
                    
                    // Wait a bit for movement
                    yield return new WaitForSeconds(2f);
                    
                    // ✅ ADD NULL CHECK HERE
                    while (customerMovement != null && !customerMovement.HasReachedDestination())
                    {
                        yield return new WaitForSeconds(0.5f);
                    }
                }
                
                // Try to select products at current shelf every few seconds
                if (shoppedTime - lastProductCheckTime >= 3f)
                {
                    TrySelectProductsAtCurrentShelf();
                    lastProductCheckTime = shoppedTime;
                }
                
                yield return new WaitForSeconds(1f);
                shoppedTime += 1f;
            }
            
            Debug.Log($"CustomerBehavior {name} finished shopping. Selected {selectedProducts.Count} products totaling ${totalPurchaseAmount:F2}");
        }
        
        /// <summary>
        /// Handle purchasing state behavior
        /// Customer brings selected products to checkout counter and waits for completion
        /// Uses the new checkout counter workflow instead of direct purchase processing
        /// </summary>
        private IEnumerator HandlePurchasingState()
        {
            Debug.Log($"CustomerBehavior {name} proceeding to checkout with {selectedProducts.Count} products");
            
            // Move to checkout counter
            bool reachedCheckout = false;
            CheckoutCounter targetCheckoutCounter = null;
            
            if (customerMovement != null)
            {
                reachedCheckout = customerMovement.MoveToCheckoutPoint();
                
                // Find the checkout counter we're moving to
                targetCheckoutCounter = FindNearestCheckoutCounter();
            }
            else
            {
                Debug.LogError($"CustomerBehavior {name} cannot move to checkout - CustomerMovement component not found!");
                yield break;
            }
            
            if (reachedCheckout && targetCheckoutCounter != null)
            {
                // Wait until we reach the checkout counter
                while (customerMovement != null && !customerMovement.HasReachedDestination())
                {
                    yield return new WaitForSeconds(0.5f);
                }
                
                Debug.Log($"CustomerBehavior {name} has reached the checkout counter");
                
                // Notify the checkout counter that customer has arrived
                targetCheckoutCounter.OnCustomerArrival(GetComponent<Customer>());
                
                // Place items on the checkout counter
                yield return StartCoroutine(PlaceItemsOnCounter(targetCheckoutCounter));
                
                // Wait for checkout completion
                yield return StartCoroutine(WaitForCheckoutCompletion(targetCheckoutCounter));
                
                // Collect items and complete transaction
                yield return StartCoroutine(CollectItemsAndLeave(targetCheckoutCounter));
            }
            else
            {
                Debug.LogWarning($"CustomerBehavior {name} could not find checkout counter or failed to reach it");
                // Fallback to leaving without purchase
                ChangeState(CustomerState.Leaving);
            }
        }
        
        /// <summary>
        /// Handle leaving state behavior
        /// </summary>
        private IEnumerator HandleLeavingState()
        {
            Debug.Log($"CustomerBehavior {name} leaving the shop");
            
            // ✅ ADD NULL CHECK HERE
            bool foundExit = false;
            if (customerMovement != null)
            {
                foundExit = customerMovement.MoveToExitPoint();
            }
            else
            {
                Debug.LogError($"CustomerBehavior {name} cannot leave - CustomerMovement component not found!");
                yield break;
            }
            
            if (foundExit)
            {
                // ✅ ADD NULL CHECK HERE
                while (customerMovement != null && !customerMovement.HasReachedDestination())
                {
                    yield return new WaitForSeconds(0.5f);
                }
                
                Debug.Log($"CustomerBehavior {name} has left the shop");
                
                // Destroy all products the customer was carrying
                DestroyCustomerProducts();
                
                // Optional: Destroy customer object after leaving
                yield return new WaitForSeconds(2f);
                Debug.Log($"CustomerBehavior {name} cleanup - removing from scene");
                Destroy(gameObject);
            }
            else
            {
                Debug.LogError($"CustomerBehavior {name} couldn't find exit!");
            }
        }
        
        #endregion
        
        #region Target Management
        
        /// <summary>
        /// Set target shelf for shopping
        /// </summary>
        /// <param name="shelf">Target shelf slot</param>
        public void SetTargetShelf(ShelfSlot shelf)
        {
            targetShelf = shelf;
            OnTargetShelfChanged?.Invoke(shelf);
            
            if (shelf != null)
            {
                Debug.Log($"CustomerBehavior {name} targeting shelf: {shelf.name}");
            }
            else
            {
                Debug.Log($"CustomerBehavior {name} cleared target shelf");
            }
        }
        
        /// <summary>
        /// Clear current target shelf
        /// </summary>
        public void ClearTargetShelf()
        {
            SetTargetShelf(null);
        }
        
        /// <summary>
        /// Set destination to a random shelf in the scene and update target
        /// </summary>
        /// <returns>True if a random shelf destination was set successfully</returns>
        public bool SetRandomShelfDestination()
        {
            ShelfSlot[] availableShelves = FindObjectsByType<ShelfSlot>(FindObjectsSortMode.None);
            
            if (availableShelves.Length == 0)
            {
                Debug.LogWarning($"CustomerBehavior {name} cannot find any shelves in scene!");
                return false;
            }
            
            // Select random shelf
            ShelfSlot randomShelf = availableShelves[UnityEngine.Random.Range(0, availableShelves.Length)];
            SetTargetShelf(randomShelf);
            
            // ✅ ADD NULL CHECK HERE
            if (customerMovement != null)
            {
                return customerMovement.MoveToShelfPosition(randomShelf);
            }
            else
            {
                Debug.LogError($"CustomerBehavior {name} cannot move to shelf - CustomerMovement component not found!");
                return false;
            }
        }
        
        #endregion
        
        #region Shopping Behavior
        
        /// <summary>
        /// Perform shopping interaction at current shelf
        /// </summary>
        public void PerformShoppingInteraction()
        {
            if (targetShelf != null)
            {
                Debug.Log($"CustomerBehavior {name} interacting with shelf: {targetShelf.name}");
                // Here you could add specific shopping behaviors like:
                // - Examining products
                // - Picking up items
                // - Putting items back
                // - Making decisions based on preferences
            }
        }
        
        /// <summary>
        /// Try to select products at the current target shelf
        /// </summary>
        private void TrySelectProductsAtCurrentShelf()
        {
            if (targetShelf == null) 
            {
                Debug.Log($"CustomerBehavior {name} - No target shelf set for product selection");
                return;
            }
            
            Debug.Log($"CustomerBehavior {name} checking shelf {targetShelf.name} - IsEmpty: {targetShelf.IsEmpty}, HasCurrentProduct: {targetShelf.CurrentProduct != null}");
            
            // Check if the target shelf has any products
            if (!targetShelf.IsEmpty && targetShelf.CurrentProduct != null)
            {
                Product availableProduct = targetShelf.CurrentProduct;
                
                Debug.Log($"CustomerBehavior {name} found product: {availableProduct.ProductData?.ProductName ?? "Unknown"} - Price: ${availableProduct.CurrentPrice}, IsPurchased: {availableProduct.IsPurchased}, IsOnShelf: {availableProduct.IsOnShelf}");
                
                bool canAfford = CanAffordProduct(availableProduct);
                bool wantsProduct = WantsProduct(availableProduct);
                
                Debug.Log($"CustomerBehavior {name} - CanAfford: {canAfford}, WantsProduct: {wantsProduct}, RemainingBudget: ${baseSpendingPower - totalPurchaseAmount:F2}");
                
                // Check if customer can afford and wants this product
                if (canAfford && wantsProduct)
                {
                    // Add to selected products list before removing from shelf
                    selectedProducts.Add(availableProduct);
                    totalPurchaseAmount += availableProduct.CurrentPrice;
                    
                    // Remove product from shelf immediately - product is now in customer's possession
                    // Use ShelfSlot's RemoveProduct which handles notifying the product correctly
                    Product removedProduct = targetShelf.RemoveProduct();
                    
                    // Double-check the product was properly removed from the shelf
                    if (removedProduct != null && removedProduct.IsOnShelf)
                    {
                        Debug.LogWarning($"Product {removedProduct.ProductData?.ProductName} still marked as on shelf after removal. Forcing removal.");
                        removedProduct.RemoveFromShelf();
                    }
                    
                    // Optional: Update the product's position to follow the customer
                    // This visually shows the customer has the product
                    StartCoroutine(AttachProductToCustomer(availableProduct));
                    
                    Debug.Log($"CustomerBehavior {name} ✅ SELECTED {availableProduct.ProductData?.ProductName ?? "Product"} for ${availableProduct.CurrentPrice} (Total: ${totalPurchaseAmount:F2}, Products: {selectedProducts.Count})");
                }
                else
                {
                    Debug.Log($"CustomerBehavior {name} ❌ DID NOT SELECT product due to CanAfford: {canAfford}, WantsProduct: {wantsProduct}");
                }
            }
            else
            {
                Debug.Log($"CustomerBehavior {name} ❌ Shelf {targetShelf.name} has no available products (IsEmpty: {targetShelf.IsEmpty})");
            }
        }
        
        /// <summary>
        /// Check if customer can afford a product
        /// </summary>
        /// <param name="product">Product to check</param>
        /// <returns>True if customer can afford the product</returns>
        private bool CanAffordProduct(Product product)
        {
            if (product == null) return false;
            
            float remainingBudget = baseSpendingPower - totalPurchaseAmount;
            return product.CurrentPrice <= remainingBudget;
        }
        
        /// <summary>
        /// Check if customer wants a specific product (based on purchase probability and preferences)
        /// </summary>
        /// <param name="product">Product to check</param>
        /// <returns>True if customer wants the product</returns>
        private bool WantsProduct(Product product)
        {
            if (product == null || product.IsPurchased || !product.IsOnShelf) 
            {
                Debug.Log($"CustomerBehavior {name} - Product failed basic checks: IsNull: {product == null}, IsPurchased: {product?.IsPurchased}, IsOnShelf: {product?.IsOnShelf}");
                return false;
            }
            
            // Base probability of wanting any product
            float randomValue = UnityEngine.Random.value;
            bool wants = randomValue <= purchaseProbability;
            
            Debug.Log($"CustomerBehavior {name} - Random value: {randomValue:F3}, Purchase probability: {purchaseProbability:F3}, Wants product: {wants}");
            
            return wants;
        }
        
        /// <summary>
        /// Calculate customer satisfaction based on their shopping experience
        /// </summary>
        /// <returns>Satisfaction value between 0 and 1</returns>
        private float CalculateCustomerSatisfaction()
        {
            float baseSatisfaction = 0.7f; // Default satisfaction
            
            // Boost satisfaction if customer found products they wanted
            if (selectedProducts.Count > 0)
            {
                baseSatisfaction += 0.2f;
            }
            
            // Boost satisfaction if they didn't overspend
            if (totalPurchaseAmount <= baseSpendingPower * 0.8f)
            {
                baseSatisfaction += 0.1f;
            }
            
            // Add some randomness for personality variation
            baseSatisfaction += UnityEngine.Random.Range(-0.1f, 0.1f);
            
            return Mathf.Clamp01(baseSatisfaction);
        }
        
        /// <summary>
        /// Attach a selected product to the customer (visually showing that they are carrying it)
        /// </summary>
        /// <param name="product">The product to attach</param>
        private IEnumerator AttachProductToCustomer(Product product)
        {
            if (product != null)
            {
                // Define an offset position where the product will "follow" the customer
                Vector3 offset = new Vector3(0.3f, 1.2f, 0.2f); 
                
                // While the customer exists and the product is selected but not placed on counter or purchased
                while (this != null && product != null && selectedProducts.Contains(product) && !placedOnCounterProducts.Contains(product))
                {
                    // Have the product follow the customer with a slight delay/smoothing
                    if (product.transform != null && transform != null)
                    {
                        product.transform.position = Vector3.Lerp(
                            product.transform.position, 
                            transform.position + transform.rotation * offset, 
                            Time.deltaTime * 5f);
                    }
                    
                    yield return null;
                }
                
                Debug.Log($"CustomerBehavior {name} stopped attaching product {product.ProductData?.ProductName ?? "Product"} - either placed on counter or purchased");
            }
        }
        
        /// <summary>
        /// Check if customer is satisfied with current shopping selection
        /// </summary>
        /// <returns>True if customer is ready to proceed to checkout</returns>
        public bool IsSatisfiedWithShopping()
        {
            // Simple satisfaction logic - could be expanded with more complex AI
            return UnityEngine.Random.value > 0.3f; // 70% chance of being satisfied
        }
        
        /// <summary>
        /// Get preferred shopping duration based on customer personality
        /// </summary>
        /// <returns>Preferred shopping time in seconds</returns>
        public float GetPreferredShoppingDuration()
        {
            return shoppingTime;
        }
        
        #endregion
        
        #region Delayed Initialization
        
        /// <summary>
        /// Delayed initialization to ensure all components are ready
        /// </summary>
        public IEnumerator DelayedInitialization()
        {
            yield return null; // Wait one frame
            
            if (customerMovement == null)
            {
                customerMovement = GetComponent<CustomerMovement>();
            }
            
            Debug.Log($"CustomerBehavior {name} delayed initialization completed");
        }
        
        #endregion
        
        #region Legacy Field Migration
        
        /// <summary>
        /// Migrate legacy fields from main Customer component
        /// </summary>
        public void MigrateLegacyFields(float legacyShoppingTime, ShelfSlot legacyTargetShelf)
        {
            shoppingTime = legacyShoppingTime;
            targetShelf = legacyTargetShelf;
            
            Debug.Log("CustomerBehavior: Legacy fields migrated successfully");
        }
        
        #endregion

        /// <summary>
        /// Destroy all products that the customer has selected but not purchased
        /// Called when the customer leaves the shop
        /// </summary>
        private void DestroyCustomerProducts()
        {
            // Count unpurchased products
            int unpurchasedCount = 0;
            foreach (Product product in selectedProducts)
            {
                if (product != null && !product.IsPurchased)
                {
                    unpurchasedCount++;
                }
            }
            
            // Log how many products the customer is taking with them
            if (unpurchasedCount > 0)
            {
                Debug.Log($"CustomerBehavior {name} is leaving with {unpurchasedCount} unpurchased products - cleaning up");
            
                // Create a new list to avoid modification during iteration
                List<Product> productsToDestroy = new List<Product>();
                
                foreach (Product product in selectedProducts)
                {
                    if (product != null && !product.IsPurchased)
                    {
                        productsToDestroy.Add(product);
                    }
                }
                
                // Destroy each product GameObject that hasn't been purchased
                foreach (Product product in productsToDestroy)
                {
                    Debug.Log($"Destroying unpurchased product: {product.ProductData?.ProductName ?? "Unknown"}");
                    Destroy(product.gameObject);
                }
            }
            
            // Clear all product lists regardless
            selectedProducts.Clear();
            placedOnCounterProducts.Clear();
        }
        
        /// <summary>
        /// Find the nearest checkout counter to this customer
        /// </summary>
        /// <returns>The nearest CheckoutCounter component, or null if none found</returns>
        private CheckoutCounter FindNearestCheckoutCounter()
        {
            CheckoutCounter[] checkoutCounters = FindObjectsByType<CheckoutCounter>(FindObjectsSortMode.None);
            CheckoutCounter nearest = null;
            float closestDistance = float.MaxValue;
            
            foreach (CheckoutCounter counter in checkoutCounters)
            {
                if (counter != null)
                {
                    float distance = Vector3.Distance(transform.position, counter.transform.position);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        nearest = counter;
                    }
                }
            }
            
            if (nearest != null)
            {
                Debug.Log($"CustomerBehavior {name} found nearest checkout counter: {nearest.name} at distance {closestDistance:F1}");
            }
            else
            {
                Debug.LogWarning($"CustomerBehavior {name} could not find any checkout counters");
            }
            
            return nearest;
        }
        
        /// <summary>
        /// Coroutine to place selected items on the checkout counter
        /// </summary>
        /// <param name="checkoutCounter">The checkout counter to place items on</param>
        private IEnumerator PlaceItemsOnCounter(CheckoutCounter checkoutCounter)
        {
            Debug.Log($"CustomerBehavior {name} placing {selectedProducts.Count} items on checkout counter");
            
            foreach (Product product in selectedProducts)
            {
                if (product != null)
                {
                    // Add to placed products list FIRST to stop the attachment coroutine
                    placedOnCounterProducts.Add(product);
                    
                    // Place the existing product on the checkout counter
                    checkoutCounter.PlaceProduct(product);
                    
                    // Disable any movement components to ensure the product stays put
                    DisableProductMovement(product);
                    
                    Debug.Log($"CustomerBehavior {name} placed {product.ProductData?.ProductName ?? product.name} on checkout counter and disabled movement");
                    
                    // Small delay between placing each item
                    yield return new WaitForSeconds(UnityEngine.Random.Range(0.5f, 1.0f));
                }
            }
            
            Debug.Log($"CustomerBehavior {name} finished placing all items on checkout counter");
            
            // Reorganize products on the counter for better layout after all items are placed
            checkoutCounter.ReorganizeProducts();
            
            // Force UI refresh to ensure all products are visible
            checkoutCounter.RefreshUI();
        }
        
        /// <summary>
        /// Disable any movement components on a product to ensure it stays stationary after placement
        /// </summary>
        /// <param name="product">The product to disable movement for</param>
        private void DisableProductMovement(Product product)
        {
            if (product == null) return;
            
            // Disable NavMeshAgent if present
            UnityEngine.AI.NavMeshAgent navAgent = product.GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (navAgent != null)
            {
                navAgent.enabled = false;
                Debug.Log($"Disabled NavMeshAgent on {product.name}");
            }
            
            // Disable Rigidbody if present
            Rigidbody rb = product.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true;
                Debug.Log($"Set Rigidbody to kinematic on {product.name}");
            }
            
            // Disable any custom movement scripts that might be attached
            MonoBehaviour[] customScripts = product.GetComponents<MonoBehaviour>();
            foreach (MonoBehaviour script in customScripts)
            {
                // Check for common movement script patterns
                string scriptName = script.GetType().Name.ToLower();
                if (scriptName.Contains("movement") || scriptName.Contains("mover") || scriptName.Contains("follow"))
                {
                    script.enabled = false;
                    Debug.Log($"Disabled movement script {script.GetType().Name} on {product.name}");
                }
            }
        }
        
        /// <summary>
        /// Coroutine to wait for checkout completion
        /// Customer waits patiently while items are scanned and payment is processed
        /// </summary>
        /// <param name="checkoutCounter">The checkout counter being used</param>
        private IEnumerator WaitForCheckoutCompletion(CheckoutCounter checkoutCounter)
        {
            Debug.Log($"CustomerBehavior {name} waiting for checkout completion");
            
            // Set flag to indicate we're waiting for checkout
            isWaitingForCheckout = true;
            
            // Wait until checkout is completed (OnCheckoutCompleted will be called)
            while (isWaitingForCheckout)
            {
                // Optional: Add some idle animations or behaviors here
                yield return new WaitForSeconds(0.5f);
                
                // Safety check - if checkout counter becomes null or inactive, stop waiting
                if (checkoutCounter == null || !checkoutCounter.gameObject.activeInHierarchy)
                {
                    Debug.LogWarning($"CustomerBehavior {name} checkout counter became invalid, stopping wait");
                    break;
                }
            }
            
            Debug.Log($"CustomerBehavior {name} checkout completed, proceeding to collect items");
        }
        
        /// <summary>
        /// Coroutine to collect items after purchase and prepare to leave
        /// </summary>
        /// <param name="checkoutCounter">The checkout counter to collect from</param>
        private IEnumerator CollectItemsAndLeave(CheckoutCounter checkoutCounter)
        {
            Debug.Log($"CustomerBehavior {name} collecting items after purchase");
            
            // Brief delay for collecting items
            yield return new WaitForSeconds(UnityEngine.Random.Range(1.0f, 2.0f));
            
            // Mark products as purchased
            foreach (Product product in selectedProducts)
            {
                if (product != null)
                {
                    product.Purchase();
                    Debug.Log($"CustomerBehavior {name} purchased {product.ProductData?.ProductName ?? "Product"} successfully");
                }
            }
            
            // Notify checkout counter that customer is departing
            checkoutCounter.OnCustomerDeparture();
            
            Debug.Log($"CustomerBehavior {name} completed purchase collection, proceeding to leave");
            
            // Transition to leaving state
            ChangeState(CustomerState.Leaving);
        }
        
        /// <summary>
        /// Public method called by checkout counter when checkout process is completed
        /// </summary>
        public void OnCheckoutCompleted()
        {
            Debug.Log($"CustomerBehavior {name} received checkout completion notification");
            isWaitingForCheckout = false;
        }
        
        #region Debug and Testing Methods
        
        /// <summary>
        /// Test method to verify the product placement fix
        /// </summary>
        [ContextMenu("Test Product Placement Fix")]
        public void TestProductPlacementFix()
        {
            Debug.Log($"=== Testing Product Placement Fix for {name} ===");
            Debug.Log($"Selected products: {selectedProducts.Count}");
            Debug.Log($"Placed on counter products: {placedOnCounterProducts.Count}");
            
            foreach (Product product in selectedProducts)
            {
                if (product != null)
                {
                    bool isPlaced = placedOnCounterProducts.Contains(product);
                    Debug.Log($"Product: {product.ProductData?.ProductName ?? product.name} - Placed: {isPlaced}");
                }
            }
        }
        
        /// <summary>
        /// Force place all selected products on checkout counter for testing
        /// </summary>
        [ContextMenu("Force Place Products on Counter")]
        public void ForceePlaceProductsOnCounter()
        {
            CheckoutCounter counter = FindNearestCheckoutCounter();
            if (counter != null && selectedProducts.Count > 0)
            {
                Debug.Log($"Force placing {selectedProducts.Count} products on counter for testing");
                StartCoroutine(PlaceItemsOnCounter(counter));
            }
            else
            {
                Debug.LogWarning("Cannot force place products - no counter found or no selected products");
            }
        }
        
        #endregion
    }
}
