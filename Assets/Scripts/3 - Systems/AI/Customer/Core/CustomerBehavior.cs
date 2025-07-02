using UnityEngine;
using System.Collections.Generic;
using System;

namespace TabletopShop
{
    /// <summary>
    /// CustomerBehavior - BEHAVIOR DESIGNER ONLY VERSION
    /// All legacy coroutine and state machine code removed
    /// Only provides essential properties and BD task tracking
    /// </summary>
    public class CustomerBehavior : MonoBehaviour
    {
        [Header("State Management")]
        [SerializeField] private CustomerState currentState = CustomerState.Entering;
        
        [Header("Behavior Designer Mode")]
        public bool useBehaviorDesigner = true;   // Always true - legacy removed
        
        [Header("Shopping Configuration")]
        [SerializeField] private float shoppingTime;
        [SerializeField] private ShelfSlot targetShelf;
        
        [Header("Purchase Configuration")]
        [SerializeField] private float baseSpendingPower = 100f;
        [SerializeField] private float purchaseProbability = 0.95f;
        
        // Component references
        private CustomerMovement customerMovement;
        private Customer mainCustomer;
        
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
        
        // Properties
        public CustomerState CurrentState => currentState;
        public float ShoppingTime => shoppingTime;
        public ShelfSlot TargetShelf => targetShelf;
        public List<Product> SelectedProducts => selectedProducts;
        public float TotalPurchaseAmount => totalPurchaseAmount;
        public float BaseSpendingPower => baseSpendingPower;
        public float PurchaseProbability => purchaseProbability;
        public List<Product> PlacedOnCounterProducts => placedOnCounterProducts;
        
        // State Machine Helper Methods for BD tasks
        public void AddSelectedProduct(Product product) => selectedProducts.Add(product);
        public void RemoveSelectedProduct(Product product) => selectedProducts.Remove(product);
        public void AddPlacedProduct(Product product) => placedOnCounterProducts.Add(product);
        public void UpdateTotalPurchaseAmount(float amount) => totalPurchaseAmount += amount;
        public void SetWaitingForCheckout(bool waiting) => isWaitingForCheckout = waiting;
        
        // Component Access Helper Methods
        public Customer GetMainCustomer() => mainCustomer;
        public CustomerMovement GetMovement() => customerMovement;
        
        // Queue properties for BD tasks
        public bool IsInQueue => isInQueue;
        public int QueuePosition => queuePosition;
        public CheckoutCounter QueuedCheckout => queuedCheckout;
        public bool WaitingForCheckoutTurn => waitingForCheckoutTurn;
        public bool IsWaitingForCheckout => isWaitingForCheckout;
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            InitializeShoppingTime();
        }
        
