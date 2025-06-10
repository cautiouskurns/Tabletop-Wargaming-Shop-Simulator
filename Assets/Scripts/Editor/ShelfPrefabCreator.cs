using UnityEngine;
using UnityEditor;
using System.IO;

namespace TabletopShop.Editor
{
    /// <summary>
    /// Editor script to create shelf prefabs with proper setup
    /// </summary>
    public class ShelfPrefabCreator : EditorWindow
    {
        [MenuItem("Tabletop Shop/Create Shelf Prefab")]
        public static void CreateShelfPrefab()
        {
            // Ensure directories exist
            string prefabPath = "Assets/Prefabs";
            string materialsPath = "Assets/Materials";
            
            if (!AssetDatabase.IsValidFolder(prefabPath))
                AssetDatabase.CreateFolder("Assets", "Prefabs");
            
            if (!AssetDatabase.IsValidFolder(materialsPath))
                AssetDatabase.CreateFolder("Assets", "Materials");
            
            // Create shelf material if it doesn't exist
            CreateShelfMaterial();
            
            // Create the shelf prefab
            CreateBasicShelfPrefab();
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            Debug.Log("Shelf prefab created successfully!");
            Debug.Log("Prefab: Assets/Prefabs/BasicShelf_Prefab.prefab");
            Debug.Log("You can now drag this prefab into your scene to create shelves.");
        }
        
        private static void CreateShelfMaterial()
        {
            string materialPath = "Assets/Materials/ShelfMaterial.mat";
            
            // Check if material already exists
            Material existingMaterial = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if (existingMaterial != null)
            {
                Debug.Log("Shelf material already exists, skipping creation.");
                return;
            }
            
            // Get URP shader (fallback to Standard if URP not available)
            Shader urpShader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            
            // Create shelf material using MaterialUtility
            Material shelfMaterial = MaterialUtility.CreateStandardMaterial(
                new Color(0.6f, 0.4f, 0.2f, 1f), // Brown wood color
                0f, // metallic
                0.4f // smoothness
            );
            
            // Override shader if URP is available
            if (urpShader.name.Contains("Universal"))
            {
                shelfMaterial.shader = urpShader;
            }
            
            AssetDatabase.CreateAsset(shelfMaterial, materialPath);
            Debug.Log($"Created shelf material: {materialPath}");
        }
        
