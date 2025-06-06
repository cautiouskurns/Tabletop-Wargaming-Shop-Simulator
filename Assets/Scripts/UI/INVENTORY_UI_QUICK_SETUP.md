# InventoryUI Quick Setup Reference

## UI Hierarchy Structure
```
InventoryCanvas (Canvas + InventoryUI script)
└── InventoryPanel (GameObject + CanvasGroup)
    ├── MiniatureBoxButton (Button + TextMeshPro child)
    ├── PaintPotButton (Button + TextMeshPro child)
    └── RulebookButton (Button + TextMeshPro child)
```

## Inspector Setup Checklist

### InventoryUI Component (on InventoryCanvas):
- ✅ **Inventory Canvas Group:** InventoryPanel's CanvasGroup
- ✅ **Product Buttons [3]:**
  - Element 0: MiniatureBoxButton
  - Element 1: PaintPotButton  
  - Element 2: RulebookButton

### InventoryManager (separate GameObject):
- ✅ **Available Products [3]:**
  - IronLegionStarter.asset (MiniatureBox)
  - CrimsonBattlePaint.asset (PaintPot)
  - CoreRulebook.asset (Rulebook)
- ✅ **Starting Quantity Per Product:** 5
- ✅ **Auto Load Products On Start:** ✓

## Controls
- **Tab Key:** Toggle inventory panel
- **Mouse Click:** Select product type
- **Number Keys 1-3:** Quick select (if you add this feature)

## Expected Assets
Your existing ProductData assets:
- `IronLegionStarter.asset` - Should be ProductType.MiniatureBox
- `CrimsonBattlePaint.asset` - Should be ProductType.PaintPot  
- `CoreRulebook.asset` - Should be ProductType.Rulebook

## Test Steps
1. Play the scene
2. Press Tab → Panel should fade in
3. Click any button → Button should highlight yellow
4. Press Tab again → Panel should fade out
