using UnityEngine;
using System.Collections.Generic;

namespace TabletopShop
{
    /// <summary>
    /// Manages customer character variety by randomly selecting from available character meshes
    /// Provides visual diversity for spawned customers
    /// </summary>
    public class CustomerVarietyManager : MonoBehaviour
    {
        [Header("Character Mesh Options")]
        [SerializeField] private List<GameObject> maleCharacterPrefabs = new List<GameObject>();
        [SerializeField] private List<GameObject> femaleCharacterPrefabs = new List<GameObject>();
        [SerializeField] private bool randomizeGender = true;
        [SerializeField] private float maleSpawnProbability = 0.5f;
        
        [Header("Character Customization")]
        [SerializeField] private bool overrideCharacterMaterials = false;
        [SerializeField] private List<Material> customMaterials = new List<Material>();
        
        [Header("Animation Variety")]
        [SerializeField] private List<RuntimeAnimatorController> animatorControllers = new List<RuntimeAnimatorController>();
        [SerializeField] private bool randomizeAnimations = false;
        
        // Static instance for easy access
        public static CustomerVarietyManager Instance { get; private set; }
        
        private void Awake()
        {
            // Singleton pattern
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }
            
            ValidateSetup();
        }
        
        /// <summary>
        /// Apply random character variety to a customer
        /// </summary>
        public void ApplyRandomVariety(GameObject customer)
        {
            if (customer == null) return;
            
            // Find existing character mesh child
            Transform characterMesh = customer.transform.Find("CharacterMesh");
            if (characterMesh != null)
            {
                // Replace with random character
                ReplaceCharacterMesh(customer, characterMesh.gameObject);
            }
            else
            {
                Debug.LogWarning($"Customer {customer.name} does not have a CharacterMesh child object");
            }
        }
        
        /// <summary>
        /// Replace the character mesh with a random selection
        /// </summary>
        private void ReplaceCharacterMesh(GameObject customer, GameObject oldCharacterMesh)
        {
            // Select random character prefab
            GameObject selectedCharacterPrefab = GetRandomCharacterPrefab();
            if (selectedCharacterPrefab == null)
            {
                Debug.LogWarning("No character prefabs available for variety");
                return;
            }
            
            // Store transform data
            Transform parent = oldCharacterMesh.transform.parent;
            Vector3 localPosition = oldCharacterMesh.transform.localPosition;
            Quaternion localRotation = oldCharacterMesh.transform.localRotation;
            Vector3 localScale = oldCharacterMesh.transform.localScale;
            
            // Destroy old character mesh
            if (Application.isPlaying)
                Destroy(oldCharacterMesh);
            else
                DestroyImmediate(oldCharacterMesh);
            
            // Instantiate new character mesh
            GameObject newCharacterMesh = Instantiate(selectedCharacterPrefab, parent);
            newCharacterMesh.name = "CharacterMesh";
            newCharacterMesh.transform.localPosition = localPosition;
            newCharacterMesh.transform.localRotation = localRotation;
            newCharacterMesh.transform.localScale = localScale;
            
            // Remove any colliders from character mesh (customer should handle collision)
            var colliders = newCharacterMesh.GetComponentsInChildren<Collider>();
            foreach (var collider in colliders)
            {
                if (Application.isPlaying)
                    Destroy(collider);
                else
                    DestroyImmediate(collider);
            }
            
            // Apply material customization if enabled
            if (overrideCharacterMaterials && customMaterials.Count > 0)
            {
                ApplyRandomMaterial(newCharacterMesh);
            }
            
            // Apply animation variety if enabled
            if (randomizeAnimations)
            {
                ApplyRandomAnimator(customer);
            }
            
            Debug.Log($"Applied character variety to {customer.name}: {selectedCharacterPrefab.name}");
        }
        