        private static void CreateBasicShelfPrefab()
        {
            // Create main shelf GameObject
            GameObject shelf = new GameObject("BasicShelf");
            shelf.transform.position = Vector3.zero;
            shelf.transform.rotation = Quaternion.identity;
            
            // Add Shelf component
            Shelf shelfScript = shelf.AddComponent<Shelf>();
            
            // Load shelf material
            Material shelfMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/ShelfMaterial.mat");
            
            // Configure shelf settings using reflection to access private fields
            var shelfScriptType = typeof(Shelf);
            
            // Set shelf material through serialized field
            if (shelfMaterial != null)
            {
                var materialField = shelfScriptType.GetField("shelfMaterial", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                materialField?.SetValue(shelfScript, shelfMaterial);
            }
            
            // Configure other settings
            var maxSlotsField = shelfScriptType.GetField("maxSlots",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            maxSlotsField?.SetValue(shelfScript, 5);
            
            var slotSpacingField = shelfScriptType.GetField("slotSpacing",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            slotSpacingField?.SetValue(shelfScript, 1.5f);
            
            var autoCreateSlotsField = shelfScriptType.GetField("autoCreateSlots",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            autoCreateSlotsField?.SetValue(shelfScript, true);
            
            var allowAnyProductTypeField = shelfScriptType.GetField("allowAnyProductType",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            allowAnyProductTypeField?.SetValue(shelfScript, true);
            
            // Force the shelf to initialize (simulate Awake)
            var awakeMethod = shelfScriptType.GetMethod("Awake", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            awakeMethod?.Invoke(shelfScript, null);
            
            // Save as prefab
            string prefabPath = "Assets/Prefabs/BasicShelf_Prefab.prefab";
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(shelf, prefabPath);
            
            // Clean up scene
            DestroyImmediate(shelf);
            
            Debug.Log($"Created shelf prefab: {prefabPath}");
            Debug.Log("Shelf has 5 slots, allows any product type, and auto-creates visual elements.");
            
            // Select the created prefab in the project window
            Selection.activeObject = prefab;
            EditorGUIUtility.PingObject(prefab);
        }
        
        [MenuItem("Tabletop Shop/Create Specialized Shelf Prefabs")]
        public static void CreateSpecializedShelfPrefabs()
        {
            CreateShelfMaterial(); // Ensure material exists
            
            // Create specialized shelf prefabs for each product type
            CreateSpecializedShelf("MiniatureShelf", ProductType.MiniatureBox, new Color(0.8f, 0.4f, 0.2f));
            CreateSpecializedShelf("PaintShelf", ProductType.PaintPot, new Color(0.2f, 0.6f, 0.8f));
            CreateSpecializedShelf("BookShelf", ProductType.Rulebook, new Color(0.6f, 0.2f, 0.4f));
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            Debug.Log("Specialized shelf prefabs created!");
            Debug.Log("- MiniatureShelf_Prefab.prefab (Miniature boxes only)");
            Debug.Log("- PaintShelf_Prefab.prefab (Paint pots only)");
            Debug.Log("- BookShelf_Prefab.prefab (Rulebooks only)");
        }
        
        private static void CreateSpecializedShelf(string shelfName, ProductType allowedType, Color accentColor)
        {
            // Create main shelf GameObject
            GameObject shelf = new GameObject(shelfName);
            shelf.transform.position = Vector3.zero;
            shelf.transform.rotation = Quaternion.identity;
            
            // Add Shelf component
            Shelf shelfScript = shelf.AddComponent<Shelf>();
            
            // Create specialized material
            Shader urpShader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            Material specializedMaterial = new Material(urpShader);
            
            // Mix the base brown with accent color
            Color baseColor = new Color(0.6f, 0.4f, 0.2f, 1f);
            specializedMaterial.color = Color.Lerp(baseColor, accentColor, 0.3f);
            
            // Set material properties
            if (urpShader.name.Contains("Universal"))
            {
                if (specializedMaterial.HasProperty("_Metallic"))
                    specializedMaterial.SetFloat("_Metallic", 0f);
                if (specializedMaterial.HasProperty("_Smoothness"))
                    specializedMaterial.SetFloat("_Smoothness", 0.4f);
            }
            else
            {
                if (specializedMaterial.HasProperty("_Metallic"))
                    specializedMaterial.SetFloat("_Metallic", 0f);
                if (specializedMaterial.HasProperty("_Glossiness"))
                    specializedMaterial.SetFloat("_Glossiness", 0.4f);
            }
            
            // Save specialized material
            string materialPath = $"Assets/Materials/{shelfName}Material.mat";
            AssetDatabase.CreateAsset(specializedMaterial, materialPath);
            
            // Configure shelf settings using reflection
            var shelfScriptType = typeof(Shelf);
            
            var materialField = shelfScriptType.GetField("shelfMaterial", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            materialField?.SetValue(shelfScript, specializedMaterial);
            
            var maxSlotsField = shelfScriptType.GetField("maxSlots",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            maxSlotsField?.SetValue(shelfScript, 5);
            
            var slotSpacingField = shelfScriptType.GetField("slotSpacing",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            slotSpacingField?.SetValue(shelfScript, 1.5f);
            
            var autoCreateSlotsField = shelfScriptType.GetField("autoCreateSlots",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            autoCreateSlotsField?.SetValue(shelfScript, true);
            
            var allowAnyProductTypeField = shelfScriptType.GetField("allowAnyProductType",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            allowAnyProductTypeField?.SetValue(shelfScript, false);
            
            var allowedProductTypeField = shelfScriptType.GetField("allowedProductType",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            allowedProductTypeField?.SetValue(shelfScript, allowedType);
            
            // Force the shelf to initialize
            var awakeMethod = shelfScriptType.GetMethod("Awake", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            awakeMethod?.Invoke(shelfScript, null);
            
            // Save as prefab
            string prefabPath = $"Assets/Prefabs/{shelfName}_Prefab.prefab";
            PrefabUtility.SaveAsPrefabAsset(shelf, prefabPath);
            
            // Clean up scene
            DestroyImmediate(shelf);
            
            Debug.Log($"Created specialized shelf: {prefabPath} (allows {allowedType} only)");
        }
    }
}
