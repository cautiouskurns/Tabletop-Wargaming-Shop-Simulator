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
        
        [Header("Debug")]
        [SerializeField] private bool showShelfGizmos = true;
        [SerializeField] private bool showSlotGizmos = false; // Let slots control their own gizmos
        
        // Component references
        private ShelfVisuals shelfVisuals;
        
        // Properties
        public int TotalSlots => shelfSlots.Count;
        public int OccupiedSlots => shelfSlots.Count(slot => !slot.IsEmpty);
        public int EmptySlots => shelfSlots.Count(slot => slot.IsEmpty);
        public bool IsFull => EmptySlots == 0;
        public bool IsEmpty => OccupiedSlots == 0;
        public ProductType AllowedProductType => allowedProductType;
        public bool AllowsAnyProductType => allowAnyProductType;
        
        // Visual properties (delegated to ShelfVisuals component)
        public Material ShelfMaterial => shelfVisuals?.ShelfMaterial ?? shelfMaterial;
        public Vector3 ShelfDimensions => shelfVisuals?.ShelfDimensions ?? shelfDimensions;
        public bool ShowShelfGizmos => shelfVisuals?.ShowShelfGizmos ?? showShelfGizmos;
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            // Initialize component references
            shelfVisuals = GetComponent<ShelfVisuals>();
            if (shelfVisuals == null)
            {
                shelfVisuals = gameObject.AddComponent<ShelfVisuals>();
                
                // Transfer visual settings to the new component
                if (shelfMaterial != null || shelfDimensions != Vector3.zero)
                {
                    shelfVisuals.InitializeComponent(shelfMaterial, shelfDimensions, showShelfGizmos);
                }
            }
            
            // Only auto-create slots during play mode to avoid SendMessage errors
            // Check both the shelfSlots list AND actual child objects to avoid duplicate creation
            int listCount = shelfSlots.Count;
            int childCount = GetComponentsInChildren<ShelfSlot>().Length;
            bool hasExistingSlots = listCount > 0 || childCount > 0;
            
            Debug.Log($"Shelf {name} Awake: listCount={listCount}, childCount={childCount}, hasExistingSlots={hasExistingSlots}, autoCreateSlots={autoCreateSlots}");
            
            if (autoCreateSlots && !hasExistingSlots && Application.isPlaying)
            {
                Debug.Log($"Creating slots for {name} in play mode");
                CreateSlots();
            }
            #if UNITY_EDITOR
            // In edit mode, defer slot creation to avoid SendMessage errors during Awake
            else if (autoCreateSlots && !hasExistingSlots && !Application.isPlaying)
            {
                Debug.Log($"Deferring slot creation for {name} in edit mode");
                UnityEditor.EditorApplication.delayCall += () => {
                    if (this != null) // Check if object still exists
                    {
                        Debug.Log($"Deferred creating slots for {name}");
                        CreateSlots();
                    }
                };
            }
            #endif
            
            ValidateSlots();
        }
        
        private void Start()
        {
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
        /// Configure the shelf and create slots
        /// </summary>
        /// <param name="maxSlots">Maximum number of slots to create</param>
        /// <param name="slotSpacing">Spacing between slots</param>
        /// <param name="allowedType">The type of products allowed on this shelf</param>
        /// <param name="allowAnyType">Whether to allow any product type</param>
        public void Initialize(int maxSlots = 5, float slotSpacing = 1.5f, ProductType allowedType = ProductType.MiniatureBox, bool allowAnyType = true)
        {
            // Configure shelf parameters
            this.maxSlots = maxSlots;
            this.slotSpacing = slotSpacing;
            this.allowedProductType = allowedType;
            this.allowAnyProductType = allowAnyType;
            
            // Create slots with new configuration
            CreateSlots();
            
            Debug.Log($"Shelf '{name}' initialized with {TotalSlots} slots, spacing: {slotSpacing}, allows: {(allowAnyType ? "Any Product" : allowedType.ToString())}");
        }
        
        #endregion
        
        #region Setup and Validation
        
        /// <summary>
        /// Calculate position for a slot at given index
        /// </summary>
        /// <param name="index">Slot index</param>
        /// <returns>Local position for the slot</returns>
        private Vector3 CalculateSlotPosition(int index)
        {
            float totalWidth = (maxSlots - 1) * slotSpacing;
            float startX = -totalWidth / 2f;
            return new Vector3(startX + (index * slotSpacing), 0.1f, 0);
        }
        
        /// <summary>
        /// Create and position all shelf slots
        /// </summary>
        private void CreateSlots()
        {
            Debug.Log($"CreateSlots() called for {name} - Application.isPlaying: {Application.isPlaying}");
            
            #if UNITY_EDITOR
            // Don't create slots if this is a prefab asset (prevents data corruption)
            if (UnityEditor.PrefabUtility.IsPartOfPrefabAsset(this))
            {
                Debug.Log($"Skipping slot creation for prefab asset {name}");
                return;
            }
            #endif
            
            Debug.Log($"Clearing existing {shelfSlots.Count} slots and creating {maxSlots} new slots for {name}");
            shelfSlots.Clear();
            
            for (int i = 0; i < maxSlots; i++)
            {
                // Create slot GameObject - use prefab if available, otherwise create basic GameObject
                GameObject slotObject = slotPrefab != null 
                    ? Instantiate(slotPrefab, transform)
                    : new GameObject($"Slot_{i + 1}");
                
                slotObject.transform.SetParent(transform, false);
                slotObject.transform.localPosition = CalculateSlotPosition(i);
                slotObject.name = $"Slot_{i + 1}";
                
                // Setup ShelfSlot component
                ShelfSlot slot = slotObject.GetComponent<ShelfSlot>() ?? slotObject.AddComponent<ShelfSlot>();
                slot.Initialize(Vector3.zero);
                
                shelfSlots.Add(slot);
            }
            
            Debug.Log($"Created {maxSlots} slots for shelf {name}");
        }
        
        /// <summary>
        /// Simple slot validation - remove null references and sync with child components
        /// </summary>
        private void ValidateSlots()
        {
            // Remove null references
            shelfSlots.RemoveAll(slot => slot == null);
            
            // If list is empty but we have ShelfSlot children, rebuild the list
            if (shelfSlots.Count == 0)
            {
                ShelfSlot[] childSlots = GetComponentsInChildren<ShelfSlot>();
                if (childSlots.Length > 0)
                {
                    shelfSlots.AddRange(childSlots);
                    Debug.Log($"Rebuilt slot list for {name} - found {childSlots.Length} existing slots");
                }
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
        /// Draw gizmos for shelf visualization (coordinated to avoid conflicts)
        /// </summary>
        private void OnDrawGizmos()
        {
            // Let ShelfVisuals handle the shelf-specific gizmo drawing
            // We only handle slot coordination here
            
            // Only draw slot indicators if slots aren't drawing their own gizmos
            if (!showSlotGizmos && shelfSlots != null)
            {
                Gizmos.color = Color.green;
                foreach (var slot in shelfSlots)
                {
                    if (slot != null)
                    {
                        // Use transform position as fallback if SlotPosition fails
                        Vector3 position;
                        try
                        {
                            position = slot.SlotPosition;
                        }
                        catch
                        {
                            position = slot.transform.position;
                        }
                        Gizmos.DrawWireSphere(position, 0.2f);
                    }
                }
            }
        }

        /// <summary>
        /// Configure gizmo drawing for all slots
        /// </summary>
        public void SetSlotGizmoDrawing(bool enabled)
        {
            showSlotGizmos = enabled;
            foreach (var slot in shelfSlots)
            {
                if (slot != null)
                {
                    slot.SetGizmoDrawing(enabled);
                }
            }
        }
        
        /// <summary>
        /// Configure gizmo drawing for the shelf visual
        /// </summary>
        public void SetShelfGizmoDrawing(bool enabled)
        {
            showShelfGizmos = enabled;
            if (shelfVisuals != null)
            {
                shelfVisuals.SetGizmoDrawing(enabled);
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
            
            // Do NOT create slots during OnValidate to avoid Unity lifecycle errors
            // Parameter validation only - slots will be created when:
            // - Game starts (via Awake())  
            // - Initialize() is called programmatically
            // - Manual slot creation in inspector
            
            // Sync visual settings with ShelfVisuals component if it exists
            // Only do this during play mode to avoid component creation issues during OnValidate
            if (shelfVisuals != null && Application.isPlaying)
            {
                shelfVisuals.InitializeComponent(shelfMaterial, shelfDimensions, showShelfGizmos);
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
        
        #endregion
    }
}
