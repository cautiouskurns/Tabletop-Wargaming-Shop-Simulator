using UnityEngine;
using System.Collections.Generic;
using System;

namespace TabletopShop
{
    /// <summary>
    /// Core state machine that manages customer state transitions and execution.
    /// Handles state registration, validation, transitions, and update loops.
    /// </summary>
    public class CustomerStateMachine
    {
        // State management
        private Dictionary<CustomerState, ICustomerState> states;
        private ICustomerState currentState;
        private CustomerStateContext context;
        private bool isInitialized = false;
        
        // State transition tracking
        private Queue<StateTransition> pendingTransitions;
        private List<StateTransition> transitionHistory;
        private const int MaxHistorySize = 50;
        
        // Events
        public event Action<CustomerState, CustomerState> OnStateChanged;
        public event Action<CustomerState> OnStateEntered;
        public event Action<CustomerState> OnStateExited;
        public event Action<string> OnStateMachineError;
        
        // Properties
        public CustomerState CurrentStateType => currentState?.GetStateType() ?? CustomerState.Entering;
        public string CurrentStateName => currentState?.StateName ?? "None";
        public bool IsInitialized => isInitialized;
        public int TransitionHistoryCount => transitionHistory?.Count ?? 0;
        
        /// <summary>
        /// State transition data structure for tracking and debugging
        /// </summary>
        public struct StateTransition
        {
            public CustomerState FromState;
            public CustomerState ToState;
            public float Timestamp;
            public string Reason;
            public bool WasSuccessful;
            
            public StateTransition(CustomerState from, CustomerState to, string reason, bool successful)
            {
                FromState = from;
                ToState = to;
                Timestamp = Time.time;
                Reason = reason;
                WasSuccessful = successful;
            }
        }
        
        /// <summary>
        /// Constructor for CustomerStateMachine
        /// </summary>
        /// <param name="context">Customer state context</param>
        public CustomerStateMachine(CustomerStateContext context)
        {
            this.context = context;
            states = new Dictionary<CustomerState, ICustomerState>();
            pendingTransitions = new Queue<StateTransition>();
            transitionHistory = new List<StateTransition>();
            
            // Set state machine reference in context
            context.SetStateMachine(this);
            
            // Subscribe to context events
            context.OnStateTransitionRequested += HandleStateTransitionRequest;
            
            context.LogDebug("State machine created");
        }
        
        /// <summary>
        /// Initialize the state machine with all required states
        /// </summary>
        /// <param name="initialState">The starting state</param>
        public void Initialize(CustomerState initialState = CustomerState.Entering)
        {
            if (isInitialized)
            {
                context.LogWarning("State machine already initialized!");
                return;
            }
            
            // Validate that we have all required states
            ValidateRequiredStates();
            
            // Set initial state
            if (states.ContainsKey(initialState))
            {
                currentState = states[initialState];
                currentState.OnEnter(context);
                isInitialized = true;
                
                context.LogDebug($"State machine initialized with initial state: {initialState}");
                OnStateEntered?.Invoke(initialState);
            }
            else
            {
                string error = $"Cannot initialize: Initial state {initialState} not registered!";
                context.LogError(error);
                OnStateMachineError?.Invoke(error);
            }
        }
        
        /// <summary>
        /// Register a state with the state machine
        /// </summary>
        /// <param name="state">The state to register</param>
        public void RegisterState(ICustomerState state)
        {
            if (state == null)
            {
                context.LogError("Cannot register null state!");
                return;
            }
            
            CustomerState stateType = state.GetStateType();
            
            if (states.ContainsKey(stateType))
            {
                context.LogWarning($"State {stateType} already registered! Replacing existing state.");
            }
            
            states[stateType] = state;
            context.LogDebug($"Registered state: {stateType} ({state.StateName})");
        }
        
