using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

namespace TabletopShop
{
    /// <summary>
    /// Unified button action types for the ShopUI control system.
    /// Provides centralized button behavior management with enum-based actions.
    /// </summary>
    public enum ShopUIAction
    {
        EndDay,
        Pause,
        InventoryToggle,
        Settings,
        DailySummaryContinue,
        ConfirmPrice,
        CancelPrice
    }

    /// <summary>
    /// Handles all button interactions and control logic for the ShopUI system.
    /// Extracted from ShopUI.cs to separate control concerns from display logic.
    /// Uses a unified enum-based action system for consistent button behavior.
    /// 
    /// Key Features:
    /// - Unified action system using ShopUIAction enum
    /// - Centralized button event management
    /// - Clean separation from display logic
    /// - Null-safe reference handling
    /// - Comprehensive logging and error handling
    /// </summary>
    public class ShopUIControls : MonoBehaviour
    {
        #region Events
        
        /// <summary>
        /// Event triggered when daily summary should be shown.
        /// </summary>
        public UnityEvent OnShowDailySummary = new UnityEvent();
        
        /// <summary>
        /// Event triggered when daily summary should be hidden.
        /// </summary>
        public UnityEvent OnHideDailySummary = new UnityEvent();
        
        /// <summary>
        /// Event triggered when price setting should be hidden.
        /// </summary>
        public UnityEvent OnHidePriceSetting = new UnityEvent();
        
        #endregion
        #region Button References
        
        [Header("Main Control Buttons")]
        [Tooltip("Button to end the current day and proceed to next day")]
        [SerializeField] private Button endDayButton;
        
        [Tooltip("Button to pause/unpause the game simulation")]
        [SerializeField] private Button pauseButton;
        
        [Tooltip("Button to toggle inventory panel visibility")]
        [SerializeField] private Button inventoryToggleButton;
        
        [Tooltip("Button to open settings/options panel")]
        [SerializeField] private Button settingsButton;
        
        [Header("Dialog Buttons")]
        [Tooltip("Button to continue to next day from daily summary")]
        [SerializeField] private Button dailySummaryContinueButton;
        
        [Tooltip("Button to confirm price change")]
        [SerializeField] private Button confirmPriceButton;
        
        [Tooltip("Button to cancel price change")]
        [SerializeField] private Button cancelPriceButton;
        
        [Header("Input Elements")]
        [Tooltip("Input field for entering new product price")]
        [SerializeField] private TMP_InputField priceInputField;
        
        #endregion
        
        #region Component References
        
        /// <summary>
        /// Cached reference to GameManager for game state management.
        /// </summary>
        private GameManager gameManager;
        
        /// <summary>
        /// Reference to InventoryUI for inventory panel toggle functionality.
        /// </summary>
        private InventoryUI inventoryUI;
        
        /// <summary>
        /// Reference to SettingsUI for settings panel management.
        /// </summary>
        private SettingsUI settingsUI;
        
        #endregion
        
        #region State Variables
        
        /// <summary>
        /// Current pause state of the game.
        /// </summary>
        private bool isGamePaused = false;
        
        /// <summary>
        /// Reference to the product currently being price-adjusted.
        /// </summary>
        private Product currentPricingProduct = null;
        
        /// <summary>
        /// Button text constants for pause/resume functionality.
        /// </summary>
        private const string PAUSE_TEXT = "Pause";
        private const string RESUME_TEXT = "Resume";
        
        #endregion
        
        #region Unity Lifecycle
        
        /// <summary>
        /// Initialize component references and setup button event handlers.
        /// </summary>
        private void Start()
        {
            InitializeReferences();
            SetupButtonEventHandlers();
        }
        
        /// <summary>
        /// Clean up button event handlers to prevent memory leaks.
        /// </summary>
        private void OnDestroy()
        {
            CleanupButtonEventHandlers();
        }
        
        #endregion
        
        #region Initialization
        
