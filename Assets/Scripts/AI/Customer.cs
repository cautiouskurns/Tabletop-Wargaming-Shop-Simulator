using UnityEngine;
using UnityEngine.AI;
using System.Collections;

namespace TabletopShop
{
    /// <summary>
    /// Customer state enumeration for managing customer behavior
    /// </summary>
    public enum CustomerState
    {
        Entering,    // Customer is entering the shop
        Shopping,    // Customer is browsing/looking at products
        Purchasing,  // Customer is at checkout/purchase area
        Leaving      // Customer is exiting the shop
    }

    /// <summary>
    /// Customer AI controller with basic state management and NavMeshAgent movement
    /// Handles customer behavior states and movement within the shop
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    public class Customer : MonoBehaviour
    {
        [Header("Customer State")]
        [SerializeField] private CustomerState currentState = CustomerState.Entering;
        
        [Header("Shopping Configuration")]
        [SerializeField] private float shoppingTime;
        [SerializeField] private ShelfSlot targetShelf;
        
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
        public CustomerState CurrentState => currentState;
        public float ShoppingTime => shoppingTime;
        public ShelfSlot TargetShelf => targetShelf;
        public bool IsMoving => navMeshAgent != null && navMeshAgent.velocity.magnitude > 0.1f;
        public Vector3 CurrentDestination => currentDestination;
        public bool HasDestination => hasDestination;
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            InitializeNavMeshAgent();
            InitializeShoppingTime();
        }
        
        private void Start()
        {
            Debug.Log($"Customer {name} initialized with state: {currentState}, shopping time: {shoppingTime:F1}s");
        }
        
        private void Update()
        {
            // Basic movement and state monitoring
            UpdateMovementState();
            UpdatePathfindingState();
        }
        
        #endregion
        
        #region Initialization
        
        /// <summary>
        /// Initialize NavMeshAgent component and configure movement settings
        /// </summary>
        private void InitializeNavMeshAgent()
        {
            navMeshAgent = GetComponent<NavMeshAgent>();
            
            if (navMeshAgent == null)
            {
                Debug.LogError($"Customer {name} is missing NavMeshAgent component!");
                return;
            }
            
            // Configure NavMeshAgent settings
            navMeshAgent.speed = movementSpeed;
            navMeshAgent.stoppingDistance = stoppingDistance;
            navMeshAgent.autoBraking = true;
            navMeshAgent.autoRepath = true;
            
            Debug.Log($"Customer {name} NavMeshAgent initialized - Speed: {movementSpeed}, Stopping Distance: {stoppingDistance}");
        }
        
        /// <summary>
        /// Initialize random shopping time between 10-30 seconds
        /// </summary>
        private void InitializeShoppingTime()
        {
            shoppingTime = Random.Range(10f, 30f);
        }
        
        #endregion
        
        #region State Management
        
        /// <summary>
        /// Change customer state and log the transition
        /// </summary>
        /// <param name="newState">The new state to transition to</param>
        public void ChangeState(CustomerState newState)
        {
            CustomerState previousState = currentState;
            currentState = newState;
            
            Debug.Log($"Customer {name} state changed: {previousState} -> {currentState}");
            
            // Handle state-specific initialization
            OnStateChanged(previousState, currentState);
        }
        
        /// <summary>
        /// Get the current customer state
        /// </summary>
        /// <returns>Current CustomerState</returns>
        public CustomerState GetCurrentState()
        {
            return currentState;
        }
        
        /// <summary>
        /// Called when state changes to handle state-specific setup
        /// </summary>
        /// <param name="previousState">The previous state</param>
        /// <param name="newState">The new state</param>
        private void OnStateChanged(CustomerState previousState, CustomerState newState)
        {
            switch (newState)
            {
                case CustomerState.Entering:
                    Debug.Log($"Customer {name} is entering the shop");
                    break;
                    
                case CustomerState.Shopping:
                    Debug.Log($"Customer {name} started shopping (duration: {shoppingTime:F1}s)");
                    break;
                    
                case CustomerState.Purchasing:
                    Debug.Log($"Customer {name} is making a purchase");
                    break;
                    
                case CustomerState.Leaving:
                    Debug.Log($"Customer {name} is leaving the shop");
                    break;
            }
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
                Debug.LogWarning($"Customer {name} cannot move - NavMeshAgent not found!");
                return false;
            }
            
            // Check if destination is on NavMesh
            NavMeshHit hit;
            if (!NavMesh.SamplePosition(destination, out hit, 5f, NavMesh.AllAreas))
            {
                Debug.LogWarning($"Customer {name} destination not on NavMesh: {destination}");
                return HandlePathfindingFailure(destination);
            }
            
            bool pathSet = navMeshAgent.SetDestination(hit.position);
            
            if (pathSet)
            {
                currentDestination = hit.position;
                hasDestination = true;
                pathfindingRetries = 0;
                StartStuckDetection();
                Debug.Log($"Customer {name} moving to destination: {hit.position}");
            }
            else
            {
                Debug.LogWarning($"Customer {name} failed to set destination: {destination}");
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
                Debug.LogWarning($"Customer {name} cannot find any shelves in scene!");
                return false;
            }
            
