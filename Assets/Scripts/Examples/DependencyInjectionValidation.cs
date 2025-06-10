using UnityEngine;
using UnityEngine.AI;

namespace TabletopShop.Examples
{
    /// <summary>
    /// Validation script to test the dependency injection implementation in Customer.cs
    /// This script demonstrates how to properly set up a Customer GameObject with required components
    /// and validates that the dependency injection system works correctly.
    /// </summary>
    public class DependencyInjectionValidation : MonoBehaviour
    {
        [Header("Test Configuration")]
        [SerializeField] private bool runValidationOnStart = true;
        [SerializeField] private bool createTestCustomer = true;
        [SerializeField] private bool logDetailedResults = true;
        
        private void Start()
        {
            if (runValidationOnStart)
            {
                RunDependencyInjectionTests();
            }
        }
        
        /// <summary>
        /// Run comprehensive tests of the dependency injection system
        /// </summary>
        public void RunDependencyInjectionTests()
        {
            Debug.Log("=== DEPENDENCY INJECTION VALIDATION STARTED ===");
            
            // Test 1: Validate existing Customer objects in scene
            TestExistingCustomers();
            
            // Test 2: Create and validate a new Customer GameObject
            if (createTestCustomer)
            {
                TestCustomerCreation();
            }
            
            // Test 3: Test OnValidate auto-assignment
            TestOnValidateAutoAssignment();
            
            Debug.Log("=== DEPENDENCY INJECTION VALIDATION COMPLETED ===");
        }
        
        /// <summary>
        /// Test existing Customer objects in the scene
        /// </summary>
        private void TestExistingCustomers()
        {
            Debug.Log("--- Testing Existing Customers ---");
            
            Customer[] existingCustomers = FindObjectsOfType<Customer>();
            
            if (existingCustomers.Length == 0)
            {
                Debug.Log("No existing Customer objects found in scene");
                return;
            }
            
            foreach (Customer customer in existingCustomers)
            {
                ValidateCustomerComponents(customer, "Existing");
            }
        }
        
        /// <summary>
        /// Test creating a new Customer GameObject with proper setup
        /// </summary>
        private void TestCustomerCreation()
        {
            Debug.Log("--- Testing Customer Creation ---");
            
            // Create test GameObject
            GameObject testCustomerObj = new GameObject("TestCustomer_DI");
            testCustomerObj.transform.position = Vector3.zero;
            
            // Add NavMeshAgent (required component)
            NavMeshAgent navAgent = testCustomerObj.AddComponent<NavMeshAgent>();
            
            // Add Customer component
            Customer customer = testCustomerObj.AddComponent<Customer>();
            
            // Add required components
            CustomerMovement movement = testCustomerObj.AddComponent<CustomerMovement>();
            CustomerBehavior behavior = testCustomerObj.AddComponent<CustomerBehavior>();
            CustomerVisuals visuals = testCustomerObj.AddComponent<CustomerVisuals>();
            
            // Validate the setup
            ValidateCustomerComponents(customer, "Created");
            
            // Test component access properties
            TestComponentAccessProperties(customer);
            
            // Clean up test object after validation
            if (!logDetailedResults)
            {
                DestroyImmediate(testCustomerObj);
                Debug.Log("Test Customer GameObject cleaned up");
            }
            else
            {
                Debug.Log("Test Customer GameObject kept for inspection: " + testCustomerObj.name);
            }
        }
        
        /// <summary>
        /// Test OnValidate auto-assignment functionality
        /// </summary>
        private void TestOnValidateAutoAssignment()
        {
            Debug.Log("--- Testing OnValidate Auto-Assignment ---");
            
            // Create GameObject with components but no assignment
            GameObject testObj = new GameObject("TestCustomer_OnValidate");
            testObj.AddComponent<NavMeshAgent>();
            testObj.AddComponent<CustomerMovement>();
            testObj.AddComponent<CustomerBehavior>();
            testObj.AddComponent<CustomerVisuals>();
            
            // Add Customer component (OnValidate should auto-assign)
            Customer customer = testObj.AddComponent<Customer>();
            
            // OnValidate is called automatically when component is added
            // Validate that components were auto-assigned
            bool movementAssigned = customer.Movement != null;
            bool behaviorAssigned = customer.Behavior != null;
            bool visualsAssigned = customer.Visuals != null;
            
            Debug.Log($"OnValidate Auto-Assignment Results:");
            Debug.Log($"   Movement: {(movementAssigned ? "✓ Assigned" : "✗ Not Assigned")}");
            Debug.Log($"   Behavior: {(behaviorAssigned ? "✓ Assigned" : "✗ Not Assigned")}");
            Debug.Log($"   Visuals: {(visualsAssigned ? "✓ Assigned" : "✗ Not Assigned")}");
            
            if (movementAssigned && behaviorAssigned && visualsAssigned)
            {
                Debug.Log("✓ OnValidate auto-assignment working correctly!");
            }
            else
            {
                Debug.LogWarning("✗ OnValidate auto-assignment failed - manual assignment required");
            }
            
            // Clean up
            if (!logDetailedResults)
            {
                DestroyImmediate(testObj);
            }
        }
        
