# Lesson 1.3: NavMesh Integration with Behavior Trees

## 📋 Lesson Overview
**Module**: Behavior Designer Integration  
**Difficulty**: 🟡  
**Duration**: 75 minutes  
**Prerequisites**: Lesson 1.1 (Action vs Conditional Tasks), Basic NavMesh understanding

### 🎯 Learning Objectives
By the end of this lesson, you will be able to:
- [ ] Integrate Unity's NavMesh system with Behavior Designer tasks
- [ ] Handle NavMesh pathfinding failures gracefully in AI behavior
- [ ] Debug movement issues using Unity's navigation tools
- [ ] Implement reliable position validation for AI navigation

### 🔗 Project Context
**Real-World Application**: Customer movement system that handles complex navigation scenarios  
**Files Referenced**: 
- `Assets/Scripts/6 - Testing/Prototyping/MoveToShelfAction.cs:45-85`
- `Assets/Scripts/6 - Testing/Prototyping/MoveToExitTask.cs:40-70`
- `Assets/Scripts/3 - Systems/AI/Customer/Movement/CustomerMovement.cs:120-180`

---

## 📖 Core Concepts

### Concept 1: NavMesh Pathfinding in Behavior Trees
Unity's NavMesh system provides intelligent pathfinding, but behavior tree tasks must handle pathfinding failures, dynamic obstacles, and edge cases.

#### Why This Matters
In our project, customers need to navigate to shelves, checkout counters, and exits. NavMesh provides the core navigation, but our behavior tree tasks must handle cases where pathfinding fails or destinations are unreachable.

#### Implementation Details
- **Position Validation**: Check if target positions are on the NavMesh before starting movement
- **Pathfinding Status**: Monitor NavMeshAgent status during movement
- **Fallback Strategies**: Handle cases where direct pathfinding fails

### Concept 2: Movement State Integration
Behavior tree tasks need to coordinate with Unity's NavMeshAgent component and provide meaningful status updates to the behavior tree system.

#### Why This Matters
Movement is often the longest-running task in behavior trees. Proper integration ensures smooth AI behavior and prevents stuck agents.

#### Implementation Details
- **Agent Status Monitoring**: Check pathPending, hasPath, and pathStatus
- **Destination Validation**: Ensure targets are reachable before starting movement
- **Completion Detection**: Reliable methods for detecting arrival at destination

---

## 💻 Code Examples

### Example 1: Robust Movement Task with NavMesh Integration
**File**: `Assets/Scripts/6 - Testing/Prototyping/MoveToShelfAction.cs`  
**Lines**: 45-85

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
        
        if (customer.Movement == null)
        {
            Debug.LogError("[MoveToShelfAction] Movement component not available!");
            return;
        }
        
        if (customer.currentTargetShelf == null)
        {
            Debug.LogError($"[MoveToShelfAction] {customer.name}: No target shelf assigned!");
            return;
        }
        
        // Use the proven CustomerMovement method that handles NavMesh properly
        bool moveStarted = customer.Movement.MoveToShelfPosition(customer.currentTargetShelf);
        isMoving = moveStarted;
        
        if (moveStarted)
        {
            if (customer.showDebugLogs)
                Debug.Log($"[MoveToShelfAction] ✅ {customer.name}: Started moving to shelf {customer.currentTargetShelf.name}");
        }
        else
        {
            Debug.LogError($"[MoveToShelfAction] {customer.name}: Failed to start movement to shelf!");
        }
    }
    
    public override TaskStatus OnUpdate()
    {
        if (!isMoving)
            return TaskStatus.Failure;
        
        Customer customer = GetComponent<Customer>();
        if (customer == null)
            return TaskStatus.Failure;
        
        // Check if reached destination
        if (customer.Movement != null && customer.Movement.HasReachedDestination())
        {
            if (customer.showDebugLogs)
                Debug.Log($"[MoveToShelfAction] ✅ {customer.name}: Reached shelf destination");
            return TaskStatus.Success;
        }
        
        // Check for movement failure (stuck, no path, etc.)
        if (customer.Movement != null && customer.Movement.IsMovementFailed())
        {
            Debug.LogWarning($"[MoveToShelfAction] {customer.name}: Movement failed - NavMesh issue detected");
            return TaskStatus.Failure;
        }
        
        return TaskStatus.Running;
    }
    
    public override void OnEnd()
    {
        isMoving = false;
    }
}
```

**Explanation**:
- **Error Prevention**: Validates all required components before attempting movement
- **Delegation**: Uses CustomerMovement.MoveToShelfPosition() which handles NavMesh complexity
- **Status Monitoring**: Checks both success (HasReachedDestination) and failure (IsMovementFailed) conditions
- **Clean Termination**: OnEnd() ensures clean state regardless of how task terminates

### Example 2: NavMesh Position Validation
**File**: `Assets/Scripts/3 - Systems/AI/Customer/Movement/CustomerMovement.cs`  
**Lines**: 120-160

```csharp
public class CustomerMovement : MonoBehaviour
{
    [Header("NavMesh Configuration")]
    [SerializeField] private float navMeshSampleDistance = 2f;
    [SerializeField] private float stoppingDistance = 0.5f;
    
