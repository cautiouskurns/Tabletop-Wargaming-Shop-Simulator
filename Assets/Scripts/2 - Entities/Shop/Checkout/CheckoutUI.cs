using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

namespace TabletopShop
{
    /// <summary>
    /// UI component for displaying checkout information including scanned items, running total, and customer info
    /// Follows existing UI patterns with fade-in/fade-out animations and proper component structure
    /// </summary>
    public class CheckoutUI : MonoBehaviour
    {
        [Header("UI Panel References")]
        [SerializeField] private CanvasGroup checkoutCanvasGroup;
        [SerializeField] private GameObject checkoutPanel;
        
        [Header("Display Elements")]
        [SerializeField] private TextMeshProUGUI totalDisplay;
        [SerializeField] private TextMeshProUGUI customerNameDisplay;
        [SerializeField] private TextMeshProUGUI itemCountDisplay;
        [SerializeField] private ScrollRect itemsScrollRect;
        [SerializeField] private Transform itemsContainer;
        
        [Header("Item List UI")]
        [SerializeField] private GameObject itemDisplayPrefab;
        [SerializeField] private Color scannedItemColor = Color.green;
        [SerializeField] private Color unscannedItemColor = Color.gray;
        
        [Header("Status Elements")]
        [SerializeField] private TextMeshProUGUI statusText;
        [SerializeField] private Button paymentButton;
        [SerializeField] private GameObject paymentProgressIndicator;
        
        [Header("Animation Settings")]
        [SerializeField] private float fadeInDuration = 0.3f;
        [SerializeField] private float fadeOutDuration = 0.2f;
        [SerializeField] private AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        
        // Private fields
        private bool isVisible = false;
        private Coroutine fadeCoroutine;
        private List<GameObject> itemDisplayObjects = new List<GameObject>();
        private CheckoutCounter associatedCounter;
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            InitializeUI();
        }
        
        private void Start()
        {
            ValidateReferences();
            SetupInitialState();
        }
        
        #endregion
        
        #region Initialization
        
        /// <summary>
        /// Initialize the checkout UI component
        /// </summary>
        private void InitializeUI()
        {
            // Find associated checkout counter
            associatedCounter = GetComponentInParent<CheckoutCounter>();
            
            // Ensure canvas group exists
            if (checkoutCanvasGroup == null && checkoutPanel != null)
            {
                checkoutCanvasGroup = checkoutPanel.GetComponent<CanvasGroup>();
                if (checkoutCanvasGroup == null)
                {
                    checkoutCanvasGroup = checkoutPanel.AddComponent<CanvasGroup>();
                }
            }
            
            // Setup payment button
            if (paymentButton != null)
            {
                paymentButton.onClick.AddListener(OnPaymentButtonClicked);
            }
        }
        
        /// <summary>
        /// Validate that all required references are assigned
        /// </summary>
        private void ValidateReferences()
        {
            if (checkoutCanvasGroup == null)
            {
                Debug.LogWarning($"CheckoutUI on {gameObject.name}: No CanvasGroup found for fade animations");
            }
            
            if (totalDisplay == null)
            {
                Debug.LogWarning($"CheckoutUI on {gameObject.name}: Total display not assigned");
            }
            
            if (itemsContainer == null)
            {
                Debug.LogWarning($"CheckoutUI on {gameObject.name}: Items container not assigned");
            }
            
            if (itemDisplayPrefab == null)
            {
                Debug.LogWarning($"CheckoutUI on {gameObject.name}: Item display prefab not assigned");
            }
        }
        
