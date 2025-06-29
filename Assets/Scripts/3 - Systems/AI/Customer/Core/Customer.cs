using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

namespace TabletopShop
{
    /// <summary>
    /// Customer state enumeration for managing customer behavior
    /// </summary>
    public enum CustomerState
    {
        Entering,    // Customer is entering the shop
        Shopping,    // Customer is browsing/looking at products
        Purchasing,  // Customer is at checkout/purchase area
        Leaving      // Customer is exiting the shop
    }

    /// <summary>
    /// Customer AI coordinator using pure composition pattern.
    /// 
    /// DESIGN PATTERN: Pure Coordinator
    /// - Provides direct access to specialized components
    /// - Coordinates high-level actions that involve multiple components
    /// - NO delegation methods - use direct component access instead
    /// - Implements ICustomer interface for enhanced interface segregation
    /// - Uses Unity's NavMeshAgent for movement
    /// </summary>

    [RequireComponent(typeof(NavMeshAgent))]
    public class Customer : MonoBehaviour, ICustomer
    {
        [Header("Required Components")]
        [SerializeField] private CustomerMovement customerMovement;
        [SerializeField] private CustomerBehavior customerBehavior;
        [SerializeField] private CustomerVisuals customerVisuals;
        
        [Header("Behavior Designer Compatibility")]
        [SerializeField] private float startingMoney = 1000f;
        [SerializeField] private int maxProducts = 3;

        [Header("Debug")]
        public bool showDebugLogs = true;
        
        // Behavior Designer compatibility properties
        private float _currentMoney;
        private List<Product> _selectedProducts = new List<Product>();
        private float _shoppingStartTime;
        
        // Component access properties - expose components directly for better flexibility
        public CustomerMovement Movement => customerMovement;
        public CustomerBehavior Behavior => customerBehavior;
        public CustomerVisuals Visuals => customerVisuals;
        
        // Interface implementations for ICustomer
        ICustomerMovement ICustomer.Movement => customerMovement;
        ICustomerBehavior ICustomer.Behavior => customerBehavior;
        ICustomerVisuals ICustomer.Visuals => customerVisuals;
        
        // Convenience properties (delegated from components) for ICustomer interface
        public CustomerState CurrentState => customerBehavior?.CurrentState ?? CustomerState.Entering;
        public float ShoppingTime => customerBehavior?.ShoppingTime ?? 15f;
        public ShelfSlot TargetShelf => customerBehavior?.TargetShelf;
        public bool IsMoving => customerMovement?.IsMoving ?? false;
        public Vector3 CurrentDestination => customerMovement?.CurrentDestination ?? Vector3.zero;
        public bool HasDestination => customerMovement?.HasDestination ?? false;
        
        // Behavior Designer compatibility properties
        public float currentMoney 
        { 
            get => _currentMoney; 
            set => _currentMoney = value; 
        }
        
        public List<Product> selectedProducts 
        { 
            get => _selectedProducts; 
            set => _selectedProducts = value ?? new List<Product>(); 
        }
        
        public ShelfSlot currentTargetShelf 
        { 
            get => TargetShelf; 
            set => SetTargetShelf(value); 
        }
        
        public Vector3 spawnPosition { get; private set; }
        public float shoppingStartTime => _shoppingStartTime;
        
        // Configuration properties for Behavior Designer
        public float StartingMoney => startingMoney;
        public int MaxProducts => maxProducts;
        
        // Note: Legacy properties removed - use direct component access instead:
        // customer.Behavior.CurrentState, customer.Movement.IsMoving, etc.

        #region Unity Lifecycle
        
        private void Awake()
        {
            try
            {
                Debug.Log($"Customer {name}: Awake() START");
                
                // Initialize Behavior Designer compatibility
                _currentMoney = startingMoney;
                spawnPosition = transform.position;
                
                InitializeComponents();
                
                Debug.Log($"Customer {name}: Awake() SUCCESS");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Customer {name}: CRASH in Awake(): {ex.Message}\nStack: {ex.StackTrace}");
            }
        }

        private void Start()
        {
            try
            {
                Debug.Log($"Customer {name}: Start() BEGIN");
                
                var currentState = customerBehavior?.CurrentState ?? CustomerState.Entering;
                var shoppingTime = customerBehavior?.ShoppingTime ?? 15f;
                
                Debug.Log($"Customer {name} initialized with state: {currentState}, shopping time: {shoppingTime:F1}s");
                
                if (customerBehavior != null)
                {
                    customerBehavior.StartCustomerLifecycle(currentState);
                }
                else
                {
                    Debug.LogError($"Customer {name} cannot start lifecycle - CustomerBehavior component not found!");
                }
                
                Debug.Log($"Customer {name}: Start() SUCCESS");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Customer {name}: CRASH in Start(): {ex.Message}\nStack: {ex.StackTrace}");
            }
        }
        
        #endregion
        
        #region Component Initialization
        
        /// <summary>
        /// Initialize and setup components using dependency injection
        /// </summary>
        private void InitializeComponents()
        {
            // Validate required dependencies
            if (customerMovement == null || customerBehavior == null || customerVisuals == null)
            {
                Debug.LogError($"Customer {name} is missing required components! Use OnValidate to auto-assign or manually assign in inspector.");
                return;
            }
            
            // Initialize cross-component references
            customerMovement.Initialize();
            customerBehavior.Initialize(customerMovement, this);
            customerVisuals.Initialize(customerMovement, this);
            
            // Subscribe to component events for coordination
            customerBehavior.OnTargetShelfChanged += HandleTargetShelfChanged;
            
            Debug.Log($"Customer {name} components initialized successfully");
        }
        
