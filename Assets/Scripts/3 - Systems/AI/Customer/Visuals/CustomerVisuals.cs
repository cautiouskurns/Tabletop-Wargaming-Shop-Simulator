using UnityEngine;

namespace TabletopShop
{
    /// <summary>
    /// Handles customer visual feedback, debug information, and gizmo drawing.
    /// Manages visual representation and debugging tools for customer AI.
    /// Includes dynamic color system that changes customer appearance based on behavior phase.
    /// Implements ICustomerVisuals interface for enhanced interface segregation.
    /// </summary>
    public class CustomerVisuals : MonoBehaviour, ICustomerVisuals
    {
        [Header("State-Based Color System")]
        [SerializeField] private Color enteringColor = new Color(0.3f, 0.7f, 1f, 1f);    // Light blue
        [SerializeField] private Color shoppingColor = new Color(0.2f, 0.8f, 0.2f, 1f);  // Green
        [SerializeField] private Color purchasingColor = new Color(1f, 0.7f, 0.1f, 1f);  // Orange
        [SerializeField] private Color leavingColor = new Color(0.8f, 0.3f, 0.8f, 1f);   // Magenta
        [SerializeField] private Color defaultColor = Color.white;
        
        [Header("Visual Settings")]
        [SerializeField] private bool enableColorSystem = true;
        [SerializeField] private float colorTransitionSpeed = 2f;
        [SerializeField] private bool useEmissiveGlow = true;
        [SerializeField] private float emissiveIntensity = 0.3f;
        
        [Header("State Indicator System")]
        [SerializeField] private bool enableStateIndicator = true;
        [SerializeField] private float indicatorHeight = 2.5f; // Height above customer
        [SerializeField] private Vector3 indicatorOffset = Vector3.zero;
        [SerializeField] private bool showStateText = true;
        [SerializeField] private bool showStateIcon = false;
        [SerializeField] private float indicatorScale = 0.01f; // Much smaller scale for UI
        [SerializeField] private int textFontSize = 12; // Configurable font size
        
        [Header("State Icons (Optional)")]
        [SerializeField] private Sprite enteringIcon;
        [SerializeField] private Sprite shoppingIcon;
        [SerializeField] private Sprite purchasingIcon;
        [SerializeField] private Sprite leavingIcon;
        
        // Component references
        private CustomerMovement customerMovement;
        private Customer mainCustomer;
        
        // Visual state
        private bool showDebugGizmos = true;
        private MeshRenderer[] customerRenderers;
        private Material[] originalMaterials;
        private Material[] customerMaterials;
        private Color currentTargetColor;
        private Color currentColor;
        
        // State indicator components
        private GameObject stateIndicatorObject;
        private Canvas stateCanvas;
        private UnityEngine.UI.Text stateText;
        private UnityEngine.UI.Image stateIcon;
        private CustomerState lastDisplayedState;
        
        // Properties
        public bool ShowDebugGizmos 
        { 
            get => showDebugGizmos; 
            set => showDebugGizmos = value; 
        }
        
        public bool EnableColorSystem
        {
            get => enableColorSystem;
            set => enableColorSystem = value;
        }
        
        #region Initialization
        
        /// <summary>
        /// Initialize with component references
        /// </summary>
        public void Initialize(CustomerMovement movement, Customer customer)
        {
            customerMovement = movement;
            mainCustomer = customer;
            
            // Initialize color system
            SetupColorSystem();
            
            // Subscribe to state changes if possible
            if (mainCustomer != null)
            {
                // Update initial color based on current state
                UpdateColorForState(mainCustomer.Behavior.CurrentState);
            }
            
            // Initialize state indicator system
            if (enableStateIndicator)
            {
                SetupStateIndicator();
            }
        }
        
        private void Start()
        {
            // Fallback initialization if Initialize wasn't called
            if (mainCustomer == null)
            {
                mainCustomer = GetComponent<Customer>();
            }
            if (customerMovement == null)
            {
                customerMovement = GetComponent<CustomerMovement>();
            }
            
            if (customerRenderers == null)
            {
                SetupColorSystem();
            }
            
            // Initialize state indicator
            if (enableStateIndicator)
            {
                SetupStateIndicator();
            }
        }
        
        private void Update()
        {
            // Smooth color transitions
            if (enableColorSystem && currentColor != currentTargetColor)
            {
                currentColor = Color.Lerp(currentColor, currentTargetColor, colorTransitionSpeed * Time.deltaTime);
                ApplyColorToRenderers(currentColor);
            }
            
            // Update state indicator
            if (enableStateIndicator && stateIndicatorObject != null)
            {
                UpdateStateIndicator();
            }
        }
        
