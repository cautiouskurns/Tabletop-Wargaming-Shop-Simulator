using UnityEngine;
using UnityEngine.AI;
using System.Collections;

namespace TabletopShop
{
    /// <summary>
    /// Handles customer movement, pathfinding, and navigation using NavMeshAgent.
    /// Manages destination setting, stuck detection, and movement validation.
    /// </summary>
    public class CustomerMovement : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float movementSpeed = 1.5f;
        [SerializeField] private float stoppingDistance = 1f;
        
        [Header("Pathfinding Settings")]
        [SerializeField] private float stuckDetectionTime = 5f;
        [SerializeField] private float stuckDistanceThreshold = 0.5f;
        [SerializeField] private float destinationReachedDistance = 1.5f;
        [SerializeField] private int maxPathfindingRetries = 3;
        [SerializeField] private float pathfindingRetryDelay = 1f;
        
        // Component references
        private NavMeshAgent navMeshAgent;
        
        // Pathfinding state
        private Vector3 currentDestination;
        private bool hasDestination = false;
        private Vector3 lastPosition;
        private float stuckTimer = 0f;
        private int pathfindingRetries = 0;
        private Coroutine stuckDetectionCoroutine;
        
        // Properties
        public bool IsMoving => navMeshAgent != null && navMeshAgent.velocity.magnitude > 0.1f;
        public Vector3 CurrentDestination => currentDestination;
        public bool HasDestination => hasDestination;
        public NavMeshAgent NavMeshAgent => navMeshAgent;
        
        #region Initialization
        
        private void Awake()
        {
            InitializeNavMeshAgent();
        }
        
        /// <summary>
        /// Initialize NavMeshAgent component and configure movement settings
        /// </summary>
        public void Initialize()
        {
            InitializeNavMeshAgent();
        }
        
        /// <summary>
        /// Initialize NavMeshAgent component and configure movement settings
        /// </summary>
        private void InitializeNavMeshAgent()
        {
            navMeshAgent = GetComponent<NavMeshAgent>();
            
            if (navMeshAgent == null)
            {
                Debug.LogError($"CustomerMovement {name} is missing NavMeshAgent component!");
                return;
            }
            
            // Configure NavMeshAgent settings
            navMeshAgent.speed = movementSpeed;
            navMeshAgent.stoppingDistance = stoppingDistance;
            navMeshAgent.autoBraking = true;
            navMeshAgent.autoRepath = true;
            
            Debug.Log($"CustomerMovement {name} NavMeshAgent initialized - Speed: {movementSpeed}, Stopping Distance: {stoppingDistance}");
        }
        
        #endregion
        
        #region Movement and Pathfinding
        
        /// <summary>
        /// Set movement destination using NavMeshAgent with pathfinding validation
        /// </summary>
        /// <param name="destination">Target position to move to</param>
        /// <returns>True if destination was set successfully</returns>
        public bool SetDestination(Vector3 destination)
        {
            if (navMeshAgent == null)
            {
                Debug.LogWarning($"CustomerMovement {name} cannot move - NavMeshAgent not found!");
                return false;
            }
            
            // Check if destination is on NavMesh
            NavMeshHit hit;
            if (!NavMesh.SamplePosition(destination, out hit, 5f, NavMesh.AllAreas))
            {
                Debug.LogWarning($"CustomerMovement {name} destination not on NavMesh: {destination}");
                return HandlePathfindingFailure(destination);
            }
            
            bool pathSet = navMeshAgent.SetDestination(hit.position);
            
            if (pathSet)
            {
                currentDestination = hit.position;
                hasDestination = true;
                pathfindingRetries = 0;
                StartStuckDetection();
                Debug.Log($"CustomerMovement {name} moving to destination: {hit.position}");
            }
            else
            {
                Debug.LogWarning($"CustomerMovement {name} failed to set destination: {destination}");
                return HandlePathfindingFailure(destination);
            }
            
            return pathSet;
        }
        
        /// <summary>
        /// Set destination to a random shelf in the scene
        /// </summary>
        /// <returns>True if a random shelf destination was set successfully</returns>
        public bool SetRandomShelfDestination()
        {
            ShelfSlot[] availableShelves = FindObjectsByType<ShelfSlot>(FindObjectsSortMode.None);
            
            if (availableShelves.Length == 0)
            {
                Debug.LogWarning($"CustomerMovement {name} cannot find any shelves in scene!");
                return false;
            }
            
            // Select random shelf
            ShelfSlot randomShelf = availableShelves[Random.Range(0, availableShelves.Length)];
            
            // Move to shelf position
            return MoveToShelfPosition(randomShelf);
        }
        