        /// <summary>
        /// Handle target shelf changes from behavior component
        /// </summary>
        private void HandleTargetShelfChanged(ShelfSlot shelf)
        {
            // Target shelf is now managed by the behavior component
            // This event handler can be used for additional coordination if needed
        }
        
        #endregion
        
        #region Behavior Designer Helper Methods
        
        /// <summary>
        /// Start shopping timer for Behavior Designer tasks
        /// </summary>
        public void StartShoppingTimer()
        {
            _shoppingStartTime = Time.time;
            if (showDebugLogs)
                Debug.Log($"Customer {name}: Shopping timer started at {_shoppingStartTime:F1}s");
        }
        
        /// <summary>
        /// Add product to selected products and deduct money (Behavior Designer compatibility)
        /// </summary>
        public void AddProduct(Product product)
        {
            if (product == null)
            {
                Debug.LogWarning($"Customer {name}: Tried to add null product");
                return;
            }
            
            _selectedProducts.Add(product);
            _currentMoney -= product.CurrentPrice;
            
            if (showDebugLogs)
                Debug.Log($"Customer {name}: Added {product.ProductData?.ProductName ?? "Unknown"} for ${product.CurrentPrice:F2}. Money: ${_currentMoney:F2}");
        }
        
        /// <summary>
        /// Cleanup selected products on destroy (Behavior Designer compatibility)
        /// </summary>
        public void CleanupOnDestroy()
        {
            if (showDebugLogs)
                Debug.Log($"Customer {name}: Cleanup - Selected {_selectedProducts.Count} products, Spent ${startingMoney - _currentMoney:F2}");
            
            foreach (Product product in _selectedProducts)
            {
                if (product != null && !product.IsPurchased)
                    Destroy(product.gameObject);
            }
            
            _selectedProducts.Clear();
        }
        
        #endregion
        
        // Note: State management methods removed - use direct component access:
        // customer.Behavior.ChangeState(state), customer.Behavior.CurrentState, etc.
        
        #region High-Level Customer Actions
        
        /// <summary>
        /// Start shopping behavior - high-level action that coordinates components
        /// </summary>
        public void StartShopping()
        {
            // Direct component coordination - no delegation
            if (customerBehavior != null)
            {
                customerBehavior.ChangeState(CustomerState.Shopping);
            }
            
            if (customerMovement != null)
            {
                customerMovement.SetRandomShelfDestination();
            }
            else
            {
                Debug.LogError($"Customer {name} cannot start shopping - Movement component not available!");
            }
        }
        
        /// <summary>
        /// Start purchasing behavior - high-level action for checkout
        /// </summary>
        public void StartPurchasing()
        {
            // Direct component coordination - no delegation
            if (customerBehavior != null)
            {
                customerBehavior.ChangeState(CustomerState.Purchasing);
            }
            
            if (customerMovement != null)
            {
                customerMovement.MoveToCheckoutPoint();
            }
            else
            {
                Debug.LogError($"Customer {name} cannot start purchasing - Movement component not available!");
            }
        }
        
        /// <summary>
        /// Start leaving behavior - high-level action for exiting
        /// </summary>
        public void StartLeaving()
        {
            // Direct component coordination - no delegation
            if (customerBehavior != null)
            {
                customerBehavior.ChangeState(CustomerState.Leaving);
            }
            
            if (customerMovement != null)
            {
                customerMovement.MoveToExitPoint();
            }
            else
            {
                Debug.LogError($"Customer {name} cannot start leaving - Movement component not available!");
            }
        }
        
        #endregion
        
        #region ICustomer Interface Implementation
        
        /// <summary>
        /// Change customer state (interface delegation)
        /// </summary>
        public void ChangeState(CustomerState newState)
        {
            customerBehavior?.ChangeState(newState);
        }
        
        /// <summary>
        /// Check if customer is in specific state (interface delegation)
        /// </summary>
        public bool IsInState(CustomerState state)
        {
            return customerBehavior?.IsInState(state) ?? false;
        }
        
        /// <summary>
        /// Set target shelf (interface delegation)
        /// </summary>
        public void SetTargetShelf(ShelfSlot shelf)
        {
            customerBehavior?.SetTargetShelf(shelf);
        }
        
        /// <summary>
        /// Clear target shelf (interface delegation)
        /// </summary>
        public void ClearTargetShelf()
        {
            customerBehavior?.ClearTargetShelf();
        }
        
        /// <summary>
        /// Get debug information (interface delegation)
        /// </summary>
        public string GetDebugInfo()
        {
            return customerBehavior?.GetDebugInfo() ?? $"Customer {name}: No behavior component";
        }
        
        #endregion


        #region Unity Editor Support
        
        private void OnValidate()
        {
            // Validate dependencies are assigned and auto-assign if missing
            if (!customerMovement) customerMovement = GetComponent<CustomerMovement>();
            if (!customerBehavior) customerBehavior = GetComponent<CustomerBehavior>();
            if (!customerVisuals) customerVisuals = GetComponent<CustomerVisuals>();
            
            // Warn if dependencies are still missing after auto-assignment attempt
            if (!customerMovement) Debug.LogWarning($"Customer {name} is missing CustomerMovement component!");
            if (!customerBehavior) Debug.LogWarning($"Customer {name} is missing CustomerBehavior component!");
            if (!customerVisuals) Debug.LogWarning($"Customer {name} is missing CustomerVisuals component!");
        }
        
        #endregion
    }
}
