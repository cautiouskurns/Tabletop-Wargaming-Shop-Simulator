using UnityEngine;

namespace TabletopShop.Examples
{
    /// <summary>
    /// Example demonstrating the improved Customer API with direct component access
    /// instead of delegation methods.
    /// </summary>
    public class CustomerUsageExample : MonoBehaviour
    {
        [Header("Example Customer Reference")]
        [SerializeField] private Customer exampleCustomer;
        
        /// <summary>
        /// Demonstrates the OLD way using delegation methods (removed)
        /// </summary>
        private void DemonstrateOldDelegationApproach()
        {
            // OLD WAY (now removed) - Multiple delegation methods with repetitive error handling:
            
            // exampleCustomer.SetDestination(Vector3.zero);           // Delegated to Movement
            // exampleCustomer.SetRandomShelfDestination();            // Delegated to Movement  
            // exampleCustomer.MoveToShelfPosition(someShelf);         // Delegated to Movement
            // exampleCustomer.MoveToCheckoutPoint();                  // Delegated to Movement
            // exampleCustomer.MoveToExitPoint();                      // Delegated to Movement
            // exampleCustomer.StopMovement();                         // Delegated to Movement
            // exampleCustomer.HasReachedDestination();                // Delegated to Movement
            
            // PROBLEMS with old approach:
            // 1. Repetitive null checks and error handling in every method
            // 2. Limited flexibility - can only use predefined delegation methods
            // 3. API bloat - Customer class has 7+ movement methods
            // 4. Maintenance overhead - changes to CustomerMovement require updating Customer
        }
        
        /// <summary>
        /// Demonstrates the NEW way with direct component access
        /// </summary>
        private void DemonstrateNewComponentAccess()
        {
            if (exampleCustomer == null) return;
            
            // NEW WAY 1: Direct component access for full flexibility
            if (exampleCustomer.Movement != null)
            {
                // Access any method on CustomerMovement directly
                exampleCustomer.Movement.SetDestination(Vector3.zero);
                exampleCustomer.Movement.SetRandomShelfDestination();
                exampleCustomer.Movement.MoveToCheckoutPoint();
                exampleCustomer.Movement.StopMovement();
                
                // Can access properties too
                Vector3 destination = exampleCustomer.Movement.CurrentDestination;
                bool isMoving = exampleCustomer.Movement.IsMoving;
                bool hasReached = exampleCustomer.Movement.HasReachedDestination();
            }
            
            // NEW WAY 2: Use high-level actions for common workflows
            exampleCustomer.StartShopping();    // State change + random shelf movement
            exampleCustomer.StartPurchasing();  // State change + checkout movement
            exampleCustomer.StartLeaving();     // State change + exit movement
            
            // NEW WAY 3: Access other components directly
            if (exampleCustomer.Behavior != null)
            {
                // Direct access to behavior methods
                exampleCustomer.Behavior.StartCustomerLifecycle(CustomerState.Shopping);
                float shoppingTime = exampleCustomer.Behavior.ShoppingTime;
            }
            
            if (exampleCustomer.Visuals != null)
            {
                // Direct access to visual methods
                exampleCustomer.Visuals.UpdateColorForState(CustomerState.Purchasing);
                string debugInfo = exampleCustomer.Visuals.GetDebugInfo();
            }
        }
        
        /// <summary>
        /// Demonstrates advanced usage scenarios made possible by direct component access
        /// </summary>
        private void DemonstrateAdvancedUsage()
        {
            if (exampleCustomer?.Movement == null) return;
            
            // ADVANCED USAGE 1: Custom movement patterns
            Vector3[] waypoints = { Vector3.zero, Vector3.forward, Vector3.right };
            foreach (Vector3 waypoint in waypoints)
            {
                exampleCustomer.Movement.SetDestination(waypoint);
                // Wait for customer to reach each waypoint...
            }
            
            // ADVANCED USAGE 2: Conditional logic based on component state
            if (exampleCustomer.Movement.IsMoving && exampleCustomer.IsInState(CustomerState.Shopping))
            {
                // Customer is actively shopping and moving
                Debug.Log("Customer is browsing while moving");
            }
            
            // ADVANCED USAGE 3: Component interaction
            if (exampleCustomer.Movement.HasReachedDestination() && 
                exampleCustomer.Behavior != null)
            {
                // Trigger behavior change when movement completes
                exampleCustomer.Behavior.StartCustomerLifecycle(CustomerState.Purchasing);
            }
        }
        
        /// <summary>
        /// Shows how to handle component availability gracefully
        /// </summary>
        private void DemonstrateNullSafetyPatterns()
        {
            // PATTERN 1: Null-conditional operator (recommended)
            exampleCustomer?.Movement?.SetDestination(Vector3.zero);
            
            // PATTERN 2: Explicit null check for complex operations
            if (exampleCustomer?.Movement != null)
            {
                exampleCustomer.Movement.SetRandomShelfDestination();
                Vector3 destination = exampleCustomer.Movement.CurrentDestination;
                Debug.Log($"Moving to: {destination}");
            }
            
            // PATTERN 3: Fallback values using null-coalescing
            Vector3 currentDest = exampleCustomer?.Movement?.CurrentDestination ?? Vector3.zero;
            bool isMoving = exampleCustomer?.Movement?.IsMoving ?? false;
        }
        
        /// <summary>
        /// Example of migrating from old delegation API to new component API
        /// </summary>
        private void MigrationExample()
        {
            // OLD: exampleCustomer.SetRandomShelfDestination();
            // NEW: exampleCustomer.Movement?.SetRandomShelfDestination();
            
            // OLD: exampleCustomer.MoveToCheckoutPoint();
            // NEW: exampleCustomer.Movement?.MoveToCheckoutPoint();
            //  OR: exampleCustomer.StartPurchasing(); // High-level action
            
            // OLD: exampleCustomer.HasReachedDestination();
            // NEW: exampleCustomer.Movement?.HasReachedDestination() ?? true;
            //  OR: exampleCustomer.HasDestination // Property still available
            
            // OLD: exampleCustomer.GetDebugInfo();
            // NEW: exampleCustomer.Visuals?.GetDebugInfo();
            //  OR: exampleCustomer.GetDebugInfo(); // Legacy method still available
        }
    }
    
    /// <summary>
    /// Benefits of the new approach:
    /// 
    /// 1. REDUCED CODE COMPLEXITY
    ///    - Eliminated 7+ delegation methods (100+ lines of repetitive code)
    ///    - No more repetitive null checks and error handling
    /// 
    /// 2. IMPROVED FLEXIBILITY  
    ///    - Direct access to full component API, not just predefined methods
    ///    - Can access properties, events, and advanced methods
    /// 
    /// 3. BETTER MAINTAINABILITY
    ///    - Changes to components don't require updating Customer class
    ///    - Single responsibility: Customer coordinates, components implement
    /// 
    /// 4. ENHANCED TESTABILITY
    ///    - Can test components independently
    ///    - Can mock individual components for unit testing
    /// 
    /// 5. FOLLOWS SOLID PRINCIPLES
    ///    - Interface Segregation: Consumers use only what they need
    ///    - Open/Closed: Can extend components without modifying Customer
    ///    - Dependency Inversion: Depend on component abstractions
    /// 
    /// 6. BACKWARD COMPATIBILITY
    ///    - Legacy properties still available (IsMoving, HasDestination, etc.)
    ///    - High-level actions provide simple API for common operations
    ///    - Gradual migration path from old to new API
    /// </summary>
    public class BenefitsDocumentation { }
}
