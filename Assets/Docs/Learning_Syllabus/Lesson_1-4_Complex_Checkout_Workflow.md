# Lesson 1.4: Complex Checkout Workflow Implementation

## 📋 Lesson Overview
**Module**: Behavior Designer Integration  
**Difficulty**: 🔴  
**Duration**: 90 minutes  
**Prerequisites**: Lessons 1.1-1.3, Understanding of Unity Coroutines, Component Architecture

### 🎯 Learning Objectives
By the end of this lesson, you will be able to:
- [ ] Design multi-phase behavior tree tasks that coordinate complex workflows
- [ ] Implement robust error handling and timeout management in AI systems
- [ ] Create behavior tree tasks that integrate with existing game systems
- [ ] Handle asynchronous operations and external dependencies in behavior trees

### 🔗 Project Context
**Real-World Application**: Complete customer checkout process from queue joining to payment completion  
**Files Referenced**: 
- `Assets/Scripts/6 - Testing/Prototyping/CompleteCheckoutTask.cs:54-150`
- `Assets/Scripts/6 - Testing/Prototyping/JoinQueueTask.cs:46-85`
- `Assets/Scripts/6 - Testing/Prototyping/PlaceProductsTask.cs:90-116`
- `Assets/Scripts/2 - Entities/Shop/Checkout/CheckoutCounter.cs:360-417`

---

## 📖 Core Concepts

### Concept 1: Multi-Phase Task Design
Complex real-world behaviors often require multiple distinct phases that must execute in sequence, with each phase having its own success/failure conditions and cleanup requirements.

#### Why This Matters
Our checkout workflow involves: navigation → queue joining → product placement → scanning wait → payment processing → departure. Each phase depends on the previous one and has different timing and error conditions.

#### Implementation Details
- **State Progression**: Tracking which phase of a complex task is currently executing
- **Phase Validation**: Ensuring prerequisites are met before advancing to next phase
- **Rollback Handling**: Cleaning up when phases fail partway through

### Concept 2: External System Integration
Behavior tree tasks often need to coordinate with systems outside the AI (checkout counters, UI elements, game managers) while maintaining reliable behavior tree semantics.

#### Why This Matters
Our checkout tasks must integrate with the CheckoutCounter system, queue management, and payment processing while providing proper TaskStatus feedback to the behavior tree.

#### Implementation Details
- **System Coupling**: Loose coupling between AI tasks and game systems
- **Event Coordination**: Handling events and callbacks from external systems
- **Timeout Management**: Preventing tasks from hanging when external systems fail

---

## 💻 Code Examples

### Example 1: Multi-Phase Task with State Management
**File**: `Assets/Scripts/6 - Testing/Prototyping/CompleteCheckoutTask.cs`  
**Lines**: 54-110

