using UnityEngine;

namespace TabletopShop
{
    /// <summary>
    /// Utility class for material creation, modification, and cleanup
    /// Centralizes material management to prevent memory leaks and code duplication
    /// </summary>
    public static class MaterialUtility
    {
        /// <summary>
        /// Create a highlight material from a base material with specified highlight color
        /// </summary>
        /// <param name="baseMaterial">The base material to copy properties from</param>
        /// <param name="highlightColor">The color to apply as highlight</param>
        /// <returns>New material with highlight color applied</returns>
        public static Material CreateHighlightMaterial(Material baseMaterial, Color highlightColor)
        {
            if (baseMaterial == null)
            {
                Debug.LogWarning("BaseMaterial is null, creating default highlight material");
                return CreateHighlightMaterial(highlightColor);
            }

            Material highlightMaterial = new Material(baseMaterial);
            highlightMaterial.color = highlightColor;
            
            // Set standard material properties for highlighting
            highlightMaterial.SetFloat("_Metallic", 0f);
            highlightMaterial.SetFloat("_Smoothness", 0.5f);
            
            return highlightMaterial;
        }

        /// <summary>
        /// Create a highlight material with specified color using Standard shader
        /// </summary>
        /// <param name="highlightColor">The color for the highlight material</param>
        /// <returns>New highlight material with Standard shader</returns>
        public static Material CreateHighlightMaterial(Color highlightColor)
        {
            Material highlightMaterial = new Material(Shader.Find("Standard"));
            highlightMaterial.color = highlightColor;
            highlightMaterial.SetFloat("_Metallic", 0f);
            highlightMaterial.SetFloat("_Smoothness", 0.5f);
            
            return highlightMaterial;
        }

        /// <summary>
        /// Create an emissive material with base color and emission color
        /// </summary>
        /// <param name="baseColor">The base color of the material</param>
        /// <param name="emissionColor">The emission color for glow effect</param>
        /// <param name="emissionIntensity">Intensity of the emission (default: 1.0f)</param>
        /// <returns>New emissive material</returns>
        public static Material CreateEmissiveMaterial(Color baseColor, Color emissionColor, float emissionIntensity = 1.0f)
        {
            Material emissiveMaterial = new Material(Shader.Find("Standard"));
            emissiveMaterial.color = baseColor;
            
            // Enable emission
            emissiveMaterial.EnableKeyword("_EMISSION");
            emissiveMaterial.SetColor("_EmissionColor", emissionColor * emissionIntensity);
            
            // Set other standard properties
            emissiveMaterial.SetFloat("_Metallic", 0f);
            emissiveMaterial.SetFloat("_Smoothness", 0.5f);
            
            return emissiveMaterial;
        }

        /// <summary>
        /// Create an emissive material from a base material with emission added
        /// </summary>
        /// <param name="baseMaterial">The base material to copy properties from</param>
        /// <param name="emissionColor">The emission color for glow effect</param>
        /// <param name="emissionIntensity">Intensity of the emission (default: 1.0f)</param>
        /// <returns>New emissive material based on the original</returns>
        public static Material CreateEmissiveMaterial(Material baseMaterial, Color emissionColor, float emissionIntensity = 1.0f)
        {
            if (baseMaterial == null)
            {
                Debug.LogWarning("BaseMaterial is null, creating default emissive material");
                return CreateEmissiveMaterial(Color.white, emissionColor, emissionIntensity);
            }

            Material emissiveMaterial = new Material(baseMaterial);
            
            // Enable emission
            emissiveMaterial.EnableKeyword("_EMISSION");
            emissiveMaterial.SetColor("_EmissionColor", emissionColor * emissionIntensity);
            
            return emissiveMaterial;
        }

        /// <summary>
        /// Safely destroy a material, checking for null and preventing destruction of default materials
        /// </summary>
        /// <param name="material">The material to destroy</param>
        /// <param name="immediate">Whether to destroy immediately (for edit mode) or normally (default: false)</param>
        public static void SafeDestroyMaterial(Material material, bool immediate = false)
        {
            if (material == null) return;
            
            // Don't destroy default Unity materials or materials that are assets
            if (material.name.Contains("Default") || 
                material.name.Contains("UI-Default"))
            {
                return;
            }

            #if UNITY_EDITOR
            // In editor, also check if it's an asset
            if (!string.IsNullOrEmpty(UnityEditor.AssetDatabase.GetAssetPath(material)))
            {
                return;
            }
            #endif

            if (immediate)
            {
                Object.DestroyImmediate(material);
            }
            else
            {
                Object.Destroy(material);
            }
        }

        /// <summary>
        /// Create a standard material with specified color and properties
        /// </summary>
        /// <param name="color">The main color of the material</param>
        /// <param name="metallic">Metallic value (0-1, default: 0)</param>
        /// <param name="smoothness">Smoothness value (0-1, default: 0.5)</param>
        /// <returns>New standard material with specified properties</returns>
        public static Material CreateStandardMaterial(Color color, float metallic = 0f, float smoothness = 0.5f)
        {
            Material material = new Material(Shader.Find("Standard"));
            material.color = color;
            material.SetFloat("_Metallic", metallic);
            material.SetFloat("_Smoothness", smoothness);
            
            return material;
        }

        /// <summary>
        /// Apply a color to an existing material safely
        /// </summary>
        /// <param name="material">The material to modify</param>
        /// <param name="color">The color to apply</param>
        public static void ApplyColorToMaterial(Material material, Color color)
        {
            if (material == null)
            {
                Debug.LogWarning("Cannot apply color to null material");
                return;
            }

            material.color = color;
        }

        /// <summary>
        /// Check if a material is safe to modify (not a default Unity material)
        /// </summary>
        /// <param name="material">The material to check</param>
        /// <returns>True if safe to modify, false otherwise</returns>
        public static bool IsSafeToModify(Material material)
        {
            if (material == null) return false;
            
            bool isNotDefault = !material.name.Contains("Default") && !material.name.Contains("UI-Default");
            
            #if UNITY_EDITOR
            bool isNotAsset = string.IsNullOrEmpty(UnityEditor.AssetDatabase.GetAssetPath(material));
            return isNotDefault && isNotAsset;
            #else
            return isNotDefault;
            #endif
        }
    }
}
