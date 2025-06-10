using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace TabletopShop
{
    /// <summary>
    /// Parent manager for a shelf containing multiple ShelfSlot components
    /// Handles overall shelf operations and coordinates between slots
    /// </summary>
    public class Shelf : MonoBehaviour
    {
        [Header("Shelf Configuration")]
        [SerializeField] private List<ShelfSlot> shelfSlots = new List<ShelfSlot>();
        [SerializeField] private int maxSlots = 5;
        [SerializeField] private float slotSpacing = 1.5f;
        [SerializeField] private ProductType allowedProductType;
        [SerializeField] private bool allowAnyProductType = true;
        
        [Header("Auto-Setup")]
        [SerializeField] private bool autoCreateSlots = true;
        [SerializeField] private GameObject slotPrefab;
        
        [Header("Visual Settings")]
        [SerializeField] private Material shelfMaterial;
        [SerializeField] private Vector3 shelfDimensions = new Vector3(7.5f, 0.2f, 1f);
        
        // Properties
        public int TotalSlots => shelfSlots.Count;
        public int OccupiedSlots => shelfSlots.Count(slot => !slot.IsEmpty);
        public int EmptySlots => shelfSlots.Count(slot => slot.IsEmpty);
        public bool IsFull => EmptySlots == 0;
        public bool IsEmpty => OccupiedSlots == 0;
        public ProductType AllowedProductType => allowedProductType;
        public bool AllowsAnyProductType => allowAnyProductType;
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            if (autoCreateSlots && shelfSlots.Count == 0)
            {
                CreateShelfSlots();
            }
            
            SetupShelfVisual();
            ValidateSlots();
        }
        
        private void Start()
        {
            // Initialize all slots
            foreach (var slot in shelfSlots)
            {
                if (slot != null)
                {
                    slot.gameObject.name = $"Slot_{shelfSlots.IndexOf(slot) + 1}";
                }
            }
            
            Debug.Log($"Shelf '{name}' initialized with {TotalSlots} slots");
        }
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// Attempt to place a product on the first available slot
        /// </summary>
        /// <param name="product">The product to place</param>
        /// <returns>True if product was successfully placed</returns>
        public bool TryPlaceProduct(Product product)
        {
            if (product == null)
            {
                Debug.LogWarning($"Cannot place null product on shelf {name}");
                return false;
            }
            
            // Check if product type is allowed
            if (!IsProductTypeAllowed(product.ProductData?.Type ?? ProductType.MiniatureBox))
            {
                Debug.LogWarning($"Product type {product.ProductData?.Type} not allowed on shelf {name}");
                return false;
            }
            
            // Find first empty slot
            ShelfSlot emptySlot = GetFirstEmptySlot();
            if (emptySlot == null)
            {
                Debug.LogWarning($"No empty slots available on shelf {name}");
                return false;
            }
            
            // Place product in the slot
            return emptySlot.PlaceProduct(product);
        }
        
        /// <summary>
        /// Attempt to place a product in a specific slot
        /// </summary>
        /// <param name="product">The product to place</param>
        /// <param name="slotIndex">The index of the slot to place in</param>
        /// <returns>True if product was successfully placed</returns>
        public bool TryPlaceProductInSlot(Product product, int slotIndex)
        {
            if (!IsValidSlotIndex(slotIndex))
            {
                Debug.LogWarning($"Invalid slot index {slotIndex} for shelf {name}");
                return false;
            }
            
            if (product == null)
            {
                Debug.LogWarning($"Cannot place null product in slot {slotIndex} on shelf {name}");
                return false;
            }
            
            // Check if product type is allowed
            if (!IsProductTypeAllowed(product.ProductData?.Type ?? ProductType.MiniatureBox))
            {
                Debug.LogWarning($"Product type {product.ProductData?.Type} not allowed on shelf {name}");
                return false;
            }
            
            return shelfSlots[slotIndex].PlaceProduct(product);
        }
        
        /// <summary>
        /// Remove product from a specific slot
        /// </summary>
        /// <param name="slotIndex">The index of the slot to remove from</param>
        /// <returns>The removed product, or null if slot was empty or invalid</returns>
        public Product RemoveProductFromSlot(int slotIndex)
        {
            if (!IsValidSlotIndex(slotIndex))
            {
                Debug.LogWarning($"Invalid slot index {slotIndex} for shelf {name}");
                return null;
            }
            
            return shelfSlots[slotIndex].RemoveProduct();
        }
        
        /// <summary>
        /// Clear all products from the shelf
        /// </summary>
        public void ClearAllProducts()
        {
            foreach (var slot in shelfSlots)
            {
                if (slot != null && !slot.IsEmpty)
                {
                    slot.RemoveProduct();
                }
            }
            
            Debug.Log($"Cleared all products from shelf {name}");
        }
        
        /// <summary>
        /// Get all products currently on the shelf
        /// </summary>
        /// <returns>List of products on the shelf</returns>
        public List<Product> GetAllProducts()
        {
            List<Product> products = new List<Product>();
            
            foreach (var slot in shelfSlots)
            {
                if (slot != null && !slot.IsEmpty)
                {
                    products.Add(slot.CurrentProduct);
                }
            }
            
            return products;
        }
        
        /// <summary>
        /// Get products of a specific type
        /// </summary>
        /// <param name="productType">The type of products to find</param>
        /// <returns>List of products of the specified type</returns>
        public List<Product> GetProductsByType(ProductType productType)
        {
            return GetAllProducts()
                .Where(p => p.ProductData?.Type == productType)
                .ToList();
        }
        
        /// <summary>
        /// Get the first empty slot, or null if all slots are occupied
        /// </summary>
        /// <returns>First empty ShelfSlot or null</returns>
        public ShelfSlot GetFirstEmptySlot()
        {
            return shelfSlots.FirstOrDefault(slot => slot != null && slot.IsEmpty);
        }
        
        /// <summary>
        /// Get a specific slot by index
        /// </summary>
        /// <param name="index">The index of the slot</param>
        /// <returns>The ShelfSlot at the specified index, or null if invalid</returns>
        public ShelfSlot GetSlot(int index)
        {
            if (!IsValidSlotIndex(index))
                return null;
            
            return shelfSlots[index];
        }
        
        /// <summary>
        /// Check if a product type is allowed on this shelf
        /// </summary>
        /// <param name="productType">The product type to check</param>
        /// <returns>True if the product type is allowed</returns>
        public bool IsProductTypeAllowed(ProductType productType)
        {
            return allowAnyProductType || allowedProductType == productType;
        }
        
        /// <summary>
        /// Set the allowed product type for this shelf
        /// </summary>
        /// <param name="productType">The product type to allow</param>
        /// <param name="allowAny">Whether to allow any product type</param>
        public void SetAllowedProductType(ProductType productType, bool allowAny = false)
        {
            allowedProductType = productType;
            allowAnyProductType = allowAny;
            
            Debug.Log($"Shelf {name} now allows {(allowAny ? "any product type" : productType.ToString())}");
        }
        
        #endregion
        
        #region Public Initialization
        
        /// <summary>
        /// Public initialization method for external setup of the Shelf
        /// Can be called to configure the shelf with specific parameters
        /// </summary>
        /// <param name="maxSlots">Maximum number of slots to create</param>
        /// <param name="slotSpacing">Spacing between slots</param>
        /// <param name="allowedProductType">The type of products allowed on this shelf</param>
        /// <param name="allowAnyProductType">Whether to allow any product type</param>
        /// <param name="forceRecreateSlots">Whether to recreate slots even if they already exist</param>
        public void Initialize(int? maxSlots = null, float? slotSpacing = null, ProductType? allowedProductType = null, bool? allowAnyProductType = null, bool forceRecreateSlots = false)
        {
            // Update configuration if provided
            if (maxSlots.HasValue && maxSlots.Value > 0)
            {
                this.maxSlots = maxSlots.Value;
            }
            
            if (slotSpacing.HasValue && slotSpacing.Value >= 0.5f)
            {
                this.slotSpacing = slotSpacing.Value;
            }
            
            if (allowedProductType.HasValue)
            {
                this.allowedProductType = allowedProductType.Value;
            }
            
            if (allowAnyProductType.HasValue)
            {
                this.allowAnyProductType = allowAnyProductType.Value;
            }
            
            // Create or recreate slots if needed
            if (forceRecreateSlots || shelfSlots.Count == 0)
            {
                CreateShelfSlots();
            }
            else if (maxSlots.HasValue || slotSpacing.HasValue)
            {
                // Update existing slot positions if spacing or count changed
                UpdateSlotPositions();
            }
            
            // Setup visual representation
            SetupShelfVisual();
            
            // Validate configuration
            ValidateSlots();
            
            // Initialize all slots with the new Initialize function
            InitializeAllSlots();
            
            Debug.Log($"Shelf '{name}' initialized with {TotalSlots} slots, spacing: {this.slotSpacing}, allows: {(this.allowAnyProductType ? "Any Product" : this.allowedProductType.ToString())}");
        }
        
        /// <summary>
        /// Simple initialization with just slot configuration
        /// </summary>
        /// <param name="maxSlots">Maximum number of slots to create</param>
        /// <param name="slotSpacing">Spacing between slots</param>
        public void Initialize(ShelfSlot[] slots)
        {
            Initialize(maxSlots, slotSpacing, null, null, false);
        }
        
        /// <summary>
        /// Initialize all slots using their new Initialize function
        /// </summary>
        private void InitializeAllSlots()
        {
            for (int i = 0; i < shelfSlots.Count; i++)
            {
                if (shelfSlots[i] != null)
                {
                    // Force synchronization first to ensure consistency
                    shelfSlots[i].ForceSynchronization();
                    
                    // Calculate position for this slot
                    float totalWidth = (shelfSlots.Count - 1) * slotSpacing;
                    float startX = -totalWidth / 2f;
                    Vector3 slotPosition = new Vector3(startX + (i * slotSpacing), 0.1f, 0);
                    
                    // Initialize the slot with its position
                    shelfSlots[i].Initialize(Vector3.zero); // Local offset is zero since position is handled by transform
                    shelfSlots[i].gameObject.name = $"Slot_{i + 1}";
                    
                    Debug.Log($"Initialized slot {i} at position {slotPosition}, isEmpty: {shelfSlots[i].IsEmpty}");
                }
            }
        }
        
        /// <summary>
        /// Update positions of existing slots based on current spacing
        /// </summary>
        private void UpdateSlotPositions()
        {
            if (shelfSlots == null || shelfSlots.Count == 0)
                return;
            
            float totalWidth = (shelfSlots.Count - 1) * slotSpacing;
            float startX = -totalWidth / 2f;
            
            for (int i = 0; i < shelfSlots.Count; i++)
            {
                if (shelfSlots[i] != null)
                {
                    Vector3 slotPosition = new Vector3(startX + (i * slotSpacing), 0.1f, 0);
                    shelfSlots[i].transform.localPosition = slotPosition;
                }
            }
        }
        
        #endregion
        
        #region Setup and Validation
        
        /// <summary>
        /// Create shelf slots automatically
        /// </summary>
        private void CreateShelfSlots()
        {
            // Clear existing slots
            shelfSlots.Clear();
            
            // Calculate starting position (center the slots)
            float totalWidth = (maxSlots - 1) * slotSpacing;
            float startX = -totalWidth / 2f;
            
            for (int i = 0; i < maxSlots; i++)
            {
                GameObject slotObject;
                
                if (slotPrefab != null)
                {
                    // Use prefab if available
                    slotObject = Instantiate(slotPrefab, transform);
                }
                else
                {
                    // Create basic slot GameObject
                    slotObject = new GameObject($"Slot_{i + 1}");
                    slotObject.transform.SetParent(transform, false);
                    slotObject.AddComponent<ShelfSlot>();
                }
                
                // Position the slot
                Vector3 slotPosition = new Vector3(startX + (i * slotSpacing), 0.1f, 0);
                slotObject.transform.localPosition = slotPosition;
                
                // Get or add ShelfSlot component
                ShelfSlot slot = slotObject.GetComponent<ShelfSlot>();
                if (slot == null)
                {
                    slot = slotObject.AddComponent<ShelfSlot>();
                }
                
                // Set slot position offset (for product placement)
                slot.SetSlotPosition(Vector3.zero);
                
                // Add to slots list
                shelfSlots.Add(slot);
            }
            
            Debug.Log($"Created {maxSlots} slots for shelf {name}");
        }
        
        /// <summary>
        /// Setup the visual representation of the shelf
        /// </summary>
        private void SetupShelfVisual()
        {
            // Check if shelf already has a visual representation
            MeshRenderer existingRenderer = GetComponent<MeshRenderer>();
            if (existingRenderer != null)
                return;
            
            // Check for and clean up any duplicate ShelfVisual children
            var existingVisuals = new List<Transform>();
            for (int i = 0; i < transform.childCount; i++)
            {
                Transform child = transform.GetChild(i);
                if (child.name == "ShelfVisual")
                {
                    existingVisuals.Add(child);
                }
            }
            
            // If we have duplicates, remove all but the first one
            if (existingVisuals.Count > 1)
            {
                Debug.LogWarning($"Found {existingVisuals.Count} duplicate ShelfVisual objects on shelf {name}, cleaning up...");
                for (int i = 1; i < existingVisuals.Count; i++)
                {
                    if (Application.isPlaying)
                    {
                        Destroy(existingVisuals[i].gameObject);
                    }
                    else
                    {
                        DestroyImmediate(existingVisuals[i].gameObject);
                    }
                }
                Debug.Log($"Cleaned up {existingVisuals.Count - 1} duplicate ShelfVisual objects");
            }
            
            // If we already have one ShelfVisual, we're good
            if (existingVisuals.Count > 0)
            {
                Debug.Log($"ShelfVisual already exists for shelf {name}, skipping creation");
                return;
            }
            
            // Create shelf visual
            GameObject shelfVisual = GameObject.CreatePrimitive(PrimitiveType.Cube);
            shelfVisual.name = "ShelfVisual";
            shelfVisual.transform.SetParent(transform, false);
            shelfVisual.transform.localPosition = Vector3.zero;
            shelfVisual.transform.localScale = shelfDimensions;
            
            // Apply material if available
            MeshRenderer renderer = shelfVisual.GetComponent<MeshRenderer>();
            if (shelfMaterial != null && renderer != null)
            {
                renderer.material = shelfMaterial;
            }
            else if (renderer != null)
            {
                // Create default shelf material
                Material defaultMaterial = new Material(Shader.Find("Standard"));
                defaultMaterial.color = new Color(0.6f, 0.4f, 0.2f, 1f); // Brown wood color
                defaultMaterial.SetFloat("_Metallic", 0f);
                defaultMaterial.SetFloat("_Glossiness", 0.3f);
                renderer.material = defaultMaterial;
            }
            
            // Remove collider from visual (slots handle interactions)
            Collider visualCollider = shelfVisual.GetComponent<Collider>();
            if (visualCollider != null)
            {
                DestroyImmediate(visualCollider);
            }
        }
        
        /// <summary>
        /// Validate that all slots are properly configured
        /// </summary>
        private void ValidateSlots()
        {
            // Remove any null references
            shelfSlots.RemoveAll(slot => slot == null);
            
            // Ensure we don't exceed max slots
            if (shelfSlots.Count > maxSlots)
            {
                Debug.LogWarning($"Shelf {name} has more slots ({shelfSlots.Count}) than max allowed ({maxSlots})");
                shelfSlots = shelfSlots.Take(maxSlots).ToList();
            }
            
            // Warn if no slots
            if (shelfSlots.Count == 0)
            {
                Debug.LogWarning($"Shelf {name} has no slots! Consider enabling autoCreateSlots or manually adding ShelfSlot components.");
            }
        }
        
        /// <summary>
        /// Check if a slot index is valid
        /// </summary>
        /// <param name="index">The index to check</param>
        /// <returns>True if the index is valid</returns>
        private bool IsValidSlotIndex(int index)
        {
            return index >= 0 && index < shelfSlots.Count;
        }
        
        #endregion
        
        #region Editor Support
        
        /// <summary>
        /// Draw gizmos for shelf visualization
        /// </summary>
        private void OnDrawGizmos()
        {
            // Draw shelf bounds
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(transform.position, shelfDimensions);
            
            // Draw slot positions
            if (shelfSlots != null)
            {
                Gizmos.color = Color.green;
                foreach (var slot in shelfSlots)
                {
                    if (slot != null)
                    {
                        Gizmos.DrawWireSphere(slot.SlotPosition, 0.2f);
                    }
                }
            }
        }
        
        /// <summary>
        /// Validation in editor
        /// </summary>
        private void OnValidate()
        {
            if (maxSlots < 1)
                maxSlots = 1;
            
            if (slotSpacing < 0.5f)
                slotSpacing = 0.5f;
            
            // Update slot positions if spacing changed
            if (shelfSlots != null && shelfSlots.Count > 0)
            {
                float totalWidth = (shelfSlots.Count - 1) * slotSpacing;
                float startX = -totalWidth / 2f;
                
                for (int i = 0; i < shelfSlots.Count; i++)
                {
                    if (shelfSlots[i] != null)
                    {
                        Vector3 slotPosition = new Vector3(startX + (i * slotSpacing), 0.1f, 0);
                        shelfSlots[i].transform.localPosition = slotPosition;
                    }
                }
            }
        }
        
        #endregion
        
        #region Debug and Testing
        
        /// <summary>
        /// Get shelf status for debugging
        /// </summary>
        /// <returns>String with shelf status information</returns>
        public string GetShelfStatus()
        {
            return $"Shelf '{name}': {OccupiedSlots}/{TotalSlots} slots occupied. " +
                   $"Allows: {(allowAnyProductType ? "Any Product" : allowedProductType.ToString())}";
        }
        
        /// <summary>
        /// Debug all slots to identify synchronization issues
        /// </summary>
        public void DebugAllSlots()
        {
            Debug.Log($"=== DEBUGGING SHELF {name} ===");
            Debug.Log($"Total Slots: {TotalSlots}");
            Debug.Log($"Occupied Slots: {OccupiedSlots}");
            Debug.Log($"Empty Slots: {EmptySlots}");
            Debug.Log($"Is Empty: {IsEmpty}");
            Debug.Log($"Is Full: {IsFull}");
            
            for (int i = 0; i < shelfSlots.Count; i++)
            {
                var slot = shelfSlots[i];
                if (slot != null)
                {
                    // Force synchronization before debugging to fix any issues
                    slot.ForceSynchronization();
                    Debug.Log($"Slot {i}: {slot.GetSlotDebugInfo()}");
                }
                else
                {
                    Debug.LogError($"Slot {i}: NULL SLOT!");
                }
            }
            Debug.Log($"=== END DEBUG SHELF {name} ===");
        }
        
        #endregion
    }
}
