using UnityEngine;

namespace TabletopShop
{
    /// <summary>
    /// Main product component that coordinates specialized sub-components
    /// Manages core state and delegates operations to focused components
    /// </summary>
    [RequireComponent(typeof(MeshRenderer), typeof(Collider))]
    public class Product : MonoBehaviour, IInteractable
    {
        [Header("Product Configuration")]
        [SerializeField] private ProductData productData;
        [SerializeField] private float currentPrice;
        [SerializeField] private ProductState currentState = ProductState.Available;
        [SerializeField] private bool isOnShelf = false;
        [SerializeField] private bool isPurchased = false;
        
        [Header("Checkout Scanning")]
        [SerializeField] private bool isScannedAtCheckout = false;
        [SerializeField] private Material scannedMaterial; // Optional visual feedback
        
        [Header("Customer Association")]
        [SerializeField] private Customer placedByCustomer; // Track which customer placed this product
        
        // Component references for composition pattern
        private ProductEconomics productEconomics;
        private ProductVisuals productVisuals;
        private ProductInteraction productInteraction;
        private DynamicProduct dynamicProduct;
        
        // Core component references (still needed for basic functionality)
        private MeshRenderer meshRenderer;
        private Collider productCollider;
        
        // Shelf integration
        private ShelfSlot currentShelfSlot;
        
        // Properties - main state management
        public ProductData ProductData => productData;
        public float CurrentPrice => currentPrice;
        public bool IsOnShelf => isOnShelf;
        public bool IsPurchased => isPurchased;
        public ProductState CurrentState => currentState;
        public ShelfSlot CurrentShelfSlot => currentShelfSlot;
        
        // Checkout scanning properties
        public bool IsScannedAtCheckout => isScannedAtCheckout;
        
        // Customer association properties
        public Customer PlacedByCustomer => placedByCustomer;
        
        // IInteractable Properties - delegate to interaction component
        public string InteractionText => productInteraction?.InteractionText ?? "Product";
        public bool CanInteract => productInteraction?.CanInteract ?? false;
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            // Get required core components first
            meshRenderer = GetComponent<MeshRenderer>();
            productCollider = GetComponent<Collider>();
            
            // Validate core components
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
            
            // Set layer for interaction system
            InteractionLayers.SetProductLayer(gameObject);
        }
        
        private void Start()
        {
            // Get specialized components after all Awake() calls are complete
            productEconomics = GetComponent<ProductEconomics>();
            productVisuals = GetComponent<ProductVisuals>();
            productInteraction = GetComponent<ProductInteraction>();
            dynamicProduct = GetComponent<DynamicProduct>();
            
            // Add components if missing (for backward compatibility)
            if (productEconomics == null)
                productEconomics = gameObject.AddComponent<ProductEconomics>();
            
            if (productVisuals == null)
                productVisuals = gameObject.AddComponent<ProductVisuals>();
            
            if (productInteraction == null)
                productInteraction = gameObject.AddComponent<ProductInteraction>();
            
            // Setup component event subscriptions
            SetupComponentEvents();
            
            // Initialize with ProductData if available
            if (productData != null)
            {
                Initialize(productData);
            }
        }
        
        #endregion
        
        #region Component Event Setup
        
        /// <summary>
        /// Setup event subscriptions between components
        /// </summary>
        private void SetupComponentEvents()
        {
            // Subscribe to economics events
            if (productEconomics != null)
            {
                productEconomics.OnPurchaseProcessed += HandlePurchaseProcessed;
                productEconomics.OnPriceChanged += HandlePriceChanged;
            }
            
            // Subscribe to visual events (if needed for state coordination)
            if (productVisuals != null)
            {
                productVisuals.OnVisualStateChanged += HandleVisualStateChanged;
            }
            
            // Subscribe to interaction events
            if (productInteraction != null)
            {
                productInteraction.OnPlayerInteract += HandlePlayerInteraction;
            }
        }
        
        /// <summary>
        /// Handle purchase processed by economics component
        /// </summary>
        private void HandlePurchaseProcessed()
        {
            // Economics component handled the transaction, now update our state
            Debug.Log($"Purchase processing completed for {productData?.ProductName ?? name}");
        }
        
        /// <summary>
        /// Handle price change from economics component
        /// </summary>
        /// <param name="newPrice">The new price</param>
        private void HandlePriceChanged(float newPrice)
        {
            currentPrice = newPrice;
        }
        
        /// <summary>
        /// Handle visual state changes
        /// </summary>
        /// <param name="isVisible">Whether visuals are visible</param>
        private void HandleVisualStateChanged(bool isVisible)
        {
            // Coordinate with other components if needed
        }
        
