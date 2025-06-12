using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace TabletopShop
{
    /// <summary>
    /// Core interface for inventory management operations
    /// Handles adding, removing, and selecting products in inventory
    /// </summary>
    public interface IInventoryManager
    {
        /// <summary>
        /// Add a product to inventory with optional economic constraints
        /// </summary>
        /// <param name="product">The product to add</param>
        /// <param name="amount">The amount to add (default: 1)</param>
        /// <param name="triggerEvents">Whether to trigger change events (default: true)</param>
        /// <param name="cost">Optional cost per unit for economic validation. If null, calculated from restockCostMultiplier</param>
        /// <returns>True if product was successfully added, false if economic constraints prevented it</returns>
        bool AddProduct(ProductData product, int amount = 1, bool triggerEvents = true, float? cost = null);
        
        /// <summary>
        /// Add a product to inventory (legacy method for backward compatibility)
        /// </summary>
        /// <param name="product">The product to add</param>
        /// <param name="amount">The amount to add</param>
        /// <param name="triggerEvents">Whether to trigger change events</param>
        void AddProduct(ProductData product, int amount, bool triggerEvents);
        
        /// <summary>
        /// Remove a product from inventory
        /// </summary>
        /// <param name="product">The product to remove</param>
        /// <param name="amount">The amount to remove (default: 1)</param>
        /// <returns>True if the product was successfully removed</returns>
        bool RemoveProduct(ProductData product, int amount = 1);
        
        /// <summary>
        /// Select a product for placement/use
        /// </summary>
        /// <param name="product">The product to select</param>
        /// <returns>True if the product was successfully selected</returns>
        bool SelectProduct(ProductData product);
        
        /// <summary>
        /// Clear the current product selection
        /// </summary>
        void ClearSelection();
        
        /// <summary>
        /// Currently selected product for placement
        /// </summary>
        ProductData SelectedProduct { get; }
        
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
    }
}
