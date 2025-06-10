using UnityEngine;

namespace TabletopShop
{
    /// <summary>
    /// Interface for customer movement behavior
    /// Follows Interface Segregation Principle by exposing only movement-related methods
    /// </summary>
    public interface ICustomerMovement
    {
        // Properties
        bool IsMoving { get; }
        Vector3 CurrentDestination { get; }
        bool HasDestination { get; }
        
        // Movement methods
        bool SetDestination(Vector3 destination);
        bool SetRandomShelfDestination();
        bool MoveToShelfPosition(ShelfSlot shelf);
        bool MoveToCheckoutPoint();
        bool MoveToExitPoint();
        void StopMovement();
        bool HasReachedDestination();
    }
    
    /// <summary>
    /// Interface for customer behavior logic
    /// Handles state management and shopping behavior
    /// </summary>
    public interface ICustomerBehavior
    {
        // Properties
        float ShoppingTime { get; }
        ShelfSlot TargetShelf { get; }
        
        // Behavior methods
        void StartCustomerLifecycle(CustomerState initialState);
        void MigrateLegacyFields(float legacyShoppingTime, ShelfSlot legacyTargetShelf);
        
        // Events
        System.Action<CustomerState, CustomerState> OnStateChangeRequested { get; set; }
        System.Action<ShelfSlot> OnTargetShelfChanged { get; set; }
    }
    
    /// <summary>
    /// Interface for customer visual representation
    /// Handles visual feedback, colors, and debug information
    /// </summary>
    public interface ICustomerVisuals
    {
        // Visual methods
        void UpdateColorForState(CustomerState state);
        string GetDebugInfo();
        void MigrateLegacyFields(bool legacyShowDebugGizmos);
    }
    
    /// <summary>
    /// Improved Customer class using interfaces for better dependency inversion
    /// This demonstrates the next step in the refactoring process
    /// </summary>
    public interface ICustomer
    {
        // Core customer properties
        CustomerState CurrentState { get; }
        float ShoppingTime { get; }
        ShelfSlot TargetShelf { get; }
        bool IsMoving { get; }
        Vector3 CurrentDestination { get; }
        bool HasDestination { get; }
        
        // Component access
        ICustomerMovement Movement { get; }
        ICustomerBehavior Behavior { get; }
        ICustomerVisuals Visuals { get; }
        
        // High-level actions
        void StartShopping();
        void StartPurchasing();
        void StartLeaving();
        
        // State management
        void ChangeState(CustomerState newState);
        bool IsInState(CustomerState state);
        
        // Legacy support
        void SetTargetShelf(ShelfSlot shelf);
        void ClearTargetShelf();
        string GetDebugInfo();
    }
}
