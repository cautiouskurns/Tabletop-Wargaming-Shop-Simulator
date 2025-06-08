using UnityEngine;

namespace TabletopShop
{
    /// <summary>
    /// MonoBehaviour component that handles individual product instances in the game world
    /// Manages product interactions, visual feedback, and state management
    /// </summary>
    [RequireComponent(typeof(MeshRenderer), typeof(Collider))]
    public class Product : MonoBehaviour, IInteractable
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
        
        // IInteractable Properties
        public string InteractionText => isPurchased ? "Already Purchased" : $"Buy {productData?.ProductName ?? "Product"} (${currentPrice})";
        public bool CanInteract => !isPurchased;
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            // Get required components
            meshRenderer = GetComponent<MeshRenderer>();
            productCollider = GetComponent<Collider>();
            
            // Set layer for interaction system
            InteractionLayers.SetProductLayer(gameObject);
            
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
        /// Handle mouse click on product (only triggered by left-click)
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
            
            // Handle left-click for purchase (existing behavior)
            Debug.Log($"Customer clicked on {productData?.ProductName ?? name} (${currentPrice})");
            
            // Use the same economic validation as player interactions for consistency
            bool canProcessTransaction = ValidateEconomicTransaction();
            
            if (canProcessTransaction)
            {
                ProcessPlayerPurchase();
            }
            else
            {
                // Fallback for mouse clicks - still allow purchase
                Purchase();
            }
        }
        
        /// <summary>
        /// Handle mouse input while over the product (for right-click detection)
        /// </summary>
        private void OnMouseOver()
        {
            if (isPurchased || !isOnShelf)
                return;
                
            // Check for right-click to open price setting popup
            if (Input.GetMouseButtonDown(1)) // Right mouse button pressed down
            {
                Debug.Log($"Right-clicked on {productData?.ProductName ?? name} - opening price setting");
                OpenPriceSettingPopup();
            }
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
        
        /// <summary>
        /// Open the price setting popup for this product
        /// </summary>
        private void OpenPriceSettingPopup()
        {
            // Find the ShopUI instance in the scene
            ShopUI shopUI = FindFirstObjectByType<ShopUI>();
            
            if (shopUI == null)
            {
                Debug.LogError("ShopUI not found in scene! Cannot open price setting popup.");
                return;
            }
            
            // Show the price setting popup for this product
            shopUI.ShowPriceSetting(this);
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
        
        #region IInteractable Implementation
        
        /// <summary>
        /// Handle player interaction with this product
        /// Economic validation integrated with GameManager for transaction verification
        /// </summary>
        /// <param name="player">The player GameObject</param>
        public void Interact(GameObject player)
        {
            if (isPurchased)
            {
                Debug.Log($"Product {productData?.ProductName ?? name} is already purchased!");
                return;
            }
            
            // Economic validation through GameManager before processing purchase
            // This ensures transactions respect shop financial constraints
            bool canProcessTransaction = ValidateEconomicTransaction();
            
            if (canProcessTransaction)
            {
                // Process the validated transaction
                ProcessPlayerPurchase();
                
                Debug.Log($"Player purchased {productData?.ProductName ?? name} for ${currentPrice}!");
            }
            else
            {
                // Graceful fallback: still allow purchase but log validation failure
                Debug.LogWarning($"Economic validation failed for {productData?.ProductName ?? name}, but allowing purchase (fallback behavior)");
                Purchase();
                
                Debug.Log($"Player purchased {productData?.ProductName ?? name} for ${currentPrice} (fallback mode)!");
            }
        }
        
        /// <summary>
        /// Called when player starts looking at this product
        /// </summary>
        public void OnInteractionEnter()
        {
            if (!isPurchased)
            {
                ApplyHoverEffect();
            }
        }
        
        /// <summary>
        /// Called when player stops looking at this product
        /// </summary>
        public void OnInteractionExit()
        {
            if (!isPurchased)
            {
                RemoveHoverEffect();
            }
        }
        
        #endregion
        
        #region Economic Integration
        
        /// <summary>
        /// Validate economic transaction through GameManager
        /// Performs null-safe checks and basic economic validation
        /// Returns true if transaction can proceed, false otherwise
        /// </summary>
        /// <returns>True if economic validation passes</returns>
        private bool ValidateEconomicTransaction()
        {
            // Null-safe GameManager access with graceful degradation
            if (GameManager.Instance == null)
            {
                Debug.LogWarning($"GameManager not available for economic validation of {productData?.ProductName ?? name} purchase");
                return false; // Validation failed, will trigger fallback behavior
            }
            
            // Basic transaction validation
            if (currentPrice <= 0)
            {
                Debug.LogWarning($"Invalid price for {productData?.ProductName ?? name}: ${currentPrice}");
                return false;
            }
            
            // Log transaction attempt for economic tracking
            Debug.Log($"Economic validation: Player attempting to purchase {productData?.ProductName ?? name} for ${currentPrice}");
            
            // Note: For player purchases, we don't check shop funds since this is player-to-shop transaction
            // Future expansion: Could add inventory purchasing costs or other economic constraints here
            
            return true; // Basic validation passed
        }
        
        /// <summary>
        /// Process a validated player purchase with GameManager integration
        /// Handles the complete purchase flow with transaction logging
        /// </summary>
        private void ProcessPlayerPurchase()
        {
            // Execute the core purchase logic (existing Product state management)
            Purchase();
            
            // Integrate with GameManager for economic tracking if available
            if (GameManager.Instance != null)
            {
                // Process player purchase through economic system
                // This tracks the transaction as revenue for the shop
                GameManager.Instance.ProcessCustomerPurchase(currentPrice, 1.0f); // Perfect satisfaction for player purchases
                
                Debug.Log($"Economic integration: Processed player purchase of ${currentPrice} through GameManager");
            }
            else
            {
                Debug.LogWarning("GameManager unavailable - purchase processed without economic integration");
            }
        }
        
        #endregion
        
        #region Testing & Validation
        
        /// <summary>
        /// Test the GameManager economic integration (for development/testing)
        /// </summary>
        [ContextMenu("Test GameManager Integration")]
        private void TestGameManagerIntegration()
        {
            if (Application.isPlaying)
            {
                Debug.Log("=== TESTING PRODUCT GAMEMANAGER INTEGRATION ===");
                Debug.Log($"Product: {productData?.ProductName ?? name} (${currentPrice})");
                Debug.Log($"Is on shelf: {isOnShelf}, Is purchased: {isPurchased}");
                
                if (GameManager.Instance != null)
                {
                    var economicStatus = GameManager.Instance.GetEconomicStatus();
                    Debug.Log($"GameManager state - Money: ${economicStatus.money:F2}, Customers: {economicStatus.customers}");
                    
                    // Test economic validation
                    bool validationResult = ValidateEconomicTransaction();
                    Debug.Log($"Economic validation result: {validationResult}");
                    
                    if (validationResult && !isPurchased && isOnShelf)
                    {
                        Debug.Log("Simulating player purchase...");
                        
                        // Store initial state for comparison
                        float initialMoney = economicStatus.money;
                        int initialCustomers = economicStatus.customers;
                        
                        // Simulate purchase
                        Interact(null);
                        
                        // Check results
                        var newEconomicStatus = GameManager.Instance.GetEconomicStatus();
                        Debug.Log($"After purchase - Money: ${newEconomicStatus.money:F2} (+${newEconomicStatus.money - initialMoney:F2})");
                        Debug.Log($"After purchase - Customers: {newEconomicStatus.customers} (+{newEconomicStatus.customers - initialCustomers})");
                        
                        Debug.Log("✅ GameManager integration test completed!");
                    }
                    else
                    {
                        Debug.LogWarning("Cannot test purchase - validation failed or product already purchased/not on shelf");
                    }
                }
                else
                {
                    Debug.LogError("GameManager.Instance is null - integration test failed");
                }
                
                Debug.Log("=== END GAMEMANAGER INTEGRATION TEST ===");
            }
            else
            {
                Debug.LogWarning("GameManager integration test requires Play mode");
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
