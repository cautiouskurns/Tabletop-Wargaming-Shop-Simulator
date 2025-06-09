using UnityEngine;

namespace TabletopShop
{
    /// <summary>
    /// Handles customer visual feedback, debug information, and gizmo drawing.
    /// Manages visual representation and debugging tools for customer AI.
    /// Includes dynamic color system that changes customer appearance based on behavior phase.
    /// </summary>
    public class CustomerVisuals : MonoBehaviour
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
                UpdateColorForState(mainCustomer.CurrentState);
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
        }
        
        private void Update()
        {
            // Smooth color transitions
            if (enableColorSystem && currentColor != currentTargetColor)
            {
                currentColor = Color.Lerp(currentColor, currentTargetColor, colorTransitionSpeed * Time.deltaTime);
                ApplyColorToRenderers(currentColor);
            }
        }
        
        private void OnDestroy()
        {
            // Clean up created materials
            CleanupMaterials();
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
            
            return $"Customer {name}: State={mainCustomer.CurrentState}, " +
                   $"ShoppingTime={mainCustomer.ShoppingTime:F1}s, " +
                   $"IsMoving={customerMovement.IsMoving}, " +
                   $"HasDestination={customerMovement.HasDestination}, " +
                   $"Destination={customerMovement.CurrentDestination}, " +
                   $"Distance={Vector3.Distance(transform.position, customerMovement.CurrentDestination):F2}, " +
                   $"TargetShelf={mainCustomer.TargetShelf?.name ?? "None"}";
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
            
            string status = mainCustomer.CurrentState.ToString();
            
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
            
            switch (mainCustomer.CurrentState)
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
                gameObject.name = $"Customer_{mainCustomer.CurrentState}";
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
    }
}
