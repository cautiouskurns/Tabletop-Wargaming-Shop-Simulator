using UnityEngine;
using System.Collections.Generic;

namespace TabletopShop
{
    /// <summary>
    /// Customer spawner system for managing customer lifecycle in the shop
    /// Handles spawning customers at intervals and tracking active customers
    /// </summary>
    public class CustomerSpawner : MonoBehaviour
    {
        [Header("Customer Prefab Configuration")]
        [SerializeField] private GameObject customerPrefab;
        
        [Header("Spawn Location Configuration")]
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private Transform exitPoint;
        
        [Header("Spawning Settings")]
        [SerializeField] private float minSpawnInterval = 30f;
        [SerializeField] private float maxSpawnInterval = 90f;
        [SerializeField] private int maxCustomers = 3;
        
        [Header("Debug Information")]
        [SerializeField] private bool enableDebugLogging = true;
        
        // Active customer tracking
        private List<Customer> activeCustomers = new List<Customer>();
        
        // Spawning state
        private bool isSpawning = false;
        
        // Properties
        public int ActiveCustomerCount => activeCustomers.Count;
        public bool IsSpawning => isSpawning;
        public bool CanSpawnCustomer => activeCustomers.Count < maxCustomers;
        public GameObject CustomerPrefab => customerPrefab;
        public Transform SpawnPoint => spawnPoint;
        public Transform ExitPoint => exitPoint;
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            ValidateReferences();
        }
        
        private void Start()
        {
            InitializeSpawner();
        }
        
        #endregion
        
        #region Initialization
        
        /// <summary>
        /// Validate required references and log warnings for missing components
        /// </summary>
        private void ValidateReferences()
        {
            bool hasErrors = false;
            
            if (customerPrefab == null)
            {
                Debug.LogError($"CustomerSpawner on {name}: Customer Prefab is not assigned!");
                hasErrors = true;
            }
            else
            {
                // Validate that the prefab has a Customer component
                Customer customerComponent = customerPrefab.GetComponent<Customer>();
                if (customerComponent == null)
                {
                    Debug.LogError($"CustomerSpawner on {name}: Customer Prefab does not have a Customer component!");
                    hasErrors = true;
                }
            }
            
            if (spawnPoint == null)
            {
                Debug.LogError($"CustomerSpawner on {name}: Spawn Point is not assigned!");
                hasErrors = true;
            }
            
            if (exitPoint == null)
            {
                Debug.LogWarning($"CustomerSpawner on {name}: Exit Point is not assigned - customers may use fallback exit logic");
            }
            
            // Validate spawn interval settings
            if (minSpawnInterval <= 0f)
            {
                Debug.LogWarning($"CustomerSpawner on {name}: Min Spawn Interval should be greater than 0. Setting to 30s.");
                minSpawnInterval = 30f;
            }
            
            if (maxSpawnInterval < minSpawnInterval)
            {
                Debug.LogWarning($"CustomerSpawner on {name}: Max Spawn Interval is less than Min Spawn Interval. Adjusting values.");
                maxSpawnInterval = minSpawnInterval + 30f;
            }
            
            if (maxCustomers <= 0)
            {
                Debug.LogWarning($"CustomerSpawner on {name}: Max Customers should be greater than 0. Setting to 3.");
                maxCustomers = 3;
            }
            
            if (!hasErrors && enableDebugLogging)
            {
                Debug.Log($"CustomerSpawner on {name}: All required references validated successfully");
            }
        }
        
        /// <summary>
        /// Initialize the spawner system and log initial state
        /// </summary>
        private void InitializeSpawner()
        {
            // Initialize active customers list
            activeCustomers.Clear();
            
            if (enableDebugLogging)
            {
                Debug.Log($"CustomerSpawner initialized on {name}:");
                Debug.Log($"- Spawn Interval: {minSpawnInterval}s - {maxSpawnInterval}s");
                Debug.Log($"- Max Customers: {maxCustomers}");
                Debug.Log($"- Spawn Point: {(spawnPoint != null ? spawnPoint.name : "Not Set")}");
                Debug.Log($"- Exit Point: {(exitPoint != null ? exitPoint.name : "Not Set")}");
                Debug.Log($"- Customer Prefab: {(customerPrefab != null ? customerPrefab.name : "Not Set")}");
            }
        }
        
        #endregion
        
        #region Spawning Control
        
