# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Unity Project Overview

This is a **Tabletop Wargaming Shop Simulator** built in Unity 6000.2.0b1 - a first-person simulation game where players run a hobby store specializing in wargaming products. The game combines business simulation with narrative elements and AI-driven customer interactions.

## Development Commands

### Unity Build & Test
- Open project in Unity 6000.2.0b1
- **Build**: File → Build Settings → Build (or Ctrl/Cmd+Shift+B)
- **Play Mode**: Unity Editor → Play button (▶️)
- **Tests**: Window → General → Test Runner → PlayMode/EditMode tests

### Assembly Testing
The project uses NUnit testing framework with custom assembly definitions:
- **TabletopShop.Tests.asmdef**: Main test assembly
- Run tests via Unity Test Runner window
- Integration tests located in `Assets/Scripts/6 - Testing/`

### No External Build Tools
This is a pure Unity project without external build scripts, npm, or package managers beyond Unity's Package Manager.

## Project Architecture

### Core Design Patterns
- **Pure Coordinator Pattern**: Customer class coordinates specialized components without delegation
- **Interface Segregation**: Comprehensive interface system (ICustomer, IInventoryManager, IEconomicValidator)
- **ScriptableObject-Driven**: Data containers for products, lore, dialogue, and game configuration
- **Event-Driven Architecture**: UnityEvents for loose coupling between systems
- **Composition Over Inheritance**: Modular component design throughout

### Folder Structure (Assets/Scripts/)
```
0 - ScriptableObjects/     # Data containers (Dialogue, Lore)
1 - Core/                  # Core management and interfaces  
2 - Entities/              # Game entities (Player, Products, Shop)
3 - Systems/               # Game systems (AI, Audio, Economy, Inventory)
4 - UI/                    # User interface components
5 - Tools/                 # Editor tools and utilities
6 - Testing/               # Test files and integration tests
7 - Legacy/                # Deprecated code (avoid using)
Dialogue/                  # Dialogue system components
Interfaces/                # Core interfaces
Repository/                # Data access patterns
```

### Key Systems

#### Customer AI System
- **Dual Architecture**: State machine (new) and coroutine-based (legacy) systems
- **State Machine**: CustomerState enum (Entering → Shopping → Purchasing → Leaving)
- **Component Composition**: CustomerMovement, CustomerBehavior, CustomerVisuals
- **NavMesh Movement**: Unity AI Navigation for pathfinding
- **Direct Component Access**: Use `customer.Movement.X` not delegation methods

##### State Machine Implementation
- **Toggle Control**: `useStateMachine` bool in CustomerBehavior
- **State Classes**: EnteringState, ShoppingState, PurchasingState, LeavingState
- **Timing Constants**: 
  - `PRODUCT_CHECK_INTERVAL = 3f` - Time between product availability checks
  - `MINIMUM_BROWSE_TIME = 5f` - Required shelf browsing duration
  - `POST_SWITCH_WAIT_TIME = 2f` - Delay after switching shelves
  - `CHECKOUT_COMPLETION_TIMEOUT = 30f` - Max wait for checkout callbacks

##### Critical Timing Considerations
- **Absolute vs Relative Time**: Use `Time.time` for consistency in state timing
- **Initialization**: Set `lastProductCheckTime` to allow immediate first checks
- **Interval Checks**: Ensure proper time difference calculations
- **Debugging**: Frame-rate logging every 60 frames (~1 second intervals)

#### Economic System (GameManager)
- **Central Authority**: All economic transactions flow through GameManager
- **Validation**: IEconomicValidator interface for transaction validation
- **Daily Cycle**: Revenue/expense tracking with day/night progression
- **Event Publishing**: Economic events broadcast to UI systems

#### Inventory System
- **Repository Pattern**: Centralized inventory data access
- **Economic Integration**: Stock purchasing requires economic validation
- **Event-Driven Updates**: UI automatically reflects inventory changes
- **Product Association**: Products linked to shelf slots and customer interactions

#### Dialogue & Lore System
- **ScriptableObject Data**: DialogueTreeSO and LoreKnowledgeBaseSO
- **Branching Logic**: Conditional dialogue responses
- **Knowledge Tracking**: Player lore progression system
- **Reputation Integration**: Dialogue affects faction relationships

### Interface Guidelines
When working with existing systems:
- **Customer AI**: Use direct component access (`customer.Behavior.CurrentState`)
- **Inventory**: Use IInventoryManager interface for operations
- **Economy**: Validate all transactions through GameManager
- **Products**: Use component composition (ProductVisuals, ProductEconomics, etc.)

### Package Dependencies
Key Unity packages used:
- **AI Navigation (2.0.8)**: NavMesh customer movement
- **Input System (1.14.0)**: New Unity input handling
- **Universal Render Pipeline (17.2.0)**: Rendering system
- **Test Framework (1.5.1)**: NUnit testing
- **Unity MCP Bridge**: AI development tools integration

