using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TabletopShop
{
    /// <summary>
    /// Handles visual representation and rendering for shelf including material management and duplicate cleanup
    /// Extracted from Shelf.cs to follow single responsibility principle
    /// </summary>
    public class ShelfVisuals : MonoBehaviour
    {
    [Header("Visual Settings")]
    [SerializeField] private Material shelfMaterial;
    [SerializeField] private Vector3 shelfDimensions = new Vector3(7.5f, 0.02f, 1f);
        
        [Header("Debug")]
        [SerializeField] private bool showShelfGizmos = true;
        
        // Runtime references
        private GameObject shelfVisual;
        private MeshRenderer shelfRenderer;
        
        // Public accessors
        public Material ShelfMaterial => shelfMaterial;
        public Vector3 ShelfDimensions => shelfDimensions;
        public bool ShowShelfGizmos => showShelfGizmos;
        public GameObject ShelfVisual => shelfVisual;
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            SetupShelfVisual();
        }
        
        private void OnDestroy()
        {
            // Clean up any created materials
            if (shelfRenderer != null && shelfRenderer.material != null)
            {
                // Only destroy materials we created (not assigned ones)
                if (shelfMaterial == null && !IsAssetMaterial(shelfRenderer.material))
                {
                    MaterialUtility.SafeDestroyMaterial(shelfRenderer.material, !Application.isPlaying);
                }
            }
        }
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// Initialize component with visual settings
        /// </summary>
        /// <param name="material">Material for the shelf visual</param>
        /// <param name="dimensions">Dimensions of the shelf visual</param>
        /// <param name="showGizmos">Whether to show gizmos</param>
        public void InitializeComponent(Material material = null, Vector3? dimensions = null, bool? showGizmos = null)
        {
            if (material != null)
            {
                shelfMaterial = material;
            }
            
            if (dimensions.HasValue)
            {
                shelfDimensions = dimensions.Value;
            }
            
            if (showGizmos.HasValue)
            {
                showShelfGizmos = showGizmos.Value;
            }
            
            // Re-setup with new values
            SetupShelfVisual();
        }
        
        /// <summary>
        /// Update the shelf visual with current settings
        /// </summary>
        public void UpdateVisual()
        {
            if (shelfVisual != null)
            {
                shelfVisual.transform.localScale = shelfDimensions;
                
                if (shelfRenderer != null && shelfMaterial != null)
                {
                    shelfRenderer.material = shelfMaterial;
                }
            }
        }
        
        /// <summary>
        /// Set whether to show shelf gizmos
        /// </summary>
        /// <param name="enabled">True to show gizmos</param>
        public void SetGizmoDrawing(bool enabled)
        {
            showShelfGizmos = enabled;
        }
        
        /// <summary>
        /// Get the bounds of the shelf visual for layout calculations
        /// </summary>
        /// <returns>Bounds of the shelf visual</returns>
        public Bounds GetShelfBounds()
        {
            if (shelfRenderer != null)
            {
                return shelfRenderer.bounds;
            }
            
            // Fallback to calculated bounds
            return new Bounds(transform.position, shelfDimensions);
        }
        
        #endregion
        
        #region Private Methods
        
        /// <summary>
        /// Setup the visual representation of the shelf
        /// </summary>
        private void SetupShelfVisual()
        {
            // Check if shelf already has a visual representation
            MeshRenderer existingRenderer = GetComponent<MeshRenderer>();
            if (existingRenderer != null)
            {
                shelfRenderer = existingRenderer;
                return;
            }
            
            // Check for and clean up any duplicate ShelfVisual children
            var existingVisuals = new List<Transform>();
            for (int i = 0; i < transform.childCount; i++)
            {
                Transform child = transform.GetChild(i);
                if (child.name == "ShelfVisual")
                {
                    existingVisuals.Add(child);
                }
            }
            
            // If we have duplicates, remove all but the first one
            if (existingVisuals.Count > 1)
            {
                Debug.LogWarning($"Found {existingVisuals.Count} duplicate ShelfVisual objects on shelf {name}, cleaning up...");
                for (int i = 1; i < existingVisuals.Count; i++)
                {
                    if (Application.isPlaying)
                    {
                        Destroy(existingVisuals[i].gameObject);
                    }
                    else
                    {
                        DestroyImmediate(existingVisuals[i].gameObject);
                    }
                }
                Debug.Log($"Cleaned up {existingVisuals.Count - 1} duplicate ShelfVisual objects");
            }
            
            // If we already have one ShelfVisual, use it
            if (existingVisuals.Count > 0)
            {
                shelfVisual = existingVisuals[0].gameObject;
                shelfRenderer = shelfVisual.GetComponent<MeshRenderer>();
                Debug.Log($"ShelfVisual already exists for shelf {name}, using existing");
                return;
            }
            
            // Create shelf visual
            shelfVisual = GameObject.CreatePrimitive(PrimitiveType.Cube);
            shelfVisual.name = "ShelfVisual";
            shelfVisual.transform.SetParent(transform, false);
            shelfVisual.transform.localPosition = Vector3.zero;
            shelfVisual.transform.localScale = shelfDimensions;
            
            // Get renderer reference
            shelfRenderer = shelfVisual.GetComponent<MeshRenderer>();
            
            // Apply material if available
            if (shelfMaterial != null && shelfRenderer != null)
            {
                shelfRenderer.material = shelfMaterial;
            }
            else if (shelfRenderer != null)
            {
                // Create default shelf material using MaterialUtility
                Material defaultMaterial = MaterialUtility.CreateStandardMaterial(
                    new Color(0.6f, 0.4f, 0.2f, 1f), // Brown wood color
                    0f, // metallic
                    0.3f // smoothness
                );
                shelfRenderer.material = defaultMaterial;
                Debug.Log($"Created default material for shelf {name}");
            }
            
            // Remove collider from visual (slots handle interactions)
            Collider visualCollider = shelfVisual.GetComponent<Collider>();
            if (visualCollider != null)
            {
                if (Application.isPlaying)
                {
                    Destroy(visualCollider);
                }
                else
                {
                    DestroyImmediate(visualCollider);
                }
            }
            
            Debug.Log($"Created ShelfVisual for shelf {name}");
        }
        
        /// <summary>
        /// Check if a material is an asset (not runtime created)
        /// </summary>
        /// <param name="material">Material to check</param>
        /// <returns>True if material is an asset</returns>
        private bool IsAssetMaterial(Material material)
        {
            if (material == null) return false;
            
#if UNITY_EDITOR
            return AssetDatabase.Contains(material);
#else
            // In runtime, assume materials with specific naming patterns are assets
            return !material.name.Contains("(Instance)") && !material.name.StartsWith("New Material");
#endif
        }
        
        #endregion
        
        #region Editor Support
        
        /// <summary>
        /// Draw gizmos for shelf visualization
        /// </summary>
        private void OnDrawGizmos()
        {
            if (!showShelfGizmos) return;
            
            // Draw shelf bounds
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(transform.position, shelfDimensions);
            
            // Draw shelf label
            if (shelfVisual != null)
            {
#if UNITY_EDITOR
                UnityEditor.Handles.Label(transform.position + Vector3.up * (shelfDimensions.y + 0.2f), 
                    $"Shelf: {name}");
#endif
            }
        }
        
        /// <summary>
        /// Validation in editor
        /// </summary>
        private void OnValidate()
        {
            // Ensure minimum dimensions
            if (shelfDimensions.x < 0.1f) shelfDimensions.x = 0.1f;
            if (shelfDimensions.y < 0.01f) shelfDimensions.y = 0.01f;
            if (shelfDimensions.z < 0.1f) shelfDimensions.z = 0.1f;
            
            // Update visual if it exists
            if (shelfVisual != null)
            {
                UpdateVisual();
            }
        }
        
        #endregion
    }
}
