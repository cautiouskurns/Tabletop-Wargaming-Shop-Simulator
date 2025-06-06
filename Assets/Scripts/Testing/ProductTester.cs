using UnityEngine;

namespace TabletopShop
{
    /// <summary>
    /// Simple test script to validate Product functionality
    /// Attach to any GameObject in the scene to run tests
    /// </summary>
    public class ProductTester : MonoBehaviour
    {
        [Header("Test Configuration")]
        [SerializeField] private ProductData testProductData;
        [SerializeField] private GameObject productPrefab;
        
        [Header("Test Controls")]
        [SerializeField] private KeyCode spawnTestProductKey = KeyCode.T;
        [SerializeField] private KeyCode runBasicTestsKey = KeyCode.R;
        
        private Product spawnedProduct;
        
        private void Update()
        {
            if (Input.GetKeyDown(spawnTestProductKey))
            {
                SpawnTestProduct();
            }
            
            if (Input.GetKeyDown(runBasicTestsKey))
            {
                RunBasicTests();
            }
        }
        
        private void SpawnTestProduct()
        {
            if (productPrefab == null)
            {
                Debug.LogWarning("No product prefab assigned for testing!");
                return;
            }
            
            // Clean up previous test product
            if (spawnedProduct != null)
            {
                DestroyImmediate(spawnedProduct.gameObject);
            }
            
            // Spawn new test product
            Vector3 spawnPosition = transform.position + transform.forward * 2f + Vector3.up;
            GameObject spawnedGO = Instantiate(productPrefab, spawnPosition, Quaternion.identity);
            spawnedProduct = spawnedGO.GetComponent<Product>();
            
            if (spawnedProduct == null)
            {
                Debug.LogError("Spawned prefab doesn't have Product component!");
                return;
            }
            
            // Initialize with test data if available
            if (testProductData != null)
            {
                spawnedProduct.Initialize(testProductData);
            }
            
            // Place on shelf for testing
            spawnedProduct.PlaceOnShelf();
            
            Debug.Log($"Spawned test product: {spawnedProduct.ProductData?.ProductName ?? "Unknown"} at {spawnPosition}");
            Debug.Log("Click on the product to test purchase interaction!");
            Debug.Log("Hover over the product to test visual feedback!");
        }
        
        private void RunBasicTests()
        {
            if (spawnedProduct == null)
            {
                Debug.LogWarning("No spawned product to test! Press T to spawn one first.");
                return;
            }
            
            Debug.Log("=== Running Product Tests ===");
            
            // Test 1: Price setting
            Debug.Log("Test 1: Price Setting");
            int originalPrice = spawnedProduct.CurrentPrice;
            spawnedProduct.SetPrice(99);
            Debug.Log($"Price changed from ${originalPrice} to ${spawnedProduct.CurrentPrice}");
            
            // Test 2: Invalid price
            Debug.Log("Test 2: Invalid Price (should show warning)");
            spawnedProduct.SetPrice(-5);
            
            // Test 3: State checks
            Debug.Log("Test 3: State Validation");
            Debug.Log($"Is on shelf: {spawnedProduct.IsOnShelf}");
            Debug.Log($"Is purchased: {spawnedProduct.IsPurchased}");
            
            // Test 4: Remove from shelf
            Debug.Log("Test 4: Remove from Shelf");
            spawnedProduct.RemoveFromShelf();
            Debug.Log($"After removal - Is on shelf: {spawnedProduct.IsOnShelf}");
            
            // Test 5: Put back on shelf
            Debug.Log("Test 5: Place Back on Shelf");
            spawnedProduct.PlaceOnShelf();
            Debug.Log($"After placement - Is on shelf: {spawnedProduct.IsOnShelf}");
            
            Debug.Log("=== Tests Complete ===");
        }
        
        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10, 10, 300, 150));
            GUILayout.Label("Product Testing Controls:");
            GUILayout.Label($"Press '{spawnTestProductKey}' to spawn test product");
            GUILayout.Label($"Press '{runBasicTestsKey}' to run basic tests");
            GUILayout.Label("Click on spawned product to test purchase");
            GUILayout.Label("Hover over product to test visual feedback");
            
            if (spawnedProduct != null)
            {
                GUILayout.Space(10);
                GUILayout.Label($"Current Product: {spawnedProduct.ProductData?.ProductName ?? "Unknown"}");
                GUILayout.Label($"Price: ${spawnedProduct.CurrentPrice}");
                GUILayout.Label($"On Shelf: {spawnedProduct.IsOnShelf}");
                GUILayout.Label($"Purchased: {spawnedProduct.IsPurchased}");
            }
            
            GUILayout.EndArea();
        }
    }
}
