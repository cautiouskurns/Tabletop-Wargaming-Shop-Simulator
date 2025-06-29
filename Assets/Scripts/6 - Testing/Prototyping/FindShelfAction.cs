using UnityEngine;
using Opsive.BehaviorDesigner.Runtime.Tasks.Actions;
using Opsive.BehaviorDesigner.Runtime.Tasks;
using System.Collections.Generic;
using Opsive.GraphDesigner.Runtime.Variables;
using Opsive.BehaviorDesigner.Runtime;

namespace TabletopShop
{
    public class FindShelfTask : Action
    {
        [Tooltip("Prefer shelves with products over empty ones")]
        public bool preferProductShelves = true;
        
        public override TaskStatus OnUpdate()
        {
            Customer customer = GetComponent<Customer>();
            if (customer == null) 
            {
                Debug.LogError("[FindShelfTask] No CustomerData component found!");
                return TaskStatus.Failure;
            }
            
            // All shelf-finding logic lives here
            ShelfSlot foundShelf = FindBestShelf();
            
            if (foundShelf != null)
            {
                customer.currentTargetShelf = foundShelf;
                
                if (customer.showDebugLogs)
                    Debug.Log($"[FindShelfTask] ✅ Found shelf: {foundShelf.name}");
                return TaskStatus.Success;
            }
            
            if (customer.showDebugLogs)
                Debug.LogWarning("[FindShelfTask] ❌ No suitable shelf found!");
            return TaskStatus.Failure;
        }
        
        private ShelfSlot FindBestShelf()
        {
            ShelfSlot[] allShelves = Object.FindObjectsOfType<ShelfSlot>();
            
            if (allShelves.Length == 0)
                return null;
            
            // Logic for finding the best shelf
            if (preferProductShelves)
            {
                // First try to find shelves with products
                var shelvesWithProducts = new List<ShelfSlot>();
                foreach (var shelf in allShelves)
                {
                    if (!shelf.IsEmpty && shelf.CurrentProduct != null)
                        shelvesWithProducts.Add(shelf);
                }
                
                if (shelvesWithProducts.Count > 0)
                    return shelvesWithProducts[Random.Range(0, shelvesWithProducts.Count)];
            }
            
            // Fallback to any shelf
            return allShelves[Random.Range(0, allShelves.Length)];
        }
    }
}
