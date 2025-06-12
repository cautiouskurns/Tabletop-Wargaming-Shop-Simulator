using UnityEngine;

namespace TabletopShop
{
    /// <summary>
    /// UI component that displays basic economic status information from GameManager
    /// Provides real-time feedback on money, day progress, customers served, and shop reputation
    /// Uses event-driven updates for efficient performance and follows existing UI component patterns
    /// </summary>
    public class ShopStatusUI : MonoBehaviour
    {
        [Header("Display Settings")]
        [SerializeField] private bool showStatusUI = true;
        [SerializeField] private bool enableDebugInfo = false;
        
        [Header("Position and Styling")]
        [SerializeField] private Vector2 displayPosition = new Vector2(10, 10);
        [SerializeField] private int fontSize = 14;
        [SerializeField] private Color textColor = Color.white;
        [SerializeField] private Color backgroundTint = new Color(0, 0, 0, 0.3f);
        
        [Header("Content Configuration")]
        [SerializeField] private bool showMoney = true;
        [SerializeField] private bool showDayInfo = true;
        [SerializeField] private bool showCustomerMetrics = true;
        [SerializeField] private bool showReputation = true;
        [SerializeField] private bool showDayNightCycle = true;
        
        // Cached economic data for display
        private float currentMoney;
        private int currentDay;
        private bool isDayTime;
        private float shopReputation;
        private int customersServedToday;
        private float dailyRevenue;
        private float dailyExpenses;
        private int maxDailyCustomers;
        
        // UI state
        private bool isInitialized = false;
        private GUIStyle textStyle;
        private GUIStyle backgroundStyle;
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            // Initialize UI styles - this runs before GameManager events are available
            InitializeUIStyles();
        }
        
        private void Start()
        {
            // Subscribe to GameManager events for real-time updates
            // This is the event-driven pattern used by other UI components like InventoryUI
            SubscribeToGameManagerEvents();
            
            // Initial data fetch if GameManager is available
            RefreshEconomicData();
            
            isInitialized = true;
            Debug.Log("ShopStatusUI: Initialized and subscribed to GameManager events");
        }
        
        private void OnDestroy()
        {
            // Clean unsubscribe pattern to prevent memory leaks
            // This follows the same cleanup pattern as InventoryUI and other components
            UnsubscribeFromGameManagerEvents();
        }
        
        private void OnGUI()
        {
            // OnGUI implementation for rapid prototyping - can be replaced with Canvas UI later
            // This approach is mentioned in requirements for quick implementation
            if (!showStatusUI || !isInitialized) return;
            
            DisplayEconomicStatus();
        }
        
        #endregion
        
        #region Event Subscription Management
        
        /// <summary>
        /// Subscribe to GameManager UnityEvents for real-time updates
        /// This is the event-driven update pattern rather than polling in Update()
        /// </summary>
        private void SubscribeToGameManagerEvents()
        {
            // Null-safe GameManager.Instance access with error handling
            if (GameManager.Instance != null)
            {
                // Subscribe to economic events for real-time updates
                GameManager.Instance.OnMoneyChanged.AddListener(OnMoneyChanged);
                GameManager.Instance.OnDayChanged.AddListener(OnDayChanged);
                GameManager.Instance.OnDayNightCycleChanged.AddListener(OnDayNightCycleChanged);
                GameManager.Instance.OnReputationChanged.AddListener(OnReputationChanged);
                
                Debug.Log("ShopStatusUI: Successfully subscribed to GameManager events");
            }
            else
            {
                Debug.LogWarning("ShopStatusUI: GameManager.Instance is null during event subscription. Will retry on next refresh.");
            }
        }
        
        /// <summary>
        /// Unsubscribe from GameManager events to prevent memory leaks
        /// Essential for proper component lifecycle and event management
        /// </summary>
        private void UnsubscribeFromGameManagerEvents()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnMoneyChanged.RemoveListener(OnMoneyChanged);
                GameManager.Instance.OnDayChanged.RemoveListener(OnDayChanged);
                GameManager.Instance.OnDayNightCycleChanged.RemoveListener(OnDayNightCycleChanged);
                GameManager.Instance.OnReputationChanged.RemoveListener(OnReputationChanged);
                
