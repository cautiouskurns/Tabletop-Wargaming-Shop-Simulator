using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TabletopShop
{
    /// <summary>
    /// Implementation of IProductRepository that loads products from Unity Resources folder.
    /// Handles all Resources.LoadAll operations and product validation.
    /// </summary>
    public class ResourcesProductRepository : IProductRepository
    {
        private readonly string resourcesPath;
        private readonly bool enableDebugLogging;
        
        /// <summary>
        /// Initialize the repository with optional configuration
        /// </summary>
        /// <param name="resourcesPath">Path within Resources folder to load products from (default: "Products")</param>
        /// <param name="enableDebugLogging">Whether to enable debug logging (default: true)</param>
        public ResourcesProductRepository(string resourcesPath = "Products", bool enableDebugLogging = true)
        {
            this.resourcesPath = resourcesPath;
            this.enableDebugLogging = enableDebugLogging;
        }
        
        /// <summary>
        /// Load all available products from the Resources folder
        /// </summary>
        /// <returns>List of all available ProductData objects</returns>
        public List<ProductData> LoadAllProducts()
        {
            var products = new List<ProductData>();
            
            if (enableDebugLogging)
            {
                Debug.Log($"ResourcesProductRepository: Loading products from Resources/{resourcesPath}");
            }
            
            try
            {
                // Try to load ProductData assets from Resources folder
                ProductData[] foundProducts = Resources.LoadAll<ProductData>(resourcesPath);
                
                if (foundProducts.Length > 0)
                {
                    // Validate and add found products
                    foreach (var product in foundProducts)
                    {
                        if (IsValidProduct(product))
                        {
                            products.Add(product);
                            if (enableDebugLogging)
                            {
                                Debug.Log($"  - Loaded: {product.ProductName} (Type: {product.Type})");
                            }
                        }
                        else
                        {
                            Debug.LogWarning($"  - Skipped invalid product: {product?.name ?? "NULL"}");
                        }
                    }
                    
                    if (enableDebugLogging)
                    {
                        Debug.Log($"ResourcesProductRepository: Successfully loaded {products.Count} valid products from Resources/{resourcesPath}");
                    }
                }
                else
                {
                    Debug.LogWarning($"ResourcesProductRepository: No ProductData assets found in Resources/{resourcesPath}");
                    LogProductCreationInstructions();
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"ResourcesProductRepository: Error loading products from Resources/{resourcesPath}: {ex.Message}");
            }
            
            return products;
        }
        
        /// <summary>
        /// Get a specific product by its identifier (ProductName)
        /// </summary>
        /// <param name="productId">The unique identifier of the product (ProductName)</param>
        /// <returns>ProductData if found, null otherwise</returns>
        public ProductData GetProduct(string productId)
        {
            if (string.IsNullOrEmpty(productId))
            {
                if (enableDebugLogging)
                {
                    Debug.LogWarning("ResourcesProductRepository: Cannot get product with null or empty ID");
                }
                return null;
            }
            
            try
            {
                // Load all products and find by name
                ProductData[] allProducts = Resources.LoadAll<ProductData>(resourcesPath);
                var product = allProducts.FirstOrDefault(p => p != null && p.ProductName == productId);
                
                if (product == null && enableDebugLogging)
                {
                    Debug.LogWarning($"ResourcesProductRepository: Product with ID '{productId}' not found in Resources/{resourcesPath}");
                }
                
                return product;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"ResourcesProductRepository: Error getting product '{productId}': {ex.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// Validate if a product is valid and not null
        /// </summary>
        /// <param name="product">The product to validate</param>
        /// <returns>True if product is valid</returns>
        public bool IsValidProduct(ProductData product)
        {
            if (product == null)
            {
                return false;
            }
            
            // Check for valid product name
            if (string.IsNullOrEmpty(product.ProductName))
            {
                if (enableDebugLogging)
                {
                    Debug.LogWarning($"ResourcesProductRepository: Product validation failed - empty ProductName for {product.name}");
                }
                return false;
            }
            
            // Check for valid base price
            if (product.BasePrice <= 0)
            {
                if (enableDebugLogging)
                {
                    Debug.LogWarning($"ResourcesProductRepository: Product validation failed - invalid BasePrice {product.BasePrice} for {product.ProductName}");
                }
                return false;
            }
            
            return true;
        }
        
        /// <summary>
        /// Log instructions for creating products when none are found
        /// </summary>
        private void LogProductCreationInstructions()
        {
            Debug.LogWarning("ResourcesProductRepository: No products found. To create products:");
            Debug.LogWarning("1. Create ProductData ScriptableObject assets");
            Debug.LogWarning($"2. Place them in a Resources/{resourcesPath} folder");
            Debug.LogWarning("3. Or assign products manually to the InventoryManager");
        }
    }
}
