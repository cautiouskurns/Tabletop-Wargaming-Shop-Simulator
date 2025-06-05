# Product GameObject System - Task 2.2

## Overview
The Product GameObject System handles individual product instances in the tabletop shop game world. Each product is a GameObject with interactive capabilities, visual feedback, and state management.

## Files Created

### 1. Product.cs
**Location:** `Assets/Scripts/Product.cs`

**Purpose:** Main MonoBehaviour component that manages individual product instances

**Key Features:**
- **Component Requirements:** Automatically requires MeshRenderer and Collider
- **Product Data Integration:** Links to ProductData ScriptableObjects
- **Price Management:** Handles current pricing (defaults to ProductData.basePrice)
- **State Management:** Tracks shelf placement and purchase status
- **Mouse Interactions:** Click-to-purchase and hover effects
- **Visual Feedback:** Highlights products on mouse hover
- **Error Handling:** Comprehensive null checking and validation

**Public Methods:**
- `Initialize(ProductData data)` - Set up product from ScriptableObject
- `SetPrice(int newPrice)` - Change product price
- `Purchase()` - Handle customer purchase
- `PlaceOnShelf()` - Mark product as available for sale
- `RemoveFromShelf()` - Remove product from sale

**Properties:**
- `ProductData ProductData` - Reference to source data
- `int CurrentPrice` - Current selling price
- `bool IsOnShelf` - Whether product is available for purchase
- `bool IsPurchased` - Whether product has been sold

### 2. ProductPrefabCreator.cs
**Location:** `Assets/Scripts/Editor/ProductPrefabCreator.cs`

**Purpose:** Editor utility to create product prefabs automatically

**Usage:** 
1. Go to menu: `Tabletop Shop > Create Product Prefabs`
2. Creates 3 prefabs with proper components and materials
3. Automatically sets up materials and saves assets

**Creates:**
- `MiniatureBox_Prefab.prefab` - Orange-brown cube (1×0.6×1)
- `PaintPot_Prefab.prefab` - Blue cylinder (0.4×0.6×0.4)
- `Rulebook_Prefab.prefab` - Purple thin cube (0.7×1.2×0.1)

### 3. ProductTester.cs
**Location:** `Assets/Scripts/ProductTester.cs`

**Purpose:** Testing and validation script for Product functionality

**Usage:**
1. Add to any GameObject in scene
2. Assign a product prefab and optional ProductData
3. Press 'T' to spawn test product
4. Press 'R' to run automated tests
5. Click spawned product to test purchase
6. Hover over product to test visual feedback

## Product Prefabs

### MiniatureBox_Prefab
- **Shape:** Cube scaled to 1×0.6×1 (wide, short box)
- **Material:** Orange-brown with low metallic finish
- **Represents:** Miniature starter sets and army boxes

### PaintPot_Prefab
- **Shape:** Cylinder scaled to 0.4×0.6×0.4 (small pot)
- **Material:** Blue with medium metallic finish
- **Represents:** Acrylic paint containers

### Rulebook_Prefab
- **Shape:** Thin cube scaled to 0.7×1.2×0.1 (book proportions)
- **Material:** Purple with matte finish
- **Represents:** Gaming rulebooks and codexes

## Materials Created

### MiniatureBoxMaterial.mat
- **Color:** RGB(0.8, 0.4, 0.2) - Orange-brown
- **Metallic:** 0.1 (low shine)
- **Smoothness:** 0.3 (slightly rough)

### PaintPotMaterial.mat
- **Color:** RGB(0.2, 0.6, 0.8) - Blue
- **Metallic:** 0.3 (medium shine)
- **Smoothness:** 0.7 (smooth surface)

### RulebookMaterial.mat
- **Color:** RGB(0.6, 0.2, 0.4) - Purple
- **Metallic:** 0.0 (no metallic)
- **Smoothness:** 0.2 (matte finish)

## Interaction System

### Mouse Interactions
- **OnMouseDown():** Triggers purchase when clicked
- **OnMouseEnter():** Applies yellow highlight effect
- **OnMouseExit():** Removes highlight effect

### Visual Feedback
- **Hover Effect:** Changes material to bright yellow with emission
- **State-Based Visibility:** Hides purchased products
- **Material Management:** Prevents memory leaks with proper cleanup

### Purchase Logic
- Validates product is on shelf and not already purchased
- Disables visual rendering and collision
- Logs purchase information
- Updates internal state flags

## Integration Points

### With ProductData System
```csharp
// Initialize product from ScriptableObject
Product product = productGO.GetComponent<Product>();
product.Initialize(productDataAsset);
```

### With Shop Management
```csharp
// Place product on shelf
product.PlaceOnShelf();

// Handle customer purchase
if (product.IsOnShelf && !product.IsPurchased)
{
    product.Purchase(); // Handles everything automatically
}
```

## Testing Instructions

1. **Run ProductPrefabCreator:**
   - Menu: `Tabletop Shop > Create Product Prefabs`
   - Verify 3 prefabs created in `Assets/Prefabs/`
   - Verify 3 materials created in `Assets/Materials/`

2. **Test Product Interactions:**
   - Add `ProductTester` to any GameObject in scene
   - Assign one of the created prefabs
   - Press 'T' to spawn test product
   - Click on product to test purchase
   - Hover over product to test visual feedback

3. **Validate Component Setup:**
   - Check prefabs have `Product` component
   - Verify `MeshRenderer` and `Collider` are present
   - Confirm materials are properly assigned

## Next Steps

This system provides the foundation for:
- **Shelf Management:** Products can be placed on shelves
- **Customer AI:** Customers can interact with products
- **Inventory System:** Products can be tracked and managed
- **Price Setting UI:** Players can adjust product prices
- **Shop Economics:** Revenue tracking and profit calculation

## Error Handling

The system includes comprehensive error handling:
- Null reference checking for all components
- Validation of product states before operations
- Graceful handling of invalid price values
- Debug logging for all major operations
- Memory leak prevention with material cleanup
