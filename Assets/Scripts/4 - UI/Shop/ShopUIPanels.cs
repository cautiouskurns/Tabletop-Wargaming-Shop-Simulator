using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Globalization;

namespace TabletopShop
{
    /// <summary>
    /// Panel types for unified panel management system.
    /// Used to identify and manage different UI panels in the ShopUI system.
    /// </summary>
    public enum PanelType
    {
        DailySummary,
        PriceSetting,
        Inventory,
        Settings
    }

    /// <summary>
    /// Handles all panel management for the ShopUI system using a unified approach.
    /// Extracted from ShopUI.cs to separate panel concerns from main UI coordination.
    /// 
    /// Key Features:
    /// - Unified panel management using PanelType enum
    /// - Centralized panel visibility state tracking
    /// - Dedicated methods for each panel type
    /// - Event-driven communication with parent components
    /// - Consistent panel initialization and cleanup
    /// 
    /// Component Design:
    /// - Uses composition pattern following existing UI patterns
    /// - Maintains separation of concerns from display logic
    /// - Provides clear public API for panel operations
    /// - Null-safe reference handling throughout
    /// </summary>
    public class ShopUIPanels : MonoBehaviour
    {
        #region Panel References
        
        [Header("Panel GameObjects")]
        [Tooltip("Panel containing daily summary information and statistics")]
        [SerializeField] private GameObject dailySummaryPanel;
        
        [Tooltip("Panel for setting product prices and price adjustments")]
        [SerializeField] private GameObject priceSettingPanel;
        
        #endregion
        
        #region Daily Summary UI Elements
        
        [Header("Daily Summary Elements")]
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
        
        #endregion
        
        #region Price Setting UI Elements
        
        [Header("Price Setting Elements")]
        [Tooltip("Text component displaying current price in price setting panel")]
        [SerializeField] private TextMeshProUGUI currentPriceText;
        
        [Tooltip("Text component displaying suggested price range")]
        [SerializeField] private TextMeshProUGUI suggestedPriceText;
        
        #endregion
        
        #region Private Fields
        
        /// <summary>
        /// Cached reference to GameManager for data source access.
        /// Following existing UI pattern for null-safe GameManager integration.
        /// </summary>
        private GameManager gameManager;
        
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
        
        #endregion
        
        #region Unity Lifecycle Methods
        
        /// <summary>
        /// Initialize GameManager connection and panel states.
        /// Called when the component is first created.
        /// </summary>
        private void Start()
        {
            InitializeGameManager();
            InitializePanelStates();
        }
        
        #endregion
        
        #region Initialization Methods
        
        /// <summary>
        /// Initialize GameManager connection with null-safe access pattern.
        /// </summary>
        private void InitializeGameManager()
        {
            gameManager = GameManager.Instance;
            if (gameManager == null)
            {
                Debug.LogWarning("[ShopUIPanels] GameManager not available during initialization");
            }
            else
            {
//                Debug.Log("[ShopUIPanels] GameManager connection established");
            }
        }
        
        /// <summary>
        /// Initialize all panels to their default hidden state.
        /// Prepares panels for proper visibility management.
        /// </summary>
        public void InitializePanelStates()
        {
            // Initialize panels to hidden state
            SetPanelVisibility(PanelType.DailySummary, false);
            SetPanelVisibility(PanelType.PriceSetting, false);
            
//            Debug.Log("[ShopUIPanels] Panel states initialized - all panels hidden");
        }
        
        #endregion
        
        #region Unified Panel Management
        
        /// <summary>
        /// Set the visibility of a specific panel type.
        /// Unified method for consistent panel management across all types.
        /// </summary>
        /// <param name="panelType">The type of panel to show/hide</param>
        /// <param name="visible">Whether the panel should be visible</param>
        public void SetPanelVisibility(PanelType panelType, bool visible)
        {
            switch (panelType)
            {
                case PanelType.DailySummary:
                    SetDailySummaryVisibility(visible);
                    break;
                case PanelType.PriceSetting:
                    SetPriceSettingVisibility(visible);
                    break;
                case PanelType.Inventory:
                    Debug.LogWarning("[ShopUIPanels] Inventory panel management not implemented yet");
                    break;
                case PanelType.Settings:
                    Debug.LogWarning("[ShopUIPanels] Settings panel management not implemented yet");
                    break;
                default:
                    Debug.LogError($"[ShopUIPanels] Unknown panel type: {panelType}");
                    break;
            }
        }
        
