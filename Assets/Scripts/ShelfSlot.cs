using UnityEngine;

namespace TabletopShop
{
    /// <summary>
    /// Individual slot on a shelf that can hold one product
    /// Handles product placement, visual feedback, and player interactions
    /// </summary>
    public class ShelfSlot : MonoBehaviour
    {
        [Header("Slot Configuration")]
        [SerializeField] private Vector3 slotPosition;
        [SerializeField] private Product currentProduct;
        
        [Header("Visual Feedback")]
        [SerializeField] private Material highlightMaterial;
        [SerializeField] private Material normalMaterial;
        [SerializeField] private Color emptySlotColor = Color.green;
        [SerializeField] private Color highlightColor = Color.yellow;
        
        [Header("Slot Indicator")]
        [SerializeField] private GameObject slotIndicator;
        [SerializeField] private Vector3 indicatorScale = new Vector3(0.8f, 0.1f, 0.8f);
        
        // Component references
        private MeshRenderer indicatorRenderer;
        private Collider slotCollider;
        
        // Properties
        public bool IsEmpty => currentProduct == null;
        public Product CurrentProduct => currentProduct;
        public Vector3 SlotPosition => transform.position + slotPosition;
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            SetupSlotIndicator();
            SetupMaterials();
            
            // Get collider for mouse interactions
            slotCollider = GetComponent<Collider>();
            if (slotCollider == null)
            {
                // Add a collider if none exists
                slotCollider = gameObject.AddComponent<BoxCollider>();
                BoxCollider boxCollider = slotCollider as BoxCollider;
                boxCollider.size = new Vector3(1f, 0.5f, 1f);
                boxCollider.isTrigger = true;
            }
        }
        
        private void Start()
        {
            UpdateVisualState();
        }
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// Place a product in this slot
        /// </summary>
        /// <param name="product">The product to place</param>
        /// <returns>True if product was successfully placed</returns>
        public bool PlaceProduct(Product product)
        {
            if (product == null)
            {
                Debug.LogWarning($"Cannot place null product in slot {name}");
                return false;
            }
            
            if (!IsEmpty)
            {
                Debug.LogWarning($"Slot {name} is already occupied by {currentProduct.ProductData?.ProductName}");
                return false;
            }
            
            // Set the product reference
            currentProduct = product;
            
            // Position the product at the slot position
            product.transform.position = SlotPosition;
            product.transform.rotation = transform.rotation;
            
            // Make sure product is on shelf
            product.PlaceOnShelf();
            
            // Update visual state
            UpdateVisualState();
            
            Debug.Log($"Placed {product.ProductData?.ProductName ?? "Product"} in slot {name}");
            return true;
        }
        
        /// <summary>
        /// Remove the product from this slot
        /// </summary>
        /// <returns>The removed product, or null if slot was empty</returns>
        public Product RemoveProduct()
        {
            if (IsEmpty)
            {
                Debug.LogWarning($"Slot {name} is already empty");
                return null;
            }
            
            Product removedProduct = currentProduct;
            currentProduct = null;
            
            // Remove product from shelf
            removedProduct.RemoveFromShelf();
            
            // Update visual state
            UpdateVisualState();
            
            Debug.Log($"Removed {removedProduct.ProductData?.ProductName ?? "Product"} from slot {name}");
            return removedProduct;
        }
        
        /// <summary>
        /// Clear the slot without returning the product (for cleanup)
        /// </summary>
        public void ClearSlot()
        {
            if (!IsEmpty)
            {
                currentProduct = null;
                UpdateVisualState();
            }
        }
        
        /// <summary>
        /// Set the local position offset for this slot
        /// </summary>
        /// <param name="position">Local position offset from slot transform</param>
        public void SetSlotPosition(Vector3 position)
        {
            slotPosition = position;
        }
        
        #endregion
        
        #region Mouse Interactions
        
        /// <summary>
        /// Handle mouse click on slot - for player stocking interactions
        /// </summary>
        private void OnMouseDown()
        {
            if (IsEmpty)
            {
                Debug.Log($"Clicked on empty slot {name} - ready for product placement");
                // TODO: Integrate with inventory system for player stocking
                // For now, just log the interaction
            }
            else
            {
                Debug.Log($"Clicked on slot {name} containing {currentProduct.ProductData?.ProductName}");
                // Could allow removal or product management here
            }
        }
        
        /// <summary>
        /// Handle mouse enter for hover highlighting
        /// </summary>
        private void OnMouseEnter()
        {
            ApplyHighlight();
            
            if (IsEmpty)
            {
                Debug.Log($"Hovering over empty slot {name}");
            }
            else
            {
                Debug.Log($"Hovering over slot {name} with {currentProduct.ProductData?.ProductName}");
            }
        }
        
