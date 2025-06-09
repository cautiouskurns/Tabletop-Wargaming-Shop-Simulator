# Customer Character Setup Guide

## Overview
This guide will help you replace the basic capsule customer mesh with Kevin Iglesias Human Character Dummy assets and set up animation rigging for your customers.

## 📋 Prerequisites
- Unity project with the existing Customer system (✅ Available)
- Kevin Iglesias Human Character Dummy assets (✅ Available)
- Basic understanding of Unity Prefabs and Animation

## 🎯 What You'll Achieve
- Replace capsule mesh with human character models
- Set up proper collision for NavMesh navigation
- Add animation controller for walking/idle states
- Maintain existing customer AI functionality
- Keep the visual color system working

## 🛠️ Step-by-Step Setup

### Step 1: Use the Customer Character Setup Tool

1. **Open the Setup Window**
   - In Unity menu: `TabletopShop > Customer Character Setup`
   - This opens a custom editor window

2. **Configure Your Character**
   - **Gender**: Choose Male or Female
   - **Color**: Select from White, Blue, Green, Red, Orange, Purple, Yellow
   - **Setup Animator**: ✅ (Recommended for animations)
   - **Keep Capsule Collider**: ✅ (Required for NavMesh)
   - **Add Character Controller**: Optional (better physics)

3. **Choose Setup Method**
   - **"Create New Customer"**: Makes a new prefab alongside existing one
   - **"Update Existing"**: Modifies your current Customer.prefab

### Step 2: Manual Setup (Alternative Method)

If you prefer manual setup:

1. **Open Customer.prefab in Prefab Mode**
   - Navigate to `Assets/Prefabs/AI/Customer.prefab`
   - Double-click to enter Prefab Mode

2. **Remove Capsule Mesh**
   - Select the Customer root object
   - Remove `MeshFilter` and `MeshRenderer` components
   - Keep `CapsuleCollider` and `NavMeshAgent`

3. **Add Human Character**
   - Drag desired human character prefab from `Assets/Kevin Iglesias/Human Character Dummy/Prefabs/`
   - Make it a child of the Customer root
   - Reset its transform (Position: 0,0,0 | Rotation: 0,0,0 | Scale: 1,1,1)
   - Rename to "CharacterMesh"

4. **Setup Colliders**
   - Adjust CapsuleCollider: Height: 1.8, Radius: 0.4, Center: (0, 0.9, 0)
   - Remove any colliders from the character mesh child object

### Step 3: Animation Setup

1. **Add Animation Controller Component**
   - Add `CustomerAnimationController` script to your Customer prefab
   - This handles automatic walking animations based on movement

2. **Create Basic Animator Controller** (Optional Advanced Setup)
   - Right-click in Project > Create > Animator Controller
   - Name it "CustomerAnimator"
   - Add states: Idle, Walk
   - Add parameters: WalkSpeed (Float), IsWalking (Bool)
   - Assign to Animator component

### Step 4: Test Your Setup

1. **Place Customer in Scene**
   - Drag your updated Customer prefab into the scene
   - Make sure it's positioned on the NavMesh

2. **Verify Components**
   - Customer script ✅
   - CustomerMovement script ✅  
   - CustomerBehavior script ✅
   - CustomerVisuals script ✅ (with color system)
   - NavMeshAgent ✅
   - CapsuleCollider ✅
   - Animator ✅ (if setup)
   - CustomerAnimationController ✅ (if added)

3. **Test Movement**
   - Play the scene
   - Customer should spawn and move with human character model
   - Verify NavMesh navigation works
   - Check that color system still functions

## 🎨 Visual Customization

### Character Variations
You can create multiple customer variants:
- Different genders (Male/Female)
- Different colors (8 color options available)
- Mix and match for variety

### Color System Integration
The existing color system will still work:
- State colors override character material colors
- Entering: Cyan
- Shopping: Green  
- Purchasing: Orange
- Leaving: Magenta

### Material Modifications
The CustomerVisuals system automatically:
- Finds MeshRenderer on character mesh
- Creates material copies
- Applies state-based colors
- Handles smooth transitions

## 🔧 Advanced Animation Setup

### Basic Animation States
For simple customer movement, you only need:
- **Idle**: Standing still
- **Walk**: Moving to destination

### Animation Parameters
Recommended animator parameters:
- `WalkSpeed` (Float): Controls walk animation speed
- `IsWalking` (Bool): Switches between idle/walk
- `IsBrowsing` (Bool): Optional browsing pose when shopping

### Custom Animations
To add more animations:
1. Import animation clips
2. Add states to Animator Controller
3. Create transitions with appropriate conditions
4. Use CustomerAnimationController.TriggerAnimation() in code

## 🐛 Troubleshooting

### Common Issues

**Customer not moving:**
- Check NavMeshAgent is enabled
- Verify NavMesh exists in scene
- Ensure CapsuleCollider height/radius is appropriate

**Animation not working:**
- Verify Animator component has controller assigned
- Check animation parameter names match
- Ensure character has proper bone structure

**Color system not working:**
- Verify CustomerVisuals component exists
- Check that character mesh has MeshRenderer
- Ensure enableColorSystem is true

**Performance issues:**
- Limit number of customers simultaneously 
- Use simple materials for customer characters
- Consider LOD system for distant customers

### Component Dependencies
Your customer hierarchy should look like:
```
Customer (Root)
├── Customer.cs
├── CustomerMovement.cs  
├── CustomerBehavior.cs
├── CustomerVisuals.cs
├── CustomerAnimationController.cs
├── NavMeshAgent
├── CapsuleCollider
├── Animator
└── CharacterMesh (Child)
    ├── MeshRenderer
    ├── MeshFilter
    └── (Character bones/skeleton)
```

## 🚀 Next Steps

After basic setup:
1. **Create Multiple Variants**: Make several customer prefabs with different characters
2. **Add Animation Variety**: Import additional animation clips for more behaviors  
3. **Implement Animation Events**: Sync animations with customer state changes
4. **Optimize Performance**: Use object pooling for customer spawning
5. **Add Accessories**: Attach props or clothing variations

## 📚 Resources

- **Kevin Iglesias Asset Documentation**: Check the Human Character Dummy documentation
- **Unity NavMesh Guide**: For navigation troubleshooting
- **Unity Animation System**: For advanced animation setup
- **Customer AI System**: See existing Customer.cs, CustomerBehavior.cs documentation

---

**Note**: This setup maintains full compatibility with your existing customer AI system while adding visual fidelity and animation support.
