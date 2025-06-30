# Lesson 1.1: Understanding Action vs Conditional Tasks

## 📋 Lesson Overview
**Module**: Behavior Designer Integration  
**Difficulty**: 🟢  
**Duration**: 45 minutes  
**Prerequisites**: Basic Unity knowledge, understanding of GameObjects and Components

### 🎯 Learning Objectives
By the end of this lesson, you will be able to:
- [ ] Differentiate between Action and Conditional tasks in Behavior Designer
- [ ] Understand when to use TaskStatus.Success, TaskStatus.Failure, and TaskStatus.Running
- [ ] Implement basic Action tasks that interact with Unity components
- [ ] Create Conditional tasks that evaluate game state

### 🔗 Project Context
**Real-World Application**: Customer AI system for autonomous shopping behavior  
**Files Referenced**: 
- `Assets/Scripts/6 - Testing/Prototyping/MoveToShelfAction.cs:1-120`
- `Assets/Scripts/6 - Testing/Prototyping/CustomerStateCondition.cs:25-85`
- `Assets/Scripts/6 - Testing/Prototyping/CompleteCheckoutTask.cs:54-80`

---

## 📖 Core Concepts

### Concept 1: Action Tasks - Doing Things
Action tasks represent **behaviors** - things your AI actually does. They perform operations, move objects, change states, or interact with the game world.

#### Why This Matters
In our customer AI, actions represent concrete behaviors like "move to shelf", "pick up product", or "join checkout queue". These are the verbs of your AI system.

#### Implementation Details
- **Inherit from**: `Opsive.BehaviorDesigner.Runtime.Tasks.Actions.Action`
- **Override Methods**: `OnStart()`, `OnUpdate()`, `OnEnd()`
- **Return Types**: TaskStatus enum values indicating task progress

### Concept 2: Conditional Tasks - Making Decisions
Conditional tasks **evaluate conditions** - they check game state and return true/false to guide behavior tree decisions.

#### Why This Matters
Conditionals act as the brain's decision-making layer. They determine if a customer has finished shopping, has enough money, or has reached their destination.

#### Implementation Details
- **Inherit from**: `Opsive.BehaviorDesigner.Runtime.Tasks.Conditionals.Conditional`
- **Override Method**: `OnUpdate()` only
- **Return Types**: TaskStatus.Success (true) or TaskStatus.Failure (false)

---

## 💻 Code Examples

### Example 1: Action Task - Movement Behavior
**File**: `Assets/Scripts/6 - Testing/Prototyping/MoveToShelfAction.cs`  
**Lines**: 54-85

```csharp
public class MoveToShelfAction : Action
{
    private bool isMoving = false;
    
    public override void OnStart()
    {
        Customer customer = GetComponent<Customer>();
        if (customer == null)
        {
            Debug.LogError("[MoveToShelfAction] Customer component not found!");
            return;
        }
        
        // Start the movement action
        bool moveStarted = customer.Movement.MoveToShelfPosition(customer.currentTargetShelf);
        isMoving = moveStarted;
        
        if (customer.showDebugLogs)
            Debug.Log($"[MoveToShelfAction] ✅ {customer.name}: Started moving to shelf");
    }
    
    public override TaskStatus OnUpdate()
    {
        if (!isMoving)
            return TaskStatus.Failure;
        
        Customer customer = GetComponent<Customer>();
        if (customer == null)
            return TaskStatus.Failure;
        
        // Check if movement completed
        if (customer.Movement != null && customer.Movement.HasReachedDestination())
        {
            return TaskStatus.Success;  // Movement complete!
        }
        
        return TaskStatus.Running;  // Still moving
    }
}
```

**Explanation**:
- **OnStart()**: Initiates the movement behavior, sets up state
- **OnUpdate()**: Continuously checks if movement is complete
- **TaskStatus.Running**: Indicates ongoing work - behavior tree will keep calling OnUpdate()
- **TaskStatus.Success**: Movement completed successfully
- **TaskStatus.Failure**: Something went wrong, behavior tree can try alternatives

