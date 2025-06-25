using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace TabletopShop
{
    /// <summary>
    /// CustomerBehavior with SIMPLE state machine integration.
    /// Central control & visualization hub - everything lives here!
    /// Gradually transitioning from coroutines to state machine without breaking existing functionality.
    /// </summary>
    public class CustomerBehavior : MonoBehaviour, ICustomerBehavior
    {
        [Header("State Management")]
        [SerializeField] private CustomerState currentState = CustomerState.Entering;
        
        [Header("State Machine Visualization")]
        [SerializeField] private CustomerState currentStateDisplay;  // Shows current state in Inspector
        [SerializeField] private float timeInCurrentState;           // Live timer in Inspector
        [SerializeField] private List<string> recentTransitions = new List<string>(); // Last 10 transitions
        [SerializeField] private bool useStateMachine = true;       // Toggle between systems
        
        [Header("Shopping Configuration")]
        [SerializeField] private float shoppingTime;
        [SerializeField] private ShelfSlot targetShelf;
        
        [Header("Purchase Configuration")]
        [SerializeField] private float baseSpendingPower = 100f;
        [SerializeField] private float purchaseProbability = 0.8f;
        
        // Component references
        private CustomerMovement customerMovement;
        private Customer mainCustomer; // Reference to main customer for state changes
        
        // SIMPLE State Machine - just a dictionary!
        private Dictionary<CustomerState, BaseCustomerState> states;
        private BaseCustomerState currentStateObject;
        private float stateStartTime;
        
        // Legacy coroutine system (keep for now)
        private Coroutine lifecycleCoroutine;
        
        // Purchase tracking
        private List<Product> selectedProducts = new List<Product>();
        private float totalPurchaseAmount = 0f;
        
        // Checkout state tracking
        private bool isWaitingForCheckout = false;
        private List<Product> placedOnCounterProducts = new List<Product>();

        // Queue state tracking
        private bool isInQueue = false;
        private int queuePosition = -1;
        private CheckoutCounter queuedCheckout = null;
        private bool waitingForCheckoutTurn = false;

        // Events
        public event Action<CustomerState, CustomerState> OnStateChangeRequested;
        public event Action<ShelfSlot> OnTargetShelfChanged;
        
        // Properties - Enhanced for State Machine compatibility
        public CustomerState CurrentState => currentState;
        public float ShoppingTime => shoppingTime;
        public ShelfSlot TargetShelf => targetShelf;
        public List<Product> SelectedProducts => selectedProducts;
        public float TotalPurchaseAmount => totalPurchaseAmount;
        public float BaseSpendingPower => baseSpendingPower;
        
        // Additional property access for state machine
        public float PurchaseProbability => purchaseProbability;
        public List<Product> PlacedOnCounterProducts => placedOnCounterProducts;
        
        // State Machine Helper Methods
        public void AddSelectedProduct(Product product) => selectedProducts.Add(product);
        public void RemoveSelectedProduct(Product product) => selectedProducts.Remove(product);
        public void AddPlacedProduct(Product product) => placedOnCounterProducts.Add(product);
        public void UpdateTotalPurchaseAmount(float amount) => totalPurchaseAmount += amount;
        public void SetWaitingForCheckout(bool waiting) => isWaitingForCheckout = waiting;
        
        // Component Access Helper Methods
        public Customer GetMainCustomer() => mainCustomer;
        public CustomerMovement GetMovement() => customerMovement;
        
        // Store Hours Helper Methods (for state machine access to legacy methods)
        public bool GetIsStoreOpen()
        {
            StoreHours storeHours = FindFirstObjectByType<StoreHours>();
            if (storeHours != null)
            {
                return storeHours.IsStoreOpen;
            }
            
            // Fallback: assume store is open if no StoreHours system found
            return true;
        }
        
        public bool GetShouldLeaveStoreDueToHours()
        {
            StoreHours storeHours = FindFirstObjectByType<StoreHours>();
            if (storeHours != null)
            {
                // If store is closed, customer should leave
                if (!storeHours.IsStoreOpen)
                {
                    return true;
                }
                
                // If store is closing soon (less than 30 minutes), finish current shopping
                float timeUntilClose = storeHours.GetTimeUntilClose();
                if (timeUntilClose <= 0.5f && timeUntilClose > 0f) // Less than 30 minutes
                {
                    // Don't force leave immediately, but hurry up shopping
                    return false;
                }
            }
            
            return false;
        }
        
        public bool GetShouldHurryUpShopping()
        {
            StoreHours storeHours = FindFirstObjectByType<StoreHours>();
            if (storeHours != null)
            {
                float timeUntilClose = storeHours.GetTimeUntilClose();
                // If less than 30 minutes until close, hurry up
                return timeUntilClose <= 0.5f && timeUntilClose > 0f;
            }
            
            return false;
        }
        
        // ICustomerCheckoutBehavior properties
        public bool IsInQueue => isInQueue;
        public int QueuePosition => queuePosition;
        public CheckoutCounter QueuedCheckout => queuedCheckout;
        public bool WaitingForCheckoutTurn => waitingForCheckoutTurn;
        public bool IsWaitingForCheckout => isWaitingForCheckout;
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            InitializeShoppingTime();
            InitializeSimpleStateMachine();
        }
        
        private void Update()
        {
            // Update state machine if active
            if (useStateMachine && currentStateObject != null)
            {
                UpdateStateMachineVisualization();
                
                // Log state machine activity every few frames for debugging
                if (Time.frameCount % 300 == 0) // Every 5 seconds at 60fps
                {
                    Debug.Log($"[STATE MACHINE] {name} running in state: {currentState} for {timeInCurrentState:F1}s");
                }
                
                currentStateObject.OnUpdate(this);
            }
            else if (useStateMachine && currentStateObject == null)
            {
                // State machine is enabled but not running - this shouldn't happen
                if (Time.frameCount % 60 == 0) // Every second at 60fps
                {
                    Debug.LogWarning($"[STATE MACHINE] {name} has useStateMachine=true but currentStateObject=null! States dict: {(states != null ? states.Count : 0)} states");
                }
            }
        }
        
        #endregion
        
        #region Simple State Machine
        
        /// <summary>
        /// Initialize the SIMPLE state machine - just a dictionary!
        /// </summary>
        private void InitializeSimpleStateMachine()
        {
            Debug.Log($"[STATE MACHINE] Initializing state machine for {name}");

            states = new Dictionary<CustomerState, BaseCustomerState>
            {
                { CustomerState.Entering, new EnteringState() },
                { CustomerState.Shopping, new ShoppingState() },
                { CustomerState.Purchasing, new PurchasingState() },
                { CustomerState.Leaving, new LeavingState() }
            };
            
            Debug.Log($"[STATE MACHINE] Successfully initialized with {states.Count} states: {string.Join(", ", states.Keys)}");
            
            // DON'T set initial state here - let StartCustomerLifecycle do it properly
            // This ensures OnEnter() gets called correctly
            currentState = CustomerState.Entering; // Just for display purposes
            currentStateObject = null; // Will be set when lifecycle starts
            stateStartTime = Time.time;
            
            Debug.Log($"[STATE MACHINE] State machine ready - waiting for lifecycle start");
        }
        
        /// <summary>
        /// Update visualization in Inspector (runs every frame)
        /// </summary>
        private void UpdateStateMachineVisualization()
        {
            currentStateDisplay = currentState;
            timeInCurrentState = Time.time - stateStartTime;
        }
        
        /// <summary>
        /// Change state in the simple state machine
        /// </summary>
        public void ChangeStateSimple(CustomerState newState, string reason = "")
        {
            // Allow same-state transitions during initialization (when currentStateObject is null)
            bool isInitialTransition = currentStateObject == null;
            
            if (newState == currentState && !isInitialTransition) 
            {
                Debug.Log($"[STATE MACHINE] {name} already in state {newState} - ignoring transition request");
                return;
            }
            
            CustomerState oldState = currentState;
            
            Debug.Log($"[STATE MACHINE] {name} transitioning: {oldState} → {newState} (Reason: {reason})");
            
            // Exit old state (only if we have a current state object)
            if (currentStateObject != null)
            {
                currentStateObject.OnExit(this);
            }
            else
            {
                Debug.Log($"[STATE MACHINE] {name} no previous state to exit (initial transition)");
            }
            
            // Change state
            currentState = newState;
            currentStateObject = states[newState];
            stateStartTime = Time.time;
            
            // Update visual state indicator BEFORE entering new state
            if (mainCustomer != null && mainCustomer.Visuals != null)
            {
                mainCustomer.Visuals.UpdateStateDisplay(newState);
            }
            
            // Enter new state
            Debug.Log($"[STATE MACHINE] {name} calling OnEnter for {newState}");
            currentStateObject.OnEnter(this);
            
            // Log transition
            string transitionLog = $"{Time.time:F1}s: {oldState} → {newState} ({reason})";
            recentTransitions.Add(transitionLog);
            if (recentTransitions.Count > 10) recentTransitions.RemoveAt(0);
            
            Debug.Log($"[STATE MACHINE] {name} successfully entered {newState}");
            
            // Notify listeners
            OnStateChangeRequested?.Invoke(oldState, newState);
        }
        
        #endregion
        
        #region Initialization
        
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
        /// Change the customer state and notify listeners (LEGACY METHOD)
        /// </summary>
        /// <param name="newState">The new state to transition to</param>
        public void ChangeState(CustomerState newState)
        {
            CustomerState previousState = currentState;
            currentState = newState;
            
            Debug.Log($"CustomerBehavior {name} state changed: {previousState} -> {currentState}");
            
            // Update visual state indicator
            if (mainCustomer != null && mainCustomer.Visuals != null)
            {
                mainCustomer.Visuals.UpdateStateDisplay(newState);
            }
            
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
        
        #region Customer Lifecycle Control
        
        /// <summary>
        /// Start customer lifecycle - DUAL MODE: coroutines OR state machine
        /// </summary>
        public void StartCustomerLifecycle(CustomerState startingState)
        {
            Debug.Log($"Starting lifecycle for {name} in {startingState} mode (useStateMachine: {useStateMachine})");
            
            // ✅ VALIDATE DEPENDENCIES FIRST
            if (customerMovement == null)
            {
                Debug.LogError($"CustomerBehavior {name}: Cannot start lifecycle - CustomerMovement not initialized!");
                return;
            }
            
            if (useStateMachine)
            {
                // NEW: Use simple state machine
                Debug.Log($"[STATE MACHINE] {name}: Using new state machine system");
                
                // Stop any running legacy coroutines
                if (lifecycleCoroutine != null)
                {
                    StopCoroutine(lifecycleCoroutine);
                    lifecycleCoroutine = null;
                    Debug.Log($"[STATE MACHINE] {name}: Stopped legacy coroutine");
                }
                
                // Ensure states are initialized
                if (states == null || states.Count == 0)
                {
                    Debug.LogWarning($"[STATE MACHINE] {name}: States not initialized, re-initializing");
                    InitializeSimpleStateMachine();
                }
                
                // Start state machine by transitioning to initial state
                // This will properly call OnEnter() even if it's the same state
                Debug.Log($"[STATE MACHINE] {name}: Starting state machine with {startingState}");
                ChangeStateSimple(startingState, "Lifecycle start");
                
                // Verify state machine is now running
                if (currentStateObject != null)
                {
                    Debug.Log($"[STATE MACHINE] {name}: ✓ State machine successfully started in {currentState}");
                }
                else
                {
                    Debug.LogError($"[STATE MACHINE] {name}: ✗ Failed to start state machine - currentStateObject is null!");
                }
            }
            else
            {
                // LEGACY: Use existing coroutines (keep working!)
                Debug.Log($"CustomerBehavior {name}: Using legacy coroutine system");
                
                // ChangeState(startingState);
                if (lifecycleCoroutine != null) StopCoroutine(lifecycleCoroutine);
                lifecycleCoroutine = StartCoroutine(CustomerLifecycleCoroutine(startingState));
            }
        }
        
        /// <summary>
        /// Stop the customer lifecycle (both state machine and legacy coroutines)
        /// </summary>
        public void StopCustomerLifecycle()
        {
            // Stop state machine if active
            if (useStateMachine)
            {
                Debug.Log($"CustomerBehavior {name}: Stopping state machine");
                currentStateObject?.OnExit(this);
                currentStateObject = null;
            }
            
            // Stop legacy coroutines if running
            if (lifecycleCoroutine != null)
            {
                Debug.Log($"CustomerBehavior {name}: Stopping legacy coroutine");
                StopCoroutine(lifecycleCoroutine);
                lifecycleCoroutine = null;
            }
        }
        
        #endregion
        
        #region Legacy Coroutine System (KEEP ALL EXISTING METHODS)
        
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
                // Check if store is open before entering
                if (!IsStoreOpen())
                {
                    Debug.Log($"CustomerBehavior {name}: Store is closed - cannot enter, going straight to leaving");
                    ChangeState(CustomerState.Leaving);
                    yield return StartCoroutine(HandleLeavingState());
                    yield break;
                }
                
                yield return StartCoroutine(HandleEnteringState());
                
                // Check again after entering - store might have closed while entering
                if (ShouldLeaveStoreDueToHours())
                {
                    ChangeState(CustomerState.Leaving);
                    yield return StartCoroutine(HandleLeavingState());
                    yield break;
                }
                
                ChangeState(CustomerState.Shopping);
            }
            
            // Phase 2: Shopping behavior
            if (currentState == CustomerState.Shopping)
            {
                yield return StartCoroutine(HandleShoppingState());
                
                // Check if store is still open after shopping
                if (ShouldLeaveStoreDueToHours())
                {
                    ChangeState(CustomerState.Leaving);
                    yield return StartCoroutine(HandleLeavingState());
                    yield break;
                }
                
                ChangeState(CustomerState.Purchasing);
            }
            
            // Phase 3: Purchasing (move to checkout area and make purchase)
            if (currentState == CustomerState.Purchasing)
            {
                // Even if store is closing, allow customers to complete their purchase
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
        public IEnumerator HandleEnteringState()
        {
            Debug.Log($"CustomerBehavior {name} entering shop - looking for shelves");
            
            // Move to a random shelf to start shopping
            bool foundShelf = SetRandomShelfDestination();
            if (foundShelf)
            {
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
        public IEnumerator HandleShoppingState()
        {
            Debug.Log($"CustomerBehavior {name} browsing products for {shoppingTime:F1} seconds");
            
            float shoppedTime = 0f;
            float lastProductCheckTime = 0f;
            float originalShoppingTime = shoppingTime;
            
            while (shoppedTime < shoppingTime)
            {
                // Check if store is closing and customer should leave
                if (ShouldLeaveStoreDueToHours())
                {
                    Debug.Log($"CustomerBehavior {name} cutting shopping short - store is closing");
                    break;
                }
                
                // Check if customer should hurry up due to store closing soon
                if (ShouldHurryUpShopping())
                {
                    // Reduce remaining shopping time by half when store is closing soon
                    float remainingTime = shoppingTime - shoppedTime;
                    shoppingTime = shoppedTime + (remainingTime * 0.5f);
                    Debug.Log($"CustomerBehavior {name} hurrying up shopping - store closing soon (reduced time from {originalShoppingTime:F1}s to {shoppingTime:F1}s)");
                }
                
                // Occasionally move to different shelves while shopping
                if (UnityEngine.Random.value < 0.3f) // 30% chance every check
                {
                    SetRandomShelfDestination();
                    
                    // Wait a bit for movement
                    yield return new WaitForSeconds(2f);
                    
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
        /// Uses proper queue system - customers must wait their turn before placing items
        /// </summary>
        public IEnumerator HandlePurchasingState()
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
                
                Debug.Log($"CustomerBehavior {name} has arrived at the checkout counter");
                
                // Set waiting flag BEFORE notifying checkout counter to avoid race conditions
                waitingForCheckoutTurn = true;
                
                // Notify the checkout counter that customer has arrived
                // This will either put them in queue OR immediately call OnCheckoutReady if counter is free
                targetCheckoutCounter.OnCustomerArrival(GetComponent<Customer>());
                
                // Check if the checkout counter immediately gave us permission (backwards compatibility)
                // If OnCheckoutReady was called synchronously, waitingForCheckoutTurn would be false
                yield return new WaitForSeconds(0.1f); // Small delay to allow synchronous callbacks
                
                if (!waitingForCheckoutTurn)
                {
                    Debug.Log($"CustomerBehavior {name} got immediate checkout permission");
                }
                else
                {
                    Debug.Log($"CustomerBehavior {name} waiting for checkout turn... (waitingForCheckoutTurn = {waitingForCheckoutTurn})");
                }
                
                float waitTime = 0f;
                while (waitingForCheckoutTurn)
                {
                    yield return new WaitForSeconds(0.5f);
                    waitTime += 0.5f;
                    
                    // Debug logging every 5 seconds to track waiting
                    if (waitTime % 5f < 0.6f)
                    {
                        Debug.Log($"CustomerBehavior {name} still waiting for checkout turn... ({waitTime:F1}s elapsed)");
                    }
                    
                    // Safety check - if checkout counter becomes null or inactive, stop waiting
                    if (targetCheckoutCounter == null || !targetCheckoutCounter.gameObject.activeInHierarchy)
                    {
                        Debug.LogWarning($"CustomerBehavior {name} checkout counter became invalid, stopping wait");
                        waitingForCheckoutTurn = false;
                        break;
                    }
                    
                    // Safety timeout - if waiting too long, assume counter doesn't support proper queue management
                    if (waitTime > 10f)
                    {
                        Debug.LogWarning($"CustomerBehavior {name} has been waiting for {waitTime:F1}s - CheckoutCounter may not support queue callbacks. Proceeding anyway.");
                        waitingForCheckoutTurn = false;
                        isInQueue = false;
                        break;
                    }
                }
                
                Debug.Log($"CustomerBehavior {name} got checkout turn - proceeding to place items");
                
                // Now it's our turn - place items on counter
                yield return StartCoroutine(PlaceItemsOnCounter(targetCheckoutCounter));
                
                // Wait for checkout completion (payment processing)
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
        public IEnumerator HandleLeavingState()
        {
            Debug.Log($"CustomerBehavior {name} leaving the shop");
            
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
            }
        }
        
        /// <summary>
        /// Try to select products at a specific shelf (interface implementation)
        /// </summary>
        /// <param name="shelf">The shelf to select products from</param>
        public void TrySelectProductsAtShelf(ShelfSlot shelf)
        {
            if (shelf != null)
            {
                // Set the shelf as target and try to select products
                ShelfSlot originalTarget = targetShelf;
                SetTargetShelf(shelf);
                TrySelectProductsAtCurrentShelf();
                
                // Restore original target if no products were selected
                if (targetShelf == shelf && selectedProducts.Count == 0)
                {
                    targetShelf = originalTarget;
                }
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
        
        #region Store Hours Integration
        
        /// <summary>
        /// Check if the store is currently open for customers
        /// </summary>
        public bool IsStoreOpen()
        {
            StoreHours storeHours = FindFirstObjectByType<StoreHours>();
            if (storeHours != null)
            {
                return storeHours.IsStoreOpen;
            }
            
            // Fallback: assume store is open if no StoreHours system found
            return true;
        }
        
        /// <summary>
        /// Check if customer should continue shopping or leave due to store closing
        /// </summary>
        public bool ShouldLeaveStoreDueToHours()
        {
            StoreHours storeHours = FindFirstObjectByType<StoreHours>();
            if (storeHours != null)
            {
                // If store is closed, customer should leave
                if (!storeHours.IsStoreOpen)
                {
                    Debug.Log($"CustomerBehavior {name}: Store is closed - customer should leave");
                    return true;
                }
                
                // If store is closing soon (less than 30 minutes), finish current shopping
                float timeUntilClose = storeHours.GetTimeUntilClose();
                if (timeUntilClose <= 0.5f && timeUntilClose > 0f) // Less than 30 minutes
                {
                    Debug.Log($"CustomerBehavior {name}: Store closing soon ({timeUntilClose:F1}h) - finishing up shopping");
                    // Don't force leave immediately, but hurry up shopping
                    return false;
                }
            }
            
            return false;
        }
        
        /// <summary>
        /// Check if customer should hurry up their shopping due to store closing soon
        /// </summary>
        public bool ShouldHurryUpShopping()
        {
            StoreHours storeHours = FindFirstObjectByType<StoreHours>();
            if (storeHours != null)
            {
                float timeUntilClose = storeHours.GetTimeUntilClose();
                // If less than 30 minutes until close, hurry up
                return timeUntilClose <= 0.5f && timeUntilClose > 0f;
            }
            
            return false;
        }
        
        #endregion
        
        #region Checkout and Queue Management
        
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
        /// Only proceeds if customer is authorized to use the counter
        /// </summary>
        /// <param name="checkoutCounter">The checkout counter to place items on</param>
        public IEnumerator PlaceItemsOnCounter(CheckoutCounter checkoutCounter)
        {
            // Safety check: Only place items if we're not waiting for our turn and not in queue
            if (waitingForCheckoutTurn)
            {
                Debug.LogWarning($"CustomerBehavior {name} attempted to place items while waiting for checkout turn - blocking!");
                yield break;
            }
            
            if (isInQueue)
            {
                Debug.LogWarning($"CustomerBehavior {name} attempted to place items while still in queue - blocking!");
                yield break;
            }
            
            Debug.Log($"CustomerBehavior {name} placing {selectedProducts.Count} items on checkout counter");
            
            for (int i = 0; i < selectedProducts.Count; i++)
            {
                Product product = selectedProducts[i];
                if (product != null)
                {
                    Debug.Log($"CustomerBehavior {name} placing product {i + 1}/{selectedProducts.Count}: {product.ProductData?.ProductName ?? product.name}");
                    
                    // Place the product at checkout with customer association
                    checkoutCounter.PlaceProduct(product, mainCustomer);
                    
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
                    placedOnCounterProducts.Add(product);
                    
                    // Small delay between placements for natural look
                    yield return new WaitForSeconds(0.5f);
                }
            }
            
            Debug.Log($"CustomerBehavior {name} finished placing {placedOnCounterProducts.Count} items on counter");
            
            // Force UI refresh to ensure all products are visible
            checkoutCounter.RefreshUI();
        }
        
        /// <summary>
        /// Coroutine to wait for checkout completion
        /// Customer waits patiently while items are scanned and payment is processed
        /// </summary>
        /// <param name="checkoutCounter">The checkout counter being used</param>
        public IEnumerator WaitForCheckoutCompletion(CheckoutCounter checkoutCounter)
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
        public IEnumerator CollectItemsAndLeave(CheckoutCounter checkoutCounter)
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
        
        /// <summary>
        /// Called when customer joins a checkout queue
        /// </summary>
        /// <param name="checkoutCounter">The checkout counter they're queuing for</param>
        /// <param name="position">Their position in the queue (0 = next in line)</param>
        public void OnJoinedQueue(CheckoutCounter checkoutCounter, int position)
        {
            Debug.Log($"CustomerBehavior {name} joined queue at position {position + 1} for checkout {checkoutCounter.name}");
            
            // Store queue information
            isInQueue = true;
            queuePosition = position;
            queuedCheckout = checkoutCounter;
            
            // Customer is still waiting for their turn
            waitingForCheckoutTurn = true;
        }
        
        /// <summary>
        /// Called when customer's position in queue changes
        /// </summary>
        /// <param name="newPosition">New position in queue</param>
        public void OnQueuePositionChanged(int newPosition)
        {
            Debug.Log($"CustomerBehavior {name} moved to queue position {newPosition + 1}");
            queuePosition = newPosition;
        }
        
        /// <summary>
        /// Called when it's the customer's turn at checkout
        /// </summary>
        /// <param name="checkoutCounter">The checkout counter that's ready</param>
        public void OnCheckoutReady(CheckoutCounter checkoutCounter)
        {
            Debug.Log($"CustomerBehavior {name} can now proceed to checkout counter {checkoutCounter.name}");
            
            // Clear queue flags
            isInQueue = false;
            queuePosition = -1;
            queuedCheckout = null;
            
            // Signal that it's our turn (this will allow HandlePurchasingState to continue)
            waitingForCheckoutTurn = false;
        }
        
        /// <summary>
        /// Force customer to leave queue (for testing)
        /// </summary>
        public void ForceLeaveQueue()
        {
            if (isInQueue && queuedCheckout != null)
            {
                Debug.Log($"CustomerBehavior {name} forcibly leaving queue");
                
                // Clear queue state
                isInQueue = false;
                queuePosition = -1;
                queuedCheckout = null;
                waitingForCheckoutTurn = false;
                
                // Force customer to leave
                ChangeState(CustomerState.Leaving);
            }
            else
            {
                Debug.Log($"CustomerBehavior {name} is not in a queue");
            }
        }
        
        #endregion
        
        #region Cleanup and Utilities
        
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
        /// Get formatted debug info about customer's current state
        /// </summary>
        public string GetDebugInfo()
        {
            return $"Customer {name}: State={currentState}, InQueue={isInQueue}, QueuePos={queuePosition}, " +
                   $"WaitingTurn={waitingForCheckoutTurn}, WaitingCheckout={isWaitingForCheckout}, " +
                   $"Products={selectedProducts.Count}, Placed={placedOnCounterProducts.Count}";
        }
        
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
        
        #region Debug and Testing
        
        [ContextMenu("Test State Machine")]
        public void TestStateMachine()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("State machine test requires Play mode");
                return;
            }
            
            StartCoroutine(TestStateMachineCoroutine());
        }
        
        private IEnumerator TestStateMachineCoroutine()
        {
            Debug.Log($"=== STATE MACHINE TEST FOR {name} ===");
            
            // Force state machine mode
            useStateMachine = true;
            Debug.Log($"✓ Set useStateMachine = true");
            
            // Initialize if needed
            if (states == null || states.Count == 0)
            {
                InitializeSimpleStateMachine();
                Debug.Log($"✓ Re-initialized state machine");
            }
            
            // Test all state transitions
            Debug.Log($"Testing state transitions...");
            
            ChangeStateSimple(CustomerState.Entering, "Test - Entering");
            yield return new WaitForSeconds(2f);
            
            ChangeStateSimple(CustomerState.Shopping, "Test - Shopping");
            yield return new WaitForSeconds(2f);
            
            ChangeStateSimple(CustomerState.Purchasing, "Test - Purchasing");
            yield return new WaitForSeconds(2f);
            
            ChangeStateSimple(CustomerState.Leaving, "Test - Leaving");
            
            Debug.Log($"✓ State machine test completed for {name}");
        }
        
        [ContextMenu("Force Enable State Machine")]
        public void ForceEnableStateMachine()
        {
            useStateMachine = true;
            Debug.Log($"✓ Force enabled state machine for {name}");
            
            if (Application.isPlaying)
            {
                // Stop any coroutines
                if (lifecycleCoroutine != null)
                {
                    StopCoroutine(lifecycleCoroutine);
                    lifecycleCoroutine = null;
                    Debug.Log($"✓ Stopped legacy coroutine");
                }
                
                // Re-initialize if needed
                if (states == null || states.Count == 0)
                {
                    InitializeSimpleStateMachine();
                    Debug.Log($"✓ Re-initialized state machine");
                }
                
                // Start in entering state
                ChangeStateSimple(CustomerState.Entering, "Force enable");
            }
        }
        
        [ContextMenu("Debug Current State")]
        public void DebugCurrentState()
        {
            Debug.Log("=== CUSTOMER STATE MACHINE DEBUG ===");
            Debug.Log($"Customer: {name}");
            Debug.Log($"Use State Machine: {useStateMachine}");
            Debug.Log($"Current State: {currentState}");
            Debug.Log($"State Object: {(currentStateObject != null ? currentStateObject.GetType().Name : "NULL")}");
            Debug.Log($"Time in State: {timeInCurrentState:F1}s");
            Debug.Log($"States Dictionary: {(states != null ? $"{states.Count} states" : "NULL")}");
            Debug.Log($"Movement Component: {(customerMovement != null ? "OK" : "NULL")}");
            
            if (states != null)
            {
                Debug.Log($"Available States: {string.Join(", ", states.Keys)}");
            }
            
            Debug.Log($"Recent Transitions:");
            if (recentTransitions.Count > 0)
            {
                foreach (var transition in recentTransitions)
                {
                    Debug.Log($"  {transition}");
                }
            }
            else
            {
                Debug.Log("  No transitions recorded");
            }
            Debug.Log("========================");
        }
        
        [ContextMenu("Test Product Placement Fix")]
        public void TestProductPlacementFix()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("Product placement test requires Play mode");
                return;
            }
            
            Debug.Log("=== PRODUCT PLACEMENT FIX TEST ===");
            Debug.Log($"Customer: {name}");
            Debug.Log($"Current State: {currentState}");
            Debug.Log($"Is In Queue: {isInQueue}");
            Debug.Log($"Queue Position: {queuePosition}");
            Debug.Log($"Waiting For Checkout Turn: {waitingForCheckoutTurn}");
            Debug.Log($"Selected Products: {selectedProducts.Count}");
            Debug.Log($"Placed On Counter: {placedOnCounterProducts.Count}");
            
            // Check each selected product's interaction state
            for (int i = 0; i < selectedProducts.Count; i++)
            {
                Product product = selectedProducts[i];
                if (product != null)
                {
                    Debug.Log($"Product {i + 1}: {product.ProductData?.ProductName ?? product.name}");
                    Debug.Log($"  - Placed By: {(product.PlacedByCustomer != null ? product.PlacedByCustomer.name : "None")}");
                    Debug.Log($"  - Can Interact: {product.CanInteract}");
                    Debug.Log($"  - Interaction Text: {product.InteractionText}");
                    Debug.Log($"  - Is Scanned: {product.IsScannedAtCheckout}");
                    
                    ProductInteraction productInteraction = product.GetComponent<ProductInteraction>();
                    if (productInteraction != null)
                    {
                        Debug.Log($"  - Interaction Component: Active");
                        Debug.Log($"  - Interaction Status: {productInteraction.GetInteractionStatus()}");
                    }
                    else
                    {
                        Debug.LogError($"  - Interaction Component: MISSING!");
                    }
                }
            }
            
            Debug.Log("===================================");
        }
        
        [ContextMenu("Debug Customer Queue State")]
        public void DebugCustomerQueueState()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("Customer queue debug requires Play mode");
                return;
            }
            
            Debug.Log("=== CUSTOMER QUEUE STATE DEBUG ===");
            Debug.Log($"Customer: {name}");
            Debug.Log($"Current State: {currentState}");
            Debug.Log($"Is In Queue: {isInQueue}");
            Debug.Log($"Queue Position: {queuePosition}");
            Debug.Log($"Queued Checkout: {(queuedCheckout != null ? queuedCheckout.name : "None")}");
            Debug.Log($"Waiting For Checkout Turn: {waitingForCheckoutTurn}");
            
            if (queuedCheckout != null)
            {
                Debug.Log($"Checkout Counter Status:");
                Debug.Log($"  - Has Customer: {queuedCheckout.HasCustomer}");
                Debug.Log($"  - Queue Length: {queuedCheckout.QueueLength}");
                Debug.Log($"  - Is Occupied: {queuedCheckout.IsOccupied}");
                Debug.Log($"  - Can Place Items: {queuedCheckout.CanCustomerPlaceItems(mainCustomer)}");
            }
            
            Debug.Log("===================================");
        }
        
        [ContextMenu("Check Queue Status")]
        public void CheckQueueStatus()
        {
            Debug.Log($"=== Queue Status for {name} ===");
            Debug.Log($"Is in queue: {isInQueue}");
            Debug.Log($"Queue position: {queuePosition}");
            Debug.Log($"Queued checkout: {(queuedCheckout != null ? queuedCheckout.name : "None")}");
            Debug.Log($"Waiting for checkout turn: {waitingForCheckoutTurn}");
            Debug.Log($"Waiting for checkout: {isWaitingForCheckout}");
            Debug.Log($"Current state: {currentState}");
            Debug.Log($"Selected products: {selectedProducts.Count}");
            Debug.Log($"Placed products: {placedOnCounterProducts.Count}");
            Debug.Log("===============================");
        }
        
        [ContextMenu("Toggle State Machine System")]
        public void ToggleStateMachineSystem()
        {
            useStateMachine = !useStateMachine;
            
            Debug.Log($"✓ Toggled to {(useStateMachine ? "STATE MACHINE" : "LEGACY COROUTINES")} system for {name}");
            
            if (Application.isPlaying)
            {
                // Stop current system
                StopCustomerLifecycle();
                
                // Wait a frame then restart in current state
                StartCoroutine(RestartInCurrentState());
            }
        }
        
        /// <summary>
        /// Restart customer lifecycle in current state after system switch
        /// </summary>
        private IEnumerator RestartInCurrentState()
        {
            yield return null; // Wait one frame
            
            Debug.Log($"Restarting {name} in {currentState} using {(useStateMachine ? "state machine" : "coroutines")}");
            StartCustomerLifecycle(currentState);
        }
        
        [ContextMenu("Validate System Parity")]
        public void ValidateSystemParity()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("System parity validation requires Play mode");
                return;
            }
            
            Debug.Log("=== SYSTEM PARITY VALIDATION ===");
            Debug.Log($"Customer: {name}");
            Debug.Log($"Current System: {(useStateMachine ? "State Machine" : "Legacy Coroutines")}");
            Debug.Log($"Current State: {currentState}");
            Debug.Log($"Shopping Time: {shoppingTime:F1}s");
            Debug.Log($"Base Spending Power: ${baseSpendingPower:F2}");
            Debug.Log($"Purchase Probability: {purchaseProbability:F3}");
            Debug.Log($"Selected Products: {selectedProducts.Count}");
            Debug.Log($"Total Purchase Amount: ${totalPurchaseAmount:F2}");
            Debug.Log($"Placed Products: {placedOnCounterProducts.Count}");
            Debug.Log($"Queue State: InQueue={isInQueue}, Position={queuePosition}, WaitingTurn={waitingForCheckoutTurn}");
            Debug.Log($"Movement Component: {(customerMovement != null ? "Available" : "NULL")}");
            Debug.Log($"Main Customer: {(mainCustomer != null ? "Available" : "NULL")}");
            
            // Test store hours integration
            Debug.Log($"Store Open: {GetIsStoreOpen()}");
            Debug.Log($"Should Leave: {GetShouldLeaveStoreDueToHours()}");
            Debug.Log($"Should Hurry: {GetShouldHurryUpShopping()}");
            
            Debug.Log("=== Both systems should show identical values ===");
        }
        
        [ContextMenu("Debug Shopping Behavior")]
        public void DebugShoppingBehavior()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("Shopping behavior debug requires Play mode");
                return;
            }
            
            Debug.Log("=== SHOPPING BEHAVIOR DEBUG ===");
            Debug.Log($"Customer: {name}");
            Debug.Log($"System: {(useStateMachine ? "STATE MACHINE" : "LEGACY COROUTINES")}");
            Debug.Log($"Current State: {currentState}");
            Debug.Log($"Target Shelf: {(targetShelf != null ? targetShelf.name : "None")}");
            Debug.Log($"Is Moving: {(customerMovement != null ? customerMovement.IsMoving : false)}");
            Debug.Log($"Has Reached Destination: {(customerMovement != null ? customerMovement.HasReachedDestination() : false)}");
            Debug.Log($"Selected Products: {selectedProducts.Count}");
            Debug.Log($"Total Purchase Amount: ${totalPurchaseAmount:F2}");
            
            // Check shelf status if we have a target
            if (targetShelf != null)
            {
                Debug.Log($"--- SHELF STATUS ---");
                Debug.Log($"Shelf Name: {targetShelf.name}");
                Debug.Log($"Is Empty: {targetShelf.IsEmpty}");
                Debug.Log($"Has Product: {targetShelf.CurrentProduct != null}");
                if (targetShelf.CurrentProduct != null)
                {
                    Debug.Log($"Product: {targetShelf.CurrentProduct.ProductData?.ProductName ?? "Unknown"}");
                    Debug.Log($"Price: ${targetShelf.CurrentProduct.CurrentPrice:F2}");
                    Debug.Log($"Can Afford: {CanAffordProduct(targetShelf.CurrentProduct)}");
                    Debug.Log($"Wants Product: {WantsProduct(targetShelf.CurrentProduct)}");
                }
            }
            
            Debug.Log("=== State machine should now browse and select products ===");
        }
        
        // // Helper methods for debugging (copied from legacy implementation)
        // private bool CanAffordProduct(Product product)
        // {
        //     if (product == null) return false;
        //     float remainingBudget = baseSpendingPower - totalPurchaseAmount;
        //     return product.CurrentPrice <= remainingBudget;
        // }
        
        // private bool WantsProduct(Product product)
        // {
        //     if (product == null || product.IsPurchased || !product.IsOnShelf) return false;
        //     return UnityEngine.Random.value <= purchaseProbability;
        // }
        
        #endregion
    }

}