using UnityEngine;

namespace TabletopShop
{
    /// <summary>
    /// Base class for customer states - states control ALL their own logic
    /// </summary>
    public abstract class BaseCustomerState
    {
        protected CustomerBehavior customer;
        
        public abstract void OnEnter(CustomerBehavior customer);
        public abstract void OnUpdate(CustomerBehavior customer);
        public abstract void OnExit(CustomerBehavior customer);
        
        /// <summary>
        /// States request their own transitions
        /// </summary>
        protected void RequestTransition(CustomerState newState, string reason)
        {
            if (customer != null)
            {
                Debug.Log($"[STATE] {customer.name} requesting transition to {newState}: {reason}");
                customer.ChangeStateSimple(newState, reason);
            }
            else
            {
                Debug.LogError("[STATE] Cannot request transition - customer reference is null!");
            }
        }
        
        /// <summary>
        /// Common helper methods for all states
        /// </summary>
        protected bool IsStoreOpen()
        {
            return customer?.GetIsStoreOpen() ?? true;
        }
        
        protected bool IsStoreClosingSoon()
        {
            return customer?.GetShouldHurryUpShopping() ?? false;
        }
        
        protected bool ShouldLeaveStoreDueToHours()
        {
            return customer?.GetShouldLeaveStoreDueToHours() ?? false;
        }
    }
}
