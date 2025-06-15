using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TabletopShop
{
    /// <summary>
    /// Simple UI for displaying checkout status and product list
    /// 
    /// USAGE EXAMPLE:
    /// 1. Create CheckoutUI manually: CheckoutUI ui = CheckoutUI.CreateBasicCheckoutUI(canvas);
    /// 2. Connect to checkout: ui.TrackCheckoutCounter(checkoutCounter);
    /// 3. Update manually: ui.UpdateTotal(total); ui.AddProduct(product); ui.UpdateProductScanStatus(product);
    /// 4. Show/Hide: ui.Show(); ui.Hide();
    /// 
    /// The UI will automatically fade in/out and display:
    /// - Customer name (or "No Customer")
    /// - Running total in yellow text
    /// - Scrollable product list with scan status (✓ scanned, - unscanned)
    /// - Color coding: Green for scanned products, White for unscanned
    /// </summary>
    public class CheckoutUI : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Text customerNameText;
        [SerializeField] private Text runningTotalText;
        [SerializeField] private Transform productListParent;
        [SerializeField] private GameObject productItemPrefab;
        
        [Header("UI Settings")]
        [SerializeField] private float fadeSpeed = 5f;
        [SerializeField] private bool showByDefault = false;
        
        [Header("Styling")]
        [SerializeField] private Color scannedColor = Color.green;
        [SerializeField] private Color unscannedColor = Color.white;
        
        // State tracking
        private bool isVisible = false;
        private float targetAlpha = 0f;
        private Dictionary<Product, GameObject> productUIItems = new Dictionary<Product, GameObject>();
        private CheckoutCounter trackedCheckout = null;
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            // Get components if not assigned
            if (canvasGroup == null)
                canvasGroup = GetComponent<CanvasGroup>();
            
            // Set initial state
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            }
            
            // Hide initially unless specified
            isVisible = showByDefault;
            targetAlpha = showByDefault ? 1f : 0f;
        }
        
        private void Update()
        {
            // Handle fade in/out
            if (canvasGroup != null)
            {
                canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, targetAlpha, Time.deltaTime * fadeSpeed);
                
                // Update interactability based on visibility
                bool shouldBeInteractable = targetAlpha > 0.5f;
                if (canvasGroup.interactable != shouldBeInteractable)
                {
                    canvasGroup.interactable = shouldBeInteractable;
                    canvasGroup.blocksRaycasts = shouldBeInteractable;
                }
            }
        }
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// Show the checkout UI
        /// </summary>
        public void Show()
        {
            isVisible = true;
            targetAlpha = 1f;
        }
        
        /// <summary>
        /// Hide the checkout UI
        /// </summary>
        public void Hide()
        {
            isVisible = false;
            targetAlpha = 0f;
        }
        
        /// <summary>
        /// Toggle the checkout UI visibility
        /// </summary>
        public void Toggle()
        {
            if (isVisible)
                Hide();
            else
                Show();
        }
        
        /// <summary>
        /// Update the running total display
        /// </summary>
        /// <param name="total">New total amount</param>
        public void UpdateTotal(float total)
        {
            if (runningTotalText != null)
            {
                runningTotalText.text = $"Total: ${total:F2}";
            }
        }
        
        /// <summary>
        /// Update the customer name display
        /// </summary>
        /// <param name="customerName">Name of current customer</param>
        public void UpdateCustomer(string customerName)
        {
            if (customerNameText != null)
            {
                if (string.IsNullOrEmpty(customerName))
                {
                    customerNameText.text = "No Customer";
                }
                else
                {
                    customerNameText.text = $"Customer: {customerName}";
                }
            }
        }
        
        /// <summary>
        /// Add a product to the checkout UI list
        /// </summary>
        /// <param name="product">Product to add</param>
        public void AddProduct(Product product)
        {
            if (product == null || productUIItems.ContainsKey(product))
                return;
            
            if (productListParent == null || productItemPrefab == null)
            {
                Debug.LogWarning("CheckoutUI: Missing product list components");
                return;
            }
            
            // Create UI item for product
            GameObject productItem = Instantiate(productItemPrefab, productListParent);
            productUIItems[product] = productItem;
            
            // Update the product item display
            UpdateProductItem(product, productItem);
        }
        
        /// <summary>
        /// Remove a product from the checkout UI list
        /// </summary>
        /// <param name="product">Product to remove</param>
        public void RemoveProduct(Product product)
        {
            if (product == null || !productUIItems.ContainsKey(product))
                return;
            
            GameObject productItem = productUIItems[product];
            if (productItem != null)
            {
                Destroy(productItem);
            }
            
            productUIItems.Remove(product);
        }
        
        /// <summary>
        /// Update a product's scan status display
        /// </summary>
        /// <param name="product">Product to update</param>
        public void UpdateProductScanStatus(Product product)
        {
            if (product == null || !productUIItems.ContainsKey(product))
                return;
            
            GameObject productItem = productUIItems[product];
            UpdateProductItem(product, productItem);
        }
        
        /// <summary>
        /// Clear all products from the UI
        /// </summary>
        public void ClearProducts()
        {
            foreach (var kvp in productUIItems)
            {
                if (kvp.Value != null)
                {
                    Destroy(kvp.Value);
                }
            }
            productUIItems.Clear();
        }
        
        /// <summary>
        /// Connect to a checkout counter for automatic updates
        /// </summary>
        /// <param name="checkoutCounter">Checkout counter to track</param>
        public void TrackCheckoutCounter(CheckoutCounter checkoutCounter)
        {
            // Disconnect from previous checkout if any
            if (trackedCheckout != null)
            {
                DisconnectFromCheckout();
            }
            
            trackedCheckout = checkoutCounter;
            
            if (trackedCheckout != null)
            {
                // Note: In a full implementation, we'd subscribe to checkout events here
                // For now, this provides the framework for future event integration
                Debug.Log($"CheckoutUI: Now tracking {trackedCheckout.name}");
            }
        }
        
        /// <summary>
        /// Disconnect from current checkout counter
        /// </summary>
        public void DisconnectFromCheckout()
        {
            if (trackedCheckout != null)
            {
                Debug.Log($"CheckoutUI: Stopped tracking {trackedCheckout.name}");
                trackedCheckout = null;
            }
            
            ClearProducts();
            UpdateCustomer(null);
            UpdateTotal(0f);
        }
        
        #endregion
        
        #region Private Methods
        
        /// <summary>
        /// Update the visual display of a product item
        /// </summary>
        /// <param name="product">Product to update</param>
        /// <param name="productItem">UI GameObject for the product</param>
        private void UpdateProductItem(Product product, GameObject productItem)
        {
            if (product == null || productItem == null)
                return;
            
            // Get text component
            Text productText = productItem.GetComponent<Text>();
            if (productText == null)
                productText = productItem.GetComponentInChildren<Text>();
            
            if (productText != null)
            {
                // Format product name and scan status
                string productName = product.ProductData?.ProductName ?? product.name;
                string scanIcon = product.IsScannedAtCheckout ? "✓" : "-";
                string priceText = product.CurrentPrice > 0 ? $" (${product.CurrentPrice:F2})" : "";
                
                productText.text = $"{scanIcon} {productName}{priceText}";
                
                // Update color based on scan status
                productText.color = product.IsScannedAtCheckout ? scannedColor : unscannedColor;
            }
        }
        
        #endregion
        
        #region Static Factory Method
        
        /// <summary>
        /// Create a basic checkout UI instance
        /// </summary>
        /// <param name="parent">Parent canvas for the UI</param>
        /// <returns>Created CheckoutUI instance</returns>
        public static CheckoutUI CreateBasicCheckoutUI(Canvas parent)
        {
            if (parent == null)
            {
                Debug.LogError("CheckoutUI: Cannot create UI without parent canvas");
                return null;
            }
            
            // Create main panel
            GameObject checkoutPanel = new GameObject("CheckoutUI");
            checkoutPanel.transform.SetParent(parent.transform, false);
            
            // Add RectTransform and position it
            RectTransform rectTransform = checkoutPanel.AddComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0.7f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.95f, 0.9f);
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            
            // Add background image
            Image background = checkoutPanel.AddComponent<Image>();
            background.color = new Color(0f, 0f, 0f, 0.7f);
            
            // Add CanvasGroup for fading
            CanvasGroup canvasGroup = checkoutPanel.AddComponent<CanvasGroup>();
            
            // Add CheckoutUI component
            CheckoutUI checkoutUI = checkoutPanel.AddComponent<CheckoutUI>();
            checkoutUI.canvasGroup = canvasGroup;
            
            // Create customer name text
            GameObject customerObj = new GameObject("CustomerText");
            customerObj.transform.SetParent(checkoutPanel.transform, false);
            Text customerText = customerObj.AddComponent<Text>();
            customerText.text = "No Customer";
            customerText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            customerText.fontSize = 16;
            customerText.color = Color.white;
            
            RectTransform customerRect = customerObj.GetComponent<RectTransform>();
            customerRect.anchorMin = new Vector2(0.05f, 0.85f);
            customerRect.anchorMax = new Vector2(0.95f, 0.95f);
            customerRect.offsetMin = Vector2.zero;
            customerRect.offsetMax = Vector2.zero;
            
            checkoutUI.customerNameText = customerText;
            
            // Create total text
            GameObject totalObj = new GameObject("TotalText");
            totalObj.transform.SetParent(checkoutPanel.transform, false);
            Text totalText = totalObj.AddComponent<Text>();
            totalText.text = "Total: $0.00";
            totalText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            totalText.fontSize = 18;
            totalText.color = Color.yellow;
            totalText.fontStyle = FontStyle.Bold;
            
            RectTransform totalRect = totalObj.GetComponent<RectTransform>();
            totalRect.anchorMin = new Vector2(0.05f, 0.05f);
            totalRect.anchorMax = new Vector2(0.95f, 0.15f);
            totalRect.offsetMin = Vector2.zero;
            totalRect.offsetMax = Vector2.zero;
            
            checkoutUI.runningTotalText = totalText;
            
            // Create product list area
            GameObject listObj = new GameObject("ProductList");
            listObj.transform.SetParent(checkoutPanel.transform, false);
            
            RectTransform listRect = listObj.AddComponent<RectTransform>();
            listRect.anchorMin = new Vector2(0.05f, 0.2f);
            listRect.anchorMax = new Vector2(0.95f, 0.8f);
            listRect.offsetMin = Vector2.zero;
            listRect.offsetMax = Vector2.zero;
            
            // Add scroll view for product list
            ScrollRect scrollRect = listObj.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            
            // Create content area
            GameObject contentObj = new GameObject("Content");
            contentObj.transform.SetParent(listObj.transform, false);
            RectTransform contentRect = contentObj.AddComponent<RectTransform>();
            contentRect.anchorMin = Vector2.zero;
            contentRect.anchorMax = Vector2.one;
            contentRect.offsetMin = Vector2.zero;
            contentRect.offsetMax = Vector2.zero;
            
            // Add VerticalLayoutGroup for automatic layout
            VerticalLayoutGroup layoutGroup = contentObj.AddComponent<VerticalLayoutGroup>();
            layoutGroup.childAlignment = TextAnchor.UpperLeft;
            layoutGroup.childControlHeight = false;
            layoutGroup.childControlWidth = true;
            layoutGroup.spacing = 5f;
            
            // Add ContentSizeFitter to adjust content size
            ContentSizeFitter sizeFitter = contentObj.AddComponent<ContentSizeFitter>();
            sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            
            scrollRect.content = contentRect;
            checkoutUI.productListParent = contentObj.transform;
            
            // Create simple product item prefab
            GameObject prefab = new GameObject("ProductItemPrefab");
            Text prefabText = prefab.AddComponent<Text>();
            prefabText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            prefabText.fontSize = 14;
            prefabText.color = Color.white;
            
            RectTransform prefabRect = prefab.GetComponent<RectTransform>();
            prefabRect.sizeDelta = new Vector2(0, 25);
            
            checkoutUI.productItemPrefab = prefab;
            
            Debug.Log("CheckoutUI: Created basic checkout UI");
            return checkoutUI;
        }
        
        #endregion
    }
}
