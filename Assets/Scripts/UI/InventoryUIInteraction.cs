using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace TabletopShop
{
    /// <summary>
    /// Handles user interactions with the inventory UI including button clicks, input processing, 
    /// and event management
    /// </summary>
    public class InventoryUIInteraction : MonoBehaviour
    {
        // Component references
        private Button[] productButtons;
        private InventoryManager inventoryManager;
        
        // Events for coordination with main InventoryUI
        public System.Action OnPanelToggleRequested;
        public System.Action OnDisplayUpdateRequested;
        
        private void Awake()
        {
            // Try to get InventoryManager instance early
            inventoryManager = InventoryManager.Instance;
            Debug.Log($"InventoryUIInteraction: InventoryManager instance in Awake: {inventoryManager != null}");
        }
        
        /// <summary>
        /// Initialize with external references
        /// </summary>
        public void Initialize(Button[] buttons, InventoryManager manager)
        {
            productButtons = buttons;
            inventoryManager = manager;
            
            SetupButtonClickEvents();
            SubscribeToInventoryEvents();
        }
        
        #region Unity Lifecycle
        
        private void Update()
        {
            // Tab key input to toggle panel visibility
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                OnPanelToggleRequested?.Invoke();
                
                // CursorManager will handle cursor state automatically
                // No need to manage cursor here to prevent conflicts
            }
        }
        
        private void OnDestroy()
        {
            UnsubscribeFromInventoryEvents();
        }
        
        #endregion
        
        #region Button Setup
        
        /// <summary>
        /// Set up button click events for all product buttons
        /// </summary>
        public void SetupButtonClickEvents()
        {
            if (productButtons == null) return;
            
            var productTypes = System.Enum.GetValues(typeof(ProductType));
            
            for (int i = 0; i < productButtons.Length && i < productTypes.Length; i++)
            {
                Button button = productButtons[i];
                if (button == null) continue;
                
                int buttonIndex = i; // Capture for closure
                
                // Clear existing listeners and add new one
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => {
                    Debug.Log($"InventoryUIInteraction: Button {buttonIndex} clicked! Calling OnProductButtonClick({buttonIndex})");
                    OnProductButtonClick(buttonIndex);
                });
                
                Debug.Log($"InventoryUIInteraction: Button {buttonIndex} click event set up");
            }
        }
        
        #endregion
        
        #region Event Management
        
        /// <summary>
        /// Subscribe to inventory manager events
        /// </summary>
        private void SubscribeToInventoryEvents()
        {
            if (inventoryManager == null)
            {
                inventoryManager = InventoryManager.Instance;
                Debug.Log($"InventoryUIInteraction: Retrieved InventoryManager instance: {inventoryManager != null}");
            }
            
            if (inventoryManager != null)
            {
                Debug.Log("InventoryUIInteraction: Subscribing to InventoryManager events");
                
                // Check if events exist and subscribe
                if (inventoryManager.OnInventoryChanged != null)
                {
                    inventoryManager.OnInventoryChanged.AddListener(OnInventoryChanged);
                    Debug.Log("InventoryUIInteraction: Subscribed to OnInventoryChanged");
                }
                else
                {
                    Debug.LogError("InventoryUIInteraction: OnInventoryChanged is null!");
                }
                
                if (inventoryManager.OnProductSelected != null)
                {
                    inventoryManager.OnProductSelected.AddListener(OnProductSelected);
                    Debug.Log("InventoryUIInteraction: Subscribed to OnProductSelected");
                }
                else
                {
                    Debug.LogError("InventoryUIInteraction: OnProductSelected is null!");
                }
                
                if (inventoryManager.OnProductCountChanged != null)
                {
                    inventoryManager.OnProductCountChanged.AddListener(OnProductCountChanged);
                    Debug.Log("InventoryUIInteraction: Subscribed to OnProductCountChanged");
                }
                else
                {
                    Debug.LogError("InventoryUIInteraction: OnProductCountChanged is null!");
                }
            }
            else
            {
                Debug.LogError("InventoryUIInteraction: InventoryManager is null after attempting to get instance!");
            }
        }
        
        /// <summary>
        /// Unsubscribe from inventory manager events
        /// </summary>
        private void UnsubscribeFromInventoryEvents()
        {
            if (inventoryManager != null)
            {
                inventoryManager.OnInventoryChanged.RemoveListener(OnInventoryChanged);
                inventoryManager.OnProductSelected.RemoveListener(OnProductSelected);
                inventoryManager.OnProductCountChanged.RemoveListener(OnProductCountChanged);
            }
        }
        
        /// <summary>
        /// Delayed initialization to ensure InventoryManager is ready
        /// </summary>
        public IEnumerator DelayedInitialization()
        {
            // Wait a frame to ensure InventoryManager Start() has completed
            yield return null;
            
            Debug.Log("InventoryUIInteraction: Performing delayed initialization");
            
            // Re-check InventoryManager and update display
            if (inventoryManager == null)
            {
                inventoryManager = InventoryManager.Instance;
                Debug.Log($"InventoryUIInteraction: Re-retrieved InventoryManager instance: {inventoryManager != null}");
            }
            
            // Request initial display update
            OnDisplayUpdateRequested?.Invoke();
        }
        
        #endregion
        
        #region Button Click Handling
        
        /// <summary>
        /// Handle product button click for selection
        /// </summary>
        public void OnProductButtonClick(int buttonIndex)
        {
            // Map button index to ProductType
            ProductType[] productTypes = { ProductType.MiniatureBox, ProductType.PaintPot, ProductType.Rulebook };
            
            if (buttonIndex < 0 || buttonIndex >= productTypes.Length)
            {
                Debug.LogError($"InventoryUIInteraction: Invalid button index {buttonIndex}! Expected 0-{productTypes.Length - 1}");
                return;
            }
            
            ProductType productType = productTypes[buttonIndex];
            
            Debug.Log($"InventoryUIInteraction: Button {buttonIndex} ({productType}) clicked!");
            
            if (inventoryManager == null)
            {
                Debug.LogError("InventoryUIInteraction: InventoryManager is null during button click!");
                return;
            }
            
            Debug.Log($"InventoryUIInteraction: Searching for product of type {productType}");
            
            // Find the first available product of this type
            ProductData productToSelect = null;
            
            foreach (var product in inventoryManager.AvailableProducts)
            {
                if (product != null && product.Type == productType && inventoryManager.HasProduct(product))
                {
                    productToSelect = product;
                    Debug.Log($"InventoryUIInteraction: Found available product: {product.ProductName}");
                    break;
                }
            }
            
            if (productToSelect != null)
            {
                Debug.Log($"InventoryUIInteraction: Attempting to select product: {productToSelect.ProductName}");
                bool success = inventoryManager.SelectProduct(productToSelect);
                if (success)
                {
                    Debug.Log($"InventoryUIInteraction: Successfully selected product: {productToSelect.ProductName} of type: {productType}");
                    // Selection highlight update will be handled by event callback
                }
                else
                {
                    Debug.LogWarning($"InventoryUIInteraction: Failed to select product: {productToSelect.ProductName}");
                }
            }
            else
            {
                Debug.LogWarning($"InventoryUIInteraction: No available products found for type: {productType}");
            }
        }
        
        #endregion
        
        #region Event Handlers
        
        /// <summary>
        /// Handle inventory changed event
        /// </summary>
        private void OnInventoryChanged()
        {
            Debug.Log("InventoryUIInteraction: OnInventoryChanged event received");
            OnDisplayUpdateRequested?.Invoke();
        }
        
        /// <summary>
        /// Handle product selection changed event
        /// </summary>
        private void OnProductSelected(ProductData selectedProduct)
        {
            Debug.Log($"InventoryUIInteraction: OnProductSelected event received - Product: {selectedProduct?.ProductName ?? "None"}");
            OnDisplayUpdateRequested?.Invoke();
        }
        
        /// <summary>
        /// Handle product count changed event
        /// </summary>
        private void OnProductCountChanged(ProductData product, int newCount)
        {
            Debug.Log($"InventoryUIInteraction: OnProductCountChanged event received - Product: {product?.ProductName ?? "None"}, New Count: {newCount}");
            OnDisplayUpdateRequested?.Invoke();
        }
        
        #endregion
        
        #region Public API for Coordinator
        
        /// <summary>
        /// Get the total count of all products of a specific type
        /// </summary>
        public int GetTotalCountForType(ProductType productType)
        {
            if (inventoryManager == null) return 0;
            
            int totalCount = 0;
            
            foreach (var product in inventoryManager.AvailableProducts)
            {
                if (product != null && product.Type == productType)
                {
                    totalCount += inventoryManager.GetProductCount(product);
                }
            }
            
            return totalCount;
        }
        
        /// <summary>
        /// Check if manager is available
        /// </summary>
        public bool IsManagerAvailable => inventoryManager != null;
        
        /// <summary>
        /// Get selected product from manager
        /// </summary>
        public ProductData GetSelectedProduct()
        {
            return inventoryManager?.SelectedProduct;
        }
        
        #endregion
    }
}
