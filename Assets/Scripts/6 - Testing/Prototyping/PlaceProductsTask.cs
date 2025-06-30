using UnityEngine;
using Opsive.BehaviorDesigner.Runtime.Tasks.Actions;
using Opsive.BehaviorDesigner.Runtime.Tasks;
using System.Collections;

namespace TabletopShop
{
    /// <summary>
    /// Behavior Designer task for placing selected products on checkout counter
    /// Handles product placement with proper spacing and organization
    /// </summary>
    public class PlaceProductsTask : Action
    {
        [Header("Settings Override (Optional)")]
        [Tooltip("Leave null to use global settings from CustomerBehaviorSettingsManager")]
        public CustomerBehaviorSettings settingsOverride;
        
        private CheckoutCounter checkoutCounter = null;
        private bool hasStartedPlacement = false;
        private int productsPlaced = 0;
        
        /// <summary>
        /// Get the checkout settings to use (either override or global)
        /// </summary>
        private CheckoutSettings GetCheckoutSettings()
        {
            if (settingsOverride != null && settingsOverride.checkout != null)
                return settingsOverride.checkout;
            
            return CustomerBehaviorSettingsManager.Checkout;
        }
        
        public override void OnStart()
        {
            Customer customer = GetComponent<Customer>();
            if (customer == null)
            {
                Debug.LogError("[PlaceProductsTask] Customer component not found!");
                return;
            }
            
            // Find checkout counter
            checkoutCounter = FindNearestCheckoutCounter();
            if (checkoutCounter == null)
            {
                Debug.LogError($"[PlaceProductsTask] {customer.name}: No checkout counter found!");
                return;
            }
            
            // Check if customer can place items (is current customer)
            if (!checkoutCounter.CanCustomerPlaceItems(customer))
            {
                Debug.LogError($"[PlaceProductsTask] {customer.name}: Not authorized to place items!");
                return;
            }
            
            // Check if customer has products to place
            if (customer.selectedProducts == null || customer.selectedProducts.Count == 0)
            {
                Debug.LogWarning($"[PlaceProductsTask] {customer.name}: No products to place at checkout");
                return;
            }
            
            // Start placing products
            StartCoroutine(PlaceProductsSequentially(customer));
            hasStartedPlacement = true;
            
            if (customer.showDebugLogs)
                Debug.Log($"[PlaceProductsTask] {customer.name}: Started placing {customer.selectedProducts.Count} products");
        }
        
        public override TaskStatus OnUpdate()
        {
            if (!hasStartedPlacement)
                return TaskStatus.Failure;
                
            Customer customer = GetComponent<Customer>();
            if (customer == null)
                return TaskStatus.Failure;
            
            // Check if all products have been placed
            if (productsPlaced >= customer.selectedProducts.Count)
            {
                if (customer.showDebugLogs)
                    Debug.Log($"[PlaceProductsTask] ✅ {customer.name}: All {productsPlaced} products placed on counter");
                return TaskStatus.Success;
            }
            
            return TaskStatus.Running;
        }
        
        public override void OnEnd()
        {
            hasStartedPlacement = false;
            productsPlaced = 0;
        }
        
        /// <summary>
        /// Place products on counter one by one with spacing
        /// </summary>
        /// <param name="customer">Customer placing products</param>
        /// <returns>Coroutine</returns>
        private IEnumerator PlaceProductsSequentially(Customer customer)
        {
            productsPlaced = 0;
            
            foreach (Product product in customer.selectedProducts)
            {
                if (product != null)
                {
                    // Place product on checkout counter
                    checkoutCounter.PlaceProduct(product, customer);
                    productsPlaced++;
                    
                    if (customer.showDebugLogs)
                        Debug.Log($"[PlaceProductsTask] {customer.name}: Placed product {productsPlaced}/{customer.selectedProducts.Count}: {product.ProductData?.ProductName ?? product.name}");
                    
                    // Wait before placing next product using settings
                    if (productsPlaced < customer.selectedProducts.Count)
                    {
                        var checkoutSettings = GetCheckoutSettings();
                        float placementInterval = checkoutSettings?.productPlacementDelay ?? 0.5f;
                        yield return new WaitForSeconds(placementInterval);
                    }
                }
            }
            
            if (customer.showDebugLogs)
                Debug.Log($"[PlaceProductsTask] {customer.name}: Finished placing all products on counter");
        }
        
        /// <summary>
        /// Find the nearest checkout counter in the scene
        /// </summary>
        /// <returns>Nearest CheckoutCounter or null if none found</returns>
        private CheckoutCounter FindNearestCheckoutCounter()
        {
            CheckoutCounter[] checkoutCounters = Object.FindObjectsByType<CheckoutCounter>(FindObjectsSortMode.None);
            
            if (checkoutCounters.Length == 0)
            {
                return null;
            }
            
            // Return the first one for now
            return checkoutCounters[0];
        }
    }
}