        private void OnDestroy()
        {
            // Clean up created materials
            CleanupMaterials();
            
            // Clean up state indicator
            if (stateIndicatorObject != null)
            {
                DestroyImmediate(stateIndicatorObject);
            }
        }
        
        /// <summary>
        /// Setup the color system by finding renderers and creating materials
        /// </summary>
        private void SetupColorSystem()
        {
            if (!enableColorSystem) return;
            
            // Find all MeshRenderers on this customer (including children)
            customerRenderers = GetComponentsInChildren<MeshRenderer>();
            
            if (customerRenderers.Length == 0)
            {
                Debug.LogWarning($"CustomerVisuals {name}: No MeshRenderers found for color system");
                return;
            }
            
            // Store original materials and create copies for modification
            originalMaterials = new Material[customerRenderers.Length];
            customerMaterials = new Material[customerRenderers.Length];
            
            for (int i = 0; i < customerRenderers.Length; i++)
            {
                if (customerRenderers[i].material != null)
                {
                    originalMaterials[i] = customerRenderers[i].material;
                    customerMaterials[i] = new Material(customerRenderers[i].material);
                    customerRenderers[i].material = customerMaterials[i];
                }
            }
            
            // Set initial color
            currentColor = defaultColor;
            currentTargetColor = defaultColor;
            ApplyColorToRenderers(currentColor);
            
            Debug.Log($"CustomerVisuals {name}: Color system initialized with {customerRenderers.Length} renderers");
        }
        
        /// <summary>
        /// Apply color to all customer renderers
        /// </summary>
        private void ApplyColorToRenderers(Color color)
        {
            if (customerMaterials == null) return;
            
            for (int i = 0; i < customerMaterials.Length; i++)
            {
                if (customerMaterials[i] != null)
                {
                    // Apply base color
                    customerMaterials[i].color = color;
                    
                    // Apply emissive glow if enabled
                    if (useEmissiveGlow && customerMaterials[i].HasProperty("_EmissionColor"))
                    {
                        customerMaterials[i].EnableKeyword("_EMISSION");
                        customerMaterials[i].SetColor("_EmissionColor", color * emissiveIntensity);
                    }
                }
            }
        }
        
        /// <summary>
        /// Update color based on customer state
        /// </summary>
        public void UpdateColorForState(CustomerState state)
        {
            if (!enableColorSystem) return;
            
            Color newTargetColor = GetColorForState(state);
            
            if (newTargetColor != currentTargetColor)
            {
                currentTargetColor = newTargetColor;
                Debug.Log($"CustomerVisuals {name}: Color changing to {newTargetColor} for state {state}");
            }
        }
        
        /// <summary>
        /// Get the appropriate color for a given customer state
        /// </summary>
        private Color GetColorForState(CustomerState state)
        {
            switch (state)
            {
                case CustomerState.Entering:
                    return enteringColor;
                case CustomerState.Shopping:
                    return shoppingColor;
                case CustomerState.Purchasing:
                    return purchasingColor;
                case CustomerState.Leaving:
                    return leavingColor;
                default:
                    return defaultColor;
            }
        }
        
        /// <summary>
        /// Force immediate color change without transition
        /// </summary>
        public void SetColorImmediate(CustomerState state)
        {
            if (!enableColorSystem) return;
            
            Color newColor = GetColorForState(state);
            currentColor = newColor;
            currentTargetColor = newColor;
            ApplyColorToRenderers(currentColor);
        }
        
        /// <summary>
        /// Reset customer to default appearance
        /// </summary>
        public void ResetToDefaultColor()
        {
            if (!enableColorSystem) return;
            
            currentTargetColor = defaultColor;
        }
        
        /// <summary>
        /// Clean up created materials to prevent memory leaks
        /// </summary>
        private void CleanupMaterials()
        {
            if (customerMaterials != null)
            {
                for (int i = 0; i < customerMaterials.Length; i++)
                {
                    if (customerMaterials[i] != null)
                    {
                        if (Application.isPlaying)
                        {
                            Destroy(customerMaterials[i]);
                        }
                        else
                        {
                            DestroyImmediate(customerMaterials[i]);
                        }
                    }
                }
            }
        }
        
