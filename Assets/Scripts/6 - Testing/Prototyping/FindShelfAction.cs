using UnityEngine;
using Opsive.BehaviorDesigner.Runtime.Tasks.Actions;
using Opsive.BehaviorDesigner.Runtime.Tasks;
using Opsive.GraphDesigner.Runtime.Variables;
using Opsive.BehaviorDesigner.Runtime;

namespace TabletopShop
{
    public class FindShelfAction : Action
    {
        [Tooltip("Store the found shelf in this shared variable")]
        public SharedVariable<GameObject> targetShelf;
        
        // Debug: Static variable to track the shelf
        public static GameObject LastFoundShelf;
        
        public override TaskStatus OnUpdate()
        {
            Debug.Log("[FindShelfAction] Starting shelf search...");
            
            SimpleTestCustomer customer = GetComponent<SimpleTestCustomer>();
            if (customer == null) 
            {
                Debug.LogError("[FindShelfAction] No SimpleTestCustomer component found!");
                return TaskStatus.Failure;
            }
            
            ShelfSlot shelf = customer.FindShelf();
            if (shelf != null)
            {
                // Store the found shelf in shared variable
                targetShelf.Value = shelf.gameObject;
                LastFoundShelf = shelf.gameObject; // Debug backup
                
                Debug.Log($"[FindShelfAction] ✅ Found and stored shelf: {shelf.name}");
                Debug.Log($"[FindShelfAction] Shared variable now contains: {targetShelf.Value?.name ?? "NULL"}");
                Debug.Log($"[FindShelfAction] Static backup contains: {LastFoundShelf?.name ?? "NULL"}");
                return TaskStatus.Success;
            }
            
            Debug.LogWarning("[FindShelfAction] ❌ No shelf found!");
            return TaskStatus.Failure;
        }
    }
}
