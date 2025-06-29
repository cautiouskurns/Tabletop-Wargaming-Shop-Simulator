using UnityEngine;
using Opsive.BehaviorDesigner.Runtime.Tasks.Conditionals;
using Opsive.BehaviorDesigner.Runtime.Tasks;

namespace TabletopShop
{
    public class CustomerStateCondition : Conditional
    {
        public enum ConditionType
        {
            HasFinishedShopping,
            HasSelectedProducts,
            HasMoney,
            IsStoreOpen,
            HasReachedDestination,
            HasMaxProducts
        }

        [Tooltip("The condition to check")]
        public ConditionType conditionType;

        [Tooltip("Money amount for comparison (if needed)")]
        public float moneyAmount = 0f;

        public override TaskStatus OnUpdate()
        {
            SimpleTestCustomer customer = GetComponent<SimpleTestCustomer>();
            if (customer == null) return TaskStatus.Failure;
            
            TaskStatus result = TaskStatus.Failure;


            switch (conditionType)
            {
                case ConditionType.HasFinishedShopping:
                    result = CheckShoppingTime(customer);
                    if (customer.showDebugLogs)
                        Debug.Log($"[CustomerStateCondition] HasFinishedShopping = {result} (elapsed: {Time.time - customer.shoppingStartTime:F1}s / {customer.ShoppingTime}s)");
                    return result;

                case ConditionType.HasMaxProducts:
                    result = customer.selectedProducts.Count >= customer.MaxProducts ? TaskStatus.Success : TaskStatus.Failure;
                    if (customer.showDebugLogs)
                        Debug.Log($"[CustomerStateCondition] HasMaxProducts = {result} (products: {customer.selectedProducts.Count}/{customer.MaxProducts})");
                    return result;

                case ConditionType.HasSelectedProducts:
                    result = customer.selectedProducts.Count > 0 ? TaskStatus.Success : TaskStatus.Failure;
                    if (customer.showDebugLogs)
                        Debug.Log($"[CustomerStateCondition] HasSelectedProducts = {result} (count: {customer.selectedProducts.Count})");
                    return result;

                case ConditionType.HasMoney:
                    return customer.currentMoney >= moneyAmount ? TaskStatus.Success : TaskStatus.Failure;

                case ConditionType.IsStoreOpen:
                    return CheckStoreHours();

                case ConditionType.HasReachedDestination:
                    return CheckReachedDestination(customer);
            }

            return TaskStatus.Failure;
        }

        private TaskStatus CheckShoppingTime(SimpleTestCustomer customer)
        {
            float elapsed = Time.time - customer.shoppingStartTime;
            return elapsed >= customer.ShoppingTime ? TaskStatus.Success : TaskStatus.Failure;
        }

        private TaskStatus CheckStoreHours()
        {
            StoreHours storeHours = UnityEngine.Object.FindAnyObjectByType<StoreHours>();
            return (storeHours?.IsStoreOpen ?? true) ? TaskStatus.Success : TaskStatus.Failure;
        }

        private TaskStatus CheckReachedDestination(SimpleTestCustomer customer)
        {
            if (customer.NavAgent == null) return TaskStatus.Success;

            bool reached = !customer.NavAgent.pathPending &&
                          customer.NavAgent.remainingDistance < 1f &&
                          customer.NavAgent.velocity.magnitude < 0.1f;

            return reached ? TaskStatus.Success : TaskStatus.Running;
        }
    }
}