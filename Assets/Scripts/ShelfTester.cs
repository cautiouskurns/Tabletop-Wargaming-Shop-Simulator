using UnityEngine;
using System.Collections.Generic;

namespace TabletopShop
{
    /// <summary>
    /// Testing script for validating Shelf and ShelfSlot functionality
    /// Attach to any GameObject in the scene to run shelf tests
    /// </summary>
    public class ShelfTester : MonoBehaviour
    {
        [Header("Test Configuration")]
        [SerializeField] private Shelf testShelf;
        [SerializeField] private List<ProductData> testProductDataList = new List<ProductData>();
        [SerializeField] private List<GameObject> productPrefabs = new List<GameObject>();
        
        [Header("Test Controls")]
        [SerializeField] private KeyCode createShelfKey = KeyCode.Y;
        [SerializeField] private KeyCode stockShelfKey = KeyCode.U;
        [SerializeField] private KeyCode clearShelfKey = KeyCode.I;
        [SerializeField] private KeyCode runShelfTestsKey = KeyCode.O;
        [SerializeField] private KeyCode debugShelfKey = KeyCode.P;
        
        [Header("Test Settings")]
        [SerializeField] private Vector3 shelfSpawnPosition = new Vector3(5, 0, 0);
        [SerializeField] private bool autoFindShelf = true;
        
        private List<Product> spawnedTestProducts = new List<Product>();
        
        #region Unity Lifecycle
        
        private void Start()
        {
            if (autoFindShelf && testShelf == null)
            {
                testShelf = FindFirstObjectByType<Shelf>();
                if (testShelf != null)
                {
                    Debug.Log($"Auto-found shelf: {testShelf.name}");
                }
            }
        }
        
        private void Update()
        {
            if (Input.GetKeyDown(createShelfKey))
            {
                CreateTestShelf();
            }
            
            if (Input.GetKeyDown(stockShelfKey))
            {
                StockShelfWithTestProducts();
            }
            
            if (Input.GetKeyDown(clearShelfKey))
            {
                ClearTestShelf();
            }
            
            if (Input.GetKeyDown(runShelfTestsKey))
            {
                RunComprehensiveShelfTests();
            }
            
            if (Input.GetKeyDown(debugShelfKey))
            {
                DebugShelfSetup();
            }
        }
        
        #endregion
        
        #region Test Methods
        
        /// <summary>
        /// Create a test shelf dynamically
        /// </summary>
        private void CreateTestShelf()
        {
            if (testShelf != null)
            {
                Debug.LogWarning("Test shelf already exists! Clear it first or use the existing one.");
                return;
            }
            
            // Create shelf GameObject
            GameObject shelfObject = new GameObject("TestShelf");
            shelfObject.transform.position = shelfSpawnPosition;
            
            // Add Shelf component
            testShelf = shelfObject.AddComponent<Shelf>();
            
            Debug.Log($"Created test shelf at {shelfSpawnPosition}");
            Debug.Log($"Shelf status: {testShelf.GetShelfStatus()}");
        }
        
        /// <summary>
        /// Stock the shelf with test products
        /// </summary>
        private void StockShelfWithTestProducts()
        {
            if (testShelf == null)
            {
                Debug.LogWarning("No test shelf available! Create one first with Y key.");
                return;
            }
            
            if (productPrefabs.Count == 0)
            {
                Debug.LogWarning("No product prefabs assigned! Assign some in the inspector to test stocking.");
                return;
            }
            
            Debug.Log("=== Stocking Shelf with Test Products ===");
            
            // Debug shelf setup first
            Debug.Log($"Shelf debug info:");
            Debug.Log($"  Total Slots: {testShelf.TotalSlots}");
            Debug.Log($"  Occupied Slots: {testShelf.OccupiedSlots}");
            Debug.Log($"  Empty Slots: {testShelf.EmptySlots}");
            Debug.Log($"  Allows Any Product Type: {testShelf.AllowsAnyProductType}");
            if (!testShelf.AllowsAnyProductType)
            {
                Debug.Log($"  Allowed Product Type: {testShelf.AllowedProductType}");
            }
            
            // Check if shelf has slots
            if (testShelf.TotalSlots == 0)
            {
                Debug.LogError("Shelf has no slots! Check if autoCreateSlots is enabled or manually add ShelfSlot components.");
                return;
            }
            
            // Clear existing test products
            ClearSpawnedProducts();
            
            // Try to stock each slot with a random product
            for (int i = 0; i < testShelf.TotalSlots && i < productPrefabs.Count * 2; i++)
            {
                // Get random product prefab
                GameObject prefab = productPrefabs[Random.Range(0, productPrefabs.Count)];
                
                // Spawn product
                Vector3 spawnPos = testShelf.transform.position + Vector3.up * 2 + Vector3.forward * (i * 0.5f);
                GameObject productObject = Instantiate(prefab, spawnPos, Quaternion.identity);
                Product product = productObject.GetComponent<Product>();
                
                if (product == null)
                {
                    Debug.LogError($"Product prefab {prefab.name} doesn't have Product component!");
                    DestroyImmediate(productObject);
                    continue;
                }
                
                // Initialize with test data if available
                if (testProductDataList.Count > 0)
                {
                    ProductData testData = testProductDataList[Random.Range(0, testProductDataList.Count)];
                    product.Initialize(testData);
                    Debug.Log($"Initialized product with data: {testData.ProductName} (Type: {testData.Type})");
                }
                else
                {
                    Debug.LogWarning("No test product data available. Product may not have proper type assignment.");
                }
                
                // Debug slot availability
                ShelfSlot targetSlot = testShelf.GetSlot(i);
                if (targetSlot == null)
                {
                    Debug.LogError($"Slot {i} is null! This should not happen.");
                    DestroyImmediate(productObject);
                    continue;
                }
                
                Debug.Log($"Attempting to place product in slot {i} at position {targetSlot.SlotPosition}");
                Debug.Log($"  Slot empty: {targetSlot.IsEmpty}");
                Debug.Log($"  Product type: {product.ProductData?.Type}");
                Debug.Log($"  Type allowed: {testShelf.IsProductTypeAllowed(product.ProductData?.Type ?? ProductType.MiniatureBox)}");
                
                // Try to place on shelf
                bool placed = testShelf.TryPlaceProduct(product);
                if (placed)
                {
                    Debug.Log($"✓ Successfully placed {product.ProductData?.ProductName ?? "Product"} in shelf");
                    spawnedTestProducts.Add(product);
                }
                else
                {
                    Debug.LogError($"✗ Failed to place {product.ProductData?.ProductName ?? "Product"} - product will be destroyed");
                    DestroyImmediate(productObject);
                }
            }
            
            Debug.Log($"Stocking complete. Shelf status: {testShelf.GetShelfStatus()}");
        }
        