        /// <summary>
        /// Setup initial UI state
        /// </summary>
        private void SetupInitialState()
        {
            // Start hidden
            if (checkoutCanvasGroup != null)
            {
                checkoutCanvasGroup.alpha = 0f;
                checkoutCanvasGroup.interactable = false;
                checkoutCanvasGroup.blocksRaycasts = false;
            }
            
            if (checkoutPanel != null)
            {
                checkoutPanel.SetActive(false);
            }
            
            // Hide payment progress indicator
            if (paymentProgressIndicator != null)
            {
                paymentProgressIndicator.SetActive(false);
            }
            
            // Initialize displays
            UpdateTotalDisplay(0f);
            UpdateStatusText("Waiting for customer...");
            UpdateItemCount(0, 0);
        }
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// Show the checkout UI with fade-in animation
        /// </summary>
        public void ShowUI()
        {
            if (isVisible) return;
            
            isVisible = true;
            
            if (checkoutPanel != null)
            {
                checkoutPanel.SetActive(true);
            }
            
            if (fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
            }
            
            fadeCoroutine = StartCoroutine(FadeIn());
        }
        
        /// <summary>
        /// Hide the checkout UI with fade-out animation
        /// </summary>
        public void HideUI()
        {
            if (!isVisible) return;
            
            if (fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
            }
            
            fadeCoroutine = StartCoroutine(FadeOut());
        }
        
        /// <summary>
        /// Update the customer information display
        /// </summary>
        /// <param name="customer">Customer information to display</param>
        public void UpdateCustomerInfo(Customer customer)
        {
            if (customerNameDisplay != null)
            {
                string customerName = customer != null ? customer.name : "No Customer";
                customerNameDisplay.text = $"Customer: {customerName}";
            }
        }
        
        /// <summary>
        /// Update the running total display
        /// </summary>
        /// <param name="total">Total amount to display</param>
        public void UpdateTotalDisplay(float total)
        {
            if (totalDisplay != null)
            {
                totalDisplay.text = $"Total: ${total:F2}";
            }
        }
        
        /// <summary>
        /// Update the item count display
        /// </summary>
        /// <param name="scannedItems">Number of scanned items</param>
        /// <param name="totalItems">Total number of items</param>
        public void UpdateItemCount(int scannedItems, int totalItems)
        {
            if (itemCountDisplay != null)
            {
                itemCountDisplay.text = $"Items: {scannedItems}/{totalItems}";
            }
        }
        
        /// <summary>
        /// Update the status text
        /// </summary>
        /// <param name="status">Status message to display</param>
        public void UpdateStatusText(string status)
        {
            if (statusText != null)
            {
                statusText.text = status;
            }
        }
        
        /// <summary>
        /// Refresh the items list display
        /// </summary>
        /// <param name="checkoutItems">List of checkout items to display</param>
        public void UpdateItemsList(List<CheckoutItem> checkoutItems)
        {
            // Clear existing item displays
            ClearItemDisplays();
            
            if (itemsContainer == null || itemDisplayPrefab == null)
            {
                return;
            }
            
            // Create new item displays
            foreach (var item in checkoutItems)
            {
                if (item != null && item.IsValid())
                {
                    CreateItemDisplay(item);
                }
            }
            
            // Update scroll position to bottom if needed
            if (itemsScrollRect != null)
            {
                Canvas.ForceUpdateCanvases();
                itemsScrollRect.verticalNormalizedPosition = 0f;
            }
        }
        
        /// <summary>
        /// Show payment processing state
        /// </summary>
        public void ShowPaymentProcessing()
        {
            if (paymentProgressIndicator != null)
            {
                paymentProgressIndicator.SetActive(true);
            }
            
            if (paymentButton != null)
            {
                paymentButton.interactable = false;
            }
            
            UpdateStatusText("Processing payment...");
        }
        
        /// <summary>
        /// Hide payment processing state
        /// </summary>
        public void HidePaymentProcessing()
        {
            if (paymentProgressIndicator != null)
            {
                paymentProgressIndicator.SetActive(false);
            }
            
            if (paymentButton != null)
            {
                paymentButton.interactable = true;
            }
        }
        
        /// <summary>
        /// Update payment button state based on checkout readiness
        /// </summary>
        /// <param name="canPay">Whether payment can be processed</param>
        public void UpdatePaymentButton(bool canPay)
        {
            if (paymentButton != null)
            {
                paymentButton.interactable = canPay;
            }
        }
        
