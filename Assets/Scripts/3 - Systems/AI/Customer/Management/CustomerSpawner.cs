using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace TabletopShop
{
    /// <summary>
    /// Customer spawner system for managing customer lifecycle in the shop
    /// Handles spawning customers at intervals and tracking active customers
    /// Tesdt
    /// </summary>
    public class CustomerSpawner : MonoBehaviour
    {
        [Header("Customer Prefab Configuration")]
        [SerializeField] private List<GameObject> customerPrefabs = new List<GameObject>();
        
        [Header("Spawn Location Configuration")]
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private Transform exitPoint;
        
        [Header("Spawning Settings")]
        [SerializeField] private float minSpawnInterval = 30f;
        [SerializeField] private float maxSpawnInterval = 90f;
        [SerializeField] private int maxCustomers = 3;
        
        [Header("Debug Information")]
        [SerializeField] private bool enableDebugLogging = false;
        
        // Active customer tracking
        private List<Customer> activeCustomers = new List<Customer>();
        
        // Spawning state
        private bool isSpawning = false;
        private Coroutine spawningCoroutine;
        private Coroutine cleanupCoroutine;
        
        // Properties
        public int ActiveCustomerCount => GetActiveCustomerCount();
        public bool IsSpawning => isSpawning;
        public bool CanSpawnCustomer => CanSpawnCustomerInternal();
        public bool IsAtMaxCapacity => GetActiveCustomerCount() >= maxCustomers;
        public GameObject CustomerPrefab => customerPrefabs.Count > 0 ? customerPrefabs[0] : null;
        public Transform SpawnPoint => spawnPoint;
        public Transform ExitPoint => exitPoint;
        
        /// <summary>
        /// Enhanced spawn condition checking with GameManager day/night cycle integration
        /// Checks multiple conditions: capacity limits, day/night cycle, and daily customer limits
        /// </summary>
        /// <returns>True if a customer can be spawned</returns>
        private bool CanSpawnCustomerInternal()
        {
            // Priority 1: Check basic capacity limits (preserve existing logic)
            if (GetActiveCustomerCount() >= maxCustomers)
            {
                if (enableDebugLogging)
                {
                    Debug.Log($"CustomerSpawner {name}: Cannot spawn - at capacity limit ({GetActiveCustomerCount()}/{maxCustomers})");
                }
                return false;
            }
            
            // Priority 2: Check GameManager day/night cycle (graceful fallback if unavailable)
            if (GameManager.Instance != null)
            {
                // Only spawn customers during day time for realistic shop hours
                if (!GameManager.Instance.IsDayTime)
                {
                    if (enableDebugLogging)
                    {
                        Debug.Log($"CustomerSpawner {name}: Cannot spawn - shop is closed (night time)");
                    }
                    return false;
                }
                
                // Priority 3: Check daily customer limit to create economic pressure
                // This distinguishes between customer capacity (how many can be in shop simultaneously)
                // and daily limit (how many customers the shop can serve per day)
                if (GameManager.Instance.CustomersServedToday >= GameManager.Instance.MaxDailyCustomers)
                {
                    if (enableDebugLogging)
                    {
                        Debug.Log($"CustomerSpawner {name}: Cannot spawn - daily customer limit reached ({GameManager.Instance.CustomersServedToday}/{GameManager.Instance.MaxDailyCustomers})");
                    }
                    return false;
                }
            }
            else
            {
                // Graceful fallback: Continue spawning if GameManager unavailable (for testing/development)
                if (enableDebugLogging)
                {
                    Debug.LogWarning($"CustomerSpawner {name}: GameManager unavailable - using fallback spawn logic");
                }
            }
            
            // All conditions passed - customer can be spawned
            if (enableDebugLogging)
            {
                string gameManagerStatus = GameManager.Instance != null ? 
                    $"Day: {GameManager.Instance.CurrentDay}, IsDay: {GameManager.Instance.IsDayTime}, Served: {GameManager.Instance.CustomersServedToday}/{GameManager.Instance.MaxDailyCustomers}" : 
                    "GameManager unavailable";
                Debug.Log($"CustomerSpawner {name}: Can spawn customer - Active: {GetActiveCustomerCount()}/{maxCustomers}, {gameManagerStatus}");
            }
            
            return true;
        }
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            ValidateReferences();
        }
        
        private void Start()
        {
            InitializeSpawner();
        }
        
        private void Update()
        {
            // Periodically clean up null customer references
            if (Time.frameCount % 300 == 0) // Every 5 seconds at 60fps
            {
                CleanupActiveCustomers();
            }
        }
        
        private void OnDestroy()
        {
            CleanupSpawner();
        }
        
        #endregion
        
        #region Initialization
        
        /// <summary>
        /// Validate required references and log warnings for missing components
        /// </summary>
        private void ValidateReferences()
        {
            bool hasErrors = false;
            
            if (customerPrefabs == null || customerPrefabs.Count == 0)
            {
                Debug.LogError($"CustomerSpawner on {name}: No Customer Prefabs assigned!");
                hasErrors = true;
            }
            else
            {
                // Validate that all prefabs have Customer components
                for (int i = 0; i < customerPrefabs.Count; i++)
                {
                    if (customerPrefabs[i] == null)
                    {
                        Debug.LogError($"CustomerSpawner on {name}: Customer Prefab at index {i} is null!");
                        hasErrors = true;
                        continue;
                    }
                    
                    Customer customerComponent = customerPrefabs[i].GetComponent<Customer>();
                    if (customerComponent == null)
                    {
                        Debug.LogError($"CustomerSpawner on {name}: Customer Prefab '{customerPrefabs[i].name}' does not have a Customer component!");
                        hasErrors = true;
                    }
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
                Debug.Log($"- Customer Prefabs: {customerPrefabs.Count} assigned");
            }
            
            // Start regular cleanup coroutine
            StartCleanupCoroutine();
            
            // Auto-start spawning if all requirements are met
            if (customerPrefabs.Count > 0 && spawnPoint != null)
            {
                StartSpawning();
            }
            else
            {
                Debug.LogWarning($"CustomerSpawner on {name}: Cannot auto-start - missing required references. Call StartSpawning() manually after setup.");
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
            
            if (customerPrefabs.Count == 0 || spawnPoint == null)
            {
                Debug.LogError($"CustomerSpawner on {name}: Cannot start spawning - missing required references");
                return;
            }
            
            isSpawning = true;
            
            if (enableDebugLogging)
            {
                Debug.Log($"CustomerSpawner on {name}: Started customer spawning");
            }
            
            // Start the spawning coroutine
            spawningCoroutine = StartCoroutine(SpawnCustomers());
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
            
            // Stop the spawning coroutine
            if (spawningCoroutine != null)
            {
                StopCoroutine(spawningCoroutine);
                spawningCoroutine = null;
            }
            
            if (enableDebugLogging)
            {
                Debug.Log($"CustomerSpawner on {name}: Stopped customer spawning");
            }
        }
        
        /// <summary>
        /// Coroutine that handles the spawning of customers at random intervals
        /// </summary>
        private IEnumerator SpawnCustomers()
        {
            // Initial delay before first spawn
            if (enableDebugLogging)
            {
                Debug.Log($"CustomerSpawner on {name}: Waiting 10 seconds before first spawn");
            }
            yield return new WaitForSeconds(10f);
            
            while (isSpawning)
            {
                // Check if we can spawn more customers
                if (CanSpawnCustomer)
                {
                    SpawnCustomer();
                }
                else
                {
                    if (enableDebugLogging)
                    {
                        Debug.Log($"CustomerSpawner on {name}: At maximum customer limit ({GetActiveCustomerCount()}/{maxCustomers}), waiting...");
                    }
                }
                
                // Wait for random interval before next spawn attempt
                float nextSpawnDelay = Random.Range(minSpawnInterval, maxSpawnInterval);
                if (enableDebugLogging)
                {
                    Debug.Log($"CustomerSpawner on {name}: Next spawn attempt in {nextSpawnDelay:F1} seconds");
                }
                yield return new WaitForSeconds(nextSpawnDelay);
            }
            
            if (enableDebugLogging)
            {
                Debug.Log($"CustomerSpawner on {name}: Spawning coroutine ended");
            }
        }
        
        /// <summary>
        /// Spawn a single customer at the spawn point
        /// </summary>
        private void SpawnCustomer()
        {
            if (customerPrefabs.Count == 0 || spawnPoint == null)
            {
                Debug.LogError($"CustomerSpawner on {name}: Cannot spawn customer - missing required references");
                return;
            }
            
            if (!CanSpawnCustomer)
            {
                if (enableDebugLogging)
                {
                    Debug.LogWarning($"CustomerSpawner on {name}: Cannot spawn customer - at maximum limit ({activeCustomers.Count}/{maxCustomers})");
                }
                return;
            }
            
            try
            {
                // Randomly select a customer prefab for variety
                GameObject selectedPrefab = customerPrefabs[Random.Range(0, customerPrefabs.Count)];
                
                // Instantiate customer at spawn point
                GameObject customerObject = Instantiate(selectedPrefab, spawnPoint.position, spawnPoint.rotation);
                
                // Get the Customer component
                Customer customerComponent = customerObject.GetComponent<Customer>();
                if (customerComponent == null)
                {
                    Debug.LogError($"CustomerSpawner on {name}: Spawned customer prefab does not have Customer component!");
                    Destroy(customerObject);
                    return;
                }
                
                // Set up customer name for identification
                customerObject.name = $"Customer_{System.DateTime.Now:HHmmss}_{Random.Range(100, 999)}";
                
                // Register the customer
                RegisterCustomer(customerComponent);
                
                if (enableDebugLogging)
                {
                    Debug.Log($"CustomerSpawner on {name}: Successfully spawned customer '{customerObject.name}' at {spawnPoint.position}");
                    Debug.Log($"CustomerSpawner on {name}: Active customers: {activeCustomers.Count}/{maxCustomers}");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"CustomerSpawner on {name}: Failed to spawn customer: {ex.Message}");
            }
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
                    Debug.Log($"CustomerSpawner on {name}: Registered customer {customer.name} (Total: {GetActiveCustomerCount()}/{maxCustomers})");
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
                    Debug.Log($"CustomerSpawner on {name}: Unregistered customer {customer.name} (Total: {GetActiveCustomerCount()}/{maxCustomers})");
                }
            }
        }
        
        /// <summary>
        /// Get the current number of active customers (excludes null references)
        /// </summary>
        /// <returns>Number of active customers</returns>
        public int GetActiveCustomerCount()
        {
            // Clean up null references first, then return count
            CleanupDestroyedCustomers();
            return activeCustomers.Count;
        }
        
        /// <summary>
        /// Get a copy of the active customers list (excludes null references)
        /// </summary>
        /// <returns>List of active customers</returns>
        public List<Customer> GetActiveCustomers()
        {
            CleanupDestroyedCustomers();
            return new List<Customer>(activeCustomers);
        }
        
        #endregion
        
        #region Cleanup and Lifecycle Management
        
        /// <summary>
        /// Start the cleanup coroutine for regular maintenance
        /// </summary>
        private void StartCleanupCoroutine()
        {
            StopCleanupCoroutine();
            cleanupCoroutine = StartCoroutine(RegularCleanup());
        }
        
        /// <summary>
        /// Stop the cleanup coroutine
        /// </summary>
        private void StopCleanupCoroutine()
        {
            if (cleanupCoroutine != null)
            {
                StopCoroutine(cleanupCoroutine);
                cleanupCoroutine = null;
            }
        }
        
        /// <summary>
        /// Regular cleanup coroutine that runs every 5 seconds
        /// </summary>
        private IEnumerator RegularCleanup()
        {
            while (true)
            {
                yield return new WaitForSeconds(5f);
                CleanupDestroyedCustomers();
            }
        }
        
        /// <summary>
        /// Clean up null references from active customers list and prevent memory leaks
        /// </summary>
        public void CleanupDestroyedCustomers()
        {
            int initialCount = activeCustomers.Count;
            int removedCount = activeCustomers.RemoveAll(customer => customer == null);
            
            if (removedCount > 0)
            {
                if (enableDebugLogging)
                {
                    Debug.Log($"CustomerSpawner on {name}: Cleaned up {removedCount} destroyed customer references (was {initialCount}, now {activeCustomers.Count})");
                }
                
                // Force garbage collection if we removed many customers
                if (removedCount > 2)
                {
                    System.GC.Collect();
                }
            }
        }
        
        /// <summary>
        /// Manually trigger cleanup of destroyed customers
        /// </summary>
        public void ManualCleanup()
        {
            CleanupDestroyedCustomers();
            
            if (enableDebugLogging)
            {
                Debug.Log($"CustomerSpawner on {name}: Manual cleanup completed. Active customers: {activeCustomers.Count}/{maxCustomers}");
            }
        }
        
        /// <summary>
        /// Clean shutdown of spawner system
        /// </summary>
        private void CleanupSpawner()
        {
            if (enableDebugLogging)
            {
                Debug.Log($"CustomerSpawner on {name}: Shutting down spawner system");
            }
            
            // Stop all coroutines
            StopSpawning();
            StopCleanupCoroutine();
            
            // Clean up customer references
            CleanupDestroyedCustomers();
            
            // Clear the list
            activeCustomers.Clear();
            
            if (enableDebugLogging)
            {
                Debug.Log($"CustomerSpawner on {name}: Spawner cleanup completed");
            }
        }
        
        #endregion
        
        #region Debug and Utilities
        
        /// <summary>
        /// Get comprehensive debug information about the spawner state
        /// </summary>
        /// <returns>Debug string with spawner information</returns>
        public string GetDebugInfo()
        {
            CleanupDestroyedCustomers(); // Ensure accurate count
            
            return $"CustomerSpawner {name}: " +
                   $"IsSpawning={isSpawning}, " +
                   $"ActiveCustomers={activeCustomers.Count}/{maxCustomers}, " +
                   $"CanSpawn={CanSpawnCustomer}, " +
                   $"IsAtMaxCapacity={IsAtMaxCapacity}, " +
                   $"SpawnInterval={minSpawnInterval}-{maxSpawnInterval}s, " +
                   $"HasSpawnPoint={spawnPoint != null}, " +
                   $"HasExitPoint={exitPoint != null}, " +
                   $"HasPrefabs={customerPrefabs.Count > 0}";
        }
        
        /// <summary>
        /// Get detailed debug information including customer list
        /// </summary>
        /// <returns>Detailed debug string</returns>
        public string GetDetailedDebugInfo()
        {
            CleanupDestroyedCustomers();
            
            string baseInfo = GetDebugInfo();
            string customerList = "\nActive Customers:";
            
            if (activeCustomers.Count == 0)
            {
                customerList += " None";
            }
            else
            {
                for (int i = 0; i < activeCustomers.Count; i++)
                {
                    if (activeCustomers[i] != null)
                    {
                        customerList += $"\n  {i + 1}. {activeCustomers[i].name}";
                    }
                }
            }
            
            return baseInfo + customerList;
        }
        
        /// <summary>
        /// Clean up null references from active customers list
        /// </summary>
        private void CleanupActiveCustomers()
        {
            // Redirect to the new method name for backward compatibility
            CleanupDestroyedCustomers();
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
