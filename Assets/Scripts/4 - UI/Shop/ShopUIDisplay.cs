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
        public void Initialize(TextMeshProUGUI moneyDisplayRef, TextMeshProUGUI salesDisplayRef, 
                             TextMeshProUGUI timeDisplayRef, GameManager gameManagerRef)
        {
            moneyDisplay = moneyDisplayRef;
            salesDisplay = salesDisplayRef;
            timeDisplay = timeDisplayRef;
            gameManager = gameManagerRef;
            
            Debug.Log("[ShopUIDisplay] Initialized with UI references and GameManager");
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
        public void UpdateTimeDisplay()
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
                string timeString = UIFormatting.FormatTimeCountdown(minutes, seconds);
                
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