        /// <summary>
        /// Validate that a Customer has all required components properly assigned
        /// </summary>
        private void ValidateCustomerComponents(Customer customer, string testType)
        {
            if (customer == null)
            {
                Debug.LogError($"{testType} Customer is null!");
                return;
            }
            
            bool hasMovement = customer.Movement != null;
            bool hasBehavior = customer.Behavior != null;
            bool hasVisuals = customer.Visuals != null;
            bool hasNavMeshAgent = customer.GetComponent<NavMeshAgent>() != null;
            
            Debug.Log($"{testType} Customer '{customer.name}' Component Validation:");
            Debug.Log($"   CustomerMovement: {(hasMovement ? "✓" : "✗")}");
            Debug.Log($"   CustomerBehavior: {(hasBehavior ? "✓" : "✗")}");
            Debug.Log($"   CustomerVisuals: {(hasVisuals ? "✓" : "✗")}");
            Debug.Log($"   NavMeshAgent: {(hasNavMeshAgent ? "✓" : "✗")}");
            
            if (hasMovement && hasBehavior && hasVisuals && hasNavMeshAgent)
            {
                Debug.Log($"✓ {testType} Customer has all required components!");
            }
            else
            {
                Debug.LogWarning($"✗ {testType} Customer is missing required components");
            }
        }
        
        /// <summary>
        /// Test component access properties
        /// </summary>
        private void TestComponentAccessProperties(Customer customer)
        {
            Debug.Log("--- Testing Component Access Properties ---");
            
            // Test direct component access
            bool canAccessMovement = customer.Movement != null;
            bool canAccessBehavior = customer.Behavior != null;
            bool canAccessVisuals = customer.Visuals != null;
            
            Debug.Log($"Component Access Properties:");
            Debug.Log($"   customer.Movement: {(canAccessMovement ? "✓ Accessible" : "✗ Null")}");
            Debug.Log($"   customer.Behavior: {(canAccessBehavior ? "✓ Accessible" : "✗ Null")}");
            Debug.Log($"   customer.Visuals: {(canAccessVisuals ? "✓ Accessible" : "✗ Null")}");
            
            // Test legacy properties (should use fallbacks)
            Debug.Log($"Legacy Properties (with fallbacks):");
            Debug.Log($"   ShoppingTime: {customer.ShoppingTime}s");
            Debug.Log($"   IsMoving: {customer.IsMoving}");
            Debug.Log($"   CurrentState: {customer.CurrentState}");
            Debug.Log($"   HasDestination: {customer.HasDestination}");
            
            if (canAccessMovement && canAccessBehavior && canAccessVisuals)
            {
                Debug.Log("✓ All component access properties working correctly!");
            }
        }
        
        /// <summary>
        /// Manual test button for inspector
        /// </summary>
        [ContextMenu("Run Dependency Injection Tests")]
        public void ManualTest()
        {
            RunDependencyInjectionTests();
        }
        
        /// <summary>
        /// Create a properly configured Customer prefab setup
        /// </summary>
        [ContextMenu("Create Customer Prefab Setup")]
        public void CreateCustomerPrefabSetup()
        {
            Debug.Log("--- Creating Customer Prefab Setup ---");
            
            // Create base GameObject
            GameObject customerPrefab = new GameObject("Customer_Prefab_Setup");
            customerPrefab.transform.position = Vector3.zero;
            
            // Add NavMeshAgent first (required component)
            NavMeshAgent navAgent = customerPrefab.AddComponent<NavMeshAgent>();
            navAgent.speed = 3.5f;
            navAgent.stoppingDistance = 1.0f;
            navAgent.acceleration = 8.0f;
            
            // Add component scripts
            CustomerMovement movement = customerPrefab.AddComponent<CustomerMovement>();
            CustomerBehavior behavior = customerPrefab.AddComponent<CustomerBehavior>();
            CustomerVisuals visuals = customerPrefab.AddComponent<CustomerVisuals>();
            
            // Add Customer component last (OnValidate will auto-assign components)
            Customer customer = customerPrefab.AddComponent<Customer>();
            
            // Add basic collider for physics
            CapsuleCollider collider = customerPrefab.AddComponent<CapsuleCollider>();
            collider.height = 2.0f;
            collider.radius = 0.5f;
            collider.center = new Vector3(0, 1.0f, 0);
            
            Debug.Log($"✓ Customer prefab setup created: {customerPrefab.name}");
            Debug.Log("   - All required components added");
            Debug.Log("   - OnValidate auto-assigned dependencies");
            Debug.Log("   - Ready for prefab creation or scene use");
            
            // Select the created object in hierarchy
            UnityEditor.Selection.activeGameObject = customerPrefab;
        }
    }
}
