using UnityEngine;
using System.Collections.Generic;

namespace TabletopShop
{
    /// <summary>
    /// Simple shelf for testing customer behavior
    /// </summary>
    public class TestShelfSlot : MonoBehaviour
    {
        [Header("Shelf Configuration")]
        [SerializeField] private bool isEmpty = false;
        [SerializeField] private TestProduct currentProduct;
        
        [Header("Auto-Generate Product")]
        [SerializeField] private bool autoGenerateProduct = true;
        [SerializeField] private string[] productNames = {"Runeblade Miniature", "Void Crystal", "Battle Dice"};
        [SerializeField] private float minPrice = 10f;
        [SerializeField] private float maxPrice = 50f;
        
        public bool IsEmpty => isEmpty || currentProduct == null;
        public TestProduct CurrentProduct => currentProduct;
        
        private void Start()
        {
            if (autoGenerateProduct && currentProduct == null)
            {
                GenerateRandomProduct();
            }
        }
        
        public void GenerateRandomProduct()
        {
            // Create product GameObject
            GameObject productObj = new GameObject("TestProduct");
            productObj.transform.SetParent(this.transform);
            productObj.transform.localPosition = Vector3.up * 0.5f; // Slightly above shelf
            
            // Add visual (simple cube)
            var renderer = productObj.AddComponent<MeshRenderer>();
            var filter = productObj.AddComponent<MeshFilter>();
            filter.mesh = Resources.GetBuiltinResource<Mesh>("Cube.fbx");
            
            // Random color for visual distinction
            var material = new Material(Shader.Find("Standard"));
            material.color = new Color(Random.value, Random.value, Random.value);
            renderer.material = material;
            
            // Scale it down
            productObj.transform.localScale = Vector3.one * 0.3f;
            
            // Add TestProduct component
            currentProduct = productObj.AddComponent<TestProduct>();
            currentProduct.Initialize(
                productNames[Random.Range(0, productNames.Length)],
                Random.Range(minPrice, maxPrice)
            );
            
            isEmpty = false;
            
            Debug.Log($"Generated product: {currentProduct.ProductName} for ${currentProduct.Price:F2}");
        }
        
        public TestProduct RemoveProduct()
        {
            if (currentProduct != null)
            {
                TestProduct removedProduct = currentProduct;
                currentProduct = null;
                isEmpty = true;
                
                Debug.Log($"Removed product from shelf: {removedProduct.ProductName}");
                return removedProduct;
            }
            return null;
        }
        
        [ContextMenu("Generate New Product")]
        public void ContextGenerateProduct()
        {
            if (currentProduct != null)
            {
                DestroyImmediate(currentProduct.gameObject);
            }
            GenerateRandomProduct();
        }
    }
}