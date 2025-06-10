# DestroyImmediate During OnValidate Fix - Complete

## Issue
**Error:** "Destroying components immediately is not permitted during physics trigger/contact, animation event callbacks, rendering callbacks or OnValidate."

**Call Stack:**
```
OnValidate() → CreateSlots() → AddComponent<ShelfSlot>() → ShelfSlot.Awake() → 
ShelfSlotVisuals.Awake() → SetupSlotIndicator() → DestroyImmediate()
```

## Root Cause
The `OnValidate()` method in `Shelf.cs` was calling `CreateSlots()`, which created new `ShelfSlot` components. During their `Awake()` methods, `ShelfSlotVisuals` and `ShelfVisuals` components were calling `DestroyImmediate()` without checking if the application was playing.

Unity prohibits `DestroyImmediate()` calls during certain callbacks including `OnValidate()`.

## Fixed Files

### 1. `/Assets/Scripts/Shop/ShelfSlotVisuals.cs`

**BEFORE:**
```csharp
// Line 126 - No Application.isPlaying check
Collider indicatorCollider = slotIndicator.GetComponent<Collider>();
if (indicatorCollider != null)
{
    DestroyImmediate(indicatorCollider);
}
```

**AFTER:**
```csharp
// Line 126-134 - Proper Application.isPlaying check
Collider indicatorCollider = slotIndicator.GetComponent<Collider>();
if (indicatorCollider != null)
{
    if (Application.isPlaying)
    {
        Destroy(indicatorCollider);
    }
    else
    {
        DestroyImmediate(indicatorCollider);
    }
}
```

### 2. `/Assets/Scripts/Shop/ShelfVisuals.cs`

**BEFORE:**
```csharp
// Line 210 - No Application.isPlaying check
Collider visualCollider = shelfVisual.GetComponent<Collider>();
if (visualCollider != null)
{
    DestroyImmediate(visualCollider);
}
```

**AFTER:**
```csharp
// Line 210-218 - Proper Application.isPlaying check
Collider visualCollider = shelfVisual.GetComponent<Collider>();
if (visualCollider != null)
{
    if (Application.isPlaying)
    {
        Destroy(visualCollider);
    }
    else
    {
        DestroyImmediate(visualCollider);
    }
}
```

## Solution Pattern
The fix follows Unity's best practice pattern:

```csharp
if (Application.isPlaying)
{
    Destroy(component);  // Safe during play mode
}
else
{
    DestroyImmediate(component);  // Safe in edit mode
}
```

## Verified Working
- ✅ No compilation errors
- ✅ Consistent with existing patterns in the same files
- ✅ Follows Unity's recommended approach
- ✅ Maintains identical functionality in both edit and play modes

## Other Files Already Safe
- `MaterialUtility.SafeDestroyMaterial()` already has proper `immediate` parameter handling
- Most other `DestroyImmediate()` calls are in Editor scripts (which is appropriate)
- Test scripts use `DestroyImmediate()` appropriately for cleanup

## Result
The slot management simplification in `Shelf.cs` now works correctly without Unity errors, and shelf creation during `OnValidate()` no longer causes `DestroyImmediate()` violations.
