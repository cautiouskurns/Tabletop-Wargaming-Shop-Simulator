using UnityEngine;
using System.Linq;

namespace TabletopShop
{
    /// <summary>
    /// Test script to verify that our ShelfVisual duplication fix is working correctly
    /// Attach to any GameObject in the scene to test the fix
    /// </summary>
    public class ShelfVisualDuplicationTest : MonoBehaviour
    {
        [Header("Test Configuration")]
        [SerializeField] private KeyCode testKey = KeyCode.T;
        [SerializeField] private Vector3 testShelfPosition = new Vector3(0, 0, 0);
        
        private GameObject testShelfObject;
        private Shelf testShelf;
        
        private void Update()
        {
            if (Input.GetKeyDown(testKey))
            {
                RunDuplicationTest();
            }
        }
        
        /// <summary>
        /// Test the ShelfVisual duplication fix
        /// </summary>
        private void RunDuplicationTest()
        {
            Debug.Log("=== RUNNING SHELFVISUAL DUPLICATION TEST ===");
            
            // Clean up any existing test shelf
            if (testShelfObject != null)
            {
                DestroyImmediate(testShelfObject);
            }
            
            // Create a new test shelf
            testShelfObject = new GameObject("TestShelf_DuplicationTest");
            testShelfObject.transform.position = testShelfPosition;
            testShelf = testShelfObject.AddComponent<Shelf>();
            
            Debug.Log("Created test shelf - waiting for Awake() to run...");
            
            // Force the shelf to initialize (this will call SetupShelfVisual in Awake)
            testShelf.SendMessage("Awake", SendMessageOptions.DontRequireReceiver);
            
            // Count ShelfVisual children after first creation
            int firstCount = CountShelfVisualChildren();
            Debug.Log($"After first SetupShelfVisual() call: {firstCount} ShelfVisual objects found");
            
            // Try to trigger SetupShelfVisual again (this would previously create duplicates)
            Debug.Log("Attempting to trigger SetupShelfVisual() again...");
            var setupMethod = typeof(Shelf).GetMethod("SetupShelfVisual", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            setupMethod?.Invoke(testShelf, null);
            
            // Count ShelfVisual children after second call
            int secondCount = CountShelfVisualChildren();
            Debug.Log($"After second SetupShelfVisual() call: {secondCount} ShelfVisual objects found");
            
            // Manually create a duplicate to test cleanup
            Debug.Log("Manually creating duplicate ShelfVisual to test cleanup...");
            GameObject duplicateVisual = GameObject.CreatePrimitive(PrimitiveType.Cube);
            duplicateVisual.name = "ShelfVisual";
            duplicateVisual.transform.SetParent(testShelf.transform, false);
            
            int beforeCleanupCount = CountShelfVisualChildren();
            Debug.Log($"Before cleanup: {beforeCleanupCount} ShelfVisual objects found");
            
            // Trigger SetupShelfVisual again to test cleanup
            setupMethod?.Invoke(testShelf, null);
            
            int afterCleanupCount = CountShelfVisualChildren();
            Debug.Log($"After cleanup: {afterCleanupCount} ShelfVisual objects found");
            
            // Verify results
            if (firstCount == 1 && secondCount == 1 && afterCleanupCount == 1)
            {
                Debug.Log("<color=green>✓ DUPLICATION FIX TEST PASSED!</color>");
                Debug.Log("- No duplicates created on repeated calls");
                Debug.Log("- Existing duplicates properly cleaned up");
                Debug.Log("- Only one ShelfVisual remains as expected");
            }
            else
            {
                Debug.LogError("<color=red>✗ DUPLICATION FIX TEST FAILED!</color>");
                Debug.LogError($"Expected 1 ShelfVisual after each operation, got: {firstCount}, {secondCount}, {afterCleanupCount}");
            }
            
            Debug.Log("=== DUPLICATION TEST COMPLETE ===");
        }
        
        /// <summary>
        /// Count the number of ShelfVisual children under the test shelf
        /// </summary>
        private int CountShelfVisualChildren()
        {
            if (testShelf == null) return 0;
            
            int count = 0;
            for (int i = 0; i < testShelf.transform.childCount; i++)
            {
                Transform child = testShelf.transform.GetChild(i);
                if (child.name == "ShelfVisual")
                {
                    count++;
                }
            }
            return count;
        }
        
        /// <summary>
        /// List all ShelfVisual children for debugging
        /// </summary>
        private void ListShelfVisualChildren()
        {
            if (testShelf == null) return;
            
            Debug.Log("ShelfVisual children:");
            for (int i = 0; i < testShelf.transform.childCount; i++)
            {
                Transform child = testShelf.transform.GetChild(i);
                if (child.name == "ShelfVisual")
                {
                    Debug.Log($"  - {child.name} at position {child.position}");
                }
            }
        }
        
        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10, 50, 400, 150));
            GUILayout.Label("ShelfVisual Duplication Test");
            GUILayout.Label($"Press '{testKey}' to run duplication test");
            
            if (testShelf != null)
            {
                int visualCount = CountShelfVisualChildren();
                GUILayout.Label($"Current ShelfVisual count: {visualCount}");
                
                if (visualCount > 1)
                {
                    GUILayout.Label("<color=red>⚠ Multiple ShelfVisuals detected!</color>");
                }
                else if (visualCount == 1)
                {
                    GUILayout.Label("<color=green>✓ Single ShelfVisual (correct)</color>");
                }
            }
            else
            {
                GUILayout.Label("No test shelf created yet");
            }
            
            GUILayout.EndArea();
        }
        
        private void OnDestroy()
        {
            if (testShelfObject != null)
            {
                DestroyImmediate(testShelfObject);
            }
        }
    }
}
