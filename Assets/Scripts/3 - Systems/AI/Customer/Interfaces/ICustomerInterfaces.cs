using UnityEngine;
using UnityEngine.AI;
using System;
using System.Collections.Generic;

namespace TabletopShop
{
    // ========================================================================
    // ENHANCED INTERFACE SEGREGATION PATTERN
    // ========================================================================
    // 
    // This file implements comprehensive interface segregation for the Customer AI system.
    // Each interface focuses on a single responsibility, following SOLID principles:
    //
    // 1. ICustomerMovement - Movement, navigation, pathfinding
    // 2. ICustomerStateMachine - State management and transitions
    // 3. ICustomerShoppingBehavior - Shopping logic and product interaction
    // 4. ICustomerLifecycle - Customer lifecycle management
    // 5. ICustomerCheckoutBehavior - Queue and checkout processing
    // 6. ICustomerVisuals - Visual feedback and debugging
    // 7. ICustomerBehaviorConfiguration - Initialization and setup
    //
    // Benefits:
    // - Better testability (mock individual interfaces)
    // - Clearer dependencies (inject only what's needed)
    // - Easier maintenance (changes isolated to specific concerns)
    // - Improved extensibility (implement new behaviors without changing existing interfaces)
    // ========================================================================
    /// <summary>
    /// Configuration data for customer personality and behavior
    /// </summary>
    [System.Serializable]
    public class CustomerPersonalityConfig
    {
        public string personalityType = "Standard";
        public float shoppingDurationMultiplier = 1.0f;
        public float movementSpeedMultiplier = 1.0f;
        public float purchaseProbability = 0.8f;
        public float patienceLevel = 1.0f;
        public bool prefersFastCheckout = false;
    }
    /// <summary>
    /// Interface for customer movement and navigation behavior
    /// Handles all movement, pathfinding, and navigation concerns
    /// </summary>
    public interface ICustomerMovement
    {
        // Movement state properties
        bool IsMoving { get; }
        Vector3 CurrentDestination { get; }
        bool HasDestination { get; }
        NavMeshAgent NavMeshAgent { get; }
        
        // Basic movement methods
        bool SetDestination(Vector3 destination);
        void StopMovement();
        bool HasReachedDestination();
        bool MoveToPosition(Vector3 position);
        
        // Specific location movement
        bool SetRandomShelfDestination();
        bool MoveToShelfPosition(ShelfSlot shelf);
        bool MoveToCheckoutPoint();
        bool MoveToExitPoint();
        
        // Pathfinding management
        void UpdatePathfindingState();
        void UpdateMovementState();
        
        // Configuration
        void Initialize();
        void MigrateLegacyFields(float movementSpeed, float stoppingDistance, 
                                float stuckDetectionTime, float stuckDistanceThreshold,
                                float destinationReachedDistance, int maxPathfindingRetries,
                                float pathfindingRetryDelay);
    }
    
    /// <summary>
    /// Interface for customer state management
    /// Handles all customer state transitions and state-related logic
    /// </summary>
    public interface ICustomerStateMachine
    {
        // State properties
        CustomerState CurrentState { get; }
        
        // State management methods
        void ChangeState(CustomerState newState);
        CustomerState GetCurrentState();
        bool IsInState(CustomerState state);
        
        // State events
        event Action<CustomerState, CustomerState> OnStateChangeRequested;
    }
    
    /// <summary>
    /// Interface for customer shopping behavior and product interaction
    /// Handles product selection, shopping logic, and purchase decisions
    /// </summary>
    public interface ICustomerShoppingBehavior
    {
        // Shopping properties
        float ShoppingTime { get; }
        ShelfSlot TargetShelf { get; }
        List<Product> SelectedProducts { get; }
        float TotalPurchaseAmount { get; }
        float BaseSpendingPower { get; }
        
        // Shopping behavior methods
        void SetTargetShelf(ShelfSlot shelf);
        void ClearTargetShelf();
        bool SetRandomShelfDestination();
        void PerformShoppingInteraction();
        bool IsSatisfiedWithShopping();
        float GetPreferredShoppingDuration();
        
        // Product interaction
        void TrySelectProductsAtShelf(ShelfSlot shelf);
        void ResetShoppingState();
        
        // Shopping events
        event Action<ShelfSlot> OnTargetShelfChanged;
    }
    
    /// <summary>
    /// Interface for customer lifecycle management
    /// Handles the complete customer experience from entry to exit
    /// </summary>
    public interface ICustomerLifecycle
    {
        // Lifecycle control
        void StartCustomerLifecycle(CustomerState initialState);
        void StopCustomerLifecycle();
        
        // Lifecycle state handlers
        System.Collections.IEnumerator HandleEnteringState();
        System.Collections.IEnumerator HandleShoppingState();
        System.Collections.IEnumerator HandlePurchasingState();
        System.Collections.IEnumerator HandleLeavingState();
        
        // Store integration
        bool IsStoreOpen();
        bool ShouldLeaveStoreDueToHours();
        bool ShouldHurryUpShopping();
    }
    
    /// <summary>
    /// Interface for customer queue and checkout behavior
    /// Handles checkout queue management and purchase processing
    /// </summary>
    public interface ICustomerCheckoutBehavior
    {
        // Queue state properties
        bool IsInQueue { get; }
        int QueuePosition { get; }
        CheckoutCounter QueuedCheckout { get; }
        bool WaitingForCheckoutTurn { get; }
        bool IsWaitingForCheckout { get; }
        
