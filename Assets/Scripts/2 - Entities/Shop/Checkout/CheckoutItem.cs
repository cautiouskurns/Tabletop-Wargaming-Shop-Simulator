using UnityEngine;

namespace TabletopShop
{
    /// <summary>
    /// MonoBehaviour representing an individual item at checkout that can be interacted with
    /// Handles scan state, product data, and visual feedback for checkout items
    /// </summary>
    public class CheckoutItem : MonoBehaviour, IInteractable
    {
        [Header("Product Information")]
        [SerializeField] private ProductData productData;
        [SerializeField] private bool isScanned;
        [SerializeField] private CheckoutCounter parentCounter;
        
        [Header("Visual Feedback")]
        [SerializeField] private ProductVisuals visualFeedback;
        [SerializeField] private GameObject scannedIndicator;
        [SerializeField] private Material scannedMaterial;
        [SerializeField] private Material unscannedMaterial;
        
        [Header("Interaction Settings")]
        [SerializeField] private string scanInteractionText = "Scan Item";
        [SerializeField] private string alreadyScannedText = "Already Scanned";
        
        // IInteractable Properties
        public string InteractionText => isScanned ? alreadyScannedText : scanInteractionText;
        public bool CanInteract => !isScanned && parentCounter != null;
        
        // Properties
        public ProductData ProductData => productData;
        public bool IsScanned => isScanned;
        public CheckoutCounter ParentCounter => parentCounter;
        public float Price => productData?.BasePrice ?? 0f;
        public string ProductName => productData?.ProductName ?? "Unknown Product";
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            InitializeCheckoutItem();
        }
        
        private void Start()
        {
            UpdateVisualFeedback();
        }
        
        #endregion
        
        #region Initialization
        
        /// <summary>
        /// Initialize the checkout item component
        /// </summary>
        private void InitializeCheckoutItem()
        {
            // Set interaction layer if needed
            // InteractionLayers.SetShelfLayer(gameObject);
            
            // Find parent counter if not assigned
            if (parentCounter == null)
            {
                parentCounter = GetComponentInParent<CheckoutCounter>();
            }
            
            // Initialize visual feedback
            if (visualFeedback == null)
            {
                visualFeedback = GetComponent<ProductVisuals>();
            }
        }
        
        #endregion
        
        #region IInteractable Implementation
        
        /// <summary>
        /// Handle interaction with the checkout item (scanning)
        /// </summary>
        /// <param name="player">The player GameObject interacting</param>
        public void Interact(GameObject player)
        {
            if (!CanInteract) return;
            
            ScanItem();
        }
        
        /// <summary>
        /// Called when player starts looking at this item
        /// </summary>
        public void OnInteractionEnter()
        {
            if (visualFeedback != null)
            {
                visualFeedback.ApplyHoverEffect();
            }
        }
        
        /// <summary>
        /// Called when player stops looking at this item
        /// </summary>
        public void OnInteractionExit()
        {
            if (visualFeedback != null)
            {
                visualFeedback.RemoveHoverEffect();
            }
        }
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// Initialize the checkout item with product data
        /// </summary>
        /// <param name="productData">Product data for this item</param>
        /// <param name="checkoutCounter">Parent checkout counter</param>
        public void Initialize(ProductData productData, CheckoutCounter checkoutCounter)
        {
            this.productData = productData;
            this.parentCounter = checkoutCounter;
            this.isScanned = false;
            
            UpdateVisualFeedback();
        }
        
        /// <summary>
        /// Scan this item and notify the parent counter
        /// </summary>
        public void ScanItem()
        {
            if (isScanned) return;
            
            isScanned = true;
            UpdateVisualFeedback();
            
            // Notify parent counter
            if (parentCounter != null)
            {
                parentCounter.OnItemScanned(this);
            }
            
            // Play scan audio
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayProductScanBeep();
            }
        }
        
        /// <summary>
        /// Reset the scan status of this item
        /// </summary>
        public void ResetScanStatus()
        {
            isScanned = false;
            UpdateVisualFeedback();
        }
        
        /// <summary>
        /// Check if this checkout item is valid
        /// </summary>
        /// <returns>True if the item has valid product data</returns>
        public bool IsValid()
        {
            return productData != null;
        }
        
        #endregion
        
        #region Private Methods
        
        /// <summary>
        /// Update visual feedback based on scan status
        /// </summary>
        private void UpdateVisualFeedback()
        {
            // Update scanned indicator
            if (scannedIndicator != null)
            {
                scannedIndicator.SetActive(isScanned);
            }
            
            // Update material if available
            var renderer = GetComponent<Renderer>();
            if (renderer != null)
            {
                if (isScanned && scannedMaterial != null)
                {
                    renderer.material = scannedMaterial;
                }
                else if (!isScanned && unscannedMaterial != null)
                {
                    renderer.material = unscannedMaterial;
                }
            }
        }
        
        #endregion
        
        #region Debug
        
        /// <summary>
        /// Get string representation for debugging
        /// </summary>
        /// <returns>String representation of this checkout item</returns>
        public override string ToString()
        {
            return $"CheckoutItem: {ProductName} - ${Price:F2} - Scanned: {isScanned}";
        }
        
        #endregion
    }
}
