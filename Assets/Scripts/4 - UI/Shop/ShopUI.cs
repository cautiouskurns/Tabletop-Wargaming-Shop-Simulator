using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Globalization;


namespace TabletopShop
{
    /// <summary>
    /// Central UI controller that manages all shop-related displays and interactions.
    /// This component serves as the foundation for all shop UI sub-systems including
    /// money display, time management, and shop controls.
    /// 
    /// Component-based design following existing UI patterns in the project.
    /// Requires Canvas component for proper UI hierarchy integration.
    /// 
    /// Updated to use composition pattern with ShopUIControls for button handling.
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
        
        [Header("Note")]
        [Tooltip("Panel management has been moved to ShopUIPanels component")]
        [SerializeField] private string panelManagementNote = "Panel references moved to ShopUIPanels component";
        
        #endregion
        
        #region Component References
        
        /// <summary>
        /// Controls component handling all button interactions and control logic.
        /// Uses composition pattern for separation of concerns.
        /// </summary>
        private ShopUIControls shopUIControls;
        
        /// <summary>
        /// Panels component handling all panel management and visibility.
        /// Uses composition pattern following existing UI patterns.
        /// </summary>
        private ShopUIPanels shopUIPanels;
        
        #endregion
        
        #region Private Fields
        
        /// <summary>
        /// Cached reference to GameManager for data source access.
        /// Following existing UI pattern for null-safe GameManager integration.
        /// </summary>
        private GameManager gameManager;
        
        /// <summary>
        /// Display component that handles all display update logic using composition pattern.
        /// Separates display concerns from UI coordination and event handling.
        /// </summary>
        [SerializeField] private ShopUIDisplay shopUIDisplay = new ShopUIDisplay();
        
        /// <summary>
        /// Cached Canvas component reference for UI hierarchy management.
        /// Required component ensures this is always available.
        /// </summary>
        private Canvas canvasComponent;
        
        // Note: Panel visibility state tracking has been moved to ShopUIPanels component
        
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
            
            // Get the ShopUIControls component
            shopUIControls = GetComponent<ShopUIControls>();
            if (shopUIControls == null)
            {
                Debug.LogError("[ShopUI] ShopUIControls component not found! Adding component automatically.");
                shopUIControls = gameObject.AddComponent<ShopUIControls>();
            }
            
            // Get the ShopUIPanels component
            shopUIPanels = GetComponent<ShopUIPanels>();
            if (shopUIPanels == null)
            {
                Debug.LogError("[ShopUI] ShopUIPanels component not found! Adding component automatically.");
                shopUIPanels = gameObject.AddComponent<ShopUIPanels>();
            }
            
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
        /// - Event-driven updates using GameManager events
        /// </summary>
        private void Start()
        {
            // Attempt to establish GameManager connection
            InitializeGameManagerConnection();
            
            // Initialize UI panels to default state (hidden for now)
            InitializeUIState();
            
            // Subscribe to GameManager events for real-time updates
            SubscribeToGameManagerEvents();
            
            // Initial data refresh to populate UI
            RefreshUIData();
            
            Debug.Log("[ShopUI] Start initialization complete - event-driven updates enabled");
        }
        
        /// <summary>
        /// Handle keyboard input for UI interactions and update time display.
        /// Replaced polling-based UI updates with event-driven architecture.
        /// 
        /// Performance Improvements:
        /// - No longer polls GameManager data every frame
        /// - UI updates only occur when data actually changes via events
        /// - Reduced coupling between UI and GameManager
        /// - Better scalability for complex UI systems
        /// 
        /// Remaining Update() functionality:
        /// - Keyboard input handling for price setting panel
        /// - Time display updates (since time changes continuously)
        /// - Other real-time input processing as needed
        /// </summary>
        private void Update()
        {
            // Handle keyboard input for price setting panel
            if (shopUIPanels != null && shopUIPanels.IsPriceSettingVisible)
            {
                // Escape key to cancel price setting
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    if (shopUIControls != null)
                    {
                        shopUIControls.TriggerAction(ShopUIAction.CancelPrice);
                    }
                }
            }
            
