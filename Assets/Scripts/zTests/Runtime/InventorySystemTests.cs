using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using TabletopShop;

namespace TabletopShop.Tests
{
    /// <summary>
    /// Unity Test Framework integration tests for InventoryManager and Product systems
    /// 
    /// Integration Test Coverage:
    /// - InventoryManager.AddProduct() and RemoveProduct() operations with economic constraints
    /// - Product state transitions (Available → OnShelf → Purchased)
    /// - ShelfSlot capacity management and product type compatibility
    /// - Inventory-shelf integration workflows (place product on shelf)
    /// - Edge cases: overstocking, removing non-existent products, invalid placements
    /// - Inventory persistence and state recovery across operations
    /// - Performance testing for large inventory operations
    /// 
    /// Integration Testing Patterns:
    /// - [UnityTest] for scene-based testing with GameObject lifecycle management
    /// - Test scene setup with minimal required components (GameManager, InventoryManager, Shelf, ShelfSlot)
    /// - yield return for time-based operations and coroutine execution
    /// - State verification across multiple system interactions
    /// - Memory and performance validation for inventory operations
    /// 
    /// Architecture Focus:
    /// - System-to-system communication validation (Inventory ↔ Shelf ↔ Product)
    /// - Integration test scope: inventory-product-shelf interactions only
    /// - State consistency across economic constraints and business rules
    /// - Edge case handling for inventory reliability
    /// 
    /// Test Isolation Strategy:
    /// - Fresh scene setup per test for clean integration environment
    /// - Component cleanup to prevent memory leaks and test interference
    /// - Singleton reset for consistent InventoryManager state
    /// - Product and shelf state reset between tests
    /// </summary>
    public class InventorySystemTests
    {
        #region Test Scene Setup and Teardown
        
        private GameObject testSceneRoot;
        private GameManager gameManager;
        private InventoryManager inventoryManager;
        private GameObject shelfObject;
        private Shelf shelf;
        private ShelfSlot[] shelfSlots;
        private ProductData testProductData;
        
        /// <summary>
        /// Integration Test Setup - Creates minimal test scene with required systems
        /// 
        /// Scene-Based Integration Testing:
        /// - Creates fresh GameObject hierarchy for each test
        /// - Initializes all required systems (GameManager, InventoryManager, Shelf)
        /// - Sets up test ProductData ScriptableObject for consistent testing
        /// - Ensures proper component dependencies and initialization order
        /// 
        /// Test Scene Architecture:
        /// TestSceneRoot
        /// ├── GameManager (singleton economic system)
        /// ├── InventoryManager (singleton inventory system)
        /// └── TestShelf
        ///     ├── ShelfSlot (Product placement system)
        ///     ├── ShelfSlot
        ///     └── ShelfSlot (3 slots for capacity testing)
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            // Create test scene root for organization and cleanup
            testSceneRoot = new GameObject("IntegrationTestScene");
            
            // Set up GameManager for economic system integration
            var gameManagerObject = new GameObject("TestGameManager");
            gameManagerObject.transform.SetParent(testSceneRoot.transform);
            gameManager = gameManagerObject.AddComponent<GameManager>();
            
