using UnityEngine;
using UnityEngine.AI;
using System.Collections;

namespace TabletopShop
{
    /// <summary>
    /// Customer state enumeration for managing customer behavior
    /// </summary>
    public enum CustomerState
    {
        Entering,    // Customer is entering the shop
        Shopping,    // Customer is browsing/looking at products
        Purchasing,  // Customer is at checkout/purchase area
        Leaving      // Customer is exiting the shop
    }

    /// <summary>
    /// Customer AI controller using composition pattern with components for movement, behavior, and visuals.
    /// Maintains the same public API for backward compatibility while delegating responsibilities to specialized components.
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    public class Customer : MonoBehaviour
    {
        [Header("Required Components")]
        [SerializeField] private CustomerMovement customerMovement;
        [SerializeField] private CustomerBehavior customerBehavior;
        [SerializeField] private CustomerVisuals customerVisuals;
        
        // Component access properties - expose components directly for better flexibility
        public CustomerMovement Movement => customerMovement;
        public CustomerBehavior Behavior => customerBehavior;
        public CustomerVisuals Visuals => customerVisuals;
        
        // Legacy properties for backward compatibility (delegate to components)
        public CustomerState CurrentState => customerBehavior?.CurrentState ?? CustomerState.Entering;
        public float ShoppingTime => customerBehavior?.ShoppingTime ?? 15f; // Default fallback
        public ShelfSlot TargetShelf => customerBehavior?.TargetShelf;
        public bool IsMoving => customerMovement?.IsMoving ?? false;
        public Vector3 CurrentDestination => customerMovement?.CurrentDestination ?? Vector3.zero;
        public bool HasDestination => customerMovement?.HasDestination ?? false;
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            InitializeComponents();
            InitializeLegacyFallbacks();
        }
        
        private void Start()
        {
            Debug.Log($"Customer {name} initialized with state: {CurrentState}, shopping time: {ShoppingTime:F1}s");
            
            // Start the customer lifecycle state machine (delegate to behavior component)
            if (customerBehavior != null)
            {
                customerBehavior.StartCustomerLifecycle(CurrentState);
            }
            else
            {
                Debug.LogError($"Customer {name} cannot start lifecycle - CustomerBehavior component not found!");
            }
        }
        
        private void Update()
        {
            // All update logic is now handled by components
            // No legacy fallback needed
        }
        
        #endregion
        
        #region Component Initialization
        
        /// <summary>
        /// Initialize and setup components using dependency injection
        /// </summary>
        private void InitializeComponents()
        {
            // Validate required dependencies
            if (customerMovement == null || customerBehavior == null || customerVisuals == null)
            {
                Debug.LogError($"Customer {name} is missing required components! Use OnValidate to auto-assign or manually assign in inspector.");
                return;
            }
            
            // Initialize cross-component references
            customerMovement.Initialize();
            customerBehavior.Initialize(customerMovement, this);
            customerVisuals.Initialize(customerMovement, this);
            
            // Subscribe to component events
            customerBehavior.OnStateChangeRequested += HandleStateChangeRequest;
            customerBehavior.OnTargetShelfChanged += HandleTargetShelfChanged;
            
            // Migrate legacy field values
            MigrateLegacyFields();
            
            Debug.Log($"Customer {name} components initialized successfully");
        }
        
        /// <summary>
        /// Initialize minimal legacy compatibility
        /// </summary>
        private void InitializeLegacyFallbacks()
        {
            // Ensure components are properly initialized
            // Legacy fallbacks are now handled through component delegation
        }
        
        /// <summary>
        /// Handle state change requests from behavior component
        /// </summary>
        private void HandleStateChangeRequest(CustomerState fromState, CustomerState toState)
        {
            ChangeState(toState);
        }
        
        /// <summary>
        /// Handle target shelf changes from behavior component
        /// </summary>
        private void HandleTargetShelfChanged(ShelfSlot shelf)
        {
            // Target shelf is now managed by the behavior component
            // This event handler can be used for additional coordination if needed
        }
        
        /// <summary>
        /// Migrate legacy field values to components
        /// </summary>
        private void MigrateLegacyFields()
        {
            if (customerBehavior != null)
            {
                // Migration now handled by component initialization
                // Components initialize with their own default values
                customerBehavior.MigrateLegacyFields(UnityEngine.Random.Range(10f, 30f), null);
            }
            
            if (customerVisuals != null)
            {
                customerVisuals.MigrateLegacyFields(true); // showDebugGizmos default true
            }
        }
        
        #endregion
        
        #region State Management
        
        /// <summary>
        /// Change customer state and log the transition
        /// </summary>
        /// <param name="newState">The new state to transition to</param>
        public void ChangeState(CustomerState newState)
        {
            CustomerState previousState = CurrentState;
            
            // Delegate state change to behavior component
            if (customerBehavior != null)
            {
                customerBehavior.ChangeState(newState);
            }
            else
            {
                Debug.LogWarning($"Customer {name} cannot change state - CustomerBehavior component not found!");
                return;
            }
            
            Debug.Log($"Customer {name} state changed: {previousState} -> {CurrentState}");
            
            // Handle state-specific initialization
            OnStateChanged(previousState, CurrentState);
        }
        
