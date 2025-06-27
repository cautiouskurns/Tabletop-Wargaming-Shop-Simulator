using UnityEngine;

namespace TabletopShop
{
    /// <summary>
    /// Simple product for testing customer behavior
    /// </summary>
    public class TestProduct : MonoBehaviour
    {
        [Header("Product Info")]
        [SerializeField] private string productName;
        [SerializeField] private float price;
        [SerializeField] private bool isPurchased = false;
        [SerializeField] private bool isOnShelf = true;
        
        // Properties to match your existing Product interface
        public string ProductName => productName;
        public float Price => price;
        public float CurrentPrice => price;
        public bool IsPurchased => isPurchased;
        public bool IsOnShelf => isOnShelf;
        
        // Mock ProductData for compatibility
        public MockProductData ProductData => new MockProductData { ProductName = productName };
        
        public void Initialize(string name, float productPrice)
        {
            productName = name;
            price = productPrice;
            isPurchased = false;
            isOnShelf = true;
            
            gameObject.name = $"TestProduct_{name}";
        }
        
        public void Purchase()
        {
            isPurchased = true;
            Debug.Log($"Product purchased: {productName} for ${price:F2}");
        }
        
        public void RemoveFromShelf()
        {
            isOnShelf = false;
            Debug.Log($"Product removed from shelf: {productName}");
        }
        
        // Simple mock data class
        [System.Serializable]
        public class MockProductData
        {
            public string ProductName;
        }
    }
}