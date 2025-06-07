using UnityEngine;

namespace TabletopShop
{
    /// <summary>
    /// Handles customer visual feedback, debug information, and gizmo drawing.
    /// Manages visual representation and debugging tools for customer AI.
    /// </summary>
    public class CustomerVisuals : MonoBehaviour
    {
        // Component references
        private CustomerMovement customerMovement;
        private Customer mainCustomer;
        
        // Visual state
        private bool showDebugGizmos = true;
        
        // Properties
        public bool ShowDebugGizmos 
        { 
            get => showDebugGizmos; 
            set => showDebugGizmos = value; 
        }
        
        #region Initialization
        
        /// <summary>
        /// Initialize with component references
        /// </summary>
        public void Initialize(CustomerMovement movement, Customer customer)
        {
            customerMovement = movement;
            mainCustomer = customer;
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