                Debug.Log("ShopStatusUI: Unsubscribed from GameManager events");
            }
        }
        
        #endregion
        
        #region Event Handlers
        
        /// <summary>
        /// Handle money changes from GameManager
        /// Event-driven update triggered when money changes
        /// </summary>
        private void OnMoneyChanged()
        {
            if (GameManager.Instance != null)
            {
                currentMoney = GameManager.Instance.CurrentMoney;
                dailyRevenue = GameManager.Instance.DailyRevenue;
                dailyExpenses = GameManager.Instance.DailyExpenses;
                
                if (enableDebugInfo)
                {
                    Debug.Log($"ShopStatusUI: Money updated - ${currentMoney:F2}");
                }
            }
        }
        
        /// <summary>
        /// Handle day changes from GameManager
        /// Event-driven update triggered when day advances
        /// </summary>
        /// <param name="newDay">The new day number</param>
        private void OnDayChanged(int newDay)
        {
            currentDay = newDay;
            
            // Refresh all data when day changes to ensure accuracy
            RefreshEconomicData();
            
            if (enableDebugInfo)
            {
                Debug.Log($"ShopStatusUI: Day changed to {newDay}");
            }
        }
        
        /// <summary>
        /// Handle day/night cycle changes from GameManager
        /// Event-driven update triggered when cycle changes
        /// </summary>
        /// <param name="isDay">True if it's now day time</param>
        private void OnDayNightCycleChanged(bool isDay)
        {
            isDayTime = isDay;
            
            if (enableDebugInfo)
            {
                Debug.Log($"ShopStatusUI: Day/Night cycle changed - {(isDay ? "Day" : "Night")}");
            }
        }
        
        /// <summary>
        /// Handle reputation changes from GameManager
        /// Event-driven update triggered when reputation changes
        /// </summary>
        /// <param name="newReputation">The new reputation value</param>
        private void OnReputationChanged(float newReputation)
        {
            shopReputation = newReputation;
            
            if (enableDebugInfo)
            {
                Debug.Log($"ShopStatusUI: Reputation updated - {newReputation:F1}");
            }
        }
        
        #endregion
        
        #region Data Management
        
        /// <summary>
        /// Refresh all economic data from GameManager
        /// Used for initial load and when events might have been missed
        /// </summary>
        private void RefreshEconomicData()
        {
            // Null-safe access pattern with graceful fallback
            if (GameManager.Instance != null)
            {
                var economicStatus = GameManager.Instance.GetEconomicStatus();
                
                currentMoney = economicStatus.money;
                currentDay = economicStatus.day;
                isDayTime = economicStatus.isDay;
                shopReputation = economicStatus.reputation;
                customersServedToday = economicStatus.customers;
                dailyRevenue = economicStatus.revenue;
                dailyExpenses = economicStatus.expenses;
                maxDailyCustomers = GameManager.Instance.MaxDailyCustomers;
                
                // Retry event subscription if it failed earlier
                if (GameManager.Instance.OnMoneyChanged.GetPersistentEventCount() == 0)
                {
                    SubscribeToGameManagerEvents();
                }
            }
            else
            {
                // Graceful fallback when GameManager is unavailable
                if (enableDebugInfo)
                {
                    Debug.LogWarning("ShopStatusUI: GameManager unavailable during data refresh");
                }
            }
        }
        
        #endregion
        
        #region UI Rendering
        
        /// <summary>
        /// Initialize GUI styles for consistent appearance
        /// This separates style setup from rendering logic
        /// </summary>
        private void InitializeUIStyles()
        {
            // Text style for economic data display
            textStyle = new GUIStyle();
            textStyle.fontSize = fontSize;
            textStyle.normal.textColor = textColor;
            textStyle.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            
            // Background style for better readability
            backgroundStyle = new GUIStyle();
            backgroundStyle.normal.background = CreateBackgroundTexture();
        }
        
        /// <summary>
        /// Main UI display method using OnGUI for rapid prototyping
        /// OnGUI vs Canvas UI trade-offs:
        /// - OnGUI: Quick to implement, immediate mode, good for debugging/prototypes
        /// - Canvas: Better performance, more flexible styling, better for production
        /// This uses OnGUI for MVP as specified in requirements
        /// </summary>
        private void DisplayEconomicStatus()
        {
            // Position in top-left corner with non-intrusive design
            float yOffset = displayPosition.y;
            float lineHeight = fontSize + 4;
            float boxWidth = 250;
            
            // Calculate total height needed
            int lineCount = 0;
            if (showMoney) lineCount += 3; // Money, Revenue, Expenses
            if (showDayInfo) lineCount += 1;
            if (showDayNightCycle) lineCount += 1;
            if (showCustomerMetrics) lineCount += 1;
            if (showReputation) lineCount += 1;
            
            float totalHeight = lineCount * lineHeight + 10;
            
            // Draw semi-transparent background for better readability
            if (backgroundStyle?.normal?.background != null)
            {
                GUI.backgroundColor = backgroundTint;
                GUI.Box(new Rect(displayPosition.x, yOffset, boxWidth, totalHeight), "", backgroundStyle);
                GUI.backgroundColor = Color.white;
            }
            
            // Display economic information with configurable sections
            yOffset += 5; // Padding from background edge
            
            if (showMoney)
            {
                var style = textStyle ?? GUI.skin.label;
                GUI.Label(new Rect(displayPosition.x + 5, yOffset, boxWidth - 10, lineHeight), 
                         $"Money: ${currentMoney:F2}", style);
                yOffset += lineHeight;
                
                GUI.Label(new Rect(displayPosition.x + 5, yOffset, boxWidth - 10, lineHeight), 
                         $"Revenue: ${dailyRevenue:F2}", style);
                yOffset += lineHeight;
                
                GUI.Label(new Rect(displayPosition.x + 5, yOffset, boxWidth - 10, lineHeight), 
                         $"Expenses: ${dailyExpenses:F2}", style);
                yOffset += lineHeight;
            }
            
            if (showDayInfo)
            {
                var style = textStyle ?? GUI.skin.label;
                GUI.Label(new Rect(displayPosition.x + 5, yOffset, boxWidth - 10, lineHeight), 
                         $"Day: {currentDay}", style);
                yOffset += lineHeight;
            }
            
            if (showDayNightCycle)
            {
                var style = textStyle ?? GUI.skin.label;
                string timeOfDay = isDayTime ? "Day" : "Night";
                GUI.Label(new Rect(displayPosition.x + 5, yOffset, boxWidth - 10, lineHeight), 
                         $"Time: {timeOfDay}", style);
                yOffset += lineHeight;
            }
            
            if (showCustomerMetrics)
            {
                var style = textStyle ?? GUI.skin.label;
                GUI.Label(new Rect(displayPosition.x + 5, yOffset, boxWidth - 10, lineHeight), 
                         $"Customers: {customersServedToday}/{maxDailyCustomers}", style);
                yOffset += lineHeight;
            }
            
            if (showReputation)
            {
                var style = textStyle ?? GUI.skin.label;
                GUI.Label(new Rect(displayPosition.x + 5, yOffset, boxWidth - 10, lineHeight), 
                         $"Reputation: {shopReputation:F1}/100", style);
                yOffset += lineHeight;
            }
        }
        
        /// <summary>
        /// Create a simple background texture for better text readability
        /// </summary>
        /// <returns>A semi-transparent background texture</returns>
        private Texture2D CreateBackgroundTexture()
        {
            try
            {
                Texture2D texture = new Texture2D(1, 1);
                texture.SetPixel(0, 0, new Color(0, 0, 0, 0.5f));
                texture.Apply();
                Debug.Log("ShopStatusUI: Background texture created successfully");
                return texture;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"ShopStatusUI: Failed to create background texture: {e.Message}");
                return null;
            }
        }
        
        #endregion
        
        #region Public API
        
        /// <summary>
        /// Toggle the visibility of the status UI
        /// Public API for external control if needed
        /// </summary>
        /// <param name="visible">Whether the status UI should be visible</param>
        public void SetVisible(bool visible)
        {
            showStatusUI = visible;
        }
        
        /// <summary>
        /// Force refresh of economic data
        /// Useful for manual updates or debugging
        /// </summary>
        [ContextMenu("Refresh Economic Data")]
        public void ForceRefresh()
        {
            RefreshEconomicData();
            Debug.Log("ShopStatusUI: Forced data refresh completed");
        }
        
        /// <summary>
        /// Configure which economic metrics to display
        /// Allows customization of what information is shown
        /// </summary>
        /// <param name="money">Show money information</param>
        /// <param name="dayInfo">Show day number</param>
        /// <param name="customers">Show customer metrics</param>
        /// <param name="reputation">Show reputation</param>
        /// <param name="dayNight">Show day/night cycle</param>
        public void ConfigureDisplay(bool money = true, bool dayInfo = true, bool customers = true, bool reputation = true, bool dayNight = true)
        {
            showMoney = money;
            showDayInfo = dayInfo;
            showCustomerMetrics = customers;
            showReputation = reputation;
            showDayNightCycle = dayNight;
        }
        
        #endregion
        
        #region Expansion Points
        
        // NOTE: Expansion points for advanced economic displays:
        // 1. Add profit/loss indicators with color coding
        // 2. Implement trend arrows for reputation changes
        // 3. Add customer satisfaction metrics display
        // 4. Create time-based progress bars for day/night cycle
        // 5. Add inventory value summary
        // 6. Implement notification system for important economic events
        // 7. Add export/logging functionality for economic tracking
        // 8. Create graphical elements (charts, meters) for visual feedback
        // 9. Add tooltip system for detailed explanations
        // 10. Implement customizable alert thresholds for low money/reputation
        
        #endregion
    }
}
