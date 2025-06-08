using UnityEngine;
using UnityEngine.Events;
using System;

namespace TabletopShop
{
    /// <summary>
    /// Singleton manager for central economic authority and game state management
    /// Manages economy tracking, day/night cycles, and coordinates with Customer AI and UI systems
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        #region Singleton Pattern
        
        private static GameManager _instance;
        
        /// <summary>
        /// Singleton instance of the GameManager
        /// </summary>
        public static GameManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindAnyObjectByType<GameManager>();
                    
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("GameManager");
                        _instance = go.AddComponent<GameManager>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }
        
        #endregion
        
        #region Fields and Properties
        
        [Header("Economy Configuration")]
        [SerializeField] private float startingMoney = 1000.0f;
        [SerializeField] private float dailyRent = 100.0f;
        [SerializeField] private float dailyUtilities = 50.0f;
        [SerializeField] private bool enableEconomyValidation = true;
        
        [Header("Day/Night Cycle")]
        [SerializeField] private float dayLengthInMinutes = 10.0f;
        [SerializeField] private float nightLengthInMinutes = 2.0f;
        [SerializeField] private bool autoStartCycle = true;
        
        [Header("Customer Economy")]
        [SerializeField] private int maxDailyCustomers = 50;
        [SerializeField] private float customerSpawnInterval = 5.0f;
        [SerializeField] private float baseCustomerSpendingPower = 100.0f;
        
        [Header("Shop Performance")]
        [SerializeField] private float customerSatisfactionMultiplier = 1.0f;
        [SerializeField] private float reputationDecayRate = 0.1f;
        [SerializeField] private bool trackDailyMetrics = true;
        
        [Header("Events")]
        [SerializeField] private UnityEvent onMoneyChanged;
        [SerializeField] private UnityEvent<int> onDayChanged;
        [SerializeField] private UnityEvent<bool> onDayNightCycleChanged;
        [SerializeField] private UnityEvent<float> onReputationChanged;
        
        // Internal economy tracking
        private float currentMoney;
        private int currentDay = 1;
        private bool isDayTime = true;
        private float currentDayTime = 0.0f;
        private float shopReputation = 50.0f; // 0-100 scale
        private int customersServedToday = 0;
        private float dailyRevenue = 0.0f;
        private float dailyExpenses = 0.0f;
        private bool isInitialized = false;
        
        /// <summary>
        /// Current shop money with validation
        /// </summary>
        public float CurrentMoney 
        { 
            get => currentMoney;
            private set
            {
                float previousMoney = currentMoney;
                currentMoney = enableEconomyValidation ? Mathf.Max(0, value) : value;
                
                if (Math.Abs(previousMoney - currentMoney) > 0.01f)
                {
                    onMoneyChanged?.Invoke();
                    Debug.Log($"Money changed: ${previousMoney:F2} -> ${currentMoney:F2}");
                }
            }
        }
        
        /// <summary>
        /// Current day number (starts at 1)
        /// </summary>
        public int CurrentDay => currentDay;
        
        /// <summary>
        /// Whether it's currently day time
        /// </summary>
        public bool IsDayTime => isDayTime;
        
        /// <summary>
        /// Current time within the day cycle (0-1)
        /// </summary>
        public float DayProgress => currentDayTime / (dayLengthInMinutes * 60.0f);
        
        /// <summary>
        /// Current shop reputation (0-100)
        /// </summary>
        public float ShopReputation 
        { 
            get => shopReputation;
            private set
            {
                float previousReputation = shopReputation;
                shopReputation = Mathf.Clamp(value, 0.0f, 100.0f);
                
                if (Math.Abs(previousReputation - shopReputation) > 0.01f)
                {
                    onReputationChanged?.Invoke(shopReputation);
                    Debug.Log($"Reputation changed: {previousReputation:F1} -> {shopReputation:F1}");
                }
            }
        }
        
        /// <summary>
        /// Number of customers served today
        /// </summary>
        public int CustomersServedToday => customersServedToday;
        
        /// <summary>
        /// Today's revenue total
        /// </summary>
        public float DailyRevenue => dailyRevenue;
        
        /// <summary>
        /// Today's expenses total
        /// </summary>
        public float DailyExpenses => dailyExpenses;
        
        /// <summary>
        /// Today's profit (revenue - expenses)
        /// </summary>
        public float DailyProfit => dailyRevenue - dailyExpenses;
        
        /// <summary>
        /// Maximum customers allowed per day
        /// </summary>
        public int MaxDailyCustomers => maxDailyCustomers;
        
        /// <summary>
        /// Base spending power for customers
        /// </summary>
        public float BaseCustomerSpendingPower => baseCustomerSpendingPower;
        
        /// <summary>
        /// Current customer satisfaction multiplier
        /// </summary>
        public float CustomerSatisfactionMultiplier => customerSatisfactionMultiplier;
        
        /// <summary>
        /// Event fired when money changes
        /// </summary>
        public UnityEvent OnMoneyChanged => onMoneyChanged;
        
        /// <summary>
        /// Event fired when day changes
        /// </summary>
        public UnityEvent<int> OnDayChanged => onDayChanged;
        
        /// <summary>
        /// Event fired when day/night cycle changes
        /// </summary>
        public UnityEvent<bool> OnDayNightCycleChanged => onDayNightCycleChanged;
        
        /// <summary>
        /// Event fired when reputation changes
        /// </summary>
        public UnityEvent<float> OnReputationChanged => onReputationChanged;
        
        #endregion
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            // Implement singleton pattern
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeGameManager();
            }
            else if (_instance != this)
            {
                Debug.LogWarning("Multiple GameManager instances detected. Destroying duplicate.");
                Destroy(gameObject);
            }
        }
        
        private void Start()
        {
            if (autoStartCycle)
            {
                StartDayNightCycle();
            }
        }
        
        private void Update()
        {
            if (isInitialized)
            {
                UpdateDayNightCycle();
            }
        }
        
        #endregion
        
        #region Initialization
        
        /// <summary>
        /// Initialize the game manager system
        /// </summary>
        private void InitializeGameManager()
        {
            if (isInitialized) return;
            
            // Initialize economy
            currentMoney = startingMoney;
            currentDay = 1;
            isDayTime = true;
            currentDayTime = 0.0f;
            shopReputation = 50.0f;
            customersServedToday = 0;
            dailyRevenue = 0.0f;
            dailyExpenses = 0.0f;
            
            // Initialize events if they're null
            if (onMoneyChanged == null)
                onMoneyChanged = new UnityEvent();
            if (onDayChanged == null)
                onDayChanged = new UnityEvent<int>();
            if (onDayNightCycleChanged == null)
                onDayNightCycleChanged = new UnityEvent<bool>();
            if (onReputationChanged == null)
                onReputationChanged = new UnityEvent<float>();
            
            isInitialized = true;
            Debug.Log("GameManager initialized successfully");
        }
        
        /// <summary>
        /// Start the day/night cycle system
        /// </summary>
        private void StartDayNightCycle()
        {
            if (!isInitialized) return;
            
            Debug.Log("Day/Night cycle started");
        }
        
        #endregion
        
        #region Economy Management
        
        /// <summary>
        /// Add money to the shop treasury
        /// </summary>
        /// <param name="amount">Amount to add</param>
        /// <param name="source">Source of the money (for tracking)</param>
        public void AddMoney(float amount, string source = "Unknown")
        {
            if (amount <= 0 && enableEconomyValidation)
            {
                Debug.LogWarning($"Attempted to add invalid money amount: {amount}");
                return;
            }
            
            CurrentMoney += amount;
            
            if (isDayTime)
            {
                dailyRevenue += amount;
            }
            
            Debug.Log($"Added ${amount:F2} from {source}. New total: ${CurrentMoney:F2}");
        }
        
        /// <summary>
        /// Subtract money from the shop treasury
        /// </summary>
        /// <param name="amount">Amount to subtract</param>
        /// <param name="source">Source of the expense (for tracking)</param>
        /// <returns>True if transaction was successful</returns>
        public bool SubtractMoney(float amount, string source = "Unknown")
        {
            if (amount <= 0 && enableEconomyValidation)
            {
                Debug.LogWarning($"Attempted to subtract invalid money amount: {amount}");
                return false;
            }
            
            if (enableEconomyValidation && currentMoney < amount)
            {
                Debug.LogWarning($"Insufficient funds for {source}. Required: ${amount:F2}, Available: ${currentMoney:F2}");
                return false;
            }
            
            CurrentMoney -= amount;
            
            if (isDayTime)
            {
                dailyExpenses += amount;
            }
            
            Debug.Log($"Subtracted ${amount:F2} for {source}. New total: ${CurrentMoney:F2}");
            return true;
        }
        
        /// <summary>
        /// Check if the shop has enough money for a transaction
        /// </summary>
        /// <param name="amount">Amount to check</param>
        /// <returns>True if sufficient funds available</returns>
        public bool HasSufficientFunds(float amount)
        {
            return currentMoney >= amount;
        }
        
        /// <summary>
        /// Process a customer purchase (called by Customer AI)
        /// </summary>
        /// <param name="purchaseAmount">Amount of the purchase</param>
        /// <param name="customerSatisfaction">Customer satisfaction level (0-1)</param>
        public void ProcessCustomerPurchase(float purchaseAmount, float customerSatisfaction = 0.8f)
        {
            Debug.Log($"ProcessCustomerPurchase CALLED: amount=${purchaseAmount:F2}, satisfaction={customerSatisfaction:F2}");
            Debug.Log($"BEFORE: customersServedToday={customersServedToday}, currentMoney=${currentMoney:F2}, dailyRevenue=${dailyRevenue:F2}");
            
            if (purchaseAmount <= 0)
            {
                Debug.LogWarning("Invalid purchase amount received from customer");
                return;
            }
            
            // Add money from purchase
            AddMoney(purchaseAmount, "Customer Purchase");
            
            // Update customer metrics
            customersServedToday++;
            Debug.Log($"INCREMENTED customersServedToday to: {customersServedToday}");
            
            // Update reputation based on customer satisfaction
            float reputationChange = (customerSatisfaction - 0.5f) * 2.0f; // -1 to +1 scale
            ModifyReputation(reputationChange);
            
            Debug.Log($"AFTER: customersServedToday={customersServedToday}, currentMoney=${currentMoney:F2}, dailyRevenue=${dailyRevenue:F2}");
            Debug.Log($"Customer purchase processed: ${purchaseAmount:F2}, Satisfaction: {customerSatisfaction:F2}");
        }
        
        /// <summary>
        /// Modify shop reputation
        /// </summary>
        /// <param name="change">Change in reputation (-100 to +100)</param>
        public void ModifyReputation(float change)
        {
            ShopReputation += change;
        }
        
        #endregion
        
        #region Day/Night Cycle
        
        /// <summary>
        /// Update the day/night cycle
        /// </summary>
        private void UpdateDayNightCycle()
        {
            currentDayTime += Time.deltaTime;
            
            float cycleDuration = isDayTime ? dayLengthInMinutes * 60.0f : nightLengthInMinutes * 60.0f;
            
            if (currentDayTime >= cycleDuration)
            {
                ToggleDayNight();
                currentDayTime = 0.0f;
            }
        }
        
        /// <summary>
        /// Toggle between day and night
        /// </summary>
        private void ToggleDayNight()
        {
            isDayTime = !isDayTime;
            
            if (isDayTime)
            {
                // Start new day
                StartNewDay();
            }
            else
            {
                // End day, start night
                EndDay();
            }
            
            onDayNightCycleChanged?.Invoke(isDayTime);
            Debug.Log($"Cycle changed to: {(isDayTime ? "Day" : "Night")}");
        }
        
        /// <summary>
        /// Start a new day
        /// </summary>
        private void StartNewDay()
        {
            currentDay++;
            customersServedToday = 0;
            dailyRevenue = 0.0f;
            dailyExpenses = 0.0f;
            
            onDayChanged?.Invoke(currentDay);
            Debug.Log($"Day {currentDay} started");
        }
        
        /// <summary>
        /// End the current day
        /// </summary>
        private void EndDay()
        {
            // Process daily expenses
            ProcessDailyExpenses();
            
            // Apply reputation decay
            if (reputationDecayRate > 0)
            {
                ModifyReputation(-reputationDecayRate);
            }
            
            Debug.Log($"Day {currentDay} ended. Revenue: ${dailyRevenue:F2}, Expenses: ${dailyExpenses:F2}, Profit: ${DailyProfit:F2}");
        }
        
        /// <summary>
        /// Process daily expenses (rent, utilities, etc.)
        /// </summary>
        private void ProcessDailyExpenses()
        {
            SubtractMoney(dailyRent, "Daily Rent");
            SubtractMoney(dailyUtilities, "Daily Utilities");
        }
        
        #endregion
        
        #region Public API for Integration
        
        /// <summary>
        /// Get current economic status for UI display
        /// </summary>
        /// <returns>Economic status data</returns>
        public (float money, int day, bool isDay, float reputation, int customers, float revenue, float expenses) GetEconomicStatus()
        {
            return (CurrentMoney, CurrentDay, IsDayTime, ShopReputation, CustomersServedToday, DailyRevenue, DailyExpenses);
        }
        
        /// <summary>
        /// Force advance to next day (for testing/debugging)
        /// </summary>
        [ContextMenu("Force Next Day")]
        public void ForceNextDay()
        {
            if (!isDayTime)
            {
                ToggleDayNight(); // Switch to day
            }
            else
            {
                ToggleDayNight(); // Switch to night
                ToggleDayNight(); // Switch to next day
            }
        }
        
        /// <summary>
        /// Reset economy to starting values (for testing/new game)
        /// </summary>
        [ContextMenu("Reset Economy")]
        public void ResetEconomy()
        {
            currentMoney = startingMoney;
            currentDay = 1;
            isDayTime = true;
            currentDayTime = 0.0f;
            shopReputation = 50.0f;
            customersServedToday = 0;
            dailyRevenue = 0.0f;
            dailyExpenses = 0.0f;
            
            onMoneyChanged?.Invoke();
            onDayChanged?.Invoke(currentDay);
            onDayNightCycleChanged?.Invoke(isDayTime);
            onReputationChanged?.Invoke(shopReputation);
            
            Debug.Log("Economy reset to starting values");
        }
        
        #endregion
    }
}
