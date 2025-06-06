using UnityEngine;

namespace TabletopShop
{
    /// <summary>
    /// Utility class for managing interaction layers and tags
    /// </summary>
    public static class InteractionLayers
    {
        // Layer names (these should be set up in Unity's Layer settings)
        public const string INTERACTABLE_LAYER = "Interactable";
        public const string PRODUCT_LAYER = "Product";
        public const string SHELF_LAYER = "Shelf";
        
        // Layer indices
        public static int InteractableLayerIndex => LayerMask.NameToLayer(INTERACTABLE_LAYER);
        public static int ProductLayerIndex => LayerMask.NameToLayer(PRODUCT_LAYER);
        public static int ShelfLayerIndex => LayerMask.NameToLayer(SHELF_LAYER);
        
        // Layer masks
        public static LayerMask InteractableLayerMask => 1 << InteractableLayerIndex;
        public static LayerMask ProductLayerMask => 1 << ProductLayerIndex;
        public static LayerMask ShelfLayerMask => 1 << ShelfLayerIndex;
        public static LayerMask AllInteractablesMask => InteractableLayerMask | ProductLayerMask | ShelfLayerMask;
        
        /// <summary>
        /// Set GameObject to interactable layer
        /// </summary>
        /// <param name="gameObject">GameObject to modify</param>
        public static void SetInteractableLayer(GameObject gameObject)
        {
            if (gameObject != null && InteractableLayerIndex >= 0)
            {
                gameObject.layer = InteractableLayerIndex;
            }
        }
        
        /// <summary>
        /// Set GameObject to product layer
        /// </summary>
        /// <param name="gameObject">GameObject to modify</param>
        public static void SetProductLayer(GameObject gameObject)
        {
            if (gameObject != null && ProductLayerIndex >= 0)
            {
                gameObject.layer = ProductLayerIndex;
            }
        }
        
        /// <summary>
        /// Set GameObject to shelf layer
        /// </summary>
        /// <param name="gameObject">GameObject to modify</param>
        public static void SetShelfLayer(GameObject gameObject)
        {
            if (gameObject != null && ShelfLayerIndex >= 0)
            {
                gameObject.layer = ShelfLayerIndex;
            }
        }
        
        /// <summary>
        /// Check if layers are properly set up
        /// </summary>
        /// <returns>True if all required layers exist</returns>
        public static bool ValidateLayers()
        {
            bool valid = true;
            
            if (InteractableLayerIndex < 0)
            {
                Debug.LogWarning($"Layer '{INTERACTABLE_LAYER}' not found. Please add it in Project Settings > Tags and Layers.");
                valid = false;
            }
            
            if (ProductLayerIndex < 0)
            {
                Debug.LogWarning($"Layer '{PRODUCT_LAYER}' not found. Please add it in Project Settings > Tags and Layers.");
                valid = false;
            }
            
            if (ShelfLayerIndex < 0)
            {
                Debug.LogWarning($"Layer '{SHELF_LAYER}' not found. Please add it in Project Settings > Tags and Layers.");
                valid = false;
            }
            
            if (valid)
            {
                Debug.Log("All interaction layers are properly configured.");
            }
            
            return valid;
        }
    }
}