        /// <summary>
        /// Start the customer spawning process
        /// </summary>
        public void StartSpawning()
        {
            if (isSpawning)
            {
                if (enableDebugLogging)
                {
                    Debug.LogWarning($"CustomerSpawner on {name}: Already spawning customers");
                }
                return;
            }
            
            if (customerPrefab == null || spawnPoint == null)
            {
                Debug.LogError($"CustomerSpawner on {name}: Cannot start spawning - missing required references");
                return;
            }
            
            isSpawning = true;
            
            if (enableDebugLogging)
            {
                Debug.Log($"CustomerSpawner on {name}: Started customer spawning");
            }
            
            // TODO: Implement spawning coroutine logic
        }
        
        /// <summary>
        /// Stop the customer spawning process
        /// </summary>
        public void StopSpawning()
        {
            if (!isSpawning)
            {
                if (enableDebugLogging)
                {
                    Debug.LogWarning($"CustomerSpawner on {name}: Not currently spawning customers");
                }
                return;
            }
            
            isSpawning = false;
            
            if (enableDebugLogging)
            {
                Debug.Log($"CustomerSpawner on {name}: Stopped customer spawning");
            }
            
            // TODO: Implement spawning coroutine cleanup
        }
        
        #endregion
        
        #region Customer Management
        
        /// <summary>
        /// Add a customer to the active customers list
        /// </summary>
        /// <param name="customer">Customer to add to tracking</param>
        public void RegisterCustomer(Customer customer)
        {
            if (customer != null && !activeCustomers.Contains(customer))
            {
                activeCustomers.Add(customer);
                
                if (enableDebugLogging)
                {
                    Debug.Log($"CustomerSpawner on {name}: Registered customer {customer.name} (Total: {activeCustomers.Count}/{maxCustomers})");
                }
            }
        }
        
        /// <summary>
        /// Remove a customer from the active customers list
        /// </summary>
        /// <param name="customer">Customer to remove from tracking</param>
        public void UnregisterCustomer(Customer customer)
        {
            if (customer != null && activeCustomers.Contains(customer))
            {
                activeCustomers.Remove(customer);
                
                if (enableDebugLogging)
                {
                    Debug.Log($"CustomerSpawner on {name}: Unregistered customer {customer.name} (Total: {activeCustomers.Count}/{maxCustomers})");
                }
            }
        }
        
        /// <summary>
        /// Get the current number of active customers
        /// </summary>
        /// <returns>Number of active customers</returns>
        public int GetActiveCustomerCount()
        {
            return activeCustomers.Count;
        }
        
        /// <summary>
        /// Get a copy of the active customers list
        /// </summary>
        /// <returns>List of active customers</returns>
        public List<Customer> GetActiveCustomers()
        {
            return new List<Customer>(activeCustomers);
        }
        
        #endregion
        
        #region Debug and Utilities
        
        /// <summary>
        /// Get debug information about the spawner state
        /// </summary>
        /// <returns>Debug string with spawner information</returns>
        public string GetDebugInfo()
        {
            return $"CustomerSpawner {name}: " +
                   $"IsSpawning={isSpawning}, " +
                   $"ActiveCustomers={activeCustomers.Count}/{maxCustomers}, " +
                   $"CanSpawn={CanSpawnCustomer}, " +
                   $"SpawnInterval={minSpawnInterval}-{maxSpawnInterval}s";
        }
        
        /// <summary>
        /// Clean up null references from active customers list
        /// </summary>
        private void CleanupActiveCustomers()
        {
            int removedCount = activeCustomers.RemoveAll(customer => customer == null);
            
            if (removedCount > 0 && enableDebugLogging)
            {
                Debug.Log($"CustomerSpawner on {name}: Cleaned up {removedCount} null customer references");
            }
        }
        
        #endregion
        
        #region Unity Editor Support
        
        private void OnValidate()
        {
            // Ensure valid ranges in editor
            minSpawnInterval = Mathf.Max(1f, minSpawnInterval);
            maxSpawnInterval = Mathf.Max(minSpawnInterval, maxSpawnInterval);
            maxCustomers = Mathf.Max(1, maxCustomers);
        }
        
        private void OnDrawGizmosSelected()
        {
            // Draw spawn point
            if (spawnPoint != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(spawnPoint.position, 1f);
                Gizmos.DrawIcon(spawnPoint.position, "Assets/Gizmos/spawn_icon.png", true);
            }
            
            // Draw exit point
            if (exitPoint != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(exitPoint.position, 1f);
                Gizmos.DrawIcon(exitPoint.position, "Assets/Gizmos/exit_icon.png", true);
            }
            
            // Draw connection line
            if (spawnPoint != null && exitPoint != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(spawnPoint.position, exitPoint.position);
            }
        }
        
        #endregion
    }
}
