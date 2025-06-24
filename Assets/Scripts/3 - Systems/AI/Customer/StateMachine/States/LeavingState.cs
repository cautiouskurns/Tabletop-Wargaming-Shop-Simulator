using UnityEngine;

namespace TabletopShop
{
    /// <summary>
    /// Leaving state - delegates to existing coroutine logic for now
    /// </summary>
    public class LeavingState : BaseCustomerState
    {
        private Coroutine leavingCoroutine;
        
        public override void OnEnter(CustomerBehavior customer)
        {
            this.customer = customer;
            Debug.Log($"{customer.name} entered Leaving state");
            
            // For now, use the existing coroutine
            leavingCoroutine = customer.StartCoroutine(customer.HandleLeavingState());
        }
        
        public override void OnUpdate(CustomerBehavior customer)
        {
            // The coroutine handles everything for now
            // Later we can gradually move logic here
        }
        
        public override void OnExit(CustomerBehavior customer)
        {
            Debug.Log($"{customer.name} exiting Leaving state");
            
            if (leavingCoroutine != null)
            {
                customer.StopCoroutine(leavingCoroutine);
                leavingCoroutine = null;
            }
        }
    }
}