        /// <summary>
        /// Move to a specific shelf position
        /// </summary>
        /// <param name="shelf">Target shelf to move to</param>
        /// <returns>True if movement was initiated successfully</returns>
        public bool MoveToShelfPosition(ShelfSlot shelf)
        {
            if (shelf == null)
            {
                Debug.LogWarning($"CustomerMovement {name} cannot move to null shelf!");
                return false;
            }
            
            // Calculate position in front of shelf
            Vector3 shelfPosition = shelf.transform.position;
            Vector3 shelfForward = shelf.transform.forward;
            Vector3 targetPosition = shelfPosition + shelfForward * 2f; // Stand 2 units in front
            
            return SetDestination(targetPosition);
        }
        
        /// <summary>
        /// Move to an exit point (for leaving the shop)
        /// </summary>
        /// <returns>True if exit destination was set successfully</returns>
        public bool MoveToExitPoint()
        {
            // Look for objects tagged as "Exit" or find a reasonable exit position
            GameObject[] exitPoints = GameObject.FindGameObjectsWithTag("Exit");
            
            if (exitPoints.Length > 0)
            {
                GameObject selectedExit = exitPoints[Random.Range(0, exitPoints.Length)];
                return SetDestination(selectedExit.transform.position);
            }
            else
            {
                // Fallback: move to edge of NavMesh (simple exit strategy)
                Vector3 exitPosition = transform.position + Vector3.forward * 10f;
                Debug.LogWarning($"CustomerMovement {name} no exit points found, using fallback position: {exitPosition}");
                return SetDestination(exitPosition);
            }
        }
        
        /// <summary>
        /// Move to checkout area
        /// </summary>
        /// <returns>True if checkout destination was set successfully</returns>
        public bool MoveToCheckoutPoint()
        {
            // Look for checkout area with more detailed search options
            GameObject checkout = null;
            
            // Try to find checkout by tag first (preferred method)
            try
            {
                GameObject[] checkouts = GameObject.FindGameObjectsWithTag("Checkout");
                if (checkouts.Length > 0)
                {
                    // If multiple checkouts exist, find the closest one
                    if (checkouts.Length > 1)
                    {
                        float closestDistance = float.MaxValue;
                        foreach (GameObject checkoutPoint in checkouts)
                        {
                            float distance = Vector3.Distance(transform.position, checkoutPoint.transform.position);
                            if (distance < closestDistance)
                            {
                                closestDistance = distance;
                                checkout = checkoutPoint;
                            }
                        }
                    }
                    else
                    {
                        checkout = checkouts[0];
                    }
                }
            }
            catch (UnityException)
            {
                // Checkout tag doesn't exist, that's okay
                Debug.Log($"CustomerMovement {name}: Checkout tag not defined, searching by name");
                
                // Try to find by name as fallback
                GameObject checkoutByName = GameObject.Find("Checkout");
                if (checkoutByName != null)
                {
                    checkout = checkoutByName;
                }
            }
            
            if (checkout != null)
            {
                Debug.Log($"CustomerMovement {name} moving to checkout at {checkout.transform.position}");
                return SetDestination(checkout.transform.position);
            }
            else
            {
                // Fallback: move to center of shop
                Vector3 shopCenter = Vector3.zero;
                Debug.Log($"CustomerMovement {name} no checkout found, using shop center at {shopCenter}");
                return SetDestination(shopCenter);
            }
        }
        
        /// <summary>
        /// Stop current movement and clear destination
        /// </summary>
        public void StopMovement()
        {
            if (navMeshAgent != null && navMeshAgent.isActiveAndEnabled)
            {
                navMeshAgent.ResetPath();
                hasDestination = false;
                StopStuckDetection();
                Debug.Log($"CustomerMovement {name} stopped movement");
            }
        }
        