        /// <summary>
        /// Update the state machine (call from MonoBehaviour Update)
        /// </summary>
        public void Update()
        {
            if (!isInitialized)
            {
                return;
            }
            
            // Process pending transitions first
            ProcessPendingTransitions();
            
            // Update current state
            if (currentState != null)
            {
                try
                {
                    currentState.OnUpdate(context);
                }
                catch (Exception ex)
                {
                    string error = $"Error in {currentState.StateName} OnUpdate: {ex.Message}";
                    context.LogError(error);
                    OnStateMachineError?.Invoke(error);
                }
            }
        }
        
        /// <summary>
        /// Request a state change with validation
        /// </summary>
        /// <param name="newState">The state to transition to</param>
        /// <param name="reason">Reason for transition (for debugging)</param>
        /// <returns>True if transition was queued successfully</returns>
        public bool ChangeState(CustomerState newState, string reason = "")
        {
            if (!isInitialized)
            {
                context.LogError("Cannot change state: State machine not initialized!");
                return false;
            }
            
            if (currentState == null)
            {
                context.LogError("Cannot change state: No current state!");
                return false;
            }
            
            // Don't queue duplicate transitions
            CustomerState currentStateType = currentState.GetStateType();
            if (currentStateType == newState)
            {
                context.LogDebug($"Ignoring transition to same state: {newState}");
                return false;
            }
            
            // Validate transition
            if (!currentState.CanTransitionTo(newState, context))
            {
                string error = $"Invalid transition: {currentStateType} -> {newState}";
                context.LogWarning(error);
                RecordTransition(currentStateType, newState, reason, false);
                return false;
            }
            
            // Check if target state exists
            if (!states.ContainsKey(newState))
            {
                string error = $"Target state {newState} not registered!";
                context.LogError(error);
                OnStateMachineError?.Invoke(error);
                RecordTransition(currentStateType, newState, reason, false);
                return false;
            }
            
            // Queue the transition
            var transition = new StateTransition(currentStateType, newState, reason, true);
            pendingTransitions.Enqueue(transition);
            
            context.LogDebug($"Queued state transition: {currentStateType} -> {newState}. Reason: {reason}");
            return true;
        }
        
        /// <summary>
        /// Force a state change without validation (use with caution)
        /// </summary>
        /// <param name="newState">The state to force transition to</param>
        /// <param name="reason">Reason for forced transition</param>
        public void ForceChangeState(CustomerState newState, string reason = "Forced")
        {
            if (!states.ContainsKey(newState))
            {
                context.LogError($"Cannot force change to unregistered state: {newState}");
                return;
            }
            
            CustomerState previousState = currentState?.GetStateType() ?? CustomerState.Entering;
            
            // Exit current state
            if (currentState != null)
            {
                try
                {
                    currentState.OnExit(context);
                    OnStateExited?.Invoke(previousState);
                }
                catch (Exception ex)
                {
                    context.LogError($"Error exiting {currentState.StateName}: {ex.Message}");
                }
            }
            
            // Enter new state
            currentState = states[newState];
            try
            {
                currentState.OnEnter(context);
                OnStateEntered?.Invoke(newState);
                OnStateChanged?.Invoke(previousState, newState);
                
                context.LogDebug($"FORCED state change: {previousState} -> {newState}. Reason: {reason}");
                RecordTransition(previousState, newState, $"FORCED: {reason}", true);
            }
            catch (Exception ex)
            {
                string error = $"Error entering {currentState.StateName}: {ex.Message}";
                context.LogError(error);
                OnStateMachineError?.Invoke(error);
            }
        }
        
        /// <summary>
        /// Get current state debug information
        /// </summary>
        /// <returns>Debug information string</returns>
        public string GetDebugInfo()
        {
            if (!isInitialized)
            {
                return "State Machine: Not Initialized";
            }
            
            string stateInfo = currentState?.GetDebugInfo(context) ?? "No current state";
            string transitionInfo = $"Transitions: {transitionHistory.Count}, Pending: {pendingTransitions.Count}";
            
            return $"{stateInfo}\n{transitionInfo}";
        }
        
