using UnityEngine;
using UnityEditor;

namespace TabletopShop
{
    /// <summary>
    /// Debug utility for testing and monitoring customer behavior settings
    /// Provides runtime information about current settings configuration
    /// </summary>
    public class CustomerBehaviorSettingsDebugger : MonoBehaviour
    {
        [Header("Debug Display")]
        [SerializeField] private bool showSettingsInGUI = true;
        [SerializeField] private bool logSettingsOnStart = true;
        
        [Header("Runtime Testing")]
        [SerializeField] private CustomerBehaviorSettings testSettings;
        
        private void Start()
        {
            if (logSettingsOnStart)
            {
                LogCurrentSettings();
            }
        }
        
        private void OnGUI()
        {
            if (!showSettingsInGUI) return;
            
            GUILayout.BeginArea(new Rect(10, 10, 400, 300));
            GUILayout.BeginVertical("box");
            
            GUILayout.Label("Customer Behavior Settings Debug", EditorStyles.whiteLargeLabel);
            
            var settings = CustomerBehaviorSettingsManager.Settings;
            if (settings != null)
            {
                GUILayout.Label($"Settings Asset: {settings.name}");
                GUILayout.Space(5);
                
                // Shopping settings
                GUILayout.Label("Shopping Settings:", EditorStyles.boldLabel);
                var shopping = settings.shopping;
                GUILayout.Label($"  Buy Probability: {shopping.buyProbability:F2}");
                GUILayout.Label($"  Max Products: {shopping.maxProducts}");
                GUILayout.Label($"  Shopping Duration: {shopping.shoppingDuration}s");
                
                GUILayout.Space(5);
                
                // Checkout settings
                GUILayout.Label("Checkout Settings:", EditorStyles.boldLabel);
                var checkout = settings.checkout;
                GUILayout.Label($"  Queue Wait: {checkout.maxQueueWaitTime}s");
                GUILayout.Label($"  Scan Wait: {checkout.maxScanWaitTime}s");
                GUILayout.Label($"  Progress Check: {checkout.progressCheckInterval}s");
                
                GUILayout.Space(5);
                
                // Global settings
                GUILayout.Label("Global Settings:", EditorStyles.boldLabel);
                GUILayout.Label($"  Debug Logging: {settings.enableDebugLogging}");
                GUILayout.Label($"  Speed Multiplier: {settings.globalSpeedMultiplier:F1}");
            }
            else
            {
                GUILayout.Label("No settings available!", EditorStyles.boldLabel);
            }
            
            GUILayout.Space(10);
            
            // Runtime testing buttons
            if (GUILayout.Button("Log Current Settings"))
            {
                LogCurrentSettings();
            }
            
            if (testSettings != null && GUILayout.Button("Apply Test Settings"))
            {
                CustomerBehaviorSettingsManager.Instance.SetSettings(testSettings);
                Debug.Log($"Applied test settings: {testSettings.name}");
            }
            
            if (GUILayout.Button("Reset to Defaults"))
            {
                CustomerBehaviorSettingsManager.Instance.ResetToDefaults();
            }
            
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
        
        /// <summary>
        /// Log current settings to console for debugging
        /// </summary>
        [ContextMenu("Log Current Settings")]
        public void LogCurrentSettings()
        {
            var settings = CustomerBehaviorSettingsManager.Settings;
            if (settings != null)
            {
                Debug.Log("[CustomerBehaviorSettingsDebugger] " + settings.GetSettingsSummary());
                
                // Detailed breakdown
                Debug.Log($"[Shopping] Buy Probability: {settings.shopping.buyProbability:F3}, " +
                         $"Max Products: {settings.shopping.maxProducts}, " +
                         $"Duration: {settings.shopping.shoppingDuration}s");
                
                Debug.Log($"[Checkout] Queue Wait: {settings.checkout.maxQueueWaitTime}s, " +
                         $"Scan Wait: {settings.checkout.maxScanWaitTime}s, " +
                         $"Check Interval: {settings.checkout.progressCheckInterval}s");
            }
            else
            {
                Debug.LogError("[CustomerBehaviorSettingsDebugger] No settings available!");
            }
        }
        
        /// <summary>
        /// Test settings validation
        /// </summary>
        [ContextMenu("Test Settings Validation")]
        public void TestValidation()
        {
            CustomerBehaviorSettingsManager.Instance.ValidateSettings();
        }
        
        /// <summary>
        /// Simulate settings changes for testing
        /// </summary>
        [ContextMenu("Test Settings Modification")]
        public void TestSettingsModification()
        {
            var settings = CustomerBehaviorSettingsManager.Settings;
            if (settings != null)
            {
                Debug.Log("[Test] Before modification:");
                LogCurrentSettings();
                
                // Temporarily modify settings (this creates a runtime copy)
                var tempSettings = ScriptableObject.CreateInstance<CustomerBehaviorSettings>();
                tempSettings.shopping.buyProbability = 0.5f;
                tempSettings.checkout.maxQueueWaitTime = 30f;
                tempSettings.name = "Test Modified Settings";
                
                CustomerBehaviorSettingsManager.Instance.SetSettings(tempSettings);
                
                Debug.Log("[Test] After modification:");
                LogCurrentSettings();
            }
        }
    }
}