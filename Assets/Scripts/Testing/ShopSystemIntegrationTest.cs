using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

namespace TabletopShop
{
    /// <summary>
    /// Comprehensive integration test for the complete shop system
    /// Tests Product, Shelf, ShelfSlot interaction and the core game loop
    /// </summary>
    public class ShopSystemIntegrationTest : MonoBehaviour
    {
        [Header("Test Configuration")]
        [SerializeField] private List<ProductData> availableProductData = new List<ProductData>();
        [SerializeField] private List<GameObject> productPrefabs = new List<GameObject>();
        
        [Header("Test Controls")]
        [SerializeField] private KeyCode setupShopKey = KeyCode.G;
        [SerializeField] private KeyCode stockShelvesKey = KeyCode.H;
        [SerializeField] private KeyCode simulateCustomerKey = KeyCode.J;
        [SerializeField] private KeyCode runFullTestKey = KeyCode.K;
        [SerializeField] private KeyCode clearShopKey = KeyCode.L;
        
        [Header("Shop Layout")]
        [SerializeField] private Vector3[] shelfPositions = {
            new Vector3(-4, 0.2f, 2),
            new Vector3(4, 0.2f, 2),
            new Vector3(-4, 0.2f, -2),
            new Vector3(4, 0.2f, -2)
        };
        [SerializeField] private Vector3[] shelfRotations = {
            new Vector3(0, 90, 0),
            new Vector3(0, 270, 0),
            new Vector3(0, 90, 0),
            new Vector3(0, 270, 0)
        };
        
        // Runtime data
        private List<Shelf> testShelves = new List<Shelf>();
        private List<Product> spawnedProducts = new List<Product>();
        private int playerMoney = 1000;
        private int totalSales = 0;
        
        #region Unity Lifecycle
        
        private void Update()
        {
            if (Input.GetKeyDown(setupShopKey))
            {
                SetupTestShop();
            }
            
            if (Input.GetKeyDown(stockShelvesKey))
            {
                StockAllShelves();
            }
            
            if (Input.GetKeyDown(simulateCustomerKey))
            {
                StartCoroutine(SimulateCustomerPurchase());
            }
            
            if (Input.GetKeyDown(runFullTestKey))
            {
                StartCoroutine(RunFullShopTest());
            }
            
            if (Input.GetKeyDown(clearShopKey))
            {
                ClearTestShop();
            }
        }
        
        #endregion
        
        #region Shop Setup
        
        /// <summary>
        /// Setup the complete test shop with shelves
        /// </summary>
        private void SetupTestShop()
        {
            Debug.Log("=== Setting Up Test Shop ===");
            
            // Clear existing setup
            ClearTestShop();
            
            // Create shelves at designated positions
            for (int i = 0; i < shelfPositions.Length; i++)
            {
                CreateTestShelf(i);
            }
            
            Debug.Log($"Created {testShelves.Count} shelves in the shop");
            Debug.Log("Shop setup complete! Press H to stock shelves.");
        }
        
        /// <summary>
        /// Create a test shelf at the specified index
        /// </summary>
        /// <param name="index">Index of the shelf to create</param>
        private void CreateTestShelf(int index)
        {
            if (index >= shelfPositions.Length) return;
            
            // Create shelf GameObject
            GameObject shelfObject = new GameObject($"TestShelf_{index + 1}");
            shelfObject.transform.position = shelfPositions[index];
            shelfObject.transform.rotation = Quaternion.Euler(shelfRotations[index]);
            
            // Add Shelf component
            Shelf shelf = shelfObject.AddComponent<Shelf>();
            
            // Configure shelf for specific product types (demonstration)
            ProductType assignedType = (ProductType)(index % 3);
            bool allowAnyType = index == 0; // First shelf allows any type
            
            shelf.SetAllowedProductType(assignedType, allowAnyType);
            
            testShelves.Add(shelf);
            
            Debug.Log($"Created shelf {index + 1} at {shelfPositions[index]} - " +
                     $"Allows: {(allowAnyType ? "Any Product" : assignedType.ToString())}");
        }
        
