using UnityEngine;
using TMPro;

namespace TabletopShop
{
    /// <summary>
    /// Handles all display update logic for ShopUI using composition pattern.
    /// Manages money, sales, and time displays with consistent formatting and null-safe operations.
    /// 
    /// Composition Pattern Benefits:
    /// - Separates display logic from UI coordination
    /// - Allows for specialized display handling
    /// - Improves testability and maintainability
    /// - Enables display-specific optimizations
    /// </summary>
    [System.Serializable]
    public class ShopUIDisplay
    {
        #region Display Element References
        
        [Header("Display Elements")]
        [Tooltip("UI text component for displaying current money/cash amount")]
        [SerializeField] private TextMeshProUGUI moneyDisplay;
        
        [Tooltip("UI text component for displaying sales information")]
        [SerializeField] private TextMeshProUGUI salesDisplay;
        
        [Tooltip("UI text component for displaying current game time")]
        [SerializeField] private TextMeshProUGUI timeDisplay;
        
        #endregion
        
        #region Private Fields
        
        /// <summary>
        /// Cached reference to GameManager for data source access.
        /// Used for null-safe display updates.
        /// </summary>
        private GameManager gameManager;
        
        /// <summary>
        /// Cached reference to SimpleDayNightCycle for time data.
        /// Used for getting current time, day, and day/night cycle information.
        /// </summary>
        private SimpleDayNightCycle dayNightCycle;
        
        #endregion
        
        #region Initialization
        
        /// <summary>
        /// Initialize the display component with UI element references and GameManager.
        /// This method should be called during ShopUI's initialization phase.
        /// </summary>
        /// <param name="moneyDisplayRef">Reference to money display TextMeshPro component</param>
        /// <param name="salesDisplayRef">Reference to sales display TextMeshPro component</param>
        /// <param name="timeDisplayRef">Reference to time display TextMeshPro component</param>
        /// <param name="gameManagerRef">Reference to GameManager for data access</param>
        /// <param name="dayNightCycleRef">Reference to SimpleDayNightCycle for time data</param>
        public void Initialize(TextMeshProUGUI moneyDisplayRef, TextMeshProUGUI salesDisplayRef, 
                             TextMeshProUGUI timeDisplayRef, GameManager gameManagerRef, 
                             SimpleDayNightCycle dayNightCycleRef = null)
        {
            moneyDisplay = moneyDisplayRef;
            salesDisplay = salesDisplayRef;
            timeDisplay = timeDisplayRef;
            gameManager = gameManagerRef;
            dayNightCycle = dayNightCycleRef;
            
            // Try to find SimpleDayNightCycle if not provided
            if (dayNightCycle == null)
            {
                dayNightCycle = Object.FindFirstObjectByType<SimpleDayNightCycle>();
                if (dayNightCycle != null)
                {
                    Debug.Log("[ShopUIDisplay] Found SimpleDayNightCycle automatically");
                }
                else
                {
                    Debug.LogWarning("[ShopUIDisplay] SimpleDayNightCycle not found - time display may not work correctly");
                }
            }
            
            Debug.Log("[ShopUIDisplay] Initialized with UI references, GameManager, and DayNightCycle");
        }
        
        /// <summary>
        /// Validate that all required UI references are properly assigned.
        /// Used for error checking during initialization.
        /// </summary>
        /// <returns>True if all essential references are valid</returns>
        public bool HasValidReferences()
        {
            return moneyDisplay != null && salesDisplay != null && timeDisplay != null;
        }
        
        /// <summary>
        /// Initialize display texts to default values.
        /// Called during UI initialization to set fallback display states.
        /// </summary>
        public void InitializeDisplayTexts()
        {
            if (moneyDisplay != null)
            {
                moneyDisplay.text = "$0";
            }
            
            if (salesDisplay != null)
            {
                salesDisplay.text = "No sales today";
            }
            
            if (timeDisplay != null)
            {
                timeDisplay.text = "Day 1 - 00:00";
            }
            
            Debug.Log("[ShopUIDisplay] Display texts initialized to default values");
        }
        
        #endregion
        
        #region Central Display Updates
        
        /// <summary>
        /// Update all display elements with current GameManager data.
        /// 
        /// Centralized Update Pattern Benefits:
        /// - Single point of control for all display updates
        /// - Easy to enable/disable entire display system
        /// - Consistent update ordering and error handling
        /// - Simple maintenance and debugging
        /// 
        /// This method ensures all displays stay synchronized with GameManager data
        /// and provides a foundation for future display enhancements.
        /// </summary>
        public void UpdateAllDisplays()
        {
            // Update all display elements with current GameManager data
            // Each method includes null-safety and fallback values
            UpdateMoneyDisplay();
            UpdateSalesDisplay();
            UpdateTimeDisplay();
        }
        
        /// <summary>
        /// Refresh all displays from GameManager.
        /// Used for initial load and when events might have been missed.
        /// </summary>
        public void RefreshDisplays()
        {
            if (gameManager != null && HasValidReferences())
            {
                UpdateAllDisplays();
                Debug.Log("[ShopUIDisplay] All displays refreshed from GameManager");
            }
            else
            {
                Debug.LogWarning("[ShopUIDisplay] Cannot refresh displays - GameManager or UI references unavailable");
            }
        }
        
        #endregion
        
