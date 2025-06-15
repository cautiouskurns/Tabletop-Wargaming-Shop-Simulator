using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TabletopShop
{
    /// <summary>
    /// Simple checkout counter that manages products and customer transactions
    /// </summary>
    public class CheckoutCounter : MonoBehaviour, IInteractable
    {
        [Header("Checkout Configuration")]
        [SerializeField] private Transform checkoutArea;
        [SerializeField] private float productSpacing = 0.5f;
        [SerializeField] private Transform queueStartPoint;
        [SerializeField] private float queueSpacing = 2f;
        
        [Header("UI Settings")]
        [SerializeField] private bool showUIOnStart = true; // For testing
        
        [Header("Checkout State")]
        [SerializeField] private List<Product> productsAtCheckout = new List<Product>();
        [SerializeField] private Customer currentCustomer;
        [SerializeField] private float runningTotal = 0f;
        [SerializeField] private Queue<Customer> customerQueue = new Queue<Customer>();
        [SerializeField] private bool isProcessingCustomer = false;
        
        // UI reference
        private CheckoutUI checkoutUI;
        
        // Properties for checkout state
        public bool HasCustomer => currentCustomer != null;
        public bool HasProducts => productsAtCheckout.Count > 0;
        public bool AllProductsScanned => HasProducts && productsAtCheckout.All(p => p.IsScannedAtCheckout);
        public bool IsOccupied => isProcessingCustomer || HasCustomer;
        public int QueueLength => customerQueue.Count;
        
        // IInteractable properties
        public string InteractionText => GetInteractionText();
        public bool CanInteract => true;

        #region Unity Lifecycle

        private void Awake()
        {
            // Set up checkout area if not assigned
            if (checkoutArea == null)
            {
                checkoutArea = transform;
            }
            
            // Set up queue start point if not assigned
            if (queueStartPoint == null)
            {
                // Create a queue start point behind the checkout area
                GameObject queueGO = new GameObject("QueueStartPoint");
                queueGO.transform.SetParent(transform);
                queueGO.transform.localPosition = Vector3.back * 3f; // 3 units behind checkout
                queueStartPoint = queueGO.transform;
                Debug.Log("CheckoutCounter: Created default queue start point");
            }
            
            // Find existing CheckoutUI in the scene
            checkoutUI = FindFirstObjectByType<CheckoutUI>();
            
            if (checkoutUI != null)
            {
                checkoutUI.TrackCheckoutCounter(this);
                Debug.Log($"CheckoutCounter: Found and connected to CheckoutUI");
                
                if (showUIOnStart)
                {
                    checkoutUI.Show();
                    checkoutUI.UpdateCustomer("Test Customer");
                    checkoutUI.UpdateTotal(0f);
                }
            }
            else
            {
                Debug.LogWarning("CheckoutCounter: No CheckoutUI found in scene - UI will not work");
            }
        }
            
        #endregion
        
        #region Product Management
        
        /// <summary>
        /// Place a product at the checkout counter
        /// </summary>
        /// <param name="product">The product to place at checkout</param>
        public void PlaceProduct(Product product)
        {
            if (product == null)
            {
                Debug.LogWarning("Cannot place null product at checkout");
                return;
            }
            
            if (productsAtCheckout.Contains(product))
            {
                Debug.LogWarning($"Product {product.ProductData?.ProductName ?? product.name} is already at checkout");
                return;
            }
            
            // Add to checkout list
            productsAtCheckout.Add(product);
            
            // Move product to checkout position
            Vector3 newPosition = GetNextProductPosition();
            product.transform.position = newPosition;
            product.transform.SetParent(checkoutArea);
            
            // Ensure the product stays stationary by disabling movement components
            EnsureProductStationary(product);
            
            // Update UI
            if (checkoutUI != null)
            {
                checkoutUI.AddProduct(product);
                checkoutUI.Show(); // Make sure UI is visible when products are added
                Debug.Log("CheckoutCounter: Showing UI because product was placed");
            }
            else
            {
                Debug.LogWarning("CheckoutCounter: No CheckoutUI found - cannot update UI");
            }
            
            Debug.Log($"Placed {product.ProductData?.ProductName ?? product.name} at checkout (${product.CurrentPrice}) and made stationary");
        }
        
        /// <summary>
        /// Ensure a product remains stationary after being placed on the counter
        /// </summary>
        /// <param name="product">The product to make stationary</param>
        private void EnsureProductStationary(Product product)
        {
            if (product == null) return;
            
            // Disable NavMeshAgent if present
            UnityEngine.AI.NavMeshAgent navAgent = product.GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (navAgent != null)
            {
                navAgent.enabled = false;
                Debug.Log($"CheckoutCounter: Disabled NavMeshAgent on {product.name}");
            }
            
            // Make Rigidbody kinematic if present
            Rigidbody rb = product.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true;
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                Debug.Log($"CheckoutCounter: Set Rigidbody to kinematic on {product.name}");
            }
            
            // Disable any movement scripts that might interfere
            MonoBehaviour[] scripts = product.GetComponents<MonoBehaviour>();
            foreach (MonoBehaviour script in scripts)
            {
                string scriptName = script.GetType().Name.ToLower();
                if (scriptName.Contains("movement") || scriptName.Contains("mover") || scriptName.Contains("follow"))
                {
                    script.enabled = false;
                    Debug.Log($"CheckoutCounter: Disabled movement script {script.GetType().Name} on {product.name}");
                }
            }
        }
        
        /// <summary>
        /// Scan a product at checkout
        /// </summary>
        /// <param name="product">The product to scan</param>
        public void ScanProduct(Product product)
        {
            if (product == null)
            {
                Debug.LogWarning("Cannot scan null product");
                return;
            }
            
            if (!productsAtCheckout.Contains(product))
            {
                Debug.LogWarning($"Product {product.ProductData?.ProductName ?? product.name} is not at checkout - cannot scan");
                return;
            }
            
            if (product.IsScannedAtCheckout)
            {
                Debug.LogWarning($"Product {product.ProductData?.ProductName ?? product.name} is already scanned");
                return;
            }
            
            // Scan the product
            product.ScanAtCheckout();
            
            // Add to running total
            runningTotal += product.CurrentPrice;
            
            // Update UI
            if (checkoutUI != null)
            {
                checkoutUI.UpdateProductScanStatus(product);
                checkoutUI.UpdateTotal(runningTotal);
            }
            
            // Play scan sound
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayProductScanBeep();
            }
            
            Debug.Log($"Scanned {product.ProductData?.ProductName ?? product.name} - Running total: ${runningTotal:F2}");
        }
        
        /// <summary>
        /// Get the next position for placing a product
        /// </summary>
        /// <returns>World position for the next product</returns>
        private Vector3 GetNextProductPosition()
        {
            Vector3 basePosition = checkoutArea.position;
            // Place products on the counter surface (slightly elevated)
            Vector3 surfaceOffset = checkoutArea.up * 0.1f; // 10cm above the counter
            float horizontalOffset = productsAtCheckout.Count * productSpacing;
            return basePosition + surfaceOffset + checkoutArea.right * horizontalOffset;
        }
        
        #endregion
        
        #region Payment Processing
        
        /// <summary>
        /// Process payment when all products are scanned
        /// </summary>
        public void ProcessPayment()
        {
            if (!HasCustomer)
            {
                Debug.LogWarning("Cannot process payment - no customer at checkout");
                return;
            }
            
            if (!HasProducts)
            {
                Debug.LogWarning("Cannot process payment - no products at checkout");
                return;
            }
            
            if (!AllProductsScanned)
            {
                Debug.LogWarning("Cannot process payment - not all products are scanned");
                return;
            }
            
            // Process the purchase through GameManager
            if (GameManager.Instance != null)
            {
                // Calculate customer satisfaction (could be enhanced based on wait time, service quality, etc.)
                float customerSatisfaction = 0.8f; // Default satisfaction level
                
                GameManager.Instance.ProcessCustomerPurchase(runningTotal, customerSatisfaction);
            }
            else
            {
                Debug.LogError("GameManager.Instance is null - cannot process purchase");
                return;
            }
            
            Debug.Log($"Processed payment of ${runningTotal:F2} for {productsAtCheckout.Count} products");
            
            // Notify customer that checkout is completed
            if (currentCustomer != null)
            {
                // Get the CustomerBehavior component and notify completion
                CustomerBehavior customerBehavior = currentCustomer.GetComponent<CustomerBehavior>();
                if (customerBehavior != null)
                {
                    customerBehavior.OnCheckoutCompleted();
                    Debug.Log($"Notified customer {currentCustomer.name} that checkout is completed");
                }
                else
                {
                    Debug.LogWarning($"CustomerBehavior component not found on customer {currentCustomer.name}");
                }
            }
            
            // Note: Don't clear checkout here - let customer departure handle it
        }
        
        /// <summary>
        /// Clear all products from checkout
        /// </summary>
        private void ClearCheckout()
        {
            foreach (Product product in productsAtCheckout)
            {
                if (product != null)
                {
                    product.ResetScanState();
                }
            }
            
            productsAtCheckout.Clear();
            runningTotal = 0f;
            
            // Update UI
            if (checkoutUI != null)
            {
                checkoutUI.ClearProducts();
                checkoutUI.UpdateTotal(0f);
            }
            
            Debug.Log("Cleared checkout counter");
        }
        
        #endregion
        
        #region Customer Management
        
        /// <summary>
        /// Handle customer arrival at checkout
        /// </summary>
        /// <param name="customer">The customer arriving at checkout</param>
        public void OnCustomerArrival(Customer customer)
        {
            if (customer == null)
            {
                Debug.LogWarning("Cannot handle arrival of null customer");
                return;
            }
            
            if (HasCustomer)
            {
                Debug.LogWarning($"Customer {currentCustomer.name} is already at checkout - cannot add {customer.name}");
                return;
            }
            
            currentCustomer = customer;
            
            // Update UI
            if (checkoutUI != null)
            {
                checkoutUI.UpdateCustomer(customer.name);
                checkoutUI.Show();
                Debug.Log("CheckoutCounter: Showing UI because customer arrived");
            }
            else
            {
                Debug.LogWarning("CheckoutCounter: No CheckoutUI found - cannot update UI");
            }
            
            Debug.Log($"Customer {customer.name} arrived at checkout");
        }
        
        /// <summary>
        /// Handle customer departure from checkout
        /// </summary>
        public void OnCustomerDeparture()
        {
            if (!HasCustomer)
            {
                Debug.LogWarning("No customer to handle departure");
                return;
            }
            
            Debug.Log($"Customer {currentCustomer.name} departed from checkout");
            currentCustomer = null;
            
            // Clear any remaining products if customer leaves
            if (HasProducts)
            {
                ClearCheckout();
            }
            
            // Update UI
            if (checkoutUI != null)
            {
                checkoutUI.UpdateCustomer("");
                checkoutUI.ClearProducts();
                checkoutUI.UpdateTotal(0f);
                
                // Hide UI when no activity
                if (!showUIOnStart)
                {
                    checkoutUI.Hide();
                }
            }
        }
        
        #endregion
        
        #region IInteractable Implementation
        
        /// <summary>
        /// Handle player interaction with checkout counter
        /// </summary>
        /// <param name="player">The player GameObject</param>
        public void Interact(GameObject player)
        {
            if (!HasCustomer)
            {
                Debug.Log("No customer at checkout - nothing to do");
                return;
            }
            
            if (!HasProducts)
            {
                Debug.Log("No products at checkout - waiting for products");
                return;
            }
            
            if (AllProductsScanned)
            {
                ProcessPayment();
            }
            else
            {
                // Try to scan the next unscanned product
                Product nextProduct = productsAtCheckout.FirstOrDefault(p => !p.IsScannedAtCheckout);
                if (nextProduct != null)
                {
                    ScanProduct(nextProduct);
                }
            }
        }
        
        /// <summary>
        /// Called when player starts looking at checkout counter
        /// </summary>
        public void OnInteractionEnter()
        {
            // Visual feedback could be added here
        }
        
        /// <summary>
        /// Called when player stops looking at checkout counter
        /// </summary>
        public void OnInteractionExit()
        {
            // Visual feedback could be added here
        }
        
        /// <summary>
        /// Get appropriate interaction text based on checkout state
        /// </summary>
        /// <returns>Interaction text to display</returns>
        private string GetInteractionText()
        {
            if (!HasCustomer)
            {
                return "Checkout Counter (No Customer)";
            }
            
            if (!HasProducts)
            {
                return "Waiting for Products";
            }
            
            if (AllProductsScanned)
            {
                return $"Process Payment (${runningTotal:F2})";
            }
            
            int scannedCount = productsAtCheckout.Count(p => p.IsScannedAtCheckout);
            return $"Scan Products ({scannedCount}/{productsAtCheckout.Count})";
        }
        
        #endregion
        
        #region Debug & Testing
        
        /// <summary>
        /// Test method for development
        /// </summary>
        [ContextMenu("Test Checkout System")]
        public void TestCheckoutSystem()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("Checkout test requires Play mode");
                return;
            }
            
            Debug.Log("=== CHECKOUT COUNTER STATUS ===");
            Debug.Log($"Has Customer: {HasCustomer}");
            Debug.Log($"Has Products: {HasProducts}");
            Debug.Log($"All Scanned: {AllProductsScanned}");
            Debug.Log($"Running Total: ${runningTotal:F2}");
            Debug.Log($"Products Count: {productsAtCheckout.Count}");
            Debug.Log($"Interaction Text: {InteractionText}");
            Debug.Log($"CheckoutUI Present: {checkoutUI != null}");
            
            if (checkoutUI != null)
            {
                checkoutUI.Show();
                Debug.Log("Forced CheckoutUI to show");
            }
        }
        
        /// <summary>
        /// Toggle the checkout UI visibility (for testing)
        /// </summary>
        [ContextMenu("Toggle Checkout UI")]
        public void ToggleCheckoutUI()
        {
            if (checkoutUI != null)
            {
                checkoutUI.Toggle();
                Debug.Log("Toggled checkout UI visibility");
            }
            else
            {
                Debug.LogWarning("No checkout UI found");
            }
        }
        
        #endregion
    }
}