```csharp
public class CompleteCheckoutTask : Action
{
    [Tooltip("Maximum time to wait for payment processing")]
    public float maxPaymentWaitTime = 30f;
    
    [Tooltip("Time to wait before starting departure sequence")]
    public float departureDelay = 1f;
    
    private CheckoutCounter checkoutCounter = null;
    private float paymentStartTime = 0f;
    private bool hasRequestedPayment = false;
    private bool paymentCompleted = false;
    
    public override TaskStatus OnUpdate()
    {
        if (checkoutCounter == null)
            return TaskStatus.Failure;
            
        Customer customer = GetComponent<Customer>();
        if (customer == null)
            return TaskStatus.Failure;
        
        // Check timeout for entire checkout process
        float waitTime = Time.time - paymentStartTime;
        if (waitTime > maxPaymentWaitTime)
        {
            Debug.LogWarning($"[CompleteCheckoutTask] {customer.name}: Payment timeout after {waitTime:F1}s");
            return TaskStatus.Failure;
        }
        
        // Phase 1: Request payment processing
        if (!hasRequestedPayment)
        {
            RequestPaymentProcessing(customer);
            hasRequestedPayment = true;
            return TaskStatus.Running; // Continue to next phase
        }
        
        // Phase 2: Wait for payment completion
        if (!paymentCompleted)
        {
            if (CheckPaymentCompleted(customer))
            {
                paymentCompleted = true;
                if (customer.showDebugLogs)
                    Debug.Log($"[CompleteCheckoutTask] ✅ {customer.name}: Payment completed successfully");
                
                // Brief delay before departure (removed Invoke call for immediate progression)
                return TaskStatus.Running; // Continue to final phase
            }
            return TaskStatus.Running; // Still processing payment
        }
        
        // Phase 3: Complete departure
        if (IsDepartureComplete(customer))
        {
            if (customer.showDebugLogs)
                Debug.Log($"[CompleteCheckoutTask] ✅ {customer.name}: Checkout completed - ready to exit");
            return TaskStatus.Success;
        }
        
        return TaskStatus.Running; // Still completing departure
    }
    
    private void RequestPaymentProcessing(Customer customer)
    {
        if (customer.showDebugLogs)
            Debug.Log($"[CompleteCheckoutTask] {customer.name}: Requesting payment processing...");
        
        // Integrate with external checkout system
        checkoutCounter.ProcessPayment();
    }
    
    private bool CheckPaymentCompleted(Customer customer)
    {
        // Payment completion is detected by state changes in external system
        bool customerCleared = !checkoutCounter.HasCustomer || !checkoutCounter.CanCustomerPlaceItems(customer);
        bool productsCleared = !checkoutCounter.HasProducts;
        
        if (customerCleared || productsCleared)
        {
            if (customer.showDebugLogs)
                Debug.Log($"[CompleteCheckoutTask] {customer.name}: Payment completed - customer cleared: {customerCleared}, products cleared: {productsCleared}");
            return true;
        }
        
        return false;
    }
}
```

**Explanation**:
- **Phase State Variables**: `hasRequestedPayment` and `paymentCompleted` track progress
- **Sequential Execution**: Each phase must complete before the next begins
- **External Integration**: Calls `checkoutCounter.ProcessPayment()` and monitors results
- **Timeout Protection**: Prevents infinite waiting if external systems fail

### Example 2: Queue Management with Complex State
**File**: `Assets/Scripts/6 - Testing/Prototyping/JoinQueueTask.cs`  
**Lines**: 46-85

```csharp
public class JoinQueueTask : Action
{
    [Tooltip("Maximum time to wait in queue before giving up")]
    public float maxQueueWaitTime = 60f;
    
    private CheckoutCounter checkoutCounter = null;
    private float queueStartTime = 0f;
    private bool hasJoinedQueue = false;
    
    public override TaskStatus OnUpdate()
    {
        if (!hasJoinedQueue || checkoutCounter == null)
            return TaskStatus.Failure;
            
        Customer customer = GetComponent<Customer>();
        if (customer == null)
            return TaskStatus.Failure;
        
        // Timeout protection for queue waiting
        if (Time.time - queueStartTime > maxQueueWaitTime)
        {
            Debug.LogWarning($"[JoinQueueTask] {customer.name}: Queue wait timeout - giving up");
            return TaskStatus.Failure;
        }
        
        // Check if customer can proceed to checkout (is current customer)
        if (checkoutCounter.HasCustomer && IsCurrentCustomer(customer))
        {
            if (customer.showDebugLogs)
                Debug.Log($"[JoinQueueTask] ✅ {customer.name}: Ready for checkout - is current customer");
            return TaskStatus.Success;
        }
        
        // Provide periodic status updates while waiting
        if (customer.showDebugLogs && Time.frameCount % 180 == 0) // Every 3 seconds
        {
            int queuePosition = GetQueuePosition(customer);
            Debug.Log($"[JoinQueueTask] {customer.name}: Waiting in queue (position: {queuePosition})");
        }
        
        return TaskStatus.Running;
    }
    
    private bool IsCurrentCustomer(Customer customer)
    {
        // Use CheckoutCounter's permission system to validate current customer
        return checkoutCounter.CanCustomerPlaceItems(customer);
    }
    
    private int GetQueuePosition(Customer customer)
    {
        // Estimate queue position using available queue information
        return checkoutCounter.QueueLength;
    }
}
```

