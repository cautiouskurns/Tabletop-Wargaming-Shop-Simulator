# Using Textures/Sprites for Product Materials

## Overview
The Product GameObject System now supports both textures and solid colors for materials. This allows you to use actual product images to make your tabletop shop more realistic and visually appealing.

## How It Works

### Automatic Texture Detection
The `ProductPrefabCreator` now:
1. **Looks for texture files** in `Assets/Textures/` with specific names
2. **Applies textures** to materials if found
3. **Falls back to solid colors** if textures are missing
4. **Creates the Textures folder** automatically if it doesn't exist

### Expected Texture Names
Place your texture files in `Assets/Textures/` with these exact names:
- `MiniatureBoxTexture.png` - For miniature box products
- `PaintPotTexture.png` - For paint pot products  
- `RulebookTexture.png` - For rulebook products

## Supported Formats
Unity supports various image formats:
- **PNG** (recommended - supports transparency)
- **JPG/JPEG** (good for photos, smaller file size)
- **TGA** (high quality)
- **PSD** (Photoshop files)

## Usage Instructions

### Option 1: Use Default Solid Colors
1. Run `Tabletop Shop > Create Product Prefabs`
2. Prefabs will use colored materials (orange, blue, purple)

### Option 2: Add Custom Textures
1. Create or download product images
2. Place them in `Assets/Textures/` with the correct names:
   ```
   Assets/Textures/
   ├── MiniatureBoxTexture.png
   ├── PaintPotTexture.png
   └── RulebookTexture.png
   ```
3. Run `Tabletop Shop > Create Product Prefabs` again
4. The tool will detect and apply the textures automatically

## Texture Recommendations

### Miniature Box Texture
- **Suggested Image:** Cardboard box with miniature artwork
- **Colors:** Browns, oranges, with colorful artwork
- **Style:** Product packaging, box art

### Paint Pot Texture  
- **Suggested Image:** Cylindrical paint container
- **Colors:** Bright colors showing paint type
- **Style:** Acrylic paint pot label/branding

### Rulebook Texture
- **Suggested Image:** Book cover with game artwork
- **Colors:** Dark backgrounds with dramatic artwork
- **Style:** Fantasy/sci-fi book cover design

## Texture Size Guidelines
- **Resolution:** 256x256 or 512x512 pixels (power of 2)
- **Aspect Ratio:** Square works best for all shapes
- **File Size:** Keep under 1MB each for good performance

## Example Workflow

### Using Free Placeholder Images
1. Download placeholder textures from sites like:
   - Unsplash.com (free photos)
   - Pixabay.com (free images)
   - OpenGameArt.org (game assets)

2. Resize to 256x256 pixels in image editor

3. Save with correct names in `Assets/Textures/`

4. Run the prefab creator tool

### Creating Custom Textures
1. Take photos of real tabletop products
2. Edit in Photoshop/GIMP to fit square format
3. Add text overlays or branding if desired
4. Export as PNG with transparency if needed

## Material Properties
The system maintains these material settings regardless of texture:

**MiniatureBox Material:**
- Metallic: 0.1 (slightly reflective)
- Smoothness: 0.3 (somewhat rough)

**PaintPot Material:**
- Metallic: 0.3 (more reflective like plastic)  
- Smoothness: 0.7 (smooth surface)

**Rulebook Material:**
- Metallic: 0.0 (no reflection like paper)
- Smoothness: 0.2 (matte finish)

## Updating Textures
To change textures after prefabs are created:
1. Replace the texture files in `Assets/Textures/`
2. Run `Tabletop Shop > Create Product Prefabs` again
3. The materials will update automatically

## Integration with ProductData
The textures work alongside the ProductData ScriptableObject system:
- **ProductData** contains game data (name, price, description)
- **Prefab materials** contain visual appearance (textures, colors)
- Both systems work together but remain separate for flexibility

## Troubleshooting

### Textures Not Appearing
1. Check file names match exactly (case-sensitive)
2. Ensure files are in `Assets/Textures/` folder
3. Verify Unity has imported the images (check Inspector)
4. Re-run the prefab creator tool

### Textures Look Wrong
1. Check aspect ratio (square works best)
2. Verify import settings in Unity Inspector
3. Try different image formats (PNG recommended)

### Performance Issues
1. Reduce texture resolution (256x256 is usually sufficient)
2. Use JPG format for smaller file sizes
3. Avoid very large image files

## Advanced Customization
For more control over materials, you can:
1. Manually edit the created materials in Unity
2. Assign different shaders (Unlit, Standard, etc.)
3. Adjust texture tiling and offset
4. Add normal maps or other texture types

This texture system provides a great balance between ease of use and visual customization for your tabletop shop simulator!
