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
    /// Customer AI coordinator using pure composition pattern.
    /// 
    /// DESIGN PATTERN: Pure Coordinator
    /// - Provides direct access to specialized components
    /// - Coordinates high-level actions that involve multiple components
    /// - NO delegation methods - use direct component access instead
    /// 

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
        
        // Note: Legacy properties removed - use direct component access instead:
        // customer.Behavior.CurrentState, customer.Movement.IsMoving, etc.

        #region Unity Lifecycle
        
        private void Awake()
        {
            try
            {
                Debug.Log($"Customer {name}: Awake() START");
                
                // Comment out the initialization temporarily
                InitializeComponents();
                InitializeLegacyFallbacks();
                
                Debug.Log($"Customer {name}: Awake() SUCCESS");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Customer {name}: CRASH in Awake(): {ex.Message}\nStack: {ex.StackTrace}");
            }
        }

        private void Start()
        {
            try
            {
                Debug.Log($"Customer {name}: Start() BEGIN");
                
                var currentState = customerBehavior?.CurrentState ?? CustomerState.Entering;
                var shoppingTime = customerBehavior?.ShoppingTime ?? 15f;
                
                Debug.Log($"Customer {name} initialized with state: {currentState}, shopping time: {shoppingTime:F1}s");
                
                if (customerBehavior != null)
                {
                    customerBehavior.StartCustomerLifecycle(currentState);
                }
                else
                {
                    Debug.LogError($"Customer {name} cannot start lifecycle - CustomerBehavior component not found!");
                }
                
                Debug.Log($"Customer {name}: Start() SUCCESS");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Customer {name}: CRASH in Start(): {ex.Message}\nStack: {ex.StackTrace}");
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
            // ✅ COMMENTING OUT CIRCULAR EVENT - this was causing infinite recursion
            // customerBehavior.OnStateChangeRequested += HandleStateChangeRequest;
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
        /// Simplified to just update visuals - no circular calls
        /// </summary>
        private void HandleStateChangeRequest(CustomerState fromState, CustomerState toState)
        {
            // Just update visuals directly - no delegation or circular events
            if (customerVisuals != null)
            {
                customerVisuals.UpdateColorForState(toState);
            }
            
            // Log the change for debugging
            Debug.Log($"Customer {name} state change notification: {fromState} -> {toState}");
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
        
        // Note: State management methods removed - use direct component access:
        // customer.Behavior.ChangeState(state), customer.Behavior.CurrentState, etc.
        
        #region High-Level Customer Actions
        
        /// <summary>
        /// Start shopping behavior - high-level action that coordinates components
        /// </summary>
        public void StartShopping()
        {
            // Direct component coordination - no delegation
            if (customerBehavior != null)
            {
                customerBehavior.ChangeState(CustomerState.Shopping);
            }
            
            if (customerMovement != null)
            {
                customerMovement.SetRandomShelfDestination();
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
            // Direct component coordination - no delegation
            if (customerBehavior != null)
            {
                customerBehavior.ChangeState(CustomerState.Purchasing);
            }
            
            if (customerMovement != null)
            {
                customerMovement.MoveToCheckoutPoint();
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
            // Direct component coordination - no delegation
            if (customerBehavior != null)
            {
                customerBehavior.ChangeState(CustomerState.Leaving);
            }
            
            if (customerMovement != null)
            {
                customerMovement.MoveToExitPoint();
            }
            else
            {
                Debug.LogError($"Customer {name} cannot start leaving - Movement component not available!");
            }
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
