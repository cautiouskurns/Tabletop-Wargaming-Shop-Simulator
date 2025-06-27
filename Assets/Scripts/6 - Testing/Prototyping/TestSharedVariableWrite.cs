using UnityEngine;
using Opsive.BehaviorDesigner.Runtime.Tasks.Actions;
using Opsive.BehaviorDesigner.Runtime.Tasks;
using Opsive.GraphDesigner.Runtime.Variables;
using Opsive.BehaviorDesigner.Runtime;

namespace TabletopShop
{
    public class TestSharedVariableWrite : Action
    {
        [Tooltip("Test shared variable")]
        public SharedVariable<GameObject> testVariable;
        
        public override TaskStatus OnUpdate()
        {
            // Find any GameObject in scene to test with
            GameObject testObject = GameObject.FindFirstObjectByType<ShelfSlot>()?.gameObject;
            
            if (testObject != null)
            {
                testVariable.Value = testObject;
                Debug.Log($"[TEST WRITE] Set shared variable to: {testObject.name}");
                return TaskStatus.Success;
            }
            
            Debug.LogWarning("[TEST WRITE] No test object found!");
            return TaskStatus.Failure;
        }
    }
}
