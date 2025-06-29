using UnityEngine;
using Opsive.BehaviorDesigner.Runtime.Tasks.Actions;
using Opsive.BehaviorDesigner.Runtime.Tasks;

namespace TabletopShop
{
    public class InitializeShoppingTask : Action
    {
        public override TaskStatus OnUpdate()
        {
            SimpleTestCustomer customer = GetComponent<SimpleTestCustomer>();
            if (customer == null)
            {
                Debug.LogError("[InitializeShoppingTask] No CustomerData component found!");
                return TaskStatus.Failure;
            }
            
            // Start the shopping timer
            customer.StartShoppingTimer();
            
            if (customer.showDebugLogs)
                Debug.Log("[InitializeShoppingTask] ✅ Shopping timer started");
            
            return TaskStatus.Success;
        }
    }
}