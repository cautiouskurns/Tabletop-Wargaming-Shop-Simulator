using UnityEngine;

namespace TabletopShop
{
    /// <summary>
    /// Concrete implementation of the customer state machine manager.
    /// Handles state machine lifecycle, state registration, and operations.
    /// This class will be integrated into CustomerBehavior in Phase 3.
    /// </summary>
    public class CustomerStateMachineManager : ICustomerStateMachineManager
    {
        // State machine components
        private CustomerStateMachine stateMachine;
        private CustomerStateContext context;
        private bool isActive = false;
        
        // Component references (injected via constructor)
        private Customer customer;
        private CustomerMovement movement;
        private CustomerBehavior behavior;
        private CustomerVisuals visuals;
        
        // Events
        public event System.Action<CustomerState, CustomerState> OnStateChanged;
        public event System.Action<string> OnStateMachineError;
        
        // Properties
        public CustomerStateMachine StateMachine => stateMachine;
        public bool IsStateMachineActive => isActive && stateMachine?.IsInitialized == true;
        
        /// <summary>
        /// Constructor for CustomerStateMachineManager
        /// </summary>
        /// <param name="customer">Customer component</param>
        /// <param name="movement">Customer movement component</param>
        /// <param name="behavior">Customer behavior component</param>
        /// <param name="visuals">Customer visuals component</param>
        public CustomerStateMachineManager(Customer customer, CustomerMovement movement, 
                                         CustomerBehavior behavior, CustomerVisuals visuals)
        {
            this.customer = customer;
            this.movement = movement;
            this.behavior = behavior;
            this.visuals = visuals;
            
            Debug.Log($"CustomerStateMachineManager created for {customer?.name ?? "Unknown Customer"}");
        }
        
        /// <summary>
        /// Initialize the state machine infrastructure
        /// </summary>
        public void InitializeStateMachine()
        {
            if (stateMachine != null)
            {
                Debug.LogWarning("State machine already initialized!");
                return;
            }
            
            // Validate components
            if (customer == null || movement == null || behavior == null || visuals == null)
            {
                Debug.LogError("Cannot initialize state machine - missing required components!");
                return;
            }
            
            // Create state context
            context = new CustomerStateContext(customer, movement, behavior, visuals);
            
            // Create state machine
            stateMachine = new CustomerStateMachine(context);
            
            // Subscribe to state machine events
            stateMachine.OnStateChanged += HandleStateChanged;
            stateMachine.OnStateMachineError += HandleStateMachineError;
            
            Debug.Log($"State machine initialized for {customer.name}");
        }
        
        /// <summary>
        /// Start the state machine with all states registered
        /// </summary>
        /// <param name="initialState">Initial state to start with</param>
        public void StartStateMachine(CustomerState initialState = CustomerState.Entering)
        {
            if (stateMachine == null)
            {
                Debug.LogError("Cannot start state machine - not initialized!");
                return;
            }
            
            if (isActive)
            {
                Debug.LogWarning("State machine already active!");
                return;
            }
            
            // Register all states
            RegisterAllStates();
            
            // Initialize and start the state machine
            stateMachine.Initialize(initialState);
            isActive = true;
            
            Debug.Log($"State machine started for {customer.name} with initial state: {initialState}");
        }
        
        /// <summary>
        /// Stop the state machine
        /// </summary>
        public void StopStateMachine()
        {
            if (!isActive)
            {
                return;
            }
            
            isActive = false;
            Debug.Log($"State machine stopped for {customer?.name ?? "Unknown Customer"}");
        }
        
        /// <summary>
        /// Clean up the state machine and resources
        /// </summary>
        public void CleanupStateMachine()
        {
            if (stateMachine != null)
            {
                // Unsubscribe from events
                stateMachine.OnStateChanged -= HandleStateChanged;
                stateMachine.OnStateMachineError -= HandleStateMachineError;
                
                // Clean up state machine
                stateMachine.Cleanup();
                stateMachine = null;
            }
            
            context = null;
            isActive = false;
            
            Debug.Log($"State machine cleaned up for {customer?.name ?? "Unknown Customer"}");
        }
        
