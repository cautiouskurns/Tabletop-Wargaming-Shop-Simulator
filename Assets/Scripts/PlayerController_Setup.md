# PlayerController Setup Instructions

## Overview
The PlayerController.cs script provides a complete first-person controller for the tabletop shop simulator game. It includes WASD movement, mouse look, jumping, and ground detection.

## Features Implemented
✅ **WASD Movement** - Smooth character movement in all directions
✅ **Mouse Look** - Both horizontal (Y-axis) and vertical (X-axis) camera rotation
✅ **Configurable Speed** - Default movement speed of 5 units/second
✅ **Mouse Sensitivity** - Default sensitivity of 2.0 (adjustable in inspector)
✅ **Ground Check** - Physics-based ground detection to prevent flying
✅ **Jump Functionality** - Space key to jump (only when grounded)
✅ **Camera Integration** - Camera attached to player head position

## Setup Instructions

### 1. Player GameObject Setup
The scene already contains a basic Player setup:
- **Player** (Capsule primitive with CharacterController)
- **Player Camera** (Child object positioned at head level)

### 2. Adding the PlayerController Script
1. Select the Player GameObject in the hierarchy
2. In the Inspector, click "Add Component"
3. Search for "PlayerController" and add it
4. The script will automatically:
   - Find the CharacterController component
   - Set up the camera reference
   - Create a ground check point

### 3. Configuring the Controller
In the Inspector, you can adjust:
- **Movement Speed**: How fast the player moves (default: 5)
- **Jump Height**: How high the player jumps (default: 2)
- **Mouse Sensitivity**: Camera rotation speed (default: 2)
- **Vertical Look Limit**: Max up/down look angle (default: 80°)
- **Ground Check Settings**: For fine-tuning ground detection

### 4. Input Controls
- **WASD**: Movement
- **Mouse**: Look around
- **Space**: Jump (only when grounded)
- **Escape**: Toggle cursor lock/unlock

## Technical Details

### Required Components
- CharacterController (automatically required)
- Camera (will be created if missing)

### Physics Setup
- Uses CharacterController for movement (no Rigidbody needed)
- Ground detection using Physics.CheckSphere
- Gravity applied manually for realistic jumping

### Namespace
The script uses the `TabletopShop` namespace to organize code properly.

## Next Steps
1. Add the PlayerController script to your Player GameObject
2. Test movement and camera controls
3. Adjust settings in the Inspector as needed
4. Add interaction system for shop management

## Debugging
- Ground check visualization available in Scene view (green = grounded, red = airborne)
- Public methods available for external scripts to query player state
