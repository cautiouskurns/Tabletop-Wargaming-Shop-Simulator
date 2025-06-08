# Unity AI Prompt - Price Setting Modal UI

## 🎯 **Copy-Paste This Prompt to Unity AI:**

```
Create a Unity UI price setting modal popup that replicates a card game pricing interface. I need the following UI structure:

DELIVERABLE: Complete Unity UI hierarchy for price setting modal popup
CONTEXT: Product pricing interface for tabletop shop simulator
- Modal popup overlay that blocks game interaction
- Card-based layout showing product image and pricing details
- Professional pricing interface with profit calculation
- Input field for price modification with real-time profit display

REQUIREMENTS:
1. Create full-screen modal overlay panel (semi-transparent dark background)
2. Create centered content panel (450x350 size, rounded corners if possible)
3. Add "Set Price" title at top (white text, bold, large font)
4. Create left side: product image area (150x200, placeholder or card image)
5. Create right side: pricing information panel with:
   - "Avg Cost" label and value display
   - "Price" label with editable input field ($49.99 format)
   - "Market Price" label and blue reference value ($112.93)
   - "Profit" label with calculated green value ($49.99)
6. Add large "Done" button at bottom (orange/yellow gradient, rounded)
7. Use card game aesthetic with dark backgrounds and bright text

UI HIERARCHY STRUCTURE:
- PriceSettingModal (full screen panel)
  - PriceContentPanel (centered main panel)
    - SetPriceTitle (header text)
    - ProductImageArea (left side image placeholder)
    - PricingInfoPanel (right side container)
      - AvgCostRow (label + value)
      - PriceRow (label + input field)
      - MarketPriceRow (label + blue value)
      - ProfitRow (label + green value)
    - DoneButton (bottom action button)

VISUAL STYLING:
- Color scheme: Dark grays/blues with bright accent colors
- Text colors: White labels, blue for market price, green for profit
- Button: Orange/yellow gradient with white text
- Input field: Light gray background with dark text
- Overall aesthetic: Modern card game UI style

TECHNICAL SPECS:
- Use TextMeshPro for all text elements
- Use TMP_InputField for price input (decimal number validation)
- Use Button component for Done action
- Panel components with Image backgrounds
- Proper anchoring for responsive design
- Start with modal disabled (SetActive false)

LAYOUT POSITIONING:
- Modal: Full screen stretch
- Content panel: Center anchored, 450x350 size
- Product image: Left side, 150x200
- Pricing info: Right side, organized vertically
- Done button: Bottom center, prominent size
- All elements properly spaced for clean layout
```

## 🎨 **Additional Styling Notes for Unity AI:**

Add this follow-up if you want more specific styling:

```
ADDITIONAL STYLING REQUIREMENTS:
- Product image area: Dark frame border, aspect ratio maintained
- Pricing rows: Consistent spacing (30-35px between each row)
- Input field: White/light gray background, dark text, currency formatting
- Profit calculation: Updates automatically when price changes
- Typography: Clean, readable fonts with proper size hierarchy
- Color palette: 
  * Background: Dark blue-gray (#2d3748)
  * Text: White (#ffffff) 
  * Accent blue: (#4299e1) for market price
  * Accent green: (#48bb78) for profit
  * Button: Orange-yellow gradient (#ed8936 to #f6e05e)
```

## 🔧 **Expected Unity AI Output:**

Unity AI should create:
1. **Complete UI hierarchy** matching the visual layout
2. **Properly configured components** (TextMeshPro, InputField, Button)
3. **Anchored positioning** for responsive design
4. **Color-coded elements** matching the card game aesthetic
5. **Ready-to-use script references** for the pricing logic

## 📝 **After Unity AI Creates the UI:**

You'll need to:
1. **Assign script references** to the ShopUI.cs component
2. **Add the pricing calculation logic** from Sub-Task 5
3. **Connect button events** to the appropriate methods
4. **Test the modal show/hide** functionality

This prompt should give you a UI that closely matches the card game aesthetic shown in your image!