        /// <summary>
        /// Get the visibility state of a specific panel type.
        /// </summary>
        /// <param name="panelType">The type of panel to check</param>
        /// <returns>True if the panel is visible, false otherwise</returns>
        public bool IsPanelVisible(PanelType panelType)
        {
            switch (panelType)
            {
                case PanelType.DailySummary:
                    return isDailySummaryVisible;
                case PanelType.PriceSetting:
                    return isPriceSettingVisible;
                case PanelType.Inventory:
                case PanelType.Settings:
                    Debug.LogWarning($"[ShopUIPanels] Visibility check for {panelType} not implemented yet");
                    return false;
                default:
                    Debug.LogError($"[ShopUIPanels] Unknown panel type: {panelType}");
                    return false;
            }
        }
        
        #endregion
        
        #region Daily Summary Panel Management
        
        /// <summary>
        /// Set the visibility of the daily summary panel.
        /// Internal method called by unified panel management.
        /// </summary>
        /// <param name="visible">Whether the panel should be visible</param>
        private void SetDailySummaryVisibility(bool visible)
        {
            if (dailySummaryPanel != null)
            {
                dailySummaryPanel.SetActive(visible);
            }
            
            isDailySummaryVisible = visible;
        }
        
        /// <summary>
        /// Display the daily summary popup with key statistics from the current day.
        /// 
        /// Daily Summary Display:
        /// - Shows current day's revenue, expenses, profit, and customers served
        /// - Uses existing GameManager economic data
        /// - Formats currency values using the established FormatCurrency utility
        /// - Activates the daily summary panel and populates all text fields
        /// </summary>
        public void ShowDailySummary()
        {
            if (gameManager == null)
            {
                Debug.LogError("[ShopUIPanels] Cannot show daily summary - GameManager is not available!");
                return;
            }
            
            if (dailySummaryPanel == null)
            {
                Debug.LogError("[ShopUIPanels] Cannot show daily summary - dailySummaryPanel is not assigned!");
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
                dailySummaryRevenueText.text = UIFormatting.FormatCurrency(dailyRevenue);
            }
            
            if (dailySummaryExpensesText != null)
            {
                dailySummaryExpensesText.text = UIFormatting.FormatCurrency(dailyExpenses);
            }
            
            if (dailySummaryProfitText != null)
            {
                // Use color coding for profit/loss indication
                string profitText = UIFormatting.FormatCurrency(dailyProfit);
                dailySummaryProfitText.text = profitText;
                
                // Add color coding for profit (green) vs loss (red)
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
                dailySummaryCustomersText.text = UIFormatting.FormatSalesCount(customersServed);
            }
            
            if (dailySummaryDayText != null)
            {
                dailySummaryDayText.text = $"Day {currentDay} Summary";
            }
            
            // Show the daily summary panel
            SetPanelVisibility(PanelType.DailySummary, true);
            
            Debug.Log($"[ShopUIPanels] Daily summary displayed - Revenue: {UIFormatting.FormatCurrency(dailyRevenue)}, " +
                     $"Expenses: {UIFormatting.FormatCurrency(dailyExpenses)}, Profit: {UIFormatting.FormatCurrency(dailyProfit)}, " +
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
        public void HideDailySummary()
        {
            SetPanelVisibility(PanelType.DailySummary, false);
            
            // Now advance to the next day
            if (gameManager != null)
            {
                gameManager.ForceNextDay();
                Debug.Log("[ShopUIPanels] Daily summary hidden - advanced to next day");
            }
            else
            {
                Debug.LogError("[ShopUIPanels] Cannot advance to next day - GameManager is not available!");
            }
        }
        
        #endregion
        
        #region Price Setting Panel Management
        
        /// <summary>
        /// Set the visibility of the price setting panel.
        /// Internal method called by unified panel management.
        /// </summary>
        /// <param name="visible">Whether the panel should be visible</param>
        private void SetPriceSettingVisibility(bool visible)
        {
            if (priceSettingPanel != null)
            {
                priceSettingPanel.SetActive(visible);
            }
            
            isPriceSettingVisible = visible;
        }
        
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
        /// 1. Store reference to product being priced in controls component
        /// 2. Populate current price and suggested range
        /// 3. Show the price setting panel
        /// 4. Set visibility state flag
        /// 5. Player can enter new price and confirm/cancel
        /// </summary>
        /// <param name="product">The product to set price for</param>
        /// <param name="shopUIControls">Reference to controls component for product handling</param>
        public void ShowPriceSetting(Product product, ShopUIControls shopUIControls)
        {
            if (product == null)
            {
                Debug.LogError("[ShopUIPanels] Cannot show price setting - product is null!");
                return;
            }
            
            if (priceSettingPanel == null)
            {
                Debug.LogError("[ShopUIPanels] Cannot show price setting - priceSettingPanel is not assigned!");
                return;
            }
            
            if (isPriceSettingVisible)
            {
                Debug.LogWarning("[ShopUIPanels] Price setting panel is already visible. Closing current panel first.");
                HidePriceSetting(shopUIControls);
            }
            
            // Set product reference in controls component
            if (shopUIControls != null)
            {
                shopUIControls.SetCurrentPricingProduct(product);
            }
            
            // Populate current price display
            if (currentPriceText != null)
            {
                currentPriceText.text = $"Current Price: {UIFormatting.FormatCurrency(product.CurrentPrice)}";
            }
            
            // Calculate and display suggested price range (±20% of base price)
            if (suggestedPriceText != null)
            {
                float basePrice = product.ProductData.BasePrice;
                float minSuggested = basePrice * 0.8f;
                float maxSuggested = basePrice * 1.2f;
                suggestedPriceText.text = $"Suggested: {UIFormatting.FormatCurrency(minSuggested)} - {UIFormatting.FormatCurrency(maxSuggested)}";
            }
            
            // Show the price setting panel
            SetPanelVisibility(PanelType.PriceSetting, true);
            
            Debug.Log($"[ShopUIPanels] Price setting displayed for product: {product.ProductData.ProductName}, " +
                     $"Current price: {UIFormatting.FormatCurrency(product.CurrentPrice)}");
        }
        
        /// <summary>
        /// Hide the price setting popup and clear the current product reference.
        /// 
        /// Cancel Flow:
        /// 1. Hide the price setting panel
        /// 2. Reset visibility state flag
        /// 3. Clear product reference in controls component
        /// 4. Provide debug feedback
        /// </summary>
        /// <param name="shopUIControls">Reference to controls component for cleanup</param>
        public void HidePriceSetting(ShopUIControls shopUIControls = null)
        {
            SetPanelVisibility(PanelType.PriceSetting, false);
            
            // Clear product reference in controls component
            if (shopUIControls != null)
            {
                shopUIControls.ClearCurrentPricingProduct();
            }
            
            Debug.Log("[ShopUIPanels] Price setting panel hidden");
        }
        
        #endregion
        
        #region Public Properties
        
        /// <summary>
        /// Get the current visibility state of the daily summary panel.
        /// </summary>
        public bool IsDailySummaryVisible => isDailySummaryVisible;
        
        /// <summary>
        /// Get the current visibility state of the price setting panel.
        /// </summary>
        public bool IsPriceSettingVisible => isPriceSettingVisible;
        
        #endregion
        
        #region Debug and Utility Methods
        
        /// <summary>
        /// Get current panel states for debugging.
        /// Returns a formatted string with current panel visibility states.
        /// </summary>
        /// <returns>Debug string with current panel states</returns>
        public string GetPanelStates()
        {
            return $"[ShopUIPanels] Current States - " +
                   $"Daily Summary: {isDailySummaryVisible}, " +
                   $"Price Setting: {isPriceSettingVisible}";
        }
        
        /// <summary>
        /// Validate all panel references for proper assignment.
        /// Called during initialization to ensure UI elements are properly connected.
        /// </summary>
        public void ValidatePanelReferences()
        {
            bool hasErrors = false;
            
            if (dailySummaryPanel == null)
            {
                Debug.LogError("[ShopUIPanels] dailySummaryPanel is not assigned!");
                hasErrors = true;
            }
            
            if (priceSettingPanel == null)
            {
                Debug.LogError("[ShopUIPanels] priceSettingPanel is not assigned!");
                hasErrors = true;
            }
            
            // Daily Summary Elements
            if (dailySummaryRevenueText == null)
            {
                Debug.LogError("[ShopUIPanels] dailySummaryRevenueText is not assigned!");
                hasErrors = true;
            }
            
            if (dailySummaryExpensesText == null)
            {
                Debug.LogError("[ShopUIPanels] dailySummaryExpensesText is not assigned!");
                hasErrors = true;
            }
            
            if (dailySummaryProfitText == null)
            {
                Debug.LogError("[ShopUIPanels] dailySummaryProfitText is not assigned!");
                hasErrors = true;
            }
            
            if (dailySummaryCustomersText == null)
            {
                Debug.LogError("[ShopUIPanels] dailySummaryCustomersText is not assigned!");
                hasErrors = true;
            }
            
            if (dailySummaryDayText == null)
            {
                Debug.LogError("[ShopUIPanels] dailySummaryDayText is not assigned!");
                hasErrors = true;
            }
            
            // Price Setting Elements
            if (currentPriceText == null)
            {
                Debug.LogError("[ShopUIPanels] currentPriceText is not assigned!");
                hasErrors = true;
            }
            
            if (suggestedPriceText == null)
            {
                Debug.LogError("[ShopUIPanels] suggestedPriceText is not assigned!");
                hasErrors = true;
            }
            
            if (!hasErrors)
            {
                Debug.Log("[ShopUIPanels] All panel references validated successfully");
            }
        }
        
        #endregion
    }
}
