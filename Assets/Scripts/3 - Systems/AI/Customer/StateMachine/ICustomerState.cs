using UnityEngine;
using System.Collections;

namespace TabletopShop
{
    /// <summary>
    /// Interface defining the contract for all customer states in the state machine.
    /// Each state must implement entry, update, exit, and transition validation logic.
    /// </summary>
    public interface ICustomerState
    {
        /// <summary>
        /// Human-readable name of this state for debugging
        /// </summary>
        string StateName { get; }
        
        /// <summary>
        /// Get the CustomerState enum value this state represents
        /// </summary>
        CustomerState GetStateType();
        
        /// <summary>
        /// Called when entering this state
        /// </summary>
        /// <param name="context">Customer state context with access to all components</param>
        void OnEnter();
        
        /// <summary>
        /// Called every frame while in this state
        /// </summary>
        /// <param name="context">Customer state context with access to all components</param>
        void OnUpdate();
        
        /// <summary>
        /// Called when exiting this state
        /// </summary>
        /// <param name="context">Customer state context with access to all components</param>
        void OnExit();
        
        /// <summary>
        /// Check if this state can transition to the target state
        /// </summary>
        /// <param name="targetState">The state to transition to</param>
        /// <param name="context">Customer state context for validation logic</param>
        /// <returns>True if transition is allowed</returns>
        bool CanTransitionTo();
        
        /// <summary>
        /// Get debug information about this state's current status
        /// </summary>
        /// <param name="context">Customer state context</param>
        /// <returns>Debug information string</returns>
        string GetDebugInfo();
    }
}
