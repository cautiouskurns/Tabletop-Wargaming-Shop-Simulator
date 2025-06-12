using UnityEngine;

namespace TabletopShop
{
    /// <summary>
    /// Handles all player interaction and input for products
    /// Implements IInteractable interface and manages mouse events
    /// </summary>
    public class ProductInteraction : MonoBehaviour, IInteractable
    {
        // Component references
        private Product product;
        private ProductVisuals productVisuals;
        private ProductEconomics productEconomics;
        
        // Events
        public System.Action<GameObject> OnPlayerInteract;
        public System.Action OnHoverStarted;
        public System.Action OnHoverEnded;
        public System.Action OnPriceSettingRequested;
        
        #region IInteractable Properties
        
        public string InteractionText 
        { 
            get 
            {
                if (product == null) return "Product";
                return product.IsPurchased ? "Already Purchased" : $"Buy {product.ProductData?.ProductName ?? "Product"} (${product.CurrentPrice})";
            }
        }
        
        public bool CanInteract 
        { 
            get 
            {
                return product != null && !product.IsPurchased && product.IsOnShelf;
            }
        }
        
        #endregion
        
        #region Initialization
        
        private void Start()
        {
            // Get component references after all components are initialized
            product = GetComponent<Product>();
            productVisuals = GetComponent<ProductVisuals>();
            productEconomics = GetComponent<ProductEconomics>();
            
            // Validate required components
            if (product == null)
            {
                Debug.LogError($"ProductInteraction on {name} requires a Product component!", this);
            }
        }
        
        #endregion
        
        #region IInteractable Implementation
        
        /// <summary>
        /// Handle player interaction with this product
        /// Integrates with ProductEconomics for transaction processing
        /// </summary>
        /// <param name="player">The player GameObject</param>
        public void Interact(GameObject player)
        {
            if (!CanInteract)
            {
                Debug.Log($"Product {product?.ProductData?.ProductName ?? name} cannot be interacted with!");
                return;
            }
            
            Debug.Log($"Player interacting with {product.ProductData?.ProductName ?? name}");
            
            // Process interaction through economics component
            bool purchaseSuccessful = false;
            
            if (productEconomics != null)
            {
                // Use economics component for validated purchase
                purchaseSuccessful = productEconomics.ProcessPurchase();
                
                if (purchaseSuccessful)
                {
                    // Complete the purchase through main Product component
                    product.Purchase();
                    Debug.Log($"Player purchased {product.ProductData?.ProductName ?? name} for ${product.CurrentPrice}!");
                }
                else
                {
                    // Graceful fallback: still allow purchase but log validation failure
                    Debug.LogWarning($"Economic validation failed for {product.ProductData?.ProductName ?? name}, but allowing purchase (fallback behavior)");
                    product.Purchase();
                    Debug.Log($"Player purchased {product.ProductData?.ProductName ?? name} for ${product.CurrentPrice} (fallback mode)!");
                    purchaseSuccessful = true;
                }
            }
            else
            {
                // Direct purchase if no economics component
                product.Purchase();
                Debug.Log($"Player purchased {product.ProductData?.ProductName ?? name} for ${product.CurrentPrice} (direct mode)!");
                purchaseSuccessful = true;
            }
            
            // Fire interaction event
            OnPlayerInteract?.Invoke(player);
        }
        
        /// <summary>
        /// Called when player starts looking at this product
        /// </summary>
        public void OnInteractionEnter()
        {
            if (!CanInteract)
                return;
            
            // Apply hover effect through visuals component
            if (productVisuals != null)
            {
                productVisuals.ApplyHoverEffect();
            }
            
            // Fire interaction enter event
            OnHoverStarted?.Invoke();
        }
        
        /// <summary>
        /// Called when player stops looking at this product
        /// </summary>
        public void OnInteractionExit()
        {
            if (!CanInteract)
                return;
            
            // Remove hover effect through visuals component
            if (productVisuals != null)
            {
                productVisuals.RemoveHoverEffect();
            }
            
            // Fire interaction exit event
            OnHoverEnded?.Invoke();
        }
        
        #endregion
        
        #region Mouse Input Handling
        
        /// <summary>
        /// Handle mouse click on product (only triggered by left-click)
        /// </summary>
        private void OnMouseDown()
        {
            if (!CanInteract)
            {
                if (product != null)
                {
                    if (product.IsPurchased)
                    {
                        Debug.Log($"Product {product.ProductData?.ProductName ?? name} is already purchased!");
                    }
                    else if (!product.IsOnShelf)
                    {
                        Debug.Log($"Product {product.ProductData?.ProductName ?? name} is not available for purchase (not on shelf)");
                    }
                }
                return;
            }
            
            // Handle left-click for purchase (existing behavior)
            Debug.Log($"Customer clicked on {product.ProductData?.ProductName ?? name} (${product.CurrentPrice})");
            
            // Use the interaction system for consistency
            Interact(null); // Mouse clicks don't have a player GameObject reference
        }
        
