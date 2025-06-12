using UnityEngine;
using TabletopShop;

namespace TabletopShop
{
    /// <summary>
    /// Simple script to run the InventoryUI refactoring test in Unity
    /// </summary>
    public class RunInventoryUITest : MonoBehaviour
    {
        [ContextMenu("Run Inventory UI Refactoring Test")]
        public void RunTest()
        {
            // Find or create the test script
            InventoryUIRefactoringTest testScript = FindAnyObjectByType<InventoryUIRefactoringTest>();
            if (testScript == null)
            {
                GameObject testObject = new GameObject("InventoryUIRefactoringTest");
                testScript = testObject.AddComponent<InventoryUIRefactoringTest>();
            }
            
            // Run the test
            testScript.RunAllTests();
        }
    }
}