### Alpha Development Focus
Current development targets a 15-20 minute Alpha build featuring:
- Shop environment with first-person player movement
- Customer AI with realistic shopping behavior
- Lore codex system for fictional wargaming universes
- Dialogue system with branching conversations
- Inventory ordering and shelf stocking mechanics
- Basic economic simulation with reputation tracking

### Code Style & Conventions
- Use composition over inheritance
- Implement interfaces for major systems
- ScriptableObjects for data configuration
- UnityEvents for loose coupling
- Comprehensive error handling with try-catch in lifecycle methods
- Component validation in OnValidate() methods
- Clear separation between data, logic, and presentation layers

### Testing Strategy
- Unit tests for core economic logic
- Integration tests for system interactions
- Runtime tests for customer AI behavior
- UI component testing for inventory interactions
- Use Unity Test Runner for all testing workflows

#### AI Behavior Testing & Debugging
- **State Machine Validation**: Verify one-to-one functionality parity with legacy coroutines
- **Timing Analysis**: Monitor frame-rate performance with comprehensive debug logging
- **Systematic Debugging**: Use structured logging prefixes for categorized issue tracking
- **Product Selection Testing**: Validate affordability calculations and probability logic
- **Movement Integration**: Test NavMesh pathfinding with state transitions

## Debugging Methodologies

### Systematic AI Debugging Approach
When debugging complex AI behaviors (especially Customer AI state machines):

1. **Comprehensive Logging Strategy**:
   - Use structured debug prefixes for categorization
   - Log both successful and failed conditions
   - Include timing information and state transitions
   - Track numerical values for calculations

2. **Debug Logging Prefixes**:
   ```
   [SHOPPING-DEBUG] - General shopping state tracking
   [SHELF-DEBUG]    - Shelf browsing and movement behavior
   [PRODUCT-DEBUG]  - Product selection timing and conditions
   [SELECT-DEBUG]   - Detailed product selection process
   [WANT-DEBUG]     - Purchase probability calculations
   [PURCHASE-DEBUG] - Checkout and transaction flow
   ```

3. **State Machine Debugging Process**:
   - **Step 1**: Add frame-by-frame state tracking (every 60 frames)
   - **Step 2**: Log all timing calculations with exact values
   - **Step 3**: Track each condition in complex logic separately
   - **Step 4**: Use success/failure indicators (✅/❌) for clarity
   - **Step 5**: Include before/after values for state changes

4. **Timing Analysis Best Practices**:
   - Use consistent time references (prefer `Time.time` for absolute timing)
   - Initialize timing variables to allow immediate first checks
   - Log timing calculations with clear comparisons
   - Include both current values and required thresholds

5. **Common AI Debugging Patterns**:
   ```csharp
   // Good: Detailed condition logging
   bool condition = CheckCondition();
   Debug.Log($"[DEBUG] {name}: Condition check - {condition} (details)");
   
   // Good: Timing analysis
   float elapsed = Time.time - startTime;
   bool ready = elapsed >= threshold;
   Debug.Log($"[DEBUG] {name}: Timing - {elapsed:F1}s >= {threshold:F1}s = {ready}");
   
   // Good: State transition logging
   Debug.Log($"[DEBUG] {name}: {oldState} → {newState} (reason: {reason})");
   ```

6. **Performance Considerations**:
   - Use conditional compilation for debug builds: `#if UNITY_EDITOR`
   - Implement debug level controls for verbose logging
   - Remove or disable intensive logging for production builds
   - Consider frame-rate impact of excessive logging

## Development Workflow

### CLAUDE.md Maintenance Best Practices

#### When to Update CLAUDE.md
Update this documentation when making changes to:
- **Major Systems**: New AI behaviors, economic features, or core gameplay mechanics
- **Architecture**: Design pattern changes, interface modifications, or component restructuring  
- **Dependencies**: Unity package additions/removals or external tool integrations
- **Testing Approaches**: New debugging methodologies or testing strategies
- **Breaking Changes**: Modifications that affect how existing systems work

#### Update Process
1. **Concurrent Updates**: Modify CLAUDE.md in the same commit as related code changes
2. **Pull Request Integration**: Include documentation updates in feature PRs
3. **Accuracy Review**: Verify documentation matches actual implementation during code reviews
4. **Incremental Approach**: Make small, focused updates rather than large rewrites

#### Documentation Standards
- **Actionable Content**: Focus on "how-to" information for developers
- **Current State**: Remove outdated information promptly
- **Structured Format**: Use consistent markdown formatting and clear hierarchies
- **Code Examples**: Include practical code snippets for complex concepts
- **Debug Integration**: Document debugging approaches alongside system descriptions

#### Version Control
- Use descriptive commit messages for documentation updates
- Tag documentation-heavy commits for easy reference
- Maintain documentation accuracy as a code quality standard
- Review CLAUDE.md changes as thoroughly as code changes