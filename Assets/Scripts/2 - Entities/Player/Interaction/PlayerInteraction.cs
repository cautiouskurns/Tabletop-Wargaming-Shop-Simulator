using UnityEngine;

namespace TabletopShop
{
    /// <summary>
    /// Handles player interactions with objects in the world using raycasting
    /// </summary>
    public class PlayerInteraction : MonoBehaviour
    {
        [Header("Interaction Settings")]
        [SerializeField] protected Camera playerCamera;
        [SerializeField] protected float interactionRange = 3f;
        [SerializeField] protected LayerMask interactableLayer; // Will be set in Awake
        [SerializeField] private KeyCode interactionKey = KeyCode.E;
        
        
        [Header("UI References")]
        [SerializeField] protected CrosshairUI crosshairUI;
        [SerializeField] private Canvas uiCanvas;
        
        [Header("Debug Settings")]
        [SerializeField] private bool showDebugRay = true;
        [SerializeField] private Color debugRayColor = Color.red;
        
        [Header("Checkout Settings")]
        [SerializeField] private float checkoutDetectionRange = 5f;
        
        // Protected fields that can be accessed by derived classes
        protected IInteractable currentInteractable;
        protected Ray interactionRay;
        protected RaycastHit hitInfo;
        
        // Checkout-specific fields
        private CheckoutCounter nearbyCheckout;
        private bool isNearCheckout = false;
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            // Set up interaction layer mask
            if (interactableLayer == 0)
            {
                interactableLayer = InteractionLayers.AllInteractablesMask;
            }
            
            // Get camera if not assigned
            if (playerCamera == null)
            {
                playerCamera = Camera.main;
                if (playerCamera == null)
                {
                    playerCamera = FindAnyObjectByType<Camera>();
                }
            }
            
            if (playerCamera == null)
            {
                Debug.LogError("PlayerInteraction: No camera found! Please assign a camera.", this);
            }
            
            // Set up UI if needed
            SetupUI();
        }
        
        private void Update()
        {
            // Check for nearby checkout counter
            DetectNearbyCheckout();
            
            // Perform interaction raycast
            PerformInteractionRaycast();
            
            // Check for interaction input
            if (Input.GetKeyDown(interactionKey))
            {
                Debug.Log($"Interaction key ({interactionKey}) pressed!");
                TryInteract();
            }
        }
        
