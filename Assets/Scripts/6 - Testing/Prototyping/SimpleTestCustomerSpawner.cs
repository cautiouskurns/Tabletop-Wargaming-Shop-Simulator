using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Opsive.BehaviorDesigner.Runtime;

namespace TabletopShop
{
    /// <summary>
    /// Simple spawner for SimpleTestCustomer - lightweight and focused on testing
    /// </summary>
    public class SimpleTestCustomerSpawner : MonoBehaviour
    {
        [Header("Customer Configuration")]
        [SerializeField] private GameObject testCustomerPrefab;
        [SerializeField] private BehaviorTree behaviorTreeAsset;
        
        [Header("Spawn Settings")]
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private float spawnInterval = 30f;
        [SerializeField] private int maxCustomers = 3;
        [SerializeField] private bool autoSpawn = true;
        
        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = true;
        
        // Active customer tracking
        private List<GameObject> activeCustomers = new List<GameObject>();
        private Coroutine spawnCoroutine;
        
        // Properties
        public int ActiveCustomerCount => GetActiveCustomerCount();
        public bool CanSpawn => GetActiveCustomerCount() < maxCustomers;
        
        #region Unity Lifecycle
        
        private void Start()
        {
            ValidateSetup();
            
            if (autoSpawn && CanStartSpawning())
            {
                StartSpawning();
            }
        }
        
        private void Update()
        {
            // Clean up destroyed customers every few seconds
            if (Time.frameCount % 300 == 0) // Every 5 seconds at 60fps
            {
                CleanupDestroyedCustomers();
            }
        }
        
        private void OnDestroy()
        {
            StopSpawning();
        }
        
        #endregion
        
        #region Validation
        
        private void ValidateSetup()
        {
            bool isValid = true;
            
            if (testCustomerPrefab == null)
            {
                Debug.LogError($"[SimpleTestCustomerSpawner] No customer prefab assigned!");
                isValid = false;
            }
            else
            {
                // Check if prefab has required components
                SimpleTestCustomer customerComponent = testCustomerPrefab.GetComponent<SimpleTestCustomer>();
                if (customerComponent == null)
                {
                    Debug.LogError($"[SimpleTestCustomerSpawner] Customer prefab doesn't have SimpleTestCustomer component!");
                    isValid = false;
                }
                
                BehaviorTree behaviorTreeComponent = testCustomerPrefab.GetComponent<BehaviorTree>();
                if (behaviorTreeComponent == null)
                {
                    Debug.LogError($"[SimpleTestCustomerSpawner] Customer prefab doesn't have BehaviorTree component!");
                    isValid = false;
                }
            }
            
            if (spawnPoint == null)
            {
                Debug.LogWarning($"[SimpleTestCustomerSpawner] No spawn point assigned, using spawner position");
                spawnPoint = transform;
            }
            
            if (behaviorTreeAsset == null)
            {
                Debug.LogWarning($"[SimpleTestCustomerSpawner] No behavior tree asset assigned - customers will use prefab default");
            }
            
            if (spawnInterval <= 0f)
            {
                Debug.LogWarning($"[SimpleTestCustomerSpawner] Invalid spawn interval, setting to 30 seconds");
                spawnInterval = 30f;
            }
            
            if (showDebugLogs && isValid)
            {
                Debug.Log($"[SimpleTestCustomerSpawner] Setup validated successfully");
            }
        }
        
        private bool CanStartSpawning()
        {
            return testCustomerPrefab != null && spawnPoint != null;
        }
        
        #endregion
        
        #region Spawning Control
        
        [ContextMenu("Start Spawning")]
        public void StartSpawning()
        {
            if (spawnCoroutine != null)
            {
                if (showDebugLogs)
                    Debug.LogWarning("[SimpleTestCustomerSpawner] Already spawning customers");
                return;
            }
            
            if (!CanStartSpawning())
            {
                Debug.LogError("[SimpleTestCustomerSpawner] Cannot start spawning - missing required components");
                return;
            }
            
            spawnCoroutine = StartCoroutine(SpawningLoop());
            
            if (showDebugLogs)
                Debug.Log("[SimpleTestCustomerSpawner] Started spawning customers");
        }
        
        [ContextMenu("Stop Spawning")]
        public void StopSpawning()
        {
            if (spawnCoroutine != null)
            {
                StopCoroutine(spawnCoroutine);
                spawnCoroutine = null;
                
                if (showDebugLogs)
                    Debug.Log("[SimpleTestCustomerSpawner] Stopped spawning customers");
            }
        }
        
        [ContextMenu("Spawn Customer Now")]
        public void SpawnCustomerNow()
        {
            if (CanSpawn)
            {
                SpawnCustomer();
            }
            else
            {
                Debug.LogWarning($"[SimpleTestCustomerSpawner] Cannot spawn - at max capacity ({GetActiveCustomerCount()}/{maxCustomers})");
            }
        }
        