### Example 2: Conditional Task - State Evaluation
**File**: `Assets/Scripts/6 - Testing/Prototyping/CustomerStateCondition.cs`  
**Lines**: 47-51

```csharp
public class CustomerStateCondition : Conditional
{
    public enum ConditionType
    {
        HasSelectedProducts,
        HasFinishedShopping,
        CanAffordProducts,
        NeedsCheckout
    }
    
    [Tooltip("The condition to check")]
    public ConditionType conditionType;
    
    public override TaskStatus OnUpdate()
    {
        Customer customer = GetComponent<Customer>();
        if (customer == null) return TaskStatus.Failure;
        
        switch (conditionType)
        {
            case ConditionType.HasSelectedProducts:
                bool hasProducts = customer.selectedProducts.Count > 0;
                if (customer.showDebugLogs)
                    Debug.Log($"[CustomerStateCondition] HasSelectedProducts = {hasProducts} (count: {customer.selectedProducts.Count})");
                return hasProducts ? TaskStatus.Success : TaskStatus.Failure;
                
            case ConditionType.NeedsCheckout:
                bool needsCheckout = customer.selectedProducts.Count > 0;
                return needsCheckout ? TaskStatus.Success : TaskStatus.Failure;
        }
        
        return TaskStatus.Failure;
    }
}
```

**Explanation**:
- **No OnStart() or OnEnd()**: Conditionals only evaluate, they don't maintain state
- **Immediate Return**: Always returns Success or Failure, never Running
- **Enum-Based Design**: One conditional class handles multiple related checks
- **Debug Logging**: Shows decision-making process for debugging

---

## 🔧 Hands-On Exercise

### Exercise: Create a Simple Action and Conditional
**Objective**: Build a "WaitTask" action and "HasWaitedLongEnough" conditional

#### Instructions
1. Create a new Action task called `WaitTask` that waits for a specified duration
2. Create a conditional that checks if the wait time has elapsed
3. Test both in a simple behavior tree

#### Starter Code
```csharp
// WaitTask.cs
using UnityEngine;
using Opsive.BehaviorDesigner.Runtime.Tasks.Actions;
using Opsive.BehaviorDesigner.Runtime.Tasks;

public class WaitTask : Action
{
    [Tooltip("Time to wait in seconds")]
    public float waitTime = 2f;
    
    private float startTime;
    
    public override void OnStart()
    {
        // TODO: Record the start time
    }
    
    public override TaskStatus OnUpdate()
    {
        // TODO: Check if enough time has passed
        // Return TaskStatus.Running while waiting
        // Return TaskStatus.Success when complete
        return TaskStatus.Running;
    }
}
```

#### Success Criteria
- [ ] WaitTask properly tracks time and completes after specified duration
- [ ] Conditional correctly evaluates time-based state
- [ ] Both tasks work together in a behavior tree
- [ ] Debug logs show clear task progression

#### Common Issues & Solutions
- **Issue**: Task never completes (always returns Running)
  - **Solution**: Ensure you're comparing elapsed time correctly: `(Time.time - startTime) >= waitTime`
- **Issue**: Task completes immediately
  - **Solution**: Check that startTime is being set in OnStart(), not OnUpdate()

---

## 🧠 Knowledge Check

### Quick Questions
1. **Question**: When should an Action task return TaskStatus.Running?
   - **Answer**: When the action is still in progress and needs more time to complete (like movement or waiting)

2. **Question**: Why don't Conditional tasks use OnStart() or OnEnd()?
   - **Answer**: Conditionals are stateless evaluations - they check current game state instantly without maintaining their own state

3. **Question**: What happens if an Action task returns TaskStatus.Failure?
   - **Answer**: The behavior tree considers that branch failed and may try alternative branches or propagate the failure up the tree

### Code Review Challenge
**Given Code**: 
```csharp
public class BadMoveTask : Action
{
    public override TaskStatus OnUpdate()
    {
        transform.position = Vector3.zero;
        return TaskStatus.Success;
    }
}
```

