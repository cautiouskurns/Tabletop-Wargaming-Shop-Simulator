using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace TabletopShop
{
    /// <summary>
    /// Read-only interface for querying inventory state
    /// Provides access to inventory status without modification capabilities
    /// </summary>
    public interface IInventoryQuery
    {
        /// <summary>
        /// Currently selected product for placement
        /// </summary>
        ProductData SelectedProduct { get; }
        
        /// <summary>
        /// All available products in the inventory system
        /// </summary>
        List<ProductData> AvailableProducts { get; }
        
        /// <summary>
        /// Total number of unique products in inventory
        /// </summary>
        int UniqueProductCount { get; }
        
        /// <summary>
        /// Total quantity of all products in inventory
        /// </summary>
        int TotalProductCount { get; }
        
        /// <summary>
        /// Event fired when inventory changes
        /// </summary>
        UnityEvent OnInventoryChanged { get; }
        
        /// <summary>
        /// Event fired when a product is selected
        /// </summary>
        UnityEvent<ProductData> OnProductSelected { get; }
        
        /// <summary>
        /// Event fired when a specific product count changes
        /// </summary>
        UnityEvent<ProductData, int> OnProductCountChanged { get; }
        
        /// <summary>
        /// Check if the inventory has a specific product in the required amount
        /// </summary>
        /// <param name="product">The product to check for</param>
        /// <param name="amount">The amount required (default: 1)</param>
        /// <returns>True if inventory has enough of the product</returns>
        bool HasProduct(ProductData product, int amount = 1);
        
        /// <summary>
        /// Get the quantity of a specific product in inventory
        /// </summary>
        /// <param name="product">The product to check</param>
        /// <returns>The quantity of the product in inventory (0 if not found)</returns>
        int GetProductCount(ProductData product);
        
        /// <summary>
        /// Check if inventory is empty
        /// </summary>
        /// <returns>True if no products have quantity > 0</returns>
        bool IsEmpty();
        
        /// <summary>
        /// Get inventory status as formatted string
        /// </summary>
        /// <returns>String representation of current inventory</returns>
        string GetInventoryStatus();
        
        /// <summary>
        /// Get all products that have quantity > 0
        /// </summary>
        /// <returns>List of products with available quantity</returns>
        List<ProductData> GetAvailableProductsWithQuantity();
        
        /// <summary>
        /// Get all products grouped by type
        /// </summary>
        /// <returns>Dictionary of product types to product lists</returns>
        Dictionary<ProductType, List<ProductData>> GetProductsByType();
    }
}
