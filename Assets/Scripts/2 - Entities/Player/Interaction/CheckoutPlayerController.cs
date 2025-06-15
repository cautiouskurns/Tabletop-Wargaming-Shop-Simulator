using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace TabletopShop
{
    /// <summary>
    /// Extends PlayerInteraction to handle checkout-specific interactions
    /// Provides special handling for CheckoutItem objects and checkout counter workflows
    /// </summary>
    public class CheckoutPlayerController : PlayerInteraction
    {
        [Header("Checkout Interaction Settings")]
        [SerializeField] private float checkoutDetectionRange = 5f;
        [SerializeField] private LayerMask checkoutItemLayer;
        [SerializeField] private KeyCode finalizePurchaseKey = KeyCode.P; // Additional key for finalizing
        
        [Header("UI References")]
        [SerializeField] private CrosshairUI crosshairUI;
        [SerializeField] private CheckoutUI checkoutUIManager;
        
        [Header("Checkout Context")]
        [SerializeField] private bool inCheckoutMode = false;
        [SerializeField] private CheckoutCounter activeCheckoutCounter;
        [SerializeField] private float checkoutModeRadius = 3f;
        
        [Header("Visual Feedback")]
        [SerializeField] private Color scanPromptColor = Color.cyan;
        [SerializeField] private Color finalizePurchaseColor = Color.green;
        [SerializeField] private string scanItemPrompt = "Press E to Scan";
        [SerializeField] private string alreadyScannedPrompt = "Already Scanned";
        [SerializeField] private string finalizePurchasePrompt = "Press P to Finalize Purchase";
        
        [Header("Audio")]
        [SerializeField] private bool playCheckoutSounds = true;
        
        [Header("Debug")]
        [SerializeField] private bool enableDebugLogging = true;
        [SerializeField] private bool showCheckoutGizmos = true;
        
        // State tracking
        private CheckoutItem currentCheckoutItem;
        private List<CheckoutCounter> nearbyCheckoutCounters = new List<CheckoutCounter>();
        private bool wasInCheckoutMode = false;
        
        // Component references
        private Camera playerCamera;
        
        // Regular interaction fields (inherited from PlayerInteraction functionality)
        [Header("Base Interaction Settings")]
        [SerializeField] private float interactionRange = 3f;
        [SerializeField] private LayerMask interactableLayer;
        
        private IInteractable currentInteractable;
        private Ray interactionRay;
        private RaycastHit hitInfo;
        
        // Properties
        public bool InCheckoutMode => inCheckoutMode;
        public CheckoutCounter ActiveCheckoutCounter => activeCheckoutCounter;
        public bool CanFinalizePurchase => activeCheckoutCounter != null && 
                                          activeCheckoutCounter.AreAllItemsScanned && 
                                          activeCheckoutCounter.HasItems;
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            InitializeCheckoutController();
        }
        
        private void Start()
        {
            ValidateReferences();
        }
        
        private void Update()
        {
            // Perform base interaction raycast
            PerformInteractionRaycast();
            
            // Handle checkout-specific functionality
            UpdateCheckoutMode();
            HandleCheckoutInteractions();
            HandleFinalizePurchaseInput();
            
            // Handle regular interaction input when not in checkout mode
            if (!inCheckoutMode && Input.GetKeyDown(KeyCode.E))
            {
                // Allow normal interactions when not in checkout mode
                var interactable = GetCurrentInteractable();
                if (interactable != null && interactable.CanInteract)
                {
                    interactable.Interact(gameObject);
                }
            }
        }
        
        private void OnDrawGizmos()
        {
            if (!showCheckoutGizmos) return;
            
            // Draw checkout mode radius
            if (inCheckoutMode && activeCheckoutCounter != null)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawWireSphere(activeCheckoutCounter.transform.position, checkoutModeRadius);
            }
            
            // Draw checkout detection range
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, checkoutDetectionRange);
        }
        
        #endregion
        
        #region Initialization
        
        /// <summary>
        /// Initialize the checkout controller
        /// </summary>
        private void InitializeCheckoutController()
        {
            // Set up interaction layer mask
            if (interactableLayer == 0)
            {
                interactableLayer = InteractionLayers.AllInteractablesMask;
            }
            
            // Get player camera
            playerCamera = Camera.main;
            if (playerCamera == null)
            {
                playerCamera = FindAnyObjectByType<Camera>();
            }
            
            // Set up checkout item layer mask if not specified
            if (checkoutItemLayer == 0)
            {
                checkoutItemLayer = interactableLayer; // Use same layer as regular interactions
            }
            
            if (enableDebugLogging)
            {
                Debug.Log($"CheckoutPlayerController {name}: Initialized");
            }
        }
        
        /// <summary>
        /// Validate required references
        /// </summary>
        private void ValidateReferences()
        {
            if (crosshairUI == null)
            {
                crosshairUI = FindAnyObjectByType<CrosshairUI>();
                if (crosshairUI == null)
                {
                    Debug.LogWarning($"CheckoutPlayerController {name}: No CrosshairUI found");
                }
            }
            
            if (playerCamera == null)
            {
                Debug.LogError($"CheckoutPlayerController {name}: No camera found");
            }
        }
        
        #endregion
        
        #region Checkout Mode Management
        
        /// <summary>
        /// Update checkout mode based on proximity to checkout counters
        /// </summary>
        private void UpdateCheckoutMode()
        {
            nearbyCheckoutCounters.Clear();
            
            // Find all checkout counters within range
            CheckoutCounter[] allCounters = FindObjectsByType<CheckoutCounter>(FindObjectsSortMode.None);
            foreach (CheckoutCounter counter in allCounters)
            {
                if (counter != null)
                {
                    float distance = Vector3.Distance(transform.position, counter.transform.position);
                    if (distance <= checkoutDetectionRange)
                    {
                        nearbyCheckoutCounters.Add(counter);
                    }
                }
            }
            
            // Determine if we should be in checkout mode
            bool shouldBeInCheckoutMode = nearbyCheckoutCounters.Count > 0;
            
            if (shouldBeInCheckoutMode && !inCheckoutMode)
            {
                EnterCheckoutMode();
            }
            else if (!shouldBeInCheckoutMode && inCheckoutMode)
            {
                ExitCheckoutMode();
            }
            
            // Update active checkout counter
            if (inCheckoutMode)
            {
                UpdateActiveCheckoutCounter();
            }
        }
        
        /// <summary>
        /// Enter checkout mode
        /// </summary>
        private void EnterCheckoutMode()
        {
            inCheckoutMode = true;
            wasInCheckoutMode = false;
            
            if (enableDebugLogging)
            {
                Debug.Log($"CheckoutPlayerController {name}: Entered checkout mode");
            }
            
            // Enable checkout-specific UI elements
            if (checkoutUIManager != null)
            {
                checkoutUIManager.ShowUI();
            }
        }
        
        /// <summary>
        /// Exit checkout mode
        /// </summary>
        private void ExitCheckoutMode()
        {
            inCheckoutMode = false;
            activeCheckoutCounter = null;
            currentCheckoutItem = null;
            
            if (enableDebugLogging)
            {
                Debug.Log($"CheckoutPlayerController {name}: Exited checkout mode");
            }
            
            // Reset UI to normal interaction mode
            if (crosshairUI != null)
            {
                crosshairUI.HideInteractable();
            }
            
            // Hide checkout-specific UI
            if (checkoutUIManager != null)
            {
                checkoutUIManager.HideUI();
            }
        }
        
        /// <summary>
        /// Update which checkout counter is currently active
        /// </summary>
        private void UpdateActiveCheckoutCounter()
        {
            if (nearbyCheckoutCounters.Count == 0)
            {
                activeCheckoutCounter = null;
                return;
            }
            
            // Find the closest checkout counter with items or a customer
            CheckoutCounter bestCounter = null;
            float closestDistance = float.MaxValue;
            
            foreach (CheckoutCounter counter in nearbyCheckoutCounters)
            {
                float distance = Vector3.Distance(transform.position, counter.transform.position);
                
                // Prioritize counters with customers or items
                bool hasActivity = counter.HasCustomer || counter.HasItems;
                
                if (hasActivity && distance < closestDistance)
                {
                    closestDistance = distance;
                    bestCounter = counter;
                }
                else if (bestCounter == null && distance < closestDistance)
                {
                    closestDistance = distance;
                    bestCounter = counter;
                }
            }
            
            // Update active counter
            if (activeCheckoutCounter != bestCounter)
            {
                activeCheckoutCounter = bestCounter;
                
                if (enableDebugLogging && activeCheckoutCounter != null)
                {
                    Debug.Log($"CheckoutPlayerController {name}: Active checkout counter changed to {activeCheckoutCounter.name}");
                }
            }
        }
        
        #endregion
        
        #region Checkout Interactions
        
        /// <summary>
        /// Handle checkout-specific interactions
        /// </summary>
        private void HandleCheckoutInteractions()
        {
            if (!inCheckoutMode || playerCamera == null) return;
            
            // Perform raycast for checkout items
            Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
            RaycastHit hit;
            
            CheckoutItem newCheckoutItem = null;
            
            if (Physics.Raycast(ray, out hit, checkoutDetectionRange, checkoutItemLayer))
            {
                // Check for CheckoutItem component
                newCheckoutItem = hit.collider.GetComponent<CheckoutItem>();
                if (newCheckoutItem == null)
                {
                    newCheckoutItem = hit.collider.GetComponentInParent<CheckoutItem>();
                }
            }
            
            // Update current checkout item
            if (currentCheckoutItem != newCheckoutItem)
            {
                OnCheckoutItemChanged(currentCheckoutItem, newCheckoutItem);
                currentCheckoutItem = newCheckoutItem;
            }
            
            // Update UI based on current item
            UpdateCheckoutUI();
            
            // Handle interaction input
            if (Input.GetKeyDown(KeyCode.E) && currentCheckoutItem != null)
            {
                TryInteractWithCheckoutItem(currentCheckoutItem);
            }
        }
        
        /// <summary>
        /// Handle checkout item change events
        /// </summary>
        /// <param name="oldItem">Previously targeted item</param>
        /// <param name="newItem">Newly targeted item</param>
        private void OnCheckoutItemChanged(CheckoutItem oldItem, CheckoutItem newItem)
        {
            // Handle old item
            if (oldItem != null)
            {
                oldItem.OnInteractionExit();
            }
            
            // Handle new item
            if (newItem != null)
            {
                newItem.OnInteractionEnter();
            }
        }
        
        /// <summary>
        /// Try to interact with a checkout item (scanning)
        /// </summary>
        /// <param name="item">The checkout item to interact with</param>
        private void TryInteractWithCheckoutItem(CheckoutItem item)
        {
            if (item == null || !item.CanInteract) return;
            
            if (enableDebugLogging)
            {
                Debug.Log($"CheckoutPlayerController {name}: Attempting to scan {item.ProductName}");
            }
            
            // Interact with the item (this will trigger scanning)
            item.Interact(gameObject);
            
            // Play scan sound if enabled
            if (playCheckoutSounds && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayProductScanBeep();
            }
            
            // Update checkout UI immediately
            UpdateCheckoutUIAfterScan();
        }
        
        /// <summary>
        /// Update checkout UI displays
        /// </summary>
        private void UpdateCheckoutUI()
        {
            if (crosshairUI == null) return;
            
            if (currentCheckoutItem != null)
            {
                // Show checkout-specific prompts
                if (currentCheckoutItem.IsScanned)
                {
                    crosshairUI.ShowInteractable(alreadyScannedPrompt);
                }
                else
                {
                    crosshairUI.ShowInteractable(scanItemPrompt);
                }
            }
            else if (CanFinalizePurchase)
            {
                // Show finalize purchase prompt
                crosshairUI.ShowInteractable(finalizePurchasePrompt);
            }
            else
            {
                // No specific checkout interaction available
                crosshairUI.HideInteractable();
            }
        }
        
        /// <summary>
        /// Update UI after scanning an item
        /// </summary>
        private void UpdateCheckoutUIAfterScan()
        {
            if (checkoutUIManager != null && activeCheckoutCounter != null)
            {
                // Update the checkout UI with current state
                checkoutUIManager.UpdateTotalDisplay(activeCheckoutCounter.RunningTotal);
                checkoutUIManager.UpdateItemsList(activeCheckoutCounter.PlacedItems);
                
                int scannedCount = activeCheckoutCounter.PlacedItems.Count(item => item.IsScanned);
                checkoutUIManager.UpdateItemCount(scannedCount, activeCheckoutCounter.PlacedItems.Count);
                
                checkoutUIManager.UpdatePaymentButton(CanFinalizePurchase);
            }
        }
        
        #endregion
        
        #region Purchase Finalization
        
        /// <summary>
        /// Handle input for finalizing purchase
        /// </summary>
        private void HandleFinalizePurchaseInput()
        {
            if (!inCheckoutMode || !CanFinalizePurchase) return;
            
            if (Input.GetKeyDown(finalizePurchaseKey))
            {
                ProcessAllScannedItems();
            }
        }
        
        /// <summary>
        /// Process all scanned items and finalize checkout
        /// </summary>
        public void ProcessAllScannedItems()
        {
            if (activeCheckoutCounter == null)
            {
                Debug.LogWarning($"CheckoutPlayerController {name}: Cannot process items - no active checkout counter");
                return;
            }
            
            if (!CanFinalizePurchase)
            {
                Debug.LogWarning($"CheckoutPlayerController {name}: Cannot finalize purchase - conditions not met");
                return;
            }
            
            if (enableDebugLogging)
            {
                Debug.Log($"CheckoutPlayerController {name}: Processing all scanned items");
            }
            
            // Process payment through the checkout counter
            bool success = activeCheckoutCounter.ProcessPayment();
            
            if (success)
            {
                if (enableDebugLogging)
                {
                    Debug.Log($"CheckoutPlayerController {name}: Purchase processed successfully");
                }
                
                // Play success sound
                if (playCheckoutSounds && AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlayPurchaseSuccess();
                }
                
                // Update UI
                if (checkoutUIManager != null)
                {
                    checkoutUIManager.ClearCheckoutDisplay();
                }
            }
            else
            {
                Debug.LogWarning($"CheckoutPlayerController {name}: Failed to process purchase");
            }
        }
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// Enable or disable checkout mode manually
        /// </summary>
        /// <param name="enabled">Whether checkout mode should be enabled</param>
        public void SetCheckoutMode(bool enabled)
        {
            if (enabled && !inCheckoutMode)
            {
                EnterCheckoutMode();
            }
            else if (!enabled && inCheckoutMode)
            {
                ExitCheckoutMode();
            }
        }
        
        /// <summary>
        /// Set the active checkout counter manually
        /// </summary>
        /// <param name="counter">The checkout counter to make active</param>
        public void SetActiveCheckoutCounter(CheckoutCounter counter)
        {
            activeCheckoutCounter = counter;
            
            if (counter != null && !inCheckoutMode)
            {
                EnterCheckoutMode();
            }
        }
        
        /// <summary>
        /// Get the current checkout item being looked at
        /// </summary>
        /// <returns>Current CheckoutItem or null</returns>
        public CheckoutItem GetCurrentCheckoutItem()
        {
            return currentCheckoutItem;
        }
        
        /// <summary>
        /// Check if currently looking at a checkout item
        /// </summary>
        /// <returns>True if looking at a checkout item</returns>
        public bool IsLookingAtCheckoutItem()
        {
            return currentCheckoutItem != null;
        }
        
        /// <summary>
        /// Get the interaction text for current context
        /// </summary>
        /// <returns>Appropriate interaction text</returns>
        public string GetCurrentInteractionText()
        {
            if (currentCheckoutItem != null)
            {
                return currentCheckoutItem.IsScanned ? alreadyScannedPrompt : scanItemPrompt;
            }
            else if (CanFinalizePurchase)
            {
                return finalizePurchasePrompt;
            }
            
            return "";
        }
        
        #endregion
        
        #region Debug Methods
        
        /// <summary>
        /// Debug method to log current checkout state
        /// </summary>
        [ContextMenu("Log Checkout State")]
        private void LogCheckoutState()
        {
            Debug.Log($"CheckoutPlayerController {name} State:");
            Debug.Log($"  In Checkout Mode: {inCheckoutMode}");
            Debug.Log($"  Active Counter: {(activeCheckoutCounter != null ? activeCheckoutCounter.name : "None")}");
            Debug.Log($"  Current Item: {(currentCheckoutItem != null ? currentCheckoutItem.ProductName : "None")}");
            Debug.Log($"  Can Finalize: {CanFinalizePurchase}");
            Debug.Log($"  Nearby Counters: {nearbyCheckoutCounters.Count}");
        }
        
        /// <summary>
        /// Test method to simulate scanning current item
        /// </summary>
        [ContextMenu("Test Scan Current Item")]
        private void TestScanCurrentItem()
        {
            if (Application.isPlaying && currentCheckoutItem != null)
            {
                TryInteractWithCheckoutItem(currentCheckoutItem);
            }
        }
        
        #endregion
        
        #region Base Interaction Functionality (from PlayerInteraction)
        
        /// <summary>
        /// Perform raycast to detect interactable objects (base functionality)
        /// </summary>
        private void PerformInteractionRaycast()
        {
            if (playerCamera == null) return;
            
            // Create ray from camera center
            interactionRay = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
            
            // Perform raycast
            bool hitSomething = Physics.Raycast(interactionRay, out hitInfo, interactionRange, interactableLayer);
            
            IInteractable newInteractable = null;
            
            if (hitSomething)
            {
                // Check if the hit object has an IInteractable component
                newInteractable = hitInfo.collider.GetComponent<IInteractable>();
                
                // If not on the main object, check parent objects
                if (newInteractable == null)
                {
                    newInteractable = hitInfo.collider.GetComponentInParent<IInteractable>();
                }
            }
            
            // Handle interactable changes
            if (newInteractable != currentInteractable)
            {
                // Exit previous interactable
                if (currentInteractable != null)
                {
                    currentInteractable.OnInteractionExit();
                    if (crosshairUI != null && !inCheckoutMode)
                    {
                        crosshairUI.HideInteractable();
                    }
                }
                
                // Enter new interactable
                currentInteractable = newInteractable;
                if (currentInteractable != null && currentInteractable.CanInteract)
                {
                    currentInteractable.OnInteractionEnter();
                    if (crosshairUI != null && !inCheckoutMode)
                    {
                        string interactionText = $"[E] {currentInteractable.InteractionText}";
                        crosshairUI.ShowInteractable(interactionText);
                    }
                }
            }
        }
        
        /// <summary>
        /// Get the current interactable object
        /// </summary>
        /// <returns>The current interactable, or null if none</returns>
        public new IInteractable GetCurrentInteractable()
        {
            return currentInteractable;
        }
        
        #endregion
    }
}
