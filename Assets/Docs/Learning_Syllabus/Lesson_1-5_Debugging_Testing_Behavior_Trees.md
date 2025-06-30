# Lesson 1.5: Debugging and Testing Behavior Trees

## 📋 Lesson Overview
**Module**: Behavior Designer Integration  
**Difficulty**: 🟡  
**Duration**: 45 minutes  
**Prerequisites**: Lessons 1.1-1.4, Basic understanding of Unity debugging tools

### 🎯 Learning Objectives
By the end of this lesson, you will be able to:
- [ ] Use Behavior Designer's visual debugging tools effectively
- [ ] Implement comprehensive logging systems for AI behavior analysis
- [ ] Create test scenarios for complex behavior tree workflows
- [ ] Diagnose and fix common behavior tree issues using systematic approaches

### 🔗 Project Context
**Real-World Application**: Debugging customer AI issues and validating behavior tree performance  
**Files Referenced**: 
- `Assets/Scripts/6 - Testing/Prototyping/CustomerStateCondition.cs:37-74`
- `Assets/Scripts/3 - Systems/AI/Customer/Core/CustomerBehavior.cs:125-155`
- `Assets/Scripts/6 - Testing/Prototyping/MoveToShelfAction.cs:70-85`

---

## 📖 Core Concepts

### Concept 1: Multi-Level Debug Logging
Effective behavior tree debugging requires logging at multiple levels: individual task execution, overall behavior flow, and system integration points.

#### Why This Matters
In our project, customers have complex behaviors spanning multiple systems. When issues arise (customers getting stuck, not shopping, or failing to checkout), structured logging helps identify exactly where problems occur.

#### Implementation Details
- **Task-Level Logging**: Individual task entry, progress, and completion
- **Flow-Level Logging**: Behavior tree execution path and decision points
- **System-Level Logging**: Integration with game systems (NavMesh, checkout, etc.)

### Concept 2: Visual Debugging Integration
Behavior Designer provides real-time visual debugging, but maximizing its effectiveness requires understanding how to structure tasks and data for optimal visualization.

#### Why This Matters
Visual debugging shows the flow of execution, current task states, and variable values in real-time, making complex behavior issues much easier to diagnose than text logs alone.

#### Implementation Details
- **Task Status Visualization**: How TaskStatus values appear in the visual debugger
- **Variable Monitoring**: Exposing key state variables for real-time monitoring
- **Execution Flow Tracking**: Understanding visual cues for behavior tree navigation

---

## 💻 Code Examples

### Example 1: Comprehensive Task Logging System
**File**: `Assets/Scripts/6 - Testing/Prototyping/CustomerStateCondition.cs`  
**Lines**: 37-50

```csharp
public class CustomerStateCondition : Conditional
{
    public override TaskStatus OnUpdate()
    {
        Customer customer = GetComponent<Customer>();
        if (customer == null) return TaskStatus.Failure;
        
        TaskStatus result = TaskStatus.Failure;

        switch (conditionType)
        {
            case ConditionType.HasFinishedShopping:
                result = CheckShoppingTime(customer);
                // Detailed logging with context and timing information
                if (customer.showDebugLogs)
                    Debug.Log($"[CustomerStateCondition] HasFinishedShopping = {result} " +
                             $"(elapsed: {Time.time - customer.shoppingStartTime:F1}s / {customer.ShoppingTime}s)");
                return result;

            case ConditionType.HasMaxProducts:
                result = customer.selectedProducts.Count >= customer.MaxProducts ? TaskStatus.Success : TaskStatus.Failure;
                if (customer.showDebugLogs)
                    Debug.Log($"[CustomerStateCondition] HasMaxProducts = {result} " +
                             $"(products: {customer.selectedProducts.Count}/{customer.MaxProducts})");
                return result;

            case ConditionType.HasSelectedProducts:
                result = customer.selectedProducts.Count > 0 ? TaskStatus.Success : TaskStatus.Failure;
                if (customer.showDebugLogs)
                    Debug.Log($"[CustomerStateCondition] HasSelectedProducts = {result} " +
                             $"(count: {customer.selectedProducts.Count})");
                return result;

            case ConditionType.NeedsCheckout:
                result = CheckNeedsCheckout(customer);
                if (customer.showDebugLogs)
                    Debug.Log($"[CustomerStateCondition] NeedsCheckout = {result} " +
                             $"(has products: {customer.selectedProducts.Count > 0})");
                return result;
        }

        return TaskStatus.Failure;
    }
}
```

