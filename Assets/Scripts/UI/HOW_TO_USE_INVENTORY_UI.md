# How to Use the InventoryUI Script

## Step 1: Create the UI Hierarchy in Unity

1. **Create the main Canvas:**
   - Right-click in Hierarchy → UI → Canvas
   - Name it "InventoryCanvas"
   - Set Render Mode to "Screen Space - Overlay"

2. **Create the Inventory Panel:**
   - Right-click on InventoryCanvas → Create Empty
   - Name it "InventoryPanel"
   - Add a `CanvasGroup` component to it
   - Set the CanvasGroup Alpha to 0 (will start hidden)

3. **Create Product Buttons:**
   - Right-click on InventoryPanel → UI → Button - TextMeshPro
   - Create 3 buttons for the 3 product types
   - Name them: "MiniatureBoxButton", "PaintPotButton", "RulebookButton"

## Step 2: Set Up the InventoryUI Component

1. **Add the InventoryUI script:**
   - Select the InventoryCanvas GameObject
   - Click "Add Component"
   - Search for "InventoryUI" and add it

2. **Assign the references in the Inspector:**
   - **Inventory Canvas Group:** Drag the InventoryPanel (with CanvasGroup) here
   - **Product Buttons:** Set Size to 3, then drag your 3 buttons into the array slots

3. **Configure Animation Settings:**
   - **Fade In Duration:** 0.3 (default is good)
   - **Fade Out Duration:** 0.2 (default is good)

4. **Configure Visual Feedback:**
   - **Selected Button Color:** Yellow (or your preferred highlight color)
   - **Default Button Color:** White (or your preferred normal color)

## Step 3: Set Up the InventoryManager

1. **Create an InventoryManager GameObject:**
   - Right-click in Hierarchy → Create Empty
   - Name it "InventoryManager"
   - Add the `InventoryManager` component

2. **Assign ProductData assets:**
   - In the InventoryManager Inspector, find "Available Products"
   - Drag your ProductData ScriptableObjects from the Project window
   - Make sure you have at least one ProductData for each ProductType

## Step 4: Test the System

### In Play Mode:
- Press **Tab** to toggle the inventory panel
- Click product buttons to select different product types
- The selected button will highlight in yellow
- Button counts will update automatically when inventory changes

### Expected Behavior:
1. **Panel starts hidden** when the game begins
2. **Tab key toggles** the inventory panel with smooth fade animation
3. **Product buttons show counts** of available items
4. **Selected product highlights** with the specified color
5. **Real-time updates** when inventory changes

## Step 5: Customization Options

### Button Layout:
You can customize the button appearance by:
- Adding icons to the buttons (add Image component as child)
- Changing text labels to show product names
- Adjusting button sizes and positions

### Animation Tweaks:
- Adjust `fadeInDuration` and `fadeOutDuration` for different animation speeds
- The animations use smooth linear interpolation

### Visual Feedback:
- Change `selectedButtonColor` to match your game's theme
- Modify `defaultButtonColor` for normal button appearance

## Common Issues and Solutions

### Issue: "Panel doesn't appear when pressing Tab"
**Solution:** Check that:
- InventoryCanvasGroup is assigned
- The Canvas is set to Screen Space - Overlay
- The InventoryUI script is on an active GameObject

### Issue: "Buttons don't show counts"
**Solution:** Ensure:
- Product buttons have TextMeshPro components as children
- InventoryManager has ProductData assets assigned
- ProductData assets have the correct ProductType set

### Issue: "Button clicks don't work"
**Solution:** Verify:
- All 3 buttons are assigned in the Product Buttons array
- Buttons have the Button component
- InventoryManager is properly initialized

### Issue: "No products available"
**Solution:** 
- Check that ProductData ScriptableObjects exist in your project
- Ensure they're assigned to the InventoryManager's Available Products list
- Verify the InventoryManager is calling InitializeStartingInventory()

## Integration with Other Systems

The InventoryUI automatically connects to the InventoryManager singleton and:
- Subscribes to inventory change events
- Updates button states when products are added/removed
- Handles product selection through button clicks
- Maintains visual feedback for the current selection

No additional code is required - just set up the UI hierarchy and assign the references!