        /// <summary>
        /// Check if customer has reached destination
        /// </summary>
        /// <returns>True if customer has reached their destination</returns>
        public bool HasReachedDestination()
        {
            if (navMeshAgent == null || !hasDestination) return true;
            
            // Check NavMeshAgent status
            bool navMeshReached = !navMeshAgent.pathPending && 
                                 navMeshAgent.remainingDistance < navMeshAgent.stoppingDistance;
            
            // Check direct distance to destination
            float distanceToDestination = Vector3.Distance(transform.position, currentDestination);
            bool directDistanceReached = distanceToDestination <= destinationReachedDistance;
            
            if (navMeshReached || directDistanceReached)
            {
                hasDestination = false;
                StopStuckDetection();
                Debug.Log($"CustomerMovement {name} reached destination: {currentDestination}");
                return true;
            }
            
            return false;
        }
        
        /// <summary>
        /// Update pathfinding state and handle issues
        /// </summary>
        public void UpdatePathfindingState()
        {
            if (navMeshAgent == null || !hasDestination) return;
            
            // Check for pathfinding status
            if (navMeshAgent.pathStatus == NavMeshPathStatus.PathInvalid)
            {
                Debug.LogWarning($"CustomerMovement {name} has invalid path!");
                HandlePathfindingFailure(currentDestination);
            }
            else if (navMeshAgent.pathStatus == NavMeshPathStatus.PathPartial)
            {
                Debug.LogWarning($"CustomerMovement {name} has partial path - may not reach exact destination");
            }
        }
        
        /// <summary>
        /// Update movement state monitoring
        /// </summary>
        public void UpdateMovementState()
        {
            if (navMeshAgent == null) return;
            
            // Debug movement information (can be removed in production)
            if (IsMoving && Time.frameCount % 60 == 0) // Log every 60 frames (~1 second)
            {
                Debug.Log($"CustomerMovement {name} moving - Remaining distance: {navMeshAgent.remainingDistance:F2}, " +
                         $"Destination: {currentDestination}");
            }
        }
        
        /// <summary>
        /// Handle pathfinding failures with retry logic
        /// </summary>
        /// <param name="originalDestination">The original destination that failed</param>
        /// <returns>True if a fallback solution was found</returns>
        private bool HandlePathfindingFailure(Vector3 originalDestination)
        {
            pathfindingRetries++;
            
            if (pathfindingRetries >= maxPathfindingRetries)
            {
                Debug.LogError($"CustomerMovement {name} exceeded pathfinding retry limit ({maxPathfindingRetries})");
                
                // Final fallback: stop and wait
                StopMovement();
                return false;
            }
            
            Debug.Log($"CustomerMovement {name} pathfinding retry {pathfindingRetries}/{maxPathfindingRetries}");
            
            // Try nearby positions
            for (int i = 0; i < 8; i++)
            {
                Vector3 randomOffset = Random.insideUnitSphere * 3f;
                randomOffset.y = 0; // Keep on ground level
                Vector3 fallbackDestination = originalDestination + randomOffset;
                
                NavMeshHit hit;
                if (NavMesh.SamplePosition(fallbackDestination, out hit, 2f, NavMesh.AllAreas))
                {
                    if (navMeshAgent.SetDestination(hit.position))
                    {
                        currentDestination = hit.position;
                        hasDestination = true;
                        Debug.Log($"CustomerMovement {name} using fallback destination: {hit.position}");
                        return true;
                    }
                }
            }
            
            // If all fallbacks fail, try again after delay
            StartCoroutine(RetryPathfindingAfterDelay(originalDestination));
            return false;
        }
        
        /// <summary>
        /// Retry pathfinding after a delay
        /// </summary>
        private IEnumerator RetryPathfindingAfterDelay(Vector3 destination)
        {
            yield return new WaitForSeconds(pathfindingRetryDelay);
            SetDestination(destination);
        }
        
        #endregion
        
        #region Stuck Detection
        
        /// <summary>
        /// Start stuck detection monitoring
        /// </summary>
        private void StartStuckDetection()
        {
            StopStuckDetection(); // Stop any existing detection
            lastPosition = transform.position;
            stuckTimer = 0f;
            stuckDetectionCoroutine = StartCoroutine(MonitorStuckState());
        }
        
        /// <summary>
        /// Stop stuck detection monitoring
        /// </summary>
        private void StopStuckDetection()
        {
            if (stuckDetectionCoroutine != null)
            {
                StopCoroutine(stuckDetectionCoroutine);
                stuckDetectionCoroutine = null;
            }
        }
        
