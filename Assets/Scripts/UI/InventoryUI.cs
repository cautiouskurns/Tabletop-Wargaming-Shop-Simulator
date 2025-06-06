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
            inventoryManager = InventoryManager.Instance;
        }
        
        private void Start()
        {
            Debug.Log("InventoryUI Start() called");
            
            // Subscribe to inventory events
            if (inventoryManager != null)
            {
                Debug.Log("InventoryUI: Subscribing to InventoryManager events");
                inventoryManager.OnInventoryChanged.AddListener(UpdateDisplay);
                inventoryManager.OnProductSelected.AddListener(OnProductSelected);
                inventoryManager.OnProductCountChanged.AddListener(OnProductCountChanged);
            }
            else
            {
                Debug.LogError("InventoryUI: InventoryManager is null!");
            }
            
            Debug.Log($"InventoryUI: productButtons array length: {productButtons?.Length ?? 0}");
            
            // Initialize display
            UpdateDisplay();
            
            // Start with panel hidden
            SetPanelVisibility(false, false);
        }
        
        private void Update()
        {
            // Tab key input to toggle panel visibility
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                TogglePanel();
                
                // Auto-manage cursor when toggling inventory
                CursorManager cursorManager = FindAnyObjectByType<CursorManager>();
                if (cursorManager != null)
                {
                    // Unlock cursor when showing inventory, lock when hiding
                    cursorManager.SetCursorState(!isPanelVisible);
                }
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
            if (buttonIndex >= productButtons.Length) return;
            
            Button button = productButtons[buttonIndex];
            if (button == null) return;
            
            // Get total count for this product type
            int totalCount = GetTotalCountForType(productType);
            
            // Update button interactability
            button.interactable = totalCount > 0;
            
            // Update count text if available (specifically find ProductCount child)
            Transform countTransform = button.transform.Find("ProductCount");
            Text countText = countTransform?.GetComponent<Text>();
            if (countText != null)
            {
                countText.text = totalCount.ToString();
            }
            
            // Set up button click event
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => OnProductButtonClick(productType));
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
        public void OnProductButtonClick(ProductType productType)
        {
            if (inventoryManager == null) return;
            
            // Find the first available product of this type
            ProductData productToSelect = null;
            
            foreach (var product in inventoryManager.AvailableProducts)
            {
                if (product != null && product.Type == productType && inventoryManager.HasProduct(product))
                {
                    productToSelect = product;
                    break;
                }
            }
            
            if (productToSelect != null)
            {
                bool success = inventoryManager.SelectProduct(productToSelect);
                if (success)
                {
                    Debug.Log($"Selected product: {productToSelect.ProductName} of type: {productType}");
                    // Force immediate visual update
                    UpdateSelectionHighlight();
                }
                else
                {
                    Debug.LogWarning($"Failed to select product: {productToSelect.ProductName}");
                }
            }
            else
            {
                Debug.Log($"No available products found for type: {productType}");
            }
        }
        
        /// <summary>
        /// Handle product selection changed event
        /// </summary>
        private void OnProductSelected(ProductData selectedProduct)
        {
            Debug.Log($"InventoryUI: Product selection changed to: {selectedProduct?.ProductName ?? "None"}");
            UpdateSelectionHighlight();
        }
        
        /// <summary>
        /// Handle product count changed event
        /// </summary>
        private void OnProductCountChanged(ProductData product, int newCount)
        {
            UpdateDisplay();
        }
        
        #endregion
    }
}