    private NavMeshAgent navAgent;
    
    public bool MoveToShelfPosition(ShelfSlot targetShelf)
    {
        if (targetShelf == null)
        {
            Debug.LogError($"[CustomerMovement] {name}: Cannot move to null shelf!");
            return false;
        }
        
        // Calculate customer position relative to shelf
        Vector3 targetPosition = CalculateCustomerPosition(targetShelf);
        
        // Validate position is on NavMesh before starting movement
        if (!IsPositionOnNavMesh(targetPosition))
        {
            Debug.LogWarning($"[CustomerMovement] {name}: Target position {targetPosition} is not on NavMesh!");
            
            // Try to find nearby valid position
            Vector3 adjustedPosition;
            if (FindNearestNavMeshPosition(targetPosition, out adjustedPosition))
            {
                targetPosition = adjustedPosition;
                Debug.Log($"[CustomerMovement] {name}: Using adjusted position {adjustedPosition}");
            }
            else
            {
                Debug.LogError($"[CustomerMovement] {name}: Could not find valid NavMesh position near {targetPosition}");
                return false;
            }
        }
        
        // Start movement with validated position
        return StartMovement(targetPosition);
    }
    
    private bool IsPositionOnNavMesh(Vector3 position)
    {
        NavMeshHit hit;
        bool onNavMesh = NavMesh.SamplePosition(position, out hit, navMeshSampleDistance, NavMesh.AllAreas);
        
        if (showDebugLogs)
            Debug.Log($"[CustomerMovement] Position {position} on NavMesh: {onNavMesh}");
        
        return onNavMesh;
    }
    
    private bool FindNearestNavMeshPosition(Vector3 originalPosition, out Vector3 validPosition)
    {
        NavMeshHit hit;
        bool found = NavMesh.SamplePosition(originalPosition, out hit, navMeshSampleDistance * 2f, NavMesh.AllAreas);
        validPosition = found ? hit.position : originalPosition;
        return found;
    }
    
    private bool StartMovement(Vector3 targetPosition)
    {
        if (navAgent == null)
        {
            Debug.LogError($"[CustomerMovement] {name}: NavMeshAgent not found!");
            return false;
        }
        
        navAgent.stoppingDistance = stoppingDistance;
        navAgent.SetDestination(targetPosition);
        
        // Verify path calculation started
        if (navAgent.pathPending)
        {
            if (showDebugLogs)
                Debug.Log($"[CustomerMovement] {name}: Path calculation started to {targetPosition}");
            return true;
        }
        else
        {
            Debug.LogWarning($"[CustomerMovement] {name}: Failed to start path calculation to {targetPosition}");
            return false;
        }
    }
}
```

**Explanation**:
- **Position Validation**: Uses NavMesh.SamplePosition to verify targets are reachable
- **Automatic Correction**: Attempts to find nearby valid positions when initial target fails
- **Path Verification**: Checks that NavMeshAgent successfully starts path calculation
- **Configurable Parameters**: Exposes key NavMesh settings for tuning

### Example 3: Movement Completion Detection
**File**: `Assets/Scripts/3 - Systems/AI/Customer/Movement/CustomerMovement.cs`  
**Lines**: 180-220

```csharp
public bool HasReachedDestination()
{
    if (navAgent == null)
        return true; // Consider reached if no agent
    
    // Don't consider reached while path is still calculating
    if (navAgent.pathPending)
        return false;
    
    // Check if agent has a valid path
    if (navAgent.pathStatus == NavMeshPathStatus.PathInvalid)
    {
        Debug.LogWarning($"[CustomerMovement] {name}: Invalid path detected");
        return false; // Will be handled as movement failure
    }
    
    // Check if within stopping distance
    if (!navAgent.hasPath || navAgent.velocity.sqrMagnitude < 0.1f)
    {
        float distanceToDestination = Vector3.Distance(transform.position, navAgent.destination);
        bool reached = distanceToDestination <= navAgent.stoppingDistance + 0.1f;
        
        if (reached && showDebugLogs)
            Debug.Log($"[CustomerMovement] {name}: Reached destination (distance: {distanceToDestination:F2})");
        
        return reached;
    }
    
    return false;
}

