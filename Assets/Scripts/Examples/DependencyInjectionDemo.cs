using UnityEngine;

namespace TabletopShop.Examples
{
    /// <summary>
    /// Demonstrates the benefits of the dependency injection refactor in Customer.cs
    /// Shows improved testability, explicit dependencies, and better architecture
    /// </summary>
    public class DependencyInjectionDemo : MonoBehaviour
    {
        [Header("Demo Setup")]
        [SerializeField] private Customer demoCustomer;
        
        void Start()
        {
            if (demoCustomer == null)
            {
                Debug.LogWarning("DependencyInjectionDemo: No demo customer assigned!");
                return;
            }
            
            DemonstrateExplicitDependencies();
            DemonstrateImprovedValidation();
            DemonstrateTestability();
            DemonstrateInspectorBenefits();
        }
        
        /// <summary>
        /// Shows how dependencies are now explicit and visible
        /// </summary>
        private void DemonstrateExplicitDependencies()
        {
            Debug.Log("=== EXPLICIT DEPENDENCIES DEMONSTRATION ===");
            
            // BEFORE: Hidden dependencies - you couldn't see what Customer needed
            // Components were created automatically with AddComponent()
            
            // AFTER: Dependencies are visible in Inspector and required
            Debug.Log("✅ Customer dependencies are now:");
            Debug.Log("   - CustomerMovement: Explicitly assigned in Inspector");
            Debug.Log("   - CustomerBehavior: Explicitly assigned in Inspector");
            Debug.Log("   - CustomerVisuals: Explicitly assigned in Inspector");
            
            // Verify dependencies are present
            bool hasMovement = demoCustomer.Movement != null;
            bool hasBehavior = demoCustomer.Behavior != null;
            bool hasVisuals = demoCustomer.Visuals != null;
            
            Debug.Log($"✅ Dependency Verification:");
            Debug.Log($"   Movement: {(hasMovement ? "✓ Present" : "✗ Missing")}");
            Debug.Log($"   Behavior: {(hasBehavior ? "✓ Present" : "✗ Missing")}");
            Debug.Log($"   Visuals: {(hasVisuals ? "✓ Present" : "✗ Missing")}");
        }
        
        /// <summary>
        /// Shows improved validation and error handling
        /// </summary>
        private void DemonstrateImprovedValidation()
        {
            Debug.Log("=== IMPROVED VALIDATION ===");
            
            // BEFORE: Components created silently, no validation
            // Errors only appeared at runtime when components failed
            
            // AFTER: Clear validation with helpful error messages
            Debug.Log("✅ Validation improvements:");
            Debug.Log("   - OnValidate() auto-assigns missing components in editor");
            Debug.Log("   - Clear error messages if dependencies are missing");
            Debug.Log("   - Fail-fast initialization prevents silent failures");
            Debug.Log("   - Dependencies visible in Inspector for verification");
            
            // Demonstrate validation behavior
            if (demoCustomer.Movement != null && demoCustomer.Behavior != null && demoCustomer.Visuals != null)
            {
                Debug.Log("✅ All dependencies validated and ready for use");
            }
            else
            {
                Debug.LogWarning("⚠️ Some dependencies missing - Customer initialization may fail");
            }
        }
        
        /// <summary>
        /// Shows how the new pattern improves testability
        /// </summary>
        private void DemonstrateTestability()
        {
            Debug.Log("=== TESTABILITY IMPROVEMENTS ===");
            
            // BEFORE: Hard to test due to hidden AddComponent() calls
            // Components were tightly coupled and created automatically
            
            // AFTER: Easy to test with mockable dependencies
            Debug.Log("✅ Testing benefits:");
            Debug.Log("   - Dependencies can be mocked for unit tests");
            Debug.Log("   - No hidden component creation during tests");
            Debug.Log("   - Clear separation between Customer and its dependencies");
            Debug.Log("   - Easy to test Customer logic in isolation");
            
            // Example of how testing would work now:
            Debug.Log("✅ Example test scenario:");
            Debug.Log("   1. Create mock CustomerMovement, CustomerBehavior, CustomerVisuals");
            Debug.Log("   2. Inject mocks into Customer via inspector or setup method");
            Debug.Log("   3. Test Customer coordination logic without real component behavior");
            Debug.Log("   4. Verify Customer calls correct methods on dependencies");
        }
        
        /// <summary>
        /// Shows Inspector and workflow benefits
        /// </summary>
        private void DemonstrateInspectorBenefits()
        {
            Debug.Log("=== INSPECTOR & WORKFLOW BENEFITS ===");
            
            // BEFORE: Empty Customer component in Inspector
            // No way to see or modify dependencies
            
            // AFTER: Clear dependency section in Inspector
            Debug.Log("✅ Inspector improvements:");
            Debug.Log("   - 'Required Components' section shows all dependencies");
            Debug.Log("   - Dependencies are drag-and-drop assignable");
            Debug.Log("   - OnValidate auto-fills missing components");
            Debug.Log("   - Visual feedback when dependencies are missing");
            
            // Workflow benefits
            Debug.Log("✅ Workflow improvements:");
            Debug.Log("   - Setup Customer GameObject with all components");
            Debug.Log("   - OnValidate automatically wires up references");
            Debug.Log("   - Dependencies clearly visible for debugging");
            Debug.Log("   - Easy to swap out components for different behaviors");
        }
        
        /// <summary>
        /// Demonstrates advanced usage scenarios enabled by dependency injection
        /// </summary>
        private void DemonstrateAdvancedUsage()
        {
            Debug.Log("=== ADVANCED USAGE SCENARIOS ===");
            
            // Component swapping for different behaviors
            Debug.Log("✅ Component Swapping:");
            Debug.Log("   - Easy to create different Customer variants");
            Debug.Log("   - Swap CustomerBehavior for AggressiveCustomer, CautiousCustomer, etc.");
            Debug.Log("   - Mix and match components for different behaviors");
            
            // Polymorphism support
            Debug.Log("✅ Polymorphism Support:");
            Debug.Log("   - Components can implement interfaces");
            Debug.Log("   - Customer works with any ICustomerMovement implementation");
            Debug.Log("   - Easy to extend without modifying Customer class");
            
            // Configuration flexibility
            Debug.Log("✅ Configuration Flexibility:");
            Debug.Log("   - Each component configured independently");
            Debug.Log("   - Customer focuses purely on coordination");
            Debug.Log("   - Clear separation of concerns");
        }
        
        void Update()
        {
            // Demonstrate runtime benefits - no component creation overhead
            if (Time.frameCount % 60 == 0) // Every second
            {
                if (demoCustomer != null)
                {
                    // Fast access to dependencies - no GetComponent() calls
                    bool isMoving = demoCustomer.Movement?.IsMoving ?? false;
                    float shoppingTime = demoCustomer.Behavior?.ShoppingTime ?? 0f;
                    
                    Debug.Log($"Runtime Check - Dependencies directly accessible: Moving={isMoving}, ShoppingTime={shoppingTime:F1}s");
                }
            }
        }
    }
}
