# Scripts Folder - Organized Structure

## 📂 Complete Folder Hierarchy

```
Scripts/
├── 📁 Core/                           # Fundamental interfaces and core systems
│   ├── IInteractable.cs              # Base interface for all interactable objects
│   └── InteractionLayers.cs          # Layer management for interaction system
│
├── 📁 Editor/                         # Unity Editor tools and extensions
│   ├── LayerSetup.cs                 # Automatic layer configuration
│   ├── ProductDataCreator.cs         # Product data asset creation tool
│   ├── ProductPrefabCreator.cs       # Product prefab generation utility
│   └── ShelfPrefabCreator.cs         # Shelf prefab creation utility
│
├── 📁 Interaction/                    # Player interaction system
│   ├── InteractionSystemDebugger.cs  # Debug tools and visualization
│   └── PlayerInteraction.cs          # Player interaction logic and raycasting
│
├── 📁 Player/                         # Player control systems
│   ├── PlayerController.cs           # Advanced player movement controller
│   └── SimplePlayerController.cs     # Basic player movement implementation
│
├── 📁 Products/                       # Product system and data
│   ├── Product.cs                    # Individual product behavior
│   ├── ProductData.cs                # ScriptableObject for product data
│   └── ProductType.cs                # Product category enums
│
├── 📁 Shop/                          # Shop management and inventory
│   ├── InventoryManager.cs           # Singleton inventory manager
│   ├── Shelf.cs                      # Shop shelf behavior
│   └── ShelfSlot.cs                  # Individual shelf slot logic
│
├── 📁 Testing/                       # Test scripts and validation
│   ├── InventoryTestSimple.cs        # Inventory system testing
│   ├── ProductTester.cs              # Product functionality testing
│   ├── ShelfTester.cs                # Shelf system testing
│   └── ShopSystemIntegrationTest.cs  # Full system integration tests
│
├── 📁 UI/                            # User interface components
│   └── CrosshairUI.cs                # Crosshair behavior and feedback
│
├── 📁 Utilities/                     # Setup helpers and tools
│   ├── InteractionSystemSetupHelper.cs # Runtime setup and configuration
│   └── QuickInteractionSetup.cs       # Quick setup utility
│
├── FOLDER_STRUCTURE.md               # This documentation file
└── README.md                         # Original project readme
```

## 🎯 Organization Benefits

### ✅ **Improved Maintainability**
- Related scripts are grouped together
- Easy to locate specific functionality
- Clear separation of concerns

### ✅ **Better Scalability**
- Clear structure for adding new features
- Logical grouping supports growth
- Modular organization

### ✅ **Enhanced Collaboration**
- Team members can easily find relevant code
- Clear ownership boundaries
- Consistent organization patterns

### ✅ **Simplified Navigation**
- Folder names clearly indicate content
- Hierarchical structure matches functionality
- Reduced cognitive load when browsing code

## 🔧 Usage Notes

- **Core** folder contains the foundation - modify carefully
- **Editor** scripts only run in Unity Editor
- **Testing** scripts can be removed in production builds
- **Utilities** are meant for development and setup phases
- Each domain folder is relatively independent

This organization follows Unity best practices and industry standards for medium to large-scale projects.
