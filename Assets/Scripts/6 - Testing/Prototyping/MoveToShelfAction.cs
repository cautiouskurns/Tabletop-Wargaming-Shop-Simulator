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
        [Tooltip("Distance from shelf to stand")]
        public float standDistance = 2f;

        [Tooltip("How close to consider 'reached'")]
        public float reachThreshold = 1f;

        private bool isMoving = false;

        public override void OnStart()
        {
            SimpleTestCustomer customer = GetComponent<SimpleTestCustomer>();
            if (customer == null || customer.currentTargetShelf == null)
            {
                Debug.LogError("[MoveToShelfTask] No customer data or target shelf!");
                return;
            }

            // All movement logic lives here
            Vector3 targetPosition = CalculateCustomerPosition(customer.currentTargetShelf);
            bool moveStarted = StartMovement(customer, targetPosition);

            if (moveStarted)
            {
                isMoving = true;
                if (customer.showDebugLogs)
                    Debug.Log($"[MoveToShelfTask] ✅ Started moving to {customer.currentTargetShelf.name}");
            }
            else
            {
                Debug.LogError("[MoveToShelfTask] Failed to start movement!");
            }
        }

        public override TaskStatus OnUpdate()
        {
            if (!isMoving)
                return TaskStatus.Failure;

            SimpleTestCustomer customer = GetComponent<SimpleTestCustomer>();
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

        private Vector3 CalculateCustomerPosition(ShelfSlot shelf)
        {
            // Logic for where customer should stand
            Vector3 shelfPosition = shelf.transform.position;
            Vector3 shelfForward = shelf.transform.forward;
            return shelfPosition + shelfForward * standDistance;
        }

        private bool StartMovement(SimpleTestCustomer customer, Vector3 targetPosition)
        {
            if (customer.NavAgent != null && customer.NavAgent.isActiveAndEnabled)
            {
                customer.NavAgent.SetDestination(targetPosition);
                return true;
            }
            return false;
        }

        private bool HasReachedDestination(SimpleTestCustomer customer)
        {
            if (customer.NavAgent == null) return true;

            return !customer.NavAgent.pathPending &&
                   customer.NavAgent.remainingDistance < reachThreshold &&
                   customer.NavAgent.velocity.magnitude < 0.1f;
        }
    }
}
