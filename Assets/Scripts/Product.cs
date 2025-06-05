using UnityEngine;

namespace TabletopShop
{
    /// <summary>
    /// MonoBehaviour component that handles individual product instances in the game world
    /// Manages product interactions, visual feedback, and state management
    /// </summary>
    [RequireComponent(typeof(MeshRenderer), typeof(Collider))]
    public class Product : MonoBehaviour
    {
        [Header("Product Configuration")]
        [SerializeField] private ProductData productData;
        [SerializeField] private int currentPrice;
        [SerializeField] private bool isOnShelf = false;
        [SerializeField] private bool isPurchased = false;
        
        [Header("Visual Feedback")]
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color hoverColor = Color.yellow;
        [SerializeField] private float hoverIntensity = 1.5f;
        
        // Component references
        private MeshRenderer meshRenderer;
        private Collider productCollider;
        private Material originalMaterial;
        private Material highlightMaterial;
        
        // Properties
        public ProductData ProductData => productData;
        public int CurrentPrice => currentPrice;
        public bool IsOnShelf => isOnShelf;
        public bool IsPurchased => isPurchased;
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            // Get required components
            meshRenderer = GetComponent<MeshRenderer>();
            productCollider = GetComponent<Collider>();
            
            // Validate components
            if (meshRenderer == null)
            {
                Debug.LogError($"Product {name} is missing MeshRenderer component!", this);
                return;
            }
            
            if (productCollider == null)
            {
                Debug.LogError($"Product {name} is missing Collider component!", this);
                return;
            }
            
            // Store original material and create highlight material
            SetupMaterials();
        }
        
        private void Start()
        {
            // Initialize with ProductData if available
            if (productData != null)
            {
                Initialize(productData);
            }
        }
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// Initialize the product with data from a ProductData ScriptableObject
        /// </summary>
        /// <param name="data">The ProductData to initialize from</param>
        public void Initialize(ProductData data)
        {
            if (data == null)
            {
                Debug.LogError($"Cannot initialize Product {name} with null ProductData!", this);
                return;
            }
            
            productData = data;
            currentPrice = data.BasePrice;
            
            // Update the GameObject name to match the product
            gameObject.name = $"Product_{data.ProductName.Replace(" ", "_")}";
            
            Debug.Log($"Initialized product: {data.ProductName} with price ${currentPrice}");
        }
        
        /// <summary>
        /// Set a new price for this product instance
        /// </summary>
        /// <param name="newPrice">The new price to set</param>
        public void SetPrice(int newPrice)
        {
            if (newPrice < 0)
            {
                Debug.LogWarning($"Cannot set negative price for {productData?.ProductName ?? name}. Price remains ${currentPrice}");
                return;
            }
            
            int oldPrice = currentPrice;
            currentPrice = newPrice;
            
            Debug.Log($"Price changed for {productData?.ProductName ?? name}: ${oldPrice} → ${currentPrice}");
        }
        
        /// <summary>
        /// Handle product purchase
        /// </summary>
        public void Purchase()
        {
            if (isPurchased)
            {
                Debug.LogWarning($"Product {productData?.ProductName ?? name} is already purchased!");
                return;
            }
            
            if (!isOnShelf)
            {
                Debug.LogWarning($"Cannot purchase {productData?.ProductName ?? name} - not on shelf!");
                return;
            }
            
            isPurchased = true;
            isOnShelf = false;
            
            // Disable visual and collision
            if (meshRenderer != null)
                meshRenderer.enabled = false;
            
            if (productCollider != null)
                productCollider.enabled = false;
            
            Debug.Log($"PURCHASED: {productData?.ProductName ?? name} for ${currentPrice}!");
            
            // TODO: Add money to player inventory
            // TODO: Trigger purchase effects (sound, particles, etc.)
        }
        
        /// <summary>
        /// Remove product from shelf (for restocking or management)
        /// </summary>
        public void RemoveFromShelf()
        {
            if (!isOnShelf)
            {
                Debug.LogWarning($"Product {productData?.ProductName ?? name} is not on shelf!");
                return;
            }
            
            isOnShelf = false;
            
            Debug.Log($"Removed {productData?.ProductName ?? name} from shelf");
            
            // TODO: Return to inventory or destroy
        }
        