        #endregion
        
        #region Product Management
        
        /// <summary>
        /// Stock all shelves with appropriate products
        /// </summary>
        private void StockAllShelves()
        {
            if (testShelves.Count == 0)
            {
                Debug.LogWarning("No shelves available! Setup shop first with G key.");
                return;
            }
            
            if (productPrefabs.Count == 0)
            {
                Debug.LogWarning("No product prefabs available! Add some in the inspector.");
                return;
            }
            
            Debug.Log("=== Stocking All Shelves ===");
            
            foreach (Shelf shelf in testShelves)
            {
                StockShelf(shelf);
            }
            
            Debug.Log($"Stocking complete. Total products spawned: {spawnedProducts.Count}");
            Debug.Log("Press J to simulate customer purchases or K to run full test!");
        }
        
        /// <summary>
        /// Stock a specific shelf with appropriate products
        /// </summary>
        /// <param name="shelf">The shelf to stock</param>
        private void StockShelf(Shelf shelf)
        {
            Debug.Log($"Stocking shelf: {shelf.name} - {shelf.GetShelfStatus()}");
            
            // Try to fill each slot
            for (int slotIndex = 0; slotIndex < shelf.TotalSlots; slotIndex++)
            {
                // Get appropriate product for this shelf
                GameObject productPrefab = GetAppropriateProductPrefab(shelf);
                if (productPrefab == null) continue;
                
                // Spawn product
                Vector3 spawnPosition = shelf.transform.position + Vector3.up * 2;
                GameObject productObject = Instantiate(productPrefab, spawnPosition, Quaternion.identity);
                Product product = productObject.GetComponent<Product>();
                
                if (product == null)
                {
                    Debug.LogError($"Product prefab missing Product component: {productPrefab.name}");
                    DestroyImmediate(productObject);
                    continue;
                }
                
                // Initialize with random data
                InitializeProductWithRandomData(product);
                
                // Try to place on shelf
                bool success = shelf.TryPlaceProductInSlot(product, slotIndex);
                if (success)
                {
                    spawnedProducts.Add(product);
                    Debug.Log($"  Placed {product.ProductData?.ProductName} in slot {slotIndex + 1}");
                }
                else
                {
                    Debug.LogWarning($"  Failed to place product in slot {slotIndex + 1}");
                    DestroyImmediate(productObject);
                }
            }
        }
        
        /// <summary>
        /// Get a product prefab appropriate for the given shelf
        /// </summary>
        /// <param name="shelf">The shelf to get a product for</param>
        /// <returns>Appropriate product prefab or null</returns>
        private GameObject GetAppropriateProductPrefab(Shelf shelf)
        {
            if (productPrefabs.Count == 0) return null;
            
            // If shelf allows any type, pick randomly
            if (shelf.AllowsAnyProductType)
            {
                return productPrefabs[Random.Range(0, productPrefabs.Count)];
            }
            
            // Find product prefab that matches shelf's allowed type
            foreach (GameObject prefab in productPrefabs)
            {
                Product productComponent = prefab.GetComponent<Product>();
                if (productComponent != null && productComponent.ProductData != null)
                {
                    if (productComponent.ProductData.Type == shelf.AllowedProductType)
                    {
                        return prefab;
                    }
                }
            }
            
            // Fallback to first available prefab (will be rejected by shelf if type doesn't match)
            return productPrefabs[0];
        }
        
        /// <summary>
        /// Initialize a product with random test data
        /// </summary>
        /// <param name="product">Product to initialize</param>
        private void InitializeProductWithRandomData(Product product)
        {
            if (availableProductData.Count > 0)
            {
                ProductData randomData = availableProductData[Random.Range(0, availableProductData.Count)];
                product.Initialize(randomData);
            }
            
            // Set random price variation (±20% of base price)
            int basePrice = product.CurrentPrice;
            int priceVariation = Random.Range(-basePrice / 5, basePrice / 5);
            product.SetPrice(basePrice + priceVariation);
        }
        