        /// <summary>
        /// Get the current customer state
        /// </summary>
        /// <returns>Current CustomerState</returns>
        public CustomerState GetCurrentState()
        {
            return CurrentState;
        }
        
        /// <summary>
        /// Called when state changes to handle state-specific setup
        /// </summary>
        /// <param name="previousState">The previous state</param>
        /// <param name="newState">The new state</param>
        private void OnStateChanged(CustomerState previousState, CustomerState newState)
        {
            // Update visual color system based on new state
            if (customerVisuals != null)
            {
                customerVisuals.UpdateColorForState(newState);
            }
            
            switch (newState)
            {
                case CustomerState.Entering:
                    Debug.Log($"Customer {name} is entering the shop");
                    break;
                    
                case CustomerState.Shopping:
                    Debug.Log($"Customer {name} started shopping (duration: {ShoppingTime:F1}s)");
                    break;
                    
                case CustomerState.Purchasing:
                    Debug.Log($"Customer {name} is making a purchase");
                    break;
                    
                case CustomerState.Leaving:
                    Debug.Log($"Customer {name} is leaving the shop");
                    break;
            }
        }
        
        #endregion
        
        #region High-Level Customer Actions
        
        /// <summary>
        /// Start shopping behavior - high-level action that coordinates components
        /// </summary>
        public void StartShopping()
        {
            ChangeState(CustomerState.Shopping);
            
            if (Movement != null)
            {
                Movement.SetRandomShelfDestination();
            }
            else
            {
                Debug.LogError($"Customer {name} cannot start shopping - Movement component not available!");
            }
        }
        
        /// <summary>
        /// Start purchasing behavior - high-level action for checkout
        /// </summary>
        public void StartPurchasing()
        {
            ChangeState(CustomerState.Purchasing);
            
            if (Movement != null)
            {
                Movement.MoveToCheckoutPoint();
            }
            else
            {
                Debug.LogError($"Customer {name} cannot start purchasing - Movement component not available!");
            }
        }
        
        /// <summary>
        /// Start leaving behavior - high-level action for exiting
        /// </summary>
        public void StartLeaving()
        {
            ChangeState(CustomerState.Leaving);
            
            if (Movement != null)
            {
                Movement.MoveToExitPoint();
            }
            else
            {
                Debug.LogError($"Customer {name} cannot start leaving - Movement component not available!");
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
            // Delegate to behavior component for target shelf management
            if (customerBehavior != null)
            {
                customerBehavior.SetTargetShelf(shelf);
            }
            
            if (shelf != null)
            {
                Debug.Log($"Customer {name} targeting shelf: {shelf.name}");
            }
            else
            {
                Debug.Log($"Customer {name} cleared target shelf");
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
        
        #region Public Getters
        
        /// <summary>
        /// Get NavMeshAgent component reference
        /// </summary>
        /// <returns>NavMeshAgent component</returns>
        public NavMeshAgent GetNavMeshAgent()
        {
            return customerMovement?.NavMeshAgent ?? GetComponent<NavMeshAgent>();
        }
        
        /// <summary>
        /// Check if customer is currently in a specific state
        /// </summary>
        /// <param name="state">State to check</param>
        /// <returns>True if customer is in the specified state</returns>
        public bool IsInState(CustomerState state)
        {
            return CurrentState == state;
        }
        
        #endregion
        
        #region Debug and Visualization
        
        /// <summary>
        /// Get debug information about customer state
        /// Access customerVisuals.GetDebugInfo() directly for full debug functionality
        /// </summary>
        /// <returns>Debug string with customer information</returns>
        public string GetDebugInfo()
        {
            return Visuals?.GetDebugInfo() ?? $"Customer {name}: Debug info unavailable - CustomerVisuals component not found!";
        }
        
        #endregion
        
        #region Unity Editor Support
        
        private void OnValidate()
        {
            // Validate dependencies are assigned and auto-assign if missing
            if (!customerMovement) customerMovement = GetComponent<CustomerMovement>();
            if (!customerBehavior) customerBehavior = GetComponent<CustomerBehavior>();
            if (!customerVisuals) customerVisuals = GetComponent<CustomerVisuals>();
            
            // Warn if dependencies are still missing after auto-assignment attempt
            if (!customerMovement) Debug.LogWarning($"Customer {name} is missing CustomerMovement component!");
            if (!customerBehavior) Debug.LogWarning($"Customer {name} is missing CustomerBehavior component!");
            if (!customerVisuals) Debug.LogWarning($"Customer {name} is missing CustomerVisuals component!");
        }
        
        #endregion
    }
}