        /// <summary>
        /// Setup the state indicator UI system
        /// </summary>
        private void SetupStateIndicator()
        {
            if (!enableStateIndicator) return;
            
            // Clean up any existing indicator first
            if (stateIndicatorObject != null)
            {
                if (Application.isPlaying)
                    Destroy(stateIndicatorObject);
                else
                    DestroyImmediate(stateIndicatorObject);
            }
            
            // Create the state indicator GameObject
            stateIndicatorObject = new GameObject($"{name}_StateIndicator");
            stateIndicatorObject.transform.SetParent(transform);
            
            // Add Canvas component for World Space UI
            stateCanvas = stateIndicatorObject.AddComponent<Canvas>();
            stateCanvas.renderMode = RenderMode.WorldSpace;
            stateCanvas.worldCamera = Camera.main;
            
            // Scale the canvas to make it much smaller
            stateIndicatorObject.transform.localScale = Vector3.one * indicatorScale;
            
            // Add CanvasScaler for consistent sizing
            var canvasScaler = stateIndicatorObject.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasScaler.scaleFactor = 1f;
            canvasScaler.dynamicPixelsPerUnit = 100f; // Higher value for sharper text at small scale
            
            // Add GraphicRaycaster (required for UI)
            stateIndicatorObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            
            // Set initial position
            stateIndicatorObject.transform.position = transform.position + Vector3.up * indicatorHeight + indicatorOffset;
            
            // Add Text component for state name
            if (showStateText)
            {
                var textObject = new GameObject("StateText");
                textObject.transform.SetParent(stateIndicatorObject.transform, false);
                
                stateText = textObject.AddComponent<UnityEngine.UI.Text>();
                stateText.text = GetStateDisplayName(CustomerState.Entering);
                stateText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                stateText.fontSize = textFontSize;
                stateText.color = Color.white;
                stateText.alignment = TextAnchor.MiddleCenter;
                
                // Set RectTransform for proper sizing - larger canvas space but smaller scale
                var rectTransform = stateText.GetComponent<RectTransform>();
                rectTransform.sizeDelta = new Vector2(200, 50); // Larger canvas size
                rectTransform.anchoredPosition = Vector2.zero;
            }
            
            // Add Image component for state icon
            if (showStateIcon)
            {
                var iconObject = new GameObject("StateIcon");
                iconObject.transform.SetParent(stateIndicatorObject.transform, false);
                
                stateIcon = iconObject.AddComponent<UnityEngine.UI.Image>();
                stateIcon.color = Color.white;
                
                // Position icon above or below text
                var rectTransform = stateIcon.GetComponent<RectTransform>();
                rectTransform.sizeDelta = new Vector2(48, 48); // Larger for better visibility at small scale
                rectTransform.anchoredPosition = showStateText ? new Vector2(0, 40) : Vector2.zero;
            }
            
            // Initial update
            UpdateStateIndicator();
            
            Debug.Log($"CustomerVisuals {name}: State indicator initialized with scale {indicatorScale}");
        }
        
        /// <summary>
        /// Get display name for a customer state - now prioritizes BD task names
        /// </summary>
        private string GetStateDisplayName(CustomerState state)
        {
            // Try to get BD task name first if customer has useBehaviorDesigner enabled
            if (mainCustomer != null && mainCustomer.Behavior != null && mainCustomer.Behavior.useBehaviorDesigner)
            {
                string bdTaskName = mainCustomer.Behavior.GetCurrentBDTaskName();
                if (!string.IsNullOrEmpty(bdTaskName) && bdTaskName != "BD Error" && bdTaskName != "No BD")
                {
                    return bdTaskName;
                }
            }
            
            // Fallback to legacy state names
            switch (state)
            {
                case CustomerState.Entering:
                    return "Entering";
                case CustomerState.Shopping:
                    return "Shopping";
                case CustomerState.Purchasing:
                    return "Checkout";
                case CustomerState.Leaving:
                    return "Leaving";
                default:
                    return state.ToString();
            }
        }
        
        /// <summary>
        /// Update the state indicator UI based on the current state
        /// </summary>
        private void UpdateStateIndicator()
        {
            if (!enableStateIndicator || stateIndicatorObject == null || mainCustomer == null) 
                return;
            
            // Ensure the indicator exists and is properly initialized
            if (stateText == null && showStateText || stateIcon == null && showStateIcon)
            {
                // Reinitialize if components are missing
                SetupStateIndicator();
                return;
            }
            
            // Update position to follow the customer
            stateIndicatorObject.transform.position = transform.position + Vector3.up * indicatorHeight + indicatorOffset;
            
            // Update state text with clean transition
            if (stateText != null && showStateText)
            {
                string newStateText = GetStateDisplayName(mainCustomer.Behavior.CurrentState);
                if (stateText.text != newStateText)
                {
                    stateText.text = newStateText;
                    Debug.Log($"CustomerVisuals {name}: State text updated to '{newStateText}'");
                }
            }
            
            // Update state icon
            if (stateIcon != null && showStateIcon)
            {
                Sprite newSprite = GetIconForState(mainCustomer.Behavior.CurrentState);
                stateIcon.sprite = newSprite;
                stateIcon.enabled = newSprite != null;
            }
            
            lastDisplayedState = mainCustomer.Behavior.CurrentState;
        }
        
