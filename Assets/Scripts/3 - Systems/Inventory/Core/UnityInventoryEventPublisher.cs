using UnityEngine;
using UnityEngine.Events;

namespace TabletopShop
{
    /// <summary>
    /// Unity-based implementation of IInventoryEventPublisher.
    /// Uses UnityEvent system for inventory event publishing, maintaining compatibility
    /// with existing UI components and inspector-based event subscriptions.
    /// 
    /// This implementation preserves all existing event functionality while
    /// providing abstraction for the InventoryManager.
    /// </summary>
    public class UnityInventoryEventPublisher : MonoBehaviour, IInventoryEventPublisher
    {
        [Header("Unity Events")]
        [SerializeField] private UnityEvent onInventoryChanged;
        [SerializeField] private UnityEvent<ProductData> onProductSelected;
        [SerializeField] private UnityEvent<ProductData, int> onProductCountChanged;
        
        #region Public Properties (for UI subscription)
        
        /// <summary>
        /// Event fired when inventory changes
        /// </summary>
        public UnityEvent OnInventoryChanged => onInventoryChanged;
        
        /// <summary>
        /// Event fired when a product is selected
        /// </summary>
        public UnityEvent<ProductData> OnProductSelected => onProductSelected;
        
        /// <summary>
        /// Event fired when a specific product count changes
        /// </summary>
        public UnityEvent<ProductData, int> OnProductCountChanged => onProductCountChanged;
        
        #endregion
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            // Initialize events if they're null
            if (onInventoryChanged == null)
                onInventoryChanged = new UnityEvent();
            if (onProductSelected == null)
                onProductSelected = new UnityEvent<ProductData>();
            if (onProductCountChanged == null)
                onProductCountChanged = new UnityEvent<ProductData, int>();
            
//            Debug.Log("UnityInventoryEventPublisher initialized successfully.");
        }
        
        #endregion
        
        #region IInventoryEventPublisher Implementation
        
        /// <summary>
        /// Publish an inventory changed event
        /// Fired when the overall inventory state changes (items added/removed)
        /// </summary>
        public void PublishInventoryChanged()
        {
            onInventoryChanged?.Invoke();
//            Debug.Log("Published InventoryChanged event");
        }
        
        /// <summary>
        /// Publish a product selection event
        /// Fired when a different product is selected for placement/use
        /// </summary>
        /// <param name="product">The newly selected product (null if selection cleared)</param>
        public void PublishProductSelected(ProductData product)
        {
            onProductSelected?.Invoke(product);
            Debug.Log($"Published ProductSelected event: {product?.ProductName ?? "None"}");
        }
        
        /// <summary>
        /// Publish a product count changed event
        /// Fired when a specific product's quantity changes
        /// </summary>
        /// <param name="product">The product whose count changed</param>
        /// <param name="count">The new count of the product</param>
        public void PublishProductCountChanged(ProductData product, int count)
        {
            onProductCountChanged?.Invoke(product, count);
            // Debug.Log($"Published ProductCountChanged event: {product?.ProductName ?? "Unknown"} -> {count}");
        }
        
        #endregion
        
        #region Testing Methods
        
        /// <summary>
        /// Test event publishing functionality
        /// </summary>
        [ContextMenu("Test Event Publishing")]
        public void TestEventPublishing()
        {
            Debug.Log("=== TESTING UNITY INVENTORY EVENT PUBLISHER ===");
            
            // Test inventory changed event
            Debug.Log("Testing InventoryChanged event...");
            PublishInventoryChanged();
            
            // Test product selected event (null)
            Debug.Log("Testing ProductSelected event (null)...");
            PublishProductSelected(null);
            
            // Test product count changed event (mock data)
            Debug.Log("Testing ProductCountChanged event (mock data)...");
            PublishProductCountChanged(null, 5);
            
            Debug.Log("=== EVENT PUBLISHING TEST COMPLETE ===");
        }
        
        /// <summary>
        /// Get status information about this event publisher
        /// </summary>
        public string GetEventPublisherStatus()
        {
            return $"Unity Event Publisher Status:\n" +
                   $"- OnInventoryChanged: {(onInventoryChanged != null ? "Initialized" : "NULL")}\n" +
                   $"- OnProductSelected: {(onProductSelected != null ? "Initialized" : "NULL")}\n" +
                   $"- OnProductCountChanged: {(onProductCountChanged != null ? "Initialized" : "NULL")}\n" +
                   $"- Component Active: {enabled && gameObject.activeInHierarchy}";
        }
        
        /// <summary>
        /// Debug current event publisher state
        /// </summary>
        [ContextMenu("Debug Event Publisher State")]
        public void DebugEventPublisherState()
        {
            Debug.Log("=== UNITY INVENTORY EVENT PUBLISHER DEBUG ===");
            Debug.Log(GetEventPublisherStatus());
            Debug.Log("=== END DEBUG INFO ===");
        }
        
        #endregion
    }
}