**Explanation**:
- **Structured Format**: `[TaskName] ConditionType = Result (context)`
- **Contextual Information**: Includes relevant state values that influenced the decision
- **Conditional Logging**: Respects `customer.showDebugLogs` to avoid log spam
- **Quantitative Data**: Provides actual numbers for analysis (elapsed time, product counts)

### Example 2: System-Level Status Monitoring
**File**: `Assets/Scripts/3 - Systems/AI/Customer/Core/CustomerBehavior.cs`  
**Lines**: 125-155

```csharp
private void Update()
{
    if (useBehaviorDesigner)
    {
        UpdateStateMachineVisualization();
        
        // Periodic comprehensive status logging
        if (Time.frameCount % 300 == 0) // Every 5 seconds at 60fps
        {
            Debug.Log($"🎯 BEHAVIOR DESIGNER MODE: {name} - Visual updates only, legacy AI disabled");
            LogDetailedStatus();
        }
        return;
    }
    
    // Legacy state machine update
    UpdateLegacyStateMachine();
}

private void LogDetailedStatus()
{
    if (!customer.showDebugLogs) return;
    
    // Comprehensive status including all relevant systems
    string statusReport = $"🎯 CUSTOMER STATUS REPORT: {name}\n" +
                         $"├─ Products: {customer.selectedProducts.Count}/{customer.MaxProducts}\n" +
                         $"├─ Money: ${customer.currentMoney:F0}\n" +
                         $"├─ Shopping Time: {(Time.time - customer.shoppingStartTime):F1}s / {customer.ShoppingTime:F1}s\n" +
                         $"├─ Current State: {customer.currentState}\n" +
                         $"├─ Target Shelf: {(customer.currentTargetShelf?.name ?? \"None\")}\n" +
                         $"├─ Movement Status: {GetMovementStatus()}\n" +
                         $"└─ Behavior Tree: {(useBehaviorDesigner ? \"Active\" : \"Disabled\")}";
    
    Debug.Log(statusReport);
}

private string GetMovementStatus()
{
    if (customer.Movement == null)
        return "No Movement Component";
    
    if (customer.Movement.HasReachedDestination())
        return "At Destination";
    
    if (customer.Movement.IsMovementFailed())
        return "Movement Failed";
    
    return "Moving";
}
```

**Explanation**:
- **Hierarchical Formatting**: Uses tree-style formatting for easy reading
- **Multi-System Overview**: Covers products, money, timing, state, movement in one report
- **Smart Frequency**: Updates every 5 seconds to avoid performance impact
- **Status Aggregation**: Combines information from multiple components

### Example 3: Task Execution Tracing
**File**: `Assets/Scripts/6 - Testing/Prototyping/MoveToShelfAction.cs`  
**Lines**: 70-85

```csharp
public class MoveToShelfAction : Action
{
    public override TaskStatus OnUpdate()
    {
        if (!isMoving)
        {
            if (customer.showDebugLogs)
                Debug.LogError($"[MoveToShelfAction] {customer.name}: Task running but movement not started!");
            return TaskStatus.Failure;
        }
        
        Customer customer = GetComponent<Customer>();
        if (customer == null)
        {
            Debug.LogError("[MoveToShelfAction] Customer component lost during execution!");
            return TaskStatus.Failure;
        }
        
        // Detailed movement progress tracking
        if (customer.Movement != null && customer.Movement.HasReachedDestination())
        {
            if (customer.showDebugLogs)
                Debug.Log($"[MoveToShelfAction] ✅ {customer.name}: Successfully reached shelf destination");
            return TaskStatus.Success;
        }
        
        // Check for movement failure with diagnostic information
        if (customer.Movement != null && customer.Movement.IsMovementFailed())
        {
            Vector3 currentPos = transform.position;
            Vector3 targetPos = customer.Movement.GetCurrentDestination();
            float distance = Vector3.Distance(currentPos, targetPos);
            
            Debug.LogWarning($"[MoveToShelfAction] {customer.name}: Movement failed!\n" +
                           $"├─ Current Position: {currentPos}\n" +
                           $"├─ Target Position: {targetPos}\n" +
                           $"├─ Distance: {distance:F2}m\n" +
                           $"└─ NavMesh Agent Status: {customer.Movement.GetAgentStatus()}");
            return TaskStatus.Failure;
        }
        
        // Periodic progress updates for long movements
        if (customer.showDebugLogs && Time.frameCount % 120 == 0) // Every 2 seconds
        {
            float progress = customer.Movement.GetMovementProgress();
            Debug.Log($"[MoveToShelfAction] {customer.name}: Moving to shelf... Progress: {progress:P0}");
        }
        
        return TaskStatus.Running;
    }
}
```