        /// <summary>
        /// Clear all products from the test shelf
        /// </summary>
        private void ClearTestShelf()
        {
            if (testShelf == null)
            {
                Debug.LogWarning("No test shelf to clear!");
                return;
            }
            
            Debug.Log("=== Clearing Test Shelf ===");
            
            // Remove all products from shelf
            List<Product> shelfProducts = testShelf.GetAllProducts();
            foreach (Product product in shelfProducts)
            {
                if (product != null)
                {
                    DestroyImmediate(product.gameObject);
                }
            }
            
            // Clear shelf slots
            testShelf.ClearAllProducts();
            
            // Clear spawned products list
            ClearSpawnedProducts();
            
            Debug.Log($"Shelf cleared. Status: {testShelf.GetShelfStatus()}");
        }
        
        /// <summary>
        /// Run comprehensive tests on the shelf system
        /// </summary>
        private void RunComprehensiveShelfTests()
        {
            if (testShelf == null)
            {
                Debug.LogWarning("No test shelf available! Create one first with Y key.");
                return;
            }
            
            Debug.Log("=== Running Comprehensive Shelf Tests ===");
            
            // Test 1: Basic shelf info
            Debug.Log($"Test 1 - Shelf Info: {testShelf.GetShelfStatus()}");
            
            // Test 2: Slot access
            Debug.Log("Test 2 - Slot Access:");
            for (int i = 0; i < testShelf.TotalSlots; i++)
            {
                ShelfSlot slot = testShelf.GetSlot(i);
                if (slot != null)
                {
                    Debug.Log($"  Slot {i}: {(slot.IsEmpty ? "Empty" : $"Occupied by {slot.CurrentProduct.ProductData?.ProductName}")}");
                }
            }
            
            // Test 3: Product type filtering
            Debug.Log("Test 3 - Product Type Filtering:");
            var miniBoxes = testShelf.GetProductsByType(ProductType.MiniatureBox);
            var paintPots = testShelf.GetProductsByType(ProductType.PaintPot);
            var rulebooks = testShelf.GetProductsByType(ProductType.Rulebook);
            
            Debug.Log($"  Miniature Boxes: {miniBoxes.Count}");
            Debug.Log($"  Paint Pots: {paintPots.Count}");
            Debug.Log($"  Rulebooks: {rulebooks.Count}");
            
            // Test 4: Empty slot finding
            Debug.Log("Test 4 - Empty Slot Finding:");
            ShelfSlot emptySlot = testShelf.GetFirstEmptySlot();
            if (emptySlot != null)
            {
                Debug.Log($"  First empty slot found: {emptySlot.name}");
            }
            else
            {
                Debug.Log($"  No empty slots available (shelf is full)");
            }
            
            // Test 5: Capacity checks
            Debug.Log("Test 5 - Capacity Checks:");
            Debug.Log($"  Is Full: {testShelf.IsFull}");
            Debug.Log($"  Is Empty: {testShelf.IsEmpty}");
            Debug.Log($"  Occupied Slots: {testShelf.OccupiedSlots}/{testShelf.TotalSlots}");
            Debug.Log($"  Empty Slots: {testShelf.EmptySlots}");
            
            // Test 6: Product type restrictions
            Debug.Log("Test 6 - Product Type Restrictions:");
            Debug.Log($"  Allows any type: {testShelf.AllowsAnyProductType}");
            Debug.Log($"  Allowed type: {testShelf.AllowedProductType}");
            Debug.Log($"  Allows Miniature Boxes: {testShelf.IsProductTypeAllowed(ProductType.MiniatureBox)}");
            Debug.Log($"  Allows Paint Pots: {testShelf.IsProductTypeAllowed(ProductType.PaintPot)}");
            Debug.Log($"  Allows Rulebooks: {testShelf.IsProductTypeAllowed(ProductType.Rulebook)}");
            
            Debug.Log("=== Shelf Tests Complete ===");
        }
        
