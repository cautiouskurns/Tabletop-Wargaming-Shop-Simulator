using UnityEngine;
using Opsive.BehaviorDesigner.Runtime.Tasks.Actions;
using Opsive.BehaviorDesigner.Runtime.Tasks;

namespace TabletopShop
{
    public class MoveToExitTask : Action
    {
        [Tooltip("Distance behind spawn point to consider as exit")]
        public float exitDistance = 25f;
        
        private bool isMoving = false;
        
        public override void OnStart()
        {
            Customer customer = GetComponent<Customer>();
            if (customer == null)
            {
                Debug.LogError("[MoveToExitTask] No CustomerData component found!");
                return;
            }
            
            // Calculate exit position
            Vector3 exitPosition = customer.spawnPosition + Vector3.back * exitDistance;
            
            // Start movement
            if (customer.Movement != null)
            {
                bool moveStarted = customer.Movement.MoveToPosition(exitPosition);
                isMoving = moveStarted;
                
                if (customer.showDebugLogs)
                    Debug.Log("[MoveToExitTask] ✅ Moving to exit");
            }
            else
            {
                Debug.LogError("[MoveToExitTask] Movement component not available!");
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