using UnityEngine;
using Opsive.BehaviorDesigner.Runtime.Tasks.Actions;
using Opsive.BehaviorDesigner.Runtime.Tasks;

namespace TabletopShop
{
    public class MoveToExitTask : Action
    {
        private bool isMoving = false;
        
        public override void OnStart()
        {
            Customer customer = GetComponent<Customer>();
            if (customer == null)
            {
                Debug.LogError("[MoveToExitTask] Customer component not found!");
                return;
            }
            
            if (customer.Movement == null)
            {
                Debug.LogError("[MoveToExitTask] Movement component not available!");
                return;
            }
            
            // Use the existing CustomerMovement method that handles NavMesh properly
            bool moveStarted = customer.Movement.MoveToExitPoint();
            isMoving = moveStarted;
            
            if (moveStarted)
            {
                if (customer.showDebugLogs)
                    Debug.Log($"[MoveToExitTask] ✅ {customer.name}: Started moving to exit");
            }
            else
            {
                Debug.LogError($"[MoveToExitTask] {customer.name}: Failed to start exit movement!");
            }
        }
        
        public override TaskStatus OnUpdate()
        {
            if (!isMoving)
                return TaskStatus.Failure;
            
            Customer customer = GetComponent<Customer>();
            if (customer == null)
                return TaskStatus.Failure;
            
            // Check if reached exit
            if (customer.Movement != null && customer.Movement.HasReachedDestination())
            {
                if (customer.showDebugLogs)
                    Debug.Log("[MoveToExitTask] ✅ Reached exit");
                return TaskStatus.Success;
            }
            
            return TaskStatus.Running;
        }
        
        public override void OnEnd()
        {
            isMoving = false;
        }
    }
}