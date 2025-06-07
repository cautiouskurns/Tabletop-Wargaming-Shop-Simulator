using UnityEngine;
using System.Collections;
using System;

namespace TabletopShop
{
    /// <summary>
    /// Handles customer AI behavior, lifecycle state machine, and shopping patterns.
    /// Manages the complete customer experience from entering to leaving the shop.
    /// </summary>
    public class CustomerBehavior : MonoBehaviour
    {
        [Header("Shopping Configuration")]
        [SerializeField] private float shoppingTime;
        [SerializeField] private ShelfSlot targetShelf;
        
        // Component references
        private CustomerMovement customerMovement;
        private Customer mainCustomer; // Reference to main customer for state changes
        
        // State tracking
        private Coroutine lifecycleCoroutine;
        
        // Events
        public event Action<CustomerState, CustomerState> OnStateChangeRequested;
        public event Action<ShelfSlot> OnTargetShelfChanged;
        
        // Properties
        public float ShoppingTime => shoppingTime;
        public ShelfSlot TargetShelf => targetShelf;
        
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
        
        #endregion
        
        #region Customer Lifecycle State Machine
        
        /// <summary>
        /// Start the complete customer lifecycle automatically
        /// Progresses through: Entering → Shopping → Purchasing → Leaving
        /// </summary>
        public void StartCustomerLifecycle(CustomerState startingState)
        {
            if (lifecycleCoroutine != null)
            {
                StopCoroutine(lifecycleCoroutine);
            }
            
            lifecycleCoroutine = StartCoroutine(CustomerLifecycleCoroutine(startingState));
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
        private IEnumerator CustomerLifecycleCoroutine(CustomerState currentState)
        {
            Debug.Log($"CustomerBehavior {name} starting lifecycle in state: {currentState}");
            
            // Phase 1: Entering the shop
            if (currentState == CustomerState.Entering)
            {
                yield return StartCoroutine(HandleEnteringState());
                currentState = CustomerState.Shopping;
                OnStateChangeRequested?.Invoke(CustomerState.Entering, CustomerState.Shopping);
            }
            
            // Phase 2: Shopping behavior
            if (currentState == CustomerState.Shopping)
            {
                yield return StartCoroutine(HandleShoppingState());
                currentState = CustomerState.Purchasing;
                OnStateChangeRequested?.Invoke(CustomerState.Shopping, CustomerState.Purchasing);
            }
            
            // Phase 3: Purchasing (move to checkout area)
            if (currentState == CustomerState.Purchasing)
            {
                yield return StartCoroutine(HandlePurchasingState());
                currentState = CustomerState.Leaving;
                OnStateChangeRequested?.Invoke(CustomerState.Purchasing, CustomerState.Leaving);
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
                // Wait for customer to reach shelf
                while (!customerMovement.HasReachedDestination())
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
            while (shoppedTime < shoppingTime)
            {
                // Occasionally move to different shelves while shopping
                if (UnityEngine.Random.value < 0.3f) // 30% chance every check
                {
                    SetRandomShelfDestination();
                    
                    // Wait a bit for movement
                    yield return new WaitForSeconds(2f);
                    
                    // Wait to reach new shelf
                    while (!customerMovement.HasReachedDestination())
                    {
                        yield return new WaitForSeconds(0.5f);
                    }
                }
                
                yield return new WaitForSeconds(1f);
                shoppedTime += 1f;
            }
        }
        
        /// <summary>
        /// Handle purchasing state behavior
        /// </summary>
        private IEnumerator HandlePurchasingState()
        {
            Debug.Log($"CustomerBehavior {name} proceeding to checkout");
            
            bool reachedCheckout = customerMovement.MoveToCheckoutPoint();
            
            if (reachedCheckout)
            {
                // Wait to reach checkout
                while (!customerMovement.HasReachedDestination())
                {
                    yield return new WaitForSeconds(0.5f);
                }
                
                // Simulate purchase time
                float purchaseTime = UnityEngine.Random.Range(3f, 8f);
                Debug.Log($"CustomerBehavior {name} making purchase (taking {purchaseTime:F1}s)");
                yield return new WaitForSeconds(purchaseTime);
            }
        }
        
        /// <summary>
        /// Handle leaving state behavior
        /// </summary>
        private IEnumerator HandleLeavingState()
        {
            Debug.Log($"CustomerBehavior {name} leaving the shop");
            
            bool foundExit = customerMovement.MoveToExitPoint();
            if (foundExit)
            {
                // Wait to reach exit
                while (!customerMovement.HasReachedDestination())
                {
                    yield return new WaitForSeconds(0.5f);
                }
                
                Debug.Log($"CustomerBehavior {name} has left the shop");
                
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
            
            // Move to shelf position using movement component
            return customerMovement.MoveToShelfPosition(randomShelf);
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
    }
}
