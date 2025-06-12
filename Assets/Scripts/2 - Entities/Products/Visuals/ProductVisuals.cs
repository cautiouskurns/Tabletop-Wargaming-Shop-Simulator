using UnityEngine;

namespace TabletopShop
{
    /// <summary>
    /// Handles all visual effects and material management for products
    /// Manages hover effects, material creation, and visual state changes
    /// </summary>
    [RequireComponent(typeof(MeshRenderer))]
    public class ProductVisuals : MonoBehaviour
    {
        [Header("Visual Configuration")]
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color hoverColor = Color.yellow;
        [SerializeField] private float hoverIntensity = 1.5f;
        
        // Component references
        private Product product;
        private MeshRenderer meshRenderer;
        private Collider productCollider;
        
        // Material management
        private Material originalMaterial;
        private Material highlightMaterial;
        
        // State tracking
        private bool isHovering = false;
        
        // Events
        public System.Action OnHoverEnter;
        public System.Action OnHoverExit;
        public System.Action<bool> OnVisualStateChanged;
        
        #region Initialization
        
        private void Start()
        {
            // Get required components after all components are initialized
            product = GetComponent<Product>();
            meshRenderer = GetComponent<MeshRenderer>();
            productCollider = GetComponent<Collider>();
            
            // Validate components
            if (product == null)
            {
                Debug.LogError($"ProductVisuals on {name} requires a Product component!", this);
            }
            
            if (meshRenderer == null)
            {
                Debug.LogError($"ProductVisuals on {name} is missing MeshRenderer component!", this);
                return;
            }
            
            if (productCollider == null)
            {
                Debug.LogError($"ProductVisuals on {name} is missing Collider component!", this);
                return;
            }
            
            // Setup materials after all components are initialized
            SetupMaterials();
        }
        
        #endregion
        
        #region Public Visual Methods
        
        /// <summary>
        /// Initialize visual settings with custom parameters
        /// </summary>
        /// <param name="normalColor">Normal state color</param>
        /// <param name="hoverColor">Hover state color</param>
        /// <param name="hoverIntensity">Hover effect intensity</param>
        public void InitializeVisuals(Color? normalColor = null, Color? hoverColor = null, float? hoverIntensity = null)
        {
            if (normalColor.HasValue)
                this.normalColor = normalColor.Value;
            
            if (hoverColor.HasValue)
                this.hoverColor = hoverColor.Value;
            
            if (hoverIntensity.HasValue)
                this.hoverIntensity = hoverIntensity.Value;
            
            // Recreate materials with new settings
            SetupMaterials();
        }
        
        /// <summary>
        /// Apply visual hover effect
        /// </summary>
        public void ApplyHoverEffect()
        {
            if (product != null && (product.IsPurchased || !product.IsOnShelf))
                return;
            
            if (meshRenderer != null && highlightMaterial != null)
            {
                meshRenderer.material = highlightMaterial;
                isHovering = true;
                OnHoverEnter?.Invoke();
                OnVisualStateChanged?.Invoke(true);
                
                // Show product info
                string productInfo = product?.ProductData != null 
                    ? $"{product.ProductData.ProductName} - ${product.CurrentPrice}"
                    : $"Product - ${product?.CurrentPrice ?? 0}";
                
                Debug.Log($"Hovering over: {productInfo}");
            }
        }
        
        /// <summary>
        /// Remove visual hover effect
        /// </summary>
        public void RemoveHoverEffect()
        {
            if (product != null && (product.IsPurchased || !product.IsOnShelf))
                return;
            
            if (meshRenderer != null && originalMaterial != null)
            {
                meshRenderer.material = originalMaterial;
                isHovering = false;
                OnHoverExit?.Invoke();
                OnVisualStateChanged?.Invoke(false);
            }
        }
        
        /// <summary>
        /// Update visual state based on product status
        /// </summary>
        /// <param name="isVisible">Whether the product should be visible</param>
        /// <param name="hasCollision">Whether the product should have collision</param>
        public void UpdateVisualState(bool isVisible, bool hasCollision)
        {
            if (meshRenderer != null)
            {
                meshRenderer.enabled = isVisible;
            }
            
            if (productCollider != null)
            {
                productCollider.enabled = hasCollision;
            }
            
            // Remove hover effect if product is no longer visible
            if (!isVisible && isHovering)
            {
                RemoveHoverEffect();
            }
            
            OnVisualStateChanged?.Invoke(isVisible);
        }
        
        /// <summary>
        /// Get current visual state information
        /// </summary>
        /// <returns>Visual state description</returns>
        public string GetVisualState()
        {
            bool isVisible = meshRenderer != null && meshRenderer.enabled;
            bool hasCollision = productCollider != null && productCollider.enabled;
            
            return $"Visible: {isVisible}, Collision: {hasCollision}, Hovering: {isHovering}";
        }
        
