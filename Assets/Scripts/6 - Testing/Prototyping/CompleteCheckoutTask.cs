using UnityEngine;
using Opsive.BehaviorDesigner.Runtime.Tasks.Actions;
using Opsive.BehaviorDesigner.Runtime.Tasks;

namespace TabletopShop
{
    /// <summary>
    /// Behavior Designer task for completing checkout process
    /// Handles payment processing and customer departure
    /// </summary>
    public class CompleteCheckoutTask : Action
    {
        [Header("Settings Override (Optional)")]
        [Tooltip("Leave null to use global settings from CustomerBehaviorSettingsManager")]
        public CustomerBehaviorSettings settingsOverride;
        
        private CheckoutCounter checkoutCounter = null;
        private float paymentStartTime = 0f;
        private bool hasRequestedPayment = false;
        private bool paymentCompleted = false;
        
        /// <summary>
        /// Get the checkout settings to use (either override or global)
        /// </summary>
        private CheckoutSettings GetCheckoutSettings()
        {
            if (settingsOverride != null && settingsOverride.checkout != null)
                return settingsOverride.checkout;
            
            return CustomerBehaviorSettingsManager.Checkout;
        }
        
        public override void OnStart()
        {
            Customer customer = GetComponent<Customer>();
            if (customer == null)
            {
                Debug.LogError("[CompleteCheckoutTask] Customer component not found!");
                return;
            }
            
            // Find checkout counter
            checkoutCounter = FindNearestCheckoutCounter();
            if (checkoutCounter == null)
            {
                Debug.LogError($"[CompleteCheckoutTask] {customer.name}: No checkout counter found!");
                return;
            }
            
            // Validate preconditions
            if (!checkoutCounter.HasCustomer || !checkoutCounter.CanCustomerPlaceItems(customer))
            {
                Debug.LogError($"[CompleteCheckoutTask] {customer.name}: Not current customer at checkout!");
                return;
            }
            
            if (!checkoutCounter.HasProducts)
            {
                Debug.LogError($"[CompleteCheckoutTask] {customer.name}: No products at checkout!");
                return;
            }
            
            if (!checkoutCounter.AllProductsScanned)
            {
                Debug.LogError($"[CompleteCheckoutTask] {customer.name}: Not all products scanned!");
                return;
            }
            
            paymentStartTime = Time.time;
            hasRequestedPayment = false;
            paymentCompleted = false;
            
            if (customer.showDebugLogs)
                Debug.Log($"[CompleteCheckoutTask] {customer.name}: Ready to complete checkout");
        }
        
        public override TaskStatus OnUpdate()
        {
            if (checkoutCounter == null)
                return TaskStatus.Failure;
                
            Customer customer = GetComponent<Customer>();
            if (customer == null)
                return TaskStatus.Failure;
            
            // Check timeout using settings
            var checkoutSettings = GetCheckoutSettings();
            float maxPaymentWaitTime = checkoutSettings?.maxPaymentWaitTime ?? 30f;
            float waitTime = Time.time - paymentStartTime;
            if (waitTime > maxPaymentWaitTime)
            {
                Debug.LogWarning($"[CompleteCheckoutTask] {customer.name}: Payment timeout after {waitTime:F1}s (max: {maxPaymentWaitTime}s)");
                return TaskStatus.Failure;
            }
            
            // Step 1: Request payment processing
            if (!hasRequestedPayment)
            {
                RequestPaymentProcessing(customer);
                hasRequestedPayment = true;
                return TaskStatus.Running;
            }
            
            // Step 2: Wait for payment completion
            if (!paymentCompleted)
            {
                if (CheckPaymentCompleted(customer))
                {
                    paymentCompleted = true;
                    if (customer.showDebugLogs)
                        Debug.Log($"[CompleteCheckoutTask] ✅ {customer.name}: Payment completed successfully");
                    
                    // Start departure sequence after brief delay from settings
                    float departureDelay = checkoutSettings?.departureDelay ?? 1f;
                    //Invoke(nameof(StartDepartureSequence), departureDelay);
                    return TaskStatus.Running;
                }
                return TaskStatus.Running;
            }
            
            // Step 3: Complete departure
            if (IsDepartureComplete(customer))
            {
                if (customer.showDebugLogs)
                    Debug.Log($"[CompleteCheckoutTask] ✅ {customer.name}: Checkout completed - ready to exit");
                return TaskStatus.Success;
            }
            
            return TaskStatus.Running;
        }
        
        public override void OnEnd()
        {
            Customer customer = GetComponent<Customer>();
            if (customer != null && customer.showDebugLogs)
            {
                float totalTime = Time.time - paymentStartTime;
                Debug.Log($"[CompleteCheckoutTask] {customer.name}: Checkout task completed in {totalTime:F1}s");
            }
        }
        
        /// <summary>
        /// Request payment processing from the checkout counter
        /// </summary>
        /// <param name="customer">Customer completing checkout</param>
        private void RequestPaymentProcessing(Customer customer)
        {
            if (customer.showDebugLogs)
                Debug.Log($"[CompleteCheckoutTask] {customer.name}: Requesting payment processing...");
            
            // Trigger payment processing on the checkout counter
            checkoutCounter.ProcessPayment();
        }
        
        /// <summary>
        /// Check if payment has been completed
        /// </summary>
        /// <param name="customer">Customer being checked</param>
        /// <returns>True if payment completed</returns>
        private bool CheckPaymentCompleted(Customer customer)
        {
            // Payment is complete when:
            // 1. Customer is no longer at checkout (cleared by payment processing)
            // 2. OR checkout counter has no products (cleared after payment)
            // 3. OR customer has been notified of completion
            
            bool customerCleared = !checkoutCounter.HasCustomer || !checkoutCounter.CanCustomerPlaceItems(customer);
            bool productsCleared = !checkoutCounter.HasProducts;
            
            if (customerCleared || productsCleared)
            {
                if (customer.showDebugLogs)
                    Debug.Log($"[CompleteCheckoutTask] {customer.name}: Payment completed - customer cleared: {customerCleared}, products cleared: {productsCleared}");
                return true;
            }
            
            return false;
        }
        
        /// <summary>
        /// Check if customer departure is complete
        /// </summary>
        /// <param name="customer">Customer being checked</param>
        /// <returns>True if departure complete</returns>
        private bool IsDepartureComplete(Customer customer)
        {
            // Departure is complete when customer is no longer associated with checkout
            return !checkoutCounter.HasCustomer || !checkoutCounter.CanCustomerPlaceItems(customer);
        }
        
        /// <summary>
        /// Start the departure sequence (called after payment completion)
        /// </summary>
        private void StartDepartureSequence()
        {
            Customer customer = GetComponent<Customer>();
            if (customer != null && customer.showDebugLogs)
                Debug.Log($"[CompleteCheckoutTask] {customer.name}: Starting departure sequence");
            
            // Additional departure logic could be added here if needed
            // For now, the checkout counter handles customer departure in ProcessPayment()
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
            
            Customer customer = GetComponent<Customer>();
            if (customer == null)
                return checkoutCounters[0];
            
            // Find the checkout counter where this customer is currently active
            foreach (CheckoutCounter checkout in checkoutCounters)
            {
                if (checkout.HasCustomer && checkout.CanCustomerPlaceItems(customer))
                {
                    return checkout;
                }
            }
            
            // Fallback to first checkout counter
            return checkoutCounters[0];
        }
    }
}