        /// <summary>
        /// Get the icon sprite for a given customer state
        /// </summary>
        private Sprite GetIconForState(CustomerState state)
        {
            switch (state)
            {
                case CustomerState.Entering:
                    return enteringIcon;
                case CustomerState.Shopping:
                    return shoppingIcon;
                case CustomerState.Purchasing:
                    return purchasingIcon;
                case CustomerState.Leaving:
                    return leavingIcon;
                default:
                    return null;
            }
        }
        
        #endregion
        
        #region Debug and Visualization
        
        /// <summary>
        /// Get debug information about customer state
        /// </summary>
        /// <returns>Debug string with customer information</returns>
        public string GetDebugInfo()
        {
            if (mainCustomer == null || customerMovement == null)
            {
                return $"Customer {name}: Components not initialized";
            }
            
            return $"Customer {name}: State={mainCustomer.Behavior.CurrentState}, " +
                   $"ShoppingTime={mainCustomer.Behavior.ShoppingTime:F1}s, " +
                   $"IsMoving={customerMovement.IsMoving}, " +
                   $"HasDestination={customerMovement.HasDestination}, " +
                   $"Destination={customerMovement.CurrentDestination}, " +
                   $"Distance={Vector3.Distance(transform.position, customerMovement.CurrentDestination):F2}, " +
                   $"TargetShelf={mainCustomer.Behavior.TargetShelf?.name ?? "None"}";
        }
        
        /// <summary>
        /// Log detailed debug information to console
        /// </summary>
        public void LogDebugInfo()
        {
            Debug.Log(GetDebugInfo());
        }
        
        /// <summary>
        /// Get customer status for UI display
        /// </summary>
        /// <returns>Formatted status string</returns>
        public string GetStatusString()
        {
            if (mainCustomer == null)
            {
                return "Not Initialized";
            }
            
            string status = mainCustomer.Behavior.CurrentState.ToString();
            
            if (customerMovement != null && customerMovement.IsMoving)
            {
                status += " (Moving)";
            }
            
            return status;
        }
        
        /// <summary>
        /// Draw debug gizmos for destination and pathfinding
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            if (!showDebugGizmos || customerMovement == null) return;
            
            if (customerMovement.HasDestination)
            {
                Vector3 destination = customerMovement.CurrentDestination;
                
                // Draw destination
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(destination, 0.5f);
                
                // Draw line to destination
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(transform.position, destination);
                
                // Draw stopping distance
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(destination, 1.5f); // Use destinationReachedDistance
            }
            
            // Draw NavMeshAgent path if available
            if (customerMovement.NavMeshAgent != null && customerMovement.NavMeshAgent.hasPath)
            {
                Gizmos.color = Color.blue;
                var path = customerMovement.NavMeshAgent.path.corners;
                for (int i = 0; i < path.Length - 1; i++)
                {
                    Gizmos.DrawLine(path[i], path[i + 1]);
                }
            }
            
            // Draw current state indicator
            DrawStateIndicator();
        }
        
        /// <summary>
        /// Draw visual indicator for current customer state
        /// </summary>
        private void DrawStateIndicator()
        {
            if (mainCustomer == null) return;
            
            Vector3 indicatorPosition = transform.position + Vector3.up * 2f;
            
            switch (mainCustomer.Behavior.CurrentState)
            {
                case CustomerState.Entering:
                    Gizmos.color = Color.cyan;
                    break;
                case CustomerState.Shopping:
                    Gizmos.color = Color.green;
                    break;
                case CustomerState.Purchasing:
                    Gizmos.color = Color.yellow;
                    break;
                case CustomerState.Leaving:
                    Gizmos.color = Color.magenta;
                    break;
                default:
                    Gizmos.color = Color.white;
                    break;
            }
            
            Gizmos.DrawWireCube(indicatorPosition, Vector3.one * 0.3f);
        }
        
