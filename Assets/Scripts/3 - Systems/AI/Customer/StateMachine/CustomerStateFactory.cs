using UnityEngine;
using System.Collections.Generic;

namespace TabletopShop
{
    /// <summary>
    /// Factory for creating customer state instances.
    /// Provides centralized state creation and configuration.
    /// </summary>
    public static class CustomerStateFactory
    {
        /// <summary>
        /// Create all standard customer states
        /// </summary>
        /// <returns>Dictionary of all customer states</returns>
        public static Dictionary<CustomerState, ICustomerState> CreateAllStates()
        {
            var states = new Dictionary<CustomerState, ICustomerState>();
            
            states[CustomerState.Entering] = CreateEnteringState();
            states[CustomerState.Shopping] = CreateShoppingState();
            states[CustomerState.Purchasing] = CreatePurchasingState();
            states[CustomerState.Leaving] = CreateLeavingState();
            
            return states;
        }
        
        /// <summary>
        /// Create an entering state instance
        /// </summary>
        /// <returns>Entering state instance</returns>
        public static ICustomerState CreateEnteringState()
        {
            return new EnteringState();
        }
        
        /// <summary>
        /// Create a shopping state instance
        /// </summary>
        /// <returns>Shopping state instance</returns>
        public static ICustomerState CreateShoppingState()
        {
            return new ShoppingState();
        }
        
        /// <summary>
        /// Create a purchasing state instance
        /// </summary>
        /// <returns>Purchasing state instance</returns>
        public static ICustomerState CreatePurchasingState()
        {
            return new PurchasingState();
        }
        
        /// <summary>
        /// Create a leaving state instance
        /// </summary>
        /// <returns>Leaving state instance</returns>
        public static ICustomerState CreateLeavingState()
        {
            return new LeavingState();
        }
        
        /// <summary>
        /// Create a specific state by type
        /// </summary>
        /// <param name="stateType">The type of state to create</param>
        /// <returns>State instance or null if type not supported</returns>
        public static ICustomerState CreateState(CustomerState stateType)
        {
            switch (stateType)
            {
                case CustomerState.Entering:
                    return CreateEnteringState();
                case CustomerState.Shopping:
                    return CreateShoppingState();
                case CustomerState.Purchasing:
                    return CreatePurchasingState();
                case CustomerState.Leaving:
                    return CreateLeavingState();
                default:
                    Debug.LogError($"Unknown customer state type: {stateType}");
                    return null;
            }
        }
        
        /// <summary>
        /// Register all states with a state machine
        /// </summary>
        /// <param name="stateMachine">State machine to register states with</param>
        public static void RegisterAllStates(CustomerStateMachine stateMachine)
        {
            if (stateMachine == null)
            {
                Debug.LogError("Cannot register states with null state machine");
                return;
            }
            
            var states = CreateAllStates();
            foreach (var kvp in states)
            {
                stateMachine.RegisterState(kvp.Value);
            }
            
            Debug.Log($"Registered {states.Count} customer states with state machine");
        }
    }
}
