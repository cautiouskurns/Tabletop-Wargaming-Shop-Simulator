# Unity Player Interaction System - Testing Guide

## Overview
This guide provides comprehensive testing procedures for the Unity player interaction system with raycast-based interactions.

## System Components

### Core Components
- **IInteractable Interface**: Defines interaction contract for all interactable objects
- **PlayerInteraction.cs**: Main raycast system with E key input handling
- **CrosshairUI.cs**: Visual feedback system with color-changing crosshair
- **SimplePlayerController.cs**: First-person movement and camera control
- **InteractionLayers.cs**: Layer management utility for proper filtering

### Interactable Objects
- **ShelfSlot.cs**: Implements shelf slot interactions (empty/remove product)
- **Product.cs**: Implements product purchase interactions with hover effects

## Required Unity Setup

### 1. Layer Configuration
Navigate to **Edit > Project Settings > Tags and Layers** and add these layers:
- Layer 8: `Interactable`
- Layer 9: `Product` 
- Layer 10: `Shelf`

### 2. Scene Setup
Use the `InteractionDemoSetup` script to automatically create a test scene:
1. Create empty GameObject in scene
2. Add `InteractionDemoSetup` component
3. Click "Setup Demo Scene" in inspector
4. This creates: Player, Environment, Shelf with products

## Testing Procedures

### Test 1: Layer Validation
**Objective**: Verify all required layers are properly configured

**Steps**:
1. Open Unity Console
2. Enter Play mode
3. Check for layer validation messages
4. Should see: "All interaction layers are properly configured"

**Expected Results**:
- ✅ No missing layer warnings
- ✅ Confirmation message appears
- ❌ Warning messages indicate missing layers

### Test 2: Player Movement
**Objective**: Verify first-person controls work correctly

**Steps**:
1. Enter Play mode
2. Test WASD movement
3. Test mouse look (should move camera)
4. Test cursor lock (ESC to unlock, click to lock)

**Expected Results**:
- ✅ Smooth WASD movement in all directions
- ✅ Mouse look rotates camera smoothly
- ✅ Cursor locks/unlocks properly
- ❌ Jerky movement or non-responsive controls

### Test 3: Crosshair UI
**Objective**: Verify crosshair appears and changes color appropriately

**Steps**:
1. Enter Play mode
2. Look around scene (crosshair should be white)
3. Look at shelf slots (crosshair should turn yellow)
4. Look at products (crosshair should turn yellow)
5. Check interaction text appears

**Expected Results**:
- ✅ White crosshair visible in center of screen
- ✅ Crosshair turns yellow when looking at interactables
- ✅ Interaction text appears below crosshair
- ✅ Smooth color transitions
- ❌ No crosshair visible or color doesn't change

### Test 4: Raycast Detection
**Objective**: Verify raycast system detects interactables correctly

**Steps**:
1. Enter Play mode
2. Enable Gizmos in Scene view
3. Look at different objects while watching Scene view
4. Debug ray should be visible from camera

**Expected Results**:
- ✅ Debug ray visible in Scene view during play
- ✅ Ray length is 3 units
- ✅ Ray originates from camera center
- ✅ Ray color changes when hitting interactables
- ❌ No debug ray visible or incorrect origin/length

### Test 5: ShelfSlot Interactions
**Objective**: Test shelf slot interaction behavior

**Steps**:
1. Look at empty shelf slot
2. Verify interaction text shows "Empty Slot"
3. Press E key
4. Look at occupied shelf slot
5. Verify interaction text shows "Remove [ProductName]"
6. Press E key to remove product

**Expected Results**:
- ✅ Empty slots show "Empty Slot" text
- ✅ Occupied slots show "Remove [ProductName]" text
- ✅ E key on empty slot shows feedback message
- ✅ E key on occupied slot removes product
- ❌ Incorrect interaction text or E key not working

### Test 6: Product Interactions
**Objective**: Test product purchase interactions

**Steps**:
1. Look at a product on shelf
2. Verify interaction text shows price
3. Press E key to purchase
4. Check console for purchase messages
5. Verify product is removed from shelf

**Expected Results**:
- ✅ Products show price in interaction text
- ✅ E key triggers purchase interaction
- ✅ Purchase messages appear in console
- ✅ Product disappears after purchase
- ❌ No price shown or purchase not working

### Test 7: Interaction Range
**Objective**: Verify 3-unit interaction range

**Steps**:
1. Stand close to interactable (within 3 units)
2. Verify crosshair turns yellow
3. Move away slowly until crosshair turns white
4. Distance should be approximately 3 units

**Expected Results**:
- ✅ Interactions work within 3 units
- ✅ Interactions stop working beyond 3 units
- ✅ Visual feedback matches interaction range
- ❌ Range too short/long or inconsistent

### Test 8: LayerMask Filtering
**Objective**: Verify only interactable layers are detected

**Steps**:
1. Look at environment objects (floor, walls)
2. Crosshair should remain white
3. Look at interactable objects
4. Crosshair should turn yellow

**Expected Results**:
- ✅ Non-interactable objects don't trigger crosshair
- ✅ Only objects on correct layers are detected
- ✅ LayerMask filtering working properly
- ❌ All objects trigger interactions

## Troubleshooting

### Common Issues

**No Crosshair Visible**
- Check CrosshairUI component is on Player
- Verify Canvas and UI elements are properly created
- Check camera reference in PlayerInteraction

**Crosshair Doesn't Change Color**
- Verify layers are set up correctly
- Check LayerMask configuration in PlayerInteraction
- Ensure interactable objects have correct layers

**E Key Not Working**
- Check Input System settings
- Verify PlayerInteraction component is active
- Check console for interaction messages

**Debug Ray Not Visible**
- Enable Gizmos in Scene view
- Check if in Play mode
- Verify PlayerInteraction.showDebugRay is true

**Products/Slots Not Interactable**
- Check layer assignments on objects
- Verify IInteractable implementation
- Check colliders are present and configured

### Performance Considerations

**Raycast Optimization**
- System performs one raycast per frame
- Uses LayerMask for efficient filtering
- Debug visualization only active in Editor

**UI Performance**
- Crosshair updates only when needed
- Smooth transitions prevent visual jarring
- Text updates minimized for efficiency

## Integration Notes

### Adding New Interactables
1. Implement IInteractable interface
2. Set appropriate layer using InteractionLayers utility
3. Ensure collider is present for raycast detection
4. Test interaction text and behavior

### Customizing Interaction Range
- Modify `interactionRange` in PlayerInteraction
- Update debug ray length accordingly
- Consider UI scaling for longer ranges

### Extending UI Feedback
- CrosshairUI can be extended for additional visual effects
- Consider audio feedback for interactions
- Add haptic feedback for controller support

## Success Criteria

The interaction system is working correctly when:
- ✅ All layers properly configured
- ✅ Player movement smooth and responsive  
- ✅ Crosshair visible and changes color appropriately
- ✅ Debug ray visible in Scene view
- ✅ E key interactions work for all interactable types
- ✅ Interaction range correctly limited to 3 units
- ✅ LayerMask filtering prevents unwanted interactions
- ✅ All interaction text displays correctly
- ✅ Visual feedback smooth and responsive

## Next Steps

After successful testing:
1. Set up proper Unity layers in Project Settings
2. Create production scenes using InteractionDemoSetup
3. Add more interactable object types as needed
4. Consider adding audio/haptic feedback
5. Optimize for target platform performance
