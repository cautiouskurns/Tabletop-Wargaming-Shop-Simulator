using UnityEngine;

namespace TabletopShop
{
    /// <summary>
    /// Singleton manager for customer behavior settings
    /// Provides easy access to centralized AI configuration across all tasks
    /// </summary>
    public class CustomerBehaviorSettingsManager : MonoBehaviour
    {
        [Header("Settings Configuration")]
        [Tooltip("The main customer behavior settings asset")]
        [SerializeField] private CustomerBehaviorSettings settings;
        
        [Header("Runtime Options")]
        [Tooltip("Create default settings if none are assigned")]
        [SerializeField] private bool createDefaultIfMissing = true;
        
        // Singleton instance
        private static CustomerBehaviorSettingsManager _instance;
        private static readonly object _lock = new object();
        
        /// <summary>
        /// Singleton instance access
        /// </summary>
        public static CustomerBehaviorSettingsManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            // Try to find existing instance
                            _instance = FindFirstObjectByType<CustomerBehaviorSettingsManager>();
                            
                            if (_instance == null)
                            {
                                // Create new instance
                                GameObject go = new GameObject("CustomerBehaviorSettingsManager");
                                _instance = go.AddComponent<CustomerBehaviorSettingsManager>();
                                DontDestroyOnLoad(go);
                                
                                Debug.Log("[CustomerBehaviorSettingsManager] Created new instance");
                            }
                        }
                    }
                }
                return _instance;
            }
        }
        
        /// <summary>
        /// Current settings (with fallback to defaults)
        /// </summary>
        public static CustomerBehaviorSettings Settings
        {
            get
            {
                var manager = Instance;
                if (manager.settings == null)
                {
                    if (manager.createDefaultIfMissing)
                    {
                        manager.CreateDefaultSettings();
                    }
                    else
                    {
                        Debug.LogError("[CustomerBehaviorSettingsManager] No settings assigned and createDefaultIfMissing is false!");
                        return null;
                    }
                }
                return manager.settings;
            }
        }
        
        /// <summary>
        /// Quick access to shopping settings
        /// </summary>
        public static ShoppingSettings Shopping => Settings?.shopping;
        
        /// <summary>
        /// Quick access to checkout settings
        /// </summary>
        public static CheckoutSettings Checkout => Settings?.checkout;
        
        private void Awake()
        {
            // Singleton pattern enforcement
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
                
                // Initialize settings if needed
                if (settings == null && createDefaultIfMissing)
                {
                    CreateDefaultSettings();
                }
                
                Debug.Log("[CustomerBehaviorSettingsManager] Initialized with settings: " + 
                         (settings != null ? settings.name : "Default Runtime Settings"));
            }
            else if (_instance != this)
            {
                Debug.LogWarning("[CustomerBehaviorSettingsManager] Duplicate instance detected, destroying...");
                Destroy(gameObject);
            }
        }
        
        /// <summary>
        /// Create default settings at runtime
        /// </summary>
        private void CreateDefaultSettings()
        {
            settings = ScriptableObject.CreateInstance<CustomerBehaviorSettings>();
            settings.name = "Default Runtime Settings";
            
            Debug.Log("[CustomerBehaviorSettingsManager] Created default runtime settings");
        }
        
        /// <summary>
        /// Assign new settings asset (useful for runtime switching or testing)
        /// </summary>
        /// <param name="newSettings">New settings to use</param>
        public void SetSettings(CustomerBehaviorSettings newSettings)
        {
            if (newSettings != null)
            {
                settings = newSettings;
                Debug.Log($"[CustomerBehaviorSettingsManager] Settings changed to: {newSettings.name}");
            }
            else
            {
                Debug.LogWarning("[CustomerBehaviorSettingsManager] Attempted to set null settings");
            }
        }
        
        /// <summary>
        /// Validate current settings and log any issues
        /// </summary>
        [ContextMenu("Validate Settings")]
        public void ValidateSettings()
        {
            if (Settings == null)
            {
                Debug.LogError("[CustomerBehaviorSettingsManager] No settings available for validation");
                return;
            }
            
            Debug.Log("[CustomerBehaviorSettingsManager] Settings validation:");
            Debug.Log(Settings.GetSettingsSummary());
            
            // Additional runtime validation
            if (Shopping.buyProbability <= 0)
                Debug.LogWarning("Buy probability is very low - customers may not purchase anything");
            
            if (Checkout.maxQueueWaitTime < 10f)
                Debug.LogWarning("Queue wait time is very short - customers may leave quickly");
            
            if (Shopping.maxProducts <= 0)
                Debug.LogError("Max products must be greater than 0");
        }
        
        /// <summary>
        /// Reset to default settings
        /// </summary>
        [ContextMenu("Reset to Defaults")]
        public void ResetToDefaults()
        {
            CreateDefaultSettings();
            Debug.Log("[CustomerBehaviorSettingsManager] Reset to default settings");
        }
        
        #region Editor Support
        
#if UNITY_EDITOR
        /// <summary>
        /// Create settings asset in project (Editor only)
        /// </summary>
        [ContextMenu("Create Settings Asset")]
        public void CreateSettingsAsset()
        {
            var newSettings = ScriptableObject.CreateInstance<CustomerBehaviorSettings>();
            
            // Create directory if it doesn't exist
            string path = "Assets/Data";
            if (!UnityEditor.AssetDatabase.IsValidFolder(path))
            {
                UnityEditor.AssetDatabase.CreateFolder("Assets", "Data");
            }
            
            // Create the asset
            string assetPath = $"{path}/CustomerBehaviorSettings.asset";
            UnityEditor.AssetDatabase.CreateAsset(newSettings, assetPath);
            UnityEditor.AssetDatabase.SaveAssets();
            
            // Assign it to this manager
            settings = newSettings;
            
            Debug.Log($"[CustomerBehaviorSettingsManager] Created settings asset at: {assetPath}");
        }
#endif
        
        #endregion
    }
}