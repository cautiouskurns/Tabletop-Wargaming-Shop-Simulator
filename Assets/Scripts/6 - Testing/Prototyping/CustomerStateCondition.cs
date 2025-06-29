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
            HasMaxProducts,
            NeedsCheckout,
            CanAffordProducts
        }

        [Tooltip("The condition to check")]
        public ConditionType conditionType;

        [Tooltip("Money amount for comparison (if needed)")]
        public float moneyAmount = 0f;

        public override TaskStatus OnUpdate()
        {
            Customer customer = GetComponent<Customer>();
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
                    
                case ConditionType.NeedsCheckout:
                    result = CheckNeedsCheckout(customer);
                    if (customer.showDebugLogs)
                        Debug.Log($"[CustomerStateCondition] NeedsCheckout = {result} (has products: {customer.selectedProducts.Count > 0})");
                    return result;
                    
                case ConditionType.CanAffordProducts:
                    result = CheckCanAffordProducts(customer);
                    if (customer.showDebugLogs)
                        Debug.Log($"[CustomerStateCondition] CanAffordProducts = {result} (money: ${customer.currentMoney:F2})");
                    return result;
            }

            return TaskStatus.Failure;
        }

        private TaskStatus CheckShoppingTime(Customer customer)
        {
            float elapsed = Time.time - customer.shoppingStartTime;
            return elapsed >= customer.ShoppingTime ? TaskStatus.Success : TaskStatus.Failure;
        }

        private TaskStatus CheckStoreHours()
        {
            StoreHours storeHours = UnityEngine.Object.FindAnyObjectByType<StoreHours>();
            return (storeHours?.IsStoreOpen ?? true) ? TaskStatus.Success : TaskStatus.Failure;
        }

        private TaskStatus CheckReachedDestination(Customer customer)
        {
            if (customer.Movement == null) return TaskStatus.Success;

            bool reached = customer.Movement.HasReachedDestination();

            return reached ? TaskStatus.Success : TaskStatus.Running;
        }
        
        private TaskStatus CheckNeedsCheckout(Customer customer)
        {
            // Customer needs checkout if they have products that haven't been purchased
            bool hasUnpurchasedProducts = customer.selectedProducts.Count > 0;
            return hasUnpurchasedProducts ? TaskStatus.Success : TaskStatus.Failure;
        }
        
        private TaskStatus CheckCanAffordProducts(Customer customer)
        {
            // Calculate total cost of selected products
            float totalCost = 0f;
            foreach (Product product in customer.selectedProducts)
            {
                if (product != null)
                {
                    totalCost += product.CurrentPrice;
                }
            }
            
            // Check if customer has enough money
            bool canAfford = customer.currentMoney >= totalCost;
            return canAfford ? TaskStatus.Success : TaskStatus.Failure;
        }
    }
}