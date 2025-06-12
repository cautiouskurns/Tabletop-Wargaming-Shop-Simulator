using UnityEngine;

namespace TabletopShop
{
    /// <summary>
    /// Interface for publishing inventory-related events.
    /// Provides abstraction layer for event publishing, allowing different implementations
    /// for UI updates, testing, or alternative event systems.
    /// </summary>
    public interface IInventoryEventPublisher
    {
        /// <summary>
        /// Publish an inventory changed event
        /// Fired when the overall inventory state changes (items added/removed)
        /// </summary>
        void PublishInventoryChanged();
        
        /// <summary>
        /// Publish a product selection event
        /// Fired when a different product is selected for placement/use
        /// </summary>
        /// <param name="product">The newly selected product (null if selection cleared)</param>
        void PublishProductSelected(ProductData product);
        
        /// <summary>
        /// Publish a product count changed event
        /// Fired when a specific product's quantity changes
        /// </summary>
        /// <param name="product">The product whose count changed</param>
        /// <param name="count">The new count of the product</param>
        void PublishProductCountChanged(ProductData product, int count);
    }
}
