using UnityEngine;
using System;

namespace TabletopShop
{
    /// <summary>
    /// Context object that provides access to all customer components and shared data.
    /// Passed to states to allow them to interact with the customer system.
    /// </summary>
    public class CustomerStateContext
    {
        // Core component references
        public Customer Customer { get; private set; }
        public CustomerMovement Movement { get; private set; }
        public CustomerBehavior Behavior { get; private set; }
        public CustomerVisuals Visuals { get; private set; }
        public CustomerStateMachine StateMachine { get; private set; }
        
        // Shared state data
        public float StateStartTime { get; set; }
        public float TotalTimeInCurrentState { get; set; }
        public CustomerState PreviousState { get; set; }
        public bool IsStateChangePending { get; set; }
        
        // Shopping-specific shared data
        public float ShoppingStartTime { get; set; }
        public int ShelvesVisited { get; set; }
        public bool HasSelectedProducts { get; set; }
        
        // Events for state communication
        public event Action<CustomerState, CustomerState> OnStateTransitionRequested;
        public event Action<string> OnDebugMessage;
        
        /// <summary>
        /// Constructor for CustomerStateContext
        /// </summary>
        /// <param name="customer">Main customer component</param>
        /// <param name="movement">Customer movement component</param>
        /// <param name="behavior">Customer behavior component</param>
        /// <param name="visuals">Customer visuals component</param>
        public CustomerStateContext(Customer customer, CustomerMovement movement, 
                                   CustomerBehavior behavior, CustomerVisuals visuals)
        {
            Customer = customer;
            Movement = movement;
            Behavior = behavior;
            Visuals = visuals;
            
            // Initialize timing data
            StateStartTime = Time.time;
            TotalTimeInCurrentState = 0f;
            PreviousState = CustomerState.Entering;
            IsStateChangePending = false;
            
            // Initialize shopping data
            ShoppingStartTime = 0f;
            ShelvesVisited = 0;
            HasSelectedProducts = false;
        }
        
        /// <summary>
        /// Set the state machine reference (called after state machine creation)
        /// </summary>
        /// <param name="stateMachine">The customer state machine</param>
        public void SetStateMachine(CustomerStateMachine stateMachine)
        {
            StateMachine = stateMachine;
        }
        
        /// <summary>
        /// Update timing information (called by state machine)
        /// </summary>
        public void UpdateTiming()
        {
            TotalTimeInCurrentState = Time.time - StateStartTime;
        }
        
        /// <summary>
        /// Reset timing for a new state
        /// </summary>
        /// <param name="newState">The new state being entered</param>
        public void ResetTimingForNewState(CustomerState newState)
        {
            PreviousState = StateMachine?.CurrentStateType ?? CustomerState.Entering;
            StateStartTime = Time.time;
            TotalTimeInCurrentState = 0f;
            IsStateChangePending = false;
        }
        
        /// <summary>
        /// Request a state transition (to be processed by state machine)
        /// </summary>
        /// <param name="targetState">The state to transition to</param>
        /// <param name="reason">Reason for the transition (for debugging)</param>
        public void RequestStateTransition(CustomerState targetState, string reason = "")
        {
            if (!IsStateChangePending)
            {
                IsStateChangePending = true;
                OnStateTransitionRequested?.Invoke(StateMachine.CurrentStateType, targetState);
                LogDebug($"State transition requested: {StateMachine.CurrentStateType} -> {targetState}. Reason: {reason}");
            }
        }
        
        /// <summary>
        /// Log a debug message with customer identification
        /// </summary>
        /// <param name="message">Debug message</param>
        public void LogDebug(string message)
        {
            string customerName = Customer?.name ?? "Unknown Customer";
            string fullMessage = $"[{customerName}] {message}";
            Debug.Log(fullMessage);
            OnDebugMessage?.Invoke(fullMessage);
        }
        
        /// <summary>
        /// Log a warning message with customer identification
        /// </summary>
        /// <param name="message">Warning message</param>
        public void LogWarning(string message)
        {
            string customerName = Customer?.name ?? "Unknown Customer";
            string fullMessage = $"[{customerName}] WARNING: {message}";
            Debug.LogWarning(fullMessage);
            OnDebugMessage?.Invoke(fullMessage);
        }
        
        /// <summary>
        /// Log an error message with customer identification
        /// </summary>
        /// <param name="message">Error message</param>
        public void LogError(string message)
        {
            string customerName = Customer?.name ?? "Unknown Customer";
            string fullMessage = $"[{customerName}] ERROR: {message}";
            Debug.LogError(fullMessage);
            OnDebugMessage?.Invoke(fullMessage);
        }
        
        /// <summary>
        /// Get comprehensive debug information about the context
        /// </summary>
        /// <returns>Debug information string</returns>
        public string GetDebugInfo()
        {
            return $"Customer: {Customer?.name ?? "null"}\n" +
                   $"Current State: {StateMachine?.CurrentStateType ?? CustomerState.Entering}\n" +
                   $"Previous State: {PreviousState}\n" +
                   $"Time in State: {TotalTimeInCurrentState:F1}s\n" +
                   $"State Change Pending: {IsStateChangePending}\n" +
                   $"Has Movement: {Movement != null}\n" +
                   $"Is Moving: {Movement?.IsMoving ?? false}\n" +
                   $"Has Destination: {Movement?.HasDestination ?? false}\n" +
                   $"Shelves Visited: {ShelvesVisited}\n" +
                   $"Has Selected Products: {HasSelectedProducts}";
        }
    }
}
