using UnityEngine;
using TMPro;

namespace TabletopShop
{
    /// <summary>
    /// Interactive shop window sign that displays OPEN/CLOSED status
    /// Can be placed on windows, doors, or anywhere in the shop
    /// Players can interact with it to manually open/close the shop
    /// </summary>
    public class ShopWindowSign : MonoBehaviour, IInteractable
    {
        [Header("Sign Configuration")]
        [SerializeField] private TextMeshPro signText;
        [SerializeField] private MeshRenderer signBackground;
        
        [Header("Sign Appearance")]
        [SerializeField] private string openText = "OPEN";
        [SerializeField] private string closedText = "CLOSED";
        
        [Header("Colors")]
        [SerializeField] private Color openTextColor = Color.green;
        [SerializeField] private Color closedTextColor = Color.red;
        [SerializeField] private Material openBackgroundMaterial;
        [SerializeField] private Material closedBackgroundMaterial;
        
        [Header("Auto-Connect")]
        [SerializeField] private bool autoFindStoreHours = true;
        
        [Header("Manual Override")]
        [SerializeField] private bool allowPlayerControl = true;
        [SerializeField] private bool manualOverrideActive = false;
        [SerializeField] private string interactionPrompt = "Toggle Shop";
        
        // Component references
        private StoreHours storeHours;
        private bool isCurrentlyOpen = true;
        private bool hasHighlight = false;
        
        #region IInteractable Implementation
        
        public string InteractionText 
        { 
            get 
            {
                if (!allowPlayerControl) return "Sign (Read Only)";
                
                bool currentState = GetCurrentStoreState();
                bool targetState = !currentState;
                string actionText = targetState ? "Open Shop" : "Close Shop";
                string currentStateText = currentState ? "OPEN" : "CLOSED";
                
                return $"[E] {actionText} (Currently {currentStateText})";
            } 
        }
        
        public bool CanInteract => allowPlayerControl;
        
        /// <summary>
        /// Handle player interaction to toggle shop state
        /// </summary>
        /// <param name="player">The player GameObject</param>
        public void Interact(GameObject player)
        {
            if (!allowPlayerControl)
            {
                Debug.Log("Shop sign interaction is disabled");
                return;
            }
            
            Debug.Log($"ShopWindowSign: Player {player.name} interacting with sign");
            DebugState();
            ToggleShopState();
        }
        
        /// <summary>
        /// Called when player starts looking at the sign
        /// </summary>
        public void OnInteractionEnter()
        {
            if (!allowPlayerControl) return;
            
            // Add visual highlight to indicate interactability
            ApplyInteractionHighlight();
            hasHighlight = true;
        }
        
        /// <summary>
        /// Called when player stops looking at the sign
        /// </summary>
        public void OnInteractionExit()
        {
            if (!hasHighlight) return;
            
            // Remove visual highlight
            RemoveInteractionHighlight();
            hasHighlight = false;
        }
        
        #endregion
        
        #region Unity Lifecycle
        
        private void Start()
        {
            // Set up text component if not assigned
            if (signText == null)
                signText = GetComponentInChildren<TextMeshPro>();
            
            // Set up background renderer if not assigned
            if (signBackground == null)
                signBackground = GetComponent<MeshRenderer>();
            
            // Find StoreHours if auto-connect is enabled
            if (autoFindStoreHours && storeHours == null)
            {
                storeHours = FindFirstObjectByType<StoreHours>();
                if (storeHours != null)
                {
                    Debug.Log($"ShopWindowSign: Connected to StoreHours automatically");
                }
            }
            
            // Setup interaction collider if needed
            SetupInteractionCollider();
            
            // Set initial sign state
            UpdateSignDisplay();
        }
        
        /// <summary>
        /// Setup collider for interaction system
        /// </summary>
        private void SetupInteractionCollider()
        {
            if (!allowPlayerControl) return;
            
            Collider signCollider = GetComponent<Collider>();
            if (signCollider == null)
            {
                // Add a collider for interaction
                BoxCollider boxCollider = gameObject.AddComponent<BoxCollider>();
                
                // Set reasonable size based on sign
                if (signText != null)
                {
                    Bounds textBounds = signText.bounds;
                    boxCollider.size = new Vector3(
                        Mathf.Max(textBounds.size.x, 1f),
                        Mathf.Max(textBounds.size.y, 0.5f),
                        0.2f
                    );
                }
                else
                {
                    boxCollider.size = new Vector3(2f, 1f, 0.2f);
                }
                
                Debug.Log("ShopWindowSign: Added interaction collider");
            }
            
            // Set appropriate layer for interaction system
            // You may need to adjust this based on your interaction layer setup
            if (gameObject.layer == 0) // Default layer
            {
                gameObject.layer = LayerMask.NameToLayer("Interactable");
                if (gameObject.layer == -1)
                {
                    Debug.LogWarning("ShopWindowSign: 'Interactable' layer not found. Please create it or set the layer manually.");
                }
            }
        }
        
