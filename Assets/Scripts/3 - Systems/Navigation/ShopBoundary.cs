using UnityEngine;

namespace TabletopShop
{
    /// <summary>
    /// Defines the shop boundary area where customers should shop
    /// Used to determine if customers are inside the shop
    /// </summary>
    public class ShopBoundary : MonoBehaviour
    {
        [Header("Boundary Settings")]
        [SerializeField] private bool showGizmos = true;
        [SerializeField] private Color gizmoColor = new Color(0f, 1f, 0f, 0.3f);
        
        private static ShopBoundary _instance;
        public static ShopBoundary Instance => _instance;
        
        private Collider boundaryCollider;
        
        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                boundaryCollider = GetComponent<Collider>();
                
                // Ensure the collider is a trigger
                if (boundaryCollider != null)
                {
                    boundaryCollider.isTrigger = true;
                }
                else
                {
                    Debug.LogWarning("[ShopBoundary] No collider found! Please add a collider to define the shop boundary.");
                }
            }
            else if (_instance != this)
            {
                Debug.LogWarning("[ShopBoundary] Multiple ShopBoundary instances found! Destroying duplicate.");
                Destroy(gameObject);
            }
        }
        
        /// <summary>
        /// Check if a position is within the shop boundary
        /// </summary>
        /// <param name="position">Position to check</param>
        /// <returns>True if position is inside shop boundary</returns>
        public bool IsPositionInShop(Vector3 position)
        {
            if (boundaryCollider == null)
            {
                Debug.LogWarning("[ShopBoundary] No boundary collider set!");
                return true; // Assume inside if no boundary defined
            }
            
            return boundaryCollider.bounds.Contains(position);
        }
        
        /// <summary>
        /// Check if a GameObject is within the shop boundary
        /// </summary>
        /// <param name="gameObject">GameObject to check</param>
        /// <returns>True if GameObject is inside shop boundary</returns>
        public bool IsObjectInShop(GameObject gameObject)
        {
            if (gameObject == null) return false;
            return IsPositionInShop(gameObject.transform.position);
        }
        
        /// <summary>
        /// Get the center point of the shop boundary
        /// </summary>
        /// <returns>Center position of the shop</returns>
        public Vector3 GetShopCenter()
        {
            if (boundaryCollider != null)
                return boundaryCollider.bounds.center;
            
            return transform.position;
        }
        
        /// <summary>
        /// Find a random point inside the shop boundary
        /// </summary>
        /// <returns>Random position inside shop, or shop center if no boundary</returns>
        public Vector3 GetRandomPointInShop()
        {
            if (boundaryCollider == null)
                return GetShopCenter();
            
            Bounds bounds = boundaryCollider.bounds;
            Vector3 randomPoint;
            int attempts = 0;
            const int maxAttempts = 10;
            
            do
            {
                randomPoint = new Vector3(
                    Random.Range(bounds.min.x, bounds.max.x),
                    bounds.center.y,
                    Random.Range(bounds.min.z, bounds.max.z)
                );
                attempts++;
            }
            while (!IsPositionInShop(randomPoint) && attempts < maxAttempts);
            
            return randomPoint;
        }
        
        private void OnDrawGizmos()
        {
            if (!showGizmos) return;
            
            if (boundaryCollider != null)
            {
                Gizmos.color = gizmoColor;
                Gizmos.DrawCube(boundaryCollider.bounds.center, boundaryCollider.bounds.size);
                
                // Draw wireframe
                Gizmos.color = new Color(gizmoColor.r, gizmoColor.g, gizmoColor.b, 1f);
                Gizmos.DrawWireCube(boundaryCollider.bounds.center, boundaryCollider.bounds.size);
            }
        }
    }
}