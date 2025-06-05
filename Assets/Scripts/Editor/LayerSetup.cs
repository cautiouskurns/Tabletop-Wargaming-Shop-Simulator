using UnityEngine;
using UnityEditor;

namespace TabletopShop.Editor
{
    /// <summary>
    /// Editor utility to automatically set up required layers for the interaction system
    /// </summary>
    public static class LayerSetup
    {
        [MenuItem("TabletopShop/Setup Interaction Layers")]
        public static void SetupLayers()
        {
            // Get the TagManager
            SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            SerializedProperty layersProp = tagManager.FindProperty("layers");
            
            // Check and add required layers
            bool layersAdded = false;
            
            // Layer 8 - Interactable
            if (AddLayerIfMissing(layersProp, 8, InteractionLayers.INTERACTABLE_LAYER))
            {
                layersAdded = true;
                Debug.Log($"Added layer 8: {InteractionLayers.INTERACTABLE_LAYER}");
            }
            
            // Layer 9 - Product  
            if (AddLayerIfMissing(layersProp, 9, InteractionLayers.PRODUCT_LAYER))
            {
                layersAdded = true;
                Debug.Log($"Added layer 9: {InteractionLayers.PRODUCT_LAYER}");
            }
            
            // Layer 10 - Shelf
            if (AddLayerIfMissing(layersProp, 10, InteractionLayers.SHELF_LAYER))
            {
                layersAdded = true;
                Debug.Log($"Added layer 10: {InteractionLayers.SHELF_LAYER}");
            }
            
            if (layersAdded)
            {
                // Apply changes
                tagManager.ApplyModifiedProperties();
                AssetDatabase.SaveAssets();
                
                Debug.Log("<color=green>Interaction layers have been set up successfully!</color>");
                Debug.Log("Layers added: Interactable (8), Product (9), Shelf (10)");
            }
            else
            {
                Debug.Log("<color=yellow>All required layers already exist.</color>");
            }
            
            // Validate the setup
            InteractionLayers.ValidateLayers();
        }
        
        /// <summary>
        /// Check if layer exists and add it if missing
        /// </summary>
        /// <param name="layersProp">Layers property from TagManager</param>
        /// <param name="layerIndex">Index to check/set</param>
        /// <param name="layerName">Name of the layer</param>
        /// <returns>True if layer was added</returns>
        private static bool AddLayerIfMissing(SerializedProperty layersProp, int layerIndex, string layerName)
        {
            SerializedProperty layerProp = layersProp.GetArrayElementAtIndex(layerIndex);
            
            if (string.IsNullOrEmpty(layerProp.stringValue))
            {
                layerProp.stringValue = layerName;
                return true;
            }
            else if (layerProp.stringValue != layerName)
            {
                Debug.LogWarning($"Layer {layerIndex} is already occupied by '{layerProp.stringValue}'. " +
                               $"Expected '{layerName}'. Please manually set up the layer.");
            }
            
            return false;
        }
        
        [MenuItem("TabletopShop/Validate Interaction Layers")]
        public static void ValidateLayers()
        {
            InteractionLayers.ValidateLayers();
        }
    }
}