using UnityEngine;

namespace TabletopShop
{
    /// <summary>
    /// Handles the core business logic for shelf slots including product placement and removal
    /// </summary>
    public class ShelfSlotLogic : MonoBehaviour
    {
        [Header("Slot Configuration")]
        [SerializeField] private Vector3 slotPosition;
        [SerializeField] private Product currentProduct;
        
        // Events for notifying other components
        public System.Action OnProductPlaced;
        public System.Action OnProductRemoved;
        public System.Action OnVisualStateChanged;
        
        // Properties
        public bool IsEmpty => currentProduct == null;
        public Product CurrentProduct => currentProduct;
        public Vector3 SlotPosition => transform.position + slotPosition;
        
        // Public accessors for inspector values
        public Vector3 GetSlotPositionOffset() => slotPosition;
        
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
            
            // Notify observers
            OnProductPlaced?.Invoke();
            OnVisualStateChanged?.Invoke();
            
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
            
            // Notify observers
            OnProductRemoved?.Invoke();
            OnVisualStateChanged?.Invoke();
            
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
                OnVisualStateChanged?.Invoke();
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
        
        /// <summary>
        /// Create a product GameObject and place it in this slot
        /// </summary>
        /// <param name="productData">Product data to create</param>
        public void CreateAndPlaceProduct(ProductData productData)
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
            // Notify visual component if slot position changed
            OnVisualStateChanged?.Invoke();
        }
        
        #endregion
    }
}