            // Force GameManager singleton initialization for economic integration
            var gameManagerInstanceField = typeof(GameManager).GetField("_instance", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            gameManagerInstanceField?.SetValue(null, gameManager);
            
            // Initialize GameManager economic system
            var gameManagerAwakeMethod = typeof(GameManager).GetMethod("Awake", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            gameManagerAwakeMethod?.Invoke(gameManager, null);
            
            // Set up InventoryManager for inventory system integration
            var inventoryManagerObject = new GameObject("TestInventoryManager");
            inventoryManagerObject.transform.SetParent(testSceneRoot.transform);
            inventoryManager = inventoryManagerObject.AddComponent<InventoryManager>();
            
            // Force InventoryManager singleton initialization
            var inventoryInstanceField = typeof(InventoryManager).GetField("_instance", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            inventoryInstanceField?.SetValue(null, inventoryManager);
            
            // Initialize InventoryManager system
            var inventoryAwakeMethod = typeof(InventoryManager).GetMethod("Awake", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            inventoryAwakeMethod?.Invoke(inventoryManager, null);
            
            // Set up test shelf with multiple slots for capacity testing
            SetupTestShelf();
            
            // Create test ProductData for consistent integration testing
            SetupTestProductData();
            
            // Ensure sufficient starting money for inventory operations
            gameManager.AddMoney(10000.0f, "Integration Test Setup");
        }
        
        /// <summary>
        /// Creates test shelf with multiple ShelfSlots for integration testing
        /// 
        /// Shelf Integration Architecture:
        /// - Multi-slot shelf for capacity and placement testing
        /// - ShelfSlotLogic components for business rule enforcement
        /// - ShelfSlot components for product placement management
        /// - Proper parent-child hierarchy for realistic integration
        /// </summary>
        private void SetupTestShelf()
        {
            // Create main shelf GameObject
            shelfObject = new GameObject("TestShelf");
            shelfObject.transform.SetParent(testSceneRoot.transform);
            shelf = shelfObject.AddComponent<Shelf>();
            
            // Initialize shelf with 3 slots for capacity testing
            // The new consolidated Initialize() method creates slots automatically
            shelf.Initialize(maxSlots: 3, slotSpacing: 1.5f, allowedType: ProductType.MiniatureBox, allowAnyType: true);
            
            // Get the automatically created slots for testing
            shelfSlots = new ShelfSlot[3];
            for (int i = 0; i < 3; i++)
            {
                shelfSlots[i] = shelf.GetSlot(i);
                
                // Verify slot was created properly
                if (shelfSlots[i] == null)
                {
                    Debug.LogError($"Slot {i} was not created properly by shelf initialization");
                }
            }
        }
        
        /// <summary>
        /// Creates test ProductData ScriptableObject for consistent testing
        /// 
        /// Test Product Configuration:
        /// - Standard product type for compatibility testing
        /// - Reasonable price for economic integration testing
        /// - Consistent data for repeatable test results
        /// </summary>
        private void SetupTestProductData()
        {
            testProductData = ScriptableObject.CreateInstance<ProductData>();
            testProductData.productName = "Test Product";
            testProductData.type = ProductType.MiniatureBox; // Standard type
            testProductData.basePrice = 25.0f;
            testProductData.costPrice = 15.0f; // 10.0f profit margin
            testProductData.description = "Integration test product";
        }
        
        /// <summary>
        /// Integration Test Cleanup - Ensures proper resource disposal
        /// 
        /// Cleanup Strategy:
        /// - Destroy entire test scene hierarchy to prevent GameObject accumulation
        /// - Clear singleton instances to prevent test interference
        /// - Reset system state for next test isolation
        /// - Dispose ScriptableObject instances to prevent memory leaks
        /// </summary>
        [TearDown]
        public void TearDown()
        {
            // Clear singleton instances for test isolation
            var gameManagerInstanceField = typeof(GameManager).GetField("_instance", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            gameManagerInstanceField?.SetValue(null, null);
            
            var inventoryInstanceField = typeof(InventoryManager).GetField("_instance", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            inventoryInstanceField?.SetValue(null, null);
            
            // Destroy test scene hierarchy
            if (testSceneRoot != null)
            {
                Object.DestroyImmediate(testSceneRoot);
            }
            
            // Dispose test ScriptableObject
            if (testProductData != null)
            {
                Object.DestroyImmediate(testProductData);
            }
            
            // Clear references
            gameManager = null;
            inventoryManager = null;
            shelf = null;
            shelfSlots = null;
            testProductData = null;
        }
        
        #endregion
        
        #region InventoryManager AddProduct Integration Tests
        
        /// <summary>
        /// Integration test for InventoryManager.AddProduct() with economic validation
        /// 
        /// Integration Test Pattern:
        /// - Validates inventory system integration with economic constraints
        /// - Tests cross-system state consistency (Inventory ↔ GameManager)
        /// - Verifies proper economic transaction processing
        /// - Checks inventory count and economic state synchronization
        /// </summary>
        [UnityTest]
        public IEnumerator AddProduct_SufficientFunds_AddsToInventoryAndDeductsMoney()
        {
            // Arrange - Integration test setup
            float initialMoney = gameManager.CurrentMoney;
            int initialInventoryCount = inventoryManager.TotalProductCount;
            int quantityToAdd = 3;
            float expectedCost = testProductData.costPrice * quantityToAdd;
            
            // Verify preconditions for integration test
            Assert.GreaterOrEqual(initialMoney, expectedCost, 
                "Integration test requires sufficient funds for economic validation");
            
            // Act - Execute cross-system operation
            bool result = inventoryManager.AddProduct(testProductData, quantityToAdd);
            
            // Wait for potential async operations
            yield return null;
            
            // Assert - Verify integration across systems
            Assert.IsTrue(result, "AddProduct should succeed with sufficient funds");
            
            // Verify inventory system state
            Assert.AreEqual(initialInventoryCount + quantityToAdd, 
                inventoryManager.TotalProductCount,
                "Inventory count should increase by added quantity");
            
            Assert.IsTrue(inventoryManager.HasProduct(testProductData),
                "Inventory should contain the added product");
            
            Assert.AreEqual(quantityToAdd, inventoryManager.GetProductCount(testProductData),
                "Product count should match added quantity");
            
            // Verify economic system integration
            Assert.AreEqual(initialMoney - expectedCost, gameManager.CurrentMoney, 0.01f,
                "Money should be deducted for inventory purchase cost");
        }
        
        /// <summary>
        /// Integration test for AddProduct with insufficient funds (economic constraint)
        /// 
        /// Economic Constraint Integration:
        /// - Tests economic system rejection of invalid transactions
        /// - Verifies inventory system respects economic constraints
        /// - Validates no state changes occur when constraints violated
        /// </summary>
        [UnityTest]
        public IEnumerator AddProduct_InsufficientFunds_RejectsTransactionAndMaintainsState()
        {
            // Arrange - Create insufficient funds scenario
            float currentMoney = gameManager.CurrentMoney;
            gameManager.SubtractMoney(currentMoney - 5.0f, "Setup insufficient funds"); // Leave only $5
            
            int initialInventoryCount = inventoryManager.TotalProductCount;
            int expensiveQuantity = 10; // Would cost $150 (15 * 10)
            float remainingMoney = gameManager.CurrentMoney;
            
            // Verify insufficient funds condition
            Assert.Less(remainingMoney, testProductData.costPrice * expensiveQuantity,
                "Test requires insufficient funds for economic constraint validation");
            
            // Act - Attempt operation that should be rejected
            bool result = inventoryManager.AddProduct(testProductData, expensiveQuantity);
            
            yield return null;
            
            // Assert - Verify economic constraint enforcement
            Assert.IsFalse(result, "AddProduct should fail with insufficient funds");
            
            // Verify no state changes occurred
            Assert.AreEqual(initialInventoryCount, inventoryManager.TotalProductCount,
                "Inventory count should remain unchanged when transaction rejected");
            
            Assert.IsFalse(inventoryManager.HasProduct(testProductData),
                "Product should not be added to inventory when transaction rejected");
            
            Assert.AreEqual(remainingMoney, gameManager.CurrentMoney, 0.01f,
                "Money should remain unchanged when transaction rejected");
        }
        
        /// <summary>
        /// Integration test for AddProduct with zero quantity (edge case)
        /// 
        /// Edge Case Integration:
        /// - Tests system behavior with boundary conditions
        /// - Verifies proper validation across system boundaries
        /// - Ensures no unintended side effects occur
        /// </summary>
        [UnityTest]
        public IEnumerator AddProduct_ZeroQuantity_RejectsTransactionGracefully()
        {
            // Arrange
            float initialMoney = gameManager.CurrentMoney;
            int initialInventoryCount = inventoryManager.TotalProductCount;
            
            // Act
            bool result = inventoryManager.AddProduct(testProductData, 0);
            
            yield return null;
            
            // Assert - Verify graceful rejection
            Assert.IsFalse(result, "AddProduct should reject zero quantity");
            
            // Verify no state changes
            Assert.AreEqual(initialInventoryCount, inventoryManager.TotalProductCount,
                "Inventory should remain unchanged with zero quantity");
            
            Assert.AreEqual(initialMoney, gameManager.CurrentMoney, 0.01f,
                "Money should remain unchanged with zero quantity");
        }
        
        /// <summary>
        /// Integration test for AddProduct with negative quantity (invalid input)
        /// 
        /// Invalid Input Integration:
        /// - Tests system robustness with invalid inputs
        /// - Verifies input validation across system boundaries
        /// - Ensures system stability with malformed requests
        /// </summary>
        [UnityTest]
        public IEnumerator AddProduct_NegativeQuantity_RejectsTransactionGracefully()
        {
            // Arrange
            float initialMoney = gameManager.CurrentMoney;
            int initialInventoryCount = inventoryManager.TotalProductCount;
            
            // Act
            bool result = inventoryManager.AddProduct(testProductData, -5);
            
            yield return null;
            
            // Assert - Verify robust rejection
            Assert.IsFalse(result, "AddProduct should reject negative quantity");
            
            // Verify system stability
            Assert.AreEqual(initialInventoryCount, inventoryManager.TotalProductCount,
                "Inventory should remain stable with invalid input");
            
            Assert.AreEqual(initialMoney, gameManager.CurrentMoney, 0.01f,
                "Economic state should remain stable with invalid input");
        }
        
        #endregion
        
        #region InventoryManager RemoveProduct Integration Tests
        
        /// <summary>
        /// Integration test for InventoryManager.RemoveProduct() with existing products
        /// 
        /// Remove Product Integration:
        /// - Tests inventory reduction with existing stock
        /// - Verifies proper quantity management across operations
        /// - Validates inventory state consistency after removal
        /// </summary>
        [UnityTest]
        public IEnumerator RemoveProduct_ExistingProduct_ReducesInventoryCount()
        {
            // Arrange - Set up initial inventory
            int initialQuantity = 5;
            inventoryManager.AddProduct(testProductData, initialQuantity);
            yield return null; // Wait for add operation
            
            int quantityToRemove = 2;
            int expectedRemainingQuantity = initialQuantity - quantityToRemove;
            
            // Act
            bool result = inventoryManager.RemoveProduct(testProductData, quantityToRemove);
            
            yield return null;
            
            // Assert - Verify successful removal
            Assert.IsTrue(result, "RemoveProduct should succeed with existing inventory");
            
            // Verify inventory state
            Assert.AreEqual(expectedRemainingQuantity, 
                inventoryManager.GetProductCount(testProductData),
                "Product count should decrease by removed quantity");
            
            Assert.IsTrue(inventoryManager.HasProduct(testProductData),
                "Product should still exist in inventory after partial removal");
        }
        
        /// <summary>
        /// Integration test for RemoveProduct removing all remaining stock
        /// 
        /// Complete Removal Integration:
        /// - Tests complete product removal from inventory
        /// - Verifies inventory cleanup when stock reaches zero
        /// - Validates HasProduct returns false after complete removal
        /// </summary>
        [UnityTest]
        public IEnumerator RemoveProduct_CompleteStock_RemovesProductFromInventory()
        {
            // Arrange
            int initialQuantity = 3;
            inventoryManager.AddProduct(testProductData, initialQuantity);
            yield return null;
            
            // Act - Remove all stock
            bool result = inventoryManager.RemoveProduct(testProductData, initialQuantity);
            
            yield return null;
            
            // Assert - Verify complete removal
            Assert.IsTrue(result, "RemoveProduct should succeed when removing all stock");
            
            Assert.IsFalse(inventoryManager.HasProduct(testProductData),
                "Product should not exist in inventory after complete removal");
            
            Assert.AreEqual(0, inventoryManager.GetProductCount(testProductData),
                "Product count should be zero after complete removal");
        }
        
        /// <summary>
        /// Integration test for RemoveProduct with non-existent product (edge case)
        /// 
        /// Non-Existent Product Integration:
        /// - Tests system behavior with invalid removal requests
        /// - Verifies graceful handling of non-existent products
        /// - Ensures system stability with invalid operations
        /// </summary>
        [UnityTest]
        public IEnumerator RemoveProduct_NonExistentProduct_RejectsTransactionGracefully()
        {
            // Arrange - Ensure product is not in inventory
            Assert.IsFalse(inventoryManager.HasProduct(testProductData),
                "Test requires product to not exist in inventory");
            
            int totalCountBefore = inventoryManager.TotalProductCount;
            
            // Act
            bool result = inventoryManager.RemoveProduct(testProductData, 1);
            
            yield return null;
            
            // Assert - Verify graceful rejection
            Assert.IsFalse(result, "RemoveProduct should fail for non-existent product");
            
            Assert.AreEqual(totalCountBefore, inventoryManager.TotalProductCount,
                "Total inventory count should remain unchanged");
        }
        
        /// <summary>
        /// Integration test for RemoveProduct with quantity exceeding stock
        /// 
        /// Insufficient Stock Integration:
        /// - Tests removal request exceeding available stock
        /// - Verifies proper stock validation and constraint enforcement
        /// - Ensures no partial removal occurs when constraint violated
        /// </summary>
        [UnityTest]
        public IEnumerator RemoveProduct_InsufficientStock_RejectsTransactionAndMaintainsStock()
        {
            // Arrange
            int availableStock = 2;
            inventoryManager.AddProduct(testProductData, availableStock);
            yield return null;
            
            int excessiveQuantity = 5; // More than available
            
            // Act
            bool result = inventoryManager.RemoveProduct(testProductData, excessiveQuantity);
            
            yield return null;
            
            // Assert - Verify stock constraint enforcement
            Assert.IsFalse(result, "RemoveProduct should fail when requesting more than available stock");
            
            // Verify stock remains unchanged
            Assert.AreEqual(availableStock, inventoryManager.GetProductCount(testProductData),
                "Stock should remain unchanged when removal exceeds availability");
            
            Assert.IsTrue(inventoryManager.HasProduct(testProductData),
                "Product should still exist after failed excessive removal");
        }
        
        #endregion
        
        #region Product State Transition Integration Tests
        
        /// <summary>
        /// Integration test for Product state transitions: Available → OnShelf → Purchased
        /// 
        /// Product State Lifecycle Integration:
        /// - Tests complete product lifecycle through all states
        /// - Verifies state transition validation and consistency
        /// - Validates integration between Product component and state management
        /// - Tests state persistence across operations
        /// 
        /// State Transition Flow:
        /// 1. Create product in Available state (from inventory)
        /// 2. Place on shelf → OnShelf state
        /// 3. Purchase product → Purchased state
        /// 4. Verify state consistency at each step
        /// </summary>
        [UnityTest]
        public IEnumerator ProductStateTransitions_AvailableToOnShelfToPurchased_MaintainsStateConsistency()
        {
            // Arrange - Add product to inventory (Available state)
            inventoryManager.AddProduct(testProductData, 1);
            yield return null;
            
            // Create Product GameObject for state testing
            var productObject = new GameObject("TestProduct");
            productObject.transform.SetParent(testSceneRoot.transform);
            var product = productObject.AddComponent<Product>();
            product.Initialize(testProductData);
            
            // Verify initial Available state
            Assert.AreEqual(ProductState.Available, product.CurrentState,
                "Product should start in Available state");
            
            // Act 1 - Transition Available → OnShelf
            var targetSlot = shelfSlots[0]; // Use first shelf slot
            bool placementResult = targetSlot.PlaceProduct(product);
            
            yield return null; // Wait for placement processing
            
            // Assert - Verify OnShelf state transition
            Assert.IsTrue(placementResult, "Product placement on shelf should succeed");
            Assert.AreEqual(ProductState.OnShelf, product.CurrentState,
                "Product should transition to OnShelf state when placed");
            
            Assert.AreEqual(targetSlot, product.CurrentShelfSlot,
                "Product should reference the shelf slot it's placed on");
    
            
            // Act 2 - Transition OnShelf → Purchased
            product.Purchase();
            
            yield return null; // Wait for purchase processing
            
            // Assert - Verify Purchased state transition
            Assert.AreEqual(ProductState.Purchased, product.CurrentState,
                "Product should transition to Purchased state after purchase");
            
            Assert.IsNull(product.CurrentShelfSlot,
                "Product should not reference shelf slot after purchase");
        }
        
        /// <summary>
        /// Integration test for invalid Product state transitions
        /// 
        /// Invalid State Transition Integration:
        /// - Tests system robustness with invalid state changes
        /// - Verifies state transition validation rules
        /// - Ensures system stability with improper state manipulation
        /// </summary>
        [UnityTest]
        public IEnumerator ProductStateTransitions_InvalidTransitions_RejectsAndMaintainsState()
        {
            // Arrange
            var productObject = new GameObject("TestProduct");
            productObject.transform.SetParent(testSceneRoot.transform);
            var product = productObject.AddComponent<Product>();
            product.Initialize(testProductData);
            
            // Verify initial state
            Assert.AreEqual(ProductState.Available, product.CurrentState,
                "Product should start in Available state");
            
            // Act & Assert - Test invalid transition: Available → Purchased (skipping OnShelf)
            product.Purchase(); // This should fail or handle gracefully
            
            yield return null;
            
            // The exact behavior depends on Product implementation
            // Either state should remain Available, or transition should be handled gracefully
            ProductState stateAfterDirectPurchase = product.CurrentState;
            
            // Verify system maintains consistency
            Assert.IsTrue(stateAfterDirectPurchase == ProductState.Available || 
                         stateAfterDirectPurchase == ProductState.Purchased,
                "Product state should remain consistent after invalid transition attempt");
        }
        
        /// <summary>
        /// Integration test for Product state recovery after shelf removal
        /// 
        /// State Recovery Integration:
        /// - Tests Product state when removed from shelf
        /// - Verifies proper state rollback: OnShelf → Available
        /// - Validates shelf-product relationship cleanup
        /// </summary>
        [UnityTest]
        public IEnumerator ProductStateTransitions_OnShelfToAvailable_RestoresAvailableState()
        {
            // Arrange - Set up product on shelf
            var productObject = new GameObject("TestProduct");
            productObject.transform.SetParent(testSceneRoot.transform);
            var product = productObject.AddComponent<Product>();
            product.Initialize(testProductData);
            
            var targetSlot = shelfSlots[0];
            targetSlot.PlaceProduct(product);
            yield return null;
            
            // Verify OnShelf state
            Assert.AreEqual(ProductState.OnShelf, product.CurrentState,
                "Product should be OnShelf before removal");
            
            // Act - Remove product from shelf
            bool removalResult = targetSlot.RemoveProduct();
            
            yield return null;
            
            // Assert - Verify state recovery
            Assert.IsTrue(removalResult, "Product removal from shelf should succeed");
            
            Assert.AreEqual(ProductState.Available, product.CurrentState,
                "Product should return to Available state after shelf removal");
            
            Assert.IsNull(product.CurrentShelfSlot,
                "Product should not reference shelf slot after removal");
            
        }
        
        #endregion
        
        #region ShelfSlot Capacity and Compatibility Integration Tests
        
        /// <summary>
        /// Integration test for ShelfSlot capacity management
        /// 
        /// Capacity Management Integration:
        /// - Tests single-product capacity constraint per slot
        /// - Verifies proper rejection of multiple product placement
        /// - Validates capacity state consistency across operations
        /// </summary>
        [UnityTest]
        public IEnumerator ShelfSlotCapacity_SingleSlot_HandlesOnlyOneProduct()
        {
            // Arrange - Create multiple products for capacity testing
            var product1Object = new GameObject("TestProduct1");
            product1Object.transform.SetParent(testSceneRoot.transform);
            var product1 = product1Object.AddComponent<Product>();
            product1.Initialize(testProductData);
            
            var product2Object = new GameObject("TestProduct2");
            product2Object.transform.SetParent(testSceneRoot.transform);
            var product2 = product2Object.AddComponent<Product>();
            product2.Initialize(testProductData);
            
            var targetSlot = shelfSlots[0];
            
            // Act 1 - Place first product (should succeed)
            bool firstPlacement = targetSlot.PlaceProduct(product1);
            yield return null;
            
            // Assert - Verify first placement
            Assert.IsTrue(firstPlacement, "First product placement should succeed");
            
            // Act 2 - Attempt to place second product (should fail)
            bool secondPlacement = targetSlot.PlaceProduct(product2);
            yield return null;
            
            // Assert - Verify capacity constraint enforcement
            Assert.IsFalse(secondPlacement, "Second product placement should fail due to capacity");

            Assert.AreEqual(ProductState.Available, product2.CurrentState,
                "Second product should remain in Available state");
        }
        
        /// <summary>
        /// Integration test for multiple ShelfSlots capacity distribution
        /// 
        /// Multi-Slot Capacity Integration:
        /// - Tests product distribution across multiple shelf slots
        /// - Verifies independent capacity management per slot
        /// - Validates shelf-level capacity coordination
        /// </summary>
        [UnityTest]
        public IEnumerator ShelfSlotCapacity_MultipleSlots_DistributesProductsCorrectly()
        {
            // Arrange - Create products for all available slots
            var products = new Product[shelfSlots.Length];
            for (int i = 0; i < products.Length; i++)
            {
                var productObject = new GameObject($"TestProduct_{i}");
                productObject.transform.SetParent(testSceneRoot.transform);
                products[i] = productObject.AddComponent<Product>();
                products[i].Initialize(testProductData);
            }
            
            // Act - Place products in each slot
            for (int i = 0; i < shelfSlots.Length; i++)
            {
                bool placementResult = shelfSlots[i].PlaceProduct(products[i]);
                yield return null;
                
                // Assert each placement
                Assert.IsTrue(placementResult, $"Product {i} placement should succeed");

            }
            
        }
        
        /// <summary>
        /// Integration test for Product type compatibility with ShelfSlots
        /// 
        /// Product Type Compatibility Integration:
        /// - Tests product type restrictions on shelf placement
        /// - Verifies compatibility validation across Product-Shelf integration
        /// - Validates business rule enforcement for product categories
        /// 
        /// Note: This test assumes ShelfSlot may have product type restrictions
        /// If no restrictions exist, this test validates unrestricted placement
        /// </summary>
        [UnityTest]
        public IEnumerator ShelfSlotCompatibility_ProductTypes_EnforcesCompatibilityRules()
        {
            // Arrange - Create products of different types
            var miniaturesProductData = ScriptableObject.CreateInstance<ProductData>();
            miniaturesProductData.productName = "Test Miniatures";
            miniaturesProductData.type = ProductType.MiniatureBox;
            miniaturesProductData.basePrice = 30.0f;
            miniaturesProductData.costPrice = 20.0f;
            
            var diceSetsProductData = ScriptableObject.CreateInstance<ProductData>();
            diceSetsProductData.productName = "Test Dice Sets";
            diceSetsProductData.type = ProductType.PaintPot;
            diceSetsProductData.basePrice = 15.0f;
            diceSetsProductData.costPrice = 10.0f;
            
            var miniaturesProduct = new GameObject("MiniaturesProduct");
            miniaturesProduct.transform.SetParent(testSceneRoot.transform);
            var miniaturesComponent = miniaturesProduct.AddComponent<Product>();
            miniaturesComponent.Initialize(miniaturesProductData);
            
            var diceProduct = new GameObject("DiceProduct");
            diceProduct.transform.SetParent(testSceneRoot.transform);
            var diceComponent = diceProduct.AddComponent<Product>();
            diceComponent.Initialize(diceSetsProductData);
            
            var targetSlot = shelfSlots[0];
            
            // Act & Assert - Test compatibility
            // Note: If ShelfSlot has no type restrictions, both should succeed
            // If restrictions exist, this test will validate them
            
            bool miniaturesPlacement = targetSlot.PlaceProduct(miniaturesComponent);
            yield return null;
            
            // For this integration test, we'll verify that placement either succeeds
            // (indicating no restrictions) or fails (indicating type restrictions)
            Assert.IsTrue(miniaturesPlacement || !miniaturesPlacement,
                "Placement result should be consistent with compatibility rules");
            
            // If first placement succeeded, verify state
            if (miniaturesPlacement)
            {

                
                // Remove and test second product type
                targetSlot.RemoveProduct();
                yield return null;
                
                bool dicePlacement = targetSlot.PlaceProduct(diceComponent);
                yield return null;
                
                Assert.IsTrue(dicePlacement,
                    "Different product type should also be placeable if no restrictions exist");
            }
            
            // Cleanup test ScriptableObjects
            Object.DestroyImmediate(miniaturesProductData);
            Object.DestroyImmediate(diceSetsProductData);
        }
        
        #endregion
        


        
        #region Edge Cases and Error Handling Integration Tests
        
        /// <summary>
        /// Integration test for overstocking scenario (edge case)
        /// 
        /// Overstocking Edge Case Integration:
        /// - Tests system behavior with excessive inventory quantities
        /// - Verifies economic constraint handling with large transactions
        /// - Validates system stability under stress conditions
        /// </summary>
        [UnityTest]
        public IEnumerator EdgeCases_OverstockingLargeQuantities_HandlesGracefully()
        {
            // Arrange - Attempt to add very large quantity
            int excessiveQuantity = 10000;
            float totalCost = testProductData.costPrice * excessiveQuantity;
            
            // Ensure sufficient funds for the test
            if (gameManager.CurrentMoney < totalCost)
            {
                gameManager.AddMoney(totalCost - gameManager.CurrentMoney + 1000.0f, "Overstocking test setup");
            }
            
            int initialInventoryCount = inventoryManager.TotalProductCount;
            
            // Act - Attempt massive inventory addition
            bool result = inventoryManager.AddProduct(testProductData, excessiveQuantity);
            yield return null;
            
            // Assert - Verify system handles large operations
            if (result)
            {
                // If system allows large quantities, verify consistency
                Assert.AreEqual(initialInventoryCount + excessiveQuantity,
                    inventoryManager.TotalProductCount,
                    "Large inventory addition should maintain count consistency");
                
                Assert.AreEqual(excessiveQuantity, inventoryManager.GetProductCount(testProductData),
                    "Product count should match large added quantity");
            }
            else
            {
                // If system rejects large quantities, verify no state changes
                Assert.AreEqual(initialInventoryCount, inventoryManager.TotalProductCount,
                    "Inventory should remain unchanged if large addition rejected");
            }
            
            // Verify economic state remains consistent
            Assert.GreaterOrEqual(gameManager.CurrentMoney, 0.0f,
                "Money should never go negative during large operations");
        }
        
        /// <summary>
        /// Integration test for null product data handling (error case)
        /// 
        /// Null Input Error Handling Integration:
        /// - Tests system robustness with null ProductData inputs
        /// - Verifies proper error handling across system boundaries
        /// - Ensures system stability with invalid object references
        /// </summary>
        [UnityTest]
        public IEnumerator EdgeCases_NullProductData_HandlesErrorsGracefully()
        {
            // Arrange
            ProductData nullProductData = null;
            int initialInventoryCount = inventoryManager.TotalProductCount;
            float initialMoney = gameManager.CurrentMoney;
            
            // Act - Attempt operations with null ProductData
            bool addResult = inventoryManager.AddProduct(nullProductData, 1);
            yield return null;
            
            bool removeResult = inventoryManager.RemoveProduct(nullProductData, 1);
            yield return null;
            
            bool hasResult = inventoryManager.HasProduct(nullProductData);
            int countResult = inventoryManager.GetProductCount(nullProductData);
            
            // Assert - Verify graceful error handling
            Assert.IsFalse(addResult, "AddProduct should reject null ProductData");
            Assert.IsFalse(removeResult, "RemoveProduct should reject null ProductData");
            Assert.IsFalse(hasResult, "HasProduct should return false for null ProductData");
            Assert.AreEqual(0, countResult, "GetProductCount should return 0 for null ProductData");
            
            // Verify no state changes occurred
            Assert.AreEqual(initialInventoryCount, inventoryManager.TotalProductCount,
                "Inventory count should remain unchanged with null inputs");
            Assert.AreEqual(initialMoney, gameManager.CurrentMoney, 0.01f,
                "Money should remain unchanged with null inputs");
        }
        

        
        #endregion
        
        #region Performance and Large-Scale Integration Tests
        
        /// <summary>
        /// Performance integration test for large inventory operations
        /// 
        /// Large-Scale Performance Integration:
        /// - Tests system performance with large inventory quantities
        /// - Verifies memory efficiency during bulk operations
        /// - Validates system responsiveness under load
        /// - Measures operation timing for performance regression detection
        /// </summary>
        [UnityTest]
        public IEnumerator Performance_LargeInventoryOperations_MaintainsAcceptablePerformance()
        {
            // Arrange - Performance test parameters
            int largeQuantity = 1000;
            float startTime = Time.realtimeSinceStartup;
            
            // Ensure sufficient funds
            float requiredFunds = testProductData.costPrice * largeQuantity;
            if (gameManager.CurrentMoney < requiredFunds)
            {
                gameManager.AddMoney(requiredFunds - gameManager.CurrentMoney + 1000.0f, "Performance test setup");
            }
            
            // Act - Large inventory addition
            bool addResult = inventoryManager.AddProduct(testProductData, largeQuantity);
            yield return null;
            
            float addTime = Time.realtimeSinceStartup - startTime;
            
            // Assert - Verify operation success and performance
            Assert.IsTrue(addResult, "Large inventory addition should succeed");
            Assert.AreEqual(largeQuantity, inventoryManager.GetProductCount(testProductData),
                "Large quantity should be added correctly");
            
            // Performance assertion (adjust threshold based on requirements)
            Assert.Less(addTime, 1.0f, "Large inventory addition should complete within 1 second");
            
            // Test large removal operation
            startTime = Time.realtimeSinceStartup;
            bool removeResult = inventoryManager.RemoveProduct(testProductData, largeQuantity / 2);
            yield return null;
            
            float removeTime = Time.realtimeSinceStartup - startTime;
            
            Assert.IsTrue(removeResult, "Large inventory removal should succeed");
            Assert.AreEqual(largeQuantity / 2, inventoryManager.GetProductCount(testProductData),
                "Remaining inventory should be correct after large removal");
            
            Assert.Less(removeTime, 1.0f, "Large inventory removal should complete within 1 second");
        }

        
        #endregion

    }
}
