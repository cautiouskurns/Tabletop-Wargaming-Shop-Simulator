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
        
        [Header("Checkout State")]
        [SerializeField] private List<Product> productsAtCheckout = new List<Product>();
        [SerializeField] private Customer currentCustomer;
        [SerializeField] private float runningTotal = 0f;
        
        // Properties for checkout state
        public bool HasCustomer => currentCustomer != null;
        public bool HasProducts => productsAtCheckout.Count > 0;
        public bool AllProductsScanned => HasProducts && productsAtCheckout.All(p => p.IsScannedAtCheckout);
        
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
            
            Debug.Log($"Placed {product.ProductData?.ProductName ?? product.name} at checkout (${product.CurrentPrice})");
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
            float offset = productsAtCheckout.Count * productSpacing;
            return basePosition + checkoutArea.right * offset;
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
                GameManager.Instance.ProcessCustomerPurchase(runningTotal, productsAtCheckout.Count);
            }
            else
            {
                Debug.LogError("GameManager.Instance is null - cannot process purchase");
                return;
            }
            
            Debug.Log($"Processed payment of ${runningTotal:F2} for {productsAtCheckout.Count} products");
            
            // Clear checkout after successful payment
            ClearCheckout();
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
        }
        
        #endregion
    }
}
