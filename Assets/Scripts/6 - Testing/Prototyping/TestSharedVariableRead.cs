using UnityEngine;
using Opsive.BehaviorDesigner.Runtime.Tasks.Actions;
using Opsive.BehaviorDesigner.Runtime.Tasks;
using Opsive.GraphDesigner.Runtime.Variables;
using Opsive.BehaviorDesigner.Runtime;

namespace TabletopShop
{
    public class TestSharedVariableRead : Action
    {
        [Tooltip("Test shared variable")]
        public SharedVariable<GameObject> testVariable;
        
        public override TaskStatus OnUpdate()
        {
            if (testVariable.Value != null)
            {
                Debug.Log($"[TEST READ] ✅ Shared variable contains: {testVariable.Value.name}");
                return TaskStatus.Success;
            }
            else
            {
                Debug.LogWarning("[TEST READ] ❌ Shared variable is NULL!");
                return TaskStatus.Failure;
            }
        }
    }
}