**Explanation**:
- **State Validation**: Checks for inconsistent internal state
- **Progress Tracking**: Provides periodic updates for long-running tasks
- **Failure Diagnostics**: Detailed information when movement fails
- **Performance Awareness**: Limits frequent logging to avoid performance impact

---

## 🔧 Hands-On Exercise

### Exercise: Create a Behavior Tree Debug Dashboard
**Objective**: Build a comprehensive debugging system that provides real-time behavior tree analysis

#### Instructions
1. Create a `BehaviorTreeDebugger` component that monitors multiple customers
2. Implement a visual dashboard showing task execution states
3. Add performance metrics (task duration, success rates)
4. Create debug commands for triggering specific scenarios

#### Starter Code
```csharp
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class BehaviorTreeDebugger : MonoBehaviour
{
    [Header("Monitoring Configuration")]
    public bool enableDebugger = true;
    public float updateInterval = 1f;
    public int maxLogEntries = 100;
    
    [Header("Display Options")]
    public bool showInInspector = true;
    public bool logToConsole = false;
    public bool trackPerformanceMetrics = true;
    
    [Header("Debug Information")]
    [SerializeField] private List<CustomerDebugInfo> customerInfos = new List<CustomerDebugInfo>();
    [SerializeField] private int totalCustomers = 0;
    [SerializeField] private int activeCustomers = 0;
    [SerializeField] private float averageTaskDuration = 0f;
    
    private float lastUpdateTime;
    private List<TaskExecutionLog> executionLogs = new List<TaskExecutionLog>();
    
    [System.Serializable]
    private class CustomerDebugInfo
    {
        public string customerName;
        public string currentTask;
        public string taskStatus;
        public float taskDuration;
        public Vector3 position;
        public string phase;
    }
    
    [System.Serializable]
    private class TaskExecutionLog
    {
        public float timestamp;
        public string customerName;
        public string taskName;
        public string result;
        public float duration;
    }
    
    private void Update()
    {
        if (!enableDebugger) return;
        
        if (Time.time - lastUpdateTime >= updateInterval)
        {
            UpdateDebugInformation();
            lastUpdateTime = Time.time;
        }
    }
    
    private void UpdateDebugInformation()
    {
        // TODO: Find all customers in scene
        // TODO: Collect behavior tree status from each
        // TODO: Update performance metrics
        // TODO: Log significant state changes
    }
    
    [ContextMenu("Generate Debug Report")]
    public void GenerateDebugReport()
    {
        // TODO: Create comprehensive behavior tree analysis report
    }
    
    [ContextMenu("Clear Debug Logs")]
    public void ClearDebugLogs()
    {
        // TODO: Clear accumulated debug information
    }
    
    public void LogTaskExecution(string customerName, string taskName, string result, float duration)
    {
        // TODO: Add task execution to log with timestamp
    }
    
    private void OnGUI()
    {
        if (!enableDebugger || !showInInspector) return;
        
        // TODO: Display debug information overlay
    }
}
```

#### Success Criteria
- [ ] Successfully monitors multiple customers simultaneously
- [ ] Provides clear visualization of behavior tree execution
- [ ] Tracks performance metrics accurately
- [ ] Offers useful debug commands and reports
- [ ] Minimal performance impact on gameplay

#### Common Issues & Solutions
- **Issue**: Debug system causes frame rate drops
  - **Solution**: Limit update frequency and avoid expensive operations in Update()
- **Issue**: Information overload in debug display
  - **Solution**: Implement filtering and prioritization for most relevant information

---

## 🧠 Knowledge Check

### Quick Questions
1. **Question**: Why is structured logging better than simple Debug.Log() calls?
   - **Answer**: Structured logging provides consistent format, searchable content, and contextual information that makes debugging much more efficient

2. **Question**: How often should behavior tree tasks log their status?
   - **Answer**: Critical events (start/end/failure) always; progress updates periodically; detailed diagnostics only when debug mode is enabled

3. **Question**: What information is most valuable when debugging behavior tree failures?
   - **Answer**: Task execution sequence, state variables at failure point, external system status, and timing information

