using UnityEngine;
using Opsive.BehaviorDesigner.Runtime.Tasks.Actions;
using Opsive.BehaviorDesigner.Runtime.Tasks;

namespace TabletopShop
{
    public class CleanupAndDestroyTask : Action
    {
        [Tooltip("Delay before destroying customer")]
        public float destroyDelay = 1f;
        
        public override TaskStatus OnUpdate()
        {
            Customer customer = GetComponent<Customer>();
            if (customer == null)
                return TaskStatus.Failure;
            
            // Cleanup logic
            customer.CleanupOnDestroy();
            
            // Destroy after delay
            Object.Destroy(customer.gameObject, destroyDelay);
            
            if (customer.showDebugLogs)
                Debug.Log("[CleanupAndDestroyTask] ✅ Customer cleanup complete");
            
            return TaskStatus.Success;
        }
    }
}