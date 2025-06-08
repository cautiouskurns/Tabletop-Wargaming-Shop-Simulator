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
        [SerializeField] private Vector3 indicatorScale = new Vector3(0.8f, 0.1f, 0.8f);
        
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
        
        // IInteractable Properties - delegate to interaction component with null checks
        public string InteractionText => EnsureInteractionComponent() ? slotInteraction.InteractionText : "Empty Slot";
        public bool CanInteract => EnsureInteractionComponent() ? slotInteraction.CanInteract : true;
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            // Initialize components if they don't exist
            InitializeComponents();
        }
        
        private void Start()
        {
            // Components handle their own initialization
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
            
            // Handle migration of legacy fields to components
            MigrateLegacyFields();
        }
        
        /// <summary>
        /// Migrate legacy serialized fields to the appropriate components
        /// </summary>
        private void MigrateLegacyFields()
        {
            if (hasBeenMigrated) return;
            
            // Migrate logic fields using reflection to access private fields
            var logicType = typeof(ShelfSlotLogic);
            var visualsType = typeof(ShelfSlotVisuals);
            
            // Migrate slotPosition to logic component
            var slotPositionField = logicType.GetField("slotPosition", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (slotPositionField != null)
            {
                slotPositionField.SetValue(slotLogic, slotPosition);
            }
            
            // Migrate currentProduct to logic component
            var currentProductField = logicType.GetField("currentProduct", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (currentProductField != null)
            {
                currentProductField.SetValue(slotLogic, currentProduct);
            }
            
            // Migrate visual fields to visuals component
            var highlightMaterialField = visualsType.GetField("highlightMaterial", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (highlightMaterialField != null)
            {
                highlightMaterialField.SetValue(slotVisuals, highlightMaterial);
            }
            
            var normalMaterialField = visualsType.GetField("normalMaterial", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (normalMaterialField != null)
            {
                normalMaterialField.SetValue(slotVisuals, normalMaterial);
            }
            
            var emptySlotColorField = visualsType.GetField("emptySlotColor", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (emptySlotColorField != null)
            {
                emptySlotColorField.SetValue(slotVisuals, emptySlotColor);
            }
            
            var highlightColorField = visualsType.GetField("highlightColor", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (highlightColorField != null)
            {
                highlightColorField.SetValue(slotVisuals, highlightColor);
            }
            
            var slotIndicatorField = visualsType.GetField("slotIndicator", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (slotIndicatorField != null)
            {
                slotIndicatorField.SetValue(slotVisuals, slotIndicator);
            }
            
            var indicatorScaleField = visualsType.GetField("indicatorScale", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (indicatorScaleField != null)
            {
                indicatorScaleField.SetValue(slotVisuals, indicatorScale);
            }
            
            hasBeenMigrated = true;
            
            // Clear legacy fields to avoid confusion
            slotPosition = Vector3.zero;
            currentProduct = null;
            highlightMaterial = null;
            normalMaterial = null;
            slotIndicator = null;
            
            #if UNITY_EDITOR
            // Mark the object as dirty for editor serialization
            UnityEditor.EditorUtility.SetDirty(this);
            #endif
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
                }
            }
            return slotVisuals != null;
        }

        #endregion
    }
}
