using UnityEngine;
using Opsive.BehaviorDesigner.Runtime.Tasks.Actions;
using Opsive.BehaviorDesigner.Runtime.Tasks;
using Opsive.GraphDesigner.Runtime.Variables;
using Opsive.BehaviorDesigner.Runtime;

namespace TabletopShop
{
    public class DebugSharedVariable : Action
    {
        [Tooltip("The shared variable to debug")]
        [SerializeField] protected SharedVariable<GameObject> sharedGameObject;
        
        public override TaskStatus OnUpdate()
        {
            if (sharedGameObject.Value != null)
            {
                Debug.Log($"[DEBUG] Shared variable contains: {sharedGameObject.Value.name}");
            }
            else
            {
                Debug.Log("[DEBUG] Shared variable is NULL or empty");
            }
            
            return TaskStatus.Success;
        }
    }
}