        /// <summary>
        /// Get recent transition history for debugging
        /// </summary>
        /// <param name="count">Number of recent transitions to return</param>
        /// <returns>List of recent transitions</returns>
        public List<StateTransition> GetRecentTransitions(int count = 10)
        {
            if (transitionHistory == null || transitionHistory.Count == 0)
            {
                return new List<StateTransition>();
            }
            
            int startIndex = Mathf.Max(0, transitionHistory.Count - count);
            int takeCount = Mathf.Min(count, transitionHistory.Count);
            
            return transitionHistory.GetRange(startIndex, takeCount);
        }
        
        /// <summary>
        /// Clean up the state machine
        /// </summary>
        public void Cleanup()
        {
            // Exit current state
            if (currentState != null)
            {
                try
                {
                    currentState.OnExit(context);
                }
                catch (Exception ex)
                {
                    context.LogError($"Error during cleanup exit: {ex.Message}");
                }
            }
            
            // Unsubscribe from events
            if (context != null)
            {
                context.OnStateTransitionRequested -= HandleStateTransitionRequest;
            }
            
            // Clear data
            states?.Clear();
            pendingTransitions?.Clear();
            currentState = null;
            isInitialized = false;
            
            context.LogDebug("State machine cleaned up");
        }
        
        #region Private Methods
        
        /// <summary>
        /// Process all pending state transitions
        /// </summary>
        private void ProcessPendingTransitions()
        {
            while (pendingTransitions.Count > 0)
            {
                var transition = pendingTransitions.Dequeue();
                ExecuteTransition(transition);
            }
        }
        
        /// <summary>
        /// Execute a single state transition
        /// </summary>
        /// <param name="transition">The transition to execute</param>
        private void ExecuteTransition(StateTransition transition)
        {
            try
            {
                // Exit current state
                if (currentState != null)
                {
                    currentState.OnExit(context);
                    OnStateExited?.Invoke(transition.FromState);
                }
                
                // Enter new state
                currentState = states[transition.ToState];
                currentState.OnEnter(context);
                OnStateEntered?.Invoke(transition.ToState);
                OnStateChanged?.Invoke(transition.FromState, transition.ToState);
                
                // Record successful transition
                RecordTransition(transition.FromState, transition.ToState, transition.Reason, true);
                
                context.LogDebug($"State transition completed: {transition.FromState} -> {transition.ToState}");
            }
            catch (Exception ex)
            {
                string error = $"Error executing transition {transition.FromState} -> {transition.ToState}: {ex.Message}";
                context.LogError(error);
                OnStateMachineError?.Invoke(error);
                RecordTransition(transition.FromState, transition.ToState, $"ERROR: {transition.Reason}", false);
            }
        }
        
        /// <summary>
        /// Handle state transition requests from context
        /// </summary>
        /// <param name="fromState">Current state</param>
        /// <param name="toState">Target state</param>
        private void HandleStateTransitionRequest(CustomerState fromState, CustomerState toState)
        {
            ChangeState(toState, "Context request");
        }
        
        /// <summary>
        /// Record a transition in history for debugging
        /// </summary>
        /// <param name="fromState">Source state</param>
        /// <param name="toState">Target state</param>
        /// <param name="reason">Reason for transition</param>
        /// <param name="successful">Whether transition was successful</param>
        private void RecordTransition(CustomerState fromState, CustomerState toState, string reason, bool successful)
        {
            var transition = new StateTransition(fromState, toState, reason, successful);
            transitionHistory.Add(transition);
            
            // Limit history size
            if (transitionHistory.Count > MaxHistorySize)
            {
                transitionHistory.RemoveAt(0);
            }
        }
        
        /// <summary>
        /// Validate that all required states are registered
        /// </summary>
        private void ValidateRequiredStates()
        {
            var requiredStates = new[] { CustomerState.Entering, CustomerState.Shopping, CustomerState.Purchasing, CustomerState.Leaving };
            
            foreach (var requiredState in requiredStates)
            {
                if (!states.ContainsKey(requiredState))
                {
                    context.LogWarning($"Required state {requiredState} not registered!");
                }
            }
        }
        
        #endregion
    }
}
