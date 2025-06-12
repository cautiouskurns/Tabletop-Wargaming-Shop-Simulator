using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace TabletopShop
{
    /// <summary>
    /// Helper script to automatically populate shelves with products for testing and development.
    /// Simulates the same interactions as if a player manually placed products on shelves.
    /// </summary>
    public class ShelfAutoPopulator : MonoBehaviour
    {
        [Header("Auto-Population Settings")]
        [SerializeField] private bool autoPopulateOnStart = true;
        [SerializeField] private bool populateEmptyShelvesOnly = true;
        [SerializeField] private float delayBetweenPlacements = 0.1f;
        
        [Header("Product Selection")]
        [SerializeField] private PopulationMode populationMode = PopulationMode.RandomFromInventory;
        [SerializeField] private List<ProductData> specificProducts = new List<ProductData>();
        [SerializeField] private bool respectProductTypes = true;
        
        [Header("Shelf Filtering")]
        [SerializeField] private bool populateAllShelves = true;
        [SerializeField] private List<ShelfSlot> specificShelves = new List<ShelfSlot>();
        [SerializeField] private string shelfTagFilter = ""; // Optional tag filter
        
        [Header("Debug Options")]
        [SerializeField] private bool enableDebugLogging = true;
        [SerializeField] private bool showProgressInConsole = true;
        
        public enum PopulationMode
        {
            RandomFromInventory,    // Use random products from inventory
            SpecificProducts,       // Use only products from specificProducts list
            OneOfEachType,         // Place one product of each type
            RoundRobin             // Cycle through available products
        }
        
        private List<ShelfSlot> targetShelves = new List<ShelfSlot>();
        private List<ProductData> availableProducts = new List<ProductData>();
        private int currentProductIndex = 0;
        
        #region Unity Lifecycle
        
        private void Start()
        {
            if (autoPopulateOnStart)
            {
                // Delay to ensure all systems are initialized
                Invoke(nameof(StartAutoPopulation), 1f);
            }
        }
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// Start the auto-population process
        /// </summary>
        [ContextMenu("Start Auto Population")]
        public void StartAutoPopulation()
        {
            if (enableDebugLogging)
            {
                Debug.Log("ShelfAutoPopulator: Starting auto-population process...");
            }
            
            if (!ValidateRequirements())
            {
                return;
            }
            
            StartCoroutine(PopulateShelvesCoroutine());
        }
        
        /// <summary>
        /// Clear all products from shelves
        /// </summary>
        [ContextMenu("Clear All Shelves")]
        public void ClearAllShelves()
        {
            FindTargetShelves();
            
            int clearedCount = 0;
            foreach (ShelfSlot shelf in targetShelves)
            {
                if (!shelf.IsEmpty)
                {
                    // Simulate removing the product (triggers same events as manual removal)
                    if (shelf.CurrentProduct != null)
                    {
                        // Return product to inventory
                        InventoryManager.Instance.AddProduct(shelf.CurrentProduct.ProductData, 1);
                    }
                    
                    shelf.RemoveProduct();
                    clearedCount++;
                }
            }
            
            if (enableDebugLogging)
            {
                Debug.Log($"ShelfAutoPopulator: Cleared {clearedCount} shelves");
            }
        }
        
        /// <summary>
        /// Populate only empty shelves
        /// </summary>
        [ContextMenu("Populate Empty Shelves Only")]
        public void PopulateEmptyShelves()
        {
            populateEmptyShelvesOnly = true;
            StartAutoPopulation();
        }
        
        /// <summary>
        /// Force populate all shelves (replace existing products)
        /// </summary>
        [ContextMenu("Force Populate All Shelves")]
        public void ForcePopulateAllShelves()
        {
            populateEmptyShelvesOnly = false;
            StartAutoPopulation();
        }
        
        /// <summary>
        /// Get population statistics
        /// </summary>
        public PopulationStats GetPopulationStats()
        {
            FindTargetShelves();
            
            int totalShelves = targetShelves.Count;
            int populatedShelves = targetShelves.Count(s => !s.IsEmpty);
            int emptyShelves = totalShelves - populatedShelves;
            
            return new PopulationStats
            {
                TotalShelves = totalShelves,
                PopulatedShelves = populatedShelves,
                EmptyShelves = emptyShelves,
                PopulationPercentage = totalShelves > 0 ? (float)populatedShelves / totalShelves * 100f : 0f
            };
        }
        
        #endregion
        
        #region Private Methods
        
        /// <summary>
        /// Validate that all required components and settings are available
        /// </summary>
        private bool ValidateRequirements()
        {
            // Check InventoryManager
            if (InventoryManager.Instance == null)
            {
                Debug.LogError("ShelfAutoPopulator: InventoryManager not found! Cannot populate shelves.");
                return false;
            }
            
            // Find target shelves
            FindTargetShelves();
            if (targetShelves.Count == 0)
            {
                Debug.LogWarning("ShelfAutoPopulator: No target shelves found. Nothing to populate.");
                return false;
            }
            
            // Get available products
            GetAvailableProducts();
            if (availableProducts.Count == 0)
            {
                Debug.LogWarning("ShelfAutoPopulator: No available products found. Cannot populate shelves.");
                return false;
            }
            
            if (enableDebugLogging)
            {
                Debug.Log($"ShelfAutoPopulator: Validation passed. Found {targetShelves.Count} shelves and {availableProducts.Count} products.");
            }
            
            return true;
        }
        
        /// <summary>
        /// Find all shelves that should be populated
        /// </summary>
        private void FindTargetShelves()
        {
            targetShelves.Clear();
            
            if (populateAllShelves)
            {
                // Find all ShelfSlot components in the scene
                ShelfSlot[] allShelves = FindObjectsByType<ShelfSlot>(FindObjectsSortMode.None);
                
                foreach (ShelfSlot shelf in allShelves)
                {
                    if (ShouldIncludeShelf(shelf))
                    {
                        targetShelves.Add(shelf);
                    }
                }
            }
            else
            {
                // Use specific shelves list
                foreach (ShelfSlot shelf in specificShelves)
                {
                    if (shelf != null && ShouldIncludeShelf(shelf))
                    {
                        targetShelves.Add(shelf);
                    }
                }
            }
            
            if (enableDebugLogging)
            {
                Debug.Log($"ShelfAutoPopulator: Found {targetShelves.Count} target shelves");
            }
        }
        
        /// <summary>
        /// Check if a shelf should be included in population
        /// </summary>
        private bool ShouldIncludeShelf(ShelfSlot shelf)
        {
            if (shelf == null) return false;
            
            // Check if shelf is empty (if we only want to populate empty shelves)
            if (populateEmptyShelvesOnly && !shelf.IsEmpty)
            {
                return false;
            }
            
            // Check tag filter
            if (!string.IsNullOrEmpty(shelfTagFilter) && !shelf.CompareTag(shelfTagFilter))
            {
                return false;
            }
            
            return true;
        }
        
        /// <summary>
        /// Get available products based on population mode
        /// </summary>
        private void GetAvailableProducts()
        {
            availableProducts.Clear();
            
            switch (populationMode)
            {
                case PopulationMode.RandomFromInventory:
                    availableProducts = InventoryManager.Instance.GetAvailableProductsWithQuantity();
                    break;
                    
                case PopulationMode.SpecificProducts:
                    availableProducts = specificProducts.Where(p => p != null && InventoryManager.Instance.HasProduct(p)).ToList();
                    break;
                    
                case PopulationMode.OneOfEachType:
                    var productsByType = InventoryManager.Instance.GetProductsByType();
                    foreach (var typeGroup in productsByType)
                    {
                        var firstProduct = typeGroup.Value.FirstOrDefault(p => InventoryManager.Instance.HasProduct(p));
                        if (firstProduct != null)
                        {
                            availableProducts.Add(firstProduct);
                        }
                    }
                    break;
                    
                case PopulationMode.RoundRobin:
                    availableProducts = InventoryManager.Instance.GetAvailableProductsWithQuantity();
                    break;
            }
            
            if (enableDebugLogging)
            {
                Debug.Log($"ShelfAutoPopulator: Available products for mode {populationMode}: {availableProducts.Count}");
            }
        }
        
        /// <summary>
        /// Coroutine to populate shelves with delays
        /// </summary>
        private System.Collections.IEnumerator PopulateShelvesCoroutine()
        {
            int populatedCount = 0;
            int totalToPopulate = targetShelves.Count;
            
            if (showProgressInConsole)
            {
                Debug.Log($"ShelfAutoPopulator: Starting population of {totalToPopulate} shelves...");
            }
            
            foreach (ShelfSlot shelf in targetShelves)
            {
                if (PopulateShelf(shelf))
                {
                    populatedCount++;
                    
                    if (showProgressInConsole && populatedCount % 10 == 0)
                    {
                        Debug.Log($"ShelfAutoPopulator: Populated {populatedCount}/{totalToPopulate} shelves...");
                    }
                }
                
                // Delay between placements to avoid overwhelming the system
                if (delayBetweenPlacements > 0)
                {
                    yield return new WaitForSeconds(delayBetweenPlacements);
                }
            }
            
            if (enableDebugLogging)
            {
                Debug.Log($"ShelfAutoPopulator: Population complete! Populated {populatedCount} out of {totalToPopulate} target shelves.");
                
                var stats = GetPopulationStats();
                Debug.Log($"ShelfAutoPopulator: Final stats - {stats.PopulatedShelves}/{stats.TotalShelves} shelves populated ({stats.PopulationPercentage:F1}%)");
            }
        }
        
        /// <summary>
        /// Populate a single shelf with an appropriate product
        /// </summary>
        private bool PopulateShelf(ShelfSlot shelf)
        {
            if (shelf == null)
            {
                return false;
            }
            
            // Skip if shelf is not empty and we only want to populate empty shelves
            if (populateEmptyShelvesOnly && !shelf.IsEmpty)
            {
                return false;
            }
            
            // Get product to place
            ProductData productToPlace = GetProductForShelf(shelf);
            if (productToPlace == null)
            {
                if (enableDebugLogging)
                {
                    Debug.LogWarning($"ShelfAutoPopulator: No suitable product found for shelf {shelf.name}");
                }
                return false;
            }
            
            // Check if we have the product in inventory
            if (!InventoryManager.Instance.HasProduct(productToPlace))
            {
                if (enableDebugLogging)
                {
                    Debug.LogWarning($"ShelfAutoPopulator: Product {productToPlace.ProductName} not available in inventory");
                }
                return false;
            }
            
            // Remove existing product if shelf is not empty
            if (!shelf.IsEmpty)
            {
                if (shelf.CurrentProduct != null)
                {
                    // Return current product to inventory
                    InventoryManager.Instance.AddProduct(shelf.CurrentProduct.ProductData, 1);
                }
                shelf.RemoveProduct();
            }
            
            // Create product instance (simulating player placement)
            GameObject productPrefab = GetProductPrefab(productToPlace);
            if (productPrefab == null)
            {
                if (enableDebugLogging)
                {
                    Debug.LogError($"ShelfAutoPopulator: No prefab found for product {productToPlace.ProductName}");
                }
                return false;
            }
            
            // Instantiate product at shelf position
            GameObject productInstance = Instantiate(productPrefab, shelf.transform.position, shelf.transform.rotation);
            Product productComponent = productInstance.GetComponent<Product>();
            
            if (productComponent == null)
            {
                Debug.LogError($"ShelfAutoPopulator: Product prefab {productPrefab.name} does not have Product component!");
                Destroy(productInstance);
                return false;
            }
            
            // Initialize the product
            productComponent.Initialize(productToPlace);
            
            // Place product on shelf (simulating player interaction)
            bool placementSuccess = shelf.PlaceProduct(productComponent);
            
            if (placementSuccess)
            {
                // Remove product from inventory (simulating player usage)
                InventoryManager.Instance.RemoveProduct(productToPlace, 1);
                
                if (enableDebugLogging)
                {
                    Debug.Log($"ShelfAutoPopulator: Successfully placed {productToPlace.ProductName} on shelf {shelf.name}");
                }
                return true;
            }
            else
            {
                // Placement failed, clean up
                Destroy(productInstance);
                if (enableDebugLogging)
                {
                    Debug.LogWarning($"ShelfAutoPopulator: Failed to place {productToPlace.ProductName} on shelf {shelf.name}");
                }
                return false;
            }
        }
        
        /// <summary>
        /// Get the appropriate product for a specific shelf
        /// </summary>
        private ProductData GetProductForShelf(ShelfSlot shelf)
        {
            if (availableProducts.Count == 0)
            {
                return null;
            }
            
            ProductData selectedProduct = null;
            
            switch (populationMode)
            {
                case PopulationMode.RandomFromInventory:
                    selectedProduct = availableProducts[Random.Range(0, availableProducts.Count)];
                    break;
                    
                case PopulationMode.SpecificProducts:
                    selectedProduct = availableProducts[Random.Range(0, availableProducts.Count)];
                    break;
                    
                case PopulationMode.OneOfEachType:
                    selectedProduct = availableProducts[Random.Range(0, availableProducts.Count)];
                    break;
                    
                case PopulationMode.RoundRobin:
                    selectedProduct = availableProducts[currentProductIndex % availableProducts.Count];
                    currentProductIndex++;
                    break;
            }
            
            // If respecting product types, try to match shelf type preferences
            if (respectProductTypes && selectedProduct != null)
            {
                // You could add logic here to match product types to shelf preferences
                // For now, we'll use the selected product as-is
            }
            
            return selectedProduct;
        }
        
        /// <summary>
        /// Get the prefab for a product (you'll need to implement this based on your project structure)
        /// </summary>
        private GameObject GetProductPrefab(ProductData productData)
        {
            // Option 1: If ProductData has a prefab reference
            if (productData.Prefab != null)
            {
                return productData.Prefab;
            }
            
            // Option 2: Load from Resources folder
            string prefabPath = $"ProductPrefabs/{productData.ProductName}";
            GameObject prefab = Resources.Load<GameObject>(prefabPath);
            if (prefab != null)
            {
                return prefab;
            }
            
            // Option 3: Search for prefab by name
            prefabPath = $"Prefabs/Products/{productData.ProductName}";
            prefab = Resources.Load<GameObject>(prefabPath);
            if (prefab != null)
            {
                return prefab;
            }
            
            // Option 4: Use a generic product prefab
            GameObject genericPrefab = Resources.Load<GameObject>("Prefabs/GenericProduct");
            if (genericPrefab != null)
            {
                Debug.LogWarning($"ShelfAutoPopulator: Using generic prefab for {productData.ProductName}");
                return genericPrefab;
            }
            
            Debug.LogError($"ShelfAutoPopulator: Could not find prefab for product {productData.ProductName}");
            return null;
        }
        
        #endregion
        
        #region Helper Classes
        
        [System.Serializable]
        public class PopulationStats
        {
            public int TotalShelves;
            public int PopulatedShelves;
            public int EmptyShelves;
            public float PopulationPercentage;
            
            public override string ToString()
            {
                return $"Shelves: {PopulatedShelves}/{TotalShelves} ({PopulationPercentage:F1}% populated)";
            }
        }
        
        #endregion
        
        #region Editor Debug Methods
        
        [ContextMenu("Debug: Show Population Stats")]
        private void DebugShowStats()
        {
            var stats = GetPopulationStats();
            Debug.Log($"ShelfAutoPopulator Stats: {stats}");
        }
        
        [ContextMenu("Debug: List Target Shelves")]
        private void DebugListTargetShelves()
        {
            FindTargetShelves();
            Debug.Log($"ShelfAutoPopulator: Found {targetShelves.Count} target shelves:");
            for (int i = 0; i < targetShelves.Count; i++)
            {
                string status = targetShelves[i].IsEmpty ? "Empty" : $"Contains {targetShelves[i].CurrentProduct?.ProductData?.ProductName}";
                Debug.Log($"  {i + 1}. {targetShelves[i].name} - {status}");
            }
        }
        
        [ContextMenu("Debug: List Available Products")]
        private void DebugListAvailableProducts()
        {
            GetAvailableProducts();
            Debug.Log($"ShelfAutoPopulator: Found {availableProducts.Count} available products:");
            for (int i = 0; i < availableProducts.Count; i++)
            {
                int quantity = InventoryManager.Instance.GetProductCount(availableProducts[i]);
                Debug.Log($"  {i + 1}. {availableProducts[i].ProductName} (Quantity: {quantity})");
            }
        }
        
        #endregion
    }
}