        // Queue management
        void OnJoinedQueue(CheckoutCounter checkoutCounter, int position);
        void OnQueuePositionChanged(int newPosition);
        void OnCheckoutReady(CheckoutCounter checkoutCounter);
        void OnCheckoutCompleted();
        
        // Checkout process
        System.Collections.IEnumerator PlaceItemsOnCounter(CheckoutCounter checkoutCounter);
        System.Collections.IEnumerator WaitForCheckoutCompletion(CheckoutCounter checkoutCounter);
        System.Collections.IEnumerator CollectItemsAndLeave(CheckoutCounter checkoutCounter);
        
        // Debug methods
        void CheckQueueStatus();
        void DebugCustomerQueueState();
        void ForceLeaveQueue();
    }
    
    /// <summary>
    /// Interface for customer behavior initialization and configuration
    /// Handles component setup and legacy migration
    /// </summary>
    public interface ICustomerBehaviorConfiguration
    {
        // Initialization
        void Initialize(CustomerMovement movement, Customer customer);
        System.Collections.IEnumerator DelayedInitialization();
        
        // Legacy support
        void MigrateLegacyFields(float legacyShoppingTime, ShelfSlot legacyTargetShelf);
        
        // Debug support
        string GetDebugInfo();
    }
    
    /// <summary>
    /// Interface for customer visual representation and feedback
    /// Handles visual effects, colors, debug visualization, and UI feedback
    /// </summary>
    public interface ICustomerVisuals
    {
        // Visual state properties
        bool ShowDebugGizmos { get; set; }
        bool EnableColorSystem { get; set; }
        
        // Color system methods
        void UpdateColorForState(CustomerState state);
        void SetColorImmediate(CustomerState state);
        void ResetToDefaultColor();
        
        // Visual feedback methods
        void UpdateVisualFeedback();
        void ShowDestinationReachedEffect();
        void ShowStateChangeEffect(CustomerState oldState, CustomerState newState);
        
        // Debug and information methods
        string GetDebugInfo();
        string GetStatusString();
        void LogDebugInfo();
        void PrintDebugInfo();
        void ToggleDebugGizmos();
        
        // Initialization and cleanup
        void Initialize(CustomerMovement movement, Customer customer);
        void MigrateLegacyFields(bool legacyShowDebugGizmos);
    }
    
    /// <summary>
    /// Comprehensive interface for complete customer behavior
    /// Combines all customer behavior interfaces for full functionality
    /// </summary>
    public interface ICustomerBehavior : ICustomerStateMachine, 
                                         ICustomerShoppingBehavior, 
                                         ICustomerLifecycle, 
                                         ICustomerCheckoutBehavior, 
                                         ICustomerBehaviorConfiguration
    {
        // This interface combines all behavior aspects
        // No additional methods needed - all functionality comes from composed interfaces
    }
    
    /// <summary>
    /// Enhanced Customer interface using comprehensive interface segregation
    /// Provides access to all customer functionality through focused interfaces
    /// </summary>
    public interface ICustomer
    {
        // Core customer identity
        string name { get; }
        UnityEngine.Transform transform { get; }
        
        // Direct component access (current pattern)
        ICustomerMovement Movement { get; }
        ICustomerBehavior Behavior { get; }
        ICustomerVisuals Visuals { get; }
        
        // Convenience properties (delegated from components)
        CustomerState CurrentState { get; }
        float ShoppingTime { get; }
        ShelfSlot TargetShelf { get; }
        bool IsMoving { get; }
        Vector3 CurrentDestination { get; }
        bool HasDestination { get; }
        
        // High-level customer actions (coordination methods)
        void StartShopping();
        void StartPurchasing();
        void StartLeaving();
        
        // Legacy compatibility methods
        void ChangeState(CustomerState newState);
        bool IsInState(CustomerState state);
        void SetTargetShelf(ShelfSlot shelf);
        void ClearTargetShelf();
        string GetDebugInfo();
    }
    
    /// <summary>
    /// Factory interface for creating customer instances
    /// Supports dependency injection and configuration
    /// </summary>
    public interface ICustomerFactory
    {
        ICustomer CreateCustomer(Vector3 spawnPosition);
        ICustomer CreateCustomer(Vector3 spawnPosition, CustomerPersonalityConfig personality);
        void ConfigureCustomerDependencies(ICustomer customer);
    }
    
    /// <summary>
    /// Service interface for customer analytics and monitoring
    /// Observes customer behavior for metrics and optimization
    /// </summary>
    public interface ICustomerAnalytics
    {
        // Customer tracking
        void OnCustomerEntered(ICustomer customer);
        void OnCustomerStateChanged(ICustomer customer, CustomerState oldState, CustomerState newState);
        void OnCustomerPurchase(ICustomer customer, float amount);
        void OnCustomerLeft(ICustomer customer);
        
        // Analytics queries
        int ActiveCustomerCount { get; }
        float AverageShoppingTime { get; }
        float TotalRevenueToday { get; }
        CustomerState GetMostCommonState();
    }
    
    /// <summary>
    /// Configuration interface for customer personality and behavior parameters
    /// Allows for different customer types with varying behaviors
    /// </summary>
    public interface ICustomerPersonality
    {
        // Personality traits
        string PersonalityType { get; }
        float ShoppingDuration { get; }
        float MovementSpeed { get; }
        float PurchaseProbability { get; }
        float PatienceLevel { get; }
        bool PrefersFastCheckout { get; }
        
        // Behavior modifiers
        float GetShoppingTimeMultiplier();
        float GetMovementSpeedMultiplier();
        bool ShouldHurryWhenStoreClosing();
        int GetMaxProductsToSelect();
    }
}
