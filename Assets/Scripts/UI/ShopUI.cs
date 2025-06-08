using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace TabletopShop
{
    /// <summary>
    /// Central UI controller that manages all shop-related displays and interactions.
    /// This component serves as the foundation for all shop UI sub-systems including
    /// money display, time management, and shop controls.
    /// 
    /// Component-based design following existing UI patterns in the project.
    /// Requires Canvas component for proper UI hierarchy integration.
    /// </summary>
    [RequireComponent(typeof(Canvas))]
    public class ShopUI : MonoBehaviour
    {
        #region UI Element References
        
        [Header("Display Elements")]
        [Tooltip("UI text component for displaying current money/cash amount")]
        [SerializeField] private TextMeshProUGUI moneyDisplay;
        
        [Tooltip("UI text component for displaying sales information")]
        [SerializeField] private TextMeshProUGUI salesDisplay;
        
        [Tooltip("UI text component for displaying current game time")]
        [SerializeField] private TextMeshProUGUI timeDisplay;
        
        [Header("Control Buttons")]
        [Tooltip("Button to end the current day and proceed to next day")]
        [SerializeField] private Button endDayButton;
        
        [Tooltip("Button to pause/unpause the game simulation")]
        [SerializeField] private Button pauseButton;
        
        [Tooltip("Button to toggle inventory panel visibility")]
        [SerializeField] private Button inventoryToggleButton;
        
        [Header("Panel References")]
        [Tooltip("Panel containing daily summary information and statistics")]
        [SerializeField] private GameObject dailySummaryPanel;
        
        [Tooltip("Panel for setting product prices and price adjustments")]
        [SerializeField] private GameObject priceSettingPanel;
        
        #endregion
        
        #region Private Fields
        
        /// <summary>
        /// Cached reference to GameManager for data source access.
        /// Following existing UI pattern for null-safe GameManager integration.
        /// </summary>
        private GameManager gameManager;
        
        /// <summary>
        /// Cached Canvas component reference for UI hierarchy management.
        /// Required component ensures this is always available.
        /// </summary>
        private Canvas canvasComponent;
        
        #endregion
        
        #region Unity Lifecycle Methods
        
        /// <summary>
        /// Initialize component references and validate UI element assignments.
        /// Called before Start() for early initialization of cached components.
        /// 
        /// Component initialization pattern:
        /// - Cache required components (Canvas is guaranteed by RequireComponent)
        /// - Validate SerializeField UI references with error logging
        /// - Prepare for GameManager integration
        /// </summary>
        private void Awake()
        {
            // Cache the required Canvas component
            canvasComponent = GetComponent<Canvas>();
            
            // Validate UI element references with detailed error logging
            ValidateUIReferences();
            
            Debug.Log("[ShopUI] Component initialized - UI references validated");
        }
        
        /// <summary>
        /// Initialize GameManager connection and prepare UI for functionality.
        /// Called after Awake() when all components are initialized.
        /// 
        /// GameManager integration pattern:
        /// - Null-safe access to GameManager.Instance
        /// - Error logging for missing dependencies
        /// - Preparation for event-driven updates in future sub-tasks
        /// </summary>
        private void Start()
        {
            // Attempt to establish GameManager connection
            InitializeGameManagerConnection();
            
            // Initialize UI panels to default state (hidden for now)
            InitializeUIState();
            
            Debug.Log("[ShopUI] Start initialization complete - ready for functionality implementation");
        }
        
        /// <summary>
        /// Clean up any subscriptions or connections when component is destroyed.
        /// Following existing UI pattern for proper cleanup and memory management.
        /// 
        /// Future sub-tasks will add:
        /// - GameManager event unsubscription
        /// - Button event cleanup
        /// - Any other resource cleanup
        /// </summary>
        private void OnDestroy()
        {
            // Placeholder for future cleanup implementation
            // Event unsubscriptions will be added in future sub-tasks
            
            Debug.Log("[ShopUI] Component destroyed - cleanup complete");
        }
        
        #endregion
        
        #region Initialization Methods
        
        /// <summary>
        /// Validate all SerializeField UI element references.
        /// 
        /// SerializeField vs public field usage explanation:
        /// - SerializeField: Exposes private fields to Unity Inspector without making them public
        /// - Maintains encapsulation while allowing Inspector assignment
        /// - Preferred pattern for UI component references in Unity
        /// </summary>
        private void ValidateUIReferences()
        {
            bool allReferencesValid = true;
            
            // Validate display elements
            if (moneyDisplay == null)
            {
                Debug.LogError("[ShopUI] Money display TextMeshProUGUI reference is missing! Please assign in Inspector.");
                allReferencesValid = false;
            }
            
            if (salesDisplay == null)
            {
                Debug.LogError("[ShopUI] Sales display TextMeshProUGUI reference is missing! Please assign in Inspector.");
                allReferencesValid = false;
            }
            
            if (timeDisplay == null)
            {
                Debug.LogError("[ShopUI] Time display TextMeshProUGUI reference is missing! Please assign in Inspector.");
                allReferencesValid = false;
            }
            
            // Validate control buttons
            if (endDayButton == null)
            {
                Debug.LogError("[ShopUI] End day Button reference is missing! Please assign in Inspector.");
                allReferencesValid = false;
            }
            
            if (pauseButton == null)
            {
                Debug.LogError("[ShopUI] Pause Button reference is missing! Please assign in Inspector.");
                allReferencesValid = false;
            }
            
            if (inventoryToggleButton == null)
            {
                Debug.LogError("[ShopUI] Inventory toggle Button reference is missing! Please assign in Inspector.");
                allReferencesValid = false;
            }
            
            // Validate panel references
            if (dailySummaryPanel == null)
            {
                Debug.LogError("[ShopUI] Daily summary panel GameObject reference is missing! Please assign in Inspector.");
                allReferencesValid = false;
            }
            
            if (priceSettingPanel == null)
            {
                Debug.LogError("[ShopUI] Price setting panel GameObject reference is missing! Please assign in Inspector.");
                allReferencesValid = false;
            }
            
            if (allReferencesValid)
            {
                Debug.Log("[ShopUI] All UI references validated successfully");
            }
            else
            {
                Debug.LogWarning("[ShopUI] Some UI references are missing - functionality may be limited until assigned");
            }
        }
        
        /// <summary>
        /// Initialize connection to GameManager for data source access.
        /// 
        /// Canvas dependency and UI hierarchy requirements:
        /// - Canvas component is required for proper UI rendering
        /// - ShopUI should be attached to a Canvas GameObject in the scene
        /// - UI hierarchy: Canvas > ShopUI > Child UI elements
        /// - GameManager provides centralized data for UI updates
        /// </summary>
        private void InitializeGameManagerConnection()
        {
            // Null-safe GameManager access following existing UI patterns
            gameManager = GameManager.Instance;
            
            if (gameManager == null)
            {
                Debug.LogError("[ShopUI] GameManager.Instance is null! ShopUI requires GameManager for data source. " +
                             "Ensure GameManager is present in the scene and initialized before ShopUI.");
            }
            else
            {
                Debug.Log("[ShopUI] GameManager connection established successfully");
                
                // Preparation for GameManager integration in future sub-tasks:
                // - Event subscription for money updates
                // - Event subscription for time updates  
                // - Event subscription for sales data updates
                // - Event subscription for game state changes
            }
        }
        
        /// <summary>
        /// Initialize UI panels and elements to their default state.
        /// Prepares UI for event-driven updates from GameManager in future sub-tasks.
        /// </summary>
        private void InitializeUIState()
        {
            // Initialize panels to hidden state (will be controlled by buttons in future sub-tasks)
            if (dailySummaryPanel != null)
            {
                dailySummaryPanel.SetActive(false);
            }
            
            if (priceSettingPanel != null)
            {
                priceSettingPanel.SetActive(false);
            }
            
            // Initialize display texts to default values (will be updated from GameManager in future sub-tasks)
            if (moneyDisplay != null)
            {
                moneyDisplay.text = "$0.00";
            }
            
            if (salesDisplay != null)
            {
                salesDisplay.text = "Sales: $0.00";
            }
            
            if (timeDisplay != null)
            {
                timeDisplay.text = "Day 1 - 09:00";
            }
            
            Debug.Log("[ShopUI] UI state initialized to default values");
        }
        
        #endregion
        
        #region Public Properties (for future sub-task integration)
        
        /// <summary>
        /// Public access to Canvas component for external UI coordination.
        /// Useful for other UI systems that need to interact with the shop UI hierarchy.
        /// </summary>
        public Canvas Canvas => canvasComponent;
        
        /// <summary>
        /// Check if all UI references are properly assigned.
        /// Useful for validation in other systems or during runtime checks.
        /// </summary>
        public bool HasValidReferences => 
            moneyDisplay != null && salesDisplay != null && timeDisplay != null &&
            endDayButton != null && pauseButton != null && inventoryToggleButton != null &&
            dailySummaryPanel != null && priceSettingPanel != null;
        
        #endregion
    }
}