public bool IsMovementFailed()
{
    if (navAgent == null)
        return true;
    
    // Check for invalid path
    if (navAgent.pathStatus == NavMeshPathStatus.PathInvalid)
        return true;
    
    // Check for partial path (destination unreachable)
    if (navAgent.pathStatus == NavMeshPathStatus.PathPartial)
    {
        Debug.LogWarning($"[CustomerMovement] {name}: Partial path - destination may be unreachable");
        return true;
    }
    
    // Check if agent is stuck (not moving for extended period)
    if (navAgent.hasPath && navAgent.velocity.sqrMagnitude < 0.01f)
    {
        // Could add time-based stuck detection here
        return false; // For now, don't consider this a failure
    }
    
    return false;
}
```

**Explanation**:
- **Path Status Monitoring**: Checks NavMeshAgent.pathStatus for validity
- **Distance-Based Detection**: Uses stopping distance plus tolerance for arrival detection
- **Velocity Consideration**: Ensures agent has actually stopped moving
- **Failure Detection**: Identifies stuck agents and invalid paths

---

## 🔧 Hands-On Exercise

### Exercise: Create a Smart Waypoint Navigation Task
**Objective**: Build a navigation task that handles multiple waypoints with fallback strategies

#### Instructions
1. Create a `WaypointNavigationTask` that moves through a series of waypoints
2. Implement automatic waypoint skipping when positions are unreachable
3. Add visualization to show planned vs actual paths
4. Test with complex NavMesh areas including obstacles

#### Starter Code
```csharp
using UnityEngine;
using UnityEngine.AI;
using Opsive.BehaviorDesigner.Runtime.Tasks.Actions;
using Opsive.BehaviorDesigner.Runtime.Tasks;
using System.Collections.Generic;

public class WaypointNavigationTask : Action
{
    [Tooltip("Array of waypoint positions to navigate through")]
    public Transform[] waypoints;
    
    [Tooltip("Skip unreachable waypoints instead of failing")]
    public bool skipUnreachableWaypoints = true;
    
    private NavMeshAgent navAgent;
    private int currentWaypointIndex = 0;
    private bool isMoving = false;
    
    public override void OnStart()
    {
        navAgent = GetComponent<NavMeshAgent>();
        if (navAgent == null)
        {
            Debug.LogError("[WaypointNavigationTask] NavMeshAgent component required!");
            return;
        }
        
        if (waypoints == null || waypoints.Length == 0)
        {
            Debug.LogError("[WaypointNavigationTask] No waypoints assigned!");
            return;
        }
        
        currentWaypointIndex = 0;
        // TODO: Start navigation to first waypoint
    }
    
    public override TaskStatus OnUpdate()
    {
        // TODO: Check if current waypoint reached
        // TODO: Move to next waypoint or complete task
        // TODO: Handle unreachable waypoints
        
        return TaskStatus.Running;
    }
    
