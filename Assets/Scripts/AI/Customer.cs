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
        [Header("Customer State")]
        [SerializeField] private CustomerState currentState = CustomerState.Entering;
        
        [Header("Shopping Configuration")]
        [SerializeField] private float shoppingTime;
        [SerializeField] private ShelfSlot targetShelf;
        
        [Header("Movement Settings")]
        [SerializeField] private float movementSpeed = 1.5f;
        [SerializeField] private float stoppingDistance = 1f;
        
        // Component references - composition pattern
        private CustomerMovement customerMovement;
        private CustomerBehavior customerBehavior;
        private CustomerVisuals customerVisuals;
        
        // Legacy component reference (kept for minimal compatibility)
        private NavMeshAgent navMeshAgent;
        
        // Properties - delegating to components
        public CustomerState CurrentState => currentState;
        public float ShoppingTime => customerBehavior != null ? customerBehavior.ShoppingTime : shoppingTime;
        public ShelfSlot TargetShelf => customerBehavior != null ? customerBehavior.TargetShelf : targetShelf;
        public bool IsMoving => customerMovement != null ? customerMovement.IsMoving : false;
        public Vector3 CurrentDestination => customerMovement != null ? customerMovement.CurrentDestination : Vector3.zero;
        public bool HasDestination => customerMovement != null ? customerMovement.HasDestination : false;
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            InitializeComponents();
            InitializeLegacyFallbacks();
        }
        
        private void Start()
        {
            Debug.Log($"Customer {name} initialized with state: {currentState}, shopping time: {ShoppingTime:F1}s");
            
            // Start the customer lifecycle state machine (delegate to behavior component)
            if (customerBehavior != null)
            {
                customerBehavior.StartCustomerLifecycle(currentState);
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
        /// Initialize and setup components using composition pattern
        /// </summary>
        private void InitializeComponents()
        {
            // Get or create movement component
            customerMovement = EnsureMovementComponent();
            
            // Get or create behavior component  
            customerBehavior = EnsureBehaviorComponent();
            
            // Get or create visuals component
            customerVisuals = EnsureVisualsComponent();
            
            // Initialize cross-component references
            if (customerMovement != null && customerBehavior != null && customerVisuals != null)
            {
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
        }
        
        /// <summary>
        /// Initialize minimal legacy compatibility
        /// </summary>
        private void InitializeLegacyFallbacks()
        {
            // Keep NavMeshAgent reference for basic compatibility
            navMeshAgent = GetComponent<NavMeshAgent>();
            
            // Initialize legacy shopping time if not set
            if (shoppingTime <= 0)
            {
                shoppingTime = UnityEngine.Random.Range(10f, 30f);
            }
        }
        
        /// <summary>
        /// Ensure CustomerMovement component exists and return reference
        /// </summary>
        private CustomerMovement EnsureMovementComponent()
        {
            CustomerMovement movement = GetComponent<CustomerMovement>();
            if (movement == null)
            {
                movement = gameObject.AddComponent<CustomerMovement>();
                Debug.Log($"Customer {name} created CustomerMovement component");
            }
            return movement;
        }
        
        /// <summary>
        /// Ensure CustomerBehavior component exists and return reference
        /// </summary>
        private CustomerBehavior EnsureBehaviorComponent()
        {
            CustomerBehavior behavior = GetComponent<CustomerBehavior>();
            if (behavior == null)
            {
                behavior = gameObject.AddComponent<CustomerBehavior>();
                Debug.Log($"Customer {name} created CustomerBehavior component");
            }
            return behavior;
        }
        
        /// <summary>
        /// Ensure CustomerVisuals component exists and return reference
        /// </summary>
        private CustomerVisuals EnsureVisualsComponent()
        {
            CustomerVisuals visuals = GetComponent<CustomerVisuals>();
            if (visuals == null)
            {
                visuals = gameObject.AddComponent<CustomerVisuals>();
                Debug.Log($"Customer {name} created CustomerVisuals component");
            }
            return visuals;
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
            targetShelf = shelf;
        }
        
        /// <summary>
        /// Migrate legacy field values to components
        /// </summary>
        private void MigrateLegacyFields()
        {
            if (customerBehavior != null)
            {
                customerBehavior.MigrateLegacyFields(shoppingTime, targetShelf);
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
            CustomerState previousState = currentState;
            currentState = newState;
            
            Debug.Log($"Customer {name} state changed: {previousState} -> {currentState}");
            
            // Handle state-specific initialization
            OnStateChanged(previousState, currentState);
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
                    Debug.Log($"Customer {name} started shopping (duration: {shoppingTime:F1}s)");
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
        
        #region Movement and Pathfinding - Delegated to CustomerMovement Component
        
        /// <summary>
        /// Set movement destination using NavMeshAgent with pathfinding validation
        /// </summary>
        /// <param name="destination">Target position to move to</param>
        /// <returns>True if destination was set successfully</returns>
        public bool SetDestination(Vector3 destination)
        {
            // Delegate to movement component
            if (customerMovement != null)
            {
                return customerMovement.SetDestination(destination);
            }
            
            Debug.LogError($"Customer {name} cannot move - CustomerMovement component not found!");
            return false;
        }
        
        /// <summary>
        /// Set destination to a random shelf in the scene
        /// </summary>
        /// <returns>True if a random shelf destination was set successfully</returns>
        public bool SetRandomShelfDestination()
        {
            // Delegate to movement component
            if (customerMovement != null)
            {
                return customerMovement.SetRandomShelfDestination();
            }
            
            Debug.LogError($"Customer {name} cannot move - CustomerMovement component not found!");
            return false;
        }
        
        /// <summary>
        /// Move to a specific shelf position
        /// </summary>
        /// <param name="shelf">Target shelf to move to</param>
        /// <returns>True if movement was initiated successfully</returns>
        public bool MoveToShelfPosition(ShelfSlot shelf)
        {
            // Delegate to movement component
            if (customerMovement != null)
            {
                return customerMovement.MoveToShelfPosition(shelf);
            }
            
            Debug.LogError($"Customer {name} cannot move - CustomerMovement component not found!");
            return false;
        }
        
        /// <summary>
        /// Move to an exit point (for leaving the shop)
        /// </summary>
        /// <returns>True if exit destination was set successfully</returns>
        public bool MoveToExitPoint()
        {
            // Delegate to movement component
            if (customerMovement != null)
            {
                return customerMovement.MoveToExitPoint();
            }
            
            Debug.LogError($"Customer {name} cannot move - CustomerMovement component not found!");
            return false;
        }
        
        /// <summary>
        /// Stop current movement and clear destination
        /// </summary>
        public void StopMovement()
        {
            // Delegate to movement component
            if (customerMovement != null)
            {
                customerMovement.StopMovement();
                return;
            }
            
            Debug.LogError($"Customer {name} cannot stop movement - CustomerMovement component not found!");
        }
        
        /// <summary>
        /// Move to checkout point for purchasing
        /// </summary>
        /// <returns>True if checkout destination was set successfully</returns>
        public bool MoveToCheckoutPoint()
        {
            // Delegate to movement component
            if (customerMovement != null)
            {
                return customerMovement.MoveToCheckoutPoint();
            }
            
            Debug.LogError($"Customer {name} cannot move - CustomerMovement component not found!");
            return false;
        }
        
        /// <summary>
        /// Check if customer has reached destination
        /// </summary>
        /// <returns>True if customer has reached their destination</returns>
        public bool HasReachedDestination()
        {
            // Delegate to movement component
            if (customerMovement != null)
            {
                return customerMovement.HasReachedDestination();
            }
            
            Debug.LogError($"Customer {name} cannot check destination - CustomerMovement component not found!");
            return true; // Return true to avoid infinite loops
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
            return navMeshAgent;
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
        
        #region Debug and Visualization - Delegated to CustomerVisuals Component
        
        /// <summary>
        /// Get debug information about customer state
        /// </summary>
        /// <returns>Debug string with customer information</returns>
        public string GetDebugInfo()
        {
            // Delegate to visuals component
            if (customerVisuals != null)
            {
                return customerVisuals.GetDebugInfo();
            }
            
            return $"Customer {name}: Debug info unavailable - CustomerVisuals component not found!";
        }
        
        /// <summary>
        /// Draw debug gizmos for destination and pathfinding
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            // Delegate to visuals component
            // The CustomerVisuals component handles its own OnDrawGizmosSelected
            // No fallback needed here as gizmos are optional
        }
        
        #endregion
        
        #region Unity Editor Support
        
        private void OnValidate()
        {
            // Ensure valid ranges in editor
            movementSpeed = Mathf.Max(0.1f, movementSpeed);
            stoppingDistance = Mathf.Max(0.1f, stoppingDistance);
            shoppingTime = Mathf.Max(1f, shoppingTime);
        }
        
        #endregion
    }
}