        private IEnumerator SpawningLoop()
        {
            // Initial delay
            yield return new WaitForSeconds(5f);
            
            while (true)
            {
                if (CanSpawn)
                {
                    SpawnCustomer();
                }
                else
                {
                    if (showDebugLogs)
                        Debug.Log($"[SimpleTestCustomerSpawner] At max capacity ({GetActiveCustomerCount()}/{maxCustomers}), waiting...");
                }
                
                yield return new WaitForSeconds(spawnInterval);
            }
        }
        
        #endregion
        
        #region Customer Spawning
        
        private void SpawnCustomer()
        {
            try
            {
                // Instantiate customer
                GameObject customerObj = Instantiate(testCustomerPrefab, spawnPoint.position, spawnPoint.rotation);
                customerObj.name = $"TestCustomer_{System.DateTime.Now:HHmmss}_{Random.Range(100, 999)}";
                
                // Configure components
                SimpleTestCustomer customer = customerObj.GetComponent<SimpleTestCustomer>();
                BehaviorTree behaviorTree = customerObj.GetComponent<BehaviorTree>();
                
                if (customer == null || behaviorTree == null)
                {
                    Debug.LogError("[SimpleTestCustomerSpawner] Spawned customer missing required components!");
                    Destroy(customerObj);
                    return;
                }
                

                
                // Add to tracking
                activeCustomers.Add(customerObj);
                

                
                if (showDebugLogs)
                {
                    Debug.Log($"[SimpleTestCustomerSpawner] ✅ Spawned customer '{customerObj.name}' " +
                             $"(Active: {GetActiveCustomerCount()}/{maxCustomers})");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[SimpleTestCustomerSpawner] Failed to spawn customer: {ex.Message}");
            }
        }
        
        #endregion
        
        #region Customer Management
        
        private int GetActiveCustomerCount()
        {
            CleanupDestroyedCustomers();
            return activeCustomers.Count;
        }
        
        private void CleanupDestroyedCustomers()
        {
            int initialCount = activeCustomers.Count;
            activeCustomers.RemoveAll(customer => customer == null);
            
            int removedCount = initialCount - activeCustomers.Count;
            if (removedCount > 0 && showDebugLogs)
            {
                Debug.Log($"[SimpleTestCustomerSpawner] Cleaned up {removedCount} destroyed customers " +
                         $"(was {initialCount}, now {activeCustomers.Count})");
            }
        }
        
        [ContextMenu("Cleanup Destroyed Customers")]
        public void ManualCleanup()
        {
            CleanupDestroyedCustomers();
            Debug.Log($"[SimpleTestCustomerSpawner] Manual cleanup complete. Active: {activeCustomers.Count}");
        }
        
        [ContextMenu("Destroy All Customers")]
        public void DestroyAllCustomers()
        {
            int count = 0;
            foreach (GameObject customer in activeCustomers)
            {
                if (customer != null)
                {
                    Destroy(customer);
                    count++;
                }
            }
            
            activeCustomers.Clear();
            Debug.Log($"[SimpleTestCustomerSpawner] Destroyed {count} customers");
        }
        
        #endregion
        
        #region Debug Info
        
        [ContextMenu("Show Debug Info")]
        public void ShowDebugInfo()
        {
            CleanupDestroyedCustomers();
            
            Debug.Log($"=== SimpleTestCustomerSpawner Debug Info ===");
            Debug.Log($"Active Customers: {activeCustomers.Count}/{maxCustomers}");
            Debug.Log($"Can Spawn: {CanSpawn}");
            Debug.Log($"Is Spawning: {spawnCoroutine != null}");
            Debug.Log($"Spawn Interval: {spawnInterval}s");
            Debug.Log($"Auto Spawn: {autoSpawn}");
            
            if (activeCustomers.Count > 0)
            {
                Debug.Log($"Customer List:");
                for (int i = 0; i < activeCustomers.Count; i++)
                {
                    if (activeCustomers[i] != null)
                    {
                        SimpleTestCustomer customer = activeCustomers[i].GetComponent<SimpleTestCustomer>();
                        Debug.Log($"  {i + 1}. {activeCustomers[i].name} - " +
                                 $"Money: ${customer?.currentMoney:F2}, " +
                                 $"Products: {customer?.selectedProducts.Count ?? 0}");
                    }
                }
            }
        }
        
        #endregion
        
        #region Editor Support
        
        private void OnValidate()
        {
            spawnInterval = Mathf.Max(1f, spawnInterval);
            maxCustomers = Mathf.Max(1, maxCustomers);
        }
        
        private void OnDrawGizmosSelected()
        {
            // Draw spawn point
            if (spawnPoint != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(spawnPoint.position, 1f);
                Gizmos.DrawIcon(spawnPoint.position + Vector3.up * 2f, "sv_icon_dot3_pix16_gizmo", true);
            }
            
            // Draw spawn range indicator
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, 3f);
        }
        
        #endregion
    }
}