### Code Review Challenge
**Given Code**: 
```csharp
public class BadLoggingTask : Action
{
    public override TaskStatus OnUpdate()
    {
        Debug.Log("Task running");
        
        if (SomeCondition())
        {
            Debug.Log("Success");
            return TaskStatus.Success;
        }
        
        return TaskStatus.Running;
    }
}
```

**Tasks**:
- **Issues**: No context, no timing, logs every frame, no identification, no conditional logging
- **Problems**: Log spam, no useful debugging information, performance impact
- **Improvements**: Add task name, customer context, conditional logging, meaningful messages
- **Best Practices**: Structured format, relevant context, performance consideration

---

## 🔍 Deep Dive: Advanced Topics

### Advanced Concept 1: Automated Testing Integration
Behavior trees can be tested systematically using Unity's Test Framework:

```csharp
[UnityTest]
public IEnumerator CustomerCompletesShoppingWorkflow()
{
    // Setup test customer and environment
    Customer testCustomer = CreateTestCustomer();
    yield return new WaitForSeconds(0.1f); // Allow initialization
    
    // Validate shopping phase
    Assert.IsTrue(testCustomer.currentState == CustomerState.Shopping);
    yield return new WaitForSeconds(testCustomer.ShoppingTime + 1f);
    
    // Validate checkout completion
    Assert.IsTrue(testCustomer.selectedProducts.Count > 0);
    Assert.IsTrue(testCustomer.currentState == CustomerState.Exiting);
}
```

### Performance Considerations
- **Log Volume**: Use conditional compilation for debug builds
- **Update Frequency**: Batch debug updates to reduce frame rate impact
- **Memory Management**: Limit log retention and clean up old entries

### Integration Patterns
- **Observer Pattern**: Debug systems observe behavior tree events
- **Command Pattern**: Debug commands for triggering specific scenarios
- **Strategy Pattern**: Different debugging strategies for different scenarios

---

## 🔗 Connections & Next Steps

### Related Lessons
- **Previous**: Lesson 1.4 - Complex Checkout Workflow - Debugging the workflows created
- **Next**: Module 2 lessons - Advanced patterns building on debugging skills
- **Related**: Lesson 3.4 - Testing Component Systems - Broader testing approaches

### Real Project Usage
**Current Implementation**: Comprehensive logging system across all behavior tree tasks  
**Evolution**: Started with basic Debug.Log, evolved to structured, contextual logging  
**Future Enhancements**: Could add automated testing, performance profiling, or remote debugging

### Extended Learning
- **Unity Documentation**: [Console Window and Logging](https://docs.unity3d.com/Manual/Console.html)
- **Testing**: [Unity Test Framework](https://docs.unity3d.com/Packages/com.unity.test-framework@1.1/manual/index.html)
- **Behavior Designer**: [Runtime Debugging](https://opsive.com/support/documentation/behavior-designer/runtime-debugging/)

---

## 📚 Resources & References

### Project Files
- CustomerStateCondition.cs - Structured conditional logging examples
- CustomerBehavior.cs - System-level status monitoring
- MoveToShelfAction.cs - Task execution tracing patterns

### External Resources
- [Unity Debugging Best Practices](https://docs.unity3d.com/Manual/BestPracticeUnderstandingPerformanceInUnity.html)
- [Behavior Designer Debugging](https://opsive.com/support/documentation/behavior-designer/behavior-tree-component/)
- [C# Logging Patterns](https://docs.microsoft.com/en-us/dotnet/core/extensions/logging)

---

## 📝 Instructor Notes

### Teaching Tips
- Use live debugging sessions to show real-time behavior tree execution
- Demonstrate the difference between good and bad logging practices
- Show how visual debugging complements text logging
- Emphasize the importance of performance-conscious debugging

### Time Management
- **Concept Introduction**: 15 minutes
- **Code Examples**: 20 minutes  
- **Hands-On Exercise**: 8 minutes
- **Q&A and Review**: 2 minutes

### Assessment Rubric
| Criteria | Excellent (4) | Proficient (3) | Developing (2) | Beginning (1) |
|----------|---------------|----------------|----------------|---------------|
| Logging Quality | Comprehensive, structured, contextual | Good logging practices | Basic logging implementation | Poor or missing logging |
| Debug Integration | Excellent use of visual and text debugging | Good debugging approach | Some debugging tools used | Little debugging consideration |
| Performance Awareness | Optimal performance with debug features | Good performance practices | Acceptable performance impact | Poor performance practices |

---

*Lesson Version: 1.0*  
*Last Updated: December 29, 2025*  
*Author: Claude with Project Context*