        /// <summary>
        /// Get a random character prefab based on gender settings
        /// </summary>
        private GameObject GetRandomCharacterPrefab()
        {
            List<GameObject> availablePrefabs = new List<GameObject>();
            
            if (randomizeGender)
            {
                // Randomly choose gender based on probability
                bool selectMale = Random.value <= maleSpawnProbability;
                
                if (selectMale && maleCharacterPrefabs.Count > 0)
                    availablePrefabs.AddRange(maleCharacterPrefabs);
                else if (!selectMale && femaleCharacterPrefabs.Count > 0)
                    availablePrefabs.AddRange(femaleCharacterPrefabs);
                else
                {
                    // Fallback to any available
                    availablePrefabs.AddRange(maleCharacterPrefabs);
                    availablePrefabs.AddRange(femaleCharacterPrefabs);
                }
            }
            else
            {
                // Use all available prefabs
                availablePrefabs.AddRange(maleCharacterPrefabs);
                availablePrefabs.AddRange(femaleCharacterPrefabs);
            }
            
            if (availablePrefabs.Count == 0)
                return null;
            
            return availablePrefabs[Random.Range(0, availablePrefabs.Count)];
        }
        
        /// <summary>
        /// Apply a random material from the custom materials list
        /// </summary>
        private void ApplyRandomMaterial(GameObject characterMesh)
        {
            if (customMaterials.Count == 0) return;
            
            var renderers = characterMesh.GetComponentsInChildren<MeshRenderer>();
            Material randomMaterial = customMaterials[Random.Range(0, customMaterials.Count)];
            
            foreach (var renderer in renderers)
            {
                renderer.material = randomMaterial;
            }
        }
        
        /// <summary>
        /// Apply a random animator controller
        /// </summary>
        private void ApplyRandomAnimator(GameObject customer)
        {
            if (animatorControllers.Count == 0) return;
            
            var animator = customer.GetComponent<Animator>();
            if (animator != null)
            {
                var randomController = animatorControllers[Random.Range(0, animatorControllers.Count)];
                animator.runtimeAnimatorController = randomController;
            }
        }
        
        /// <summary>
        /// Automatically populate character prefab lists from Kevin Iglesias assets
        /// </summary>
        [ContextMenu("Auto-Populate Character Lists")]
        public void AutoPopulateCharacterLists()
        {
            maleCharacterPrefabs.Clear();
            femaleCharacterPrefabs.Clear();
            
            // Find all Kevin Iglesias human dummy prefabs
            string[] prefabGUIDs = UnityEditor.AssetDatabase.FindAssets("HumanDummy t:Prefab", new[] { "Assets/Kevin Iglesias" });
            
            foreach (string guid in prefabGUIDs)
            {
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(path);
                
                if (prefab != null)
                {
                    if (prefab.name.Contains("_M "))
                        maleCharacterPrefabs.Add(prefab);
                    else if (prefab.name.Contains("_F "))
                        femaleCharacterPrefabs.Add(prefab);
                }
            }
            
            Debug.Log($"Auto-populated: {maleCharacterPrefabs.Count} male prefabs, {femaleCharacterPrefabs.Count} female prefabs");
        }
        
        /// <summary>
        /// Validate the setup and warn about missing components
        /// </summary>
        private void ValidateSetup()
        {
            if (maleCharacterPrefabs.Count == 0 && femaleCharacterPrefabs.Count == 0)
            {
                Debug.LogWarning("CustomerVarietyManager: No character prefabs assigned. Use 'Auto-Populate Character Lists' context menu.");
            }
            
            if (randomizeGender && (maleCharacterPrefabs.Count == 0 || femaleCharacterPrefabs.Count == 0))
            {
                Debug.LogWarning("CustomerVarietyManager: Gender randomization enabled but missing male or female prefabs.");
            }
        }
        
        /// <summary>
        /// Get statistics about available character variety
        /// </summary>
        public string GetVarietyStats()
        {
            return $"Character Variety: {maleCharacterPrefabs.Count} male, {femaleCharacterPrefabs.Count} female, " +
                   $"{customMaterials.Count} materials, {animatorControllers.Count} animations";
        }
    }
}
