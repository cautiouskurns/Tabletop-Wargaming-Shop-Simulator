using UnityEngine;
using UnityEngine.AI;

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
    /// Customer AI controller with basic state management and NavMeshAgent movement
    /// Handles customer behavior states and movement within the shop
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
        
        // Component references
        private NavMeshAgent navMeshAgent;
        
        // Properties
        public CustomerState CurrentState => currentState;
        public float ShoppingTime => shoppingTime;
        public ShelfSlot TargetShelf => targetShelf;
        public bool IsMoving => navMeshAgent != null && navMeshAgent.velocity.magnitude > 0.1f;
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            InitializeNavMeshAgent();
            InitializeShoppingTime();
        }
        
        private void Start()
        {
            Debug.Log($"Customer {name} initialized with state: {currentState}, shopping time: {shoppingTime:F1}s");
        }
        
        private void Update()
        {
            // Basic movement and state monitoring
            UpdateMovementState();
        }
        
        #endregion
        
        #region Initialization
        
        /// <summary>
        /// Initialize NavMeshAgent component and configure movement settings
        /// </summary>
        private void InitializeNavMeshAgent()
        {
            navMeshAgent = GetComponent<NavMeshAgent>();
            
            if (navMeshAgent == null)
            {
                Debug.LogError($"Customer {name} is missing NavMeshAgent component!");
                return;
            }
            
            // Configure NavMeshAgent settings
            navMeshAgent.speed = movementSpeed;
            navMeshAgent.stoppingDistance = stoppingDistance;
            navMeshAgent.autoBraking = true;
            navMeshAgent.autoRepath = true;
            
            Debug.Log($"Customer {name} NavMeshAgent initialized - Speed: {movementSpeed}, Stopping Distance: {stoppingDistance}");
        }
        
        /// <summary>
        /// Initialize random shopping time between 10-30 seconds
        /// </summary>
        private void InitializeShoppingTime()
        {
            shoppingTime = Random.Range(10f, 30f);
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
        
        #region Movement
        
        /// <summary>
        /// Set movement destination using NavMeshAgent
        /// </summary>
        /// <param name="destination">Target position to move to</param>
        /// <returns>True if destination was set successfully</returns>
        public bool SetDestination(Vector3 destination)
        {
            if (navMeshAgent == null)
            {
                Debug.LogWarning($"Customer {name} cannot move - NavMeshAgent not found!");
                return false;
            }
            
            bool pathSet = navMeshAgent.SetDestination(destination);
            
            if (pathSet)
            {
                Debug.Log($"Customer {name} moving to destination: {destination}");
            }
            else
            {
                Debug.LogWarning($"Customer {name} failed to set destination: {destination}");
            }
            
            return pathSet;
        }
        
        /// <summary>
        /// Stop current movement
        /// </summary>
        public void StopMovement()
        {
            if (navMeshAgent != null && navMeshAgent.isActiveAndEnabled)
            {
                navMeshAgent.ResetPath();
                Debug.Log($"Customer {name} stopped movement");
            }
        }
        
        /// <summary>
        /// Check if customer has reached destination
        /// </summary>
        /// <returns>True if customer has reached their destination</returns>
        public bool HasReachedDestination()
        {
            if (navMeshAgent == null) return true;
            
            return !navMeshAgent.pathPending && 
                   navMeshAgent.remainingDistance < navMeshAgent.stoppingDistance;
        }
        
        /// <summary>
        /// Update movement state monitoring
        /// </summary>
        private void UpdateMovementState()
        {
            if (navMeshAgent == null) return;
            
            // Debug movement information (can be removed in production)
            if (IsMoving && Time.frameCount % 60 == 0) // Log every 60 frames (~1 second)
            {
                Debug.Log($"Customer {name} moving - Remaining distance: {navMeshAgent.remainingDistance:F2}");
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
        
        #region Debug
        
        /// <summary>
        /// Get debug information about customer state
        /// </summary>
        /// <returns>Debug string with customer information</returns>
        public string GetDebugInfo()
        {
            return $"Customer {name}: State={currentState}, " +
                   $"ShoppingTime={shoppingTime:F1}s, " +
                   $"IsMoving={IsMoving}, " +
                   $"TargetShelf={targetShelf?.name ?? "None"}";
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
