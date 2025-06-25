using UnityEngine;

namespace TabletopShop
{
    /// <summary>
    /// ENTERING STATE - Controls its own entry logic completely
    /// </summary>
    public class EnteringState : BaseCustomerState
    {
        private float entryStartTime;
        private bool hasFoundShelf = false;
        private bool isMovingToShelf = false;
        private const float MAX_ENTRY_TIME = 30f;
        
        public override void OnEnter(CustomerBehavior customer)
        {
            this.customer = customer;
            entryStartTime = Time.time;
            hasFoundShelf = false;
            isMovingToShelf = false;
            
            Debug.Log($"{customer.name} entering shop");
            
            // STATE DECIDES: Check if store is open
            if (!IsStoreOpen())
            {
                Debug.Log($"{customer.name}: Store closed, leaving immediately");
                RequestTransition(CustomerState.Leaving, "Store closed");
                return;
            }
            
            // STATE CONTROLS: Find and move to shelf
            FindAndMoveToShelf();
        }
        
        public override void OnUpdate(CustomerBehavior customer)
        {
            float entryTime = Time.time - entryStartTime;
            
            // STATE DECIDES: Timeout check
            if (entryTime > MAX_ENTRY_TIME)
            {
                Debug.LogWarning($"{customer.name}: Entry timeout");
                RequestTransition(CustomerState.Leaving, "Entry timeout");
                return;
            }
            
            // STATE DECIDES: Store closing check
            if (!IsStoreOpen() || ShouldLeaveStoreDueToHours())
            {
                RequestTransition(CustomerState.Leaving, "Store closed during entry");
                return;
            }
            
            // STATE DECIDES: Check if reached shelf
            if (isMovingToShelf && HasReachedDestination())
            {
                Debug.Log($"{customer.name}: Reached initial shelf");
                RequestTransition(CustomerState.Shopping, "Reached shelf");
            }
            else if (!isMovingToShelf && !hasFoundShelf)
            {
                // Retry finding shelf if we haven't started moving yet
                FindAndMoveToShelf();
            }
        }
        
        public override void OnExit(CustomerBehavior customer)
        {
            float totalEntryTime = Time.time - entryStartTime;
            Debug.Log($"{customer.name} finished entering after {totalEntryTime:F1}s");
        }
        
        /// <summary>
        /// STATE CONTROLS: Find and move to shelf logic
        /// </summary>
        private void FindAndMoveToShelf()
        {
            // Find available shelves
            ShelfSlot[] availableShelves = Object.FindObjectsByType<ShelfSlot>(FindObjectsSortMode.None);
            
            if (availableShelves.Length == 0)
            {
                Debug.LogWarning($"{customer.name} couldn't find any shelves");
                RequestTransition(CustomerState.Leaving, "No shelves available");
                return;
            }
            
            // Select random shelf
            ShelfSlot randomShelf = availableShelves[UnityEngine.Random.Range(0, availableShelves.Length)];
            customer.SetTargetShelf(randomShelf);
            
            // Move to shelf
            var movement = customer.GetMovement();
            if (movement != null && movement.MoveToShelfPosition(randomShelf))
            {
                hasFoundShelf = true;
                isMovingToShelf = true;
                Debug.Log($"{customer.name} found shelf and started moving to {randomShelf.name}");
            }
            else
            {
                Debug.LogError($"{customer.name} failed to move to shelf");
                RequestTransition(CustomerState.Leaving, "Movement failed");
            }
        }
        
        /// <summary>
        /// STATE CHECKS: Has reached destination
        /// </summary>
        private bool HasReachedDestination()
        {
            var movement = customer.GetMovement();
            return movement?.HasReachedDestination() ?? false;
        }
    }
    
}
