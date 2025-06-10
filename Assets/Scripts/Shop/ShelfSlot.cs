using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TabletopShop
{
    /// <summary>
    /// Individual slot on a shelf that can hold one product
    /// Uses composition pattern to coordinate logic, visual, and interaction components
    /// </summary>
    public class ShelfSlot : MonoBehaviour, IInteractable
    {
        // Legacy serialized fields for backward compatibility - these will be migrated to components
        [Header("Slot Configuration")]
        [SerializeField] private Vector3 slotPosition;
        [SerializeField] private Product currentProduct;
        
        [Header("Visual Feedback")]
        [SerializeField] private Material highlightMaterial;
        [SerializeField] private Material normalMaterial;
        [SerializeField] private Color emptySlotColor = Color.green;
        [SerializeField] private Color highlightColor = Color.yellow;
        
        [Header("Slot Indicator")]
        [SerializeField] private GameObject slotIndicator;
        [SerializeField] private Vector3 indicatorScale = new Vector3(0.2f, 0.01f, 0.2f);
        
        [Header("Debug")]
        [SerializeField] private bool showSlotGizmos = true;
        
        // Component references
        private ShelfSlotLogic slotLogic;
        private ShelfSlotVisuals slotVisuals;
        private ShelfSlotInteraction slotInteraction;
        
        // Migration flag
        [HideInInspector]
        [SerializeField] private bool hasBeenMigrated = false;
        
        // Properties - delegate to logic component with null checks
        public bool IsEmpty => EnsureLogicComponent() ? slotLogic.IsEmpty : currentProduct == null;
        public Product CurrentProduct => EnsureLogicComponent() ? slotLogic.CurrentProduct : currentProduct;
        public Vector3 SlotPosition => EnsureLogicComponent() ? slotLogic.SlotPosition : transform.position + slotPosition;
        
        // Debug properties to help troubleshoot component synchronization
        public bool IsLogicComponentInitialized => slotLogic != null;
        public bool LegacyCurrentProductNull => currentProduct == null;
        public bool LogicCurrentProductNull => slotLogic?.CurrentProduct == null;
        
        // IInteractable Properties - delegate to interaction component with null checks
        public string InteractionText => EnsureInteractionComponent() ? slotInteraction.InteractionText : "Empty Slot";
        public bool CanInteract => EnsureInteractionComponent() ? slotInteraction.CanInteract : true;
        
        #region Unity Lifecycle
         private void Awake()
        {
            // Phase 1: Ensure all components exist (but don't initialize them yet)
            EnsureComponentsExist();
        }

        private void Start()
        {
            // Phase 2: Initialize components with proper order after all components exist
            InitializeComponentsInOrder();
        }

        /// <summary>
        /// Ensure all required components exist without initializing them
        /// </summary>
        private void EnsureComponentsExist()
        {
            if (slotLogic == null)
                slotLogic = GetComponent<ShelfSlotLogic>() ?? gameObject.AddComponent<ShelfSlotLogic>();
                
            if (slotVisuals == null)
                slotVisuals = GetComponent<ShelfSlotVisuals>() ?? gameObject.AddComponent<ShelfSlotVisuals>();
                
            if (slotInteraction == null)
                slotInteraction = GetComponent<ShelfSlotInteraction>() ?? gameObject.AddComponent<ShelfSlotInteraction>();
        }

        /// <summary>
        /// Initialize components in the correct order to prevent timing issues
        /// </summary>
        private void InitializeComponentsInOrder()
        {
            if (hasBeenMigrated) return;

            // 1. Initialize logic first (core state)
            InitializeLogicComponent();
            
            // 2. Then visuals (depends on logic state)
            InitializeVisualsComponent();
            
            // 3. Finally interaction (depends on both logic and visuals)
            // (ShelfSlotInteraction initializes itself)

            // 4. Configure gizmo drawing (only logic component draws gizmos)
            if (slotLogic != null)
            {
                slotLogic.SetGizmoDrawing(showSlotGizmos);
            }

            hasBeenMigrated = true;
            Debug.Log($"Components initialized in order for slot {name}");
        }
        
        #endregion
        
        #region Component Setup
        
        /// <summary>
        /// Initialize the required components for composition pattern
        /// </summary>
        private void InitializeComponents()
        {
            // Get or add logic component
            slotLogic = GetComponent<ShelfSlotLogic>();
            if (slotLogic == null)
            {
                slotLogic = gameObject.AddComponent<ShelfSlotLogic>();
            }
            
            // Get or add visuals component
            slotVisuals = GetComponent<ShelfSlotVisuals>();
            if (slotVisuals == null)
            {
                slotVisuals = gameObject.AddComponent<ShelfSlotVisuals>();
            }
            
            // Get or add interaction component
            slotInteraction = GetComponent<ShelfSlotInteraction>();
            if (slotInteraction == null)
            {
                slotInteraction = gameObject.AddComponent<ShelfSlotInteraction>();
            }
            
            // Initialize all components with current data
            InitializeAllComponents();
        }
        
        /// <summary>
        /// Initialize all components with current configuration (replaces complex migration)
        /// </summary>
        private void InitializeAllComponents()
        {
            if (hasBeenMigrated) return;

            // Ensure all components exist
            EnsureLogicComponent();
            EnsureVisualsComponent();
            EnsureInteractionComponent();

            hasBeenMigrated = true;
            Debug.Log($"Initialized all components for slot {name}");
        }
        
        #endregion
        
        #region Public Initialization
        
        /// <summary>
        /// Public initialization method for external setup of the ShelfSlot
        /// Can be called to configure the slot with specific parameters
        /// </summary>
        /// <param name="position">Local position offset for the slot</param>
        /// <param name="emptyColor">Color to display when slot is empty</param>
        /// <param name="highlightColor">Color to display when slot is highlighted</param>
        /// <param name="indicatorScale">Scale of the slot indicator GameObject</param>
        public void Initialize(Vector3? position = null, Color? emptyColor = null, Color? highlightColor = null, Vector3? indicatorScale = null)
        {
            // Ensure all components are initialized first
            InitializeComponents();
            
            // Apply position if provided
            if (position.HasValue && EnsureLogicComponent())
            {
                slotLogic.SetSlotPosition(position.Value);
            }
            
            // Apply visual settings if provided and visuals component is available
            if (EnsureVisualsComponent())
            {
                // Use reflection to set visual properties if provided
                var visualsType = typeof(ShelfSlotVisuals);
                
                if (emptyColor.HasValue)
                {
                    var emptySlotColorField = visualsType.GetField("emptySlotColor", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    emptySlotColorField?.SetValue(slotVisuals, emptyColor.Value);
                }
                
                if (highlightColor.HasValue)
                {
                    var highlightColorField = visualsType.GetField("highlightColor", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    highlightColorField?.SetValue(slotVisuals, highlightColor.Value);
                }
                
                if (indicatorScale.HasValue)
                {
                    var indicatorScaleField = visualsType.GetField("indicatorScale", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    indicatorScaleField?.SetValue(slotVisuals, indicatorScale.Value);
                }
            }
            
            #if UNITY_EDITOR
            // Mark the object as dirty for editor serialization
            UnityEditor.EditorUtility.SetDirty(this);
            #endif
        }
        
        /// <summary>
        /// Simple initialization with just position
        /// </summary>
        /// <param name="position">Local position offset for the slot</param>
        public void Initialize(Vector3 position)
        {
            Initialize(position, null, null, null);
        }
        
        #endregion
        
        #region Public Methods - Delegate to Logic Component
        
        /// <summary>
        /// Place a product in this slot
        /// </summary>
        /// <param name="product">The product to place</param>
        /// <returns>True if product was successfully placed</returns>
        public bool PlaceProduct(Product product)
        {
            return slotLogic.PlaceProduct(product);
        }
        
        /// <summary>
        /// Remove the product from this slot
        /// </summary>
        /// <returns>The removed product, or null if slot was empty</returns>
        public Product RemoveProduct()
        {
            return slotLogic.RemoveProduct();
        }
        
        /// <summary>
        /// Clear the slot without returning the product (for cleanup)
        /// </summary>
        public void ClearSlot()
        {
            slotLogic.ClearSlot();
        }
        
        /// <summary>
        /// Set the local position offset for this slot
        /// </summary>
        /// <param name="position">Local position offset from slot transform</param>
        public void SetSlotPosition(Vector3 position)
        {
            slotLogic.SetSlotPosition(position);
        }
        
        #endregion
        
        #region IInteractable Implementation - Delegate to Interaction Component
        
        /// <summary>
        /// Handle player interaction with this slot
        /// </summary>
        /// <param name="player">The player GameObject</param>
        public void Interact(GameObject player)
        {
            slotInteraction.Interact(player);
        }
        
        /// <summary>
        /// Called when player starts looking at this slot
        /// </summary>
        public void OnInteractionEnter()
        {
            slotInteraction.OnInteractionEnter();
        }
        
        /// <summary>
        /// Called when player stops looking at this slot
        /// </summary>
        public void OnInteractionExit()
        {
            slotInteraction.OnInteractionExit();
        }
        
        #endregion
        
        #region Component Helpers
        
        /// <summary>
        /// Ensure logic component is initialized and return true if available
        /// </summary>
        /// <returns>True if logic component is available</returns>
        private bool EnsureLogicComponent()
        {
            if (slotLogic == null)
            {
                slotLogic = GetComponent<ShelfSlotLogic>();
                if (slotLogic == null)
                {
                    slotLogic = gameObject.AddComponent<ShelfSlotLogic>();
                    // Initialize with current data instead of reflection migration
                    InitializeLogicComponent();
                }
            }
            return slotLogic != null;
        }
        
        /// <summary>
        /// Ensure interaction component is initialized and return true if available
        /// </summary>
        /// <returns>True if interaction component is available</returns>
        private bool EnsureInteractionComponent()
        {
            if (slotInteraction == null)
            {
                slotInteraction = GetComponent<ShelfSlotInteraction>();
                if (slotInteraction == null)
                {
                    slotInteraction = gameObject.AddComponent<ShelfSlotInteraction>();
                }
            }
            return slotInteraction != null;
        }
        
        /// <summary>
        /// Ensure visuals component is initialized and return true if available
        /// </summary>
        /// <returns>True if visuals component is available</returns>
        private bool EnsureVisualsComponent()
        {
            if (slotVisuals == null)
            {
                slotVisuals = GetComponent<ShelfSlotVisuals>();
                if (slotVisuals == null)
                {
                    slotVisuals = gameObject.AddComponent<ShelfSlotVisuals>();
                    // Initialize with current data instead of reflection migration
                    InitializeVisualsComponent();
                }
            }
            return slotVisuals != null;
        }

        /// <summary>
        /// Initialize logic component with current data (no reflection needed)
        /// </summary>
        private void InitializeLogicComponent()
        {
            if (slotLogic == null) return;

            // Direct initialization without reflection
            slotLogic.SetSlotPosition(slotPosition);
            
            if (currentProduct != null)
            {
                slotLogic.InitializeWithProduct(currentProduct);
                Debug.Log($"Initialized logic component with product {currentProduct.ProductData?.ProductName} for slot {name}");
            }
            
            hasBeenMigrated = true;
        }

        /// <summary>
        /// Initialize visuals component with current data (no reflection needed)
        /// </summary>
        private void InitializeVisualsComponent()
        {
            if (slotVisuals == null) return;

            // Direct initialization without reflection
            slotVisuals.InitializeComponent(emptySlotColor, highlightColor, slotIndicator, indicatorScale);
            Debug.Log($"Initialized visuals component for slot {name}");
        }

        #endregion

        /// <summary>
        /// Debug method to check slot state consistency
        /// </summary>
        public string GetSlotDebugInfo()
        {
            return $"Slot {name}: " +
                   $"IsEmpty={IsEmpty}, " +
                   $"HasLogicComp={IsLogicComponentInitialized}, " +
                   $"LegacyProdNull={LegacyCurrentProductNull}, " +
                   $"LogicProdNull={LogicCurrentProductNull}, " +
                   $"HasMigrated={hasBeenMigrated}";
        }

        /// <summary>
        /// Ensure synchronization between legacy fields and components (simplified)
        /// </summary>
        public void ForceSynchronization()
        {
            if (!EnsureLogicComponent()) return;

            // Check if there's a product mismatch and fix it
            bool legacyHasProduct = currentProduct != null;
            bool logicHasProduct = slotLogic.CurrentProduct != null;

            if (legacyHasProduct != logicHasProduct)
            {
                Debug.LogWarning($"Product synchronization mismatch detected in slot {name}! Legacy has product: {legacyHasProduct}, Logic has product: {logicHasProduct}");
                
                // Trust the legacy field as the source of truth and update logic component directly
                if (legacyHasProduct)
                {
                    slotLogic.InitializeWithProduct(currentProduct);
                    Debug.Log($"Fixed synchronization: set logic component product to {currentProduct?.ProductData?.ProductName ?? "null"}");
                }
                else
                {
                    slotLogic.ClearSlot();
                    Debug.Log($"Fixed synchronization: cleared logic component product");
                }
            }
        }

        /// <summary>
        /// Control gizmo drawing across all components
        /// </summary>
        public void SetGizmoDrawing(bool enabled)
        {
            showSlotGizmos = enabled;
            if (slotLogic != null)
            {
                slotLogic.SetGizmoDrawing(enabled);
            }
        }
    }
}
