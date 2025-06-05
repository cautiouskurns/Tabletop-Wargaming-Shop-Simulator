using UnityEngine;
using UnityEditor;
using System.IO;

namespace TabletopShop.Editor
{
    /// <summary>
    /// Editor script to create product prefabs with proper setup
    /// </summary>
    public class ProductPrefabCreator : EditorWindow
    {
        [MenuItem("Tabletop Shop/Create Product Prefabs")]
        public static void CreateProductPrefabs()
        {
            // Ensure directories exist
            string prefabPath = "Assets/Prefabs";
            string materialsPath = "Assets/Materials";
            string texturesPath = "Assets/Textures";
            
            if (!AssetDatabase.IsValidFolder(prefabPath))
                AssetDatabase.CreateFolder("Assets", "Prefabs");
            
            if (!AssetDatabase.IsValidFolder(materialsPath))
                AssetDatabase.CreateFolder("Assets", "Materials");
                
            if (!AssetDatabase.IsValidFolder(texturesPath))
            {
                AssetDatabase.CreateFolder("Assets", "Textures");
                Debug.Log("Created Assets/Textures folder. Add your product texture files here!");
            }
            
            // Create materials first
            CreateMaterials();
            
            // Create the three product prefabs
            CreateMiniatureBoxPrefab();
            CreatePaintPotPrefab();
            CreateRulebookPrefab();
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            Debug.Log("Product prefabs created successfully!");
            Debug.Log("Materials configured for URP with 2D Sprite settings.");
            Debug.Log("To use custom textures, place PNG files in Assets/Textures/ with these names:");
            Debug.Log("- MiniatureBoxTexture.png");
            Debug.Log("- PaintPotTexture.png"); 
            Debug.Log("- RulebookTexture.png");
            Debug.Log("Textures will be automatically configured as Sprites with proper import settings.");
            Debug.Log("Then run this tool again to apply the textures.");
        }
        
        private static void CreateMaterials()
        {
            // Try to load textures first, fallback to solid colors if not found
            Texture2D miniBoxTexture = LoadAndConfigureTexture("Assets/Textures/MiniatureBoxTexture.png");
            Texture2D paintPotTexture = LoadAndConfigureTexture("Assets/Textures/PaintPotTexture.png");
            Texture2D rulebookTexture = LoadAndConfigureTexture("Assets/Textures/RulebookTexture.png");
            
            // Get URP shader (fallback to Standard if URP not available)
            Shader urpShader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            if (urpShader.name.Contains("Universal"))
            {
                Debug.Log("Using URP Lit shader for materials");
            }
            else
            {
                Debug.Log("URP not detected, using Standard shader");
            }
            
            // Miniature Box Material
            Material miniBoxMat = new Material(urpShader);
            if (miniBoxTexture != null)
            {
                miniBoxMat.mainTexture = miniBoxTexture;
                Debug.Log("Applied texture to MiniatureBox material");
            }
            else
            {
                miniBoxMat.color = new Color(0.8f, 0.4f, 0.2f, 1f); // Fallback color
                Debug.Log("No texture found for MiniatureBox, using solid color");
            }
            SetMaterialProperties(miniBoxMat, 0.1f, 0.3f);
            AssetDatabase.CreateAsset(miniBoxMat, "Assets/Materials/MiniatureBoxMaterial.mat");
            
            // Paint Pot Material
            Material paintMat = new Material(urpShader);
            if (paintPotTexture != null)
            {
                paintMat.mainTexture = paintPotTexture;
                Debug.Log("Applied texture to PaintPot material");
            }
            else
            {
                paintMat.color = new Color(0.2f, 0.6f, 0.8f, 1f); // Fallback color
                Debug.Log("No texture found for PaintPot, using solid color");
            }
            SetMaterialProperties(paintMat, 0.3f, 0.7f);
            AssetDatabase.CreateAsset(paintMat, "Assets/Materials/PaintPotMaterial.mat");
            
            // Rulebook Material
            Material bookMat = new Material(urpShader);
            if (rulebookTexture != null)
            {
                bookMat.mainTexture = rulebookTexture;
                Debug.Log("Applied texture to Rulebook material");
            }
            else
            {
                bookMat.color = new Color(0.6f, 0.2f, 0.4f, 1f); // Fallback color
                Debug.Log("No texture found for Rulebook, using solid color");
            }
            SetMaterialProperties(bookMat, 0f, 0.2f);
            AssetDatabase.CreateAsset(bookMat, "Assets/Materials/RulebookMaterial.mat");
        }
        
