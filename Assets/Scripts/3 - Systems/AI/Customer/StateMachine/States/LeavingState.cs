using UnityEngine;

namespace TabletopShop
{
 
    /// <summary>
    /// LEAVING STATE - Controls exit behavior
    /// </summary>
    public class LeavingState : BaseCustomerState
    {
        private float leavingStartTime;
        private bool hasFoundExit = false;
        private const float MAX_LEAVING_TIME = 45f;
        private const float CLEANUP_DELAY = 2f;
        
        public override void OnEnter(CustomerBehavior customer)
        {
            this.customer = customer;
            leavingStartTime = Time.time;
            hasFoundExit = false;
            
            Debug.Log($"{customer.name} leaving shop");
            
            // STATE CONTROLS: Clean up any remaining state
            CleanupCustomerState();
            
            // STATE CONTROLS: Start movement to exit
            StartExitMovement();
        }
        
        public override void OnUpdate(CustomerBehavior customer)
        {
            float leavingTime = Time.time - leavingStartTime;
            
            // STATE DECIDES: Timeout check
            if (leavingTime > MAX_LEAVING_TIME)
            {
                Debug.LogWarning($"{customer.name}: Leaving timeout, destroying");
                Object.Destroy(customer.gameObject);
                return;
            }
            
            // STATE DECIDES: Check if reached exit
            if (hasFoundExit && HasReachedDestination())
            {
                Debug.Log($"{customer.name}: Reached exit");
                
                // Wait briefly then destroy
                if (leavingTime >= CLEANUP_DELAY)
                {
                    DestroyCustomer();
                }
            }
        }
        
        public override void OnExit(CustomerBehavior customer)
        {
            float totalLeavingTime = Time.time - leavingStartTime;
            Debug.Log($"{customer.name} finished leaving after {totalLeavingTime:F1}s");
        }
        
        /// <summary>
        /// STATE CONTROLS: Clean up customer state
        /// </summary>
        private void CleanupCustomerState()
        {
            // Clean up queue state
            if (customer.IsInQueue)
            {
                customer.ForceLeaveQueue();
            }
            
            // Stop any movement
            var movement = customer.GetMovement();
            movement?.StopMovement();
            
            // Clean up unpurchased products
            DestroyUnpurchasedProducts();
        }
        
        /// <summary>
        /// STATE CONTROLS: Start movement to exit
        /// </summary>
        private void StartExitMovement()
        {
            var movement = customer.GetMovement();
            if (movement != null && movement.MoveToExitPoint())
            {
                hasFoundExit = true;
                Debug.Log($"{customer.name} started moving to exit");
            }
            else
            {
                Debug.LogWarning($"{customer.name} couldn't find exit, destroying immediately");
                Object.Destroy(customer.gameObject, 1f);
            }
        }
        
        /// <summary>
        /// STATE CHECKS: Has reached destination
        /// </summary>
        private bool HasReachedDestination()
        {
            var movement = customer.GetMovement();
            return movement?.HasReachedDestination() ?? false;
        }
        
        /// <summary>
        /// STATE CONTROLS: Destroy unpurchased products
        /// </summary>
        private void DestroyUnpurchasedProducts()
        {
            // Count unpurchased products
            int unpurchasedCount = 0;
            foreach (Product product in customer.SelectedProducts)
            {
                if (product != null && !product.IsPurchased)
                {
                    unpurchasedCount++;
                }
            }
            
            // Log how many products the customer is taking with them
            if (unpurchasedCount > 0)
            {
                Debug.Log($"{customer.name} is leaving with {unpurchasedCount} unpurchased products - cleaning up");
                
                // Create a new list to avoid modification during iteration
                System.Collections.Generic.List<Product> productsToDestroy = new System.Collections.Generic.List<Product>();
                
                foreach (Product product in customer.SelectedProducts)
                {
                    if (product != null && !product.IsPurchased)
                    {
                        productsToDestroy.Add(product);
                    }
                }
                
                // Destroy each product GameObject that hasn't been purchased
                foreach (Product product in productsToDestroy)
                {
                    Debug.Log($"Destroying unpurchased product: {product.ProductData?.ProductName ?? "Unknown"}");
                    Object.Destroy(product.gameObject);
                }
            }
            
            // Clear all product lists regardless
            customer.SelectedProducts.Clear();
            customer.PlacedOnCounterProducts.Clear();
        }
        
        /// <summary>
        /// STATE CONTROLS: Destroy customer
        /// </summary>
        private void DestroyCustomer()
        {
            Debug.Log($"{customer.name} cleanup - removing from scene");
            Object.Destroy(customer.gameObject);
        }
    }
}