        /// <summary>
        /// Force refresh of visual materials
        /// </summary>
        public void RefreshMaterials()
        {
            SetupMaterials();
            
            // Reapply current state
            if (isHovering)
            {
                ApplyHoverEffect();
            }
            else
            {
                RemoveHoverEffect();
            }
        }
        
        #endregion
        
        #region Private Visual Logic
        
        /// <summary>
        /// Setup materials for normal and highlight states
        /// </summary>
        private void SetupMaterials()
        {
            if (meshRenderer == null || meshRenderer.material == null)
                return;
            
            // Store reference to original material
            originalMaterial = meshRenderer.material;
            
            // Create highlight material using MaterialUtility
            if (highlightMaterial != null)
            {
                MaterialUtility.SafeDestroyMaterial(highlightMaterial, !Application.isPlaying);
            }
            
            highlightMaterial = MaterialUtility.CreateEmissiveMaterial(originalMaterial, hoverColor, hoverIntensity);
            
            Debug.Log($"Materials setup for {product?.ProductData?.ProductName ?? name}");
        }
        
        #endregion
        
        #region Product State Integration
        
        /// <summary>
        /// Handle product purchase visual changes
        /// </summary>
        public void OnProductPurchased()
        {
            // Disable visual and collision when purchased
            UpdateVisualState(false, false);
            Debug.Log($"Product visuals disabled for purchased item: {product?.ProductData?.ProductName ?? name}");
        }
        
        /// <summary>
        /// Handle product placement on shelf visual changes
        /// </summary>
        public void OnProductPlacedOnShelf()
        {
            // Ensure visual and collision are enabled when placed on shelf
            UpdateVisualState(true, true);
            Debug.Log($"Product visuals enabled for shelf placement: {product?.ProductData?.ProductName ?? name}");
        }
        
        /// <summary>
        /// Handle product removal from shelf visual changes
        /// </summary>
        public void OnProductRemovedFromShelf()
        {
            // Product visuals remain enabled when removed from shelf (for inventory, etc.)
            // But remove any hover effects
            if (isHovering)
            {
                RemoveHoverEffect();
            }
            
            Debug.Log($"Product visuals updated for shelf removal: {product?.ProductData?.ProductName ?? name}");
        }
        
        #endregion
        
        #region Material Validation
        
        /// <summary>
        /// Validate that materials are properly set up
        /// </summary>
        /// <returns>True if materials are valid</returns>
        public bool ValidateMaterials()
        {
            bool isValid = originalMaterial != null && highlightMaterial != null;
            
            if (!isValid)
            {
                Debug.LogWarning($"Material validation failed for {product?.ProductData?.ProductName ?? name}. Attempting to recreate materials.");
                SetupMaterials();
                isValid = originalMaterial != null && highlightMaterial != null;
            }
            
            return isValid;
        }
        
        #endregion
        
        #region Editor Support
        
        /// <summary>
        /// Validation in editor
        /// </summary>
        private void OnValidate()
        {
            // Ensure hover intensity is positive
            if (hoverIntensity < 0)
                hoverIntensity = 1.0f;
            
            // Refresh materials if playing and they exist
            if (Application.isPlaying && originalMaterial != null)
            {
                SetupMaterials();
            }
        }
        
        #endregion
        
        #region Cleanup
        
        private void OnDestroy()
        {
            // Clean up created materials to prevent memory leaks using MaterialUtility
            MaterialUtility.SafeDestroyMaterial(highlightMaterial, !Application.isPlaying);
        }
        
        #endregion
        
        #region Testing
        
        /// <summary>
        /// Test visual effects (for development/testing)
        /// </summary>
        [ContextMenu("Test Visual Effects")]
        public void TestVisualEffects()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("Visual effects test requires Play mode");
                return;
            }
            
            Debug.Log("=== TESTING PRODUCT VISUAL EFFECTS ===");
            Debug.Log($"Product: {product?.ProductData?.ProductName ?? name}");
            Debug.Log($"Visual State: {GetVisualState()}");
            Debug.Log($"Materials Valid: {ValidateMaterials()}");
            
            // Test hover effect
            Debug.Log("Testing hover effect...");
            ApplyHoverEffect();
            
            // Wait a moment then remove
            Invoke(nameof(TestRemoveHover), 2f);
        }
        
        private void TestRemoveHover()
        {
            Debug.Log("Removing hover effect...");
            RemoveHoverEffect();
            Debug.Log("✅ Visual effects test completed!");
        }
        
        #endregion
    }
}
