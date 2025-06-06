using UnityEngine;

namespace TabletopShop
{
    /// <summary>
    /// ScriptableObject that defines the data for a product in the tabletop shop
    /// </summary>
    [CreateAssetMenu(fileName = "New Product", menuName = "Tabletop Shop/Product Data")]
    public class ProductData : ScriptableObject
    {
        [Header("Product Information")]
        [SerializeField] private string productName;
        [SerializeField] private int basePrice;
        [SerializeField] private ProductType type;
        [SerializeField] private string description;
        
        [Header("Visual Assets")]
        [SerializeField] private Sprite icon;
        [SerializeField] private GameObject prefab;
        
        // Public properties for accessing the data
        public string ProductName => productName;
        public int BasePrice => basePrice;
        public ProductType Type => type;
        public string Description => description;
        public Sprite Icon => icon;
        public GameObject Prefab => prefab;
        
        // Method to get formatted price string
        public string GetFormattedPrice()
        {
            return $"${basePrice}";
        }
        
        // Method to get display name with type
        public string GetDisplayName()
        {
            return $"{productName} ({type})";
        }
        
        // Validation method to ensure data is properly set
        private void OnValidate()
        {
            if (basePrice < 0)
            {
                basePrice = 0;
            }
            
            if (string.IsNullOrEmpty(productName))
            {
                productName = "Unnamed Product";
            }
        }
    }
}
