using UnityEngine;
using TMPro;

namespace TabletopShop
{
    /// <summary>
    /// Dynamic visual zones for product display - integrates with existing Product system
    /// Handles modular text and image zones while working with ProductData SO
    /// </summary>
    public class DynamicProduct : MonoBehaviour
    {
        [Header("Zone References")]
        [SerializeField] private TextMeshPro productNameText;
        [SerializeField] private TextMeshPro priceText;
        [SerializeField] private TextMeshPro descriptionText;
        [SerializeField] private MeshRenderer mainProductImage;
        [SerializeField] private MeshRenderer brandLogo;

        [Header("Material Templates")]
        [SerializeField] private Material imageMaterialTemplate;

        // Reference to main Product component
        private Product parentProduct;
        private Material mainImageMaterial;
        private Material logoMaterial;

        private void Awake()
        {
            parentProduct = GetComponent<Product>();
            InitializeMaterials();
        }

        private void Start()
        {
            // Subscribe to Product events
            if (parentProduct != null)
            {
                // Listen for when ProductData is set
                UpdateFromProductData();
            }
        }

        /// <summary>
        /// Initialize materials for dynamic image zones
        /// Creates instances so multiple products don't share the same material
        /// </summary>
        private void InitializeMaterials()
        {
            // Create material instances for each image zone
            if (mainProductImage != null && imageMaterialTemplate != null)
            {
                mainImageMaterial = new Material(imageMaterialTemplate);
                mainProductImage.material = mainImageMaterial;
            }
            
            if (brandLogo != null && imageMaterialTemplate != null)
            {
                logoMaterial = new Material(imageMaterialTemplate);
                brandLogo.material = logoMaterial;
            }
        }

        /// <summary>
        /// Update all zones from the parent Product's ProductData
        /// </summary>
        public void UpdateFromProductData()
        {
            if (parentProduct?.ProductData == null) return;

            var data = parentProduct.ProductData;

            // Update text zones
            if (productNameText != null)
                productNameText.text = data.ProductName;

            if (priceText != null)
                priceText.text = $"${parentProduct.CurrentPrice:F2}";

            if (descriptionText != null)
                descriptionText.text = data.Description;

            // Update image zones
            // Main product image
            if (mainImageMaterial != null && data.Icon != null)
                mainImageMaterial.mainTexture = data.Icon.texture;

            // NEW: IP/Brand logo
            if (logoMaterial != null && data.IPLogo != null)
                logoMaterial.mainTexture = data.IPLogo.texture;
        }

        /// <summary>
        /// Override specific zones with custom content
        /// </summary>
        public void SetCustomText(string zoneName, string text)
        {
            switch (zoneName.ToLower())
            {
                case "name":
                    if (productNameText != null) productNameText.text = text;
                    break;
                case "price":
                    if (priceText != null) priceText.text = text;
                    break;
                case "description":
                    if (descriptionText != null) descriptionText.text = text;
                    break;
            }
        }

        public void SetCustomImage(string zoneName, Texture2D texture)
        {
            switch (zoneName.ToLower())
            {
                case "main":
                    if (mainImageMaterial != null) mainImageMaterial.mainTexture = texture;
                    break;
                case "logo":
                case "ip":
                case "brand":
                    if (logoMaterial != null) logoMaterial.mainTexture = texture;
                    break;
            }
        }

        // NEW: Method to set IP logo specifically
        public void SetIPLogo(Sprite ipLogo)
        {
            if (logoMaterial != null && ipLogo != null)
                logoMaterial.mainTexture = ipLogo.texture;
        }
    }
}