        /// <summary>
        /// Register all customer states with the state machine
        /// </summary>
        public void RegisterAllStates()
        {
            if (stateMachine == null)
            {
                Debug.LogError("Cannot register states - state machine not initialized!");
                return;
            }
            
            CustomerStateFactory.RegisterAllStates(stateMachine);
        }
        
        /// <summary>
        /// Register a specific state with the state machine
        /// </summary>
        /// <param name="state">State to register</param>
        public void RegisterState(ICustomerState state)
        {
            if (stateMachine == null)
            {
                Debug.LogError("Cannot register state - state machine not initialized!");
                return;
            }
            
            stateMachine.RegisterState(state);
        }
        
        /// <summary>
        /// Request a state change
        /// </summary>
        /// <param name="newState">Target state</param>
        /// <param name="reason">Reason for transition</param>
        /// <returns>True if transition was queued successfully</returns>
        public bool ChangeState(CustomerState newState, string reason = "")
        {
            if (!IsStateMachineActive)
            {
                Debug.LogWarning("Cannot change state - state machine not active!");
                return false;
            }
            
            return stateMachine.ChangeState(newState, reason);
        }
        
        /// <summary>
        /// Force a state change without validation
        /// </summary>
        /// <param name="newState">Target state</param>
        /// <param name="reason">Reason for forced transition</param>
        public void ForceChangeState(CustomerState newState, string reason = "")
        {
            if (stateMachine == null)
            {
                Debug.LogWarning("Cannot force state change - state machine not initialized!");
                return;
            }
            
            stateMachine.ForceChangeState(newState, reason);
        }
        
        /// <summary>
        /// Get comprehensive debug information about the state machine
        /// </summary>
        /// <returns>Debug information string</returns>
        public string GetStateMachineDebugInfo()
        {
            if (stateMachine == null)
            {
                return "State Machine: Not Initialized";
            }
            
            if (!isActive)
            {
                return "State Machine: Initialized but not active";
            }
            
            string machineInfo = stateMachine.GetDebugInfo();
            string contextInfo = context?.GetDebugInfo() ?? "No context";
            
            return $"=== State Machine Manager ===\n" +
                   $"Active: {isActive}\n" +
                   $"Customer: {customer?.name ?? "null"}\n" +
                   $"=== State Machine ===\n" +
                   $"{machineInfo}\n" +
                   $"=== Context ===\n" +
                   $"{contextInfo}";
        }
        
        /// <summary>
        /// Get the current state
        /// </summary>
        /// <returns>Current customer state</returns>
        public CustomerState GetCurrentState()
        {
            return stateMachine?.CurrentStateType ?? CustomerState.Entering;
        }
        
        /// <summary>
        /// Check if customer is in a specific state
        /// </summary>
        /// <param name="state">State to check</param>
        /// <returns>True if in the specified state</returns>
        public bool IsInState(CustomerState state)
        {
            return GetCurrentState() == state;
        }
        
        /// <summary>
        /// Update the state machine (call from MonoBehaviour Update)
        /// </summary>
        public void Update()
        {
            if (IsStateMachineActive)
            {
                stateMachine.Update();
            }
        }
        
        #region Event Handlers
        
        /// <summary>
        /// Handle state change events from the state machine
        /// </summary>
        /// <param name="fromState">Previous state</param>
        /// <param name="toState">New state</param>
        private void HandleStateChanged(CustomerState fromState, CustomerState toState)
        {
            Debug.Log($"[{customer?.name ?? "Unknown"}] State changed: {fromState} -> {toState}");
            OnStateChanged?.Invoke(fromState, toState);
        }
        
        /// <summary>
        /// Handle error events from the state machine
        /// </summary>
        /// <param name="error">Error message</param>
        private void HandleStateMachineError(string error)
        {
            Debug.LogError($"[{customer?.name ?? "Unknown"}] State machine error: {error}");
            OnStateMachineError?.Invoke(error);
        }
        
        #endregion
    }
}
