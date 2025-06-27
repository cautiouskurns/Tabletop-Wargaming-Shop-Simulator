using UnityEngine;
using Opsive.BehaviorDesigner.Runtime.Tasks.Actions;
using Opsive.BehaviorDesigner.Runtime.Tasks;
using Opsive.GraphDesigner.Runtime.Variables;
using Opsive.BehaviorDesigner.Runtime;

namespace TabletopShop
{
    public class TestCustomerAction : Action
    {
        public enum ActionType
        {
            FindShelf,
            MoveToShelf,
            TrySelectProduct,
            StartShopping,
            MoveToExit,
            Cleanup
        }
        
        [Tooltip("The action to perform")]
        [SerializeField] protected SharedVariable<ActionType> m_ActionType;
        
        [Tooltip("Shared shelf reference for communication between nodes")]
        [SerializeField] protected SharedVariable<GameObject> m_TargetShelf;
        
        public override TaskStatus OnUpdate()
        {
            SimpleTestCustomer customer = GetComponent<SimpleTestCustomer>();
            if (customer == null) return TaskStatus.Failure;
            
            switch (m_ActionType.Value)
            {
                case ActionType.FindShelf:
                    ShelfSlot shelf = customer.FindShelf();
                    if (shelf != null)
                    {
                        // Store the found shelf in shared variable
                        m_TargetShelf.Value = shelf.gameObject;
                        return TaskStatus.Success;
                    }
                    return TaskStatus.Failure;
                    
                case ActionType.MoveToShelf:
                    // Use the shelf from shared variable
                    if (m_TargetShelf.Value != null)
                    {
                        ShelfSlot shelfSlot = m_TargetShelf.Value.GetComponent<ShelfSlot>();
                        if (shelfSlot != null)
                        {
                            return customer.MoveToShelf(shelfSlot) ? TaskStatus.Success : TaskStatus.Failure;
                        }
                    }
                    
                    // Fallback: use current target shelf
                    if (customer.currentTargetShelf != null)
                    {
                        return customer.MoveToShelf(customer.currentTargetShelf) ? TaskStatus.Success : TaskStatus.Failure;
                    }
                    
                    return TaskStatus.Failure;
                    
                case ActionType.TrySelectProduct:
                    customer.TrySelectProduct();
                    return TaskStatus.Success;
                    
                case ActionType.StartShopping:
                    customer.StartShopping();
                    return TaskStatus.Success;
                    
                case ActionType.MoveToExit:
                    return customer.MoveToExit() ? TaskStatus.Success : TaskStatus.Failure;
                    
                case ActionType.Cleanup:
                    customer.CleanupAndDestroy();
                    return TaskStatus.Success;
            }
            
            return TaskStatus.Failure;
        }
    }
}