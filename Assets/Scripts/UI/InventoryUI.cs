using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace TabletopShop
{
    /// <summary>
    /// Manages the inventory UI system for the tabletop wargaming shop simulator.
    /// Handles inventory panel display, product button interactions, and real-time updates.
    /// </summary>
    public class InventoryUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private CanvasGroup inventoryCanvasGroup;
        [SerializeField] private Button[] productButtons;
        
        [Header("Animation Settings")]
        [SerializeField] private float fadeInDuration = 0.3f;
        [SerializeField] private float fadeOutDuration = 0.2f;
        
        [Header("Visual Feedback")]
        [SerializeField] private Color selectedButtonColor = Color.yellow;
        [SerializeField] private Color defaultButtonColor = Color.white;
        
        private bool isPanelVisible = false;
        private Coroutine fadeCoroutine;
        private InventoryManager inventoryManager;
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            Debug.Log("InventoryUI Awake() called");
            
            // Try to get InventoryManager instance early
            inventoryManager = InventoryManager.Instance;
            Debug.Log($"InventoryUI: InventoryManager instance in Awake: {inventoryManager != null}");
            
            // Validate required components
            if (inventoryCanvasGroup == null)
            {
                inventoryCanvasGroup = GetComponent<CanvasGroup>();
                if (inventoryCanvasGroup == null)
                {
                    Debug.LogError("InventoryUI: No CanvasGroup found! Adding one automatically.");
                    inventoryCanvasGroup = gameObject.AddComponent<CanvasGroup>();
                }
            }
            
            if (productButtons == null || productButtons.Length == 0)
            {
                Debug.LogWarning("InventoryUI: No product buttons assigned in Inspector!");
            }
        }
        
        private void Start()
        {
            Debug.Log("InventoryUI Start() called");
            
            // Ensure we have the InventoryManager instance
            if (inventoryManager == null)
            {
                inventoryManager = InventoryManager.Instance;
                Debug.Log($"InventoryUI: Retrieved InventoryManager instance: {inventoryManager != null}");
            }
            
            // Subscribe to inventory events with additional error checking
            if (inventoryManager != null)
            {
                Debug.Log("InventoryUI: Subscribing to InventoryManager events");
                
                // Check if events exist and subscribe
                if (inventoryManager.OnInventoryChanged != null)
                {
                    inventoryManager.OnInventoryChanged.AddListener(UpdateDisplay);
                    Debug.Log("InventoryUI: Subscribed to OnInventoryChanged");
                }
                else
                {
                    Debug.LogError("InventoryUI: OnInventoryChanged is null!");
                }
                
                if (inventoryManager.OnProductSelected != null)
                {
                    inventoryManager.OnProductSelected.AddListener(OnProductSelected);
                    Debug.Log("InventoryUI: Subscribed to OnProductSelected");
                }
                else
                {
                    Debug.LogError("InventoryUI: OnProductSelected is null!");
                }
                
                if (inventoryManager.OnProductCountChanged != null)
                {
                    inventoryManager.OnProductCountChanged.AddListener(OnProductCountChanged);
                    Debug.Log("InventoryUI: Subscribed to OnProductCountChanged");
                }
                else
                {
                    Debug.LogError("InventoryUI: OnProductCountChanged is null!");
                }
            }
            else
            {
                Debug.LogError("InventoryUI: InventoryManager is null after attempting to get instance!");
            }
            
            Debug.Log($"InventoryUI: productButtons array length: {productButtons?.Length ?? 0}");
            
            // Delay initial display update to ensure InventoryManager is fully initialized
            StartCoroutine(DelayedInitialization());
            
            // Start with panel hidden
            SetPanelVisibility(false, false);
        }
        
        /// <summary>
        /// Delayed initialization to ensure InventoryManager is ready
        /// </summary>
        private IEnumerator DelayedInitialization()
        {
            // Wait a frame to ensure InventoryManager Start() has completed
            yield return null;
            
            Debug.Log("InventoryUI: Performing delayed initialization");
            
            // Re-check InventoryManager and update display
            if (inventoryManager == null)
            {
                inventoryManager = InventoryManager.Instance;
                Debug.Log($"InventoryUI: Re-retrieved InventoryManager instance: {inventoryManager != null}");
            }
            
            // Initialize display
            UpdateDisplay();
        }
        
        private void Update()
        {
            // Tab key input to toggle panel visibility
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                TogglePanel();
                
                // CursorManager will handle cursor state automatically
                // No need to manage cursor here to prevent conflicts
            }
        }
        
        private void OnDestroy()
        {
            // Unsubscribe from events
            if (inventoryManager != null)
            {
                inventoryManager.OnInventoryChanged.RemoveListener(UpdateDisplay);
                inventoryManager.OnProductSelected.RemoveListener(OnProductSelected);
                inventoryManager.OnProductCountChanged.RemoveListener(OnProductCountChanged);
            }
        }
        
        #endregion
        
        #region Panel Management
        
        /// <summary>
        /// Toggle the visibility of the inventory panel
        /// </summary>
        public void TogglePanel()
        {
            SetPanelVisibility(!isPanelVisible);
        }
        
        /// <summary>
        /// Set the visibility of the inventory panel with smooth transitions
        /// </summary>
        /// <param name="visible">Whether the panel should be visible</param>
        /// <param name="animate">Whether to animate the transition</param>
        private void SetPanelVisibility(bool visible, bool animate = true)
        {
            if (isPanelVisible == visible) return;
            
            isPanelVisible = visible;
            
            if (fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
            }
            
            if (animate)
            {
                fadeCoroutine = StartCoroutine(AnimatePanelFade(visible));
            }
            else
            {
                inventoryCanvasGroup.alpha = visible ? 1f : 0f;
                inventoryCanvasGroup.interactable = visible;
                inventoryCanvasGroup.blocksRaycasts = visible;
            }
        }
        
        /// <summary>
        /// Animate the panel fade in/out
        /// </summary>
        private IEnumerator AnimatePanelFade(bool fadeIn)
        {
            float duration = fadeIn ? fadeInDuration : fadeOutDuration;
            float startAlpha = inventoryCanvasGroup.alpha;
            float targetAlpha = fadeIn ? 1f : 0f;
            
            // Set interactability for fade in
            if (fadeIn)
            {
                inventoryCanvasGroup.interactable = true;
                inventoryCanvasGroup.blocksRaycasts = true;
            }
            
            float elapsedTime = 0f;
            
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / duration;
                
                inventoryCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
                
                yield return null;
            }
            
            inventoryCanvasGroup.alpha = targetAlpha;
            
            // Set interactability for fade out
            if (!fadeIn)
            {
                inventoryCanvasGroup.interactable = false;
                inventoryCanvasGroup.blocksRaycasts = false;
            }
            
            fadeCoroutine = null;
        }
        
        #endregion
        
        #region Display Management
        
        /// <summary>
        /// Update the display of all product buttons
        /// </summary>
        public void UpdateDisplay()
        {
            Debug.Log($"UpdateDisplay called - inventoryManager: {inventoryManager != null}, productButtons: {productButtons?.Length ?? 0}");
            
            if (inventoryManager == null || productButtons == null) return;
            
            var productTypes = System.Enum.GetValues(typeof(ProductType));
            Debug.Log($"UpdateDisplay - Found {productTypes.Length} product types");
            
            for (int i = 0; i < productButtons.Length && i < productTypes.Length; i++)
            {
                ProductType productType = (ProductType)productTypes.GetValue(i);
                Debug.Log($"UpdateDisplay - Processing button {i} for type {productType}");
                UpdateProductButton(i, productType);
            }
            
            // Update selection highlight
            UpdateSelectionHighlight();
        }
        
        /// <summary>
        /// Update a specific product button
        /// </summary>
        private void UpdateProductButton(int buttonIndex, ProductType productType)
        {
            if (buttonIndex >= productButtons.Length) 
            {
                Debug.LogWarning($"InventoryUI: Button index {buttonIndex} out of range (array length: {productButtons.Length})");
                return;
            }
            
            Button button = productButtons[buttonIndex];
            if (button == null) 
            {
                Debug.LogWarning($"InventoryUI: Button at index {buttonIndex} is null");
                return;
            }
            
            Debug.Log($"InventoryUI: Updating button {buttonIndex} for product type {productType}");
            
            // Get total count for this product type
            int totalCount = GetTotalCountForType(productType);
            Debug.Log($"InventoryUI: Total count for {productType}: {totalCount}");
            
            // Update button interactability
            button.interactable = totalCount > 0;
            
            // Update count text if available (specifically find ProductCount child)
            Transform countTransform = button.transform.Find("ProductCount");
            Text countText = countTransform?.GetComponent<Text>();
            if (countText != null)
            {
                countText.text = totalCount.ToString();
                Debug.Log($"InventoryUI: Updated count text for button {buttonIndex} to: {totalCount}");
            }
            else
            {
                Debug.LogWarning($"InventoryUI: No ProductCount text found for button {buttonIndex}");
            }
            
            // Set up button click event
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => {
                Debug.Log($"InventoryUI: Button {buttonIndex} clicked! Calling OnProductButtonClick({buttonIndex})");
                OnProductButtonClick(buttonIndex);
            });
            
            Debug.Log($"InventoryUI: Button {buttonIndex} click event set up for {productType}");
        }
        
        /// <summary>
        /// Get the total count of all products of a specific type
        /// </summary>
        private int GetTotalCountForType(ProductType productType)
        {
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
        /// Update visual selection highlight on buttons
        /// </summary>
        private void UpdateSelectionHighlight()
        {
            if (inventoryManager == null || productButtons == null) return;
            
            ProductData selectedProduct = inventoryManager.SelectedProduct;
            var productTypes = System.Enum.GetValues(typeof(ProductType));
            
            Debug.Log($"UpdateSelectionHighlight: Currently selected product is {selectedProduct?.ProductName ?? "None"} of type {selectedProduct?.Type.ToString() ?? "None"}");
            
            for (int i = 0; i < productButtons.Length && i < productTypes.Length; i++)
            {
                ProductType productType = (ProductType)productTypes.GetValue(i);
                Button button = productButtons[i];
                
                if (button != null)
                {
                    Image buttonImage = button.GetComponent<Image>();
                    if (buttonImage != null)
                    {
                        bool isSelected = selectedProduct != null && selectedProduct.Type == productType;
                        Color newColor = isSelected ? selectedButtonColor : defaultButtonColor;
                        buttonImage.color = newColor;
                        
                        Debug.Log($"Button {i} ({productType}): Selected={isSelected}, Color={newColor}");
                    }
                }
            }
        }
        
        #endregion
        
        #region Event Handlers
        
        /// <summary>
        /// Handle product button click for selection
        /// </summary>
        public void OnProductButtonClick(int buttonIndex)
        {
            // Map button index to ProductType
            ProductType[] productTypes = { ProductType.MiniatureBox, ProductType.PaintPot, ProductType.Rulebook };
            
            if (buttonIndex < 0 || buttonIndex >= productTypes.Length)
            {
                Debug.LogError($"InventoryUI: Invalid button index {buttonIndex}! Expected 0-{productTypes.Length - 1}");
                return;
            }
            
            ProductType productType = productTypes[buttonIndex];
            
            Debug.Log($"Button {buttonIndex} ({productType}) clicked!");

            Debug.Log($"InventoryUI: Button clicked for product type: {productType}");
            
            if (inventoryManager == null)
            {
                Debug.LogError("InventoryUI: InventoryManager is null during button click!");
                return;
            }
            
            Debug.Log($"InventoryUI: Searching for product of type {productType}");
            
            // Find the first available product of this type
            ProductData productToSelect = null;
            
            foreach (var product in inventoryManager.AvailableProducts)
            {
                if (product != null && product.Type == productType && inventoryManager.HasProduct(product))
                {
                    productToSelect = product;
                    Debug.Log($"InventoryUI: Found available product: {product.ProductName}");
                    break;
                }
            }
            
            if (productToSelect != null)
            {
                Debug.Log($"InventoryUI: Attempting to select product: {productToSelect.ProductName}");
                bool success = inventoryManager.SelectProduct(productToSelect);
                if (success)
                {
                    Debug.Log($"InventoryUI: Successfully selected product: {productToSelect.ProductName} of type: {productType}");
                    // Force immediate visual update
                    UpdateSelectionHighlight();
                }
                else
                {
                    Debug.LogWarning($"InventoryUI: Failed to select product: {productToSelect.ProductName}");
                }
            }
            else
            {
                Debug.LogWarning($"InventoryUI: No available products found for type: {productType}");
            }
        }
        
        /// <summary>
        /// Handle product selection changed event
        /// </summary>
        private void OnProductSelected(ProductData selectedProduct)
        {
            Debug.Log($"InventoryUI: OnProductSelected event received - Product: {selectedProduct?.ProductName ?? "None"}");
            UpdateSelectionHighlight();
        }
        
        /// <summary>
        /// Handle product count changed event
        /// </summary>
        private void OnProductCountChanged(ProductData product, int newCount)
        {
            Debug.Log($"InventoryUI: OnProductCountChanged event received - Product: {product?.ProductName ?? "None"}, New Count: {newCount}");
            UpdateDisplay();
        }
        
        #endregion
    }
}
