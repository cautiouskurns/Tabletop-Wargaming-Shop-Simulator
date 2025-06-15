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
                
                if (product.IsPurchased) 
                    return "Already Purchased";
                
                // Check if product is at checkout for scanning
                if (IsAtCheckout())
                {
                    if (product.IsScannedAtCheckout)
                        return $"{product.ProductData?.ProductName ?? "Product"} (Scanned)";
                    else
                        return $"Scan {product.ProductData?.ProductName ?? "Product"} (${product.CurrentPrice})";
                }
                
                // Normal purchase on shelf
                if (product.IsOnShelf)
                    return $"Buy {product.ProductData?.ProductName ?? "Product"} (${product.CurrentPrice})";
                
                return product.ProductData?.ProductName ?? "Product";
            }
        }
        
        public bool CanInteract 
        { 
            get 
            {
                if (product == null || product.IsPurchased) 
                    return false;
                
                // Can interact if on shelf (for purchase) or at checkout (for scanning)
                return product.IsOnShelf || IsAtCheckout();
            }
        }
        
        /// <summary>
        /// Quick check if product is at checkout (used by properties)
        /// </summary>
        private bool IsAtCheckout()
        {
            if (product == null || product.IsOnShelf || product.IsPurchased)
                return false;
            
            // Check if parent contains "checkout" in name (simple check)
            Transform parent = transform.parent;
            return parent != null && parent.name.ToLower().Contains("checkout");
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
        /// Now also handles checkout scanning when product is at checkout
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
            
            // Check if product is at checkout counter for scanning
            if (IsAtCheckout())
            {
                HandleCheckoutScanning();
                return;
            }
            
            // Normal purchase flow for products on shelf
            if (product.IsOnShelf)
            {
                HandleNormalPurchase(player);
                return;
            }
            
            Debug.LogWarning($"Product {product.ProductData?.ProductName ?? name} is not on shelf or at checkout - cannot interact");
        }
        
        /// <summary>
        /// Handle normal purchase when product is on shelf
        /// </summary>
        /// <param name="player">The player GameObject</param>
        private void HandleNormalPurchase(GameObject player)
        {
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
        /// Handle checkout scanning when product is at checkout counter
        /// </summary>
        private void HandleCheckoutScanning()
        {
            if (product.IsScannedAtCheckout)
            {
                Debug.Log($"Product {product.ProductData?.ProductName ?? name} is already scanned!");
                return;
            }
            
            // Find the checkout counter this product belongs to
            CheckoutCounter checkoutCounter = FindAssociatedCheckoutCounter();
            if (checkoutCounter != null)
            {
                // Use the checkout counter's scan method to handle the scan
                checkoutCounter.ScanProduct(product);
                Debug.Log($"Scanned {product.ProductData?.ProductName ?? name} at checkout!");
            }
            else
            {
                Debug.LogWarning($"Could not find checkout counter for product {product.ProductData?.ProductName ?? name}");
            }
        }
        
        /// <summary>
        /// Check if this product is currently at a checkout counter (enhanced version)
        /// </summary>
        /// <returns>True if product is at checkout</returns>
        private bool IsProductAtCheckout()
        {
            // A product is at checkout if:
            // 1. It's not on shelf
            // 2. It's not purchased yet
            // 3. Its parent is a checkout area (or it's very close to a checkout counter)
            if (product.IsOnShelf || product.IsPurchased)
                return false;
            
            // Check if parent is a checkout area
            Transform parent = transform.parent;
            if (parent != null && parent.name.ToLower().Contains("checkout"))
                return true;
            
            // Alternative: Check proximity to checkout counter
            CheckoutCounter nearbyCheckout = FindNearestCheckoutCounter();
            if (nearbyCheckout != null)
            {
                float distance = Vector3.Distance(transform.position, nearbyCheckout.transform.position);
                return distance <= 5f; // Within 5 units of checkout counter
            }
            
            return false;
        }
        
        /// <summary>
        /// Find the checkout counter this product is associated with
        /// </summary>
        /// <returns>The associated checkout counter, or null</returns>
        private CheckoutCounter FindAssociatedCheckoutCounter()
        {
            // First try to find by parent relationship
            Transform parent = transform.parent;
            while (parent != null)
            {
                CheckoutCounter checkout = parent.GetComponent<CheckoutCounter>();
                if (checkout != null)
                    return checkout;
                parent = parent.parent;
            }
            
            // If not found by parent, find the nearest checkout counter
            return FindNearestCheckoutCounter();
        }
        
        /// <summary>
        /// Find the nearest checkout counter to this product
        /// </summary>
        /// <returns>The nearest checkout counter, or null</returns>
        private CheckoutCounter FindNearestCheckoutCounter()
        {
            CheckoutCounter[] checkouts = FindObjectsByType<CheckoutCounter>(FindObjectsSortMode.None);
            CheckoutCounter nearest = null;
            float nearestDistance = float.MaxValue;
            
            foreach (CheckoutCounter checkout in checkouts)
            {
                if (checkout != null)
                {
                    float distance = Vector3.Distance(transform.position, checkout.transform.position);
                    if (distance < nearestDistance)
                    {
                        nearestDistance = distance;
                        nearest = checkout;
                    }
                }
            }
            
            return nearest;
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
        
        /// <summary>
        /// Test product checkout scanning functionality
        /// </summary>
        [ContextMenu("Test Checkout Scanning")]
        public void TestCheckoutScanning()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("Checkout scanning test requires Play mode");
                return;
            }
            
            Debug.Log("=== PRODUCT CHECKOUT SCANNING TEST ===");
            Debug.Log($"Product: {product?.ProductData?.ProductName ?? name}");
            Debug.Log($"Is On Shelf: {product?.IsOnShelf ?? false}");
            Debug.Log($"Is Purchased: {product?.IsPurchased ?? false}");
            Debug.Log($"Is Scanned: {product?.IsScannedAtCheckout ?? false}");
            Debug.Log($"Is At Checkout: {IsAtCheckout()}");
            Debug.Log($"Can Interact: {CanInteract}");
            Debug.Log($"Interaction Text: {InteractionText}");
            Debug.Log($"Parent: {transform.parent?.name ?? "None"}");
            Debug.Log($"Position: {transform.position}");
            
            // Test finding checkout counter
            CheckoutCounter checkoutCounter = FindAssociatedCheckoutCounter();
            if (checkoutCounter != null)
            {
                Debug.Log($"Associated Checkout Counter: {checkoutCounter.name}");
            }
            else
            {
                Debug.LogWarning("No associated checkout counter found");
            }
        }
        
        #endregion
    }
}