            // Select random shelf
            ShelfSlot randomShelf = availableShelves[Random.Range(0, availableShelves.Length)];
            SetTargetShelf(randomShelf);
            
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
                Debug.LogWarning($"Customer {name} cannot move to null shelf!");
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
                Debug.LogWarning($"Customer {name} no exit points found, using fallback position: {exitPosition}");
                return SetDestination(exitPosition);
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
                Debug.Log($"Customer {name} stopped movement");
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
                Debug.Log($"Customer {name} reached destination: {currentDestination}");
                return true;
            }
            
            return false;
        }
        
        /// <summary>
        /// Update pathfinding state and handle issues
        /// </summary>
        private void UpdatePathfindingState()
        {
            if (navMeshAgent == null || !hasDestination) return;
            
            // Check for pathfinding status
            if (navMeshAgent.pathStatus == NavMeshPathStatus.PathInvalid)
            {
                Debug.LogWarning($"Customer {name} has invalid path!");
                HandlePathfindingFailure(currentDestination);
            }
            else if (navMeshAgent.pathStatus == NavMeshPathStatus.PathPartial)
            {
                Debug.LogWarning($"Customer {name} has partial path - may not reach exact destination");
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
                Debug.LogError($"Customer {name} exceeded pathfinding retry limit ({maxPathfindingRetries})");
                
                // Final fallback: stop and wait
                StopMovement();
                return false;
            }
            
            Debug.Log($"Customer {name} pathfinding retry {pathfindingRetries}/{maxPathfindingRetries}");
            
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
                        Debug.Log($"Customer {name} using fallback destination: {hit.position}");
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
                        Debug.LogWarning($"Customer {name} detected as stuck! Attempting recovery...");
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
            Debug.Log($"Customer {name} implementing stuck recovery");
            
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
        
        /// <summary>
        /// Update movement state monitoring
        /// </summary>
        private void UpdateMovementState()
        {
            if (navMeshAgent == null) return;
            
            // Debug movement information (can be removed in production)
            if (IsMoving && Time.frameCount % 60 == 0) // Log every 60 frames (~1 second)
            {
                Debug.Log($"Customer {name} moving - Remaining distance: {navMeshAgent.remainingDistance:F2}, " +
                         $"Destination: {currentDestination}");
            }
        }
        
        #region Target Management
        
        /// <summary>
        /// Set target shelf for shopping
        /// </summary>
        /// <param name="shelf">Target shelf slot</param>
        public void SetTargetShelf(ShelfSlot shelf)
        {
            targetShelf = shelf;
            
            if (shelf != null)
            {
                Debug.Log($"Customer {name} targeting shelf: {shelf.name}");
            }
            else
            {
                Debug.Log($"Customer {name} cleared target shelf");
            }
        }
        
        /// <summary>
        /// Clear current target shelf
        /// </summary>
        public void ClearTargetShelf()
        {
            SetTargetShelf(null);
        }
        
        #endregion
        
        #region Public Getters
        
        /// <summary>
        /// Get NavMeshAgent component reference
        /// </summary>
        /// <returns>NavMeshAgent component</returns>
        public NavMeshAgent GetNavMeshAgent()
        {
            return navMeshAgent;
        }
        
        /// <summary>
        /// Check if customer is currently in a specific state
        /// </summary>
        /// <param name="state">State to check</param>
        /// <returns>True if customer is in the specified state</returns>
        public bool IsInState(CustomerState state)
        {
            return currentState == state;
        }
        
        #endregion
        
        #region Debug and Visualization
        
        /// <summary>
        /// Get debug information about customer state
        /// </summary>
        /// <returns>Debug string with customer information</returns>
        public string GetDebugInfo()
        {
            return $"Customer {name}: State={currentState}, " +
                   $"ShoppingTime={shoppingTime:F1}s, " +
                   $"IsMoving={IsMoving}, " +
                   $"HasDestination={hasDestination}, " +
                   $"Destination={currentDestination}, " +
                   $"Distance={Vector3.Distance(transform.position, currentDestination):F2}, " +
                   $"TargetShelf={targetShelf?.name ?? "None"}";
        }
        
        /// <summary>
        /// Draw debug gizmos for destination and pathfinding
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            if (hasDestination)
            {
                // Draw destination
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(currentDestination, 0.5f);
                
                // Draw line to destination
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(transform.position, currentDestination);
                
                // Draw stopping distance
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(currentDestination, destinationReachedDistance);
            }
            
            // Draw NavMeshAgent path if available
            if (navMeshAgent != null && navMeshAgent.hasPath)
            {
                Gizmos.color = Color.blue;
                Vector3[] path = navMeshAgent.path.corners;
                for (int i = 0; i < path.Length - 1; i++)
                {
                    Gizmos.DrawLine(path[i], path[i + 1]);
                }
            }
        }
        
        #endregion
        
        #region Unity Editor Support
        
        private void OnValidate()
        {
            // Ensure valid ranges in editor
            movementSpeed = Mathf.Max(0.1f, movementSpeed);
            stoppingDistance = Mathf.Max(0.1f, stoppingDistance);
            shoppingTime = Mathf.Max(1f, shoppingTime);
        }
        
        #endregion
    }
}
