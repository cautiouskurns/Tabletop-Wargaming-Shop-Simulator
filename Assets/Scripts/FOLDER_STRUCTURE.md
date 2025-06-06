# Scripts Folder Structure

This document describes the organized folder hierarchy for the Tabletop Wargaming Shop Simulator project scripts.

## 📁 Folder Organization

### 🔧 **Core/**
*Fundamental interfaces and core system components*
- `IInteractable.cs` - Base interface for all interactable objects
- `InteractionLayers.cs` - Layer management for interaction system

### 🎯 **Interaction/**
*Player interaction system components*
- `PlayerInteraction.cs` - Handles player interaction logic and raycasting
- `InteractionSystemDebugger.cs` - Debug tools and visualization for interaction system

### 🎮 **Player/**
*Player control and movement systems*
- `PlayerController.cs` - Advanced player movement controller
- `SimplePlayerController.cs` - Basic player movement implementation

### 📦 **Products/**
*Product system and data management*
- `Product.cs` - Individual product behavior and interaction
- `ProductData.cs` - ScriptableObject for product data storage
- `ProductType.cs` - Enum definitions for product categories

### 🏪 **Shop/**
*Shop management and inventory systems*
- `InventoryManager.cs` - Singleton manager for player inventory
- `Shelf.cs` - Shop shelf behavior and management
- `ShelfSlot.cs` - Individual shelf slot interaction and placement

### 🖥️ **UI/**
*User interface components*
- `CrosshairUI.cs` - Crosshair behavior and visual feedback

### 🧪 **Testing/**
*Test scripts and integration tests*
- `InventoryTestSimple.cs` - Inventory system testing
- `ProductTester.cs` - Product functionality testing
- `ShelfTester.cs` - Shelf system testing
- `ShopSystemIntegrationTest.cs` - Complete system integration tests

### 🛠️ **Utilities/**
*Setup helpers and utility scripts*
- `InteractionSystemSetupHelper.cs` - Runtime setup and configuration helper
- `QuickInteractionSetup.cs` - Quick setup utility for interaction system

### 📝 **Editor/**
*Unity Editor tools and custom inspectors*
- `LayerSetup.cs` - Editor script for automatic layer configuration
- `ProductDataCreator.cs` - Editor tool for creating product data assets
- `ProductPrefabCreator.cs` - Editor utility for generating product prefabs
- `ShelfPrefabCreator.cs` - Editor utility for creating shelf prefabs

## 🔄 Dependencies Flow

```
Core (IInteractable, InteractionLayers)
  ↓
Interaction (PlayerInteraction, Debugger)
  ↓
Player (Controllers) ←→ Products (Product, ProductData)
  ↓                        ↓
UI (CrosshairUI)     ←→    Shop (Inventory, Shelves)
  ↓                        ↓
Testing (All test scripts)
  ↓
Utilities (Setup helpers)
```

## 📋 Best Practices

### Adding New Scripts
1. **Core** - Add fundamental interfaces and system-wide components
2. **Interaction** - Add interaction-specific logic and behaviors
3. **Player** - Add player-related functionality and controls
4. **Products** - Add product types, data structures, and behaviors
5. **Shop** - Add shop management, inventory, and commerce logic
6. **UI** - Add user interface elements and HUD components
7. **Testing** - Add test scripts and validation tools
8. **Utilities** - Add setup helpers and development tools
9. **Editor** - Add Unity Editor extensions and custom inspectors

### Naming Conventions
- Use descriptive names that indicate the script's purpose
- Group related functionality in the same folder
- Keep interfaces in Core folder
- Keep MonoBehaviour components in their respective domain folders

### Cross-Folder Dependencies
- Core scripts should have minimal dependencies
- Higher-level systems can depend on Core
- Avoid circular dependencies between domain folders
- Use interfaces (from Core) to decouple systems

## 🚀 Getting Started

1. **Setup**: Run scripts from `Utilities/` folder to configure the system
2. **Core Systems**: Ensure `Core/` scripts are properly configured
3. **Player Setup**: Configure player using `Player/` scripts
4. **Testing**: Use `Testing/` scripts to validate functionality
5. **Development**: Add new features to appropriate domain folders

This organization promotes:
- **Maintainability** - Easy to find and modify related functionality
- **Scalability** - Clear structure for adding new features
- **Reusability** - Modular components that can be easily reused
- **Team Collaboration** - Clear ownership and responsibility areas