        #region Individual Display Updates
        
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
        public void UpdateMoneyDisplay()
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
        public void UpdateSalesDisplay()
        {
            if (salesDisplay == null) return;
            
            // Use GameManager.Instance directly to avoid potential reference issues
            // and add debugging to check for reference mismatches
            GameManager actualGameManager = GameManager.Instance;
            
            if (gameManager != actualGameManager)
            {
                Debug.LogWarning($"[ShopUIDisplay] GameManager reference mismatch! cached={gameManager?.name ?? "null"}, actual={actualGameManager?.name ?? "null"}");
                // Update our reference to the correct instance
                gameManager = actualGameManager;
            }
            
            if (actualGameManager != null)
            {
                int customersServed = actualGameManager.CustomersServedToday;
                float dailyRevenue = actualGameManager.DailyRevenue;
                
                Debug.Log($"[ShopUIDisplay] UpdateSalesDisplay: customersServed={customersServed}, dailyRevenue=${dailyRevenue:F2}");
                
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
                
                Debug.Log($"[ShopUIDisplay] Sales display updated to: '{salesDisplay.text}'");
            }
            else
            {
                // Fallback display when GameManager unavailable
                salesDisplay.text = "0 sales";
                Debug.LogWarning("[ShopUIDisplay] GameManager.Instance is null in UpdateSalesDisplay");
            }
        }
        
        /// <summary>
        /// Update time display with current day and time information from SimpleDayNightCycle.
        /// 
        /// Time Display Format: "Day 3 - 14:30" or "Day 3 - Night"
        /// - Shows current day number
        /// - Shows current time in 24-hour format
        /// - Indicates day/night status
        /// 
        /// Data Source: SimpleDayNightCycle component provides accurate time data
        /// </summary>
        public void UpdateTimeDisplay()
        {
            if (timeDisplay == null) return;
            
            // Get time data from SimpleDayNightCycle (more accurate than GameManager)
            if (dayNightCycle != null)
            {
                int currentDay = dayNightCycle.CurrentDay;
                bool isDayTime = dayNightCycle.IsDayTime;
                string formattedTime = dayNightCycle.FormattedTime;
                
                // Display format: "Day 3 - 14:30" or "Day 3 - Night 22:30"
                string cycleIndicator = isDayTime ? "" : "Night ";
                timeDisplay.text = $"Day {currentDay} - {cycleIndicator}{formattedTime}";
            }
            else
            {
                // Fallback: Try to get from GameManager (legacy support)
                if (gameManager != null)
                {
                    // Note: This path may not work if GameManager doesn't have time properties
                    // Keep for backward compatibility
                    timeDisplay.text = "Day 1 - 08:00";
                }
                else
                {
                    // Final fallback display when no time source available
                    timeDisplay.text = "Day 1 - 08:00";
                }
            }
        }
        
        #endregion
        
        #region Event-Driven Updates
        
        /// <summary>
        /// Handle money changes from GameManager.
        /// Event-driven update triggered when money changes from transactions.
        /// 
        /// Benefits:
        /// - Immediate UI response to economic changes
        /// - No polling overhead
        /// - Precise updates only when data changes
        /// </summary>
        public void OnMoneyChanged()
        {
            Debug.Log("[ShopUIDisplay] OnMoneyChanged called - updating money and sales displays");
            UpdateMoneyDisplay();
            
            // Also update sales display as it includes revenue information
            UpdateSalesDisplay();
            
            Debug.Log("[ShopUIDisplay] Money changed - displays updated");
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
        public void OnDayChanged(int newDay)
        {
            // Update time display to show new day
            UpdateTimeDisplay();
            
            // Refresh sales display for new day (resets to 0)
            UpdateSalesDisplay();
            
            Debug.Log($"[ShopUIDisplay] Day changed to {newDay} - displays updated");
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
        public void OnDayNightCycleChanged(bool isDay)
        {
            UpdateTimeDisplay();
            
            string timeOfDay = isDay ? "Day" : "Night";
            Debug.Log($"[ShopUIDisplay] Day/Night cycle changed to {timeOfDay} - time display updated");
        }
        
        /// <summary>
        /// Handle reputation changes from GameManager.
        /// Event-driven update triggered when shop reputation changes.
        /// 
        /// Currently unused but prepared for future reputation displays.
        /// Reputation affects customer behavior and could influence UI feedback.
        /// </summary>
        /// <param name="newReputation">The new reputation value (0-100)</param>
        public void OnReputationChanged(float newReputation)
        {
            // Future: Update reputation displays, visual feedback, etc.
            // For now, just log the change for debugging
            
            Debug.Log($"[ShopUIDisplay] Reputation changed to {newReputation:F1} - ready for future UI integration");
        }
        
        #endregion
        
        #region Utility Methods
        
        /// <summary>
        /// Update the GameManager reference.
        /// Used when GameManager becomes available or changes.
        /// </summary>
        /// <param name="newGameManager">New GameManager reference</param>
        public void UpdateGameManagerReference(GameManager newGameManager)
        {
            gameManager = newGameManager;
            Debug.Log("[ShopUIDisplay] GameManager reference updated");
        }
        
        /// <summary>
        /// Get current display states for debugging.
        /// Returns a formatted string with current display values.
        /// </summary>
        /// <returns>Debug string with current display states</returns>
        public string GetDisplayStates()
        {
            return $"[ShopUIDisplay] Current States - " +
                   $"Money: '{moneyDisplay?.text ?? "NULL"}', " +
                   $"Sales: '{salesDisplay?.text ?? "NULL"}', " +
                   $"Time: '{timeDisplay?.text ?? "NULL"}'";
        }
        
        #endregion
    }
}
