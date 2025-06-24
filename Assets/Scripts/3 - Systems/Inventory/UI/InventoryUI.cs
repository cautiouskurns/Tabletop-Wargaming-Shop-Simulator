using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace TabletopShop
{
    /// <summary>
    /// Manages the inventory UI system for the tabletop wargaming shop simulator.
    /// Uses composition pattern to coordinate visuals and interactions while maintaining the same public interface.
    /// Handles inventory panel display, product button interactions, and real-time updates.
    /// </summary>
    public class InventoryUI : MonoBehaviour
    {
        // Legacy serialized fields for backward compatibility - these will be migrated to components
        [Header("UI References")]
        [SerializeField] private CanvasGroup inventoryCanvasGroup;
        [SerializeField] private Button[] productButtons;
        
        [Header("Animation Settings")]
        [SerializeField] private float fadeInDuration = 0.3f;
        [SerializeField] private float fadeOutDuration = 0.2f;
        
        [Header("Visual Feedback")]
        [SerializeField] private Color selectedButtonColor = Color.yellow;
        [SerializeField] private Color defaultButtonColor = Color.white;
        
        // Component references
        private InventoryUIVisuals uiVisuals;
        private InventoryUIInteraction uiInteraction;
        private InventoryManager inventoryManager;
        
        // Migration flag
        [HideInInspector]
        [SerializeField] private bool hasBeenMigrated = false;
        
        // Legacy state for backward compatibility
        private bool isPanelVisible = false;
        private Coroutine fadeCoroutine;
        
        #region Unity Lifecycle
        
        private void Awake()
        {
//            Debug.Log("InventoryUI Awake() called");
            
            // Ensure component initialization
            EnsureComponents();
            
            // Try to get InventoryManager instance early
            inventoryManager = InventoryManager.Instance;
//            Debug.Log($"InventoryUI: InventoryManager instance in Awake: {inventoryManager != null}");
            
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
            
            // Initialize components with references
            InitializeComponents();
        }
        
        private void Start()
        {
//            Debug.Log("InventoryUI Start() called");
            
            // Ensure we have the InventoryManager instance
            if (inventoryManager == null)
            {
                inventoryManager = InventoryManager.Instance;
                Debug.Log($"InventoryUI: Retrieved InventoryManager instance: {inventoryManager != null}");
            }
            
            // Setup event handlers through interaction component
            if (EnsureInteractionComponent())
            {
                uiInteraction.OnPanelToggleRequested += TogglePanel;
                uiInteraction.OnDisplayUpdateRequested += UpdateDisplay;
            }
            
//            Debug.Log($"InventoryUI: productButtons array length: {productButtons?.Length ?? 0}");
            
            // Delay initial display update to ensure InventoryManager is fully initialized
            StartCoroutine(DelayedInitialization());
            
            // Start with panel hidden
            TogglePanelVisibility(false, false);
        }
        
        /// <summary>
        /// Delayed initialization to ensure InventoryManager is ready
        /// </summary>
        private IEnumerator DelayedInitialization()
        {
            // Delegate to interaction component for delayed initialization
            if (EnsureInteractionComponent())
            {
                yield return StartCoroutine(uiInteraction.DelayedInitialization());
            }
            else
            {
                // Fallback to original logic if component not available
                yield return null;
                Debug.Log("InventoryUI: Performing delayed initialization (fallback)");
                
                if (inventoryManager == null)
                {
                    inventoryManager = InventoryManager.Instance;
                    Debug.Log($"InventoryUI: Re-retrieved InventoryManager instance: {inventoryManager != null}");
                }
                
                UpdateDisplay();
            }
        }
        
        private void Update()
        {
            // Input handling is now managed by interaction component
            // This method is kept for backward compatibility but delegates work
        }
        
        private void OnDestroy()
        {
            // Cleanup component event handlers
            if (uiInteraction != null)
            {
                uiInteraction.OnPanelToggleRequested -= TogglePanel;
                uiInteraction.OnDisplayUpdateRequested -= UpdateDisplay;
            }
        }
        
        #endregion
        
        #region Component Management
        
        /// <summary>
        /// Ensure all required components are present
        /// </summary>
        private void EnsureComponents()
        {
            EnsureVisualsComponent();
            EnsureInteractionComponent();
        }
        
        /// <summary>
        /// Ensure visuals component exists and return success
        /// </summary>
        private bool EnsureVisualsComponent()
        {
            if (uiVisuals == null)
            {
                uiVisuals = GetComponent<InventoryUIVisuals>();
                if (uiVisuals == null)
                {
                    uiVisuals = gameObject.AddComponent<InventoryUIVisuals>();
//                    Debug.Log("InventoryUI: Added InventoryUIVisuals component");
                }
            }
            return uiVisuals != null;
        }
        
        /// <summary>
        /// Ensure interaction component exists and return success
        /// </summary>
        private bool EnsureInteractionComponent()
        {
            if (uiInteraction == null)
            {
                uiInteraction = GetComponent<InventoryUIInteraction>();
                if (uiInteraction == null)
                {
                    uiInteraction = gameObject.AddComponent<InventoryUIInteraction>();
//                    Debug.Log("InventoryUI: Added InventoryUIInteraction component");
                }
            }
            return uiInteraction != null;
        }
        
        /// <summary>
        /// Initialize components with references and migrate legacy fields
        /// </summary>
        private void InitializeComponents()
        {
            if (!hasBeenMigrated)
            {
                MigrateLegacyFields();
                hasBeenMigrated = true;
            }
            
            // Initialize visuals component
            if (EnsureVisualsComponent())
            {
                uiVisuals.Initialize(inventoryCanvasGroup, productButtons);
            }
            
            // Initialize interaction component
            if (EnsureInteractionComponent())
            {
                uiInteraction.Initialize(productButtons, inventoryManager);
            }
        }
        
        /// <summary>
        /// Migrate legacy serialized fields to components
        /// </summary>
        private void MigrateLegacyFields()
        {
            if (EnsureVisualsComponent())
            {
                uiVisuals.MigrateLegacyFields(fadeInDuration, fadeOutDuration, selectedButtonColor, defaultButtonColor);
            }
            
//            Debug.Log("InventoryUI: Legacy fields migrated to components");
        }
        
        #endregion
        
        #region Panel Management
        
        /// <summary>
        /// Toggle the visibility of the inventory panel
        /// </summary>
        public void TogglePanel()
        {
            bool newVisibility = EnsureVisualsComponent() ? !uiVisuals.IsPanelVisible : !isPanelVisible;
            TogglePanelVisibility(newVisibility);
        }
        
        /// <summary>
        /// Set the visibility of the inventory panel with smooth transitions
        /// </summary>
        /// <param name="visible">Whether the panel should be visible</param>
        /// <param name="animate">Whether to animate the transition</param>
        private void TogglePanelVisibility(bool visible, bool animate = true)
        {
            if (EnsureVisualsComponent())
            {
                uiVisuals.SetPanelVisibility(visible, animate);
                isPanelVisible = uiVisuals.IsPanelVisible; // Keep legacy state in sync
            }
            else
            {
                // Fallback to legacy behavior if component not available
                SetPanelVisibilityLegacy(visible, animate);
            }
        }
        
        /// <summary>
        /// Legacy panel visibility method for backward compatibility
        /// </summary>
        private void SetPanelVisibilityLegacy(bool visible, bool animate = true)
        {
            if (isPanelVisible == visible) return;
            
            isPanelVisible = visible;
            
            if (fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
            }
            
            if (animate)
            {
                fadeCoroutine = StartCoroutine(AnimatePanelFadeLegacy(visible));
            }
            else
            {
                inventoryCanvasGroup.alpha = visible ? 1f : 0f;
                inventoryCanvasGroup.interactable = visible;
                inventoryCanvasGroup.blocksRaycasts = visible;
            }
        }
        
        /// <summary>
        /// Legacy animate the panel fade in/out
        /// </summary>
        private IEnumerator AnimatePanelFadeLegacy(bool fadeIn)
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
            // Debug.Log($"UpdateDisplay called - inventoryManager: {inventoryManager != null}, productButtons: {productButtons?.Length ?? 0}");
            
            if (inventoryManager == null || productButtons == null) return;
            
            var productTypes = System.Enum.GetValues(typeof(ProductType));
            // Debug.Log($"UpdateDisplay - Found {productTypes.Length} product types");
            
            for (int i = 0; i < productButtons.Length && i < productTypes.Length; i++)
            {
                ProductType productType = (ProductType)productTypes.GetValue(i);
//                Debug.Log($"UpdateDisplay - Processing button {i} for type {productType}");
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
                // Debug.LogWarning($"InventoryUI: Button index {buttonIndex} out of range (array length: {productButtons.Length})");
                return;
            }
            
            Button button = productButtons[buttonIndex];
            if (button == null) 
            {
                // Debug.LogWarning($"InventoryUI: Button at index {buttonIndex} is null");
                return;
            }
            
//            Debug.Log($"InventoryUI: Updating button {buttonIndex} for product type {productType}");
            
            // Get total count for this product type (delegate to interaction component if available)
            int totalCount = EnsureInteractionComponent() ? 
                uiInteraction.GetTotalCountForType(productType) : 
                GetTotalCountForTypeLegacy(productType);
            
            // Debug.Log($"InventoryUI: Total count for {productType}: {totalCount}");
            
            // Update button visual state through visuals component
            if (EnsureVisualsComponent())
            {
                uiVisuals.UpdateButtonVisualState(buttonIndex, totalCount > 0);
                uiVisuals.UpdateButtonCountDisplay(buttonIndex, totalCount);
            }
            else
            {
                // Fallback to legacy behavior
                UpdateProductButtonLegacy(buttonIndex, totalCount);
            }
        }
        
        /// <summary>
        /// Legacy method to update product button
        /// </summary>
        private void UpdateProductButtonLegacy(int buttonIndex, int totalCount)
        {
            Button button = productButtons[buttonIndex];
            
            // Update button interactability
            button.interactable = totalCount > 0;
            
            // Update count text if available (specifically find ProductCount child)
            Transform countTransform = button.transform.Find("ProductCount");
            Text countText = countTransform?.GetComponent<Text>();
            if (countText != null)
            {
                countText.text = totalCount.ToString();
                // Debug.Log($"InventoryUI: Updated count text for button {buttonIndex} to: {totalCount}");
            }
            else
            {
                // Debug.LogWarning($"InventoryUI: No ProductCount text found for button {buttonIndex}");
            }
            
            // Set up button click event
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => {
                // Debug.Log($"InventoryUI: Button {buttonIndex} clicked! Calling OnProductButtonClick({buttonIndex})");
                OnProductButtonClick(buttonIndex);
            });
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
        /// Legacy method to get the total count of all products of a specific type
        /// </summary>
        private int GetTotalCountForTypeLegacy(ProductType productType)
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
            if (EnsureVisualsComponent() && EnsureInteractionComponent())
            {
                ProductData selectedProduct = uiInteraction.GetSelectedProduct();
                uiVisuals.UpdateSelectionHighlight(selectedProduct);
            }
            else
            {
                // Fallback to legacy behavior
                UpdateSelectionHighlightLegacy();
            }
        }
        
        /// <summary>
        /// Legacy method to update visual selection highlight on buttons
        /// </summary>
        private void UpdateSelectionHighlightLegacy()
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

        /// <summary>
        /// Handle product button click for selection
        /// </summary>
        public void OnProductButtonClick(int buttonIndex)
        {
            // Delegate to interaction component if available, otherwise use legacy behavior
            if (EnsureInteractionComponent())
            {
                uiInteraction.OnProductButtonClick(buttonIndex);
            }
            else
            {
                OnProductButtonClickLegacy(buttonIndex);
            }
        }
        
        /// <summary>
        /// Legacy product button click handler for backward compatibility
        /// </summary>
        private void OnProductButtonClickLegacy(int buttonIndex)
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
        
        #endregion
    }
}
