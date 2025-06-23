# Customer State Indicator System

## Overview
The Customer State Indicator System provides visual feedback to show what state each customer is in. It displays floating text and/or icons above customers to indicate their current behavior state (Entering, Shopping, Purchasing, Leaving).

## Features

### Visual Indicators
- **Floating Text**: Shows state names above customers ("Entering", "Shopping", "Checkout", "Leaving")
- **State Icons**: Optional sprite icons for each state
- **Color System Integration**: Works with existing customer color system
- **World Space UI**: Always faces the camera and scales properly

### Configuration Options
- Enable/disable the entire system
- Toggle between text and/or icons
- Adjustable height and positioning
- Customizable scaling
- State-specific icon assignment

## Setup Instructions

### 1. Enable in Inspector
On any Customer prefab or instance:
1. Select the GameObject with the `CustomerVisuals` component
2. In the Inspector, find the **"State Indicator System"** section
3. Check **"Enable State Indicator"** to turn on the system
4. Configure the following settings:

```
✅ Enable State Indicator: true
📏 Indicator Height: 2.5 (height above customer)
📍 Indicator Offset: (0, 0, 0) (additional positioning)
✅ Show State Text: true (display text labels)
❌ Show State Icon: false (display sprite icons)
📏 Indicator Scale: 1.0 (overall size multiplier)
```

### 2. Optional: Add State Icons
If you want to use icons instead of or in addition to text:
1. Set **"Show State Icon"** to `true`
2. Assign sprites to the icon fields:
   - **Entering Icon**: Icon for customers entering the shop
   - **Shopping Icon**: Icon for customers browsing products  
   - **Purchasing Icon**: Icon for customers at checkout
   - **Leaving Icon**: Icon for customers exiting

### 3. Testing
Use the `CustomerStateIndicatorTest` script to test the system:
1. Attach the script to any Customer GameObject
2. Configure test settings in Inspector:
   - **Enable Auto Test**: Automatically cycles through states
   - **State Change Interval**: Time between automatic state changes
   - **Manual Test Key**: Press this key to manually cycle states
3. Run the scene and watch the indicators change!

## Integration

### Automatic Integration
The system integrates automatically with the Customer AI:
- ✅ **State Machine**: Updates when state machine transitions occur
- ✅ **Legacy Coroutines**: Updates when `CustomerBehavior.ChangeState()` is called
- ✅ **Color System**: Coordinates with existing customer color changes
- ✅ **Performance**: Minimal overhead, only updates when states change

### Manual Integration
You can also manually update indicators from custom code:
```csharp
// Get the customer visuals component
CustomerVisuals visuals = customer.GetComponent<CustomerVisuals>();

// Update the state display
visuals.UpdateStateDisplay(CustomerState.Shopping);
```

## Customization

### Text Appearance
The text uses Unity's built-in font by default. To customize:
1. Modify the `SetupStateIndicator()` method in `CustomerVisuals.cs`
2. Change the font, size, color, or alignment properties:
```csharp
stateText.font = yourCustomFont;
stateText.fontSize = 16;
stateText.color = Color.yellow;
stateText.alignment = TextAnchor.MiddleCenter;
```

### State Display Names
To change the displayed text for each state:
1. Modify the `GetStateDisplayName()` method in `CustomerVisuals.cs`
2. Update the return values:
```csharp
case CustomerState.Entering:
    return "Arriving"; // Instead of "Entering"
case CustomerState.Shopping:
    return "Browsing"; // Instead of "Shopping"
// etc...
```

### Positioning and Scaling
Adjust the visual positioning:
- **Indicator Height**: How high above the customer (in world units)
- **Indicator Offset**: Additional X/Y/Z positioning adjustment
- **Indicator Scale**: Overall size multiplier for the entire indicator

### Canvas Settings
The system uses a World Space Canvas for proper 3D integration:
- Always faces the camera
- Scales with distance
- Integrates properly with Unity's UI system
- Can be clicked/interacted with if needed

## Performance Considerations

### Optimization
- Indicators only update when customer state actually changes
- Uses object pooling pattern (creates once, reuses)
- Minimal draw calls (uses Unity UI batching)
- Automatic cleanup when customers are destroyed

### Best Practices
- Keep indicator count reasonable (< 50 active customers)
- Use simple, low-resolution icons if using sprites
- Consider disabling indicators in release builds if not needed
- Test performance with target customer counts

## Troubleshooting

### Indicators Not Showing
1. Check that **"Enable State Indicator"** is checked
2. Verify the **Indicator Height** isn't too low/high
3. Make sure the customer has a `CustomerVisuals` component
4. Check that the Main Camera is properly set

### Text Not Displaying
1. Ensure **"Show State Text"** is enabled
2. Check that Unity's built-in font loaded properly
3. Verify the text color isn't transparent or matching background

### Icons Not Showing
1. Confirm **"Show State Icon"** is enabled
2. Assign sprite assets to the icon fields in Inspector
3. Check that sprites are properly imported and readable

### Performance Issues
1. Reduce the number of active customers
2. Disable state indicators in builds (`enableStateIndicator = false`)
3. Use simpler, smaller icon sprites
4. Consider updating indicators less frequently

## Technical Details

### Components Used
- **Canvas**: World Space rendering
- **CanvasScaler**: Consistent sizing across distances
- **GraphicRaycaster**: UI interaction support
- **Text**: State name display
- **Image**: State icon display (optional)

### Files Modified
- `CustomerVisuals.cs`: Main implementation
- `CustomerBehavior.cs`: Integration with state changes
- `CustomerStateIndicatorTest.cs`: Testing utilities

### Dependencies
- Unity UI system (`UnityEngine.UI`)
- Customer AI components
- World Space Canvas rendering

## Future Enhancements

### Possible Improvements
- **Animation**: Smooth fade in/out when states change
- **Progress Bars**: Show shopping time remaining, queue position, etc.
- **Dynamic Sizing**: Scale based on camera distance
- **Custom Shaders**: More advanced visual effects
- **Localization**: Multi-language support for state names
- **Accessibility**: Audio cues for state changes

### Extension Points
The system is designed for easy extension:
- Add new visual indicator types by modifying `SetupStateIndicator()`
- Create custom state display logic in `GetStateDisplayName()`
- Integrate with other customer systems through `UpdateStateDisplay()`

## Example Usage

### Basic Setup
```csharp
// Enable the system on a customer
customer.Visuals.enableStateIndicator = true;
customer.Visuals.showStateText = true;
customer.Visuals.indicatorHeight = 3f;
```

### Advanced Configuration
```csharp
// Setup with custom icons and positioning
customer.Visuals.enableStateIndicator = true;
customer.Visuals.showStateText = false;
customer.Visuals.showStateIcon = true;
customer.Visuals.enteringIcon = myEnteringSprite;
customer.Visuals.shoppingIcon = myShoppingSprite;
customer.Visuals.indicatorHeight = 2f;
customer.Visuals.indicatorOffset = new Vector3(0, 0.5f, 0);
customer.Visuals.indicatorScale = 1.2f;
```

The Customer State Indicator System provides an intuitive way to visualize customer behavior and is essential for debugging, demonstrations, and user feedback in your tabletop shop simulation!
