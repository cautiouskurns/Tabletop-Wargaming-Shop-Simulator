using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using System.Linq;

namespace TabletopShop
{
    /// <summary>
    /// Singleton manager for handling player inventory of products
    /// Manages product quantities, selection, and inventory operations
    /// </summary>
    public class InventoryManager : MonoBehaviour
    {
        #region Singleton Pattern
        
        private static InventoryManager _instance;
        
        /// <summary>
        /// Singleton instance of the InventoryManager
        /// </summary>
        public static InventoryManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<InventoryManager>();
                    
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("InventoryManager");
                        _instance = go.AddComponent<InventoryManager>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }
        
        #endregion
        
        #region Fields and Properties
        
        [Header("Inventory Configuration")]
        [SerializeField] private List<ProductData> availableProducts = new List<ProductData>();
        [SerializeField] private int startingQuantityPerProduct = 5;
        [SerializeField] private bool autoLoadProductsOnStart = true;
        
        [Header("Current Selection")]
        [SerializeField] private ProductData selectedProduct;
        
        [Header("Events")]
        [SerializeField] private UnityEvent onInventoryChanged;
        [SerializeField] private UnityEvent<ProductData> onProductSelected;
        [SerializeField] private UnityEvent<ProductData, int> onProductCountChanged;
        
        // Internal inventory tracking
        private Dictionary<ProductData, int> productCounts = new Dictionary<ProductData, int>();
        private bool isInitialized = false;
        
        /// <summary>
        /// Currently selected product for placement
        /// </summary>
        public ProductData SelectedProduct 
        { 
            get => selectedProduct;
            private set
            {
                if (selectedProduct != value)
                {
                    selectedProduct = value;
                    onProductSelected?.Invoke(selectedProduct);
                    Debug.Log($"Selected product: {selectedProduct?.ProductName ?? "None"}");
                }
            }
        }
        
        /// <summary>
        /// All available products in the inventory system
        /// </summary>
        public List<ProductData> AvailableProducts => availableProducts;
        
        /// <summary>
        /// Total number of unique products in inventory
        /// </summary>
        public int UniqueProductCount => productCounts.Count;
        
        /// <summary>
        /// Total quantity of all products in inventory
        /// </summary>
        public int TotalProductCount => productCounts.Values.Sum();
        
        /// <summary>
        /// Event fired when inventory changes
        /// </summary>
        public UnityEvent OnInventoryChanged => onInventoryChanged;
        
        /// <summary>
        /// Event fired when a product is selected
        /// </summary>
        public UnityEvent<ProductData> OnProductSelected => onProductSelected;
        
        /// <summary>
        /// Event fired when a specific product count changes
        /// </summary>
        public UnityEvent<ProductData, int> OnProductCountChanged => onProductCountChanged;
        
