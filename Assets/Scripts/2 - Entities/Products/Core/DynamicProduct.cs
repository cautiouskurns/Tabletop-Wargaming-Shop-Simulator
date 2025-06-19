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

            // Update image zones with proper fitting
            if (mainImageMaterial != null && data.Icon != null)
            {
                mainImageMaterial.mainTexture = data.Icon.texture;
                // Option 1: Scale quad to fit (max 0.8 x 0.8 units)
                FitTextureInQuad(mainProductImage, data.Icon.texture, 0.6f, 0.6f);
                
                // OR Option 2: Keep quad size, scale texture
                // FitTextureInMaterial(mainImageMaterial, data.Icon.texture, 1f, 1f);
            }

            if (logoMaterial != null && data.IPLogo != null)
            {
                logoMaterial.mainTexture = data.IPLogo.texture;
                // Smaller max size for logos
                FitTextureInQuad(brandLogo, data.IPLogo.texture, 0.5f, 0.5f);
            }
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
        
        /// <summary>
        /// Scale the quad to fit the texture while preserving aspect ratio and staying within bounds
        /// </summary>
        /// <param name="meshRenderer">The quad's MeshRenderer</param>
        /// <param name="texture">The texture being applied</param>
        /// <param name="maxWidth">Maximum width the quad can be</param>
        /// <param name="maxHeight">Maximum height the quad can be</param>
        private void FitTextureInQuad(MeshRenderer meshRenderer, Texture texture, float maxWidth = 1f, float maxHeight = 1f)
        {
            if (meshRenderer == null || texture == null) return;
            
            // Get texture aspect ratio
            float textureAspect = (float)texture.width / texture.height;
            float quadAspect = maxWidth / maxHeight;
            
            float newWidth, newHeight;
            
            // Scale to fit within the maximum bounds
            if (textureAspect > quadAspect) // Image is wider relative to container
            {
                // Fit to width, scale height down
                newWidth = maxWidth;
                newHeight = maxWidth / textureAspect;
            }
            else // Image is taller relative to container
            {
                // Fit to height, scale width down
                newHeight = maxHeight;
                newWidth = maxHeight * textureAspect;
            }
            
            // Apply the scale
            meshRenderer.transform.localScale = new Vector3(newWidth, newHeight, 1f);
        }
    }
}