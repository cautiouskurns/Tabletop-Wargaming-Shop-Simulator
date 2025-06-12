using UnityEngine;

namespace TabletopShop
{
    /// <summary>
    /// Test script to verify that slot indicator scales persist correctly
    /// Attach to any GameObject in the scene to test the scale persistence
    /// </summary>
    public class SlotIndicatorScaleTest : MonoBehaviour
    {
        [Header("Test Configuration")]
        [SerializeField] private KeyCode testKey = KeyCode.S;
        [SerializeField] private Vector3 testSlotPosition = new Vector3(2, 0, 0);
        
        private GameObject testSlotObject;
        private ShelfSlot testSlot;
        
        private void Update()
        {
            if (Input.GetKeyDown(testKey))
            {
                RunScaleTest();
            }
        }
        
        /// <summary>
        /// Test the slot indicator scale persistence
        /// </summary>
        private void RunScaleTest()
        {
            Debug.Log("=== RUNNING SLOT INDICATOR SCALE TEST ===");
            
            // Clean up any existing test slot
            if (testSlotObject != null)
            {
                DestroyImmediate(testSlotObject);
            }
            
            // Create a new test slot
            testSlotObject = new GameObject("TestSlot_ScaleTest");
            testSlotObject.transform.position = testSlotPosition;
            testSlot = testSlotObject.AddComponent<ShelfSlot>();
            
            Debug.Log("Created test slot - waiting for components to initialize...");
            
            // Wait a frame for components to initialize
            StartCoroutine(CheckScaleAfterInitialization());
        }
        
        private System.Collections.IEnumerator CheckScaleAfterInitialization()
        {
            yield return null; // Wait one frame
            
            // Get the ShelfSlotVisuals component
            ShelfSlotVisuals visuals = testSlot.GetComponent<ShelfSlotVisuals>();
            if (visuals != null && visuals.SlotIndicator != null)
            {
                Vector3 currentScale = visuals.SlotIndicator.transform.localScale;
                Debug.Log($"Slot indicator scale after initialization: {currentScale}");
                
                if (currentScale.y <= 0.02f) // Allow some tolerance
                {
                    Debug.Log("<color=green>✓ THIN SCALE PRESERVED!</color>");
                    Debug.Log($"Y-axis scale is {currentScale.y}, which is thin as expected");
                }
                else
                {
                    Debug.LogWarning("<color=orange>⚠ Scale might not be as thin as expected</color>");
                    Debug.LogWarning($"Y-axis scale is {currentScale.y}, you might want it thinner");
                }
                
                // Test scale persistence after position update
                yield return new WaitForSeconds(1f);
                
                Debug.Log("Testing scale persistence after transform updates...");
                
                // Trigger a visual state update which calls UpdateIndicatorTransform
                visuals.UpdateVisualState();
                
                Vector3 scaleAfterUpdate = visuals.SlotIndicator.transform.localScale;
                Debug.Log($"Scale after visual state update: {scaleAfterUpdate}");
                
                if (Vector3.Distance(currentScale, scaleAfterUpdate) < 0.001f)
                {
                    Debug.Log("<color=green>✓ SCALE PERSISTENCE TEST PASSED!</color>");
                    Debug.Log("Scale remained consistent after visual updates");
                }
                else
                {
                    Debug.LogError("<color=red>✗ SCALE PERSISTENCE TEST FAILED!</color>");
                    Debug.LogError($"Scale changed from {currentScale} to {scaleAfterUpdate}");
                }
            }
            else
            {
                Debug.LogError("Could not find ShelfSlotVisuals component or SlotIndicator!");
            }
            
            Debug.Log("=== SCALE TEST COMPLETE ===");
        }
        
        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10, 350, 400, 150));
            GUILayout.Label("Slot Indicator Scale Test");
            GUILayout.Label($"Press '{testKey}' to run scale persistence test");
            
            if (testSlot != null)
            {
                var visuals = testSlot.GetComponent<ShelfSlotVisuals>();
                if (visuals != null && visuals.SlotIndicator != null)
                {
                    Vector3 currentScale = visuals.SlotIndicator.transform.localScale;
                    GUILayout.Label($"Current indicator scale: {currentScale}");
                    
                    if (currentScale.y <= 0.02f)
                    {
                        GUILayout.Label("<color=green>✓ Thin scale (good!)</color>");
                    }
                    else
                    {
                        GUILayout.Label("<color=orange>⚠ Thick scale</color>");
                    }
                }
            }
            else
            {
                GUILayout.Label("No test slot created yet");
            }
            
            GUILayout.EndArea();
        }
        
        private void OnDestroy()
        {
            if (testSlotObject != null)
            {
                DestroyImmediate(testSlotObject);
            }
        }
    }
}