        #endregion
        
        #region Helper Methods
        
        /// <summary>
        /// Clear spawned test products
        /// </summary>
        private void ClearSpawnedProducts()
        {
            foreach (Product product in spawnedTestProducts)
            {
                if (product != null)
                {
                    DestroyImmediate(product.gameObject);
                }
            }
            spawnedTestProducts.Clear();
        }
        
        /// <summary>
        /// Debug the shelf setup to identify issues
        /// </summary>
        private void DebugShelfSetup()
        {
            if (testShelf == null)
            {
                Debug.LogWarning("No test shelf available! Create one first with Y key or assign one in the inspector.");
                return;
            }
            
            Debug.Log("=== SHELF SETUP DEBUG ===");
            Debug.Log($"Shelf GameObject: {testShelf.name}");
            Debug.Log($"Shelf Position: {testShelf.transform.position}");
            Debug.Log($"Shelf Rotation: {testShelf.transform.rotation}");
            
            // Check Shelf component settings
            Debug.Log($"Total Slots: {testShelf.TotalSlots}");
            Debug.Log($"Allows Any Product Type: {testShelf.AllowsAnyProductType}");
            if (!testShelf.AllowsAnyProductType)
            {
                Debug.Log($"Allowed Product Type: {testShelf.AllowedProductType}");
            }
            
            // Check each slot
            for (int i = 0; i < testShelf.TotalSlots; i++)
            {
                ShelfSlot slot = testShelf.GetSlot(i);
                if (slot != null)
                {
                    Debug.Log($"Slot {i}: {slot.name}");
                    Debug.Log($"  Position: {slot.transform.position}");
                    Debug.Log($"  Slot Position: {slot.SlotPosition}");
                    Debug.Log($"  Is Empty: {slot.IsEmpty}");
                    Debug.Log($"  Has Collider: {slot.GetComponent<Collider>() != null}");
                    
                    if (!slot.IsEmpty)
                    {
                        Debug.Log($"  Current Product: {slot.CurrentProduct.ProductData?.ProductName}");
                    }
                }
                else
                {
                    Debug.LogError($"Slot {i} is NULL!");
                }
            }
            
            // Check child objects
            Debug.Log($"Shelf has {testShelf.transform.childCount} child objects:");
            for (int i = 0; i < testShelf.transform.childCount; i++)
            {
                Transform child = testShelf.transform.GetChild(i);
                ShelfSlot slotComponent = child.GetComponent<ShelfSlot>();
                Debug.Log($"  Child {i}: {child.name} (Has ShelfSlot: {slotComponent != null})");
            }
            
            Debug.Log("=== END SHELF DEBUG ===");
        }
        
        #endregion
        
        #region GUI
        
        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10, 200, 350, 300));
            GUILayout.Label("Shelf Testing Controls:");
            GUILayout.Label($"Press '{createShelfKey}' to create test shelf");
            GUILayout.Label($"Press '{stockShelfKey}' to stock shelf with products");
            GUILayout.Label($"Press '{clearShelfKey}' to clear shelf");
            GUILayout.Label($"Press '{runShelfTestsKey}' to run comprehensive tests");
            GUILayout.Label($"Press '{debugShelfKey}' to debug shelf setup");
            
            GUILayout.Space(10);
            
            if (testShelf != null)
            {
                GUILayout.Label($"Test Shelf: {testShelf.name}");
                GUILayout.Label($"Status: {testShelf.GetShelfStatus()}");
                GUILayout.Label($"Occupied: {testShelf.OccupiedSlots}/{testShelf.TotalSlots}");
                GUILayout.Label($"Is Full: {testShelf.IsFull}");
                GUILayout.Label($"Is Empty: {testShelf.IsEmpty}");
                GUILayout.Label($"Allows Any Type: {testShelf.AllowsAnyProductType}");
                if (!testShelf.AllowsAnyProductType)
                {
                    GUILayout.Label($"Allowed Type: {testShelf.AllowedProductType}");
                }
                
                GUILayout.Space(5);
                GUILayout.Label("First Empty Slot:");
                ShelfSlot emptySlot = testShelf.GetFirstEmptySlot();
                GUILayout.Label(emptySlot != null ? emptySlot.name : "None (shelf full)");
            }
            else
            {
                GUILayout.Label("No test shelf available");
                GUILayout.Label("Create one or assign in inspector");
            }
            
            GUILayout.EndArea();
        }
        
        #endregion
        
        #region Cleanup
        
        private void OnDestroy()
        {
            ClearSpawnedProducts();
        }
        
        #endregion
    }
}
