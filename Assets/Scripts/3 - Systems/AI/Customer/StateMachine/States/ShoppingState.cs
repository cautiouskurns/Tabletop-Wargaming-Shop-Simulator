using UnityEngine;

namespace TabletopShop
{
    /// <summary>
    /// State representing customer shopping behavior.
    /// Handles shelf browsing, product selection, and shopping time management.
    /// Converts existing coroutine logic from CustomerBehavior.HandleShoppingState.
    /// </summary>
        /// <summary>
    /// Shopping state - delegates to existing coroutine logic for now
    /// </summary>
    public class ShoppingState : BaseCustomerState
    {
        private Coroutine shoppingCoroutine;
        
        public override void OnEnter(CustomerBehavior customer)
        {
            this.customer = customer;
            Debug.Log($"{customer.name} entered Shopping state");
            
            // For now, use the existing coroutine
            shoppingCoroutine = customer.StartCoroutine(customer.HandleShoppingState());
        }
        
        public override void OnUpdate(CustomerBehavior customer)
        {
            // The coroutine handles everything for now
            // Later we can gradually move logic here
        }
        
        public override void OnExit(CustomerBehavior customer)
        {
            Debug.Log($"{customer.name} exiting Shopping state");
            
            if (shoppingCoroutine != null)
            {
                customer.StopCoroutine(shoppingCoroutine);
                shoppingCoroutine = null;
            }
        }
    }
    
}
