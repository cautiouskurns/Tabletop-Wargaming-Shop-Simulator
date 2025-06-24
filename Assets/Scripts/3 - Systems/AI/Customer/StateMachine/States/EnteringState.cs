using UnityEngine;

namespace TabletopShop
{
    /// <summary>
    /// State representing customer entering the shop.
    /// Handles initial movement to a random shelf and store validation.
    /// Converts existing coroutine logic from CustomerBehavior.HandleEnteringState.

    public class EnteringState : BaseCustomerState
    {
        public override void OnEnter(CustomerBehavior customer)
        {
            this.customer = customer;
            Debug.Log($"{customer.name} entered Entering state");
            
            // For now, just use the existing coroutine
            customer.StartCoroutine(customer.HandleEnteringState());
        }
        
        public override void OnUpdate(CustomerBehavior customer)
        {
            // The coroutine handles everything for now
            // Later we can gradually move logic here
        }
        
        public override void OnExit(CustomerBehavior customer)
        {
            Debug.Log($"{customer.name} exiting Entering state");
        }
    }
    
}
