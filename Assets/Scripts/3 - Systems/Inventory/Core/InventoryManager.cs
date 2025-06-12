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
                    _instance = FindAnyObjectByType<InventoryManager>();
                    
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
        
        [Header("Economic Constraints")]
        [SerializeField] private bool enableEconomicConstraints = true;
        [SerializeField] private float restockCostMultiplier = 0.7f; // 70% of retail price for restocking
        [SerializeField] private bool logEconomicTransactions = true;
        
        [Header("Current Selection")]
        [SerializeField] private ProductData selectedProduct;
        
        // Internal inventory tracking
        private Dictionary<ProductData, int> productCounts = new Dictionary<ProductData, int>();
        private bool isInitialized = false;
        
        // Event publishing abstraction
        private IInventoryEventPublisher eventPublisher;
        
        // Economic validation abstraction
        private IEconomicValidator economicValidator;
        
        // Product data access abstraction
        private IProductRepository productRepository;
        
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
                    eventPublisher?.PublishProductSelected(selectedProduct);
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
        public UnityEvent OnInventoryChanged => 
            eventPublisher is UnityInventoryEventPublisher unityPublisher ? unityPublisher.OnInventoryChanged : null;
        
        /// <summary>
        /// Event fired when a product is selected
        /// </summary>
        public UnityEvent<ProductData> OnProductSelected => 
            eventPublisher is UnityInventoryEventPublisher unityPublisher ? unityPublisher.OnProductSelected : null;
        
        /// <summary>
        /// Event fired when a specific product count changes
        /// </summary>
        public UnityEvent<ProductData, int> OnProductCountChanged => 
            eventPublisher is UnityInventoryEventPublisher unityPublisher ? unityPublisher.OnProductCountChanged : null;
        
        #endregion
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            // Implement singleton pattern
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
                
                // Initialize economic validator
                economicValidator = new GameManagerEconomicValidator(logEconomicTransactions);
                
                // Initialize product repository
                productRepository = new ResourcesProductRepository("Products", logEconomicTransactions);
                
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
                LoadAvailableProductsFromRepository();
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
            
            // Initialize event publisher
            if (eventPublisher == null)
            {
                var eventPublisherObject = new GameObject("InventoryEventPublisher");
                eventPublisherObject.transform.SetParent(transform);
                eventPublisher = eventPublisherObject.AddComponent<UnityInventoryEventPublisher>();
            }
            
            isInitialized = true;
            Debug.Log("InventoryManager initialized successfully.");
        }
        
        /// <summary>
        /// Load all available ProductData assets using the product repository
        /// </summary>
        private void LoadAvailableProductsFromRepository()
        {
            Debug.Log($"LoadAvailableProductsFromRepository called. Current available products count: {availableProducts.Count}");
            
            // If products are manually assigned in the inspector, use those instead
            if (availableProducts.Count > 0)
            {
                Debug.Log($"Using {availableProducts.Count} manually assigned products:");
                for (int i = 0; i < availableProducts.Count; i++)
                {
                    if (productRepository.IsValidProduct(availableProducts[i]))
                    {
                        Debug.Log($"  - {i}: {availableProducts[i].ProductName} (Type: {availableProducts[i].Type})");
                    }
                    else
                    {
                        Debug.LogWarning($"  - {i}: INVALID PRODUCT FOUND: {availableProducts[i]?.name ?? "NULL"}");
                    }
                }
                return;
            }
            
            // Load products from repository
            List<ProductData> loadedProducts = productRepository.LoadAllProducts();
            
            if (loadedProducts.Count > 0)
            {
                availableProducts.AddRange(loadedProducts);
                Debug.Log($"Successfully loaded {loadedProducts.Count} products from repository");
            }
            else
            {
                Debug.LogWarning("No ProductData assets found in repository. You may need to create some or assign them manually.");
            }
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
                    // Use legacy method without economic constraints during initialization
                    AddProduct(product, startingQuantityPerProduct, false);
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
            eventPublisher?.PublishInventoryChanged();
            
            Debug.Log($"Initialized starting inventory with {startingQuantityPerProduct} of each product type.");
            LogInventoryStatus();
        }
        
        #endregion
        
        #region Economic Constraint Methods
        
        /// <summary>
        /// Check if there are sufficient funds for restocking products at calculated cost
        /// </summary>
        /// <param name="product">The product to check restocking cost for</param>
        /// <param name="amount">The amount to restock</param>
        /// <returns>True if sufficient funds are available</returns>
        public bool HasSufficientFundsForRestock(ProductData product, int amount = 1)
        {
            if (product == null)
            {
                Debug.LogWarning("Cannot check restock funds for null product.");
                return false;
            }
            
            if (!enableEconomicConstraints)
            {
                return true; // Economic constraints disabled
            }
            
            float restockCost = economicValidator.CalculateRestockCost(amount, product.BasePrice, restockCostMultiplier);
            bool hasFunds = economicValidator.CanAffordCost(restockCost);
            
            if (logEconomicTransactions)
            {
                Debug.Log($"Restock fund check for {amount} of {product.ProductName}: Cost=${restockCost:F2}, Available=${economicValidator.GetAvailableFunds():F2}, HasFunds={hasFunds}");
            }
            
            return hasFunds;
        }
        
        /// <summary>
        /// Calculate the cost to restock a product
        /// </summary>
        /// <param name="product">The product to calculate cost for</param>
        /// <param name="amount">The amount to restock</param>
        /// <returns>Total restock cost</returns>
        public float CalculateRestockCost(ProductData product, int amount = 1)
        {
            if (product == null) return 0f;
            return economicValidator.CalculateRestockCost(amount, product.BasePrice, restockCostMultiplier);
        }
        
        /// <summary>
        /// Validate an inventory purchase transaction
        /// </summary>
        /// <param name="product">Product being purchased</param>
        /// <param name="amount">Amount being purchased</param>
        /// <param name="totalCost">Total cost of the transaction</param>
        /// <returns>True if transaction can proceed</returns>
        private bool ValidateInventoryPurchase(ProductData product, int amount, float totalCost)
        {
            // Check if economic validator is available
            if (!economicValidator.IsAvailable())
            {
                if (logEconomicTransactions)
                {
                    Debug.LogWarning($"Economic validator not available for inventory purchase validation of {product.ProductName}. Allowing operation.");
                }
                return true; // Graceful fallback
            }
            
            // Basic validation
            if (totalCost <= 0)
            {
                Debug.LogWarning($"Invalid total cost for inventory purchase: ${totalCost:F2}");
                return false;
            }
            
            // Check sufficient funds
            bool hasFunds = economicValidator.CanAffordCost(totalCost);
            
            if (logEconomicTransactions)
            {
                Debug.Log($"Inventory purchase validation: {amount} of {product.ProductName} for ${totalCost:F2}. HasFunds={hasFunds}");
            }
            
            return hasFunds;
        }
        
        /// <summary>
        /// Process an inventory purchase transaction through GameManager
        /// </summary>
        /// <param name="product">Product being purchased</param>
        /// <param name="amount">Amount being purchased</param>
        /// <param name="totalCost">Total cost of the transaction</param>
        /// <returns>True if transaction was successful</returns>
        private bool ProcessInventoryPurchase(ProductData product, int amount, float totalCost)
        {
            // Use economic validator to process transaction
            string description = $"Inventory Purchase: {amount} x {product.ProductName}";
            bool success = economicValidator.ProcessTransaction(totalCost, description);
            
            if (!success && logEconomicTransactions)
            {
                Debug.LogError($"Failed to process inventory purchase: {amount} of {product.ProductName} for ${totalCost:F2}");
            }
            
            return success;
        }
        
        #endregion
        
        #region Private Helper Methods for AddProduct
        
        /// <summary>
        /// Validate basic product addition parameters
        /// </summary>
        /// <param name="product">The product to validate</param>
        /// <param name="amount">The amount to validate</param>
        /// <returns>ValidationResult indicating success or failure with error message</returns>
        private ValidationResult ValidateProductAddition(ProductData product, int amount)
        {
            if (product == null)
            {
                return ValidationResult.Failure("Cannot add null product to inventory.");
            }
            
            if (amount <= 0)
            {
                return ValidationResult.Failure($"Invalid amount {amount} for product addition. Amount must be positive.");
            }
            
            return ValidationResult.Success();
        }
        
        /// <summary>
        /// Validate economic constraints for product addition
        /// </summary>
        /// <param name="product">The product to check constraints for</param>
        /// <param name="amount">The amount being added</param>
        /// <param name="costPerUnit">Optional cost per unit. If null, uses restock cost calculation</param>
        /// <returns>True if economic constraints are satisfied or disabled</returns>
        private bool ValidateEconomicConstraints(ProductData product, int amount, float? costPerUnit)
        {
            // Skip economic validation if constraints are disabled or no cost specified
            if (!enableEconomicConstraints || !costPerUnit.HasValue)
            {
                return true;
            }
            
            float totalCost = costPerUnit.Value * amount;
            
            // Validate economic transaction
            if (!ValidateInventoryPurchase(product, amount, totalCost))
            {
                if (logEconomicTransactions)
                {
                    Debug.LogWarning($"Economic constraints prevented adding {amount} of {product.ProductName}. Cost: ${totalCost:F2}");
                }
                return false;
            }
            
            // Process the economic transaction
            if (!ProcessInventoryPurchase(product, amount, totalCost))
            {
                return false;
            }
            
            return true;
        }
        
        /// <summary>
        /// Update inventory counts and notify listeners
        /// </summary>
        /// <param name="product">The product being added</param>
        /// <param name="amount">The amount being added</param>
        /// <param name="triggerEvents">Whether to trigger change events</param>
        /// <param name="costPerUnit">Optional cost per unit for logging purposes</param>
        private void UpdateInventoryAndNotify(ProductData product, int amount, bool triggerEvents, float? costPerUnit)
        {
            // Update inventory counts
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
                eventPublisher?.PublishProductCountChanged(product, GetProductCount(product));
                eventPublisher?.PublishInventoryChanged();
            }
            
            // Log the addition
            if (logEconomicTransactions && costPerUnit.HasValue)
            {
                float totalCost = costPerUnit.Value * amount;
                Debug.Log($"Added {amount} of {product.ProductName} for ${totalCost:F2}. Total: {GetProductCount(product)}");
            }
            else
            {
                Debug.Log($"Added {amount} of {product.ProductName}. Total: {GetProductCount(product)}");
            }
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
            eventPublisher?.PublishProductCountChanged(product, GetProductCount(product));
            eventPublisher?.PublishInventoryChanged();
            
            Debug.Log($"Removed {amount} of {product.ProductName}. Remaining: {GetProductCount(product)}");
            return true;
        }
        
        /// <summary>
        /// Add a product to inventory with optional economic constraints
        /// </summary>
        /// <param name="product">The product to add</param>
        /// <param name="amount">The amount to add (default: 1)</param>
        /// <param name="triggerEvents">Whether to trigger change events (default: true)</param>
        /// <param name="cost">Optional cost per unit for economic validation. If null, calculated from restockCostMultiplier</param>
        /// <returns>True if product was successfully added, false if economic constraints prevented it</returns>
        public bool AddProduct(ProductData product, int amount = 1, bool triggerEvents = true, float? cost = null)
        {
            // Step 1: Validate basic parameters
            var validation = ValidateProductAddition(product, amount);
            if (!validation.IsSuccess)
            {
                Debug.LogWarning(validation.ErrorMessage);
                return false;
            }
            
            // Step 2: Validate economic constraints
            if (!ValidateEconomicConstraints(product, amount, cost))
            {
                return false;
            }
            
            // Step 3: Update inventory and notify listeners
            UpdateInventoryAndNotify(product, amount, triggerEvents, cost);
            
            return true;
        }
        
        /// <summary>
        /// Add a product to inventory (legacy method for backward compatibility)
        /// </summary>
        /// <param name="product">The product to add</param>
        /// <param name="amount">The amount to add (default: 1)</param>
        /// <param name="triggerEvents">Whether to trigger change events (default: true)</param>
        public void AddProduct(ProductData product, int amount, bool triggerEvents)
        {
            AddProduct(product, amount, triggerEvents, null);
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
        
        #region Economic Configuration
        
        /// <summary>
        /// Enable or disable economic constraints for inventory operations
        /// </summary>
        /// <param name="enabled">Whether to enable economic constraints</param>
        public void SetEconomicConstraints(bool enabled)
        {
            enableEconomicConstraints = enabled;
            Debug.Log($"Economic constraints {(enabled ? "enabled" : "disabled")} for InventoryManager");
        }
        
        /// <summary>
        /// Set the restock cost multiplier (percentage of retail price)
        /// </summary>
        /// <param name="multiplier">Multiplier value (e.g., 0.7 for 70% of retail price)</param>
        public void SetRestockCostMultiplier(float multiplier)
        {
            if (multiplier < 0)
            {
                Debug.LogWarning("Restock cost multiplier cannot be negative. Setting to 0.");
                multiplier = 0;
            }
            
            float oldMultiplier = restockCostMultiplier;
            restockCostMultiplier = multiplier;
            
            Debug.Log($"Restock cost multiplier changed: {oldMultiplier:F2} → {restockCostMultiplier:F2}");
        }
        
        /// <summary>
        /// Enable or disable economic transaction logging
        /// </summary>
        /// <param name="enabled">Whether to enable economic logging</param>
        public void SetEconomicLogging(bool enabled)
        {
            logEconomicTransactions = enabled;
            
            // Update validator logging if it supports it
            if (economicValidator is GameManagerEconomicValidator gameManagerValidator)
            {
                gameManagerValidator.SetLogging(enabled);
            }
            
            Debug.Log($"Economic transaction logging {(enabled ? "enabled" : "disabled")}");
        }
        
        /// <summary>
        /// Set the economic validator (useful for testing or different implementations)
        /// </summary>
        /// <param name="validator">The economic validator to use</param>
        public void SetEconomicValidator(IEconomicValidator validator)
        {
            if (validator == null)
            {
                Debug.LogWarning("Cannot set null economic validator. Using default GameManagerEconomicValidator.");
                economicValidator = new GameManagerEconomicValidator(logEconomicTransactions);
                return;
            }
            
            economicValidator = validator;
            Debug.Log($"Economic validator set to: {validator.GetType().Name}");
        }
        
        /// <summary>
        /// Set the product repository (useful for testing or different implementations)
        /// </summary>
        /// <param name="repository">The product repository to use</param>
        public void SetProductRepository(IProductRepository repository)
        {
            if (repository == null)
            {
                Debug.LogWarning("Cannot set null product repository. Using default ResourcesProductRepository.");
                productRepository = new ResourcesProductRepository("Products", logEconomicTransactions);
                return;
            }
            
            productRepository = repository;
            Debug.Log($"Product repository set to: {repository.GetType().Name}");
        }
        
        /// <summary>
        /// Set the event publisher (useful for testing or different implementations)
        /// </summary>
        /// <param name="publisher">The event publisher to use</param>
        public void SetEventPublisher(IInventoryEventPublisher publisher)
        {
            if (publisher == null)
            {
                Debug.LogWarning("Cannot set null event publisher. Creating default UnityInventoryEventPublisher.");
                var eventPublisherObject = new GameObject("InventoryEventPublisher");
                eventPublisherObject.transform.SetParent(transform);
                eventPublisher = eventPublisherObject.AddComponent<UnityInventoryEventPublisher>();
                return;
            }
            
            eventPublisher = publisher;
            Debug.Log($"Event publisher set to: {publisher.GetType().Name}");
        }
        
        /// <summary>
        /// Get current event publisher configuration as formatted string
        /// </summary>
        /// <returns>String representation of event publisher settings</returns>
        public string GetEventPublisherConfiguration()
        {
            if (eventPublisher == null)
            {
                return "Event Publisher Configuration: Not initialized";
            }
            
            string publisherType = eventPublisher.GetType().Name;
            bool isUnityPublisher = eventPublisher is UnityInventoryEventPublisher;
            
            return $"Event Publisher Configuration:\n" +
                   $"- Publisher Type: {publisherType}\n" +
                   $"- Unity Events Available: {isUnityPublisher}\n" +
                   $"- Publisher Available: {eventPublisher != null}";
        }
        
        /// <summary>
        /// Get current economic configuration as formatted string
        /// </summary>
        /// <returns>String representation of economic settings</returns>
        public string GetEconomicConfiguration()
        {
            return $"Economic Configuration:\n" +
                   $"- Constraints Enabled: {enableEconomicConstraints}\n" +
                   $"- Restock Cost Multiplier: {restockCostMultiplier:F2} ({restockCostMultiplier * 100:F0}% of retail)\n" +
                   $"- Transaction Logging: {logEconomicTransactions}\n" +
                   $"- Economic Validator Available: {economicValidator?.IsAvailable() ?? false}";
        }
        
        #endregion
        
        #region Core Validation
        
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
        
        #endregion
    }
}
