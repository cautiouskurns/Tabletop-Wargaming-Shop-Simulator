using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

namespace TabletopShop
{
    /// <summary>
    /// Clean, minimal customer for behavior tree testing
    /// </summary>
    public class SimpleTestCustomer : MonoBehaviour
    {
        [Header("Shopping Configuration")]
        [SerializeField] private float startingMoney = 1000f;
        [SerializeField] private float shoppingTime = 10f;
        [SerializeField] private int maxProducts = 3;
        
        [Header("Current State")]
        public float currentMoney;
        public List<Product> selectedProducts = new List<Product>();
        public ShelfSlot currentTargetShelf;
        public Vector3 spawnPosition;
        
        [Header("Debug")]
        public bool showDebugLogs = true;
        
        private NavMeshAgent navAgent;
        private float shoppingStartTime;
        
        private void Awake()
        {
            navAgent = GetComponent<NavMeshAgent>();
            currentMoney = startingMoney;
            spawnPosition = transform.position;
            
            if (showDebugLogs)
                Debug.Log($"TestCustomer initialized with ${currentMoney}");
        }
        
        #region Movement Methods
        
        public bool MoveToPosition(Vector3 position)
        {
            if (navAgent != null && navAgent.isActiveAndEnabled)
            {
                navAgent.SetDestination(position);
                if (showDebugLogs)
                    Debug.Log($"TestCustomer moving to {position}");
                return true;
            }
            return false;
        }
        
        public bool HasReachedDestination()
        {
            if (navAgent == null) return true;
            
            bool reached = !navAgent.pathPending && 
                          navAgent.remainingDistance < 1f && 
                          navAgent.velocity.magnitude < 0.1f;
            
            return reached;
        }
        
        public bool MoveToShelf(ShelfSlot shelf)
        {
            if (shelf == null) return false;
            
            Vector3 shelfPosition = shelf.transform.position;
            Vector3 customerPosition = shelfPosition + shelf.transform.forward * 2f;
            
            currentTargetShelf = shelf;
            return MoveToPosition(customerPosition);
        }
        
        public bool MoveToExit()
        {
            // Simple exit strategy - move towards spawn + some distance
            Vector3 exitPosition = spawnPosition + Vector3.back * 5f;
            return MoveToPosition(exitPosition);
        }
        
        #endregion
        
        #region Shopping Methods
        
        // Add this to your SimpleTestCustomer class
        public ShelfSlot FindShelf()
        {
            // Try to find your existing ShelfSlot first
            ShelfSlot[] shelves = FindObjectsByType<ShelfSlot>(FindObjectsSortMode.None);
            
            if (shelves.Length > 0)
            {
                var shelvesWithProducts = new List<ShelfSlot>();
                foreach (var shelf in shelves)
                {
                    if (!shelf.IsEmpty && shelf.CurrentProduct != null)
                        shelvesWithProducts.Add(shelf);
                }
                
                ShelfSlot[] targetShelves = shelvesWithProducts.Count > 0 ? 
                    shelvesWithProducts.ToArray() : shelves;
                
                ShelfSlot selectedShelf = targetShelves[Random.Range(0, targetShelves.Length)];
                currentTargetShelf = selectedShelf;
                return selectedShelf;
            }
            
            // Fallback: Try TestShelfSlot
            TestShelfSlot[] testShelves = FindObjectsByType<TestShelfSlot>(FindObjectsSortMode.None);
            
            if (testShelves.Length > 0)
            {
                TestShelfSlot testShelf = testShelves[Random.Range(0, testShelves.Length)];
                // Note: This won't work with your existing system, you need adapter
                if (showDebugLogs)
                    Debug.Log($"Found test shelf: {testShelf.name}");
            }
            
            if (showDebugLogs)
                Debug.LogWarning("No shelves found in scene!");
            return null;
        }
        