        /// <summary>
        /// Initialize references to required components.
        /// </summary>
        private void InitializeReferences()
        {
            // Get GameManager instance
            gameManager = GameManager.Instance;
            if (gameManager == null)
            {
                Debug.LogWarning("[ShopUIControls] GameManager.Instance is null during initialization. Will retry during actions.");
            }
            
            // Find InventoryUI in scene
            inventoryUI = FindFirstObjectByType<InventoryUI>();
            if (inventoryUI == null)
            {
                Debug.LogWarning("[ShopUIControls] InventoryUI component not found in scene. Inventory toggle will be unavailable.");
            }
            
            // Find SettingsUI in scene
            settingsUI = FindFirstObjectByType<SettingsUI>();
            if (settingsUI == null)
            {
//                Debug.LogWarning("[ShopUIControls] SettingsUI component not found in scene. Settings toggle will be unavailable.");
            }
            
//            Debug.Log("[ShopUIControls] Component references initialized");
        }
        
        #endregion
        
        #region Button Event Setup
        
        /// <summary>
        /// Setup button event handlers using the unified action system.
        /// Each button is mapped to a specific ShopUIAction enum value.
        /// </summary>
        private void SetupButtonEventHandlers()
        {
            // Setup main control buttons
            SetupButton(endDayButton, ShopUIAction.EndDay, "End Day");
            SetupButton(pauseButton, ShopUIAction.Pause, "Pause");
            SetupButton(inventoryToggleButton, ShopUIAction.InventoryToggle, "Inventory Toggle");
            SetupButton(settingsButton, ShopUIAction.Settings, "Settings");
            
            // Setup dialog buttons
            SetupButton(dailySummaryContinueButton, ShopUIAction.DailySummaryContinue, "Daily Summary Continue");
            SetupButton(confirmPriceButton, ShopUIAction.ConfirmPrice, "Confirm Price");
            SetupButton(cancelPriceButton, ShopUIAction.CancelPrice, "Cancel Price");
            
            // Setup price input field
            if (priceInputField != null)
            {
                priceInputField.onEndEdit.AddListener(OnPriceInputEndEdit);
                Debug.Log("[ShopUIControls] Price input field event handler added");
            }
            else
            {
                Debug.LogWarning("[ShopUIControls] Price input field reference is null - input functionality unavailable");
            }
        }
        
        /// <summary>
        /// Helper method to setup individual button with unified action handler.
        /// </summary>
        /// <param name="button">The button to setup</param>
        /// <param name="action">The action to associate with the button</param>
        /// <param name="buttonName">Name for logging purposes</param>
        private void SetupButton(Button button, ShopUIAction action, string buttonName)
        {
            if (button != null)
            {
                button.onClick.AddListener(() => HandleButtonAction(action));
////                Debug.Log($"[ShopUIControls] {buttonName} button event handler added");
            }
            else
            {
                Debug.LogWarning($"[ShopUIControls] {buttonName} button reference is null - button functionality unavailable");
            }
        }
        
        /// <summary>
        /// Clean up button event handlers to prevent memory leaks.
        /// </summary>
        private void CleanupButtonEventHandlers()
        {
            // Clean up main control buttons
            CleanupButton(endDayButton, "End Day");
            CleanupButton(pauseButton, "Pause");
            CleanupButton(inventoryToggleButton, "Inventory Toggle");
            CleanupButton(settingsButton, "Settings");
            
            // Clean up dialog buttons
            CleanupButton(dailySummaryContinueButton, "Daily Summary Continue");
            CleanupButton(confirmPriceButton, "Confirm Price");
            CleanupButton(cancelPriceButton, "Cancel Price");
            
            // Clean up price input field
            if (priceInputField != null)
            {
                priceInputField.onEndEdit.RemoveListener(OnPriceInputEndEdit);
            }
            
            Debug.Log("[ShopUIControls] Button event handlers cleaned up");
        }
        
        /// <summary>
        /// Helper method to clean up individual button event handlers.
        /// </summary>
        /// <param name="button">The button to clean up</param>
        /// <param name="buttonName">Name for logging purposes</param>
        private void CleanupButton(Button button, string buttonName)
        {
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
            }
        }
        