        /// <summary>
        /// Handle mouse exit to remove highlighting
        /// </summary>
        private void OnMouseExit()
        {
            RemoveHighlight();
        }
        
        #endregion
        
        #region Visual Management
        
        /// <summary>
        /// Setup the visual indicator for the slot
        /// </summary>
        private void SetupSlotIndicator()
        {
            // Create slot indicator if it doesn't exist
            if (slotIndicator == null)
            {
                slotIndicator = GameObject.CreatePrimitive(PrimitiveType.Cube);
                slotIndicator.name = $"{name}_Indicator";
                slotIndicator.transform.SetParent(transform, false);
                
                // Position and scale the indicator
                slotIndicator.transform.localPosition = slotPosition;
                slotIndicator.transform.localScale = indicatorScale;
                
                // Remove collider from indicator (we want slot collider to handle interactions)
                Collider indicatorCollider = slotIndicator.GetComponent<Collider>();
                if (indicatorCollider != null)
                {
                    DestroyImmediate(indicatorCollider);
                }
            }
            
            // Get renderer for material changes
            indicatorRenderer = slotIndicator.GetComponent<MeshRenderer>();
        }
        
        /// <summary>
        /// Setup materials for normal and highlight states
        /// </summary>
        private void SetupMaterials()
        {
            if (indicatorRenderer == null) return;
            
            // Create normal material if not assigned
            if (normalMaterial == null)
            {
                normalMaterial = new Material(Shader.Find("Standard"));
                normalMaterial.color = emptySlotColor;
                normalMaterial.SetFloat("_Metallic", 0f);
                normalMaterial.SetFloat("_Glossiness", 0.3f);
            }
            
            // Create highlight material if not assigned
            if (highlightMaterial == null)
            {
                highlightMaterial = new Material(Shader.Find("Standard"));
                highlightMaterial.color = highlightColor;
                highlightMaterial.SetFloat("_Metallic", 0f);
                highlightMaterial.SetFloat("_Glossiness", 0.5f);
                
                // Add emission for better visibility
                highlightMaterial.EnableKeyword("_EMISSION");
                highlightMaterial.SetColor("_EmissionColor", highlightColor * 0.3f);
            }
        }
        
        /// <summary>
        /// Update visual state based on slot contents
        /// </summary>
        private void UpdateVisualState()
        {
            if (slotIndicator == null) return;
            
            // Show indicator only when slot is empty
            slotIndicator.SetActive(IsEmpty);
            
            // Apply normal material when not highlighted
            if (IsEmpty && indicatorRenderer != null && normalMaterial != null)
            {
                indicatorRenderer.material = normalMaterial;
            }
        }
        
        /// <summary>
        /// Apply highlight effect
        /// </summary>
        private void ApplyHighlight()
        {
            if (IsEmpty && indicatorRenderer != null && highlightMaterial != null)
            {
                indicatorRenderer.material = highlightMaterial;
            }
        }
        
        /// <summary>
        /// Remove highlight effect
        /// </summary>
        private void RemoveHighlight()
        {
            if (IsEmpty && indicatorRenderer != null && normalMaterial != null)
            {
                indicatorRenderer.material = normalMaterial;
            }
        }
        
        #endregion
        
        #region Editor Support
        
        /// <summary>
        /// Draw gizmos in editor for slot position visualization
        /// </summary>
        private void OnDrawGizmos()
        {
            // Draw slot position
            Gizmos.color = IsEmpty ? Color.green : Color.red;
            Gizmos.DrawWireCube(SlotPosition, Vector3.one * 0.5f);
            
            // Draw slot number/name
            #if UNITY_EDITOR
            UnityEditor.Handles.Label(SlotPosition + Vector3.up * 0.7f, name);
            #endif
        }
        
        /// <summary>
        /// Validation in editor
        /// </summary>
        private void OnValidate()
        {
            // Update indicator position if changed in editor
            if (slotIndicator != null)
            {
                slotIndicator.transform.localPosition = slotPosition;
            }
        }
        
        #endregion
        
        #region Cleanup
        
        private void OnDestroy()
        {
            // Clean up created materials
            if (Application.isPlaying)
            {
                if (normalMaterial != null) Destroy(normalMaterial);
                if (highlightMaterial != null) Destroy(highlightMaterial);
            }
            else
            {
                if (normalMaterial != null) DestroyImmediate(normalMaterial);
                if (highlightMaterial != null) DestroyImmediate(highlightMaterial);
            }
        }
        
        #endregion
    }
}
