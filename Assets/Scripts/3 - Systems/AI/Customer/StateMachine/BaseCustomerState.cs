using UnityEngine;

namespace TabletopShop
{
    /// <summary>
    /// Abstract base class for customer states.
    /// Provides common functionality and default implementations for state behavior.
    /// </summary>
    public abstract class BaseCustomerState : ICustomerState
    {
        /// <summary>
        /// Human-readable name of this state
        /// </summary>
        public abstract string StateName { get; }
        
        /// <summary>
        /// The CustomerState enum value this state represents
        /// </summary>
        public abstract CustomerState GetStateType();
        
        /// <summary>
        /// Called when entering this state.
        /// Override in derived classes for state-specific entry logic.
        /// </summary>
        /// <param name="context">Customer state context</param>
        public virtual void OnEnter(CustomerStateContext context)
        {
            context.LogDebug($"Entering {StateName} state");
            context.ResetTimingForNewState(GetStateType());
            
            // Update visuals for new state
            if (context.Visuals != null)
            {
                context.Visuals.UpdateColorForState(GetStateType());
            }
        }
        
        /// <summary>
        /// Called every frame while in this state.
        /// Override in derived classes for state-specific update logic.
        /// </summary>
        /// <param name="context">Customer state context</param>
        public virtual void OnUpdate(CustomerStateContext context)
        {
            context.UpdateTiming();
            
            // Default update behavior - log periodic debug info
            if (Time.time % 5f < Time.deltaTime) // Every 5 seconds
            {
                context.LogDebug($"Still in {StateName} state (Time: {context.TotalTimeInCurrentState:F1}s)");
            }
        }
        
        /// <summary>
        /// Called when exiting this state.
        /// Override in derived classes for state-specific exit logic.
        /// </summary>
        /// <param name="context">Customer state context</param>
        public virtual void OnExit(CustomerStateContext context)
        {
            context.LogDebug($"Exiting {StateName} state (Duration: {context.TotalTimeInCurrentState:F1}s)");
        }
        
        /// <summary>
        /// Check if this state can transition to the target state.
        /// Override in derived classes for state-specific transition rules.
        /// </summary>
        /// <param name="targetState">The state to transition to</param>
        /// <param name="context">Customer state context for validation</param>
        /// <returns>True if transition is allowed</returns>
        public virtual bool CanTransitionTo(CustomerState targetState, CustomerStateContext context)
        {
            // Default: allow transition to Leaving state from any state (emergency exit)
            if (targetState == CustomerState.Leaving)
            {
                return true;
            }
            
            // Default: don't allow transitions to the same state
            if (targetState == GetStateType())
            {
                context.LogWarning($"Attempted transition to same state: {targetState}");
                return false;
            }
            
            // Override in derived classes for specific transition rules
            return false;
        }
        
        /// <summary>
        /// Get debug information about this state's current status.
        /// Override in derived classes for state-specific debug info.
        /// </summary>
        /// <param name="context">Customer state context</param>
        /// <returns>Debug information string</returns>
        public virtual string GetDebugInfo(CustomerStateContext context)
        {
            return $"State: {StateName}\n" +
                   $"Type: {GetStateType()}\n" +
                   $"Duration: {context.TotalTimeInCurrentState:F1}s\n" +
                   $"Customer: {context.Customer?.name ?? "null"}";
        }
        
        /// <summary>
        /// Helper method to check if store is open (common across states)
        /// </summary>
        /// <returns>True if store is open</returns>
        protected virtual bool IsStoreOpen()
        {
            // Find StoreHours component in scene
            var storeHours = Object.FindFirstObjectByType<StoreHours>();
            if (storeHours != null)
            {
                //return storeHours.IsStoreOpen();
            }
            
            // Default to open if no StoreHours component found
            return true;
        }
        
        /// <summary>
        /// Helper method to check if store is closing soon (common across states)
        /// </summary>
        /// <returns>True if store is closing soon</returns>
        protected virtual bool IsStoreClosingSoon()
        {
            var storeHours = Object.FindFirstObjectByType<StoreHours>();
            if (storeHours != null)
            {
                // Check if store closes within 5 minutes
                //return storeHours.GetTimeUntilClosing() < 300f; // 5 minutes
            }
            
            return false;
        }
        
        /// <summary>
        /// Helper method to validate movement component availability
        /// </summary>
        /// <param name="context">Customer state context</param>
        /// <returns>True if movement component is available</returns>
        protected bool ValidateMovement(CustomerStateContext context)
        {
            if (context.Movement == null)
            {
                context.LogError($"{StateName} state requires movement component but none found!");
                return false;
            }
            return true;
        }
        
        /// <summary>
        /// Helper method to validate behavior component availability
        /// </summary>
        /// <param name="context">Customer state context</param>
        /// <returns>True if behavior component is available</returns>
        protected bool ValidateBehavior(CustomerStateContext context)
        {
            if (context.Behavior == null)
            {
                context.LogError($"{StateName} state requires behavior component but none found!");
                return false;
            }
            return true;
        }
    }
}
