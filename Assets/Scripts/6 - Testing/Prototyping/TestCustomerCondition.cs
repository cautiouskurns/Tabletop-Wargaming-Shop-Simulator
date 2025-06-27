using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using Opsive.BehaviorDesigner.Runtime.Tasks.Actions;
using Opsive.BehaviorDesigner.Runtime.Tasks;
using Opsive.GraphDesigner.Runtime.Variables;
using Opsive.BehaviorDesigner.Runtime.Tasks.Conditionals;
using Opsive.BehaviorDesigner.Runtime;

namespace TabletopShop
{
    public class TestCustomerCondition : Conditional
    {
        public enum ConditionType
        {
            HasReachedDestination,
            IsStoreOpen,
            IsStoreClosed,
            HasSelectedProducts,
            HasFinishedShopping,
            HasMoney
        }
        
        [Tooltip("The condition to check")]
        [SerializeField] protected SharedVariable<ConditionType> m_ConditionType;
        
        [Tooltip("Money amount for comparison (if needed)")]
        [SerializeField] protected SharedVariable<float> m_MoneyAmount;
        
        public override TaskStatus OnUpdate()
        {
            SimpleTestCustomer customer = GetComponent<SimpleTestCustomer>();
            if (customer == null) return TaskStatus.Failure;
            
            switch (m_ConditionType.Value)
            {
                case ConditionType.HasReachedDestination:
                    return customer.HasReachedDestination() ? TaskStatus.Success : TaskStatus.Running;
                    
                case ConditionType.IsStoreOpen:
                    return customer.IsStoreOpen() ? TaskStatus.Success : TaskStatus.Failure;
                    
                case ConditionType.IsStoreClosed:
                    return customer.IsStoreClosed() ? TaskStatus.Success : TaskStatus.Failure;
                    
                case ConditionType.HasSelectedProducts:
                    return customer.HasSelectedProducts() ? TaskStatus.Success : TaskStatus.Failure;
                    
                case ConditionType.HasFinishedShopping:
                    return customer.HasFinishedShopping() ? TaskStatus.Success : TaskStatus.Failure;
                    
                case ConditionType.HasMoney:
                    return customer.currentMoney >= m_MoneyAmount.Value ? TaskStatus.Success : TaskStatus.Failure;
            }
            
            return TaskStatus.Failure;
        }
    }
}