        /// <summary>
        /// Clear all checkout displays
        /// </summary>
        public void ClearCheckoutDisplay()
        {
            ClearItemDisplays();
            UpdateTotalDisplay(0f);
            UpdateCustomerInfo(null);
            UpdateItemCount(0, 0);
            UpdateStatusText("Waiting for customer...");
            HidePaymentProcessing();
        }
        
        #endregion
        
        #region Private Methods
        
        /// <summary>
        /// Create a display object for a checkout item
        /// </summary>
        /// <param name="item">Checkout item to create display for</param>
        private void CreateItemDisplay(CheckoutItem item)
        {
            GameObject itemDisplay = Instantiate(itemDisplayPrefab, itemsContainer);
            itemDisplayObjects.Add(itemDisplay);
            
            // Setup item display components
            var nameText = itemDisplay.GetComponentInChildren<TextMeshProUGUI>();
            if (nameText != null)
            {
                nameText.text = $"{item.ProductName} - ${item.Price:F2}";
                nameText.color = item.IsScanned ? scannedItemColor : unscannedItemColor;
            }
            
            // Add scan indicator if available
            var scanIndicator = itemDisplay.transform.Find("ScanIndicator");
            if (scanIndicator != null)
            {
                scanIndicator.gameObject.SetActive(item.IsScanned);
            }
        }
        
        /// <summary>
        /// Clear all item display objects
        /// </summary>
        private void ClearItemDisplays()
        {
            foreach (var itemDisplay in itemDisplayObjects)
            {
                if (itemDisplay != null)
                {
                    DestroyImmediate(itemDisplay);
                }
            }
            itemDisplayObjects.Clear();
        }
        
        /// <summary>
        /// Handle payment button click
        /// </summary>
        private void OnPaymentButtonClicked()
        {
            if (associatedCounter != null)
            {
                associatedCounter.ProcessPayment();
            }
        }
        
        #endregion
        
        #region Animation Coroutines
        
        /// <summary>
        /// Fade in animation coroutine
        /// </summary>
        private IEnumerator FadeIn()
        {
            if (checkoutCanvasGroup == null) yield break;
            
            float elapsedTime = 0f;
            float startAlpha = checkoutCanvasGroup.alpha;
            
            checkoutCanvasGroup.interactable = true;
            checkoutCanvasGroup.blocksRaycasts = true;
            
            while (elapsedTime < fadeInDuration)
            {
                elapsedTime += Time.deltaTime;
                float progress = elapsedTime / fadeInDuration;
                float curveValue = fadeCurve.Evaluate(progress);
                
                checkoutCanvasGroup.alpha = Mathf.Lerp(startAlpha, 1f, curveValue);
                yield return null;
            }
            
            checkoutCanvasGroup.alpha = 1f;
            fadeCoroutine = null;
        }
        
        /// <summary>
        /// Fade out animation coroutine
        /// </summary>
        private IEnumerator FadeOut()
        {
            if (checkoutCanvasGroup == null) yield break;
            
            float elapsedTime = 0f;
            float startAlpha = checkoutCanvasGroup.alpha;
            
            checkoutCanvasGroup.interactable = false;
            
            while (elapsedTime < fadeOutDuration)
            {
                elapsedTime += Time.deltaTime;
                float progress = elapsedTime / fadeOutDuration;
                float curveValue = fadeCurve.Evaluate(progress);
                
                checkoutCanvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, curveValue);
                yield return null;
            }
            
            checkoutCanvasGroup.alpha = 0f;
            checkoutCanvasGroup.blocksRaycasts = false;
            
            if (checkoutPanel != null)
            {
                checkoutPanel.SetActive(false);
            }
            
            isVisible = false;
            fadeCoroutine = null;
        }
        
        #endregion
        
        #region Debug
        
        private void OnValidate()
        {
            // Ensure fade durations are positive
            fadeInDuration = Mathf.Max(0.1f, fadeInDuration);
            fadeOutDuration = Mathf.Max(0.1f, fadeOutDuration);
        }
        
        #endregion
    }
}