        private static Texture2D LoadAndConfigureTexture(string path)
        {
            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            
            if (texture != null)
            {
                // Get the texture importer to configure settings
                TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer != null)
                {
                    bool needsReimport = false;
                    
                    // Set texture type to Sprite (2D and UI)
                    if (importer.textureType != TextureImporterType.Sprite)
                    {
                        importer.textureType = TextureImporterType.Sprite;
                        needsReimport = true;
                    }
                    
                    // Enable Read/Write for runtime material creation
                    if (!importer.isReadable)
                    {
                        importer.isReadable = true;
                        needsReimport = true;
                    }
                    
                    // Set sprite mode to Single
                    if (importer.spriteImportMode != SpriteImportMode.Single)
                    {
                        importer.spriteImportMode = SpriteImportMode.Single;
                        needsReimport = true;
                    }
                    
                    // Set filter mode for crisp sprites
                    if (importer.filterMode != FilterMode.Bilinear)
                    {
                        importer.filterMode = FilterMode.Bilinear;
                        needsReimport = true;
                    }
                    
                    // Apply changes if needed
                    if (needsReimport)
                    {
                        importer.SaveAndReimport();
                        Debug.Log($"Configured texture import settings for {path}");
                    }
                }
            }
            
            return texture;
        }
        
        private static void SetMaterialProperties(Material material, float metallic, float smoothness)
        {
            // Set properties based on shader type
            if (material.shader.name.Contains("Universal"))
            {
                // URP properties
                if (material.HasProperty("_Metallic"))
                    material.SetFloat("_Metallic", metallic);
                if (material.HasProperty("_Smoothness"))
                    material.SetFloat("_Smoothness", smoothness);
            }
            else
            {
                // Built-in render pipeline properties
                if (material.HasProperty("_Metallic"))
                    material.SetFloat("_Metallic", metallic);
                if (material.HasProperty("_Glossiness"))
                    material.SetFloat("_Glossiness", smoothness);
            }
        }
        
        private static void CreateMiniatureBoxPrefab()
        {
            // Create cube GameObject
            GameObject miniBox = GameObject.CreatePrimitive(PrimitiveType.Cube);
            miniBox.name = "MiniatureBox_Prefab";
            
            // Scale it to look like a miniature box
            miniBox.transform.localScale = new Vector3(1f, 0.6f, 1f);
            
            // Add Product component
            Product productScript = miniBox.AddComponent<Product>();
            
            // Apply material
            Material material = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/MiniatureBoxMaterial.mat");
            if (material != null)
            {
                miniBox.GetComponent<MeshRenderer>().material = material;
            }
            
            // Save as prefab
            string prefabPath = "Assets/Prefabs/MiniatureBox_Prefab.prefab";
            PrefabUtility.SaveAsPrefabAsset(miniBox, prefabPath);
            
            // Clean up scene
            DestroyImmediate(miniBox);
            
            Debug.Log($"Created {prefabPath}");
        }
        
        private static void CreatePaintPotPrefab()
        {
            // Create cylinder GameObject
            GameObject paintPot = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            paintPot.name = "PaintPot_Prefab";
            
            // Scale it to look like a paint pot
            paintPot.transform.localScale = new Vector3(0.4f, 0.6f, 0.4f);
            
            // Add Product component
            Product productScript = paintPot.AddComponent<Product>();
            
            // Apply material
            Material material = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/PaintPotMaterial.mat");
            if (material != null)
            {
                paintPot.GetComponent<MeshRenderer>().material = material;
            }
            
            // Save as prefab
            string prefabPath = "Assets/Prefabs/PaintPot_Prefab.prefab";
            PrefabUtility.SaveAsPrefabAsset(paintPot, prefabPath);
            
            // Clean up scene
            DestroyImmediate(paintPot);
            
            Debug.Log($"Created {prefabPath}");
        }
        
        private static void CreateRulebookPrefab()
        {
            // Create cube GameObject (thin for book)
            GameObject rulebook = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rulebook.name = "Rulebook_Prefab";
            
            // Scale it to look like a book
            rulebook.transform.localScale = new Vector3(0.7f, 1.2f, 0.1f);
            
            // Add Product component
            Product productScript = rulebook.AddComponent<Product>();
            
            // Apply material
            Material material = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/RulebookMaterial.mat");
            if (material != null)
            {
                rulebook.GetComponent<MeshRenderer>().material = material;
            }
            
            // Save as prefab
            string prefabPath = "Assets/Prefabs/Rulebook_Prefab.prefab";
            PrefabUtility.SaveAsPrefabAsset(rulebook, prefabPath);
            
            // Clean up scene
            DestroyImmediate(rulebook);
            
            Debug.Log($"Created {prefabPath}");
        }
    }
}