        public bool TrySelectProduct()
        {
            if (currentTargetShelf == null)
            {
                if (showDebugLogs)
                    Debug.LogWarning("TestCustomer: No target shelf for product selection");
                return false;
            }
            
            return TrySelectProductAtShelf(currentTargetShelf);
        }
        
        public bool TrySelectProductAtShelf(ShelfSlot shelf)
        {
            if (shelf == null || shelf.IsEmpty || shelf.CurrentProduct == null)
            {
                if (showDebugLogs)
                    Debug.Log("TestCustomer: Shelf is empty or has no product");
                return false;
            }
            
            if (selectedProducts.Count >= maxProducts)
            {
                if (showDebugLogs)
                    Debug.Log("TestCustomer: Already have maximum products");
                return false;
            }
            
            Product product = shelf.CurrentProduct;
            
            // Check if can afford
            if (product.CurrentPrice > currentMoney)
            {
                if (showDebugLogs)
                    Debug.Log($"TestCustomer: Cannot afford {product.ProductData?.ProductName} (${product.CurrentPrice})");
                return false;
            }
            
            // Simple probability - 70% chance to buy
            if (Random.value > 0.7f)
            {
                if (showDebugLogs)
                    Debug.Log("TestCustomer: Not interested in product");
                return false;
            }
            
            // Select the product
            selectedProducts.Add(product);
            currentMoney -= product.CurrentPrice;
            
            // Remove from shelf
            Product removedProduct = shelf.RemoveProduct();
            if (removedProduct != null && removedProduct.IsOnShelf)
            {
                removedProduct.RemoveFromShelf();
            }
            
            if (showDebugLogs)
                Debug.Log($"TestCustomer ✅ SELECTED {product.ProductData?.ProductName} for ${product.CurrentPrice} (Remaining: ${currentMoney:F2})");
            
            return true;
        }
        
        public bool HasSelectedProducts()
        {
            return selectedProducts.Count > 0;
        }
        
        public void StartShopping()
        {
            shoppingStartTime = Time.time;
            if (showDebugLogs)
                Debug.Log("TestCustomer: Started shopping timer");
        }
        
        public bool HasFinishedShopping()
        {
            return Time.time - shoppingStartTime >= shoppingTime;
        }
        
        #endregion
        
        #region Store Integration
        
        public bool IsStoreOpen()
        {
            StoreHours storeHours = FindFirstObjectByType<StoreHours>();
            return storeHours?.IsStoreOpen ?? true; // Default to open if no store hours system
        }
        
        public bool IsStoreClosed()
        {
            return !IsStoreOpen();
        }
        
        #endregion
        
        #region Cleanup
        
        public void CleanupAndDestroy()
        {
            if (showDebugLogs)
                Debug.Log($"TestCustomer: Cleanup - Selected {selectedProducts.Count} products, Spent ${startingMoney - currentMoney:F2}");
            
            // Destroy any unpurchased products (they're leaving the store)
            foreach (Product product in selectedProducts)
            {
                if (product != null && !product.IsPurchased)
                {
                    if (showDebugLogs)
                        Debug.Log($"Destroying unpurchased product: {product.ProductData?.ProductName}");
                    Destroy(product.gameObject);
                }
            }
            
            selectedProducts.Clear();
            
            // Destroy customer after brief delay
            Destroy(gameObject, 1f);
        }
        
        #endregion
        
        #region Debug Methods
        
        [ContextMenu("Debug: Show Status")]
        public void DebugShowStatus()
        {
            Debug.Log($"=== TestCustomer Status ===");
            Debug.Log($"Money: ${currentMoney:F2}");
            Debug.Log($"Products: {selectedProducts.Count}/{maxProducts}");
            Debug.Log($"Target Shelf: {(currentTargetShelf ? currentTargetShelf.name : "None")}");
            Debug.Log($"Reached Destination: {HasReachedDestination()}");
            Debug.Log($"Store Open: {IsStoreOpen()}");
        }
        
        #endregion
    }
}