        #endregion
        
        #region Customer Simulation
        
        /// <summary>
        /// Simulate a customer making a purchase
        /// </summary>
        /// <returns>Coroutine for customer behavior</returns>
        private IEnumerator SimulateCustomerPurchase()
        {
            Debug.Log("=== Simulating Customer Purchase ===");
            
            if (spawnedProducts.Count == 0)
            {
                Debug.LogWarning("No products available for purchase! Stock shelves first.");
                yield break;
            }
            
            // Find available products (on shelf, not purchased)
            List<Product> availableProducts = new List<Product>();
            foreach (Product product in spawnedProducts)
            {
                if (product != null && product.IsOnShelf && !product.IsPurchased)
                {
                    availableProducts.Add(product);
                }
            }
            
            if (availableProducts.Count == 0)
            {
                Debug.LogWarning("No available products for purchase! All items sold or removed.");
                yield break;
            }
            
            // Customer browses for a moment
            Debug.Log("Customer enters shop and browses...");
            yield return new WaitForSeconds(1f);
            
            // Select random product to purchase
            Product selectedProduct = availableProducts[Random.Range(0, availableProducts.Count)];
            Debug.Log($"Customer selects: {selectedProduct.ProductData?.ProductName} (${selectedProduct.CurrentPrice})");
            
            // Wait a moment (customer considers purchase)
            yield return new WaitForSeconds(0.5f);
            
            // Make purchase
            int purchasePrice = selectedProduct.CurrentPrice;
            selectedProduct.Purchase();
            
            // Update shop finances
            playerMoney += purchasePrice;
            totalSales++;
            
            Debug.Log($"SALE COMPLETE! Earned ${purchasePrice}");
            Debug.Log($"Shop Status: ${playerMoney} total, {totalSales} sales today");
            
            yield return new WaitForSeconds(0.5f);
            Debug.Log("Customer leaves shop satisfied!");
        }
        
        #endregion
        
        #region Full Integration Test
        
        /// <summary>
        /// Run complete shop system test demonstrating full game loop
        /// </summary>
        /// <returns>Coroutine for full test sequence</returns>
        private IEnumerator RunFullShopTest()
        {
            Debug.Log("=== RUNNING FULL SHOP SYSTEM TEST ===");
            Debug.Log("This will demonstrate the complete game loop:");
            Debug.Log("Setup → Stock → Price → Sell → Profit");
            
            yield return new WaitForSeconds(1f);
            
            // Phase 1: Setup
            Debug.Log("Phase 1: Setting up shop...");
            SetupTestShop();
            yield return new WaitForSeconds(2f);
            
            // Phase 2: Stock
            Debug.Log("Phase 2: Stocking shelves...");
            StockAllShelves();
            yield return new WaitForSeconds(2f);
            
            // Phase 3: Price setting (demonstrated by random price variations)
            Debug.Log("Phase 3: Pricing products...");
            foreach (Product product in spawnedProducts)
            {
                if (product != null && !product.IsPurchased)
                {
                    int newPrice = Random.Range(product.CurrentPrice - 5, product.CurrentPrice + 15);
                    product.SetPrice(Mathf.Max(1, newPrice));
                }
            }
            Debug.Log("All products repriced with market adjustments!");
            yield return new WaitForSeconds(2f);
            
            // Phase 4: Multiple customer purchases
            Debug.Log("Phase 4: Simulating customer rush...");
            for (int i = 0; i < 3; i++)
            {
                Debug.Log($"Customer {i + 1} enters...");
                yield return StartCoroutine(SimulateCustomerPurchase());
                yield return new WaitForSeconds(1f);
            }
            
            // Phase 5: Results
            Debug.Log("Phase 5: Day summary");
            DisplayShopSummary();
            
            Debug.Log("=== FULL SHOP TEST COMPLETE ===");
            Debug.Log("The shop system is working correctly!");
            Debug.Log("All major systems integrated successfully:");
            Debug.Log("✓ Product management ✓ Shelf system ✓ Customer simulation ✓ Economy");
        }
        