        /// <summary>
        /// Place product on shelf
        /// </summary>
        public void PlaceOnShelf()
        {
            if (isOnShelf)
            {
                Debug.LogWarning($"Product {productData?.ProductName ?? name} is already on shelf!");
                return;
            }
            
            if (isPurchased)
            {
                Debug.LogWarning($"Cannot place purchased product {productData?.ProductName ?? name} on shelf!");
                return;
            }
            
            isOnShelf = true;
            
            // Ensure visual and collision are enabled
            if (meshRenderer != null)
                meshRenderer.enabled = true;
            
            if (productCollider != null)
                productCollider.enabled = true;
            
            Debug.Log($"Placed {productData?.ProductName ?? name} on shelf with price ${currentPrice}");
        }
        
        #endregion
        
        #region Mouse Interactions
        
        /// <summary>
        /// Handle mouse click on product
        /// </summary>
        private void OnMouseDown()
        {
            if (isPurchased)
            {
                Debug.Log($"Product {productData?.ProductName ?? name} is already purchased!");
                return;
            }
            
            if (!isOnShelf)
            {
                Debug.Log($"Product {productData?.ProductName ?? name} is not available for purchase (not on shelf)");
                return;
            }
            
            // Simulate customer purchase
            Debug.Log($"Customer clicked on {productData?.ProductName ?? name} (${currentPrice})");
            Purchase();
        }
        
        /// <summary>
        /// Handle mouse enter for hover effect
        /// </summary>
        private void OnMouseEnter()
        {
            if (isPurchased || !isOnShelf)
                return;
            
            ApplyHoverEffect();
            
            // Show product info
            string productInfo = productData != null 
                ? $"{productData.ProductName} - ${currentPrice}"
                : $"Product - ${currentPrice}";
            
            Debug.Log($"Hovering over: {productInfo}");
        }
        
        /// <summary>
        /// Handle mouse exit to remove hover effect
        /// </summary>
        private void OnMouseExit()
        {
            if (isPurchased || !isOnShelf)
                return;
            
            RemoveHoverEffect();
        }
        
        #endregion
        
        #region Visual Effects
        
        /// <summary>
        /// Setup materials for normal and highlight states
        /// </summary>
        private void SetupMaterials()
        {
            if (meshRenderer == null || meshRenderer.material == null)
                return;
            
            // Store reference to original material
            originalMaterial = meshRenderer.material;
            
            // Create highlight material
            highlightMaterial = new Material(originalMaterial);
            highlightMaterial.color = hoverColor;
            
            // Make highlight material emissive for better visibility
            if (highlightMaterial.HasProperty("_EmissionColor"))
            {
                highlightMaterial.EnableKeyword("_EMISSION");
                highlightMaterial.SetColor("_EmissionColor", hoverColor * hoverIntensity);
            }
        }
        
        /// <summary>
        /// Apply visual hover effect
        /// </summary>
        private void ApplyHoverEffect()
        {
            if (meshRenderer != null && highlightMaterial != null)
            {
                meshRenderer.material = highlightMaterial;
            }
        }
        
        /// <summary>
        /// Remove visual hover effect
        /// </summary>
        private void RemoveHoverEffect()
        {
            if (meshRenderer != null && originalMaterial != null)
            {
                meshRenderer.material = originalMaterial;
            }
        }
        
        #endregion
        
        #region Editor Support
        
        /// <summary>
        /// Validation in editor
        /// </summary>
        private void OnValidate()
        {
            if (currentPrice < 0)
            {
                currentPrice = 0;
            }
            
            // Sync price with ProductData if available
            if (productData != null && currentPrice == 0)
            {
                currentPrice = productData.BasePrice;
            }
        }
        
        #endregion
        
        #region Cleanup
        
        private void OnDestroy()
        {
            // Clean up created materials to prevent memory leaks
            if (highlightMaterial != null)
            {
                if (Application.isPlaying)
                    Destroy(highlightMaterial);
                else
                    DestroyImmediate(highlightMaterial);
            }
        }
        
        #endregion
    }
}
