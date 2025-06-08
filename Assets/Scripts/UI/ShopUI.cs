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
        
        [Header("Daily Summary UI Elements")]
        [Tooltip("Text component displaying daily revenue in summary")]
        [SerializeField] private TextMeshProUGUI dailySummaryRevenueText;
        
        [Tooltip("Text component displaying daily expenses in summary")]
        [SerializeField] private TextMeshProUGUI dailySummaryExpensesText;
        
        [Tooltip("Text component displaying daily profit in summary")]
        [SerializeField] private TextMeshProUGUI dailySummaryProfitText;
        
        [Tooltip("Text component displaying customers served in summary")]
        [SerializeField] private TextMeshProUGUI dailySummaryCustomersText;
        
        [Tooltip("Text component displaying the current day number in summary")]
        [SerializeField] private TextMeshProUGUI dailySummaryDayText;
        
        [Tooltip("Button to continue to next day from daily summary")]
        [SerializeField] private Button dailySummaryContinueButton;
        
        [Header("Price Setting UI Elements")]
        [Tooltip("Input field for entering new product price")]
        [SerializeField] private TMP_InputField priceInputField;
        
        [Tooltip("Text component displaying current price in price setting panel")]
        [SerializeField] private TextMeshProUGUI currentPriceText;
        
        [Tooltip("Text component displaying suggested price range")]
        [SerializeField] private TextMeshProUGUI suggestedPriceText;
        
        [Tooltip("Button to confirm price change")]
        [SerializeField] private Button confirmPriceButton;
        
        [Tooltip("Button to cancel price change")]
        [SerializeField] private Button cancelPriceButton;
        
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
        
        /// <summary>
        /// Reference to InventoryUI for inventory panel toggle functionality.
        /// Cached for efficient access without repeated lookups.
        /// </summary>
        private InventoryUI inventoryUI;
        
        /// <summary>
        /// Current pause state of the game.
        /// Used to track whether the game is paused for visual feedback and state management.
        /// </summary>
        private bool isGamePaused = false;
        
        /// <summary>
        /// Current state of the daily summary panel visibility.
        /// Used to track whether the daily summary is currently displayed.
        /// </summary>
        private bool isDailySummaryVisible = false;
        
        /// <summary>
        /// Track whether the price setting panel is currently visible.
        /// Used to prevent multiple simultaneous price setting displays.
        /// </summary>
        private bool isPriceSettingVisible = false;
        
        /// <summary>
        /// Reference to the product currently being price-adjusted.
        /// Used to apply price changes to the correct product.
        /// </summary>
        private Product currentPricingProduct = null;
        
        /// <summary>
        /// Original text for the pause button when the game is not paused.
        /// </summary>
        private const string PAUSE_TEXT = "Pause";
        
        /// <summary>
        /// Text for the pause button when the game is paused.
        /// </summary>
        private const string RESUME_TEXT = "Resume";
        
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
        /// - Event-driven updates using GameManager events
        /// </summary>
        private void Start()
        {
            // Attempt to establish GameManager connection
            InitializeGameManagerConnection();
            
            // Initialize UI panels to default state (hidden for now)
            InitializeUIState();
            
            // Setup button event handlers
            SetupButtonEventHandlers();
            
            // Subscribe to GameManager events for real-time updates
            SubscribeToGameManagerEvents();
            
            // Initial data refresh to populate UI
            RefreshUIData();
            
            Debug.Log("[ShopUI] Start initialization complete - event-driven updates enabled");
        }
        
        /// <summary>
        /// Handle keyboard input for UI interactions.
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
        /// - Other real-time input processing as needed
        /// </summary>
        private void Update()
        {
            // Handle keyboard input for price setting panel
            if (isPriceSettingVisible)
            {
                // Escape key to cancel price setting
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    OnCancelPriceButtonClicked();
                }
            }
        }
        
        /// <summary>
        /// Clean up any subscriptions or connections when component is destroyed.
        /// Following existing UI pattern for proper cleanup and memory management.
        /// 
        /// Cleanup includes:
        /// - Button event handler removal
        /// - Time.timeScale restoration
        /// - GameManager event unsubscription
        /// </summary>
        private void OnDestroy()
        {
            // Unsubscribe from GameManager events to prevent memory leaks
            UnsubscribeFromGameManagerEvents();
            
            // Clean up button event handlers to prevent memory leaks
            CleanupButtonEventHandlers();
            
            // Restore normal time scale if game was paused
            if (isGamePaused)
            {
                Time.timeScale = 1f;
                Debug.Log("[ShopUI] Restored Time.timeScale to 1 on component destroy");
            }
            
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
            
            // Validate daily summary UI elements
            if (dailySummaryRevenueText == null)
            {
                Debug.LogError("[ShopUI] Daily summary revenue TextMeshProUGUI reference is missing! Please assign in Inspector.");
                allReferencesValid = false;
            }
            
            if (dailySummaryExpensesText == null)
            {
                Debug.LogError("[ShopUI] Daily summary expenses TextMeshProUGUI reference is missing! Please assign in Inspector.");
                allReferencesValid = false;
            }
            
            if (dailySummaryProfitText == null)
            {
                Debug.LogError("[ShopUI] Daily summary profit TextMeshProUGUI reference is missing! Please assign in Inspector.");
                allReferencesValid = false;
            }
            
            if (dailySummaryCustomersText == null)
            {
                Debug.LogError("[ShopUI] Daily summary customers TextMeshProUGUI reference is missing! Please assign in Inspector.");
                allReferencesValid = false;
            }
            
            if (dailySummaryDayText == null)
            {
                Debug.LogError("[ShopUI] Daily summary day TextMeshProUGUI reference is missing! Please assign in Inspector.");
                allReferencesValid = false;
            }
            
            if (dailySummaryContinueButton == null)
            {
                Debug.LogError("[ShopUI] Daily summary continue Button reference is missing! Please assign in Inspector.");
                allReferencesValid = false;
            }
            
            // Validate price setting UI elements
            if (priceInputField == null)
            {
                Debug.LogError("[ShopUI] Price input field TMP_InputField reference is missing! Please assign in Inspector.");
                allReferencesValid = false;
            }
            
            if (currentPriceText == null)
            {
                Debug.LogError("[ShopUI] Current price TextMeshProUGUI reference is missing! Please assign in Inspector.");
                allReferencesValid = false;
            }
            
            if (suggestedPriceText == null)
            {
                Debug.LogError("[ShopUI] Suggested price TextMeshProUGUI reference is missing! Please assign in Inspector.");
                allReferencesValid = false;
            }
            
            if (confirmPriceButton == null)
            {
                Debug.LogError("[ShopUI] Confirm price Button reference is missing! Please assign in Inspector.");
                allReferencesValid = false;
            }
            
            if (cancelPriceButton == null)
            {
                Debug.LogError("[ShopUI] Cancel price Button reference is missing! Please assign in Inspector.");
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
            
            // Initialize pause button text
            if (pauseButton != null)
            {
                var buttonText = pauseButton.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                {
                    buttonText.text = PAUSE_TEXT;
                }
            }
            
            Debug.Log("[ShopUI] UI state initialized to default values");
        }
        
        /// <summary>
        /// Setup button event handlers for all interactive UI elements.
        /// Connects button onClick events to their respective handler methods.
        /// </summary>
        private void SetupButtonEventHandlers()
        {
            // Find InventoryUI reference for inventory toggle functionality
            inventoryUI = FindFirstObjectByType<InventoryUI>();
            if (inventoryUI == null)
            {
                Debug.LogWarning("[ShopUI] InventoryUI not found in scene. Inventory toggle button will be disabled.");
            }
            
            // Setup End Day button
            if (endDayButton != null)
            {
                endDayButton.onClick.AddListener(OnEndDayButtonClicked);
                Debug.Log("[ShopUI] End Day button event handler added");
            }
            else
            {
                Debug.LogWarning("[ShopUI] End Day button reference is null - button functionality unavailable");
            }
            
            // Setup Pause button
            if (pauseButton != null)
            {
                pauseButton.onClick.AddListener(OnPauseButtonClicked);
                Debug.Log("[ShopUI] Pause button event handler added");
            }
            else
            {
                Debug.LogWarning("[ShopUI] Pause button reference is null - button functionality unavailable");
            }
            
            // Setup Inventory Toggle button
            if (inventoryToggleButton != null)
            {
                inventoryToggleButton.onClick.AddListener(OnInventoryToggleButtonClicked);
                Debug.Log("[ShopUI] Inventory toggle button event handler added");
            }
            else
            {
                Debug.LogWarning("[ShopUI] Inventory toggle button reference is null - button functionality unavailable");
            }
            
            // Setup Daily Summary Continue button
            if (dailySummaryContinueButton != null)
            {
                dailySummaryContinueButton.onClick.AddListener(OnDailySummaryContinueButtonClicked);
                Debug.Log("[ShopUI] Daily summary continue button event handler added");
            }
            else
            {
                Debug.LogWarning("[ShopUI] Daily summary continue button reference is null - button functionality unavailable");
            }
            
            // Setup Price Setting buttons
            if (confirmPriceButton != null)
            {
                confirmPriceButton.onClick.AddListener(OnConfirmPriceButtonClicked);
                Debug.Log("[ShopUI] Confirm price button event handler added");
            }
            else
            {
                Debug.LogWarning("[ShopUI] Confirm price button reference is null - button functionality unavailable");
            }
            
            if (cancelPriceButton != null)
            {
                cancelPriceButton.onClick.AddListener(OnCancelPriceButtonClicked);
                Debug.Log("[ShopUI] Cancel price button event handler added");
            }
            else
            {
                Debug.LogWarning("[ShopUI] Cancel price button reference is null - button functionality unavailable");
            }
            
            // Setup price input field event handlers
            if (priceInputField != null)
            {
                priceInputField.onEndEdit.AddListener(OnPriceInputEndEdit);
                Debug.Log("[ShopUI] Price input field event handler added");
            }
            else
            {
                Debug.LogWarning("[ShopUI] Price input field reference is null - input functionality unavailable");
            }
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
                // Trigger manual updates for all displays
                UpdateMoneyDisplay();
                UpdateSalesDisplay();
                UpdateTimeDisplay();
                
                // Retry event subscription if it failed earlier
                if (GameManager.Instance != null)
                {
                    // Check if events are properly subscribed by testing listener count
                    // Note: This is a basic check - UnityEvents don't expose listener count directly
                    SubscribeToGameManagerEvents();
                }
                
                Debug.Log("[ShopUI] UI data refreshed from GameManager");
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
            UpdateMoneyDisplay();
            
            // Also update sales display as it includes revenue information
            UpdateSalesDisplay();
            
            Debug.Log("[ShopUI] Money changed - displays updated");
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
            // Update time display to show new day
            UpdateTimeDisplay();
            
            // Refresh sales display for new day (resets to 0)
            UpdateSalesDisplay();
            
            // Future: Update any day-specific UI elements
            
            Debug.Log($"[ShopUI] Day changed to {newDay} - displays updated");
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
            UpdateTimeDisplay();
            
            // Future: Could adjust UI appearance based on day/night
            // - Different color schemes
            // - Visibility of certain elements
            // - Animation states
            
            string timeOfDay = isDay ? "Day" : "Night";
            Debug.Log($"[ShopUI] Day/Night cycle changed to {timeOfDay} - time display updated");
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
            // Future: Update reputation displays, visual feedback, etc.
            // For now, just log the change for debugging
            
            Debug.Log($"[ShopUI] Reputation changed to {newReputation:F1} - ready for future UI integration");
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
            // Update all display elements with current GameManager data
            // Each method includes null-safety and fallback values
            UpdateMoneyDisplay();
            UpdateSalesDisplay();
            UpdateTimeDisplay();
            
            // Future sub-tasks will add:
            // - UpdateButtonStates() for interactive elements
            // - UpdatePanelVisibility() for dynamic panels
            // - UpdateProgressBars() for day/night cycle indicators
        }
        
        /// <summary>
        /// Update money display with proper currency formatting.
        /// 
        /// Currency Formatting Explanation:
        /// - ToString("C0"): Uses system currency symbol with no decimal places
        /// - ToString("N0"): Number format with thousands separators, no decimals
        /// - Custom "$#,##0": Manual format for consistent display regardless of locale
        /// 
        /// Performance Notes:
        /// - String.Format and ToString can create garbage collection pressure
        /// - TextMeshPro optimizes text changes to minimize rendering updates
        /// - Null-safe access prevents exceptions when GameManager unavailable
        /// 
        /// Display Format: $1,234 (thousands separator, no decimals for clean appearance)
        /// </summary>
        private void UpdateMoneyDisplay()
        {
            if (moneyDisplay == null) return;
            
            // Null-safe GameManager access with fallback display
            if (gameManager != null)
            {
                // Efficient currency formatting using custom format string
                // This avoids locale-specific currency symbols and provides consistent display
                float currentMoney = gameManager.CurrentMoney;
                moneyDisplay.text = string.Format("${0:N0}", currentMoney);
            }
            else
            {
                // Fallback display when GameManager unavailable
                moneyDisplay.text = "$0";
            }
        }
        
        /// <summary>
        /// Update sales display showing daily sales count and revenue.
        /// 
        /// Sales Display Design:
        /// - Shows both count and revenue for comprehensive information
        /// - Uses consistent formatting with money display
        /// - Provides immediate feedback on shop performance
        /// 
        /// Display Format: "5 sales - $1,234" (count and revenue for context)
        /// Alternative formats considered: "Sales: 5", "Revenue: $1,234", "5 × $246 avg"
        /// </summary>
        private void UpdateSalesDisplay()
        {
            if (salesDisplay == null) return;
            
            // Null-safe GameManager access with comprehensive sales information
            if (gameManager != null)
            {
                int customersServed = gameManager.CustomersServedToday;
                float dailyRevenue = gameManager.DailyRevenue;
                
                // Format: "X sales - $Y,YYY" for clear performance indication
                if (customersServed > 0)
                {
                    salesDisplay.text = string.Format("{0} sales - ${1:N0}", customersServed, dailyRevenue);
                }
                else
                {
                    // No sales yet today
                    salesDisplay.text = "No sales today";
                }
            }
            else
            {
                // Fallback display when GameManager unavailable
                salesDisplay.text = "0 sales";
            }
        }
        
        /// <summary>
        /// Update time display with MM:SS format showing elapsed time in current day/night cycle.
        /// 
        /// Time Formatting Calculations:
        /// - GameManager provides DayProgress (0-1) for current cycle position
        /// - Calculate elapsed time from cycle duration and current progress
        /// - Convert total seconds to MM:SS format for clear time indication
        /// 
        /// Display Format: "05:23" (minutes:seconds elapsed in current cycle)
        /// Day/Night Indication: Shows "Day 05:23" or "Night 01:45" for context
        /// 
        /// Time Display Trade-offs:
        /// - Elapsed vs Countdown: Elapsed shows progress through cycle, countdown shows urgency
        /// - Real-time vs Periodic: Real-time for immediate feedback, periodic for performance
        /// - Absolute vs Relative: Relative elapsed time more intuitive for time progression
        /// </summary>
        private void UpdateTimeDisplay()
        {
            if (timeDisplay == null) return;
            
            // Null-safe GameManager access with comprehensive time information
            if (gameManager != null)
            {
                bool isDayTime = gameManager.IsDayTime;
                int currentDay = gameManager.CurrentDay;
                float dayProgress = gameManager.DayProgress;
                
                // Calculate remaining time in current cycle
                float totalCycleSeconds;
                if (isDayTime)
                {
                    // Day cycle - get day length from GameManager (assuming 10 minutes default)
                    // Note: dayLengthInMinutes is private, using reasonable default
                    totalCycleSeconds = 10.0f * 60.0f; // 10 minutes in seconds
                }
                else
                {
                    // Night cycle - get night length from GameManager (assuming 2 minutes default)
                    totalCycleSeconds = 2.0f * 60.0f; // 2 minutes in seconds
                }
                
                // Calculate elapsed seconds (progress through cycle)
                float elapsedSeconds = totalCycleSeconds * dayProgress;
                elapsedSeconds = Mathf.Clamp(elapsedSeconds, 0, totalCycleSeconds); // Ensure within bounds
                
                // Convert to MM:SS format
                int minutes = Mathf.FloorToInt(elapsedSeconds / 60.0f);
                int seconds = Mathf.FloorToInt(elapsedSeconds % 60.0f);
                string timeString = FormatTimeCountdown(minutes, seconds);
                
                // Display format: "Day 3 - 05:23" or "Night 3 - 01:45"
                string cycleIndicator = isDayTime ? "Day" : "Night";
                timeDisplay.text = string.Format("{0} {1} - {2}", cycleIndicator, currentDay, timeString);
            }
            else
            {
                // Fallback display when GameManager unavailable
                timeDisplay.text = "Day 1 - 05:00";
            }
        }
        
        #endregion
        
        #region Formatting Utilities
        
        /// <summary>
        /// Format time values into MM:SS countdown string.
        /// 
        /// Time Formatting Utilities:
        /// - Consistent MM:SS format across all time displays
        /// - Zero-padding for professional appearance
        /// - Efficient string formatting to minimize garbage collection
        /// 
        /// Performance Considerations:
        /// - string.Format vs StringBuilder vs direct concatenation
        /// - Called every frame, so efficiency matters
        /// - ToString("D2") for zero-padding is optimized in .NET
        /// 
        /// Example outputs: "05:23", "00:47", "12:00"
        /// </summary>
        /// <param name="minutes">Minutes component (0-59 typically)</param>
        /// <param name="seconds">Seconds component (0-59)</param>
        /// <returns>Formatted time string in MM:SS format</returns>
        private string FormatTimeCountdown(int minutes, int seconds)
        {
            // Ensure values are within expected ranges
            minutes = Mathf.Clamp(minutes, 0, 99); // Support up to 99 minutes
            seconds = Mathf.Clamp(seconds, 0, 59);
            
            // Use efficient formatting with zero-padding
            return string.Format("{0:D2}:{1:D2}", minutes, seconds);
        }
        
        /// <summary>
        /// Format currency values with consistent formatting rules.
        /// 
        /// Currency Formatting Utilities:
        /// - Centralized currency formatting for consistency
        /// - Handles different value ranges (dollars, cents, thousands)
        /// - Configurable precision for different use cases
        /// 
        /// Formatting Rules:
        /// - No decimals for whole dollar amounts (cleaner appearance)
        /// - Thousands separators for readability
        /// - Dollar sign prefix for clear currency indication
        /// 
        /// Example outputs: "$1,234", "$500", "$12,000"
        /// </summary>
        /// <param name="amount">Currency amount to format</param>
        /// <param name="includeCents">Whether to include cents (default: false)</param>
        /// <returns>Formatted currency string</returns>
        private string FormatCurrency(float amount, bool includeCents = false)
        {
            // Use appropriate formatting based on cents requirement
            if (includeCents)
            {
                return string.Format("${0:N2}", amount); // $1,234.56
            }
            else
            {
                return string.Format("${0:N0}", amount); // $1,234
            }
        }
        
        /// <summary>
        /// Format sales count with appropriate singular/plural handling.
        /// 
        /// Sales Formatting Utilities:
        /// - Proper pluralization for professional appearance
        /// - Handles edge cases (0, 1, multiple sales)
        /// - Consistent format across all sales displays
        /// 
        /// Example outputs: "No sales", "1 sale", "5 sales"
        /// </summary>
        /// <param name="count">Number of sales</param>
        /// <returns>Formatted sales count string</returns>
        private string FormatSalesCount(int count)
        {
            if (count == 0)
            {
                return "No sales";
            }
            else if (count == 1)
            {
                return "1 sale";
            }
            else
            {
                return string.Format("{0} sales", count);
            }
        }
        
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
        
        #region Button Event Handlers
        
        /// <summary>
        /// Handle end day button click - shows daily summary popup before advancing to next day.
        /// 
        /// End Day Button Logic:
        /// - Shows daily summary panel with key statistics
        /// - Displays revenue, expenses, profit, and customers served
        /// - Waits for player to click Continue button before advancing day
        /// - Provides comprehensive feedback on daily performance
        /// 
        /// Daily Summary Flow:
        /// 1. End Day button clicked
        /// 2. Show daily summary panel with current day's data
        /// 3. Player reviews statistics
        /// 4. Player clicks Continue button
        /// 5. Hide summary panel and advance to next day
        /// </summary>
        private void OnEndDayButtonClicked()
        {
            if (gameManager != null)
            {
                ShowDailySummary();
                Debug.Log("[ShopUI] End day button clicked - showing daily summary");
            }
            else
            {
                Debug.LogError("[ShopUI] GameManager is not available! Cannot show daily summary.");
            }
        }
        
        /// <summary>
        /// Handle pause button click - toggles game pause state using Time.timeScale.
        /// 
        /// Pause Button Logic:
        /// - Toggles isGamePaused flag
        /// - Updates button text to reflect current state
        /// - Uses Time.timeScale to pause/resume all time-based systems
        /// 
        /// Visual Feedback:
        /// - Changes button text between "Pause" and "Resume"
        /// - Indicates current game state to the player
        /// 
        /// Time.timeScale Implementation:
        /// - timeScale = 0: Pauses all time-based updates (physics, animations, etc.)
        /// - timeScale = 1: Normal game speed
        /// - Affects all Unity systems that use Time.deltaTime
        /// </summary>
        private void OnPauseButtonClicked()
        {
            if (pauseButton == null) return;
            
            // Toggle pause state
            isGamePaused = !isGamePaused;
            
            // Update Time.timeScale to pause/resume the game
            if (isGamePaused)
            {
                Time.timeScale = 0f; // Pause all time-based systems
                UpdatePauseButtonText(RESUME_TEXT);
                Debug.Log("[ShopUI] Game paused (Time.timeScale = 0)");
            }
            else
            {
                Time.timeScale = 1f; // Resume normal time
                UpdatePauseButtonText(PAUSE_TEXT);
                Debug.Log("[ShopUI] Game resumed (Time.timeScale = 1)");
            }
        }
        
        /// <summary>
        /// Handle inventory toggle button click - toggles inventory panel visibility.
        /// 
        /// Inventory Toggle Logic:
        /// - Calls InventoryUI.TogglePanel() to show/hide inventory
        /// - Provides null-safe access to InventoryUI component
        /// - Logs appropriate messages for debugging
        /// 
        /// Integration Notes:
        /// - Uses existing InventoryUI.TogglePanel() method
        /// - Maintains separation of concerns between UI systems
        /// - No direct manipulation of inventory UI state
        /// </summary>
        private void OnInventoryToggleButtonClicked()
        {
            if (inventoryUI != null)
            {
                inventoryUI.TogglePanel();
                Debug.Log("[ShopUI] Inventory toggle button clicked - toggled inventory panel");
            }
            else
            {
                Debug.LogWarning("[ShopUI] InventoryUI reference is null! Cannot toggle inventory panel. " +
                               "Ensure InventoryUI component exists in the scene.");
            }
        }
        
        /// <summary>
        /// Update pause button text with null-safe TextMeshPro access.
        /// 
        /// Button Text Update Pattern:
        /// - Finds TextMeshProUGUI component in button's children
        /// - Updates text with appropriate pause/resume label
        /// - Provides error logging if text component not found
        /// 
        /// UI Hierarchy Assumption:
        /// - Button GameObject contains child with TextMeshProUGUI component
        /// - Standard Unity button setup with text label
        /// </summary>
        /// <param name="newText">The text to display on the pause button</param>
        private void UpdatePauseButtonText(string newText)
        {
            if (pauseButton == null) return;
            
            var buttonText = pauseButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = newText;
            }
            else
            {
                Debug.LogWarning("[ShopUI] Pause button does not have a TextMeshProUGUI component in children. " +
                               "Cannot update button text.");
            }
        }
        
        /// <summary>
        /// Clean up button event handlers to prevent memory leaks.
        /// Removes all onClick listeners that were added during initialization.
        /// </summary>
        private void CleanupButtonEventHandlers()
        {
            // Remove End Day button event handler
            if (endDayButton != null)
            {
                endDayButton.onClick.RemoveListener(OnEndDayButtonClicked);
            }
            
            // Remove Pause button event handler
            if (pauseButton != null)
            {
                pauseButton.onClick.RemoveListener(OnPauseButtonClicked);
            }
            
            // Remove Inventory Toggle button event handler
            if (inventoryToggleButton != null)
            {
                inventoryToggleButton.onClick.RemoveListener(OnInventoryToggleButtonClicked);
            }
            
            // Remove Daily Summary Continue button event handler
            if (dailySummaryContinueButton != null)
            {
                dailySummaryContinueButton.onClick.RemoveListener(OnDailySummaryContinueButtonClicked);
            }
            
            // Remove Price Setting button event handlers
            if (confirmPriceButton != null)
            {
                confirmPriceButton.onClick.RemoveListener(OnConfirmPriceButtonClicked);
            }
            
            if (cancelPriceButton != null)
            {
                cancelPriceButton.onClick.RemoveListener(OnCancelPriceButtonClicked);
            }
            
            if (priceInputField != null)
            {
                priceInputField.onEndEdit.RemoveListener(OnPriceInputEndEdit);
            }
            
            Debug.Log("[ShopUI] Button event handlers cleaned up");
        }
        
        #endregion
        
        #region Price Setting System
        
        /// <summary>
        /// Show the price setting popup for a specific product.
        /// 
        /// Price Setting Display:
        /// - Shows current price of the product
        /// - Provides input field for new price entry
        /// - Displays suggested price range based on base price
        /// - Offers confirm/cancel options for price changes
        /// 
        /// UI Flow:
        /// 1. Store reference to product being priced
        /// 2. Populate current price and suggested range
        /// 3. Show the price setting panel
        /// 4. Set visibility state flag
        /// 5. Player can enter new price and confirm/cancel
        /// </summary>
        /// <param name="product">The product to set price for</param>
        public void ShowPriceSetting(Product product)
        {
            if (product == null)
            {
                Debug.LogError("[ShopUI] Cannot show price setting - product is null!");
                return;
            }
            
            if (priceSettingPanel == null)
            {
                Debug.LogError("[ShopUI] Cannot show price setting - priceSettingPanel is not assigned!");
                return;
            }
            
            if (isPriceSettingVisible)
            {
                Debug.LogWarning("[ShopUI] Price setting panel is already visible. Closing current panel first.");
                HidePriceSetting();
            }
            
            // Store reference to product being priced
            currentPricingProduct = product;
            
            // Populate current price display
            if (currentPriceText != null)
            {
                currentPriceText.text = $"Current Price: {FormatCurrency(product.CurrentPrice)}";
            }
            
            // Calculate and display suggested price range (±20% of base price)
            if (suggestedPriceText != null)
            {
                float basePrice = product.ProductData.BasePrice;
                float minSuggested = basePrice * 0.8f;
                float maxSuggested = basePrice * 1.2f;
                suggestedPriceText.text = $"Suggested: {FormatCurrency(minSuggested)} - {FormatCurrency(maxSuggested)}";
            }
            
            // Initialize input field with current price
            if (priceInputField != null)
            {
                priceInputField.text = product.CurrentPrice.ToString("F0");
                priceInputField.Select();
                priceInputField.ActivateInputField();
            }
            
            // Show the price setting panel
            priceSettingPanel.SetActive(true);
            isPriceSettingVisible = true;
            
            Debug.Log($"[ShopUI] Price setting displayed for product: {product.ProductData.ProductName}, " +
                     $"Current price: {FormatCurrency(product.CurrentPrice)}");
        }
        
        /// <summary>
        /// Hide the price setting popup and clear the current product reference.
        /// 
        /// Cancel Flow:
        /// 1. Hide the price setting panel
        /// 2. Reset visibility state flag
        /// 3. Clear product reference
        /// 4. Provide debug feedback
        /// </summary>
        public void HidePriceSetting()
        {
            if (priceSettingPanel != null)
            {
                priceSettingPanel.SetActive(false);
            }
            
            isPriceSettingVisible = false;
            currentPricingProduct = null;
            
            Debug.Log("[ShopUI] Price setting panel hidden");
        }
        
        /// <summary>
        /// Handle confirm price button click - applies the new price to the product.
        /// 
        /// Confirm Price Logic:
        /// - Validates input field contains valid positive number
        /// - Applies new price to current product using Product.SetPrice()
        /// - Hides price setting panel
        /// - Provides feedback on price change success/failure
        /// 
        /// Price Validation:
        /// - Must be positive number (> 0)
        /// - Reasonable range check (prevents extreme values)
        /// - Error handling for invalid input
        /// </summary>
        private void OnConfirmPriceButtonClicked()
        {
            if (currentPricingProduct == null)
            {
                Debug.LogError("[ShopUI] Cannot confirm price - no product selected!");
                HidePriceSetting();
                return;
            }
            
            if (priceInputField == null)
            {
                Debug.LogError("[ShopUI] Cannot confirm price - price input field is null!");
                return;
            }
            
            // Parse the input price
            if (float.TryParse(priceInputField.text, out float newPrice))
            {
                // Validate price is positive and reasonable
                if (newPrice > 0 && newPrice <= 10000) // Max $10,000 per item
                {
                    // Convert to int for Product.SetPrice() method
                    int newPriceInt = Mathf.RoundToInt(newPrice);
                    
                    // Apply the new price using Product.SetPrice()
                    currentPricingProduct.SetPrice(newPriceInt);
                    
                    Debug.Log($"[ShopUI] Price updated for {currentPricingProduct.ProductData.ProductName}: " +
                             $"{FormatCurrency(newPriceInt)}");
                    
                    // Hide the price setting panel
                    HidePriceSetting();
                }
                else
                {
                    Debug.LogWarning($"[ShopUI] Invalid price range: {newPrice}. Price must be between $1 and $10,000.");
                    
                    // Reset input field to current price
                    if (priceInputField != null)
                    {
                        priceInputField.text = currentPricingProduct.CurrentPrice.ToString("F0");
                    }
                }
            }
            else
            {
                Debug.LogWarning($"[ShopUI] Invalid price input: '{priceInputField.text}'. Please enter a valid number.");
                
                // Reset input field to current price
                if (priceInputField != null)
                {
                    priceInputField.text = currentPricingProduct.CurrentPrice.ToString("F0");
                }
            }
        }
        
        /// <summary>
        /// Handle cancel price button click - closes price setting without changes.
        /// 
        /// Cancel Price Logic:
        /// - Closes price setting panel without applying changes
        /// - Clears current product reference
        /// - Provides user feedback
        /// </summary>
        private void OnCancelPriceButtonClicked()
        {
            Debug.Log("[ShopUI] Price setting cancelled by user");
            HidePriceSetting();
        }
        
        /// <summary>
        /// Handle price input field end edit - supports Enter key to confirm.
        /// 
        /// Input Field Logic:
        /// - Enter key confirms price change (same as clicking Confirm button)
        /// - Escape key cancels price change (same as clicking Cancel button)
        /// - Provides keyboard shortcuts for efficient price setting
        /// </summary>
        /// <param name="inputText">The text entered in the input field</param>
        private void OnPriceInputEndEdit(string inputText)
        {
            // Check if Enter was pressed to confirm
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                OnConfirmPriceButtonClicked();
            }
            // Check if Escape was pressed to cancel
            else if (Input.GetKeyDown(KeyCode.Escape))
            {
                OnCancelPriceButtonClicked();
            }
        }
        
        #endregion
        
        #region Daily Summary System
        
        /// <summary>
        /// Display the daily summary popup with key statistics from the current day.
        /// 
        /// Daily Summary Display:
        /// - Shows current day's revenue, expenses, profit, and customers served
        /// - Uses existing GameManager economic data
        /// - Formats currency values using the established FormatCurrency utility
        /// - Activates the daily summary panel and populates all text fields
        /// 
        /// UI Flow:
        /// 1. Populate all summary text elements with current day data
        /// 2. Show the daily summary panel
        /// 3. Set visibility state flag
        /// 4. Player can review performance before continuing
        /// </summary>
        private void ShowDailySummary()
        {
            if (gameManager == null)
            {
                Debug.LogError("[ShopUI] Cannot show daily summary - GameManager is not available!");
                return;
            }
            
            if (dailySummaryPanel == null)
            {
                Debug.LogError("[ShopUI] Cannot show daily summary - dailySummaryPanel is not assigned!");
                return;
            }
            
            // Get current day's economic data from GameManager
            float dailyRevenue = gameManager.DailyRevenue;
            float dailyExpenses = gameManager.DailyExpenses;
            float dailyProfit = gameManager.DailyProfit;
            int customersServed = gameManager.CustomersServedToday;
            int currentDay = gameManager.CurrentDay;
            
            // Populate summary text elements using existing formatting utilities
            if (dailySummaryRevenueText != null)
            {
                dailySummaryRevenueText.text = FormatCurrency(dailyRevenue);
            }
            
            if (dailySummaryExpensesText != null)
            {
                dailySummaryExpensesText.text = FormatCurrency(dailyExpenses);
            }
            
            if (dailySummaryProfitText != null)
            {
                // Use color coding for profit/loss indication
                string profitText = FormatCurrency(dailyProfit);
                dailySummaryProfitText.text = profitText;
                
                // Optional: Add color coding for profit (green) vs loss (red)
                if (dailyProfit >= 0)
                {
                    dailySummaryProfitText.color = Color.green;
                }
                else
                {
                    dailySummaryProfitText.color = Color.red;
                }
            }
            
            if (dailySummaryCustomersText != null)
            {
                dailySummaryCustomersText.text = FormatSalesCount(customersServed);
            }
            
            if (dailySummaryDayText != null)
            {
                dailySummaryDayText.text = $"Day {currentDay} Summary";
            }
            
            // Show the daily summary panel
            dailySummaryPanel.SetActive(true);
            isDailySummaryVisible = true;
            
            Debug.Log($"[ShopUI] Daily summary displayed - Revenue: {FormatCurrency(dailyRevenue)}, " +
                     $"Expenses: {FormatCurrency(dailyExpenses)}, Profit: {FormatCurrency(dailyProfit)}, " +
                     $"Customers: {customersServed}");
        }
        
        /// <summary>
        /// Hide the daily summary popup and proceed to the next day.
        /// 
        /// Continue Flow:
        /// 1. Hide the daily summary panel
        /// 2. Reset visibility state flag
        /// 3. Call GameManager.ForceNextDay() to advance the game
        /// 4. Provide debug feedback for the transition
        /// 
        /// This method completes the daily transition cycle that was initiated
        /// by the End Day button click.
        /// </summary>
        private void HideDailySummary()
        {
            if (dailySummaryPanel != null)
            {
                dailySummaryPanel.SetActive(false);
            }
            
            isDailySummaryVisible = false;
            
            // Now advance to the next day
            if (gameManager != null)
            {
                gameManager.ForceNextDay();
                Debug.Log("[ShopUI] Daily summary hidden - advanced to next day");
            }
            else
            {
                Debug.LogError("[ShopUI] Cannot advance to next day - GameManager is not available!");
            }
        }
        
        /// <summary>
        /// Handle continue button click from daily summary panel.
        /// 
        /// Continue Button Logic:
        /// - Called when player clicks Continue button in daily summary
        /// - Closes the daily summary popup
        /// - Proceeds to next day via HideDailySummary()
        /// - Provides seamless transition from summary to next day
        /// 
        /// Event Flow:
        /// End Day Button → ShowDailySummary() → Daily Summary Display → 
        /// Continue Button → OnDailySummaryContinueButtonClicked() → HideDailySummary() → Next Day
        /// </summary>
        private void OnDailySummaryContinueButtonClicked()
        {
            HideDailySummary();
            Debug.Log("[ShopUI] Daily summary continue button clicked - proceeding to next day");
        }
        
        #endregion
    }
}
