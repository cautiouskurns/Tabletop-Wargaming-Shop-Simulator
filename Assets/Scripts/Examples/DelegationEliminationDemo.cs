using UnityEngine;

namespace TabletopShop.Examples
{
    /// <summary>
    /// Demonstrates the benefits of eliminating delegation methods in Customer.cs
    /// Shows before/after usage patterns and the flexibility gained.
    /// </summary>
    public class DelegationEliminationDemo : MonoBehaviour
    {
        [Header("Demo Customer")]
        [SerializeField] private Customer demoCustomer;
        
        void Start()
        {
            if (demoCustomer == null)
            {
                Debug.LogWarning("DelegationEliminationDemo: No demo customer assigned!");
                return;
            }
            
            DemonstrateFlexibilityGains();
            DemonstrateBackwardCompatibility();
            DemonstrateAdvancedPatterns();
        }
        
        /// <summary>
        /// Shows the flexibility gained by direct component access
        /// </summary>
        private void DemonstrateFlexibilityGains()
        {
            Debug.Log("=== FLEXIBILITY DEMONSTRATION ===");
            
            // BEFORE: Limited to predefined delegation methods
            // demoCustomer.SetDestination(Vector3.zero);        // Only these 7 methods available
            // demoCustomer.MoveToCheckoutPoint();
            // demoCustomer.HasReachedDestination();
            
            // AFTER: Full access to all component methods and properties
            if (demoCustomer.Movement != null)
            {
                // Can use ANY method on CustomerMovement
                demoCustomer.Movement.SetDestination(Vector3.zero);
                demoCustomer.Movement.MoveToCheckoutPoint();
                demoCustomer.Movement.StopMovement();
                
                // Can access ANY property
                bool isMoving = demoCustomer.Movement.IsMoving;
                Vector3 destination = demoCustomer.Movement.CurrentDestination;
                bool hasDestination = demoCustomer.Movement.HasDestination;
                
                Debug.Log($"Customer Movement State - IsMoving: {isMoving}, Destination: {destination}, HasDestination: {hasDestination}");
            }
        }
        
        /// <summary>
        /// Shows that legacy code still works through compatibility properties
        /// </summary>
        private void DemonstrateBackwardCompatibility()
        {
            Debug.Log("=== BACKWARD COMPATIBILITY ===");
            
            // Legacy properties still work for simple cases
            bool isMoving = demoCustomer.IsMoving;                    // Works without null checks
            Vector3 destination = demoCustomer.CurrentDestination;    // Safe fallback to Vector3.zero
            bool hasDestination = demoCustomer.HasDestination;        // Safe fallback to false
            
            // High-level actions provide semantic operations
            demoCustomer.StartShopping();    // State change + movement
            demoCustomer.StartPurchasing();  // State change + checkout
            demoCustomer.StartLeaving();     // State change + exit
            
            Debug.Log($"Legacy API still works - IsMoving: {isMoving}, CurrentDestination: {destination}");
        }
        
        /// <summary>
        /// Shows advanced patterns now possible with direct component access
        /// </summary>
        private void DemonstrateAdvancedPatterns()
        {
            Debug.Log("=== ADVANCED PATTERNS ===");
            
            // PATTERN 1: Custom movement sequences
            if (demoCustomer.Movement != null)
            {
                Vector3[] waypoints = {
                    new Vector3(0, 0, 0),
                    new Vector3(5, 0, 0),
                    new Vector3(5, 0, 5),
                    new Vector3(0, 0, 5)
                };
                
                StartCoroutine(ExecuteWaypointSequence(waypoints));
            }
            
            // PATTERN 2: Conditional behavior based on component state
            if (demoCustomer.Movement != null && demoCustomer.Behavior != null)
            {
                if (demoCustomer.Movement.IsMoving && demoCustomer.IsInState(CustomerState.Shopping))
                {
                    Debug.Log("Customer is actively shopping and moving - could trigger special effects");
                }
            }
            
            // PATTERN 3: Component interaction
            if (demoCustomer.Behavior != null && demoCustomer.Visuals != null)
            {
                // Components can work together without going through Customer
                float shoppingTime = demoCustomer.Behavior.ShoppingTime;
                demoCustomer.Visuals.UpdateColorForState(CustomerState.Shopping);
                
                Debug.Log($"Component coordination - Shopping time: {shoppingTime:F1}s, Visual state updated");
            }
        }
        
        /// <summary>
        /// Example of complex movement pattern impossible with old delegation approach
        /// </summary>
        private System.Collections.IEnumerator ExecuteWaypointSequence(Vector3[] waypoints)
        {
            Debug.Log("Executing custom waypoint sequence...");
            
            foreach (Vector3 waypoint in waypoints)
            {
                // Set destination
                if (demoCustomer.Movement?.SetDestination(waypoint) == true)
                {
                    Debug.Log($"Moving to waypoint: {waypoint}");
                    
                    // Wait for arrival
                    while (demoCustomer.Movement != null && 
                           !demoCustomer.Movement.HasReachedDestination() && 
                           demoCustomer.Movement.IsMoving)
                    {
                        yield return new WaitForSeconds(0.5f);
                    }
                    
                    Debug.Log($"Reached waypoint: {waypoint}");
                    
                    // Brief pause at waypoint
                    yield return new WaitForSeconds(1f);
                }
                else
                {
                    Debug.LogWarning($"Failed to set destination to waypoint: {waypoint}");
                    break;
                }
            }
            
            Debug.Log("Waypoint sequence completed!");
        }
    }
}
