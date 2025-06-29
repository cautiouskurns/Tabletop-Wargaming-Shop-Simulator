using UnityEngine;
using UnityEngine.AI;
using Opsive.BehaviorDesigner.Runtime.Tasks.Actions;
using Opsive.BehaviorDesigner.Runtime.Tasks;
using Opsive.GraphDesigner.Runtime.Variables;
using Opsive.BehaviorDesigner.Runtime;

namespace TabletopShop
{
    public class MoveToShelfTask : Action
    {
        [Tooltip("How close to consider 'reached'")]
        public float reachThreshold = 1f;

        private bool isMoving = false;

        public override void OnStart()
        {
            Customer customer = GetComponent<Customer>();
            if (customer == null)
            {
                Debug.LogError("[MoveToShelfTask] Customer component is NULL!");
                return;
            }
            
            if (customer.currentTargetShelf == null)
            {
                Debug.LogError($"[MoveToShelfTask] {customer.name}: currentTargetShelf is NULL!");
                return;
            }

            Debug.Log($"[MoveToShelfTask] {customer.name}: OnStart - Target shelf: {customer.currentTargetShelf.name}");
            Debug.Log($"[MoveToShelfTask] {customer.name}: Customer.Movement is {(customer.Movement != null ? "AVAILABLE" : "NULL")}");

            // Use the existing CustomerMovement method that handles NavMesh properly
            bool moveStarted = customer.Movement.MoveToShelfPosition(customer.currentTargetShelf);

            if (moveStarted)
            {
                isMoving = true;
                if (customer.showDebugLogs)
                    Debug.Log($"[MoveToShelfTask] ✅ {customer.name}: Started moving to {customer.currentTargetShelf.name}");
            }
            else
            {
                Debug.LogError($"[MoveToShelfTask] {customer.name}: Failed to start movement!");
            }
        }

        public override TaskStatus OnUpdate()
        {
            if (!isMoving)
                return TaskStatus.Failure;

            Customer customer = GetComponent<Customer>();
            if (customer == null || customer.currentTargetShelf == null)
                return TaskStatus.Failure;

            // Check if reached destination using our logic
            if (HasReachedDestination(customer))
            {
                if (customer.showDebugLogs)
                    Debug.Log($"[MoveToShelfTask] ✅ Reached {customer.currentTargetShelf.name}");
                return TaskStatus.Success;
            }

            return TaskStatus.Running;
        }

        public override void OnEnd()
        {
            isMoving = false;
        }


        private bool HasReachedDestination(Customer customer)
        {
            if (customer.Movement == null) return true;

            return customer.Movement.HasReachedDestination();
        }
    }
}
