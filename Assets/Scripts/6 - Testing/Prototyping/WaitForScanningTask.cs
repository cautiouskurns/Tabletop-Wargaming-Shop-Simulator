using UnityEngine;
using Opsive.BehaviorDesigner.Runtime.Tasks.Actions;
using Opsive.BehaviorDesigner.Runtime.Tasks;

namespace TabletopShop
{
    /// <summary>
    /// Behavior Designer task for waiting while player scans products at checkout
    /// Monitors scanning progress and handles timeout scenarios
    /// </summary>
    public class WaitForScanningTask : Action
    {
        [Tooltip("Maximum time to wait for scanning before timeout")]
        public float maxScanWaitTime = 120f;
        
        [Tooltip("How often to check scanning progress (seconds)")]
        public float progressCheckInterval = 2f;
        
        private CheckoutCounter checkoutCounter = null;
        private float scanStartTime = 0f;
        private float lastProgressCheck = 0f;
        private int lastScannedCount = 0;
        
        public override void OnStart()
        {
            Customer customer = GetComponent<Customer>();
            if (customer == null)
            {
                Debug.LogError("[WaitForScanningTask] Customer component not found!");
                return;
            }
            
            // Find checkout counter
            checkoutCounter = FindNearestCheckoutCounter();
            if (checkoutCounter == null)
            {
                Debug.LogError($"[WaitForScanningTask] {customer.name}: No checkout counter found!");
                return;
            }
            
            scanStartTime = Time.time;
            lastProgressCheck = Time.time;
            lastScannedCount = 0;
            
            if (customer.showDebugLogs)
                Debug.Log($"[WaitForScanningTask] {customer.name}: Waiting for player to scan products...");
        }
        
        public override TaskStatus OnUpdate()
        {
            if (checkoutCounter == null)
                return TaskStatus.Failure;
                
            Customer customer = GetComponent<Customer>();
            if (customer == null)
                return TaskStatus.Failure;
            
            // Check timeout
            float waitTime = Time.time - scanStartTime;
            if (waitTime > maxScanWaitTime)
            {
                Debug.LogWarning($"[WaitForScanningTask] {customer.name}: Scanning timeout after {waitTime:F1}s - proceeding anyway");
                return TaskStatus.Success; // Allow customer to continue even if not all scanned
            }
            
            // Check if all products are scanned
            if (checkoutCounter.AllProductsScanned)
            {
                if (customer.showDebugLogs)
                    Debug.Log($"[WaitForScanningTask] ✅ {customer.name}: All products scanned! Ready for payment");
                return TaskStatus.Success;
            }
            
            // Periodic progress updates
            if (Time.time - lastProgressCheck >= progressCheckInterval)
            {
                CheckScanningProgress(customer);
                lastProgressCheck = Time.time;
            }
            
            return TaskStatus.Running;
        }
        
        public override void OnEnd()
        {
            Customer customer = GetComponent<Customer>();
            if (customer != null && customer.showDebugLogs)
            {
                float totalWaitTime = Time.time - scanStartTime;
                Debug.Log($"[WaitForScanningTask] {customer.name}: Scanning phase completed after {totalWaitTime:F1}s");
            }
        }
        
        /// <summary>
        /// Check and log scanning progress
        /// </summary>
        /// <param name="customer">Customer waiting</param>
        private void CheckScanningProgress(Customer customer)
        {
            if (!checkoutCounter.HasProducts)
            {
                Debug.LogWarning($"[WaitForScanningTask] {customer.name}: No products at checkout - something went wrong");
                return;
            }
            
            var products = checkoutCounter.GetProductsAtCheckout();
            int scannedCount = 0;
            
            foreach (var product in products)
            {
                if (product != null && product.IsScannedAtCheckout)
                {
                    scannedCount++;
                }
            }
            
            // Log progress if there's been a change
            if (scannedCount != lastScannedCount)
            {
                if (customer.showDebugLogs)
                    Debug.Log($"[WaitForScanningTask] {customer.name}: Scanning progress: {scannedCount}/{products.Count} products scanned");
                lastScannedCount = scannedCount;
            }
            else if (customer.showDebugLogs)
            {
                // Log waiting status occasionally
                Debug.Log($"[WaitForScanningTask] {customer.name}: Still waiting for scanning... ({scannedCount}/{products.Count} scanned)");
            }
        }
        
        /// <summary>
        /// Find the nearest checkout counter in the scene
        /// </summary>
        /// <returns>Nearest CheckoutCounter or null if none found</returns>
        private CheckoutCounter FindNearestCheckoutCounter()
        {
            CheckoutCounter[] checkoutCounters = Object.FindObjectsByType<CheckoutCounter>(FindObjectsSortMode.None);
            
            if (checkoutCounters.Length == 0)
            {
                return null;
            }
            
            // Return the first one for now
            return checkoutCounters[0];
        }
    }
}