        private void OnDrawGizmos()
        {
            // Draw debug ray in Scene view
            if (showDebugRay && playerCamera != null)
            {
                Vector3 rayOrigin = playerCamera.transform.position;
                Vector3 rayDirection = playerCamera.transform.forward;
                
                Gizmos.color = debugRayColor;
                Gizmos.DrawRay(rayOrigin, rayDirection * interactionRange);
                
                // Draw hit point if we hit something
                if (currentInteractable != null)
                {
                    Gizmos.color = Color.green;
                    Gizmos.DrawWireSphere(hitInfo.point, 0.1f);
                }
            }
            
            // Draw checkout detection range
            if (isNearCheckout && nearbyCheckout != null)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(transform.position, checkoutDetectionRange);
                
                // Draw line to nearby checkout
                Gizmos.color = Color.magenta;
                Gizmos.DrawLine(transform.position, nearbyCheckout.transform.position);
            }
        }
        
        #endregion
        
        #region Checkout Detection
        
        /// <summary>
        /// Detect nearby checkout counters for context-aware interactions
        /// </summary>
        private void DetectNearbyCheckout()
        {
            CheckoutCounter closestCheckout = null;
            float closestDistance = float.MaxValue;
            
            // Find all checkout counters in scene
            CheckoutCounter[] checkouts = FindObjectsByType<CheckoutCounter>(FindObjectsSortMode.None);
            
            foreach (CheckoutCounter checkout in checkouts)
            {
                if (checkout != null)
                {
                    float distance = Vector3.Distance(transform.position, checkout.transform.position);
                    
                    if (distance <= checkoutDetectionRange && distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestCheckout = checkout;
                    }
                }
            }
            
            // Update checkout detection state
            bool wasNearCheckout = isNearCheckout;
            CheckoutCounter previousCheckout = nearbyCheckout;
            
            nearbyCheckout = closestCheckout;
            isNearCheckout = nearbyCheckout != null;
            
            // Log checkout state changes for debugging
            if (wasNearCheckout != isNearCheckout)
            {
                if (isNearCheckout)
                {
                    Debug.Log($"PlayerInteraction: Near checkout counter - {nearbyCheckout.name}");
                }
                else
                {
                    Debug.Log("PlayerInteraction: Left checkout area");
                }
            }
        }
        
        #endregion
        
        #region Interaction Methods
        
        /// <summary>
        /// Perform raycast to detect interactable objects
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
                    if (crosshairUI != null)
                    {
                        crosshairUI.HideInteractable();
                    }
                }
                
                // Enter new interactable
                currentInteractable = newInteractable;
                if (currentInteractable != null && currentInteractable.CanInteract)
                {
                    currentInteractable.OnInteractionEnter();
                    if (crosshairUI != null)
                    {
                        string interactionText = GetContextualInteractionText();
                        crosshairUI.ShowInteractable(interactionText);
                    }
                }
            }
        }
        
        /// <summary>
        /// Get contextual interaction text based on current state and object type
        /// </summary>
        /// <returns>Formatted interaction text</returns>
        private string GetContextualInteractionText()
        {
            if (currentInteractable == null)
                return "";
            
            // Check if we're looking at a product while near checkout
            Product product = currentInteractable as Product;
            if (product != null && isNearCheckout)
            {
                if (product.IsScannedAtCheckout)
                {
                    return $"Scanned ✓ - {product.ProductData?.ProductName ?? product.name}";
                }
                else
                {
                    return $"[{interactionKey}] Scan {product.ProductData?.ProductName ?? product.name}";
                }
            }
            
            // Check if we're looking at a checkout counter
            CheckoutCounter checkout = currentInteractable as CheckoutCounter;
            if (checkout != null)
            {
                if (!checkout.HasCustomer)
                {
                    return "Checkout Counter (No Customer)";
                }
                else if (!checkout.HasProducts)
                {
                    return "Waiting for Products";
                }
                else if (checkout.AllProductsScanned)
                {
                    return $"[{interactionKey}] Process Payment";
                }
                else
                {
                    // Count unscanned products
                    // Note: We can't access the products list directly, so we'll use the checkout's interaction text
                    return $"[{interactionKey}] {checkout.InteractionText}";
                }
            }
            
            // Default interaction text for other objects
            return $"[{interactionKey}] {currentInteractable.InteractionText}";
        }

        /// <summary>
        /// Attempt to interact with the current interactable object
        /// </summary>
        private void TryInteract()
        {
            if (currentInteractable != null && currentInteractable.CanInteract)
            {
                // Handle checkout-specific interactions for products
                Product product = currentInteractable as Product;
                if (product != null && isNearCheckout && !product.IsScannedAtCheckout)
                {
                    // Scan the product through the nearby checkout
                    nearbyCheckout.ScanProduct(product);
                    Debug.Log($"Scanned product: {product.ProductData?.ProductName ?? product.name}");
                    
                    // Update the interaction text immediately
                    if (crosshairUI != null)
                    {
                        string newText = GetContextualInteractionText();
                        crosshairUI.ShowInteractable(newText);
                    }
                    return;
                }
                
                // Handle normal interactions (including checkout counter interactions)
                currentInteractable.Interact(gameObject);
                Debug.Log($"Interacted with: {currentInteractable.InteractionText}");
                
                // Update interaction text after interaction (in case state changed)
                if (crosshairUI != null)
                {
                    string newText = GetContextualInteractionText();
                    crosshairUI.ShowInteractable(newText);
                }
            }
        }

        #endregion
        
        #region UI Setup
        
        /// <summary>
        /// Set up the crosshair UI if not already configured
        /// </summary>
        private void SetupUI()
        {
            // Find or create canvas
            if (uiCanvas == null)
            {
                uiCanvas = FindAnyObjectByType<Canvas>();
            }
            
            if (uiCanvas == null)
            {
                CreateUICanvas();
            }
            
            // Create crosshair UI if not assigned
            if (crosshairUI == null && uiCanvas != null)
            {
                crosshairUI = CrosshairUI.CreateCrosshairUI(uiCanvas);
//                Debug.Log("Created crosshair UI automatically");
            }
        }
        
        /// <summary>
        /// Create a basic UI canvas for the crosshair
        /// </summary>
        private void CreateUICanvas()
        {
            GameObject canvasGO = new GameObject("InteractionUI");
            Canvas canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100; // Ensure it's on top
            
            // Add CanvasScaler for responsive scaling
            var scaler = canvasGO.AddComponent<UnityEngine.UI.CanvasScaler>();
            scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = UnityEngine.UI.CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            
            // Add GraphicRaycaster for UI interactions
            canvasGO.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            
            uiCanvas = canvas;
            
            Debug.Log("Created interaction UI canvas");
        }
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// Set the interaction range
        /// </summary>
        /// <param name="range">New interaction range</param>
        public void SetInteractionRange(float range)
        {
            interactionRange = Mathf.Max(0.1f, range);
        }
        
        /// <summary>
        /// Set the interaction key
        /// </summary>
        /// <param name="key">New interaction key</param>
        public void SetInteractionKey(KeyCode key)
        {
            interactionKey = key;
        }
        
        /// <summary>
        /// Get the current interactable object
        /// </summary>
        /// <returns>The current interactable, or null if none</returns>
        public IInteractable GetCurrentInteractable()
        {
            return currentInteractable;
        }
        
        /// <summary>
        /// Check if player is currently looking at an interactable
        /// </summary>
        /// <returns>True if looking at an interactable</returns>
        public bool IsLookingAtInteractable()
        {
            return currentInteractable != null && currentInteractable.CanInteract;
        }
        
        /// <summary>
        /// Check if player is near a checkout counter
        /// </summary>
        /// <returns>True if near a checkout counter</returns>
        public bool IsNearCheckout()
        {
            return isNearCheckout;
        }
        
        /// <summary>
        /// Get the nearby checkout counter
        /// </summary>
        /// <returns>The nearby checkout counter, or null if none</returns>
        public CheckoutCounter GetNearbyCheckout()
        {
            return nearbyCheckout;
        }
        
        /// <summary>
        /// Set the checkout detection range
        /// </summary>
        /// <param name="range">New checkout detection range</param>
        public void SetCheckoutDetectionRange(float range)
        {
            checkoutDetectionRange = Mathf.Max(0.1f, range);
        }
        
        #endregion
        
        #region Debug Methods
        
        /// <summary>
        /// Get debug information about the current interaction state
        /// </summary>
        /// <returns>Debug info string</returns>
        public string GetDebugInfo()
        {
            string info = "";
            
            // Interaction info
            if (currentInteractable != null)
            {
                info += $"Looking at: {currentInteractable.InteractionText}\n";
                info += $"Can Interact: {currentInteractable.CanInteract}\n";
                info += $"Distance: {hitInfo.distance:F2}m\n";
                
                // Add product-specific info if applicable
                Product product = currentInteractable as Product;
                if (product != null)
                {
                    info += $"Product Scanned: {product.IsScannedAtCheckout}\n";
                }
            }
            else
            {
                info += "No interactable in range\n";
            }
            
            // Checkout info
            info += $"Near Checkout: {isNearCheckout}\n";
            if (isNearCheckout && nearbyCheckout != null)
            {
                info += $"Checkout: {nearbyCheckout.name}\n";
                info += $"Has Customer: {nearbyCheckout.HasCustomer}\n";
                info += $"Has Products: {nearbyCheckout.HasProducts}\n";
                info += $"All Scanned: {nearbyCheckout.AllProductsScanned}\n";
            }
            
            return info;
        }
        
        #endregion
    }
}
