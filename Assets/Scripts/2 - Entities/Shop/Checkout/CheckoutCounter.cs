using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace TabletopShop
{
    /// <summary>
    /// CheckoutCounter component for managing customer purchases and item scanning
    /// Handles the checkout process from item placement through payment completion
    /// </summary>
    public class CheckoutCounter : MonoBehaviour, IInteractable
    {
        [Header("Checkout Configuration")]
        [SerializeField] private List<CheckoutItem> placedItems = new List<CheckoutItem>();
        [SerializeField] private CheckoutUI checkoutUI;
        [SerializeField] private CheckoutItemPlacement itemPlacement;
        [SerializeField] private Customer currentCustomer;
        [SerializeField] private float runningTotal = 0f;
        
        [Header("Interaction Settings")]
        [SerializeField] private Transform itemPlacementArea;
        [SerializeField] private float maxCheckoutDistance = 2f;
        
        [Header("Debug")]
        [SerializeField] private bool enableDebugLogging = true;
        
        // IInteractable Properties
        public string InteractionText => GetInteractionText();
        public bool CanInteract => CanInteractWithCounter();
        
        // Properties
        public List<CheckoutItem> PlacedItems => new List<CheckoutItem>(placedItems);
        public Customer CurrentCustomer => currentCustomer;
        public float RunningTotal => runningTotal;
        public bool HasCustomer => currentCustomer != null;
        public bool HasItems => placedItems.Count > 0;
        public bool AreAllItemsScanned => placedItems.All(item => item != null && item.IsScanned);
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            InitializeCheckoutCounter();
        }
        
        private void Start()
        {
            ValidateReferences();
        }
        
        #endregion
        
        #region Initialization
        
        /// <summary>
        /// Initialize the checkout counter
        /// </summary>
        private void InitializeCheckoutCounter()
        {
            // Set interaction layer
            InteractionLayers.SetShelfLayer(gameObject);
            
            // Initialize collections
            if (placedItems == null)
            {
                placedItems = new List<CheckoutItem>();
            }
            
            // Initialize placement system
            if (itemPlacement == null)
            {
                itemPlacement = GetComponentInChildren<CheckoutItemPlacement>();
                if (itemPlacement == null)
                {
                    if (enableDebugLogging)
                    {
                        Debug.LogWarning($"CheckoutCounter {name}: No CheckoutItemPlacement found in children");
                    }
                }
            }
            
            // Initialize UI
            if (checkoutUI != null)
            {
                checkoutUI.gameObject.SetActive(false);
            }
            
            if (enableDebugLogging)
            {
                Debug.Log($"CheckoutCounter {name}: Initialized successfully");
            }
        }
        
        /// <summary>
        /// Validate required references
        /// </summary>
        private void ValidateReferences()
        {
            if (checkoutUI == null)
            {
                Debug.LogWarning($"CheckoutCounter {name}: CheckoutUI reference is missing!");
            }
            
            if (itemPlacementArea == null)
            {
                Debug.LogWarning($"CheckoutCounter {name}: Item Placement Area is not assigned - using this transform as fallback");
                itemPlacementArea = transform;
            }
        }
        
        #endregion
        
        #region IInteractable Implementation
        
        /// <summary>
        /// Handle player interaction with the checkout counter
        /// </summary>
        /// <param name="player">The player GameObject</param>
        public void Interact(GameObject player)
        {
            if (!CanInteract)
            {
                if (enableDebugLogging)
                {
                    Debug.Log($"CheckoutCounter {name}: Cannot interact - conditions not met");
                }
                return;
            }
            
            // If no customer, this is likely the player trying to manage the counter
            if (currentCustomer == null)
            {
                if (enableDebugLogging)
                {
                    Debug.Log($"CheckoutCounter {name}: Player interacted - opening checkout management");
                }
                
                ToggleCheckoutUI();
            }
            else
            {
                // Customer is present, try to process payment if ready
                if (AreAllItemsScanned && HasItems)
                {
                    ProcessPayment();
                }
                else
                {
                    if (enableDebugLogging)
                    {
                        Debug.Log($"CheckoutCounter {name}: Cannot process payment - not all items scanned or no items present");
                    }
                }
            }
        }
        
        /// <summary>
        /// Called when player starts looking at the counter
        /// </summary>
        public void OnInteractionEnter()
        {
            if (enableDebugLogging)
            {
                Debug.Log($"CheckoutCounter {name}: Player looking at counter");
            }
            
            // Show UI if there are items or a customer
            if (HasItems || HasCustomer)
            {
                ShowCheckoutUI();
            }
        }
        
        /// <summary>
        /// Called when player stops looking at the counter
        /// </summary>
        public void OnInteractionExit()
        {
            if (enableDebugLogging)
            {
                Debug.Log($"CheckoutCounter {name}: Player stopped looking at counter");
            }
            
            // Hide UI if no active customer
            if (!HasCustomer)
            {
                HideCheckoutUI();
            }
        }
        
        #endregion
        
        #region Customer Management
        
        /// <summary>
        /// Handle customer arrival at checkout counter
        /// </summary>
        /// <param name="customer">The customer arriving at checkout</param>
        public void OnCustomerArrival(Customer customer)
        {
            if (customer == null)
            {
                Debug.LogWarning($"CheckoutCounter {name}: Null customer provided to OnCustomerArrival");
                return;
            }
            
            if (currentCustomer != null)
            {
                Debug.LogWarning($"CheckoutCounter {name}: Customer {customer.name} arriving but {currentCustomer.name} is already being served");
                return;
            }
            
            currentCustomer = customer;
            
            if (enableDebugLogging)
            {
                Debug.Log($"CheckoutCounter {name}: Customer {customer.name} arrived for checkout");
            }
            
            // Show checkout UI
            ShowCheckoutUI();
            UpdateCheckoutUI();
            
            // Notify GameManager about customer arrival if needed
            if (GameManager.Instance != null)
            {
                // Track customer service start time or other metrics
            }
        }
        
        /// <summary>
        /// Handle customer departure from checkout counter
        /// </summary>
        public void OnCustomerDeparture()
        {
            if (currentCustomer == null) return;
            
            if (enableDebugLogging)
            {
                Debug.Log($"CheckoutCounter {name}: Customer {currentCustomer.name} departed");
            }
            
            currentCustomer = null;
            
            // Clear checkout if customer left without completing purchase
            if (HasItems)
            {
                ClearCheckout();
            }
            
            HideCheckoutUI();
        }
        
        #endregion
        
        #region Item Management
        
        /// <summary>
        /// Place a product on the checkout counter (legacy method for Product objects)
        /// </summary>
        /// <param name="product">The product to place</param>
        /// <returns>True if the product was successfully placed</returns>
        public bool PlaceItem(Product product)
        {
            if (product == null)
            {
                Debug.LogWarning($"CheckoutCounter {name}: Cannot place null product");
                return false;
            }
            
            if (product.ProductData == null)
            {
                Debug.LogWarning($"CheckoutCounter {name}: Cannot place product with null ProductData");
                return false;
            }
            
            // Create a CheckoutItem MonoBehaviour from the product data
            CheckoutItem checkoutItem = CreateAndPlaceItem(product.ProductData);
            
            if (checkoutItem != null)
            {
                // Position the checkout item at the product's location or checkout area
                if (itemPlacementArea != null)
                {
                    Vector3 placementPosition = CalculateItemPlacementPosition(placedItems.Count - 1);
                    checkoutItem.transform.position = placementPosition;
                }
                else
                {
                    // Use the product's current position
                    checkoutItem.transform.position = product.transform.position;
                }
                
                if (enableDebugLogging)
                {
                    Debug.Log($"CheckoutCounter {name}: Placed {product.ProductData.ProductName} - Running total: ${runningTotal:F2}");
                }
                
                return true;
            }
            
            return false;
        }
        
        /// <summary>
        /// Scan a checkout item
        /// </summary>
        /// <param name="item">The checkout item to scan</param>
        /// <returns>True if the item was successfully scanned</returns>
        public bool ScanItem(CheckoutItem item)
        {
            if (item == null)
            {
                Debug.LogWarning($"CheckoutCounter {name}: Cannot scan null item");
                return false;
            }
            
            if (!placedItems.Contains(item))
            {
                Debug.LogWarning($"CheckoutCounter {name}: Cannot scan item - not found in placed items");
                return false;
            }
            
            if (item.IsScanned)
            {
                if (enableDebugLogging)
                {
                    Debug.Log($"CheckoutCounter {name}: Item {item.ProductName} already scanned");
                }
                return false;
            }
            
            // Call scan method on the item itself
            item.ScanItem();
            
            return item.IsScanned;
        }
        
        /// <summary>
        /// Called by CheckoutItem MonoBehaviour when it gets scanned
        /// </summary>
        /// <param name="item">The checkout item that was scanned</param>
        public void OnItemScanned(CheckoutItem item)
        {
            if (item == null)
            {
                Debug.LogWarning($"CheckoutCounter {name}: OnItemScanned called with null item");
                return;
            }
            
            if (!placedItems.Contains(item))
            {
                Debug.LogWarning($"CheckoutCounter {name}: OnItemScanned called for item not in placed items");
                return;
            }
            
            if (enableDebugLogging)
            {
                Debug.Log($"CheckoutCounter {name}: Item scanned - {item.ProductName} (${item.Price:F2})");
            }
            
            // Update running total
            CalculateRunningTotal();
            
            // Update UI
            UpdateCheckoutUI();
            
            // Check if all items are now scanned
            if (AreAllItemsScanned)
            {
                if (enableDebugLogging)
                {
                    Debug.Log($"CheckoutCounter {name}: All items scanned, ready for payment");
                }
                
                if (checkoutUI != null)
                {
                    checkoutUI.UpdateStatusText("Ready for payment");
                    checkoutUI.UpdatePaymentButton(true);
                }
            }
        }
        
        /// <summary>
        /// Place a CheckoutItem MonoBehaviour on the checkout counter
        /// </summary>
        /// <param name="checkoutItem">The checkout item MonoBehaviour to place</param>
        /// <returns>True if the item was successfully placed</returns>
        public bool PlaceItem(CheckoutItem checkoutItem)
        {
            if (checkoutItem == null)
            {
                Debug.LogWarning($"CheckoutCounter {name}: Cannot place null checkout item");
                return false;
            }
            
            if (!checkoutItem.IsValid())
            {
                Debug.LogWarning($"CheckoutCounter {name}: Cannot place invalid checkout item");
                return false;
            }
            
            if (placedItems.Contains(checkoutItem))
            {
                Debug.LogWarning($"CheckoutCounter {name}: Checkout item already placed");
                return false;
            }
            
            // Add to placed items
            placedItems.Add(checkoutItem);
            
            // Initialize the item with this counter as parent
            checkoutItem.Initialize(checkoutItem.ProductData, this);
            
            // Update running total
            CalculateRunningTotal();
            
            // Position the item at checkout area
            if (itemPlacementArea != null)
            {
                Vector3 placementPosition = CalculateItemPlacementPosition(placedItems.Count - 1);
                checkoutItem.transform.position = placementPosition;
            }
            
            if (enableDebugLogging)
            {
                Debug.Log($"CheckoutCounter {name}: Placed {checkoutItem.ProductName} - Running total: ${runningTotal:F2}");
            }
            
            UpdateCheckoutUI();
            return true;
        }
        
        /// <summary>
        /// Create and place a product as a CheckoutItem MonoBehaviour
        /// </summary>
        /// <param name="productData">The product data to create item from</param>
        /// <param name="itemPrefab">Prefab for the checkout item</param>
        /// <returns>The created CheckoutItem MonoBehaviour, or null if failed</returns>
        public CheckoutItem CreateAndPlaceItem(ProductData productData, GameObject itemPrefab = null)
        {
            if (productData == null)
            {
                Debug.LogWarning($"CheckoutCounter {name}: Cannot create item from null product data");
                return null;
            }
            
            CheckoutItem checkoutItem = null;
            
            // Try to use placement system if available
            if (itemPlacement != null)
            {
                checkoutItem = itemPlacement.CreateCheckoutItem(productData);
                
                if (checkoutItem != null)
                {
                    // Add to placed items list
                    placedItems.Add(checkoutItem);
                    
                    if (enableDebugLogging)
                    {
                        Debug.Log($"CheckoutCounter {name}: Created checkout item using placement system - {productData.ProductName}");
                    }
                    
                    return checkoutItem;
                }
            }
            
            // Fallback to manual creation if placement system is not available
            return CreateCheckoutItemManually(productData, itemPrefab);
        }
        
        /// <summary>
        /// Manually create a checkout item (fallback method)
        /// </summary>
        /// <param name="productData">The product data to create item from</param>
        /// <param name="itemPrefab">Prefab for the checkout item</param>
        /// <returns>The created CheckoutItem MonoBehaviour, or null if failed</returns>
        private CheckoutItem CreateCheckoutItemManually(ProductData productData, GameObject itemPrefab = null)
        {
            // Create the checkout item GameObject
            GameObject itemObject;
            if (itemPrefab != null)
            {
                itemObject = Instantiate(itemPrefab, transform);
            }
            else
            {
                // Create a basic GameObject with CheckoutItem component
                itemObject = new GameObject($"CheckoutItem_{productData.ProductName}");
                itemObject.transform.SetParent(transform);
                itemObject.AddComponent<CheckoutItem>();
            }
            
            // Get the CheckoutItem component
            CheckoutItem checkoutItem = itemObject.GetComponent<CheckoutItem>();
            if (checkoutItem == null)
            {
                Debug.LogError($"CheckoutCounter {name}: Created item object has no CheckoutItem component");
                Destroy(itemObject);
                return null;
            }
            
            // Initialize with product data
            checkoutItem.Initialize(productData, this);
            
            // Place the item using manual positioning
            if (PlaceItem(checkoutItem))
            {
                return checkoutItem;
            }
            else
            {
                Destroy(itemObject);
                return null;
            }
        }

        #endregion
        
        #region Payment Processing
        
        /// <summary>
        /// Process payment for all scanned items
        /// </summary>
        /// <returns>True if payment was successfully processed</returns>
        public bool ProcessPayment()
        {
            if (!CanProcessPayment())
            {
                if (enableDebugLogging)
                {
                    Debug.Log($"CheckoutCounter {name}: Cannot process payment - conditions not met");
                }
                return false;
            }
            
            // Calculate final total
            float finalTotal = CalculateRunningTotal();
            
            // Process through GameManager
            if (GameManager.Instance != null)
            {
                float customerSatisfaction = CalculateCustomerSatisfaction();
                GameManager.Instance.ProcessCustomerPurchase(finalTotal, customerSatisfaction);
                
                if (enableDebugLogging)
                {
                    Debug.Log($"CheckoutCounter {name}: Processed payment of ${finalTotal:F2} with satisfaction {customerSatisfaction:F2}");
                }
            }
            else
            {
                Debug.LogWarning($"CheckoutCounter {name}: GameManager not available for payment processing");
            }
            
            // Mark products as purchased
            // TODO: Implement product purchase logic for MonoBehaviour CheckoutItems
            foreach (var item in placedItems)
            {
                // Product purchase logic would go here
                // This depends on how products are managed in the inventory system
                if (enableDebugLogging)
                {
                    Debug.Log($"CheckoutCounter {name}: Processing purchase for {item.ProductName}");
                }
            }
            
            // Notify customer that checkout is completed
            if (currentCustomer != null)
            {
                var customerBehavior = currentCustomer.GetComponent<CustomerBehavior>();
                if (customerBehavior != null)
                {
                    customerBehavior.OnCheckoutCompleted();
                    
                    if (enableDebugLogging)
                    {
                        Debug.Log($"CheckoutCounter {name}: Notified customer {currentCustomer.name} of checkout completion");
                    }
                }
            }
            
            // Clear checkout
            ClearCheckout();
            
            // Customer departure
            OnCustomerDeparture();
            
            return true;
        }
        
        /// <summary>
        /// Calculate customer satisfaction based on checkout experience
        /// </summary>
        /// <returns>Customer satisfaction value between 0 and 1</returns>
        private float CalculateCustomerSatisfaction()
        {
            float baseSatisfaction = 0.8f;
            
            // All items scanned properly
            if (AreAllItemsScanned)
            {
                baseSatisfaction += 0.1f;
            }
            
            // Quick service (placeholder for timing metrics)
            // Could add timing logic here in the future
            
            return Mathf.Clamp01(baseSatisfaction);
        }
        
        /// <summary>
        /// Check if payment can be processed
        /// </summary>
        /// <returns>True if payment can be processed</returns>
        private bool CanProcessPayment()
        {
            return HasCustomer && HasItems && AreAllItemsScanned;
        }
        
        #endregion
        
        #region Checkout Management
        
        /// <summary>
        /// Clear all items from checkout counter
        /// </summary>
        public void ClearCheckout()
        {
            if (enableDebugLogging)
            {
                Debug.Log($"CheckoutCounter {name}: Clearing checkout - {placedItems.Count} items");
            }
            
            // Use placement system to clear items if available
            if (itemPlacement != null)
            {
                itemPlacement.ClearAllItems();
            }
            else
            {
                // Fallback: manually clear items
                foreach (var item in placedItems)
                {
                    if (item != null)
                    {
                        if (!item.IsScanned && enableDebugLogging)
                        {
                            Debug.Log($"CheckoutCounter {name}: Returning unscanned item {item.ProductName} to shelf");
                        }
                        Destroy(item.gameObject);
                    }
                }
            }
            
            // Clear collections
            placedItems.Clear();
            runningTotal = 0f;
            
            UpdateCheckoutUI();
        }
        
        /// <summary>
        /// Calculate the running total of all placed items
        /// </summary>
        /// <returns>The current running total</returns>
        private float CalculateRunningTotal()
        {
            runningTotal = placedItems.Sum(item => item?.Price ?? 0f);
            return runningTotal;
        }
        
        /// <summary>
        /// Calculate placement position for an item
        /// </summary>
        /// <param name="itemIndex">Index of the item in the placed items list</param>
        /// <returns>World position for item placement</returns>
        private Vector3 CalculateItemPlacementPosition(int itemIndex)
        {
            if (itemPlacementArea == null) return transform.position;
            
            // Arrange items in a grid pattern
            float spacing = 0.5f;
            int itemsPerRow = 3;
            
            int row = itemIndex / itemsPerRow;
            int col = itemIndex % itemsPerRow;
            
            Vector3 offset = new Vector3(
                (col - 1) * spacing,
                0.1f,
                row * spacing
            );
            
            return itemPlacementArea.position + offset;
        }
        
        #endregion
        
        #region UI Management
        
        /// <summary>
        /// Show the checkout UI
        /// </summary>
        private void ShowCheckoutUI()
        {
            if (checkoutUI != null)
            {
                checkoutUI.gameObject.SetActive(true);
                // CheckoutUI handles its own initialization
                checkoutUI.ShowUI();
            }
        }
        
        /// <summary>
        /// Hide the checkout UI
        /// </summary>
        private void HideCheckoutUI()
        {
            if (checkoutUI != null)
            {
                checkoutUI.gameObject.SetActive(false);
            }
        }
        
        /// <summary>
        /// Toggle checkout UI visibility
        /// </summary>
        private void ToggleCheckoutUI()
        {
            if (checkoutUI != null)
            {
                bool isActive = checkoutUI.gameObject.activeInHierarchy;
                
                if (isActive)
                {
                    checkoutUI.HideUI();
                }
                else
                {
                    checkoutUI.ShowUI();
                }
            }
        }
        
        /// <summary>
        /// Update the checkout UI with current state
        /// </summary>
        private void UpdateCheckoutUI()
        {
            if (checkoutUI != null && checkoutUI.gameObject.activeInHierarchy)
            {
                // Update running total
                checkoutUI.UpdateTotalDisplay(runningTotal);
                
                // Update customer info
                checkoutUI.UpdateCustomerInfo(currentCustomer);
                
                // Update items list
                checkoutUI.UpdateItemsList(placedItems);
                
                // Update item count
                int scannedCount = placedItems.Count(item => item.IsScanned);
                checkoutUI.UpdateItemCount(scannedCount, placedItems.Count);
                
                // Update payment button state
                checkoutUI.UpdatePaymentButton(AreAllItemsScanned && HasItems);
            }
        }
        
        #endregion
        
        #region Helper Methods
        
        /// <summary>
        /// Get interaction text based on current state
        /// </summary>
        /// <returns>Appropriate interaction text</returns>
        private string GetInteractionText()
        {
            if (!HasCustomer && !HasItems)
            {
                return "Empty Checkout Counter";
            }
            
            if (HasCustomer && !HasItems)
            {
                return $"Serve {currentCustomer.name} - No Items";
            }
            
            if (!HasCustomer && HasItems)
            {
                return $"Manage Checkout - {placedItems.Count} items";
            }
            
            if (CanProcessPayment())
            {
                return $"Process Payment - ${runningTotal:F2}";
            }
            
            int unscannedItems = placedItems.Count(item => !item.IsScanned);
            return $"Serve {currentCustomer.name} - {unscannedItems} items to scan";
        }
        
        /// <summary>
        /// Check if player can interact with counter
        /// </summary>
        /// <returns>True if interaction is possible</returns>
        private bool CanInteractWithCounter()
        {
            // Always allow interaction for management purposes
            return true;
        }
        
        /// <summary>
        /// Get list of placed items (for external access)
        /// </summary>
        /// <returns>Copy of placed items list</returns>
        public List<CheckoutItem> GetPlacedItems()
        {
            return new List<CheckoutItem>(placedItems);
        }
        
        /// <summary>
        /// Get current customer (for external access)
        /// </summary>
        /// <returns>Current customer or null</returns>
        public Customer GetCurrentCustomer()
        {
            return currentCustomer;
        }
        
        /// <summary>
        /// Get running total (for external access)
        /// </summary>
        /// <returns>Current running total</returns>
        public float GetRunningTotal()
        {
            return runningTotal;
        }
        
        #endregion
        
        #region Debug and Validation
        
        /// <summary>
        /// Validate checkout counter state
        /// </summary>
        private void OnValidate()
        {
            if (Application.isPlaying) return;
            
            // Ensure running total matches placed items
            if (placedItems != null && placedItems.Count > 0)
            {
                float calculatedTotal = placedItems.Sum(item => item?.Price ?? 0f);
                if (Mathf.Abs(runningTotal - calculatedTotal) > 0.01f)
                {
                    runningTotal = calculatedTotal;
                }
            }
        }
        
        #endregion
    }
}