        #endregion
        
        #region Unified Action Handler
        
        /// <summary>
        /// Unified button action handler that processes all button clicks.
        /// Uses enum-based routing to execute the appropriate action.
        /// Provides consistent behavior, logging, and error handling.
        /// </summary>
        /// <param name="action">The action to execute</param>
        private void HandleButtonAction(ShopUIAction action)
        {
            // Play UI click sound for all actions
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayUIClick();
            }
            
            // Route to appropriate action handler
            switch (action)
            {
                case ShopUIAction.EndDay:
                    HandleEndDayAction();
                    break;
                    
                case ShopUIAction.Pause:
                    HandlePauseAction();
                    break;
                    
                case ShopUIAction.InventoryToggle:
                    HandleInventoryToggleAction();
                    break;
                    
                case ShopUIAction.Settings:
                    HandleSettingsAction();
                    break;
                    
                case ShopUIAction.DailySummaryContinue:
                    HandleDailySummaryContinueAction();
                    break;
                    
                case ShopUIAction.ConfirmPrice:
                    HandleConfirmPriceAction();
                    break;
                    
                case ShopUIAction.CancelPrice:
                    HandleCancelPriceAction();
                    break;
                    
                default:
                    Debug.LogWarning($"[ShopUIControls] Unhandled action: {action}");
                    break;
            }
        }
        
        #endregion
        
        #region Action Implementations
        
        /// <summary>
        /// Handle end day action - shows daily summary popup before advancing to next day.
        /// </summary>
        private void HandleEndDayAction()
        {
            if (gameManager != null)
            {
                OnShowDailySummary.Invoke();
                Debug.Log("[ShopUIControls] End day action executed - showing daily summary");
            }
            else
            {
                Debug.LogError("[ShopUIControls] Cannot execute end day action - GameManager unavailable");
            }
        }
        
        /// <summary>
        /// Handle pause action - toggles game pause state using Time.timeScale.
        /// </summary>
        private void HandlePauseAction()
        {
            if (pauseButton == null) return;
            
            // Toggle pause state
            isGamePaused = !isGamePaused;
            
            // Update Time.timeScale to pause/resume the game
            if (isGamePaused)
            {
                Time.timeScale = 0f; // Pause all time-based systems
                UpdatePauseButtonText(RESUME_TEXT);
                Debug.Log("[ShopUIControls] Game paused (Time.timeScale = 0)");
            }
            else
            {
                Time.timeScale = 1f; // Resume normal time
                UpdatePauseButtonText(PAUSE_TEXT);
                Debug.Log("[ShopUIControls] Game resumed (Time.timeScale = 1)");
            }
        }
        
        /// <summary>
        /// Handle inventory toggle action - toggles inventory panel visibility.
        /// </summary>
        private void HandleInventoryToggleAction()
        {
            if (inventoryUI != null)
            {
                inventoryUI.TogglePanel();
                Debug.Log("[ShopUIControls] Inventory toggle action executed");
            }
            else
            {
                Debug.LogWarning("[ShopUIControls] InventoryUI reference is null! Cannot toggle inventory panel.");
            }
        }
        
        /// <summary>
        /// Handle settings action - toggles settings panel visibility.
        /// </summary>
        private void HandleSettingsAction()
        {
            if (settingsUI != null)
            {
                settingsUI.ToggleSettings();
                Debug.Log("[ShopUIControls] Settings action executed");
            }
            else
            {
                Debug.LogWarning("[ShopUIControls] SettingsUI reference is null! Cannot toggle settings panel.");
            }
        }
        
        /// <summary>
        /// Handle daily summary continue action - proceeds to next day.
        /// </summary>
        private void HandleDailySummaryContinueAction()
        {
            OnHideDailySummary.Invoke();
            Debug.Log("[ShopUIControls] Daily summary continue action executed");
        }
        
        /// <summary>
        /// Handle confirm price action - applies new price to current product.
        /// </summary>
        private void HandleConfirmPriceAction()
        {
            if (currentPricingProduct == null)
            {
                Debug.LogError("[ShopUIControls] Cannot confirm price - no product selected!");
                OnHidePriceSetting.Invoke();
                return;
            }
            
            if (priceInputField == null)
            {
                Debug.LogError("[ShopUIControls] Cannot confirm price - price input field is null!");
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
                    
                    Debug.Log($"[ShopUIControls] Price updated for {currentPricingProduct.ProductData.ProductName}: " +
                             $"{UIFormatting.FormatCurrency(newPriceInt)}");
                    
                    // Hide the price setting panel
                    OnHidePriceSetting.Invoke();
                    
                    // Clear current product reference
                    currentPricingProduct = null;
                }
                else
                {
                    Debug.LogWarning($"[ShopUIControls] Invalid price range: {newPrice}. Price must be between $1 and $10,000.");
                    
                    // Reset input field to current price
                    if (priceInputField != null && currentPricingProduct != null)
                    {
                        priceInputField.text = currentPricingProduct.CurrentPrice.ToString("F0");
                    }
                }
            }
            else
            {
                Debug.LogWarning($"[ShopUIControls] Invalid price input: '{priceInputField.text}'. Please enter a valid number.");
                
                // Reset input field to current price
                if (priceInputField != null && currentPricingProduct != null)
                {
                    priceInputField.text = currentPricingProduct.CurrentPrice.ToString("F0");
                }
            }
        }
        
        /// <summary>
        /// Handle cancel price action - closes price setting without changes.
        /// </summary>
        private void HandleCancelPriceAction()
        {
            Debug.Log("[ShopUIControls] Price setting cancelled by user");
            
            OnHidePriceSetting.Invoke();
            
            // Clear current product reference
            currentPricingProduct = null;
        }
        
        #endregion
        
        #region Helper Methods
        
        /// <summary>
        /// Update pause button text with null-safe TextMeshPro access.
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
                Debug.LogWarning("[ShopUIControls] Pause button does not have a TextMeshProUGUI component in children. " +
                               "Cannot update button text.");
            }
        }
        
        /// <summary>
        /// Handle price input field end edit - supports Enter key to confirm.
        /// </summary>
        /// <param name="inputText">The text entered in the input field</param>
        private void OnPriceInputEndEdit(string inputText)
        {
            // Check if Enter was pressed to confirm
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                HandleButtonAction(ShopUIAction.ConfirmPrice);
            }
            // Check if Escape was pressed to cancel
            else if (Input.GetKeyDown(KeyCode.Escape))
            {
                HandleButtonAction(ShopUIAction.CancelPrice);
            }
        }
        
        #endregion
        
        #region Public Interface
        
        /// <summary>
        /// Set the current product for price setting operations.
        /// Called by ShopUI when price setting is initiated.
        /// </summary>
        /// <param name="product">The product to set price for</param>
        public void SetCurrentPricingProduct(Product product)
        {
            currentPricingProduct = product;
            
            // Initialize input field with current price if available
            if (priceInputField != null && product != null)
            {
                priceInputField.text = product.CurrentPrice.ToString("F0");
                priceInputField.Select();
                priceInputField.ActivateInputField();
            }
            
            Debug.Log($"[ShopUIControls] Current pricing product set: {(product != null ? product.ProductData.ProductName : "null")}");
        }
        
        /// <summary>
        /// Clear the current pricing product reference.
        /// Called when price setting is cancelled or completed.
        /// </summary>
        public void ClearCurrentPricingProduct()
        {
            currentPricingProduct = null;
            Debug.Log("[ShopUIControls] Current pricing product cleared");
        }
        
        /// <summary>
        /// Get the current pause state of the game.
        /// </summary>
        public bool IsGamePaused => isGamePaused;
        
        /// <summary>
        /// Trigger a specific action programmatically.
        /// Useful for testing and external integrations.
        /// </summary>
        /// <param name="action">The action to trigger</param>
        public void TriggerAction(ShopUIAction action)
        {
            HandleButtonAction(action);
        }
        
        #endregion
    }
}
