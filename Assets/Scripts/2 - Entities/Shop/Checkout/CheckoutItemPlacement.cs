using UnityEngine;
using System.Collections.Generic;
using System.Collections;

namespace TabletopShop
{
    /// <summary>
    /// Handles spawning and placement of CheckoutItem GameObjects on the checkout counter surface
    /// Manages organized grid placement, collision detection, and item lifecycle
    /// </summary>
    public class CheckoutItemPlacement : MonoBehaviour
    {
        [Header("Placement Configuration")]
        [SerializeField] private GameObject checkoutItemPrefab;
        [SerializeField] private Transform placementSurface;
        [SerializeField] private Vector2 gridSize = new Vector2(3, 2); // 3x2 grid
        [SerializeField] private Vector2 itemSpacing = new Vector2(1.5f, 1.0f);
        [SerializeField] private Vector3 placementOffset = Vector3.zero;
        
        [Header("Item Management")]
        [SerializeField] private float itemHeight = 0.1f; // Height above surface
        [SerializeField] private float collisionCheckRadius = 0.5f;
        [SerializeField] private LayerMask collisionLayers = -1;
        [SerializeField] private bool useObjectPooling = true;
        [SerializeField] private int poolSize = 10;
        
        [Header("Visual Feedback")]
        [SerializeField] private Material placementPreviewMaterial;
        [SerializeField] private GameObject placementIndicatorPrefab;
        [SerializeField] private bool showPlacementGrid = false;
        
        [Header("Debug")]
        [SerializeField] private bool enableDebugLogging = true;
        [SerializeField] private bool showDebugGizmos = true;
        
        // State tracking
        private Vector3[,] gridPositions;
        private bool[,] occupiedPositions;
        private List<CheckoutItem> activeItems = new List<CheckoutItem>();
        private Queue<GameObject> itemPool = new Queue<GameObject>();
        private List<GameObject> placementIndicators = new List<GameObject>();
        
        // Component references
        private CheckoutCounter parentCheckoutCounter;
        
        // Properties
        public int MaxItems => (int)(gridSize.x * gridSize.y);
        public int CurrentItemCount => activeItems.Count;
        public bool HasAvailableSpace => CurrentItemCount < MaxItems;
        public Vector2 GridSize => gridSize;
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            InitializePlacement();
        }
        
        private void Start()
        {
            ValidateConfiguration();
            SetupPlacementGrid();
            
            if (useObjectPooling)
            {
                InitializeObjectPool();
            }
        }
        
        #endregion
        
        #region Initialization
        
        /// <summary>
        /// Initialize the placement system
        /// </summary>
        private void InitializePlacement()
        {
            // Get parent checkout counter
            parentCheckoutCounter = GetComponentInParent<CheckoutCounter>();
            
            // Use this transform as placement surface if none specified
            if (placementSurface == null)
            {
                placementSurface = transform;
            }
            
            if (enableDebugLogging)
            {
                Debug.Log($"CheckoutItemPlacement {name}: Initialized with {MaxItems} placement slots");
            }
        }
        
        /// <summary>
        /// Validate configuration and log warnings for missing components
        /// </summary>
        private void ValidateConfiguration()
        {
            if (checkoutItemPrefab == null)
            {
                Debug.LogWarning($"CheckoutItemPlacement {name}: No checkout item prefab assigned");
            }
            
            if (placementSurface == null)
            {
                Debug.LogWarning($"CheckoutItemPlacement {name}: No placement surface assigned, using this transform");
                placementSurface = transform;
            }
            
            if (gridSize.x <= 0 || gridSize.y <= 0)
            {
                Debug.LogWarning($"CheckoutItemPlacement {name}: Invalid grid size, using default 3x2");
                gridSize = new Vector2(3, 2);
            }
        }
        
        /// <summary>
        /// Setup the placement grid positions
        /// </summary>
        private void SetupPlacementGrid()
        {
            int gridX = (int)gridSize.x;
            int gridY = (int)gridSize.y;
            
            gridPositions = new Vector3[gridX, gridY];
            occupiedPositions = new bool[gridX, gridY];
            
            // Calculate grid starting position (centered)
            Vector3 startPos = placementSurface.position + placementOffset;
            startPos.x -= (gridX - 1) * itemSpacing.x * 0.5f;
            startPos.z -= (gridY - 1) * itemSpacing.y * 0.5f;
            startPos.y += itemHeight;
            
            // Generate grid positions
            for (int x = 0; x < gridX; x++)
            {
                for (int y = 0; y < gridY; y++)
                {
                    Vector3 gridPos = startPos + new Vector3(
                        x * itemSpacing.x,
                        0,
                        y * itemSpacing.y
                    );
                    
                    gridPositions[x, y] = gridPos;
                    occupiedPositions[x, y] = false;
                }
            }
            
            if (showPlacementGrid)
            {
                CreatePlacementIndicators();
            }
            
            if (enableDebugLogging)
            {
                Debug.Log($"CheckoutItemPlacement {name}: Setup {gridX}x{gridY} grid with {MaxItems} positions");
            }
        }
        
