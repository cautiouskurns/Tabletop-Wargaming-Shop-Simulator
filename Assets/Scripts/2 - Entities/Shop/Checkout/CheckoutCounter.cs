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
        
        [Header("Product Layout")]
        [SerializeField] private int maxProductsPerRow = 3;
        [SerializeField] private float rowSpacing = 0.4f;
        [SerializeField] private Vector3 checkoutAreaSize = new Vector3(1.5f, 0.1f, 1.0f); // Width, Height, Depth of checkout area
        [SerializeField] private bool arrangeInGrid = true;
        
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
            
            // Add slight random rotation for more natural look
            if (arrangeInGrid)
            {
                Vector3 randomRotation = new Vector3(
                    0f,
                    UnityEngine.Random.Range(-15f, 15f), // Small random Y rotation
                    0f
                );
                product.transform.rotation = checkoutArea.rotation * Quaternion.Euler(randomRotation);
            }
            
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
        /// Get the next position for placing a product using an orderly grid layout
        /// </summary>
        /// <returns>World position for the next product</returns>
        private Vector3 GetNextProductPosition()
        {
            if (arrangeInGrid)
            {
                return GetGridProductPosition();
            }
            else
            {
                return GetLinearProductPosition();
            }
        }
        
        /// <summary>
        /// Get product position using a grid layout (recommended for better organization)
        /// </summary>
        /// <returns>World position for the next product in grid layout</returns>
        private Vector3 GetGridProductPosition()
        {
            Vector3 basePosition = checkoutArea.position;
            Vector3 surfaceOffset = checkoutArea.up * 0.1f; // 10cm above the counter
            
            // Calculate grid position
            int productIndex = productsAtCheckout.Count;
            int row = productIndex / maxProductsPerRow;
            int col = productIndex % maxProductsPerRow;
            
            // Calculate offsets based on checkout area size
            float totalWidth = checkoutAreaSize.x;
            float totalDepth = checkoutAreaSize.z;
            
            // Center the grid within the checkout area
            float startX = -(totalWidth * 0.5f) + (totalWidth / (maxProductsPerRow + 1));
            float startZ = -(totalDepth * 0.5f) + (totalDepth * 0.25f);
            
            // Calculate spacing between products
            float xSpacing = totalWidth / (maxProductsPerRow + 1);
            float zSpacing = rowSpacing;
            
            // Calculate final position
            Vector3 localOffset = new Vector3(
                startX + (col * xSpacing),
                0f,
                startZ + (row * zSpacing)
            );
            
            // Transform local offset to world space using checkout area's orientation
            Vector3 worldOffset = checkoutArea.TransformDirection(localOffset);
            
            return basePosition + surfaceOffset + worldOffset;
        }
        
        /// <summary>
        /// Get product position using a linear layout (fallback method)
        /// </summary>
        /// <returns>World position for the next product in linear layout</returns>
        private Vector3 GetLinearProductPosition()
        {
            Vector3 basePosition = checkoutArea.position;
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
            
            // Mark all products as purchased and remove them from counter
            RemoveProductsFromCounter();
            
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
        
        /// <summary>
        /// Remove all products from the checkout counter after payment is processed
        /// Products are marked as purchased and visually removed from the counter
        /// </summary>
        private void RemoveProductsFromCounter()
        {
            List<Product> productsToRemove = new List<Product>(productsAtCheckout);
            
            foreach (Product product in productsToRemove)
            {
                if (product != null)
                {
                    // Mark product as purchased
                    product.Purchase();
                    
                    // Remove product from counter visually (disable or move off counter)
                    product.gameObject.SetActive(false);
                    
                    Debug.Log($"Removed and purchased product: {product.ProductData?.ProductName ?? product.name}");
                }
            }
            
            // Clear the checkout state
            ClearCheckout();
            
            Debug.Log($"Removed {productsToRemove.Count} products from checkout counter after payment");
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
        /// Now only handles payment processing - scanning is done by interacting with individual products
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
                // Inform player they need to scan individual products
                int scannedCount = productsAtCheckout.Count(p => p.IsScannedAtCheckout);
                Debug.Log($"Please scan individual products by looking at them. Scanned: {scannedCount}/{productsAtCheckout.Count}");
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
            return $"Scan Individual Products ({scannedCount}/{productsAtCheckout.Count})";
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
        
        /// <summary>
        /// Test the checkout area layout system
        /// </summary>
        [ContextMenu("Test Checkout Layout")]
        public void TestCheckoutLayout()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("Layout test requires Play mode");
                return;
            }
            
            Debug.Log("=== CHECKOUT LAYOUT TEST ===");
            Debug.Log($"Grid Layout Enabled: {arrangeInGrid}");
            Debug.Log($"Max Products Per Row: {maxProductsPerRow}");
            Debug.Log($"Checkout Area Size: {checkoutAreaSize}");
            Debug.Log($"Row Spacing: {rowSpacing}");
            Debug.Log($"Product Spacing: {productSpacing}");
            Debug.Log($"Current Products: {productsAtCheckout.Count}");
            
            // Show next few positions that would be used
            for (int i = productsAtCheckout.Count; i < productsAtCheckout.Count + 3; i++)
            {
                Vector3 pos = GetGridProductPositionAtIndex(i);
                Debug.Log($"Next position {i}: {pos}");
            }
        }
        
        /// <summary>
        /// Reorganize products manually (for testing)
        /// </summary>
        [ContextMenu("Reorganize Products")]
        public void ManualReorganizeProducts()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("Reorganize requires Play mode");
                return;
            }
            
            ReorganizeProducts();
        }
        
        /// <summary>
        /// Draw checkout area gizmos in Scene view for visual debugging
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            if (checkoutArea == null) return;
            
            // Draw checkout area bounds
            Gizmos.color = Color.yellow;
            Gizmos.matrix = checkoutArea.localToWorldMatrix;
            
            // Draw the checkout area as a wireframe box
            Vector3 center = Vector3.up * (checkoutAreaSize.y * 0.5f);
            Gizmos.DrawWireCube(center, checkoutAreaSize);
            
            // Draw the surface plane
            Gizmos.color = new Color(1f, 1f, 0f, 0.2f);
            Vector3 surfaceCenter = Vector3.up * 0.05f;
            Vector3 surfaceSize = new Vector3(checkoutAreaSize.x, 0.01f, checkoutAreaSize.z);
            Gizmos.DrawCube(surfaceCenter, surfaceSize);
            
            // Draw grid positions
            if (arrangeInGrid && Application.isPlaying)
            {
                DrawProductGridPositions();
            }
        }
        
        /// <summary>
        /// Draw visual markers for each grid position
        /// </summary>
        private void DrawProductGridPositions()
        {
            // Draw positions for current products
            Gizmos.color = Color.green;
            for (int i = 0; i < productsAtCheckout.Count; i++)
            {
                Vector3 gridPos = GetGridProductPositionAtIndex(i);
                Gizmos.DrawWireSphere(gridPos, 0.1f);
            }
            
            // Draw next few available positions
            Gizmos.color = Color.cyan;
            for (int i = productsAtCheckout.Count; i < productsAtCheckout.Count + 3; i++)
            {
                Vector3 gridPos = GetGridProductPositionAtIndex(i);
                Gizmos.DrawWireSphere(gridPos, 0.05f);
            }
        }
        
        #endregion
        
        #region Product Reorganization

        /// <summary>
        /// Reorganize all products in the checkout area to maintain orderly layout
        /// Call this after removing products to fill gaps
        /// </summary>
        public void ReorganizeProducts()
        {
            if (!arrangeInGrid || productsAtCheckout.Count == 0)
                return;
            
            for (int i = 0; i < productsAtCheckout.Count; i++)
            {
                Product product = productsAtCheckout[i];
                if (product != null)
                {
                    // Temporarily store the current count and calculate position for index i
                    int originalCount = productsAtCheckout.Count;
                    productsAtCheckout.RemoveAt(i);
                    
                    // Calculate position as if this product is at index i
                    Vector3 targetPosition = GetGridProductPositionAtIndex(i);
                    
                    // Restore the list
                    productsAtCheckout.Insert(i, product);
                    
                    // Smoothly move product to new position if it's significantly different
                    if (Vector3.Distance(product.transform.position, targetPosition) > 0.1f)
                    {
                        StartCoroutine(SmoothMoveProduct(product, targetPosition));
                    }
                }
            }
            
            Debug.Log($"Reorganized {productsAtCheckout.Count} products in checkout area");
        }
        
        /// <summary>
        /// Get the grid position for a product at a specific index
        /// </summary>
        /// <param name="index">Index of the product in the list</param>
        /// <returns>World position for the product at the given index</returns>
        private Vector3 GetGridProductPositionAtIndex(int index)
        {
            Vector3 basePosition = checkoutArea.position;
            Vector3 surfaceOffset = checkoutArea.up * 0.1f;
            
            int row = index / maxProductsPerRow;
            int col = index % maxProductsPerRow;
            
            float totalWidth = checkoutAreaSize.x;
            float totalDepth = checkoutAreaSize.z;
            
            float startX = -(totalWidth * 0.5f) + (totalWidth / (maxProductsPerRow + 1));
            float startZ = -(totalDepth * 0.5f) + (totalDepth * 0.25f);
            
            float xSpacing = totalWidth / (maxProductsPerRow + 1);
            float zSpacing = rowSpacing;
            
            Vector3 localOffset = new Vector3(
                startX + (col * xSpacing),
                0f,
                startZ + (row * zSpacing)
            );
            
            Vector3 worldOffset = checkoutArea.TransformDirection(localOffset);
            return basePosition + surfaceOffset + worldOffset;
        }
        
        /// <summary>
        /// Smoothly move a product to a new position
        /// </summary>
        /// <param name="product">Product to move</param>
        /// <param name="targetPosition">Target position</param>
        /// <returns>Coroutine enumerator</returns>
        private System.Collections.IEnumerator SmoothMoveProduct(Product product, Vector3 targetPosition)
        {
            if (product == null) yield break;
            
            Vector3 startPosition = product.transform.position;
            float moveTime = 0.5f; // Half second to move
            float elapsed = 0f;
            
            while (elapsed < moveTime && product != null)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / moveTime;
                
                // Use smooth curve for more natural movement
                float smoothProgress = Mathf.SmoothStep(0f, 1f, progress);
                
                product.transform.position = Vector3.Lerp(startPosition, targetPosition, smoothProgress);
                yield return null;
            }
            
            // Ensure final position is exact
            if (product != null)
            {
                product.transform.position = targetPosition;
            }
        }
        
        #endregion

        #region Product Scanning Test

        /// <summary>
        /// Test the new product scanning system
        /// </summary>
        [ContextMenu("Test Product Scanning")]
        public void TestProductScanning()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("Product scanning test requires Play mode");
                return;
            }
            
            Debug.Log("=== PRODUCT SCANNING TEST ===");
            Debug.Log($"Products at checkout: {productsAtCheckout.Count}");
            Debug.Log($"CheckoutUI connected: {checkoutUI != null}");
            
            if (checkoutUI != null)
            {
                Debug.Log($"Products in UI: {checkoutUI.GetProductCount()}");
            }
            
            foreach (Product product in productsAtCheckout)
            {
                if (product != null)
                {
                    bool isScanned = product.IsScannedAtCheckout;
                    string productName = product.ProductData?.ProductName ?? product.name;
                    string interactionText = product.InteractionText;
                    bool canInteract = product.CanInteract;
                    
                    Debug.Log($"Product: {productName}");
                    Debug.Log($"  - Scanned: {isScanned}");
                    Debug.Log($"  - Can Interact: {canInteract}");
                    Debug.Log($"  - Interaction Text: {interactionText}");
                    Debug.Log($"  - Position: {product.transform.position}");
                    Debug.Log($"  - Parent: {product.transform.parent?.name ?? "None"}");
                }
            }
            
            // Test UI refresh
            Debug.Log("Testing UI refresh...");
            RefreshUI();
        }
        
        #endregion

        /// <summary>
        /// Get all products currently at checkout (for UI synchronization)
        /// </summary>
        /// <returns>Read-only list of products at checkout</returns>
        public IReadOnlyList<Product> GetProductsAtCheckout()
        {
            return productsAtCheckout.AsReadOnly();
        }
        
        /// <summary>
        /// Force UI update with current checkout state
        /// </summary>
        public void RefreshUI()
        {
            if (checkoutUI != null)
            {
                // Clear UI and rebuild it
                checkoutUI.ClearProducts();
                
                // Add all current products
                foreach (Product product in productsAtCheckout)
                {
                    if (product != null)
                    {
                        checkoutUI.AddProduct(product);
                    }
                }
                
                // Update totals and customer info
                checkoutUI.UpdateTotal(runningTotal);
                if (currentCustomer != null)
                {
                    checkoutUI.UpdateCustomer(currentCustomer.name);
                }
                
                Debug.Log($"CheckoutCounter: Refreshed UI with {productsAtCheckout.Count} products");
            }
        }
    }
}
