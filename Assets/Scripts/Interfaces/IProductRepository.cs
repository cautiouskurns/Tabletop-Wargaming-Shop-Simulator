using System.Collections.Generic;
using UnityEngine;

namespace TabletopShop
{
    /// <summary>
    /// Interface for product data access operations.
    /// Abstracts product loading concerns from inventory management.
    /// </summary>
    public interface IProductRepository
    {
        /// <summary>
        /// Load all available products from the data source
        /// </summary>
        /// <returns>List of all available ProductData objects</returns>
        List<ProductData> LoadAllProducts();
        
        /// <summary>
        /// Get a specific product by its identifier
        /// </summary>
        /// <param name="productId">The unique identifier of the product</param>
        /// <returns>ProductData if found, null otherwise</returns>
        ProductData GetProduct(string productId);
        
        /// <summary>
        /// Validate if a product is valid and not null
        /// </summary>
        /// <param name="product">The product to validate</param>
        /// <returns>True if product is valid</returns>
        bool IsValidProduct(ProductData product);
    }
}