        /// <summary>
        /// Draw debug information in scene view
        /// </summary>
        private void OnDrawGizmos()
        {
            if (!showDebugGizmos) return;
            
            // Draw basic customer information
            Vector3 labelPosition = transform.position + Vector3.up * 2.5f;
            
            if (mainCustomer != null)
            {
                // Note: Unity's Gizmos.Label doesn't exist, but this shows where labels would go
                // In practice, you might use GUI.Label in OnGUI() or a custom editor script
                
                // Draw a simple sphere to indicate customer presence
                Gizmos.color = Color.white;
                Gizmos.DrawWireSphere(transform.position + Vector3.up, 0.1f);
            }
        }
        
        #endregion
        
        #region Visual Feedback
        
        /// <summary>
        /// Update visual representation based on customer state
        /// </summary>
        public void UpdateVisualFeedback()
        {
            if (mainCustomer == null) return;
            
            // Here you could add visual effects like:
            // - Changing material colors based on state
            // - Playing animations
            // - Showing particle effects
            // - Updating UI elements
            
            UpdateNameDisplay();
            UpdateStateVisuals();
        }
        
        /// <summary>
        /// Update name display for debugging
        /// </summary>
        private void UpdateNameDisplay()
        {
            if (mainCustomer != null)
            {
                gameObject.name = $"Customer_{mainCustomer.Behavior.CurrentState}";
            }
        }
        
        /// <summary>
        /// Update visual effects based on current state
        /// </summary>
        private void UpdateStateVisuals()
        {
            // This is where you would implement state-specific visual effects
            // For example:
            // - Entering: Show entrance effect
            // - Shopping: Show browsing animations
            // - Purchasing: Show interaction with checkout
            // - Leaving: Show exit effect
        }
        
        /// <summary>
        /// Show visual indicator for reaching destination
        /// </summary>
        public void ShowDestinationReachedEffect()
        {
            Debug.Log($"CustomerVisuals {name}: Destination reached - could show visual effect here");
            // Here you could add particle effects, sound, or other feedback
        }
        
        /// <summary>
        /// Show visual indicator for state change
        /// </summary>
        public void ShowStateChangeEffect(CustomerState oldState, CustomerState newState)
        {
            Debug.Log($"CustomerVisuals {name}: State changed from {oldState} to {newState}");
            // Here you could add transition effects
        }
        
        #endregion
        
        #region Editor Support
        
        /// <summary>
        /// Toggle debug gizmo visibility
        /// </summary>
        [ContextMenu("Toggle Debug Gizmos")]
        public void ToggleDebugGizmos()
        {
            showDebugGizmos = !showDebugGizmos;
            Debug.Log($"CustomerVisuals {name}: Debug gizmos {(showDebugGizmos ? "enabled" : "disabled")}");
        }
        
        /// <summary>
        /// Print debug information to console
        /// </summary>
        [ContextMenu("Print Debug Info")]
        public void PrintDebugInfo()
        {
            Debug.Log($"=== Customer Debug Info ===");
            Debug.Log(GetDebugInfo());
            Debug.Log($"========================");
        }
        
        #endregion
        
        #region Legacy Field Migration
        
        /// <summary>
        /// Migrate legacy fields from main Customer component
        /// </summary>
        public void MigrateLegacyFields(bool legacyShowDebugGizmos = true)
        {
            showDebugGizmos = legacyShowDebugGizmos;
            
            Debug.Log("CustomerVisuals: Legacy fields migrated successfully");
        }
        
        #endregion

        /// <summary>
        /// Update the state indicator when state changes (call this from CustomerBehavior)
        /// </summary>
        /// <param name="newState">The new customer state</param>
        public void UpdateStateDisplay(CustomerState newState)
        {
            if (enableStateIndicator && stateIndicatorObject != null)
            {
                lastDisplayedState = newState;
                UpdateStateIndicator();
                
                // Update color system as well
                if (enableColorSystem)
                {
                    UpdateColorForState(newState);
                }
            }
        }

        /// <summary>
        /// Reinitialize the state indicator system with current settings
        /// Call this if you change indicator settings at runtime
        /// </summary>
        public void ReinitializeStateIndicator()
        {
            if (enableStateIndicator)
            {
                SetupStateIndicator();
            }
            else
            {
                // Clean up existing indicator
                if (stateIndicatorObject != null)
                {
                    if (Application.isPlaying)
                        Destroy(stateIndicatorObject);
                    else
                        DestroyImmediate(stateIndicatorObject);
                    
                    stateIndicatorObject = null;
                    stateCanvas = null;
                    stateText = null;
                    stateIcon = null;
                }
            }
        }
    }
}