        private void Update()
        {
            // Only check for automatic state changes if manual override is not active
            if (!manualOverrideActive && storeHours != null)
            {
                bool newStatus = storeHours.IsStoreOpen;
                if (newStatus != isCurrentlyOpen)
                {
                    isCurrentlyOpen = newStatus;
                    UpdateSignDisplay();
                }
            }
            // If manual override is active, keep our current state and don't auto-update
        }
        
        #endregion
        
        #region Manual Control Methods
        
        /// <summary>
        /// Toggle the shop's open/closed state
        /// </summary>
        public void ToggleShopState()
        {
            if (storeHours == null)
            {
                Debug.LogWarning("Cannot toggle shop state - StoreHours component not found");
                return;
            }
            
            // Get current state before toggle
            bool currentState = GetCurrentStoreState();
            Debug.Log($"ShopWindowSign: Current store state before toggle: {(currentState ? "OPEN" : "CLOSED")}");
            
            // Enable manual override mode
            manualOverrideActive = true;
            
            // Toggle our internal state first
            isCurrentlyOpen = !currentState;
            
            // Then force StoreHours to match our state
            if (isCurrentlyOpen)
            {
                Debug.Log("ShopWindowSign: Forcing store to OPEN");
                storeHours.ForceOpenStore();
            }
            else
            {
                Debug.Log("ShopWindowSign: Forcing store to CLOSED");
                storeHours.ForceCloseStore();
            }
            
            // Verify the state change
            bool newState = storeHours.IsStoreOpen;
            Debug.Log($"ShopWindowSign: Store state after toggle: {(newState ? "OPEN" : "CLOSED")}");
            Debug.Log($"ShopWindowSign: Sign internal state: {(isCurrentlyOpen ? "OPEN" : "CLOSED")}");
            
            // Force update the display
            UpdateSignDisplay();
            
            Debug.Log($"ShopWindowSign: Shop manually {(isCurrentlyOpen ? "opened" : "closed")} via sign interaction");
        }
        
        /// <summary>
        /// Get the current store state (considering manual override)
        /// </summary>
        private bool GetCurrentStoreState()
        {
            if (manualOverrideActive)
            {
                // When in manual override, use our tracked state
                return isCurrentlyOpen;
            }
            
            if (storeHours != null)
                return storeHours.IsStoreOpen;
            return isCurrentlyOpen;
        }
        
        /// <summary>
        /// Disable manual override and return to automatic time-based control
        /// </summary>
        [ContextMenu("Return to Automatic Control")]
        public void ReturnToAutomaticControl()
        {
            manualOverrideActive = false;
            Debug.Log("Shop sign returned to automatic time-based control");
        }
        
        /// <summary>
        /// Apply visual highlight for interaction
        /// </summary>
        private void ApplyInteractionHighlight()
        {
            // Simple highlight effect - could be enhanced with outline or glow
            if (signText != null)
            {
                // Make text slightly brighter/more saturated
                Color currentColor = signText.color;
                signText.color = new Color(
                    Mathf.Min(currentColor.r * 1.2f, 1f),
                    Mathf.Min(currentColor.g * 1.2f, 1f),
                    Mathf.Min(currentColor.b * 1.2f, 1f),
                    currentColor.a
                );
            }
            
            if (signBackground != null)
            {
                // Optional: Apply highlight to background as well
                Material currentMat = signBackground.material;
                if (currentMat != null)
                {
                    // Increase emission or brightness
                    Color emissionColor = currentMat.GetColor("_EmissionColor");
                    if (emissionColor != Color.black)
                    {
                        currentMat.SetColor("_EmissionColor", emissionColor * 1.5f);
                    }
                }
            }
        }
        