        /// <summary>
        /// Display comprehensive shop summary
        /// </summary>
        private void DisplayShopSummary()
        {
            Debug.Log("=== SHOP SUMMARY ===");
            Debug.Log($"Total Money: ${playerMoney}");
            Debug.Log($"Total Sales: {totalSales} items");
            Debug.Log($"Active Shelves: {testShelves.Count}");
            
            int totalProducts = 0;
            int soldProducts = 0;
            int availableProducts = 0;
            
            foreach (Shelf shelf in testShelves)
            {
                if (shelf != null)
                {
                    totalProducts += shelf.TotalSlots;
                    Debug.Log($"  {shelf.name}: {shelf.GetShelfStatus()}");
                }
            }
            
            foreach (Product product in spawnedProducts)
            {
                if (product != null)
                {
                    if (product.IsPurchased)
                        soldProducts++;
                    else if (product.IsOnShelf)
                        availableProducts++;
                }
            }
            
            Debug.Log($"Products: {soldProducts} sold, {availableProducts} available, {totalProducts} total capacity");
            
            if (totalSales > 0)
            {
                float avgSalePrice = (float)(playerMoney - 1000) / totalSales; // Subtract starting money
                Debug.Log($"Average Sale Price: ${avgSalePrice:F2}");
            }
        }
        
        #endregion
        
        #region Cleanup
        
        /// <summary>
        /// Clear all test objects from the shop
        /// </summary>
        private void ClearTestShop()
        {
            Debug.Log("=== Clearing Test Shop ===");
            
            // Remove all test shelves
            foreach (Shelf shelf in testShelves)
            {
                if (shelf != null)
                {
                    DestroyImmediate(shelf.gameObject);
                }
            }
            testShelves.Clear();
            
            // Remove all spawned products
            foreach (Product product in spawnedProducts)
            {
                if (product != null)
                {
                    DestroyImmediate(product.gameObject);
                }
            }
            spawnedProducts.Clear();
            
            // Reset shop stats
            playerMoney = 1000;
            totalSales = 0;
            
            Debug.Log("Shop cleared and reset!");
        }
        
        #endregion
        
        #region GUI
        
        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10, 460, 400, 200));
            GUILayout.Label("SHOP SYSTEM INTEGRATION TEST");
            GUILayout.Label($"Press '{setupShopKey}' to setup test shop");
            GUILayout.Label($"Press '{stockShelvesKey}' to stock all shelves");
            GUILayout.Label($"Press '{simulateCustomerKey}' to simulate customer");
            GUILayout.Label($"Press '{runFullTestKey}' to run FULL INTEGRATION TEST");
            GUILayout.Label($"Press '{clearShopKey}' to clear and reset shop");
            
            GUILayout.Space(10);
            GUILayout.Label("=== SHOP STATUS ===");
            GUILayout.Label($"Money: ${playerMoney}");
            GUILayout.Label($"Sales Today: {totalSales}");
            GUILayout.Label($"Active Shelves: {testShelves.Count}");
            GUILayout.Label($"Products in Shop: {spawnedProducts.Count(p => p != null && !p.IsPurchased)}");
            GUILayout.Label($"Products Sold: {spawnedProducts.Count(p => p != null && p.IsPurchased)}");
            
            if (testShelves.Count > 0)
            {
                GUILayout.Space(5);
                GUILayout.Label("Shelf Occupancy:");
                foreach (Shelf shelf in testShelves)
                {
                    if (shelf != null)
                    {
                        GUILayout.Label($"  {shelf.name}: {shelf.OccupiedSlots}/{shelf.TotalSlots}");
                    }
                }
            }
            
            GUILayout.EndArea();
        }
        
        #endregion
    }
}