        #endregion
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            // Implement singleton pattern
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeInventory();
            }
            else if (_instance != this)
            {
                Debug.LogWarning("Multiple InventoryManager instances detected. Destroying duplicate.");
                Destroy(gameObject);
            }
        }
        
        private void Start()
        {
            if (autoLoadProductsOnStart)
            {
                LoadAvailableProducts();
                InitializeStartingInventory();
            }
        }
        
        #endregion
        
        #region Initialization
        
        /// <summary>
        /// Initialize the inventory system
        /// </summary>
        private void InitializeInventory()
        {
            if (isInitialized) return;
            
            productCounts = new Dictionary<ProductData, int>();
            
            // Initialize events if they're null
            if (onInventoryChanged == null)
                onInventoryChanged = new UnityEvent();
            if (onProductSelected == null)
                onProductSelected = new UnityEvent<ProductData>();
            if (onProductCountChanged == null)
                onProductCountChanged = new UnityEvent<ProductData, int>();
            
            isInitialized = true;
            Debug.Log("InventoryManager initialized successfully.");
        }
        
        /// <summary>
        /// Load all available ProductData assets from Resources
        /// </summary>
        private void LoadAvailableProducts()
        {
            Debug.Log($"LoadAvailableProducts called. Current available products count: {availableProducts.Count}");
            
            if (availableProducts.Count > 0)
            {
                Debug.Log($"Using {availableProducts.Count} manually assigned products:");
                for (int i = 0; i < availableProducts.Count; i++)
                {
                    if (availableProducts[i] != null)
                    {
                        Debug.Log($"  - {i}: {availableProducts[i].ProductName} (Type: {availableProducts[i].Type})");
                    }
                    else
                    {
                        Debug.LogWarning($"  - {i}: NULL PRODUCT FOUND!");
                    }
                }
                return;
            }
            
            // Try to load ProductData assets from Resources folder
            ProductData[] foundProducts = Resources.LoadAll<ProductData>("Products");
            
            if (foundProducts.Length > 0)
            {
                availableProducts.AddRange(foundProducts);
                Debug.Log($"Loaded {foundProducts.Length} products from Resources/Products folder:");
                foreach (var product in foundProducts)
                {
                    Debug.Log($"  - {product.ProductName} (Type: {product.Type})");
                }
            }
            else
            {
                Debug.LogWarning("No ProductData assets found. You may need to create some or assign them manually.");
                CreateDefaultProducts();
            }
        }
        
        /// <summary>
        /// Create default products if none are found
        /// </summary>
        private void CreateDefaultProducts()
        {
            Debug.Log("Creating default product entries for testing...");
            
            // Note: These are placeholder references. In a real setup, you'd have actual ProductData ScriptableObject assets
            // For now, we'll just log that we need actual ProductData assets
            Debug.LogWarning("To fully test the inventory system, create ProductData ScriptableObject assets and assign them to the InventoryManager.");
        }
        
        /// <summary>
        /// Initialize starting inventory quantities
        /// </summary>
        private void InitializeStartingInventory()
        {
            Debug.Log($"InitializeStartingInventory called. Processing {availableProducts.Count} products...");
            
            foreach (ProductData product in availableProducts)
            {
                if (product != null)
                {
                    Debug.Log($"Adding {startingQuantityPerProduct} of {product.ProductName} to inventory");
                    AddProduct(product, startingQuantityPerProduct, false); // false = don't trigger events during initialization
                }
                else
                {
                    Debug.LogWarning("Found null product in availableProducts list!");
                }
            }
            
            // Select the first product if available
            if (availableProducts.Count > 0 && availableProducts[0] != null)
            {
                SelectProduct(availableProducts[0]);
                Debug.Log($"Selected first product: {availableProducts[0].ProductName}");
            }
            
            // Trigger inventory changed event after all initialization
            onInventoryChanged?.Invoke();
            
            Debug.Log($"Initialized starting inventory with {startingQuantityPerProduct} of each product type.");
            LogInventoryStatus();
        }
        
        #endregion
        
        #region Public Inventory Methods
        
        /// <summary>
        /// Check if the inventory has a specific product in the required amount
        /// </summary>
        /// <param name="product">The product to check for</param>
        /// <param name="amount">The amount required (default: 1)</param>
        /// <returns>True if inventory has enough of the product</returns>
        public bool HasProduct(ProductData product, int amount = 1)
        {
            if (product == null)
            {
                Debug.LogWarning("Cannot check for null product in inventory.");
                return false;
            }
            
            if (amount <= 0)
            {
                Debug.LogWarning($"Invalid amount {amount} for product check. Amount must be positive.");
                return false;
            }
            
            return GetProductCount(product) >= amount;
        }
        
        /// <summary>
        /// Remove a product from inventory
        /// </summary>
        /// <param name="product">The product to remove</param>
        /// <param name="amount">The amount to remove (default: 1)</param>
        /// <returns>True if the product was successfully removed</returns>
        public bool RemoveProduct(ProductData product, int amount = 1)
        {
            if (product == null)
            {
                Debug.LogWarning("Cannot remove null product from inventory.");
                return false;
            }
            
            if (amount <= 0)
            {
                Debug.LogWarning($"Invalid amount {amount} for product removal. Amount must be positive.");
                return false;
            }
            
            if (!HasProduct(product, amount))
            {
                Debug.LogWarning($"Cannot remove {amount} of {product.ProductName}. Only have {GetProductCount(product)} in inventory.");
                return false;
            }
            
            // Remove the product
            productCounts[product] -= amount;
            
            // Remove from dictionary if count reaches zero
            if (productCounts[product] <= 0)
            {
                productCounts.Remove(product);
                
                // Clear selection if we just removed the selected product
                if (selectedProduct == product)
                {
                    SelectNextAvailableProduct();
                }
            }
            
            // Fire events
            onProductCountChanged?.Invoke(product, GetProductCount(product));
            onInventoryChanged?.Invoke();
            
            Debug.Log($"Removed {amount} of {product.ProductName}. Remaining: {GetProductCount(product)}");
            return true;
        }
        
        /// <summary>
        /// Add a product to inventory
        /// </summary>
        /// <param name="product">The product to add</param>
        /// <param name="amount">The amount to add (default: 1)</param>
        /// <param name="triggerEvents">Whether to trigger change events (default: true)</param>
        public void AddProduct(ProductData product, int amount = 1, bool triggerEvents = true)
        {
            if (product == null)
            {
                Debug.LogWarning("Cannot add null product to inventory.");
                return;
            }
            
            if (amount <= 0)
            {
                Debug.LogWarning($"Invalid amount {amount} for product addition. Amount must be positive.");
                return;
            }
            
            // Add to inventory
            if (productCounts.ContainsKey(product))
            {
                productCounts[product] += amount;
            }
            else
            {
                productCounts[product] = amount;
                
                // Add to available products if not already there
                if (!availableProducts.Contains(product))
                {
                    availableProducts.Add(product);
                }
            }
            
            // Fire events if requested
            if (triggerEvents)
            {
                onProductCountChanged?.Invoke(product, GetProductCount(product));
                onInventoryChanged?.Invoke();
            }
            
            Debug.Log($"Added {amount} of {product.ProductName}. Total: {GetProductCount(product)}");
        }
        
        /// <summary>
        /// Get the quantity of a specific product in inventory
        /// </summary>
        /// <param name="product">The product to check</param>
        /// <returns>The quantity of the product in inventory (0 if not found)</returns>
        public int GetProductCount(ProductData product)
        {
            if (product == null)
            {
                return 0;
            }
            
            return productCounts.ContainsKey(product) ? productCounts[product] : 0;
        }
        
        /// <summary>
        /// Select a product for placement/use
        /// </summary>
        /// <param name="product">The product to select</param>
        /// <returns>True if the product was successfully selected</returns>
        public bool SelectProduct(ProductData product)
        {
            if (product == null)
            {
                SelectedProduct = null;
                return true; // Allow clearing selection
            }
            
            if (!availableProducts.Contains(product))
            {
                Debug.LogWarning($"Cannot select {product.ProductName}. Product not in available products list.");
                return false;
            }
            
            if (!HasProduct(product))
            {
                Debug.LogWarning($"Cannot select {product.ProductName}. No quantity available in inventory.");
                return false;
            }
            
            SelectedProduct = product;
            return true;
        }
        
        /// <summary>
        /// Clear the current product selection
        /// </summary>
        public void ClearSelection()
        {
            SelectedProduct = null;
        }
        
        #endregion
        
        #region Helper Methods
        
        /// <summary>
        /// Select the next available product with quantity > 0
        /// </summary>
        private void SelectNextAvailableProduct()
        {
            ProductData nextProduct = availableProducts.FirstOrDefault(p => p != null && HasProduct(p));
            SelectedProduct = nextProduct;
        }
        
        /// <summary>
        /// Get all products that have quantity > 0
        /// </summary>
        /// <returns>List of products with available quantity</returns>
        public List<ProductData> GetAvailableProductsWithQuantity()
        {
            return availableProducts.Where(p => p != null && HasProduct(p)).ToList();
        }
        
        /// <summary>
        /// Get all products grouped by type
        /// </summary>
        /// <returns>Dictionary of product types to product lists</returns>
        public Dictionary<ProductType, List<ProductData>> GetProductsByType()
        {
            var result = new Dictionary<ProductType, List<ProductData>>();
            
            foreach (var product in availableProducts.Where(p => p != null))
            {
                if (!result.ContainsKey(product.Type))
                {
                    result[product.Type] = new List<ProductData>();
                }
                result[product.Type].Add(product);
            }
            
            return result;
        }
        
        /// <summary>
        /// Check if inventory is empty
        /// </summary>
        /// <returns>True if no products have quantity > 0</returns>
        public bool IsEmpty()
        {
            return TotalProductCount == 0;
        }
        
        /// <summary>
        /// Get inventory status as formatted string
        /// </summary>
        /// <returns>String representation of current inventory</returns>
        public string GetInventoryStatus()
        {
            if (IsEmpty())
            {
                return "Inventory is empty.";
            }
            
            var status = $"Inventory Status (Total: {TotalProductCount} items):\n";
            
            foreach (var kvp in productCounts.OrderBy(x => x.Key.ProductName))
            {
                string selectedIndicator = kvp.Key == selectedProduct ? " [SELECTED]" : "";
                status += $"- {kvp.Key.ProductName}: {kvp.Value}{selectedIndicator}\n";
            }
            
            return status;
        }
        
        /// <summary>
        /// Log current inventory status to console
        /// </summary>
        public void LogInventoryStatus()
        {
            Debug.Log(GetInventoryStatus());
        }
        
        #endregion
        
        #region Validation and Testing
        
        /// <summary>
        /// Validate the inventory state
        /// </summary>
        /// <returns>True if inventory is in a valid state</returns>
        public bool ValidateInventory()
        {
            bool isValid = true;
            
            // Check for null products in available products
            int nullCount = availableProducts.Count(p => p == null);
            if (nullCount > 0)
            {
                Debug.LogWarning($"Found {nullCount} null products in available products list.");
                isValid = false;
            }
            
            // Check for negative quantities
            var negativeProducts = productCounts.Where(kvp => kvp.Value < 0).ToList();
            if (negativeProducts.Any())
            {
                Debug.LogError($"Found products with negative quantities: {string.Join(", ", negativeProducts.Select(kvp => $"{kvp.Key.ProductName}: {kvp.Value}"))}");
                isValid = false;
            }
            
            // Check if selected product is valid
            if (selectedProduct != null && !HasProduct(selectedProduct))
            {
                Debug.LogWarning($"Selected product {selectedProduct.ProductName} has no quantity available.");
                isValid = false;
            }
            
            return isValid;
        }
        
        /// <summary>
        /// Reset inventory to initial state
        /// </summary>
        [ContextMenu("Reset Inventory")]
        public void ResetInventory()
        {
            productCounts.Clear();
            selectedProduct = null;
            
            InitializeStartingInventory();
            
            Debug.Log("Inventory has been reset to initial state.");
        }
        
        /// <summary>
        /// Add test products for development
        /// </summary>
        [ContextMenu("Add Test Products")]
        public void AddTestProducts()
        {
            foreach (var product in availableProducts.Where(p => p != null))
            {
                AddProduct(product, 10);
            }
            
            Debug.Log("Added 10 of each available product for testing.");
        }
        
        /// <summary>
        /// Force reload and reinitialize the inventory system
        /// </summary>
        [ContextMenu("Force Reload Inventory")]
        public void ForceReloadInventory()
        {
            Debug.Log("=== FORCE RELOADING INVENTORY ===");
            productCounts.Clear();
            selectedProduct = null;
            
            LoadAvailableProducts();
            InitializeStartingInventory();
            
            Debug.Log("=== INVENTORY RELOAD COMPLETE ===");
        }
        
        /// <summary>
        /// Debug current inventory state
        /// </summary>
        [ContextMenu("Debug Inventory State")]
        public void DebugInventoryState()
        {
            Debug.Log("=== INVENTORY DEBUG INFO ===");
            Debug.Log($"Available Products List Count: {availableProducts.Count}");
            Debug.Log($"Product Counts Dictionary Count: {productCounts.Count}");
            Debug.Log($"Total Product Count: {TotalProductCount}");
            Debug.Log($"Unique Product Count: {UniqueProductCount}");
            Debug.Log($"Selected Product: {selectedProduct?.ProductName ?? "None"}");
            Debug.Log($"Auto Load Products On Start: {autoLoadProductsOnStart}");
            Debug.Log($"Starting Quantity Per Product: {startingQuantityPerProduct}");
            Debug.Log($"Is Initialized: {isInitialized}");
            
            Debug.Log("Available Products Details:");
            for (int i = 0; i < availableProducts.Count; i++)
            {
                if (availableProducts[i] != null)
                {
                    Debug.Log($"  [{i}] {availableProducts[i].ProductName} - Count: {GetProductCount(availableProducts[i])}");
                }
                else
                {
                    Debug.Log($"  [{i}] NULL PRODUCT");
                }
            }
            Debug.Log("=== END DEBUG INFO ===");
        }
        
        #endregion
    }
}