        /// <summary>
        /// Initialize object pool for checkout items
        /// </summary>
        private void InitializeObjectPool()
        {
            if (checkoutItemPrefab == null) return;
            
            for (int i = 0; i < poolSize; i++)
            {
                GameObject pooledItem = CreatePooledItem();
                if (pooledItem != null)
                {
                    itemPool.Enqueue(pooledItem);
                }
            }
            
            if (enableDebugLogging)
            {
                Debug.Log($"CheckoutItemPlacement {name}: Initialized object pool with {itemPool.Count} items");
            }
        }
        
        /// <summary>
        /// Create a pooled item GameObject
        /// </summary>
        /// <returns>The created pooled item</returns>
        private GameObject CreatePooledItem()
        {
            GameObject pooledItem = Instantiate(checkoutItemPrefab, transform);
            pooledItem.SetActive(false);
            
            // Ensure it has a CheckoutItem component
            if (pooledItem.GetComponent<CheckoutItem>() == null)
            {
                pooledItem.AddComponent<CheckoutItem>();
            }
            
            return pooledItem;
        }
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// Create and place a checkout item from product data
        /// </summary>
        /// <param name="productData">Product data for the checkout item</param>
        /// <param name="position">Optional specific position (if not provided, uses next available grid position)</param>
        /// <returns>The created CheckoutItem component, or null if failed</returns>
        public CheckoutItem CreateCheckoutItem(ProductData productData, Vector3? position = null)
        {
            if (productData == null)
            {
                Debug.LogWarning($"CheckoutItemPlacement {name}: Cannot create checkout item with null ProductData");
                return null;
            }
            
            if (!HasAvailableSpace && position == null)
            {
                Debug.LogWarning($"CheckoutItemPlacement {name}: No available space for new checkout item");
                return null;
            }
            
            // Get position
            Vector3 targetPosition;
            Vector2Int gridCoord = Vector2Int.zero;
            
            if (position.HasValue)
            {
                targetPosition = position.Value;
                gridCoord = GetGridCoordinateFromPosition(targetPosition);
            }
            else
            {
                gridCoord = GetNextAvailablePosition();
                if (gridCoord.x == -1) // No available position found
                {
                    Debug.LogWarning($"CheckoutItemPlacement {name}: No available grid position for checkout item");
                    return null;
                }
                targetPosition = gridPositions[gridCoord.x, gridCoord.y];
            }
            
            // Check for collisions at target position
            if (HasCollisionAtPosition(targetPosition))
            {
                Debug.LogWarning($"CheckoutItemPlacement {name}: Collision detected at position {targetPosition}");
                return null;
            }
            
            // Create the checkout item
            GameObject itemObject = GetOrCreateItemObject();
            if (itemObject == null)
            {
                Debug.LogError($"CheckoutItemPlacement {name}: Failed to create item object");
                return null;
            }
            
            // Position the item
            itemObject.transform.position = targetPosition;
            itemObject.transform.rotation = placementSurface.rotation;
            itemObject.SetActive(true);
            
            // Get or add CheckoutItem component
            CheckoutItem checkoutItem = itemObject.GetComponent<CheckoutItem>();
            if (checkoutItem == null)
            {
                checkoutItem = itemObject.AddComponent<CheckoutItem>();
            }
            
            // Initialize the checkout item
            checkoutItem.Initialize(productData, parentCheckoutCounter);
            
            // Mark grid position as occupied
            if (gridCoord.x >= 0 && gridCoord.x < gridSize.x && gridCoord.y >= 0 && gridCoord.y < gridSize.y)
            {
                occupiedPositions[gridCoord.x, gridCoord.y] = true;
            }
            
            // Add to active items list
            activeItems.Add(checkoutItem);
            
            if (enableDebugLogging)
            {
                Debug.Log($"CheckoutItemPlacement {name}: Created checkout item '{productData.ProductName}' at position {targetPosition}");
            }
            
            return checkoutItem;
        }
        
