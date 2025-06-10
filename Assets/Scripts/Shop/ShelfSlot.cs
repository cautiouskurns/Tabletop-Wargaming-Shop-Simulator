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
        [Header("Visual Feedback")]
        [SerializeField] private Color emptySlotColor = Color.green;
        [SerializeField] private Color highlightColor = Color.yellow;
        
        [Header("Slot Indicator")]
        [SerializeField] private GameObject slotIndicator;
        [SerializeField] private Vector3 indicatorScale = new Vector3(0.2f, 0.01f, 0.2f);
        
        [Header("Debug")]
        [SerializeField] private bool showSlotGizmos = true;
        
        // Component references - composition pattern
        private ShelfSlotLogic slotLogic;
        private ShelfSlotVisuals slotVisuals;
        private ShelfSlotInteraction slotInteraction;
        
        // Public Properties - delegate to logic component
        public bool IsEmpty => slotLogic.IsEmpty;
        public Product CurrentProduct => slotLogic.CurrentProduct;
        public Vector3 SlotPosition => slotLogic.SlotPosition;
        
        // IInteractable Properties - delegate to interaction component
        public string InteractionText => slotInteraction.InteractionText;
        public bool CanInteract => slotInteraction.CanInteract;
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            // Initialize all required components
            InitializeComponents();
        }

        #endregion
        
        #region Component Setup
        
        /// <summary>
        /// Initialize all required components for composition pattern
        /// </summary>
        private void InitializeComponents()
        {
            // Get or add components
            slotLogic = GetComponent<ShelfSlotLogic>() ?? gameObject.AddComponent<ShelfSlotLogic>();
            slotVisuals = GetComponent<ShelfSlotVisuals>() ?? gameObject.AddComponent<ShelfSlotVisuals>();
            slotInteraction = GetComponent<ShelfSlotInteraction>() ?? gameObject.AddComponent<ShelfSlotInteraction>();
            
            // Initialize components with configuration
            slotVisuals.InitializeComponent(emptySlotColor, highlightColor, slotIndicator, indicatorScale);
            slotLogic.SetGizmoDrawing(showSlotGizmos);
            
            Debug.Log($"Initialized components for slot {name}");
        }
        
        #endregion
        
        #region Public Initialization
        
        /// <summary>
        /// Initialize the slot with specific parameters
        /// </summary>
        /// <param name="position">Local position offset for the slot</param>
        /// <param name="emptyColor">Color to display when slot is empty</param>
        /// <param name="highlightColor">Color to display when slot is highlighted</param>
        /// <param name="indicatorScale">Scale of the slot indicator GameObject</param>
        public void Initialize(Vector3? position = null, Color? emptyColor = null, Color? highlightColor = null, Vector3? indicatorScale = null)
        {
            // Apply position if provided
            if (position.HasValue)
            {
                slotLogic.SetSlotPosition(position.Value);
            }
            
            // Apply visual settings if provided
            if (emptyColor.HasValue || highlightColor.HasValue || indicatorScale.HasValue)
            {
                slotVisuals.InitializeComponent(
                    emptyColor ?? this.emptySlotColor,
                    highlightColor ?? this.highlightColor,
                    slotIndicator,
                    indicatorScale ?? this.indicatorScale
                );
            }
            
            #if UNITY_EDITOR
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
        
        #region Utility Methods
        
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
        
        #endregion
    }
}
