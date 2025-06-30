using UnityEngine;

namespace TabletopShop
{
    /// <summary>
    /// Settings for customer checkout behavior
    /// Centralized configuration for all checkout-related AI parameters
    /// </summary>
    [System.Serializable]
    public class CheckoutSettings
    {
        [Header("Queue Management")]
        [Tooltip("Maximum time customer will wait in queue before giving up")]
        public float maxQueueWaitTime = 60f;
        
        [Tooltip("How often to log queue status for debugging")]
        public float queueStatusLogInterval = 3f;
        
        [Header("Scanning Process")]
        [Tooltip("Maximum time to wait for player to scan all products")]
        public float maxScanWaitTime = 120f;
        
        [Tooltip("How often to check and log scanning progress")]
        public float progressCheckInterval = 2f;
        
        [Header("Payment Processing")]
        [Tooltip("Time taken to complete payment processing")]
        public float paymentProcessingTime = 5f;
        
        [Tooltip("Time customer takes to collect items after payment")]
        public float itemCollectionTime = 2f;
        
        [Header("Product Placement")]
        [Tooltip("Delay between placing each product on counter")]
        public float productPlacementDelay = 0.5f;
        
        [Tooltip("Maximum time to spend placing all products")]
        public float maxPlacementTime = 30f;
        
        [Header("Timeouts")]
        [Tooltip("General timeout for checkout-related operations")]
        public float generalCheckoutTimeout = 180f;
        
        [Tooltip("Time to wait after checkout completion before leaving")]
        public float checkoutCompleteDelay = 1f;
    }
}