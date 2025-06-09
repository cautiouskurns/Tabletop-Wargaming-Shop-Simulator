using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TabletopShop
{
    /// <summary>
    /// Handles visual feedback and rendering for shelf slots including materials, highlighting, and slot indicators
    /// </summary>
    public class ShelfSlotVisuals : MonoBehaviour
    {
        [Header("Visual Feedback")]
        [SerializeField] private Material highlightMaterial;
        [SerializeField] private Material normalMaterial;
        [SerializeField] private Color emptySlotColor = Color.green;
        [SerializeField] private Color highlightColor = Color.yellow;
        
        [Header("Slot Indicator")]
        [SerializeField] private GameObject slotIndicator;
        [SerializeField] private Vector3 indicatorScale = new Vector3(0.2f, 0.01f, 0.2f);
        [SerializeField] private bool preservePrefabScale = true; // New field to control scale preservation
        
        // Runtime storage for the actual scale to use
        private Vector3 actualIndicatorScale;
        
        // Component references
        private MeshRenderer indicatorRenderer;
        private ShelfSlotLogic slotLogic;
        
        // Public accessors for configuration
        public GameObject SlotIndicator => slotIndicator;
        public Material HighlightMaterial => highlightMaterial;
        public Material NormalMaterial => normalMaterial;
        
        private void Awake()
        {
            // Get reference to logic component
            slotLogic = GetComponent<ShelfSlotLogic>();
            
            SetupSlotIndicator();
        }
        
        private void Start()
        {
            // Subscribe to logic events
            if (slotLogic != null)
            {
                slotLogic.OnVisualStateChanged += UpdateVisualState;
            }
            
            UpdateVisualState();
        }
        
        private void OnDestroy()
        {
            // Unsubscribe from events
            if (slotLogic != null)
            {
                slotLogic.OnVisualStateChanged -= UpdateVisualState;
            }
            
            // Clean up created materials
            if (Application.isPlaying)
            {
                if (normalMaterial != null) Destroy(normalMaterial);
                if (highlightMaterial != null) Destroy(highlightMaterial);
            }
            else
            {
                if (normalMaterial != null) DestroyImmediate(normalMaterial);
                if (highlightMaterial != null) DestroyImmediate(highlightMaterial);
            }
        }
        
        /// <summary>
        /// Setup the visual indicator for the slot
        /// </summary>
        private void SetupSlotIndicator()
        {
            bool isPrefabReference = false;
            
            // Check if slotIndicator is a prefab reference
            if (slotIndicator != null)
            {
#if UNITY_EDITOR
                // In editor, use PrefabUtility to detect prefabs
                isPrefabReference = PrefabUtility.IsPartOfPrefabAsset(slotIndicator);
#else
                // In runtime, check if it has no parent and no scene (typical of prefab references)
                isPrefabReference = slotIndicator.transform.parent == null && 
                                   slotIndicator.gameObject.scene.name == null;
#endif
            }
            
            // If a prefab is assigned in inspector, instantiate it
            if (slotIndicator != null && isPrefabReference)
            {
                // Store the prefab's original scale before instantiation
                Vector3 prefabScale = slotIndicator.transform.localScale;
                
                // This is a prefab reference, instantiate it
                GameObject prefabInstance = Instantiate(slotIndicator);
                slotIndicator = prefabInstance;
                slotIndicator.name = $"{name}_Indicator";
                slotIndicator.transform.SetParent(transform, false);
                
                // Use prefab scale if preservePrefabScale is true, otherwise use indicatorScale
                actualIndicatorScale = preservePrefabScale ? prefabScale : indicatorScale;
                
                // Position and scale the indicator
                UpdateIndicatorTransform();
                
                Debug.Log($"Instantiated prefab indicator for slot {name} with scale {actualIndicatorScale}");
            }
            // Create slot indicator if it doesn't exist
            else if (slotIndicator == null)
            {
                slotIndicator = GameObject.CreatePrimitive(PrimitiveType.Cube);
                slotIndicator.name = $"{name}_Indicator";
                slotIndicator.transform.SetParent(transform, false);
                
                // Use the indicatorScale for created primitives
                actualIndicatorScale = indicatorScale;
                
                // Position and scale the indicator
                UpdateIndicatorTransform();
                
                Debug.Log($"Created default cube indicator for slot {name} with scale {actualIndicatorScale}");
                
                // Remove collider from indicator (we want slot collider to handle interactions)
                Collider indicatorCollider = slotIndicator.GetComponent<Collider>();
                if (indicatorCollider != null)
                {
                    DestroyImmediate(indicatorCollider);
                }
            }
            else
            {
                // Indicator already exists in scene, preserve its current scale
                actualIndicatorScale = slotIndicator.transform.localScale;
                
                // Just ensure proper positioning
                UpdateIndicatorTransform();
                Debug.Log($"Using existing scene indicator for slot {name} with preserved scale {actualIndicatorScale}");
            }
            
            // Remove any colliders from the indicator (we want slot collider to handle interactions)
            Collider[] indicatorColliders = slotIndicator.GetComponents<Collider>();
            foreach (Collider col in indicatorColliders)
            {
                if (Application.isPlaying)
                {
                    Destroy(col);
                }
                else
                {
                    DestroyImmediate(col);
                }
            }
            
            // Get renderer for material changes
            indicatorRenderer = slotIndicator.GetComponent<MeshRenderer>();
            
            Debug.Log($"Slot {name} indicator setup complete. Renderer found: {indicatorRenderer != null}");
            
            // Setup materials after renderer is assigned
            SetupMaterials();
        }
        
        /// <summary>
        /// Update indicator transform based on slot position
        /// </summary>
        private void UpdateIndicatorTransform()
        {
            if (slotIndicator != null && slotLogic != null)
            {
                Vector3 localSlotPosition = slotLogic.SlotPosition - transform.position;
                slotIndicator.transform.localPosition = localSlotPosition;
                
                // Use actualIndicatorScale if it's been set, otherwise fall back to indicatorScale
                Vector3 scaleToUse = actualIndicatorScale != Vector3.zero ? actualIndicatorScale : indicatorScale;
                slotIndicator.transform.localScale = scaleToUse;
            }
        }
        
        /// <summary>
        /// Setup materials for normal and highlight states
        /// </summary>
        private void SetupMaterials()
        {
            if (indicatorRenderer == null) return;
            
            // If materials are not assigned in inspector, create default ones OR preserve prefab material
            if (normalMaterial == null)
            {
                // Check if the renderer already has a material (from prefab)
                if (indicatorRenderer.material != null && indicatorRenderer.material.name != "Default-Material")
                {
                    // Use the prefab's material as the normal material
                    normalMaterial = indicatorRenderer.material;
                    Debug.Log($"Using prefab material as normal material for slot {name}");
                }
                else
                {
                    // Create default material for generated cube indicators
                    normalMaterial = new Material(Shader.Find("Standard"));
                    normalMaterial.color = emptySlotColor;
                    normalMaterial.SetFloat("_Metallic", 0f);
                    normalMaterial.SetFloat("_Glossiness", 0.3f);
                    Debug.Log($"Created default normal material for slot {name}");
                }
            }
            
            // Create highlight material if not assigned
            if (highlightMaterial == null)
            {
                // Create a highlight version based on the normal material
                highlightMaterial = new Material(normalMaterial);
                highlightMaterial.color = highlightColor;
                highlightMaterial.SetFloat("_Metallic", 0f);
                highlightMaterial.SetFloat("_Glossiness", 0.5f);
                
                // Add emission for better visibility
                highlightMaterial.EnableKeyword("_EMISSION");
                highlightMaterial.SetColor("_EmissionColor", highlightColor * 0.3f);
                Debug.Log($"Created highlight material for slot {name}");
            }
        }
        
        /// <summary>
        /// Update visual state based on slot contents
        /// </summary>
        public void UpdateVisualState()
        {
            if (slotIndicator == null || slotLogic == null) return;
            
            // Show indicator only when slot is empty
            slotIndicator.SetActive(slotLogic.IsEmpty);
            
            // Apply normal material when not highlighted (only if we have a MeshRenderer)
            if (slotLogic.IsEmpty && indicatorRenderer != null && normalMaterial != null)
            {
                indicatorRenderer.material = normalMaterial;
            }
            
            // Update indicator position in case slot position changed
            UpdateIndicatorTransform();
        }
        
        /// <summary>
        /// Apply highlight effect
        /// </summary>
        public void ApplyHighlight()
        {
            if (slotLogic != null && slotLogic.IsEmpty && indicatorRenderer != null && highlightMaterial != null)
            {
                indicatorRenderer.material = highlightMaterial;
            }
        }
        
        /// <summary>
        /// Remove highlight effect
        /// </summary>
        public void RemoveHighlight()
        {
            if (slotLogic != null && slotLogic.IsEmpty && indicatorRenderer != null && normalMaterial != null)
            {
                indicatorRenderer.material = normalMaterial;
            }
        }
        
        /// <summary>
        /// Provide visual feedback for interaction
        /// </summary>
        public System.Collections.IEnumerator InteractionFeedback()
        {
            // Flash the highlight a few times
            for (int i = 0; i < 3; i++)
            {
                ApplyHighlight();
                yield return new WaitForSeconds(0.1f);
                RemoveHighlight();
                yield return new WaitForSeconds(0.1f);
            }
            UpdateVisualState();
        }
    }
}