**Explanation**:
- **Queue Integration**: Uses CheckoutCounter's queue system with proper validation
- **Progress Monitoring**: Provides regular status updates without flooding logs
- **Permission-Based Logic**: Uses `CanCustomerPlaceItems()` to determine queue status
- **Graceful Timeout**: Fails task if waiting too long instead of hanging forever

### Example 3: Asynchronous Product Placement
**File**: `Assets/Scripts/6 - Testing/Prototyping/PlaceProductsTask.cs`  
**Lines**: 90-116

```csharp
public class PlaceProductsTask : Action
{
    [Tooltip("Time between placing each product")]
    public float placementInterval = 0.5f;
    
    private bool hasStartedPlacement = false;
    private int productsPlaced = 0;
    
    public override void OnStart()
    {
        Customer customer = GetComponent<Customer>();
        if (customer == null) return;
        
        // Validate permissions and preconditions
        CheckoutCounter checkoutCounter = FindNearestCheckoutCounter();
        if (!checkoutCounter.CanCustomerPlaceItems(customer))
        {
            Debug.LogError($"[PlaceProductsTask] {customer.name}: Not authorized to place items!");
            return;
        }
        
        // Start asynchronous placement process
        StartCoroutine(PlaceProductsSequentially(customer));
        hasStartedPlacement = true;
    }
    
    public override TaskStatus OnUpdate()
    {
        if (!hasStartedPlacement)
            return TaskStatus.Failure;
            
        Customer customer = GetComponent<Customer>();
        if (customer == null)
            return TaskStatus.Failure;
        
        // Check if all products have been placed
        if (productsPlaced >= customer.selectedProducts.Count)
        {
            if (customer.showDebugLogs)
                Debug.Log($"[PlaceProductsTask] ✅ {customer.name}: All {productsPlaced} products placed on counter");
            return TaskStatus.Success;
        }
        
        return TaskStatus.Running;
    }
    
    private IEnumerator PlaceProductsSequentially(Customer customer)
    {
        productsPlaced = 0;
        CheckoutCounter checkoutCounter = FindNearestCheckoutCounter();
        
        foreach (Product product in customer.selectedProducts)
        {
            if (product != null)
            {
                // Place product on checkout counter with external system integration
                checkoutCounter.PlaceProduct(product, customer);
                productsPlaced++;
                
                if (customer.showDebugLogs)
                    Debug.Log($"[PlaceProductsTask] {customer.name}: Placed product {productsPlaced}/{customer.selectedProducts.Count}: {product.ProductData?.ProductName ?? product.name}");
                
                // Wait between placements for realistic timing
                if (productsPlaced < customer.selectedProducts.Count)
                {
                    yield return new WaitForSeconds(placementInterval);
                }
            }
        }
        
        if (customer.showDebugLogs)
            Debug.Log($"[PlaceProductsTask] {customer.name}: Finished placing all products on counter");
    }
}
```

**Explanation**:
- **Coroutine Integration**: Uses coroutines for timed, sequential operations within behavior tree
- **Progress Tracking**: `productsPlaced` counter allows OnUpdate() to monitor coroutine progress
- **External System Calls**: Integrates with CheckoutCounter.PlaceProduct() method
- **Realistic Timing**: Adds delays between actions for believable behavior

---

## 🔧 Hands-On Exercise

### Exercise: Create a Multi-Stage Restaurant Order Task
**Objective**: Build a complex task that simulates a restaurant ordering process with multiple phases and error handling

#### Instructions
1. Create a `RestaurantOrderTask` with phases: Enter → Find Table → Order Food → Wait for Food → Eat → Pay → Leave
2. Implement timeout handling for each phase
3. Add fallback strategies (e.g., leave if no tables available)
4. Include realistic timing and external system integration