        /// <summary>
        /// Handle player interaction
        /// </summary>
        /// <param name="player">The player GameObject</param>
        private void HandlePlayerInteraction(GameObject player)
        {
            Debug.Log($"Player interaction handled for {productData?.ProductName ?? name}");
        }

        #endregion

        #region Public Methods - State Management

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

            if (dynamicProduct != null)
            {
                dynamicProduct.UpdateFromProductData();
            }

        }

        /// <summary>
        /// Set a new price for this product instance
        /// Delegates to economics component for validation
        /// </summary>
        /// <param name="newPrice">The new price to set</param>
        public void SetPrice(float newPrice)
        {
            if (productEconomics != null)
            {
                productEconomics.UpdatePrice(newPrice);
            }
            else
            {
                // Fallback if economics component not available
                currentPrice = Mathf.Max(0, newPrice);
                Debug.Log($"Set price directly to ${currentPrice} (Economics component not available)");
            }
            
            if (dynamicProduct != null)
            {
                dynamicProduct.SetCustomText("price", $"${currentPrice:F2}");
            }
        }
        
        /// <summary>
        /// Handle product purchase - delegates to economics component
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
            
            // Update core state
            isPurchased = true;
            isOnShelf = false;
            currentShelfSlot = null;
            currentState = ProductState.Purchased;
            
            // Notify visual component
            if (productVisuals != null)
            {
                productVisuals.OnProductPurchased();
            }
            
            // Notify interaction component
            if (productInteraction != null)
            {
                productInteraction.OnProductPurchased();
            }
            
            Debug.Log($"PURCHASED: {productData?.ProductName ?? name} for ${currentPrice}!");
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
            currentShelfSlot = null;
            currentState = ProductState.Available;
            
            // Notify visual component
            if (productVisuals != null)
            {
                productVisuals.OnProductRemovedFromShelf();
            }
            
            // Notify interaction component
            if (productInteraction != null)
            {
                productInteraction.OnProductRemovedFromShelf();
            }
            
            Debug.Log($"Removed {productData?.ProductName ?? name} from shelf");
        }
        
        /// <summary>
        /// Place product on shelf with optional shelf slot reference
        /// </summary>
        /// <param name="shelfSlot">The shelf slot this product is being placed on (optional)</param>
        public void PlaceOnShelf(ShelfSlot shelfSlot = null)
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
            currentShelfSlot = shelfSlot;
            currentState = ProductState.OnShelf;
            
            // Notify visual component
            if (productVisuals != null)
            {
                productVisuals.OnProductPlacedOnShelf();
            }
            
            // Notify interaction component
            if (productInteraction != null)
            {
                productInteraction.OnProductPlacedOnShelf();
            }
            
            Debug.Log($"Placed {productData?.ProductName ?? name} on shelf with price ${currentPrice}");
        }
        
        /// <summary>
        /// Scan product at checkout - marks as scanned and optionally changes material
        /// </summary>
        public void ScanAtCheckout()
        {
            if (isScannedAtCheckout)
            {
                Debug.LogWarning($"Product {productData?.ProductName ?? name} is already scanned at checkout!");
                return;
            }
            
            isScannedAtCheckout = true;
            
            // Log the scan event
            Debug.Log($"SCANNED: {productData?.ProductName ?? name} (${currentPrice}) at checkout");
            
            // Optionally change material for visual feedback
            if (scannedMaterial != null && meshRenderer != null)
            {
                meshRenderer.material = scannedMaterial;
                Debug.Log($"Applied scanned material to {productData?.ProductName ?? name}");
            }
        }
        
        /// <summary>
        /// Reset the scan state - clears the scanned flag
        /// </summary>
        public void ResetScanState()
        {
            if (!isScannedAtCheckout)
            {
                Debug.Log($"Product {productData?.ProductName ?? name} scan state is already reset");
                return;
            }
            
            isScannedAtCheckout = false;
            Debug.Log($"Reset scan state for {productData?.ProductName ?? name}");
        }
        
        /// <summary>
        /// Set which customer placed this product at checkout
        /// </summary>
        /// <param name="customer">The customer who placed the product</param>
        public void SetPlacedByCustomer(Customer customer)
        {
            placedByCustomer = customer;
            Debug.Log($"Product {productData?.ProductName ?? name} placed by customer: {customer?.name ?? "None"}");
        }
        
        /// <summary>
        /// Clear the customer association (when customer leaves or checkout is reset)
        /// </summary>
        public void ClearCustomerAssociation()
        {
            Customer previousCustomer = placedByCustomer;
            placedByCustomer = null;
            Debug.Log($"Cleared customer association for {productData?.ProductName ?? name} (was: {previousCustomer?.name ?? "None"})");
        }
        
        /// <summary>
        /// Check if a specific customer can scan this product
        /// </summary>
        /// <param name="customer">The customer trying to scan</param>
        /// <returns>True if the customer can scan this product</returns>
        public bool CanCustomerScan(Customer customer)
        {
            // If no customer placed it, anyone can scan
            if (placedByCustomer == null)
            {
                Debug.Log($"Product {productData?.ProductName ?? name} has no customer association - allowing scan");
                return true;
            }
            
            // Only the customer who placed it can scan
            bool canScan = placedByCustomer == customer;
            if (!canScan)
            {
                Debug.LogWarning($"Customer {customer?.name ?? "Unknown"} cannot scan {productData?.ProductName ?? name} - placed by {placedByCustomer.name}");
            }
            else
            {
                Debug.Log($"Customer {customer?.name ?? "Unknown"} can scan {productData?.ProductName ?? name} - they placed it");
            }
            return canScan;
        }
        
        #endregion
        
        #region IInteractable Implementation - Delegate to Interaction Component
        
        /// <summary>
        /// Handle player interaction with this product
        /// Delegates to ProductInteraction component
        /// </summary>
        /// <param name="player">The player GameObject</param>
        public void Interact(GameObject player)
        {
            if (productInteraction != null)
            {
                productInteraction.Interact(player);
            }
            else
            {
                Debug.LogWarning($"ProductInteraction component not found on {name}");
            }
        }
        
        /// <summary>
        /// Called when player starts looking at this product
        /// Delegates to ProductInteraction component
        /// </summary>
        public void OnInteractionEnter()
        {
            if (productInteraction != null)
            {
                productInteraction.OnInteractionEnter();
            }
        }
        
        /// <summary>
        /// Called when player stops looking at this product
        /// Delegates to ProductInteraction component
        /// </summary>
        public void OnInteractionExit()
        {
            if (productInteraction != null)
            {
                productInteraction.OnInteractionExit();
            }
        }
        
        #endregion
        
        #region Component Access Methods
        
        /// <summary>
        /// Get the economics component for direct access
        /// </summary>
        /// <returns>ProductEconomics component</returns>
        public ProductEconomics GetEconomics()
        {
            return productEconomics;
        }
        
        /// <summary>
        /// Get the visuals component for direct access
        /// </summary>
        /// <returns>ProductVisuals component</returns>
        public ProductVisuals GetVisuals()
        {
            return productVisuals;
        }
        
        /// <summary>
        /// Get the interaction component for direct access
        /// </summary>
        /// <returns>ProductInteraction component</returns>
        public ProductInteraction GetInteraction()
        {
            return productInteraction;
        }
        
        #endregion
        
        
        #region Testing & Integration
        
        /// <summary>
        /// Test all component integrations (for development/testing)
        /// </summary>
        [ContextMenu("Test Component Integration")]
        public void TestComponentIntegration()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("Component integration test requires Play mode");
                return;
            }
            
            Debug.Log("=== TESTING PRODUCT COMPONENT INTEGRATION ===");
            Debug.Log($"Product: {productData?.ProductName ?? name} (${currentPrice})");
            Debug.Log($"State: {currentState}, On Shelf: {isOnShelf}, Purchased: {isPurchased}");
            
            // Test economics component
            if (productEconomics != null)
            {
                Debug.Log($"Economics Status: {productEconomics.GetEconomicStatus()}");
            }
            else
            {
                Debug.LogError("ProductEconomics component not found!");
            }
            
            // Test visuals component
            if (productVisuals != null)
            {
                Debug.Log($"Visual State: {productVisuals.GetVisualState()}");
            }
            else
            {
                Debug.LogError("ProductVisuals component not found!");
            }
            
            // Test interaction component
            if (productInteraction != null)
            {
                Debug.Log($"Interaction Status: {productInteraction.GetInteractionStatus()}");
            }
            else
            {
                Debug.LogError("ProductInteraction component not found!");
            }
            
            Debug.Log("✅ Component integration test completed!");
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
            // Unsubscribe from events to prevent memory leaks
            if (productEconomics != null)
            {
                productEconomics.OnPurchaseProcessed -= HandlePurchaseProcessed;
                productEconomics.OnPriceChanged -= HandlePriceChanged;
            }
            
            if (productVisuals != null)
            {
                productVisuals.OnVisualStateChanged -= HandleVisualStateChanged;
            }
            
            if (productInteraction != null)
            {
                productInteraction.OnPlayerInteract -= HandlePlayerInteraction;
            }
        }
        
        #endregion
    }
}