        /// <summary>
        /// Get the next available placement position in the grid
        /// </summary>
        /// <returns>World space position for next item, or Vector3.zero if no space available</returns>
        public Vector3 GetNextPlacementPosition()
        {
            Vector2Int gridCoord = GetNextAvailablePosition();
            if (gridCoord.x == -1)
            {
                return Vector3.zero;
            }
            
            return gridPositions[gridCoord.x, gridCoord.y];
        }
        
        /// <summary>
        /// Clear all items from the placement area
        /// </summary>
        public void ClearAllItems()
        {
            if (enableDebugLogging)
            {
                Debug.Log($"CheckoutItemPlacement {name}: Clearing {activeItems.Count} checkout items");
            }
            
            // Return items to pool or destroy them
            for (int i = activeItems.Count - 1; i >= 0; i--)
            {
                if (activeItems[i] != null)
                {
                    RemoveCheckoutItem(activeItems[i]);
                }
            }
            
            // Clear tracking arrays
            ResetGridOccupancy();
            activeItems.Clear();
            
            if (enableDebugLogging)
            {
                Debug.Log($"CheckoutItemPlacement {name}: All items cleared");
            }
        }
        
        /// <summary>
        /// Remove a specific checkout item
        /// </summary>
        /// <param name="checkoutItem">The checkout item to remove</param>
        public void RemoveCheckoutItem(CheckoutItem checkoutItem)
        {
            if (checkoutItem == null) return;
            
            // Find and free the grid position
            Vector2Int gridCoord = GetGridCoordinateFromPosition(checkoutItem.transform.position);
            if (gridCoord.x >= 0 && gridCoord.x < gridSize.x && gridCoord.y >= 0 && gridCoord.y < gridSize.y)
            {
                occupiedPositions[gridCoord.x, gridCoord.y] = false;
            }
            
            // Remove from active items
            activeItems.Remove(checkoutItem);
            
            // Return to pool or destroy
            if (useObjectPooling && itemPool.Count < poolSize)
            {
                ReturnItemToPool(checkoutItem.gameObject);
            }
            else
            {
                Destroy(checkoutItem.gameObject);
            }
            
            if (enableDebugLogging)
            {
                Debug.Log($"CheckoutItemPlacement {name}: Removed checkout item '{checkoutItem.ProductName}'");
            }
        }
        
        /// <summary>
        /// Get all currently placed checkout items
        /// </summary>
        /// <returns>List of active CheckoutItem components</returns>
        public List<CheckoutItem> GetAllPlacedItems()
        {
            return new List<CheckoutItem>(activeItems);
        }
        
        #endregion
        
        #region Private Methods
        
        /// <summary>
        /// Get or create an item object (from pool or new)
        /// </summary>
        /// <returns>GameObject for the checkout item</returns>
        private GameObject GetOrCreateItemObject()
        {
            if (useObjectPooling && itemPool.Count > 0)
            {
                return itemPool.Dequeue();
            }
            else if (checkoutItemPrefab != null)
            {
                GameObject newItem = Instantiate(checkoutItemPrefab, transform);
                
                // Ensure it has a CheckoutItem component
                if (newItem.GetComponent<CheckoutItem>() == null)
                {
                    newItem.AddComponent<CheckoutItem>();
                }
                
                return newItem;
            }
            
            return null;
        }
        
        /// <summary>
        /// Return an item to the object pool
        /// </summary>
        /// <param name="item">Item to return to pool</param>
        private void ReturnItemToPool(GameObject item)
        {
            if (item != null)
            {
                item.SetActive(false);
                item.transform.SetParent(transform);
                itemPool.Enqueue(item);
            }
        }
        
        /// <summary>
        /// Get the next available grid position
        /// </summary>
        /// <returns>Grid coordinates, or (-1, -1) if no space available</returns>
        private Vector2Int GetNextAvailablePosition()
        {
            for (int x = 0; x < gridSize.x; x++)
            {
                for (int y = 0; y < gridSize.y; y++)
                {
                    if (!occupiedPositions[x, y])
                    {
                        return new Vector2Int(x, y);
                    }
                }
            }
            
            return new Vector2Int(-1, -1); // No available position
        }
        