#### Starter Code
```csharp
using UnityEngine;
using Opsive.BehaviorDesigner.Runtime.Tasks.Actions;
using Opsive.BehaviorDesigner.Runtime.Tasks;
using System.Collections;

public class RestaurantOrderTask : Action
{
    [Header("Phase Timeouts")]
    public float tableSearchTimeout = 30f;
    public float orderTimeout = 120f;
    public float eatingTime = 60f;
    
    [Header("External Systems")]
    public RestaurantManager restaurantManager;
    
    private enum OrderPhase
    {
        FindingTable,
        Ordering,
        WaitingForFood,
        Eating,
        Paying,
        Leaving
    }
    
    private OrderPhase currentPhase = OrderPhase.FindingTable;
    private float phaseStartTime;
    private bool phaseInProgress = false;
    
    public override void OnStart()
    {
        if (restaurantManager == null)
        {
            restaurantManager = FindObjectOfType<RestaurantManager>();
        }
        
        if (restaurantManager == null)
        {
            Debug.LogError("[RestaurantOrderTask] No RestaurantManager found!");
            return;
        }
        
        currentPhase = OrderPhase.FindingTable;
        phaseStartTime = Time.time;
        phaseInProgress = true;
        
        // TODO: Start the first phase
    }
    
    public override TaskStatus OnUpdate()
    {
        if (!phaseInProgress)
            return TaskStatus.Failure;
        
        // TODO: Check timeout for current phase
        // TODO: Execute current phase logic
        // TODO: Advance to next phase when complete
        // TODO: Return appropriate TaskStatus
        
        return TaskStatus.Running;
    }
    
    private void AdvanceToNextPhase()
    {
        // TODO: Move to next phase and reset timing
    }
    
    private bool IsPhaseComplete()
    {
        // TODO: Check if current phase is complete
        return false;
    }
    
    private bool IsPhaseTimedOut()
    {
        // TODO: Check if current phase has exceeded timeout
        return false;
    }
}
```

#### Success Criteria
- [ ] Successfully handles all phases in sequence
- [ ] Implements proper timeout handling for each phase
- [ ] Provides meaningful status updates and error messages
- [ ] Integrates cleanly with external restaurant management system
- [ ] Handles edge cases (no tables, service delays, etc.)

#### Common Issues & Solutions
- **Issue**: Phases advance too quickly or get stuck
  - **Solution**: Ensure phase completion conditions are properly implemented and tested
- **Issue**: External system integration causes null reference exceptions
  - **Solution**: Add comprehensive null checking and fallback behavior

---

## 🧠 Knowledge Check

### Quick Questions
1. **Question**: Why use multiple boolean flags instead of a single enum for phase tracking?
   - **Answer**: Boolean flags allow for overlapping states and more flexible phase management, but enums provide clearer state representation

2. **Question**: How do you prevent behavior tree tasks from hanging when external systems fail?
   - **Answer**: Implement timeout mechanisms and validate external system responses before relying on them

3. **Question**: When should a complex task return TaskStatus.Failure vs TaskStatus.Success?
   - **Answer**: Failure when the overall goal cannot be achieved; Success when the complete workflow finishes successfully

### Code Review Challenge
**Given Code**: 
```csharp
public class BadCheckoutTask : Action
{
    public override TaskStatus OnUpdate()
    {
        CheckoutCounter counter = FindObjectOfType<CheckoutCounter>();
        counter.ProcessPayment();
        return TaskStatus.Success;
    }
}
```

**Tasks**:
- **Issues**: No error checking, calls ProcessPayment every frame, no state management, no timeout handling
- **Problems**: Performance impact, payment processed multiple times, potential crashes
- **Improvements**: Add state tracking, move ProcessPayment to OnStart, add validation
- **Best Practices**: Phase management, external system validation, proper TaskStatus usage

---

## 🔍 Deep Dive: Advanced Topics

### Advanced Concept 1: Task Orchestration Patterns
Complex workflows can use different coordination patterns:

