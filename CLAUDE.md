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
- **State Machine**: CustomerState enum (Entering → Shopping → Purchasing → Leaving)
- **Component Composition**: CustomerMovement, CustomerBehavior, CustomerVisuals
- **NavMesh Movement**: Unity AI Navigation for pathfinding
- **Direct Component Access**: Use `customer.Movement.X` not delegation methods

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