using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace TabletopShop
{
    /// <summary>
    /// Example showing how to use the inventory interfaces with dependency injection
    /// </summary>
    public class InventoryInterfaceExamples : MonoBehaviour
    {
        // UI component that only needs to query inventory (read-only operations)
        [SerializeField] private Text inventoryStatusText;
        [SerializeField] private Text selectedProductText;
        
        // Cached references to interfaces
        private IInventoryQuery inventoryQuery;
        private IInventoryManager inventoryManager;
        
        private void Start()
        {
            // Get both interfaces from the same InventoryManager instance
            var manager = InventoryManager.Instance;
            inventoryQuery = manager;
            inventoryManager = manager;
            
            // Set up listeners using the query interface
            if (inventoryQuery.OnInventoryChanged != null)
            {
                inventoryQuery.OnInventoryChanged.AddListener(UpdateUI);
            }
            
            if (inventoryQuery.OnProductSelected != null)
            {
                inventoryQuery.OnProductSelected.AddListener(OnProductSelected);
            }
            
            // Initial UI update
            UpdateUI();
        }
        
        // Example of UI component using only IInventoryQuery (read-only)
        private void UpdateUI()
        {
            if (inventoryStatusText != null)
            {
                inventoryStatusText.text = inventoryQuery.GetInventoryStatus();
            }
            
            if (selectedProductText != null)
            {
                var selectedProduct = inventoryQuery.SelectedProduct;
                selectedProductText.text = selectedProduct != null 
                    ? $"Selected: {selectedProduct.ProductName} (Qty: {inventoryQuery.GetProductCount(selectedProduct)})"
                    : "No product selected";
            }
        }
        
        private void OnProductSelected(ProductData product)
        {
            Debug.Log($"Product selected: {product?.ProductName ?? "None"}");
        }
        
        // Example of using IInventoryManager for modifications
        public void AddSelectedProductToInventory(int amount)
        {
            var selectedProduct = inventoryQuery.SelectedProduct;
            
            if (selectedProduct == null)
            {
                Debug.LogWarning("Cannot add product - no product selected");
                return;
            }
            
            // Use the manager interface to modify inventory
            bool success = inventoryManager.AddProduct(selectedProduct, amount);
            
            if (success)
            {
                Debug.Log($"Added {amount} of {selectedProduct.ProductName}. New total: {inventoryQuery.GetProductCount(selectedProduct)}");
            }
            else
            {
                Debug.LogWarning($"Failed to add {amount} of {selectedProduct.ProductName} to inventory");
            }
        }
        
        // Example of using IInventoryManager to select a product
        public void SelectProductByName(string productName)
        {
            foreach (var product in inventoryQuery.AvailableProducts)
            {
                if (product.ProductName == productName && inventoryQuery.HasProduct(product))
                {
                    // Use manager interface to modify selection
                    inventoryManager.SelectProduct(product);
                    return;
                }
            }
            
            Debug.LogWarning($"Could not find product with name '{productName}' or it has no quantity in inventory");
        }
        
        // Example of using IInventoryManager to remove a product
        public void RemoveSelectedProduct(int amount)
        {
            var selectedProduct = inventoryQuery.SelectedProduct;
            
            if (selectedProduct == null)
            {
                Debug.LogWarning("Cannot remove product - no product selected");
                return;
            }
            
            // Check first using query interface if we have enough
            if (!inventoryQuery.HasProduct(selectedProduct, amount))
            {
                Debug.LogWarning($"Not enough {selectedProduct.ProductName} in inventory. Current quantity: {inventoryQuery.GetProductCount(selectedProduct)}");
                return;
            }
            
            // Use manager interface to modify inventory
            inventoryManager.RemoveProduct(selectedProduct, amount);
        }
    }
}
