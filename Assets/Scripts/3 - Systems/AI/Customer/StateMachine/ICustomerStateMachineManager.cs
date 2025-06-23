using UnityEngine;

namespace TabletopShop
{
    /// <summary>
    /// Interface for managing the customer state machine lifecycle.
    /// Provides methods for initialization, state registration, and cleanup.
    /// </summary>
    public interface ICustomerStateMachineManager
    {
        // State machine access
        CustomerStateMachine StateMachine { get; }
        bool IsStateMachineActive { get; }
        
        // Lifecycle management
        void InitializeStateMachine();
        void StartStateMachine(CustomerState initialState = CustomerState.Entering);
        void StopStateMachine();
        void CleanupStateMachine();
        
        // State registration
        void RegisterAllStates();
        void RegisterState(ICustomerState state);
        
        // State operations
        bool ChangeState(CustomerState newState, string reason = "");
        void ForceChangeState(CustomerState newState, string reason = "");
        
        // Information and debugging
        string GetStateMachineDebugInfo();
        CustomerState GetCurrentState();
        bool IsInState(CustomerState state);
    }
}
