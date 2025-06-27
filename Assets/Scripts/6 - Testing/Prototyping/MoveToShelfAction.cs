using UnityEngine;
using UnityEngine.AI;
using Opsive.BehaviorDesigner.Runtime.Tasks.Actions;
using Opsive.BehaviorDesigner.Runtime.Tasks;
using Opsive.GraphDesigner.Runtime.Variables;
using Opsive.BehaviorDesigner.Runtime;

namespace TabletopShop
{
    public class MoveToShelfAction : Action
    {
        [Tooltip("The shelf to move to (from shared variable)")]
        [SerializeField] SharedVariable<GameObject> targetShelf;
        
        public override TaskStatus OnUpdate()
        {
            Debug.Log($"[MoveToShelfAction] Starting movement check...");
            Debug.Log($"[MoveToShelfAction] Shared variable contains: {targetShelf.Value?.name ?? "NULL"}");
            Debug.Log($"[MoveToShelfAction] Static backup contains: {FindShelfAction.LastFoundShelf?.name ?? "NULL"}");
            
            SimpleTestCustomer customer = GetComponent<SimpleTestCustomer>();
            if (customer == null) 
            {
                Debug.LogError("[MoveToShelfAction] No SimpleTestCustomer component found!");
                return TaskStatus.Failure;
            }
            
            // Try shared variable first, then static backup
            GameObject shelfObject = targetShelf.Value ?? FindShelfAction.LastFoundShelf;
            
            if (shelfObject == null)
            {
                Debug.LogWarning("[MoveToShelfAction] ❌ No target shelf assigned!");
                Debug.LogWarning("[MoveToShelfAction] Both shared variable and static backup are NULL!");
                Debug.LogWarning("[MoveToShelfAction] Make sure FindShelfAction ran successfully first!");
                return TaskStatus.Failure;
            }
            
            Debug.Log($"[MoveToShelfAction] Using shelf: {shelfObject.name} (from {(targetShelf.Value != null ? "shared variable" : "static backup")})");
            
            ShelfSlot shelfSlot = shelfObject.GetComponent<ShelfSlot>();
            if (shelfSlot == null)
            {
                Debug.LogWarning($"[MoveToShelfAction] ❌ {shelfObject.name} doesn't have ShelfSlot component!");
                return TaskStatus.Failure;
            }
            
            // Start moving to shelf
            bool startedMove = customer.MoveToShelf(shelfSlot);
            if (!startedMove)
            {
                Debug.LogWarning("[MoveToShelfAction] ❌ Failed to start movement to shelf!");
                return TaskStatus.Failure;
            }
            
            // Check if reached destination
            if (customer.HasReachedDestination())
            {
                Debug.Log($"[MoveToShelfAction] ✅ Reached shelf: {shelfObject.name}");
                return TaskStatus.Success;
            }
            
            Debug.Log($"[MoveToShelfAction] 🏃 Still moving to {shelfObject.name}...");
            return TaskStatus.Running;
        }
    }
}