```csharp
// Hierarchical: Parent task coordinates child tasks
public class CheckoutOrchestratorTask : Action
{
    private Queue<System.Type> taskQueue = new Queue<System.Type>();
    
    public override void OnStart()
    {
        taskQueue.Enqueue(typeof(MoveToCheckoutTask));
        taskQueue.Enqueue(typeof(JoinQueueTask));
        taskQueue.Enqueue(typeof(PlaceProductsTask));
        // etc.
    }
}

// Event-driven: Tasks communicate through events
public class EventDrivenCheckout : Action
{
    private void OnEnable()
    {
        CheckoutEvents.OnPaymentCompleted += HandlePaymentCompleted;
    }
}
```

### Performance Considerations
- **Coroutine Management**: Clean up coroutines in OnEnd() to prevent memory leaks
- **External System Polling**: Limit frequency of external system checks
- **State Validation**: Cache validation results when possible

### Integration Patterns
- **Facade Pattern**: Simplify complex external system interactions
- **Observer Pattern**: React to external system events
- **Command Pattern**: Encapsulate complex operations for retry/undo

---

## 🔗 Connections & Next Steps

### Related Lessons
- **Previous**: Lesson 1.3 - NavMesh Integration - Movement components used in checkout workflow
- **Next**: Lesson 1.5 - Debugging and Testing - How to troubleshoot complex workflows
- **Related**: Lesson 2.3 - Event-Driven Architecture - Advanced task coordination patterns

### Real Project Usage
**Current Implementation**: 5-task checkout workflow handling complete customer purchase process  
**Evolution**: Started with simple payment, evolved to handle queue management, product placement, scanning  
**Future Enhancements**: Could add customer preferences, loyalty programs, or dynamic pricing

### Extended Learning
- **Unity Documentation**: [Coroutines and Yield Instructions](https://docs.unity3d.com/Manual/Coroutines.html)
- **Design Patterns**: [State Machine Patterns](https://gameprogrammingpatterns.com/state.html)
- **Game AI**: [Behavior Tree Best Practices](https://www.gamedeveloper.com/programming/behavior-trees-for-ai-how-they-work)

---

## 📚 Resources & References

### Project Files
- CompleteCheckoutTask.cs - Multi-phase task with external system integration
- JoinQueueTask.cs - Queue management with timeout and status monitoring
- PlaceProductsTask.cs - Asynchronous operation coordination within behavior trees

### External Resources
- [Behavior Designer Complex Tasks](https://opsive.com/support/documentation/behavior-designer/custom-tasks/)
- [Unity Coroutine Best Practices](https://docs.unity3d.com/Manual/BestPracticeUnderstandingPerformanceInUnity.html)
- [State Management Patterns](https://refactoring.guru/design-patterns/state)

---

## 📝 Instructor Notes

### Teaching Tips
- Use the behavior tree debugger to show task progression in real-time
- Demonstrate what happens when external systems are disconnected
- Show how timeout values affect gameplay feel and reliability
- Emphasize the importance of graceful degradation

### Time Management
- **Concept Introduction**: 30 minutes
- **Code Examples**: 35 minutes  
- **Hands-On Exercise**: 20 minutes
- **Q&A and Review**: 5 minutes

### Assessment Rubric
| Criteria | Excellent (4) | Proficient (3) | Developing (2) | Beginning (1) |
|----------|---------------|----------------|----------------|---------------|
| Workflow Design | Perfect phase management and coordination | Good workflow structure, minor issues | Basic workflow functionality | Poor or broken workflow |
| Error Handling | Comprehensive timeout and failure handling | Good error management | Some error handling | Little or no error handling |
| System Integration | Excellent external system coordination | Good integration practices | Basic integration | Poor or no integration |
| Code Organization | Clean, maintainable, well-documented | Good organization and structure | Acceptable organization | Poor organization |

---

*Lesson Version: 1.0*  
*Last Updated: December 29, 2025*  
*Author: Claude with Project Context*