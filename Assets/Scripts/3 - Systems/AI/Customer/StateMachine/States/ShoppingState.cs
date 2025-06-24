using UnityEngine;
using System.Collections;

namespace TabletopShop
{
    /// <summary>
    /// SHOPPING STATE - Controls all shopping behavior
    /// </summary>
    public class ShoppingState : BaseCustomerState
    {
        private float shoppingStartTime;
        private float lastProductCheckTime;
        private float lastShelfSwitchTime;
        private const float PRODUCT_CHECK_INTERVAL = 3f;
        private const float SHELF_SWITCH_PROBABILITY = 0.3f;
        private const float SHELF_SWITCH_COOLDOWN = 2f;
        
        public override void OnEnter(CustomerBehavior customer)
        {
            this.customer = customer;
            shoppingStartTime = Time.time;
            lastProductCheckTime = Time.time;
            lastShelfSwitchTime = Time.time;
            
            Debug.Log($"{customer.name} started shopping for {customer.ShoppingTime:F1}s");
        }
        
        public override void OnUpdate(CustomerBehavior customer)
        {
            float shoppedTime = Time.time - shoppingStartTime;
            float targetShoppingTime = customer.ShoppingTime;
            
            // STATE DECIDES: Check if store is closing
            if (!IsStoreOpen())
            {
                Debug.Log($"{customer.name}: Store closed during shopping");
                RequestTransition(CustomerState.Leaving, "Store closed");
                return;
            }
            
            // STATE DECIDES: Check if should hurry up
            if (IsStoreClosingSoon())
            {
                targetShoppingTime *= 0.7f; // Reduce shopping time
                Debug.Log($"{customer.name}: Store closing soon - hurrying up");
            }
            
            // STATE DECIDES: Check if shopping time is complete
            if (shoppedTime >= targetShoppingTime)
            {
                Debug.Log($"{customer.name}: Shopping time complete ({shoppedTime:F1}s)");
                RequestTransition(CustomerState.Purchasing, "Shopping time complete");
                return;
            }
            
            // STATE CONTROLS: Handle shelf browsing
            HandleShelfBrowsing(shoppedTime);
            
            // STATE CONTROLS: Handle product selection
            HandleProductSelection(shoppedTime);
        }
        
        public override void OnExit(CustomerBehavior customer)
        {
            float totalShoppingTime = Time.time - shoppingStartTime;
            Debug.Log($"{customer.name} finished shopping after {totalShoppingTime:F1}s with {customer.SelectedProducts.Count} products");
        }
        
        /// <summary>
        /// STATE CONTROLS: Handle shelf browsing and movement
        /// </summary>
        private void HandleShelfBrowsing(float shoppedTime)
        {
            var movement = customer.GetComponent<CustomerMovement>();
            if (movement == null) return;
            
            // Switch shelves occasionally
            bool shouldSwitchShelf = false;
            
            if (movement.HasReachedDestination())
            {
                shouldSwitchShelf = true;
            }
            else if (Time.time - lastShelfSwitchTime > SHELF_SWITCH_COOLDOWN && 
                     UnityEngine.Random.value < SHELF_SWITCH_PROBABILITY)
            {
                shouldSwitchShelf = true;
            }
            
            if (shouldSwitchShelf)
            {
                SwitchToRandomShelf();
            }
        }
        
        /// <summary>
        /// STATE CONTROLS: Switch to a random shelf
        /// </summary>
        private void SwitchToRandomShelf()
        {
            ShelfSlot[] availableShelves = Object.FindObjectsByType<ShelfSlot>(FindObjectsSortMode.None);
            
            if (availableShelves.Length == 0) return;
            
            ShelfSlot randomShelf = availableShelves[UnityEngine.Random.Range(0, availableShelves.Length)];
            customer.SetTargetShelf(randomShelf);
            
            var movement = customer.GetComponent<CustomerMovement>();
            if (movement != null && movement.MoveToShelfPosition(randomShelf))
            {
                lastShelfSwitchTime = Time.time;
                Debug.Log($"{customer.name} switched to shelf: {randomShelf.name}");
            }
        }
        
        /// <summary>
        /// STATE CONTROLS: Handle product selection
        /// </summary>
        private void HandleProductSelection(float shoppedTime)
        {
            if (shoppedTime - lastProductCheckTime >= PRODUCT_CHECK_INTERVAL)
            {
                lastProductCheckTime = shoppedTime;
                
                if (customer.TargetShelf != null)
                {
                    TrySelectProductsAtShelf(customer.TargetShelf);
                }
            }
        }
        
        /// <summary>
        /// STATE CONTROLS: Try to select products at a shelf
        /// </summary>
        private void TrySelectProductsAtShelf(ShelfSlot shelf)
        {
            if (shelf == null || shelf.IsEmpty || shelf.CurrentProduct == null) return;
            
            Product availableProduct = shelf.CurrentProduct;
            
            // Check if customer can afford and wants this product
            bool canAfford = CanAffordProduct(availableProduct);
            bool wantsProduct = WantsProduct(availableProduct);
            
            if (canAfford && wantsProduct)
            {
                // Add to selected products
                customer.SelectedProducts.Add(availableProduct);
                
                // Remove product from shelf
                Product removedProduct = shelf.RemoveProduct();
                if (removedProduct != null && removedProduct.IsOnShelf)
                {
                    removedProduct.RemoveFromShelf();
                }
                
                Debug.Log($"{customer.name} selected {availableProduct.ProductData?.ProductName ?? "Product"} for ${availableProduct.CurrentPrice}");
                
                // Start following customer
                customer.StartCoroutine(AttachProductToCustomer(availableProduct));
            }
        }
        
        /// <summary>
        /// STATE CHECKS: Can customer afford product
        /// </summary>
        private bool CanAffordProduct(Product product)
        {
            if (product == null) return false;
            
            float currentTotal = 0f;
            foreach (var selectedProduct in customer.SelectedProducts)
            {
                if (selectedProduct != null)
                    currentTotal += selectedProduct.CurrentPrice;
            }
            
            float remainingBudget = customer.BaseSpendingPower - currentTotal;
            return product.CurrentPrice <= remainingBudget;
        }
        
        /// <summary>
        /// STATE CHECKS: Does customer want this product
        /// </summary>
        private bool WantsProduct(Product product)
        {
            if (product == null || product.IsPurchased || !product.IsOnShelf) return false;
            
            // Simple probability check - could be expanded with preferences
            return UnityEngine.Random.value <= 0.8f; // 80% chance
        }
        
        /// <summary>
        /// STATE CONTROLS: Make product follow customer
        /// </summary>
        private IEnumerator AttachProductToCustomer(Product product)
        {
            if (product == null) yield break;
            
            Vector3 offset = new Vector3(0.3f, 1.2f, 0.2f);
            
            while (customer != null && product != null && 
                   customer.SelectedProducts.Contains(product) && 
                   !customer.IsWaitingForCheckout)
            {
                if (product.transform != null && customer.transform != null)
                {
                    product.transform.position = Vector3.Lerp(
                        product.transform.position,
                        customer.transform.position + customer.transform.rotation * offset,
                        Time.deltaTime * 5f);
                }
                
                yield return null;
            }
        }
    }
}
