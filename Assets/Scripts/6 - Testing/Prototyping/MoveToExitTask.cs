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
            SimpleTestCustomer customer = GetComponent<SimpleTestCustomer>();
            if (customer == null)
            {
                Debug.LogError("[MoveToExitTask] No CustomerData component found!");
                return;
            }
            
            // Calculate exit position
            Vector3 exitPosition = customer.spawnPosition + Vector3.back * exitDistance;
            
            // Start movement
            if (customer.NavAgent != null && customer.NavAgent.isActiveAndEnabled)
            {
                customer.NavAgent.SetDestination(exitPosition);
                isMoving = true;
                
                if (customer.showDebugLogs)
                    Debug.Log("[MoveToExitTask] ✅ Moving to exit");
            }
            else
            {
                Debug.LogError("[MoveToExitTask] NavMeshAgent not available!");
            }
        }
        
        public override TaskStatus OnUpdate()
        {
            if (!isMoving)
                return TaskStatus.Failure;
            
            SimpleTestCustomer customer = GetComponent<SimpleTestCustomer>();
            if (customer == null)
                return TaskStatus.Failure;
            
            // Check if reached exit
            if (customer.NavAgent != null && 
                !customer.NavAgent.pathPending && 
                customer.NavAgent.remainingDistance < 1f)
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