            // Update time display continuously since time changes every frame
            if (shopUIDisplay != null)
            {
                shopUIDisplay.UpdateTimeDisplay();
            }
        }
        
        /// <summary>
        /// Clean up any subscriptions or connections when component is destroyed.
        /// Following existing UI pattern for proper cleanup and memory management.
        /// 
        /// Cleanup includes:
        /// - GameManager event unsubscription
        /// </summary>
        private void OnDestroy()
        {
            // Unsubscribe from GameManager events to prevent memory leaks
            UnsubscribeFromGameManagerEvents();
            
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
            
            // Panel validation is now handled by ShopUIPanels component
            if (shopUIPanels != null)
            {
                shopUIPanels.ValidatePanelReferences();
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
                
                // Initialize the display component with references
                InitializeDisplayComponent();
            }
        }
        
        /// <summary>
        /// Initialize the ShopUIDisplay component with UI references and GameManager.
        /// Uses composition pattern to separate display logic from UI coordination.
        /// </summary>
        private void InitializeDisplayComponent()
        {
            if (shopUIDisplay == null)
            {
                shopUIDisplay = new ShopUIDisplay();
            }
            
            // Find SimpleDayNightCycle for time data
            SimpleDayNightCycle dayNightCycle = FindFirstObjectByType<SimpleDayNightCycle>();
            
            shopUIDisplay.Initialize(moneyDisplay, salesDisplay, timeDisplay, gameManager, dayNightCycle);
            shopUIDisplay.InitializeDisplayTexts();
            
            Debug.Log("[ShopUI] Display component initialized successfully");
        }
        
        /// <summary>
        /// Initialize UI panels and elements to their default state.
        /// Prepares UI for event-driven updates from GameManager in future sub-tasks.
        /// </summary>
        private void InitializeUIState()
        {
            // Initialize panels to hidden state via ShopUIPanels component
            if (shopUIPanels != null)
            {
                shopUIPanels.InitializePanelStates();
            }
            
            // Initialize display texts to default values (will be updated from GameManager)
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
        
        #region Event Subscription Management
        
        /// <summary>
        /// Subscribe to GameManager UnityEvents for real-time updates.
        /// Replaces polling-based UI updates with efficient event-driven architecture.
        /// 
        /// Event Subscription Pattern:
        /// - OnMoneyChanged: Updates money display when transactions occur
        /// - OnDayChanged: Updates day display and refreshes all daily data
        /// - OnDayNightCycleChanged: Updates time display for cycle transitions
        /// - OnReputationChanged: Future use for reputation displays
        /// 
        /// Error Handling:
        /// - Null-safe GameManager.Instance access
        /// - Retry logic for delayed initialization
        /// - Debug logging for subscription status
        /// </summary>
        private void SubscribeToGameManagerEvents()
        {
            // Attempt event subscription with null-safety
            if (GameManager.Instance != null)
            {
                // Subscribe to economic events for real-time updates
                GameManager.Instance.OnMoneyChanged.AddListener(OnMoneyChanged);
                GameManager.Instance.OnDayChanged.AddListener(OnDayChanged);
                GameManager.Instance.OnDayNightCycleChanged.AddListener(OnDayNightCycleChanged);
                GameManager.Instance.OnReputationChanged.AddListener(OnReputationChanged);
                
                Debug.Log("[ShopUI] Successfully subscribed to GameManager events");
            }
            else
            {
                Debug.LogWarning("[ShopUI] GameManager.Instance is null during event subscription. Will retry on next refresh.");
            }
            
            // Subscribe to ShopUIControls panel events
            if (shopUIControls != null)
            {
                shopUIControls.OnShowDailySummary.AddListener(ShowDailySummary);
                shopUIControls.OnHideDailySummary.AddListener(HideDailySummary);
                shopUIControls.OnHidePriceSetting.AddListener(HidePriceSetting);
                
                Debug.Log("[ShopUI] Successfully subscribed to ShopUIControls panel events");
            }
            else
            {
                Debug.LogWarning("[ShopUI] ShopUIControls component is null during event subscription.");
            }
        }
        
        /// <summary>
        /// Unsubscribe from GameManager events to prevent memory leaks.
        /// Essential for proper component lifecycle and event management.
        /// 
        /// Memory Management:
        /// - Prevents reference cycles that could cause memory leaks
        /// - Ensures clean component destruction
        /// - Follows Unity best practices for event lifecycle
        /// </summary>
        private void UnsubscribeFromGameManagerEvents()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnMoneyChanged.RemoveListener(OnMoneyChanged);
                GameManager.Instance.OnDayChanged.RemoveListener(OnDayChanged);
                GameManager.Instance.OnDayNightCycleChanged.RemoveListener(OnDayNightCycleChanged);
                GameManager.Instance.OnReputationChanged.RemoveListener(OnReputationChanged);
                
                Debug.Log("[ShopUI] Unsubscribed from GameManager events");
            }
            
            // Unsubscribe from ShopUIControls panel events
            if (shopUIControls != null)
            {
                shopUIControls.OnShowDailySummary.RemoveListener(ShowDailySummary);
                shopUIControls.OnHideDailySummary.RemoveListener(HideDailySummary);
                shopUIControls.OnHidePriceSetting.RemoveListener(HidePriceSetting);
                
                Debug.Log("[ShopUI] Unsubscribed from ShopUIControls panel events");
            }
        }
        
        /// <summary>
        /// Initialize or refresh UI data from GameManager.
        /// Used for initial load and when events might have been missed.
        /// 
        /// Initialization Strategy:
        /// - Called during Start() for initial population
        /// - Can be called manually for debugging or recovery
        /// - Includes retry logic for event subscription
        /// </summary>
        private void RefreshUIData()
        {
            if (gameManager != null && HasValidReferences)
            {
                // Use display component for coordinated updates via composition pattern
                if (shopUIDisplay != null)
                {
                    shopUIDisplay.RefreshDisplays();
                }
                
                // Retry event subscription if it failed earlier
                if (GameManager.Instance != null)
                {
                    // Check if events are properly subscribed by testing listener count
                    // Note: This is a basic check - UnityEvents don't expose listener count directly
                    SubscribeToGameManagerEvents();
                }
                
                Debug.Log("[ShopUI] UI data refreshed from GameManager via ShopUIDisplay");
            }
            else
            {
                Debug.LogWarning("[ShopUI] Cannot refresh UI data - GameManager or UI references unavailable");
            }
        }
        
        #endregion
        
        #region Event Handlers
        
        /// <summary>
        /// Handle money changes from GameManager.
        /// Event-driven update triggered when money changes from transactions.
        /// 
        /// Benefits:
        /// - Immediate UI response to economic changes
        /// - No polling overhead
        /// - Precise updates only when data changes
        /// </summary>
        private void OnMoneyChanged()
        {
            // Delegate to display component using composition pattern
            if (shopUIDisplay != null)
            {
                shopUIDisplay.OnMoneyChanged();
            }
            
            Debug.Log("[ShopUI] Money changed - displays updated via ShopUIDisplay");
        }
        
        /// <summary>
        /// Handle day changes from GameManager.
        /// Event-driven update triggered when day advances.
        /// 
        /// Day Change Actions:
        /// - Update time display with new day number
        /// - Refresh all daily data (sales, revenue, etc.)
        /// - Prepare UI for new day metrics
        /// </summary>
        /// <param name="newDay">The new day number</param>
        private void OnDayChanged(int newDay)
        {
            // Delegate to display component using composition pattern
            if (shopUIDisplay != null)
            {
                shopUIDisplay.OnDayChanged(newDay);
            }
            
            Debug.Log($"[ShopUI] Day changed to {newDay} - displays updated via ShopUIDisplay");
        }
        
        /// <summary>
        /// Handle day/night cycle changes from GameManager.
        /// Event-driven update triggered when cycle transitions between day and night.
        /// 
        /// Cycle Change Actions:
        /// - Update time display with current cycle state
        /// - Future: Adjust UI themes or visibility based on day/night
        /// </summary>
        /// <param name="isDay">True if it's now day time, false for night</param>
        private void OnDayNightCycleChanged(bool isDay)
        {
            // Delegate to display component using composition pattern
            if (shopUIDisplay != null)
            {
                shopUIDisplay.OnDayNightCycleChanged(isDay);
            }
            
            string timeOfDay = isDay ? "Day" : "Night";
            Debug.Log($"[ShopUI] Day/Night cycle changed to {timeOfDay} - displays updated via ShopUIDisplay");
        }
        
        /// <summary>
        /// Handle reputation changes from GameManager.
        /// Event-driven update triggered when shop reputation changes.
        /// 
        /// Currently unused but prepared for future reputation displays.
        /// Reputation affects customer behavior and could influence UI feedback.
        /// </summary>
        /// <param name="newReputation">The new reputation value (0-100)</param>
        private void OnReputationChanged(float newReputation)
        {
            // Delegate to display component using composition pattern
            if (shopUIDisplay != null)
            {
                shopUIDisplay.OnReputationChanged(newReputation);
            }
            
            Debug.Log($"[ShopUI] Reputation changed to {newReputation:F1} - ready for future UI integration via ShopUIDisplay");
        }

        #endregion
        
        #region Real-Time Display Updates
        
        /// <summary>
        /// Central display update method that calls all individual update methods.
        /// 
        /// Centralized Update Pattern Benefits:
        /// - Single point of control for all display updates
        /// - Easy to enable/disable entire update system
        /// - Consistent update ordering and error handling
        /// - Simple maintenance and debugging
        /// 
        /// This method ensures all UI displays stay synchronized with GameManager data
        /// and provides a foundation for future display enhancements.
        /// </summary>
        private void UpdateDisplay()
        {
            // Delegate all display updates to ShopUIDisplay component using composition pattern
            if (shopUIDisplay != null)
            {
                shopUIDisplay.UpdateAllDisplays();
            }
            
            // Future sub-tasks will add:
            // - UpdateButtonStates() for interactive elements
            // - UpdatePanelVisibility() for dynamic panels
            // - UpdateProgressBars() for day/night cycle indicators
        }
        
        #endregion
        
        #region Display Updates (Extracted to ShopUIDisplay)
        
        // Note: Display update methods have been extracted to ShopUIDisplay component
        // using the composition pattern for better separation of concerns.
        // See Assets/Scripts/UI/ShopUIDisplay.cs for implementation:
        // - UpdateMoneyDisplay() -> shopUIDisplay.UpdateMoneyDisplay()
        // - UpdateSalesDisplay() -> shopUIDisplay.UpdateSalesDisplay()  
        // - UpdateTimeDisplay() -> shopUIDisplay.UpdateTimeDisplay()
        // - UpdateAllDisplays() -> shopUIDisplay.UpdateAllDisplays()
        
        #endregion
        
        #region Public Properties (for future sub-task integration)
        
        /// <summary>
        /// Public access to Canvas component for external UI coordination.
        /// Useful for other UI systems that need to interact with the shop UI hierarchy.
        /// </summary>
        public Canvas Canvas => canvasComponent;
        
        /// <summary>
        /// Check if essential display UI references are properly assigned for real-time updates.
        /// Only checks critical display elements required for the core functionality.
        /// Optional panels (dailySummaryPanel, priceSettingPanel) are not required for basic display updates.
        /// </summary>
        public bool HasValidReferences => 
            moneyDisplay != null && salesDisplay != null && timeDisplay != null;
        
        #endregion
        
        #region Panel Management (Delegated to ShopUIPanels)
        
        /// <summary>
        /// Show the price setting popup for a specific product.
        /// Delegates to ShopUIPanels component for unified panel management.
        /// </summary>
        /// <param name="product">The product to set price for</param>
        public void ShowPriceSetting(Product product)
        {
            if (shopUIPanels != null)
            {
                shopUIPanels.ShowPriceSetting(product, shopUIControls);
            }
            else
            {
                Debug.LogError("[ShopUI] Cannot show price setting - ShopUIPanels component not available!");
            }
        }
        
        /// <summary>
        /// Hide the price setting popup and clear the current product reference.
        /// Delegates to ShopUIPanels component for unified panel management.
        /// </summary>
        public void HidePriceSetting()
        {
            if (shopUIPanels != null)
            {
                shopUIPanels.HidePriceSetting(shopUIControls);
            }
            else
            {
                Debug.LogError("[ShopUI] Cannot hide price setting - ShopUIPanels component not available!");
            }
        }
        
        /// <summary>
        /// Display the daily summary popup with key statistics from the current day.
        /// Delegates to ShopUIPanels component for unified panel management.
        /// </summary>
        public void ShowDailySummary()
        {
            if (shopUIPanels != null)
            {
                shopUIPanels.ShowDailySummary();
            }
            else
            {
                Debug.LogError("[ShopUI] Cannot show daily summary - ShopUIPanels component not available!");
            }
        }
        
        /// <summary>
        /// Hide the daily summary popup and proceed to the next day.
        /// Delegates to ShopUIPanels component for unified panel management.
        /// </summary>
        public void HideDailySummary()
        {
            if (shopUIPanels != null)
            {
                shopUIPanels.HideDailySummary();
            }
            else
            {
                Debug.LogError("[ShopUI] Cannot hide daily summary - ShopUIPanels component not available!");
            }
        }
        
        #endregion
    }
}