**Tasks**:
- **Issue**: No OnStart() method, immediate teleportation, no error checking
- **Problems**: Not gradual movement, no validation, not using Unity's movement systems
- **Improved Version**: Use gradual movement with Vector3.MoveTowards or NavMesh
- **Justification**: Smooth movement feels natural, error checking prevents crashes

---

## 🔍 Deep Dive: Advanced Topics

### Advanced Concept 1: Complex Task State Management
Our CompleteCheckoutTask shows sophisticated state management:

```csharp
public override TaskStatus OnUpdate()
{
    // Multi-phase action with different states
    if (!hasRequestedPayment)
    {
        RequestPaymentProcessing(customer);
        hasRequestedPayment = true;
        return TaskStatus.Running;  // Continue to next phase
    }
    
    if (!paymentCompleted)
    {
        if (CheckPaymentCompleted(customer))
        {
            paymentCompleted = true;
            return TaskStatus.Running;  // Continue to final phase
        }
        return TaskStatus.Running;  // Still processing
    }
    
    return TaskStatus.Success;  // All phases complete
}
```

### Performance Considerations
- **Conditional Frequency**: Conditionals run every frame they're evaluated - keep them lightweight
- **Action Cleanup**: Always clean up resources in OnEnd() - it's called even if task is interrupted
- **Memory Allocation**: Avoid creating new objects in OnUpdate() - cache references in OnStart()

---

## 🔗 Connections & Next Steps

### Related Lessons
- **Next**: Lesson 1.2 - Customer AI State Management - How these tasks coordinate to create intelligent behavior
- **Related**: Lesson 1.5 - Debugging and Testing - How to troubleshoot task issues

### Real Project Usage
**Current Implementation**: 9 custom Action tasks and 1 sophisticated Conditional task power our customer AI  
**Evolution**: Started with simple movement, evolved to complex multi-phase checkout workflow  
**Future Enhancements**: Could add customer personality traits through conditional variations

### Extended Learning
- **Unity Documentation**: [Behavior Trees and FSMs](https://docs.unity3d.com/Manual/StateMachineIntro.html)
- **Behavior Designer Docs**: [Creating Custom Tasks](https://opsive.com/support/documentation/behavior-designer/custom-tasks/)
- **Design Patterns**: Command Pattern (actions as objects), State Pattern (conditionals as state queries)

---

## 📚 Resources & References

### Project Files
- MoveToShelfAction.cs - Demonstrates gradual movement with NavMesh integration
- CustomerStateCondition.cs - Shows enum-based conditional design pattern
- CompleteCheckoutTask.cs - Example of complex multi-phase action task

### External Resources
- [Behavior Designer Task Reference](https://opsive.com/support/documentation/behavior-designer/tasks/)
- [Unity NavMesh Tutorial](https://docs.unity3d.com/Manual/nav-BuildingNavMesh.html)
- [C# Properties and Methods](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/properties)

---

## 📝 Instructor Notes

### Teaching Tips
- Start with the movement example - it's visual and intuitive
- Emphasize that TaskStatus.Running is key to smooth behaviors
- Use the debugger to show OnUpdate() being called repeatedly
- Demonstrate what happens when tasks return different statuses

### Time Management
- **Concept Introduction**: 15 minutes
- **Code Examples**: 15 minutes  
- **Hands-On Exercise**: 10 minutes
- **Q&A and Review**: 5 minutes

### Assessment Rubric
| Criteria | Excellent (4) | Proficient (3) | Developing (2) | Beginning (1) |
|----------|---------------|----------------|----------------|---------------|
| Task Implementation | Perfect task structure, proper status returns | Good implementation, minor issues | Basic functionality, some errors | Incomplete or non-functional |
| Unity Integration | Excellent use of components and Unity patterns | Good Unity integration | Some Unity concepts used | Poor or no Unity integration |

---

*Lesson Version: 1.0*  
*Last Updated: December 29, 2025*  
*Author: Claude with Project Context*