        /// <summary>
        /// Handle mouse input while over the product (for right-click detection)
        /// </summary>
        private void OnMouseOver()
        {
            if (!CanInteract)
                return;
                
            // Check for right-click to open price setting popup
            if (Input.GetMouseButtonDown(1)) // Right mouse button pressed down
            {
                Debug.Log($"Right-clicked on {product.ProductData?.ProductName ?? name} - opening price setting");
                RequestPriceSetting();
            }
        }
        
        /// <summary>
        /// Handle mouse enter for hover effect
        /// </summary>
        private void OnMouseEnter()
        {
            if (!CanInteract)
                return;
            
            // Use IInteractable method for consistency
            OnInteractionEnter();
        }
        
        /// <summary>
        /// Handle mouse exit to remove hover effect
        /// </summary>
        private void OnMouseExit()
        {
            if (!CanInteract)
                return;
            
            // Use IInteractable method for consistency
            OnInteractionExit();
        }
        
        #endregion
        
        #region UI Integration
        
        /// <summary>
        /// Request price setting popup for this product
        /// </summary>
        public void RequestPriceSetting()
        {
            if (product == null)
            {
                Debug.LogError("Cannot request price setting - Product component not found!");
                return;
            }
            
            // Find the ShopUI instance in the scene
            ShopUI shopUI = FindFirstObjectByType<ShopUI>();
            
            if (shopUI == null)
            {
                Debug.LogError("ShopUI not found in scene! Cannot open price setting popup.");
                return;
            }
            
            // Show the price setting popup for this product
            shopUI.ShowPriceSetting(product);
            
            // Fire price setting requested event
            OnPriceSettingRequested?.Invoke();
        }
        
        #endregion
        
        #region Interaction State Management
        
        /// <summary>
        /// Update interaction availability based on product state
        /// </summary>
        public void UpdateInteractionState()
        {
            // Force update of interaction availability
            // This will be called when product state changes
            
            if (!CanInteract && productVisuals != null)
            {
                // Remove any existing hover effects if product is no longer interactable
                productVisuals.RemoveHoverEffect();
            }
        }
        
        /// <summary>
        /// Get current interaction status
        /// </summary>
        /// <returns>Interaction status description</returns>
        public string GetInteractionStatus()
        {
            if (product == null)
                return "No product component";
            
            if (product.IsPurchased)
                return "Already purchased";
            
            if (!product.IsOnShelf)
                return "Not on shelf";
            
            return $"Available for purchase: ${product.CurrentPrice}";
        }
        
        #endregion
        
        #region Integration Events
        
        /// <summary>
        /// Handle product purchase completion
        /// </summary>
        public void OnProductPurchased()
        {
            // Update interaction state when product is purchased
            UpdateInteractionState();
            
            Debug.Log($"Product interaction disabled for purchased item: {product?.ProductData?.ProductName ?? name}");
        }
        
        /// <summary>
        /// Handle product placement on shelf
        /// </summary>
        public void OnProductPlacedOnShelf()
        {
            // Update interaction state when product is placed on shelf
            UpdateInteractionState();
            
            Debug.Log($"Product interaction enabled for shelf placement: {product?.ProductData?.ProductName ?? name}");
        }
        
        /// <summary>
        /// Handle product removal from shelf
        /// </summary>
        public void OnProductRemovedFromShelf()
        {
            // Update interaction state when product is removed from shelf
            UpdateInteractionState();
            
            Debug.Log($"Product interaction disabled for shelf removal: {product?.ProductData?.ProductName ?? name}");
        }
        
        #endregion
        
        #region Testing
        
        /// <summary>
        /// Test interaction functionality (for development/testing)
        /// </summary>
        [ContextMenu("Test Interaction System")]
        public void TestInteractionSystem()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("Interaction system test requires Play mode");
                return;
            }
            
            Debug.Log("=== TESTING PRODUCT INTERACTION SYSTEM ===");
            Debug.Log($"Product: {product?.ProductData?.ProductName ?? name}");
            Debug.Log($"Can Interact: {CanInteract}");
            Debug.Log($"Interaction Text: {InteractionText}");
            Debug.Log($"Interaction Status: {GetInteractionStatus()}");
            
            if (CanInteract)
            {
                Debug.Log("Testing interaction enter/exit...");
                OnInteractionEnter();
                
                Invoke(nameof(TestInteractionExit), 1f);
                Invoke(nameof(TestDirectInteraction), 2f);
            }
            else
            {
                Debug.LogWarning("Cannot test interaction - product is not interactable");
            }
        }
        
        private void TestInteractionExit()
        {
            OnInteractionExit();
            Debug.Log("Interaction exit tested");
        }
        
        private void TestDirectInteraction()
        {
            if (CanInteract)
            {
                Debug.Log("Testing direct interaction...");
                Interact(null);
                Debug.Log("✅ Interaction system test completed!");
            }
        }
        
        #endregion
    }
}
