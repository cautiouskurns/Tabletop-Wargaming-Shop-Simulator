using UnityEngine;
using UnityEditor;

namespace TabletopShop.Editor
{
    public class ProductDataCreator
    {
        [MenuItem("Tabletop Shop/Create Default Products")]
        public static void CreateDefaultProducts()
        {
            // Create Iron Legion Starter
            ProductData ironLegion = ScriptableObject.CreateInstance<ProductData>();
            
            // Use reflection to set private fields for demonstration
            var productNameField = typeof(ProductData).GetField("productName", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var basePriceField = typeof(ProductData).GetField("basePrice", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var typeField = typeof(ProductData).GetField("type", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var descriptionField = typeof(ProductData).GetField("description", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            productNameField?.SetValue(ironLegion, "Iron Legion Starter");
            basePriceField?.SetValue(ironLegion, 45);
            typeField?.SetValue(ironLegion, ProductType.MiniatureBox);
            descriptionField?.SetValue(ironLegion, "A complete starter army for the Iron Legion faction. Contains 10 detailed miniatures and assembly guide.");
            
            AssetDatabase.CreateAsset(ironLegion, "Assets/ScriptableObjects/IronLegionStarter.asset");
            
            // Create Crimson Battle Paint
            ProductData crimsonPaint = ScriptableObject.CreateInstance<ProductData>();
            
            productNameField?.SetValue(crimsonPaint, "Crimson Battle Paint");
            basePriceField?.SetValue(crimsonPaint, 3);
            typeField?.SetValue(crimsonPaint, ProductType.PaintPot);
            descriptionField?.SetValue(crimsonPaint, "High-quality acrylic paint perfect for miniature painting. Rich crimson color ideal for armor and details.");
            
            AssetDatabase.CreateAsset(crimsonPaint, "Assets/ScriptableObjects/CrimsonBattlePaint.asset");
            
            // Create Core Rulebook
            ProductData coreRulebook = ScriptableObject.CreateInstance<ProductData>();
            
            productNameField?.SetValue(coreRulebook, "Core Rulebook");
            basePriceField?.SetValue(coreRulebook, 25);
            typeField?.SetValue(coreRulebook, ProductType.Rulebook);
            descriptionField?.SetValue(coreRulebook, "Complete rules for tabletop warfare. Includes basic rules, advanced tactics, and lore sections.");
            
            AssetDatabase.CreateAsset(coreRulebook, "Assets/ScriptableObjects/CoreRulebook.asset");
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            Debug.Log("Default product assets created successfully!");
        }
    }
}