        /// <summary>
        /// Remove visual highlight
        /// </summary>
        private void RemoveInteractionHighlight()
        {
            // Restore original appearance based on current state, not auto-refresh
            bool isOpen = GetCurrentStoreState();
            
            // Update text with correct state
            if (signText != null)
            {
                signText.text = isOpen ? openText : closedText;
                signText.color = isOpen ? openTextColor : closedTextColor;
            }
            
            // Update background material with correct state
            if (signBackground != null)
            {
                if (isOpen && openBackgroundMaterial != null)
                {
                    signBackground.material = openBackgroundMaterial;
                }
                else if (!isOpen && closedBackgroundMaterial != null)
                {
                    signBackground.material = closedBackgroundMaterial;
                }
            }
        }
        
        #endregion
        
        #region Display Management
        
        /// <summary>
        /// Update the sign display based on current store status
        /// </summary>
        private void UpdateSignDisplay()
        {
            bool isOpen = GetCurrentStoreState();
            
            // Update text
            if (signText != null)
            {
                signText.text = isOpen ? openText : closedText;
                signText.color = isOpen ? openTextColor : closedTextColor;
            }
            
            // Update background material
            if (signBackground != null)
            {
                if (isOpen && openBackgroundMaterial != null)
                {
                    signBackground.material = openBackgroundMaterial;
                }
                else if (!isOpen && closedBackgroundMaterial != null)
                {
                    signBackground.material = closedBackgroundMaterial;
                }
            }
            
            // Show manual override status in debug
            string modeText = manualOverrideActive ? " (Manual)" : " (Auto)";
            Debug.Log($"ShopWindowSign: Updated to show '{(isOpen ? openText : closedText)}'{modeText}");
        }
        
        /// <summary>
        /// Manually set the store hours reference
        /// </summary>
        public void SetStoreHours(StoreHours newStoreHours)
        {
            storeHours = newStoreHours;
            UpdateSignDisplay();
        }
        
        /// <summary>
        /// Manually set the sign to open
        /// </summary>
        [ContextMenu("Show Open")]
        public void ShowOpen()
        {
            isCurrentlyOpen = true;
            if (signText != null)
            {
                signText.text = openText;
                signText.color = openTextColor;
            }
            if (signBackground != null && openBackgroundMaterial != null)
            {
                signBackground.material = openBackgroundMaterial;
            }
        }
        
        /// <summary>
        /// Manually set the sign to closed
        /// </summary>
        [ContextMenu("Show Closed")]
        public void ShowClosed()
        {
            isCurrentlyOpen = false;
            if (signText != null)
            {
                signText.text = closedText;
                signText.color = closedTextColor;
            }
            if (signBackground != null && closedBackgroundMaterial != null)
            {
                signBackground.material = closedBackgroundMaterial;
            }
        }
        
        /// <summary>
        /// Toggle between open and closed for testing
        /// </summary>
        [ContextMenu("Toggle Sign")]
        public void ToggleSign()
        {
            if (isCurrentlyOpen)
                ShowClosed();
            else
                ShowOpen();
        }
        
        /// <summary>
        /// Debug method to check state synchronization
        /// </summary>
        [ContextMenu("Debug State")]
        public void DebugState()
        {
            Debug.Log("=== ShopWindowSign State Debug ===");
            Debug.Log($"Manual Override Active: {manualOverrideActive}");
            Debug.Log($"Sign's isCurrentlyOpen: {isCurrentlyOpen}");
            if (storeHours != null)
            {
                Debug.Log($"StoreHours.IsStoreOpen: {storeHours.IsStoreOpen}");
                Debug.Log($"StoreHours object: {storeHours.name}");
            }
            else
            {
                Debug.Log("StoreHours: NULL - not connected!");
            }
            Debug.Log($"GetCurrentStoreState(): {GetCurrentStoreState()}");
            Debug.Log($"Sign Text: {(signText != null ? signText.text : "NULL")}");
            Debug.Log($"Allow Player Control: {allowPlayerControl}");
            Debug.Log($"Can Interact: {CanInteract}");
            Debug.Log($"Interaction Text: {InteractionText}");
            Debug.Log("================================");
        }
        
        /// <summary>
        /// Direct toggle method that doesn't rely on StoreHours (for troubleshooting)
        /// </summary>
        [ContextMenu("Direct Toggle (Bypass StoreHours)")]
        public void DirectToggleState()
        {
            Debug.Log("ShopWindowSign: Direct toggle (bypassing StoreHours)");
            
            // Enable manual override mode
            manualOverrideActive = true;
            
            // Toggle our internal state
            isCurrentlyOpen = !isCurrentlyOpen;
            
            Debug.Log($"ShopWindowSign: Direct state set to {(isCurrentlyOpen ? "OPEN" : "CLOSED")}");
            
            // Update display immediately
            UpdateSignDisplay();
        }
        
        #endregion
    }
}