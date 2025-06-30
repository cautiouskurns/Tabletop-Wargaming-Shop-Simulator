using UnityEngine;
using Opsive.BehaviorDesigner.Runtime.Tasks.Actions;
using Opsive.BehaviorDesigner.Runtime.Tasks;

namespace TabletopShop
{
    public class SelectProductTask : Action
    {
        [Header("Settings Override (Optional)")]
        [Tooltip("Leave null to use global settings from CustomerBehaviorSettingsManager")]
        public CustomerBehaviorSettings settingsOverride;
        
        /// <summary>
        /// Get the shopping settings to use (either override or global)
        /// </summary>
        private ShoppingSettings GetShoppingSettings()
        {
            if (settingsOverride != null && settingsOverride.shopping != null)
                return settingsOverride.shopping;
            
            return CustomerBehaviorSettingsManager.Shopping;
        }
        
        public override TaskStatus OnUpdate()
        {
            Customer customer = GetComponent<Customer>();
            if (customer == null || customer.currentTargetShelf == null)
            {
                Debug.LogError("[SelectProductTask] No customer data or target shelf!");
                return TaskStatus.Failure; // Only fail on real errors
            }
            
            // Try to select product (this might fail for various reasons)
            bool selectedProduct = TrySelectProduct(customer);
            
            if (selectedProduct)
            {
                if (customer.showDebugLogs)
                    Debug.Log("[SelectProductTask] ✅ Product selected!");
            }
            else
            {
                if (customer.showDebugLogs)
                    Debug.Log("[SelectProductTask] ⏭️ No product selected, continuing shopping...");
            }
            
            // Always return Success - let the exit conditions handle when to stop
            return TaskStatus.Success;
        }
        
        private bool TrySelectProduct(Customer customer)
        {
            ShelfSlot shelf = customer.currentTargetShelf;
            
            // Check if shelf has product
            if (shelf.IsEmpty || shelf.CurrentProduct == null)
            {
                if (customer.showDebugLogs)
                    Debug.Log("[SelectProductTask] Shelf is empty");
                return false;
            }
            
            // Check if already have max products using settings
            var shoppingSettings = GetShoppingSettings();
            int maxProducts = shoppingSettings?.maxProducts ?? customer.MaxProducts;
            if (customer.selectedProducts.Count >= maxProducts)
            {
                if (customer.showDebugLogs)
                    Debug.Log($"[SelectProductTask] Already have maximum products ({maxProducts})");
                return false;
            }
            
            Product product = shelf.CurrentProduct;
            
            // Check if can afford
            if (product.CurrentPrice > customer.currentMoney)
            {
                if (customer.showDebugLogs)
                    Debug.Log($"[SelectProductTask] Cannot afford {product.ProductData?.ProductName}");
                return false;
            }
            
            // Buying decision logic using settings
            float buyProbability = shoppingSettings?.buyProbability ?? 0.7f;
            if (Random.value > buyProbability)
            {
                if (customer.showDebugLogs)
                    Debug.Log($"[SelectProductTask] Not interested in product (buy probability: {buyProbability:F2})");
                return false;
            }
            
            // Purchase the product
            return PurchaseProduct(customer, shelf, product);
        }
        
        private bool PurchaseProduct(Customer customer, ShelfSlot shelf, Product product)
        {
            // Add to customer's selected products and deduct money
            customer.AddProduct(product);
            
            // Remove from shelf
            Product removedProduct = shelf.RemoveProduct();
            if (removedProduct != null && removedProduct.IsOnShelf)
            {
                removedProduct.RemoveFromShelf();
            }
            
            if (customer.showDebugLogs)
                Debug.Log($"[SelectProductTask] ✅ PURCHASED {product.ProductData?.ProductName} for ${product.CurrentPrice}");
            
            return true;
        }
    }
}