    private bool MoveToWaypoint(int waypointIndex)
    {
        // TODO: Validate waypoint position
        // TODO: Start navigation with fallback handling
        return false;
    }
}
```

#### Success Criteria
- [ ] Successfully navigates through all reachable waypoints
- [ ] Gracefully handles unreachable waypoints based on settings
- [ ] Provides clear debug information about navigation decisions
- [ ] Works reliably in complex NavMesh environments

#### Common Issues & Solutions
- **Issue**: Agent gets stuck between waypoints
  - **Solution**: Check for NavMeshPathStatus.PathPartial and implement stuck detection
- **Issue**: Waypoints appear unreachable when they should be valid
  - **Solution**: Increase navMeshSampleDistance or check NavMesh baking settings

---

## 🧠 Knowledge Check

### Quick Questions
1. **Question**: Why should you validate NavMesh positions before calling SetDestination()?
   - **Answer**: SetDestination() with invalid positions can cause unpredictable behavior or movement failures

2. **Question**: What's the difference between pathPending and hasPath?
   - **Answer**: pathPending means path calculation is in progress; hasPath means a valid path exists and navigation can proceed

3. **Question**: How do you detect if a NavMeshAgent is stuck?
   - **Answer**: Monitor velocity over time - if agent has a path but velocity remains near zero, it may be stuck

### Code Review Challenge
**Given Code**: 
```csharp
public class BadMovementTask : Action
{
    public override TaskStatus OnUpdate()
    {
        NavMeshAgent agent = GetComponent<NavMeshAgent>();
        agent.SetDestination(Vector3.zero);
        return agent.remainingDistance < 0.1f ? TaskStatus.Success : TaskStatus.Running;
    }
}
```

**Tasks**:
- **Issues**: No null checking, sets destination every frame, no position validation, unreliable completion detection
- **Problems**: Performance impact, potential crashes, movement to invalid positions
- **Improvements**: Move SetDestination to OnStart(), add validation, use proper completion detection
- **Best Practices**: Validate components, handle errors, use stopping distance for completion

---

## 🔍 Deep Dive: Advanced Topics

### Advanced Concept 1: Dynamic NavMesh Updates
When environment changes during gameplay, NavMesh may need updates:

```csharp
public void HandleDynamicObstacle(GameObject obstacle)
{
    NavMeshObstacle navObstacle = obstacle.GetComponent<NavMeshObstacle>();
    if (navObstacle != null && navObstacle.carving)
    {
        // Recalculate path when obstacles change
        if (navAgent.hasPath)
        {
            navAgent.ResetPath();
            navAgent.SetDestination(originalDestination);
        }
    }
}
```

### Performance Considerations
- **Path Recalculation**: Only recalculate paths when necessary
- **Update Frequency**: Don't check arrival every frame for distant targets
- **NavMesh Quality**: Higher quality NavMesh reduces pathfinding issues but increases memory

### Integration Patterns
- **Command Pattern**: Encapsulate movement commands for undo/replay
- **Observer Pattern**: Notify other systems when movement completes
- **State Pattern**: Different movement behaviors for different AI states

---

## 🔗 Connections & Next Steps

### Related Lessons
- **Previous**: Lesson 1.2 - Customer AI State Management - How movement state integrates with overall AI state
- **Next**: Lesson 1.4 - Complex Checkout Workflow - Multi-step navigation sequences
- **Related**: Lesson 3.2 - Cross-Component Communication - Advanced movement coordination

### Real Project Usage
**Current Implementation**: 5 different movement tasks handling various navigation scenarios  
**Evolution**: Started with simple movement, evolved to handle complex pathfinding edge cases  
**Future Enhancements**: Could add crowd avoidance, dynamic pathfinding, or multi-floor navigation

### Extended Learning
- **Unity Documentation**: [Navigation and Pathfinding](https://docs.unity3d.com/Manual/nav-NavigationSystem.html)
- **NavMesh Best Practices**: [Unity NavMesh Guide](https://docs.unity3d.com/Manual/nav-BuildingNavMesh.html)
- **AI Movement Patterns**: [Game AI Movement Techniques](https://www.red3d.com/cwr/steer/)

---

## 📚 Resources & References

### Project Files
- MoveToShelfAction.cs - Robust movement task with comprehensive error handling
- CustomerMovement.cs - NavMesh integration utilities and validation
- MoveToExitTask.cs - Demonstrates exit navigation with fallback strategies

### External Resources
- [Unity NavMesh Agent API](https://docs.unity3d.com/ScriptReference/AI.NavMeshAgent.html)
- [NavMesh Pathfinding](https://docs.unity3d.com/Manual/nav-InnerWorkings.html)
- [Behavior Designer Movement Tasks](https://opsive.com/support/documentation/behavior-designer/movement-pack/)

---

## 📝 Instructor Notes

### Teaching Tips
- Use Scene view to visualize NavMesh and show pathfinding in real-time
- Demonstrate common pathfinding failures by temporarily disabling NavMesh areas
- Show the difference between SetDestination() validation and direct movement
- Use the Navigation window to show NavMesh baking settings impact

### Time Management
- **Concept Introduction**: 25 minutes
- **Code Examples**: 30 minutes  
- **Hands-On Exercise**: 15 minutes
- **Q&A and Review**: 5 minutes

### Assessment Rubric
| Criteria | Excellent (4) | Proficient (3) | Developing (2) | Beginning (1) |
|----------|---------------|----------------|----------------|---------------|
| NavMesh Integration | Perfect pathfinding with error handling | Good navigation, minor issues | Basic movement works | Poor or broken navigation |
| Error Handling | Comprehensive validation and fallbacks | Good error checking | Some error handling | Little or no error handling |
| Performance | Optimal NavMesh usage | Good performance practices | Acceptable performance | Poor performance practices |

---

*Lesson Version: 1.0*  
*Last Updated: December 29, 2025*  
*Author: Claude with Project Context*