        /// <summary>
        /// Get grid coordinates from world position
        /// </summary>
        /// <param name="worldPosition">World space position</param>
        /// <returns>Grid coordinates, or (-1, -1) if outside grid</returns>
        private Vector2Int GetGridCoordinateFromPosition(Vector3 worldPosition)
        {
            for (int x = 0; x < gridSize.x; x++)
            {
                for (int y = 0; y < gridSize.y; y++)
                {
                    if (Vector3.Distance(worldPosition, gridPositions[x, y]) < itemSpacing.x * 0.5f)
                    {
                        return new Vector2Int(x, y);
                    }
                }
            }
            
            return new Vector2Int(-1, -1);
        }
        
        /// <summary>
        /// Check for collisions at a specific position
        /// </summary>
        /// <param name="position">Position to check</param>
        /// <returns>True if collision detected</returns>
        private bool HasCollisionAtPosition(Vector3 position)
        {
            Collider[] colliders = Physics.OverlapSphere(position, collisionCheckRadius, collisionLayers);
            
            // Filter out colliders that belong to the placement system itself
            foreach (Collider col in colliders)
            {
                if (col.transform.IsChildOf(transform) || col.transform == transform)
                    continue;
                    
                return true; // Found a collision
            }
            
            return false;
        }
        
        /// <summary>
        /// Reset all grid positions to unoccupied
        /// </summary>
        private void ResetGridOccupancy()
        {
            if (occupiedPositions != null)
            {
                for (int x = 0; x < gridSize.x; x++)
                {
                    for (int y = 0; y < gridSize.y; y++)
                    {
                        occupiedPositions[x, y] = false;
                    }
                }
            }
        }
        
        /// <summary>
        /// Create visual placement indicators for the grid
        /// </summary>
        private void CreatePlacementIndicators()
        {
            if (placementIndicatorPrefab == null) return;
            
            for (int x = 0; x < gridSize.x; x++)
            {
                for (int y = 0; y < gridSize.y; y++)
                {
                    GameObject indicator = Instantiate(placementIndicatorPrefab, gridPositions[x, y], Quaternion.identity, transform);
                    indicator.name = $"PlacementIndicator_{x}_{y}";
                    placementIndicators.Add(indicator);
                }
            }
        }
        
        #endregion
        
        #region Debug and Gizmos
        
        /// <summary>
        /// Draw debug gizmos for placement grid
        /// </summary>
        private void OnDrawGizmos()
        {
            if (!showDebugGizmos) return;
            
            if (gridPositions != null)
            {
                // Draw grid positions
                for (int x = 0; x < gridSize.x; x++)
                {
                    for (int y = 0; y < gridSize.y; y++)
                    {
                        Vector3 pos = gridPositions[x, y];
                        
                        // Color based on occupancy
                        if (Application.isPlaying && occupiedPositions != null)
                        {
                            Gizmos.color = occupiedPositions[x, y] ? Color.red : Color.green;
                        }
                        else
                        {
                            Gizmos.color = Color.yellow;
                        }
                        
                        Gizmos.DrawWireSphere(pos, collisionCheckRadius * 0.5f);
                        Gizmos.DrawWireCube(pos, Vector3.one * 0.1f);
                    }
                }
                
                // Draw placement surface bounds
                Gizmos.color = Color.blue;
                Vector3 surfaceCenter = placementSurface != null ? placementSurface.position : transform.position;
                Vector3 surfaceSize = new Vector3(
                    (gridSize.x - 1) * itemSpacing.x + 1,
                    0.1f,
                    (gridSize.y - 1) * itemSpacing.y + 1
                );
                Gizmos.DrawWireCube(surfaceCenter + placementOffset, surfaceSize);
            }
        }
        
        /// <summary>
        /// Test method for creating sample checkout items
        /// </summary>
        [ContextMenu("Test Create Sample Items")]
        private void TestCreateSampleItems()
        {
            if (!Application.isPlaying) return;
            
            // This would need actual ProductData assets to work
            // For testing, you could create some sample ProductData ScriptableObjects
            Debug.Log($"CheckoutItemPlacement {name}: Test method called - needs ProductData assets to create items");
        }
        
        /// <summary>
        /// Debug method to log current placement state
        /// </summary>
        [ContextMenu("Log Placement State")]
        private void LogPlacementState()
        {
            Debug.Log($"CheckoutItemPlacement {name} State:");
            Debug.Log($"  Grid Size: {gridSize.x}x{gridSize.y}");
            Debug.Log($"  Max Items: {MaxItems}");
            Debug.Log($"  Current Items: {CurrentItemCount}");
            Debug.Log($"  Available Space: {HasAvailableSpace}");
            Debug.Log($"  Object Pool Count: {(useObjectPooling ? itemPool.Count : "Not using pool")}");
        }
        
        #endregion
    }
}
