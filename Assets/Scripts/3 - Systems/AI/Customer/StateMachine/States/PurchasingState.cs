using UnityEngine;
using System.Collections;

namespace TabletopShop
{
   /// <summary>
    /// Purchasing state - delegates to existing coroutine logic for now
    /// </summary>
    public class PurchasingState : BaseCustomerState
    {
        private Coroutine purchasingCoroutine;
        
        public override void OnEnter(CustomerBehavior customer)
        {
            this.customer = customer;
            Debug.Log($"{customer.name} entered Purchasing state");
            
            // For now, use the existing coroutine
            purchasingCoroutine = customer.StartCoroutine(customer.HandlePurchasingState());
        }
        
        public override void OnUpdate(CustomerBehavior customer)
        {
            // The coroutine handles everything for now
            // Later we can gradually move logic here
        }
        
        public override void OnExit(CustomerBehavior customer)
        {
            Debug.Log($"{customer.name} exiting Purchasing state");
            
            if (purchasingCoroutine != null)
            {
                customer.StopCoroutine(purchasingCoroutine);
                purchasingCoroutine = null;
            }
        }
    }
}
