using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TabletopShop
{
    /// <summary>
    /// Individual slot on a shelf that can hold one product
    /// Handles product placement, visual feedback, and player interactions
    /// </summary>
    public class ShelfSlot : MonoBehaviour, IInteractable
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
        
        // IInteractable Properties
        public string InteractionText => IsEmpty ? GetPlacementText() : $"Remove {currentProduct.ProductData?.ProductName ?? "Product"}";
        public bool CanInteract => true;
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            SetupSlotIndicator(); // This now calls SetupMaterials() internally
            
            // Set layer for interaction system
            InteractionLayers.SetShelfLayer(gameObject);
            
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
        
        #region IInteractable Implementation
        
        /// <summary>
        /// Handle player interaction with this slot
        /// </summary>
        /// <param name="player">The player GameObject</param>
        public void Interact(GameObject player)
        {
            if (IsEmpty)
            {
                // Try to place selected product from inventory
                PlaceProductFromInventory();
            }
            else
            {
                Debug.Log($"Player interacted with slot {name} - removing {currentProduct.ProductData?.ProductName}");
                // Remove product and add back to inventory
                Product removedProduct = RemoveProduct();
                if (removedProduct != null)
                {
                    // Add product back to inventory
                    if (removedProduct.ProductData != null)
                    {
                        InventoryManager.Instance.AddProduct(removedProduct.ProductData, 1);
                        Debug.Log($"Added {removedProduct.ProductData.ProductName} back to inventory");
                    }
                    
                    // Destroy the visual product object
                    Destroy(removedProduct.gameObject);
                }
            }
        }
        
        /// <summary>
        /// Attempt to place the currently selected product from inventory
        /// </summary>
        private void PlaceProductFromInventory()
        {
            var inventory = InventoryManager.Instance;
            if (inventory == null)
            {
                Debug.LogWarning("InventoryManager not found!");
                return;
            }
            
            ProductData selectedProduct = inventory.SelectedProduct;
            if (selectedProduct == null)
            {
                Debug.Log("No product selected in inventory");
                StartCoroutine(InteractionFeedback());
                return;
            }
            
            if (!inventory.HasProduct(selectedProduct, 1))
            {
                Debug.Log($"Not enough {selectedProduct.ProductName} in inventory");
                StartCoroutine(InteractionFeedback());
                return;
            }
            
            // Remove product from inventory
            bool removed = inventory.RemoveProduct(selectedProduct, 1);
            if (!removed)
            {
                Debug.LogWarning($"Failed to remove {selectedProduct.ProductName} from inventory");
                return;
            }
            
            // Create and place product on shelf
            CreateAndPlaceProduct(selectedProduct);
            
            Debug.Log($"Placed {selectedProduct.ProductName} from inventory onto shelf. Remaining: {inventory.GetProductCount(selectedProduct)}");
        }
        
        /// <summary>
        /// Create a product GameObject and place it in this slot
        /// </summary>
        /// <param name="productData">Product data to create</param>
        private void CreateAndPlaceProduct(ProductData productData)
        {
            GameObject productObject;
            
            // Use the prefab if available, otherwise create a basic cube
            if (productData.Prefab != null)
            {
                // Instantiate the actual prefab
                productObject = Instantiate(productData.Prefab);
                productObject.name = $"Product_{productData.ProductName}";
                productObject.transform.position = SlotPosition;
                productObject.transform.SetParent(transform);
            }
            else
            {
                // Fallback to cube if no prefab is set
                productObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                productObject.name = $"Product_{productData.ProductName}";
                productObject.transform.position = SlotPosition;
                productObject.transform.SetParent(transform);
                productObject.transform.localScale = Vector3.one * 0.8f;
                
                // Color the fallback cube based on product data
                MeshRenderer renderer = productObject.GetComponent<MeshRenderer>();
                if (renderer != null)
                {
                    Material productMaterial = new Material(Shader.Find("Standard"));
                    productMaterial.color = GetProductColor(productData);
                    renderer.material = productMaterial;
                }
            }
            
            // Set product layer for interaction
            InteractionLayers.SetProductLayer(productObject);
            
            // Add Product component if it doesn't already exist (prefabs might already have one)
            Product product = productObject.GetComponent<Product>();
            if (product == null)
            {
                product = productObject.AddComponent<Product>();
            }
            product.Initialize(productData);
            
            // Ensure the object has required components for Product component
            if (productObject.GetComponent<MeshRenderer>() == null)
            {
                Debug.LogWarning($"Product prefab {productData.ProductName} is missing MeshRenderer component");
            }
            if (productObject.GetComponent<Collider>() == null)
            {
                Debug.LogWarning($"Product prefab {productData.ProductName} is missing Collider component");
                // Add a basic collider as fallback
                productObject.AddComponent<BoxCollider>();
            }
            
            // Place the product in this slot
            PlaceProduct(product);
        }
        
        /// <summary>
        /// Get a color for the product based on its properties
        /// </summary>
        /// <param name="productData">Product data</param>
        /// <returns>Color for the product</returns>
        private Color GetProductColor(ProductData productData)
        {
            // Simple color assignment based on product name hash
            int hash = productData.ProductName.GetHashCode();
            float hue = (hash % 360) / 360f;
            return Color.HSVToRGB(hue, 0.7f, 0.9f);
        }
        
        /// <summary>
        /// Get interaction text for placing products from inventory
        /// </summary>
        /// <returns>Descriptive text for the interaction</returns>
        private string GetPlacementText()
        {
            var inventory = InventoryManager.Instance;
            if (inventory == null) return "Empty Slot";
            
            ProductData selectedProduct = inventory.SelectedProduct;
            if (selectedProduct == null) return "Empty Slot - No Product Selected";
            
            int quantity = inventory.GetProductCount(selectedProduct);
            if (quantity <= 0) return $"Empty Slot - No {selectedProduct.ProductName} Available";
            
            return $"Place {selectedProduct.ProductName} ({quantity} available)";
        }
        
        /// <summary>
        /// Called when player starts looking at this slot
        /// </summary>
        public void OnInteractionEnter()
        {
            ApplyHighlight();
        }
        
        /// <summary>
        /// Called when player stops looking at this slot
        /// </summary>
        public void OnInteractionExit()
        {
            RemoveHighlight();
        }
        
        /// <summary>
        /// Provide visual feedback for interaction
        /// </summary>
        private System.Collections.IEnumerator InteractionFeedback()
        {
            // Flash the highlight a few times
            for (int i = 0; i < 3; i++)
            {
                ApplyHighlight();
                yield return new WaitForSeconds(0.1f);
                RemoveHighlight();
                yield return new WaitForSeconds(0.1f);
            }
            UpdateVisualState();
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
                Debug.Log($"Clicked on empty slot {name} - attempting to place product from inventory");
                PlaceProductFromInventory();
            }
            else
            {
                Debug.Log($"Clicked on slot {name} containing {currentProduct.ProductData?.ProductName}");
                // Allow removal by clicking (alternative to E key interaction)
                Product removedProduct = RemoveProduct();
                if (removedProduct != null && removedProduct.ProductData != null)
                {
                    // Add product back to inventory
                    InventoryManager.Instance.AddProduct(removedProduct.ProductData, 1);
                    Debug.Log($"Clicked to remove: Added {removedProduct.ProductData.ProductName} back to inventory");
                    
                    // Destroy the visual product object
                    Destroy(removedProduct.gameObject);
                }
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
            bool isPrefabReference = false;
            
            // Check if slotIndicator is a prefab reference
            if (slotIndicator != null)
            {
#if UNITY_EDITOR
                // In editor, use PrefabUtility to detect prefabs
                isPrefabReference = PrefabUtility.IsPartOfPrefabAsset(slotIndicator);
#else
                // In runtime, check if it has no parent and no scene (typical of prefab references)
                isPrefabReference = slotIndicator.transform.parent == null && 
                                   slotIndicator.gameObject.scene.name == null;
#endif
            }
            
            // If a prefab is assigned in inspector, instantiate it
            if (slotIndicator != null && isPrefabReference)
            {
                // This is a prefab reference, instantiate it
                GameObject prefabInstance = Instantiate(slotIndicator);
                slotIndicator = prefabInstance;
                slotIndicator.name = $"{name}_Indicator";
                slotIndicator.transform.SetParent(transform, false);
                
                // Position and scale the indicator
                slotIndicator.transform.localPosition = slotPosition;
                slotIndicator.transform.localScale = indicatorScale;
                
                Debug.Log($"Instantiated prefab indicator for slot {name}");
            }
            // Create slot indicator if it doesn't exist
            else if (slotIndicator == null)
            {
                slotIndicator = GameObject.CreatePrimitive(PrimitiveType.Cube);
                slotIndicator.name = $"{name}_Indicator";
                slotIndicator.transform.SetParent(transform, false);
                
                // Position and scale the indicator
                slotIndicator.transform.localPosition = slotPosition;
                slotIndicator.transform.localScale = indicatorScale;
                
                Debug.Log($"Created default cube indicator for slot {name}");
                
                // Remove collider from indicator (we want slot collider to handle interactions)
                Collider indicatorCollider = slotIndicator.GetComponent<Collider>();
                if (indicatorCollider != null)
                {
                    DestroyImmediate(indicatorCollider);
                }
            }
            else
            {
                // Indicator already exists in scene, just ensure proper positioning
                slotIndicator.transform.localPosition = slotPosition;
                slotIndicator.transform.localScale = indicatorScale;
                Debug.Log($"Using existing scene indicator for slot {name}");
            }
            
            // Remove any colliders from the indicator (we want slot collider to handle interactions)
            Collider[] indicatorColliders = slotIndicator.GetComponents<Collider>();
            foreach (Collider col in indicatorColliders)
            {
                if (Application.isPlaying)
                {
                    Destroy(col);
                }
                else
                {
                    DestroyImmediate(col);
                }
            }
            
            // Get renderer for material changes
            indicatorRenderer = slotIndicator.GetComponent<MeshRenderer>();
            
            Debug.Log($"Slot {name} indicator setup complete. Renderer found: {indicatorRenderer != null}");
            
            // Setup materials after renderer is assigned
            SetupMaterials();
        }
        
        /// <summary>
        /// Setup materials for normal and highlight states
        /// </summary>
        private void SetupMaterials()
        {
            if (indicatorRenderer == null) return;
            
            // If materials are not assigned in inspector, create default ones OR preserve prefab material
            if (normalMaterial == null)
            {
                // Check if the renderer already has a material (from prefab)
                if (indicatorRenderer.material != null && indicatorRenderer.material.name != "Default-Material")
                {
                    // Use the prefab's material as the normal material
                    normalMaterial = indicatorRenderer.material;
                    Debug.Log($"Using prefab material as normal material for slot {name}");
                }
                else
                {
                    // Create default material for generated cube indicators
                    normalMaterial = new Material(Shader.Find("Standard"));
                    normalMaterial.color = emptySlotColor;
                    normalMaterial.SetFloat("_Metallic", 0f);
                    normalMaterial.SetFloat("_Glossiness", 0.3f);
                    Debug.Log($"Created default normal material for slot {name}");
                }
            }
            
            // Create highlight material if not assigned
            if (highlightMaterial == null)
            {
                // Create a highlight version based on the normal material
                highlightMaterial = new Material(normalMaterial);
                highlightMaterial.color = highlightColor;
                highlightMaterial.SetFloat("_Metallic", 0f);
                highlightMaterial.SetFloat("_Glossiness", 0.5f);
                
                // Add emission for better visibility
                highlightMaterial.EnableKeyword("_EMISSION");
                highlightMaterial.SetColor("_EmissionColor", highlightColor * 0.3f);
                Debug.Log($"Created highlight material for slot {name}");
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
            
            // Apply normal material when not highlighted (only if we have a MeshRenderer)
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
