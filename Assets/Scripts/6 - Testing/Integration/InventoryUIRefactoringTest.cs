using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace TabletopShop
{
    /// <summary>
    /// Test script to validate InventoryUI refactoring maintains all functionality.
    /// This script verifies that the composition pattern works correctly and that
    /// all external systems continue to work unchanged.
    /// </summary>
    public class InventoryUIRefactoringTest : MonoBehaviour
    {
        [Header("Test Configuration")]
        [SerializeField] private bool runTestsOnStart = true;
        [SerializeField] private bool verboseLogging = true;
        
        private InventoryUI inventoryUI;
        private InventoryManager inventoryManager;
        private bool testsPassed = true;
        
        private void Start()
        {
            if (runTestsOnStart)
            {
                StartCoroutine(RunAllTests());
            }
        }
        
        /// <summary>
        /// Run all validation tests
        /// </summary>
        public IEnumerator RunAllTests()
        {
            Log("=== Starting InventoryUI Refactoring Validation Tests ===");
            
            // Initialize test environment
            yield return StartCoroutine(InitializeTestEnvironment());
            
            if (inventoryUI == null || inventoryManager == null)
            {
                LogError("Failed to initialize test environment!");
                yield break;
            }
            
            // Test component initialization
            yield return StartCoroutine(TestComponentInitialization());
            
            // Test public API preservation
            yield return StartCoroutine(TestPublicAPIPreservation());
            
            // Test panel management
            yield return StartCoroutine(TestPanelManagement());
            
            // Test display management
            yield return StartCoroutine(TestDisplayManagement());
            
            // Test button interactions
            yield return StartCoroutine(TestButtonInteractions());
            
            // Test event handling
            yield return StartCoroutine(TestEventHandling());
            
            // Test legacy compatibility
            yield return StartCoroutine(TestLegacyCompatibility());
            
            // Test component coordination
            yield return StartCoroutine(TestComponentCoordination());
            
            // Report results
            ReportTestResults();
            
            Log("=== InventoryUI Refactoring Validation Tests Complete ===");
        }
        
        /// <summary>
        /// Initialize test environment
        /// </summary>
        private IEnumerator InitializeTestEnvironment()
        {
            Log("Initializing test environment...");
            
            // Find InventoryUI in scene
            inventoryUI = FindFirstObjectByType<InventoryUI>();
            if (inventoryUI == null)
            {
                LogError("No InventoryUI found in scene!");
                testsPassed = false;
                yield break;
            }
            
            // Wait for InventoryManager to be available
            int attempts = 0;
            while (InventoryManager.Instance == null && attempts < 10)
            {
                yield return new WaitForSeconds(0.1f);
                attempts++;
            }
            
            inventoryManager = InventoryManager.Instance;
            if (inventoryManager == null)
            {
                LogError("InventoryManager instance not available after waiting!");
                testsPassed = false;
                yield break;
            }
            
            // Wait an additional frame for full initialization
            yield return null;
            
            Log("Test environment initialized successfully");
        }
        
        /// <summary>
        /// Test that components are properly initialized
        /// </summary>
        private IEnumerator TestComponentInitialization()
        {
            Log("Testing component initialization...");
            
            // Check for InventoryUIVisuals component
            var visualsComponent = inventoryUI.GetComponent<InventoryUIVisuals>();
            if (visualsComponent == null)
            {
                LogError("InventoryUIVisuals component not found!");
                testsPassed = false;
            }
            else
            {
                Log("✓ InventoryUIVisuals component found");
            }
            
            // Check for InventoryUIInteraction component
            var interactionComponent = inventoryUI.GetComponent<InventoryUIInteraction>();
            if (interactionComponent == null)
            {
                LogError("InventoryUIInteraction component not found!");
                testsPassed = false;
            }
            else
            {
                Log("✓ InventoryUIInteraction component found");
            }
            
            yield return null;
        }
        
        /// <summary>
        /// Test that public API methods are preserved
        /// </summary>
        private IEnumerator TestPublicAPIPreservation()
        {
            Log("Testing public API preservation...");
            
            // Test TogglePanel method exists and is callable
            try
            {
                inventoryUI.TogglePanel();
                Log("✓ TogglePanel() method accessible");
            }
            catch (System.Exception e)
            {
                LogError($"TogglePanel() failed: {e.Message}");
                testsPassed = false;
            }
            
            // Test UpdateDisplay method exists and is callable
            try
            {
                inventoryUI.UpdateDisplay();
                Log("✓ UpdateDisplay() method accessible");
            }
            catch (System.Exception e)
            {
                LogError($"UpdateDisplay() failed: {e.Message}");
                testsPassed = false;
            }
            
            // Test OnProductButtonClick method exists and is callable
            try
            {
                inventoryUI.OnProductButtonClick(0);
                Log("✓ OnProductButtonClick() method accessible");
            }
            catch (System.Exception e)
            {
                LogError($"OnProductButtonClick() failed: {e.Message}");
                testsPassed = false;
            }
            
            yield return null;
        }
        
        /// <summary>
        /// Test panel management functionality
        /// </summary>
        private IEnumerator TestPanelManagement()
        {
            Log("Testing panel management...");
            
            // Test panel toggle
            var visualsComponent = inventoryUI.GetComponent<InventoryUIVisuals>();
            if (visualsComponent != null)
            {
                bool initialState = visualsComponent.IsPanelVisible;
                
                inventoryUI.TogglePanel();
                yield return new WaitForSeconds(0.1f);
                
                bool newState = visualsComponent.IsPanelVisible;
                if (newState != initialState)
                {
                    Log("✓ Panel toggle functionality working");
                }
                else
                {
                    LogError("Panel toggle did not change state");
                    testsPassed = false;
                }
                
                // Reset to initial state
                inventoryUI.TogglePanel();
                yield return new WaitForSeconds(0.1f);
            }
            else
            {
                LogError("Cannot test panel management - InventoryUIVisuals component missing");
                testsPassed = false;
            }
        }
        
        /// <summary>
        /// Test display management functionality
        /// </summary>
        private IEnumerator TestDisplayManagement()
        {
            Log("Testing display management...");
            
            try
            {
                inventoryUI.UpdateDisplay();
                Log("✓ Display update completed without errors");
            }
            catch (System.Exception e)
            {
                LogError($"Display update failed: {e.Message}");
                testsPassed = false;
            }
            
            yield return null;
        }
        
        /// <summary>
        /// Test button interaction functionality
        /// </summary>
        private IEnumerator TestButtonInteractions()
        {
            Log("Testing button interactions...");
            
            // Test button clicks for each product type
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    inventoryUI.OnProductButtonClick(i);
                    Log($"✓ Button {i} click handled successfully");
                }
                catch (System.Exception e)
                {
                    LogError($"Button {i} click failed: {e.Message}");
                    testsPassed = false;
                }
                
                yield return new WaitForSeconds(0.1f);
            }
        }
        
        /// <summary>
        /// Test event handling functionality
        /// </summary>
        private IEnumerator TestEventHandling()
        {
            Log("Testing event handling...");
            
            var interactionComponent = inventoryUI.GetComponent<InventoryUIInteraction>();
            if (interactionComponent != null)
            {
                if (interactionComponent.IsManagerAvailable)
                {
                    Log("✓ InventoryManager connection established");
                }
                else
                {
                    LogError("InventoryManager connection not available");
                    testsPassed = false;
                }
            }
            else
            {
                LogError("Cannot test event handling - InventoryUIInteraction component missing");
                testsPassed = false;
            }
            
            yield return null;
        }
        
        /// <summary>
        /// Test legacy compatibility
        /// </summary>
        private IEnumerator TestLegacyCompatibility()
        {
            Log("Testing legacy compatibility...");
            
#if UNITY_EDITOR
            // Test that serialized fields are still accessible (for migration)
            var serializedObject = new UnityEditor.SerializedObject(inventoryUI);
            
            var fadeInProperty = serializedObject.FindProperty("fadeInDuration");
            var fadeOutProperty = serializedObject.FindProperty("fadeOutDuration");
            var selectedColorProperty = serializedObject.FindProperty("selectedButtonColor");
            var defaultColorProperty = serializedObject.FindProperty("defaultButtonColor");
            
            if (fadeInProperty != null && fadeOutProperty != null && 
                selectedColorProperty != null && defaultColorProperty != null)
            {
                Log("✓ Legacy serialized fields preserved for migration");
            }
            else
            {
                LogError("Some legacy serialized fields missing");
                testsPassed = false;
            }
#else
            Log("Legacy compatibility test skipped - requires Unity Editor");
#endif
            
            yield return null;
        }
        
        /// <summary>
        /// Test component coordination
        /// </summary>
        private IEnumerator TestComponentCoordination()
        {
            Log("Testing component coordination...");
            
            var visualsComponent = inventoryUI.GetComponent<InventoryUIVisuals>();
            var interactionComponent = inventoryUI.GetComponent<InventoryUIInteraction>();
            
            if (visualsComponent != null && interactionComponent != null)
            {
                // Test that components can coordinate through the main class
                yield return new WaitForSeconds(0.1f);
                try
                {
                    inventoryUI.UpdateDisplay();
                    inventoryUI.TogglePanel();
                    inventoryUI.TogglePanel();
                    
                    Log("✓ Component coordination working correctly");
                }
                catch (System.Exception e)
                {
                    LogError($"Component coordination failed: {e.Message}");
                    testsPassed = false;
                }
            }
            else
            {
                LogError("Cannot test component coordination - components missing");
                testsPassed = false;
            }
            
            yield return null;
        }
        
        /// <summary>
        /// Report test results
        /// </summary>
        private void ReportTestResults()
        {
            if (testsPassed)
            {
                Log("🎉 ALL TESTS PASSED! InventoryUI refactoring successful.");
                Log("✓ Composition pattern implemented correctly");
                Log("✓ Public API preserved");
                Log("✓ Components properly initialized");
                Log("✓ Backward compatibility maintained");
            }
            else
            {
                LogError("❌ SOME TESTS FAILED! Review the errors above.");
            }
        }
        
        /// <summary>
        /// Manual test trigger for inspector
        /// </summary>
        [ContextMenu("Run Tests")]
        public void RunTestsManual()
        {
            StartCoroutine(RunAllTests());
        }
        
        /// <summary>
        /// Log message with optional verbose mode
        /// </summary>
        private void Log(string message)
        {
            if (verboseLogging)
            {
                Debug.Log($"[InventoryUITest] {message}");
            }
        }
        
        /// <summary>
        /// Log error message
        /// </summary>
        private void LogError(string message)
        {
            Debug.LogError($"[InventoryUITest] {message}");
        }
    }
}