        /// <summary>
        /// Monitor for stuck state and handle recovery
        /// </summary>
        private IEnumerator MonitorStuckState()
        {
            while (hasDestination)
            {
                yield return new WaitForSeconds(1f); // Check every second
                
                float distanceMoved = Vector3.Distance(transform.position, lastPosition);
                
                if (distanceMoved < stuckDistanceThreshold && IsMoving)
                {
                    stuckTimer += 1f;
                    
                    if (stuckTimer >= stuckDetectionTime)
                    {
                        Debug.LogWarning($"CustomerMovement {name} detected as stuck! Attempting recovery...");
                        HandleStuckCustomer();
                        break;
                    }
                }
                else
                {
                    stuckTimer = 0f; // Reset timer if moving properly
                }
                
                lastPosition = transform.position;
            }
        }
        
        /// <summary>
        /// Handle stuck customer with recovery strategies
        /// </summary>
        private void HandleStuckCustomer()
        {
            Debug.Log($"CustomerMovement {name} implementing stuck recovery");
            
            // Strategy 1: Try to find a new path to the same destination
            Vector3 originalDestination = currentDestination;
            StopMovement();
            
            // Wait briefly then try original destination again
            StartCoroutine(AttemptStuckRecovery(originalDestination));
        }
        
        /// <summary>
        /// Attempt recovery from stuck state
        /// </summary>
        private IEnumerator AttemptStuckRecovery(Vector3 originalDestination)
        {
            yield return new WaitForSeconds(2f);
            
            // Try moving to a nearby position first, then to original destination
            Vector3 nearbyPosition = transform.position + Random.insideUnitSphere * 3f;
            nearbyPosition.y = transform.position.y;
            
            if (SetDestination(nearbyPosition))
            {
                // Wait for movement to nearby position
                yield return new WaitForSeconds(3f);
                
                // Now try original destination again
                SetDestination(originalDestination);
            }
            else
            {
                // If that fails, try random shelf destination
                SetRandomShelfDestination();
            }
        }
        
        #endregion
        
        #region Legacy Field Migration
        
        /// <summary>
        /// Migrate legacy fields from main Customer component
        /// </summary>
        public void MigrateLegacyFields(float legacyMovementSpeed, float legacyStoppingDistance,
                                       float legacyStuckDetectionTime, float legacyStuckDistanceThreshold,
                                       float legacyDestinationReachedDistance, int legacyMaxPathfindingRetries,
                                       float legacyPathfindingRetryDelay)
        {
            movementSpeed = legacyMovementSpeed;
            stoppingDistance = legacyStoppingDistance;
            stuckDetectionTime = legacyStuckDetectionTime;
            stuckDistanceThreshold = legacyStuckDistanceThreshold;
            destinationReachedDistance = legacyDestinationReachedDistance;
            maxPathfindingRetries = legacyMaxPathfindingRetries;
            pathfindingRetryDelay = legacyPathfindingRetryDelay;
            
            // Apply new settings to NavMeshAgent if it exists
            if (navMeshAgent != null)
            {
                navMeshAgent.speed = movementSpeed;
                navMeshAgent.stoppingDistance = stoppingDistance;
            }
            
            Debug.Log("CustomerMovement: Legacy fields migrated successfully");
        }
        
        #endregion

        /// <summary>
        /// Move to a specific world position (used for queue positioning)
        /// </summary>
        /// <param name="position">Target world position</param>
        /// <returns>True if movement was initiated successfully</returns>
        public bool MoveToPosition(Vector3 position)
        {
            if (navMeshAgent == null)
            {
                Debug.LogError($"CustomerMovement {name} cannot move - NavMeshAgent not found!");
                return false;
            }
            
            if (!navMeshAgent.enabled)
            {
                Debug.LogWarning($"CustomerMovement {name} cannot move - NavMeshAgent is disabled!");
                return false;
            }
            
            // Check if position is on NavMesh
            NavMeshHit hit;
            if (!NavMesh.SamplePosition(position, out hit, 2f, NavMesh.AllAreas))
            {
                Debug.LogWarning($"CustomerMovement {name} cannot reach position {position} - not on NavMesh");
                return false;
            }
            
            bool pathSet = navMeshAgent.SetDestination(hit.position);
            
            if (pathSet)
            {
                currentDestination = hit.position;
                hasDestination = true;
                pathfindingRetries = 0;
                StartStuckDetection();
                Debug.Log($"CustomerMovement {name} moving to position: {hit.position}");
            }
            else
            {
                Debug.LogWarning($"CustomerMovement {name} failed to set destination: {position}");
            }
            
            return pathSet;
        }
    }
}
