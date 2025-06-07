using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace TabletopShop
{
    /// <summary>
    /// Handles visual effects and animations for the inventory UI including panel transitions, 
    /// button highlighting, and fade effects
    /// </summary>
    public class InventoryUIVisuals : MonoBehaviour
    {
        [Header("Animation Settings")]
        [SerializeField] private float fadeInDuration = 0.3f;
        [SerializeField] private float fadeOutDuration = 0.2f;
        
        [Header("Visual Feedback")]
        [SerializeField] private Color selectedButtonColor = Color.yellow;
        [SerializeField] private Color defaultButtonColor = Color.white;
        
        // Component references
        private CanvasGroup inventoryCanvasGroup;
        private Button[] productButtons;
        private Coroutine fadeCoroutine;
        
        // State tracking
        private bool isPanelVisible = false;
        
        // Public accessors for configuration
        public float FadeInDuration => fadeInDuration;
        public float FadeOutDuration => fadeOutDuration;
        public Color SelectedButtonColor => selectedButtonColor;
        public Color DefaultButtonColor => defaultButtonColor;
        public bool IsPanelVisible => isPanelVisible;
        
        private void Awake()
        {
            // Component references will be set during Initialize()
            // Don't try to find components here since they may not exist yet when dynamically added
        }
        
        /// <summary>
        /// Initialize with external references
        /// </summary>
        public void Initialize(CanvasGroup canvasGroup, Button[] buttons)
        {
            inventoryCanvasGroup = canvasGroup;
            productButtons = buttons;
            
            // Start with panel hidden
            SetPanelVisibility(false, false);
        }
        
        #region Panel Management
        
        /// <summary>
        /// Set the visibility of the inventory panel with smooth transitions
        /// </summary>
        /// <param name="visible">Whether the panel should be visible</param>
        /// <param name="animate">Whether to animate the transition</param>
        public void SetPanelVisibility(bool visible, bool animate = true)
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
        
        #region Visual Feedback
        
        /// <summary>
        /// Update visual selection highlight on buttons
        /// </summary>
        public void UpdateSelectionHighlight(ProductData selectedProduct)
        {
            if (productButtons == null) return;
            
            var productTypes = System.Enum.GetValues(typeof(ProductType));
            
            Debug.Log($"InventoryUIVisuals: UpdateSelectionHighlight - Currently selected product is {selectedProduct?.ProductName ?? "None"} of type {selectedProduct?.Type.ToString() ?? "None"}");
            
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
                        
                        Debug.Log($"InventoryUIVisuals: Button {i} ({productType}): Selected={isSelected}, Color={newColor}");
                    }
                }
            }
        }
        
        /// <summary>
        /// Update button visual state based on availability
        /// </summary>
        public void UpdateButtonVisualState(int buttonIndex, bool isAvailable)
        {
            if (productButtons == null || buttonIndex >= productButtons.Length) return;
            
            Button button = productButtons[buttonIndex];
            if (button == null) return;
            
            // Update button interactability
            button.interactable = isAvailable;
            
            // Could add additional visual feedback here (opacity, color, etc.)
        }
        
        /// <summary>
        /// Update count display on a specific button
        /// </summary>
        public void UpdateButtonCountDisplay(int buttonIndex, int count)
        {
            if (productButtons == null || buttonIndex >= productButtons.Length) return;
            
            Button button = productButtons[buttonIndex];
            if (button == null) return;
            
            // Update count text if available (specifically find ProductCount child)
            Transform countTransform = button.transform.Find("ProductCount");
            Text countText = countTransform?.GetComponent<Text>();
            if (countText != null)
            {
                countText.text = count.ToString();
                Debug.Log($"InventoryUIVisuals: Updated count text for button {buttonIndex} to: {count}");
            }
            else
            {
                Debug.LogWarning($"InventoryUIVisuals: No ProductCount text found for button {buttonIndex}");
            }
        }
        
        #endregion
        
        #region Legacy Field Migration
        
        /// <summary>
        /// Migrate legacy fields from main InventoryUI component
        /// </summary>
        public void MigrateLegacyFields(float legacyFadeInDuration, float legacyFadeOutDuration, 
                                       Color legacySelectedColor, Color legacyDefaultColor)
        {
            fadeInDuration = legacyFadeInDuration;
            fadeOutDuration = legacyFadeOutDuration;
            selectedButtonColor = legacySelectedColor;
            defaultButtonColor = legacyDefaultColor;
            
            Debug.Log("InventoryUIVisuals: Legacy fields migrated successfully");
        }
        
        #endregion
    }
}
