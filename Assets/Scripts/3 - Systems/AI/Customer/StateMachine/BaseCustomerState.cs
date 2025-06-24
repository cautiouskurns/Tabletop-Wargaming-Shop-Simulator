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
            customer?.ChangeStateSimple(newState, reason);
        }
        
        /// <summary>
        /// Common helper methods for all states
        /// </summary>
        protected bool IsStoreOpen()
        {
            var storeHours = Object.FindFirstObjectByType<StoreHours>();
            return storeHours?.IsStoreOpen ?? true;
        }
        
        protected bool IsStoreClosingSoon()
        {
            var storeHours = Object.FindFirstObjectByType<StoreHours>();
            if (storeHours != null)
            {
                float timeUntilClose = storeHours.GetTimeUntilClose();
                return timeUntilClose <= 0.5f && timeUntilClose > 0f; // Less than 30 minutes
            }
            return false;
        }
    }
}