        private void Update()
        {
            // Only minimal updates for BD mode
            if (Time.frameCount % 300 == 0) // Every 5 seconds at 60fps
            {
                Debug.Log($"🎯 BEHAVIOR DESIGNER MODE: {name} - Running with BD tasks");
            }
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
        /// Initialize CustomerBehavior for Behavior Designer mode
        /// </summary>
        public void InitializeBehaviorDesignerMode()
        {
            Debug.Log($"🎯 BEHAVIOR DESIGNER MODE: Initializing {name} for Behavior Designer");
            
            // Set default state for visual display
            currentState = CustomerState.Entering;
            
            // Ensure visual components are initialized for feedback
            if (mainCustomer != null && mainCustomer.Visuals != null)
            {
                mainCustomer.Visuals.UpdateStateDisplay(currentState);
                Debug.Log($"🎯 BEHAVIOR DESIGNER MODE: Visual feedback ready for {name}");
            }
            
            Debug.Log($"🎯 BEHAVIOR DESIGNER MODE: {name} ready for Behavior Trees");
        }
        
        /// <summary>
        /// Initialize random shopping time between 10-30 seconds
        /// </summary>
        private void InitializeShoppingTime()
        {
            shoppingTime = UnityEngine.Random.Range(10f, 30f);
        }
        
        #endregion
        
        #region State Management - Basic for BD
        
        /// <summary>
        /// Change the customer state and notify listeners
        /// </summary>
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
        /// Check if customer is currently in a specific state
        /// </summary>
        public bool IsInState(CustomerState state)
        {
            return currentState == state;
        }
        
        #endregion
        
        #region Target Management
        
        /// <summary>
        /// Set target shelf for shopping
        /// </summary>
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
        
        #endregion
        
        #region Store Hours Helper Methods
        
        public bool GetIsStoreOpen()
        {
            StoreHours storeHours = FindFirstObjectByType<StoreHours>();
            if (storeHours != null)
            {
                return storeHours.IsStoreOpen;
            }
            return true; // Fallback: assume store is open
        }
        
        public bool GetShouldLeaveStoreDueToHours()
        {
            StoreHours storeHours = FindFirstObjectByType<StoreHours>();
            if (storeHours != null)
            {
                if (!storeHours.IsStoreOpen)
                {
                    return true;
                }
                
                float timeUntilClose = storeHours.GetTimeUntilClose();
                if (timeUntilClose <= 0.5f && timeUntilClose > 0f)
                {
                    return false; // Don't force leave immediately, but hurry up
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
                return timeUntilClose <= 0.5f && timeUntilClose > 0f;
            }
            return false;
        }
        
        #endregion
        
        #region Checkout and Queue Management for BD Tasks
        
        /// <summary>
        /// Called when customer joins a checkout queue
        /// </summary>
        public void OnJoinedQueue(CheckoutCounter checkoutCounter, int position)
        {
            Debug.Log($"CustomerBehavior {name} joined queue at position {position + 1} for checkout {checkoutCounter.name}");
            
            isInQueue = true;
            queuePosition = position;
            queuedCheckout = checkoutCounter;
            waitingForCheckoutTurn = true;
        }
        
        /// <summary>
        /// Called when customer's position in queue changes
        /// </summary>
        public void OnQueuePositionChanged(int newPosition)
        {
            Debug.Log($"CustomerBehavior {name} moved to queue position {newPosition + 1}");
            queuePosition = newPosition;
        }
        
        /// <summary>
        /// Called when it's the customer's turn at checkout
        /// </summary>
        public void OnCheckoutReady(CheckoutCounter checkoutCounter)
        {
            Debug.Log($"CustomerBehavior {name} can now proceed to checkout counter {checkoutCounter.name}");
            
            isInQueue = false;
            queuePosition = -1;
            queuedCheckout = null;
            waitingForCheckoutTurn = false;
        }
        
        /// <summary>
        /// Called by checkout counter when checkout process is completed
        /// </summary>
        public void OnCheckoutCompleted()
        {
            Debug.Log($"CustomerBehavior {name} received checkout completion notification");
            isWaitingForCheckout = false;
        }
        
        #endregion
        
        #region Behavior Designer Task Display
        
        /// <summary>
        /// Get the name of the currently active Behavior Designer task for display purposes
        /// </summary>
        public string GetCurrentBDTaskName()
        {
            var behaviorTree = GetComponent<Opsive.BehaviorDesigner.Runtime.BehaviorTree>();
            if (behaviorTree == null)
            {
                return "No BD";
            }
            
            if (behaviorTree.Status == Opsive.BehaviorDesigner.Runtime.Tasks.TaskStatus.Inactive)
            {
                return "BD Inactive";
            }
            
            try
            {
                var allTasks = behaviorTree.FindTasks<Opsive.BehaviorDesigner.Runtime.Tasks.Task>();
                var activeTasks = new List<(string taskName, int priority)>();
                
                for (int i = 0; i < allTasks.Length; i++)
                {
                    var task = allTasks[i];
                    if (task != null)
                    {
                        try
                        {
                            if (behaviorTree.IsNodeActive(true, i))
                            {
                                string taskName = task.GetType().Name;
                                int priority = GetTaskPriority(taskName);
                                activeTasks.Add((taskName, priority));
                            }
                        }
                        catch
                        {
                            continue;
                        }
                    }
                }
                
                activeTasks.Sort((a, b) => b.priority.CompareTo(a.priority));
                
                foreach (var (taskName, priority) in activeTasks)
                {
                    if (priority > 0)
                    {
                        return GetFriendlyTaskName(taskName);
                    }
                }
                
                return behaviorTree.Status == Opsive.BehaviorDesigner.Runtime.Tasks.TaskStatus.Running ? "BD Running" : "BD Idle";
            }
            catch
            {
                return "BD Error";
            }
        }
        
        private int GetTaskPriority(string taskName)
        {
            // Highest priority - specific action tasks
            if (taskName.Contains("EnterShopTask") ||
                taskName.Contains("MoveToCheckoutTask") || 
                taskName.Contains("JoinQueueTask") ||
                taskName.Contains("PlaceProductsTask") ||
                taskName.Contains("WaitForScanningTask") ||
                taskName.Contains("CompleteCheckoutTask") ||
                taskName.Contains("MoveToExitTask") ||
                taskName.Contains("CleanupAndDestroyTask") ||
                taskName.Contains("FindShelfTask") ||
                taskName.Contains("MoveToShelfTask") ||
                taskName.Contains("BrowseShelfTask") ||
                taskName.Contains("SelectProductTask"))
                return 100;
            
            // Medium priority - phase tasks
            if (taskName.Contains("InitializeShopping"))
                return 50;
            
            // Lower priority - container tasks
            if (taskName.Contains("ShoppingLoop") ||
                taskName.Contains("ShoppingPhase") ||
                taskName.Contains("CheckoutPhase") ||
                taskName.Contains("ExitPhase"))
                return 25;
            
            // Skip framework tasks
            if (taskName.Contains("StackedAction") || 
                taskName.Contains("Composite") || 
                taskName.Contains("Decorator") ||
                taskName.Contains("Conditional") ||
                taskName.Contains("Parallel") ||
                taskName.Contains("Sequence") ||
                taskName.Contains("Selector"))
                return 0;
            
            return taskName.EndsWith("Task") ? 10 : 1;
        }
        
        private string GetFriendlyTaskName(string taskName)
        {
            if (taskName.Contains("EnterShopTask"))
                return "Entering Shop";
            else if (taskName.Contains("InitializeShopping"))
                return "Starting Shopping";
            else if (taskName.Contains("MoveToCheckoutTask"))
                return "Moving to Checkout";
            else if (taskName.Contains("JoinQueueTask"))
                return "Joining Queue";
            else if (taskName.Contains("PlaceProductsTask"))
                return "Placing Products";
            else if (taskName.Contains("WaitForScanningTask"))
                return "Waiting for Scanning";
            else if (taskName.Contains("CompleteCheckoutTask"))
                return "Completing Checkout";
            else if (taskName.Contains("MoveToExitTask"))
                return "Leaving Store";
            else if (taskName.Contains("CleanupAndDestroyTask"))
                return "Cleaning Up";
            else if (taskName.Contains("FindShelfTask"))
                return "Finding Shelf";
            else if (taskName.Contains("MoveToShelfTask"))
                return "Moving to Shelf";
            else if (taskName.Contains("BrowseShelfTask"))
                return "Browsing Shelf";
            else if (taskName.Contains("SelectProductTask"))
                return "Selecting Product";
            else if (taskName.Contains("ShoppingPhase"))
                return "Shopping";
            else if (taskName.Contains("CheckoutPhase"))
                return "At Checkout";
            else if (taskName.Contains("ExitPhase"))
                return "Exiting";
            else if (taskName.Contains("ShoppingLoop"))
                return "Browsing Products";
            else if (taskName.EndsWith("Task"))
                return taskName.Replace("Task", "");
            else
                return taskName;
        }
        
        #endregion
        
        #region Legacy Interface Stubs - Behavior Designer Replacements
        
        // Legacy methods replaced by BD tasks - kept for compatibility
        public CustomerState GetCurrentState() => currentState;
        public bool SetRandomShelfDestination() { Debug.LogWarning("SetRandomShelfDestination called - use BD FindShelfTask instead"); return false; }
        public void PerformShoppingInteraction() { Debug.LogWarning("PerformShoppingInteraction called - use BD tasks instead"); }
        public bool IsSatisfiedWithShopping() { Debug.LogWarning("IsSatisfiedWithShopping called - use BD logic instead"); return true; }
        public float GetPreferredShoppingDuration() => shoppingTime;
        public void TrySelectProductsAtShelf(ShelfSlot shelf) { Debug.LogWarning("TrySelectProductsAtShelf called - use BD SelectProductTask instead"); }
        public void ResetShoppingState() { selectedProducts.Clear(); placedOnCounterProducts.Clear(); totalPurchaseAmount = 0f; }
        
        // Legacy lifecycle methods - now handled by BD
        public void StartCustomerLifecycle(CustomerState startingState) { Debug.LogWarning("StartCustomerLifecycle called - BD handles lifecycle now"); }
        public void StopCustomerLifecycle() { Debug.LogWarning("StopCustomerLifecycle called - BD handles lifecycle now"); }
        public System.Collections.IEnumerator HandleEnteringState() { Debug.LogWarning("HandleEnteringState called - use BD EnterShopTask instead"); yield break; }
        public System.Collections.IEnumerator HandleShoppingState() { Debug.LogWarning("HandleShoppingState called - use BD shopping tasks instead"); yield break; }
        public System.Collections.IEnumerator HandlePurchasingState() { Debug.LogWarning("HandlePurchasingState called - use BD checkout tasks instead"); yield break; }
        public System.Collections.IEnumerator HandleLeavingState() { Debug.LogWarning("HandleLeavingState called - use BD exit tasks instead"); yield break; }
        
        // Store hours methods - kept functional
        public bool IsStoreOpen() => GetIsStoreOpen();
        public bool ShouldLeaveStoreDueToHours() => GetShouldLeaveStoreDueToHours();
        public bool ShouldHurryUpShopping() => GetShouldHurryUpShopping();
        
        // Legacy checkout methods - now handled by BD tasks
        public System.Collections.IEnumerator PlaceItemsOnCounter(CheckoutCounter checkoutCounter) { Debug.LogWarning("PlaceItemsOnCounter called - use BD PlaceProductsTask instead"); yield break; }
        public System.Collections.IEnumerator WaitForCheckoutCompletion(CheckoutCounter checkoutCounter) { Debug.LogWarning("WaitForCheckoutCompletion called - use BD WaitForScanningTask instead"); yield break; }
        public System.Collections.IEnumerator CollectItemsAndLeave(CheckoutCounter checkoutCounter) { Debug.LogWarning("CollectItemsAndLeave called - use BD CompleteCheckoutTask instead"); yield break; }
        public void CheckQueueStatus() { Debug.Log(GetDebugInfo()); }
        public void DebugCustomerQueueState() { Debug.Log(GetDebugInfo()); }
        public void ForceLeaveQueue() { isInQueue = false; queuePosition = -1; queuedCheckout = null; waitingForCheckoutTurn = false; }
        
        // Legacy configuration methods
        public System.Collections.IEnumerator DelayedInitialization() { yield return null; }
        public void MigrateLegacyFields(float legacyShoppingTime, ShelfSlot legacyTargetShelf) { shoppingTime = legacyShoppingTime; targetShelf = legacyTargetShelf; }
        
        #endregion
        
        #region Utilities
        
        /// <summary>
        /// Get formatted debug info about customer's current state
        /// </summary>
        public string GetDebugInfo()
        {
            return $"Customer {name}: State={currentState}, InQueue={isInQueue}, QueuePos={queuePosition}, " +
                   $"WaitingTurn={waitingForCheckoutTurn}, WaitingCheckout={isWaitingForCheckout}, " +
                   $"Products={selectedProducts.Count}, Placed={placedOnCounterProducts.Count}";
        }
        
        #endregion
    }
}