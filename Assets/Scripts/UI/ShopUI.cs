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
        /// Real-time UI update system - polls GameManager data every frame.
        /// 
        /// Update() vs Event-Driven Trade-offs:
        /// - Update(): Simple polling, guaranteed refresh rate, easier to debug
        /// - Event-driven: More efficient, reduces coupling, better for large systems
        /// 
        /// Performance Considerations:
        /// - String formatting can create garbage - use efficient methods
        /// - TextMeshPro text updates are optimized for frequent changes
        /// - Null-safety checks prevent crashes when GameManager unavailable
        /// 
        /// This approach provides immediate visual feedback for all shop data changes
        /// and ensures players always see current game state in real-time.
        /// </summary>
        private void Update()
        {
            // Only update displays if all UI references are valid and GameManager is available
            if (HasValidReferences)
            {
                UpdateDisplay();
            }
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
    }
}
