using UnityEngine;
using UnityEditor;
using System.IO;

namespace TabletopShop.Editor
{
    /// <summary>
    /// Editor script to replace customer capsule with human character and set up animation rigging
    /// </summary>
    public class CustomerCharacterSetup : EditorWindow
    {
        public enum CharacterGender { Male, Female }
        public enum CharacterColor { White, Blue, Green, Red, Orange, Purple, Yellow }
        
        [Header("Character Selection")]
        private CharacterGender selectedGender = CharacterGender.Male;
        private CharacterColor selectedColor = CharacterColor.White;
        private bool setupAnimator = true;
        private bool keepCapsuleCollider = true;
        private bool addCharacterController = false;
        
        [MenuItem("TabletopShop/Customer Character Setup")]
        public static void ShowWindow()
        {
            var window = GetWindow<CustomerCharacterSetup>("Customer Character Setup");
            window.minSize = new Vector2(400, 300);
        }
        
        private void OnGUI()
        {
            GUILayout.Label("Customer Character Setup", EditorStyles.boldLabel);
            GUILayout.Space(10);
            
            GUILayout.Label("Replace capsule customer with human character from Kevin Iglesias assets", EditorStyles.wordWrappedLabel);
            GUILayout.Space(10);
            
            // Character selection
            GUILayout.Label("Character Options", EditorStyles.boldLabel);
            selectedGender = (CharacterGender)EditorGUILayout.EnumPopup("Gender", selectedGender);
            selectedColor = (CharacterColor)EditorGUILayout.EnumPopup("Color", selectedColor);
            
            GUILayout.Space(10);
            
            // Setup options
            GUILayout.Label("Setup Options", EditorStyles.boldLabel);
            setupAnimator = EditorGUILayout.Toggle("Setup Animator Component", setupAnimator);
            keepCapsuleCollider = EditorGUILayout.Toggle("Keep Capsule Collider", keepCapsuleCollider);
            addCharacterController = EditorGUILayout.Toggle("Add Character Controller", addCharacterController);
            
            GUILayout.Space(20);
            
            // Action buttons
            if (GUILayout.Button("Create New Customer with Human Character", GUILayout.Height(30)))
            {
                CreateHumanCustomer();
            }
            
            GUILayout.Space(10);
            
            if (GUILayout.Button("Update Existing Customer Prefab", GUILayout.Height(30)))
            {
                UpdateExistingCustomerPrefab();
            }
            
            GUILayout.Space(20);
            
            // Info section
            EditorGUILayout.HelpBox(
                "• 'Create New' will make a new customer prefab with human character\n" +
                "• 'Update Existing' will modify the current Customer.prefab\n" +
                "• Capsule collider is recommended for NavMesh navigation\n" +
                "• Character Controller provides better physics interaction", 
                MessageType.Info);
        }
        
        private void CreateHumanCustomer()
        {
            string characterPrefabPath = GetCharacterPrefabPath();
            string customerPrefabPath = "Assets/Prefabs/AI/Customer.prefab";
            
            // Load the human character prefab
            GameObject humanCharacterPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(characterPrefabPath);
            if (humanCharacterPrefab == null)
            {
                Debug.LogError($"Could not find human character prefab at: {characterPrefabPath}");
                return;
            }
            
            // Load the existing customer prefab as reference
            GameObject existingCustomerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(customerPrefabPath);
            if (existingCustomerPrefab == null)
            {
                Debug.LogError($"Could not find existing customer prefab at: {customerPrefabPath}");
                return;
            }
            
            // Create new customer GameObject
            GameObject newCustomer = new GameObject("Customer_HumanCharacter");
            
            // Add customer components first
            var customer = newCustomer.AddComponent<Customer>();
            var navMeshAgent = newCustomer.AddComponent<UnityEngine.AI.NavMeshAgent>();
            
            // Copy settings from existing customer prefab
            CopyCustomerSettings(existingCustomerPrefab.GetComponent<Customer>(), customer);
            CopyNavMeshAgentSettings(existingCustomerPrefab.GetComponent<UnityEngine.AI.NavMeshAgent>(), navMeshAgent);
            
            // Instantiate human character as child
            GameObject humanCharacter = PrefabUtility.InstantiatePrefab(humanCharacterPrefab) as GameObject;
            humanCharacter.transform.SetParent(newCustomer.transform);
            humanCharacter.transform.localPosition = Vector3.zero;
            humanCharacter.transform.localRotation = Quaternion.identity;
            humanCharacter.name = "CharacterMesh";
            
            // Setup colliders
            SetupColliders(newCustomer, humanCharacter);
            
            // Setup animator if requested
            if (setupAnimator)
            {
                SetupAnimator(newCustomer, humanCharacter);
            }
            
            // Set customer tag
            newCustomer.tag = "Customer";
            
            // Save as new prefab
            string newPrefabPath = "Assets/Prefabs/AI/Customer_HumanCharacter.prefab";
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(newCustomer, newPrefabPath);
            
            // Clean up temporary object
            DestroyImmediate(newCustomer);
            
            Debug.Log($"Created new human character customer prefab at: {newPrefabPath}");
            EditorGUIUtility.PingObject(prefab);
        }
        
        private void UpdateExistingCustomerPrefab()
        {
            string characterPrefabPath = GetCharacterPrefabPath();
            string customerPrefabPath = "Assets/Prefabs/AI/Customer.prefab";
            
            // Load prefabs
            GameObject humanCharacterPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(characterPrefabPath);
            GameObject customerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(customerPrefabPath);
            
            if (humanCharacterPrefab == null || customerPrefab == null)
            {
                Debug.LogError("Could not find required prefabs!");
                return;
            }
            
            // Enter prefab mode
            var prefabStage = PrefabUtility.LoadPrefabContents(customerPrefabPath);
            
            // Remove existing mesh components (capsule)
            var existingMeshFilter = prefabStage.GetComponent<MeshFilter>();
            var existingMeshRenderer = prefabStage.GetComponent<MeshRenderer>();
            
            if (existingMeshFilter != null)
                DestroyImmediate(existingMeshFilter);
            if (existingMeshRenderer != null)
                DestroyImmediate(existingMeshRenderer);
            
            // Add human character as child
            GameObject humanCharacter = PrefabUtility.InstantiatePrefab(humanCharacterPrefab) as GameObject;
            humanCharacter.transform.SetParent(prefabStage.transform);
            humanCharacter.transform.localPosition = Vector3.zero;
            humanCharacter.transform.localRotation = Quaternion.identity;
            humanCharacter.name = "CharacterMesh";
            
            // Setup colliders
            SetupColliders(prefabStage, humanCharacter);
            
            // Setup animator if requested
            if (setupAnimator)
            {
                SetupAnimator(prefabStage, humanCharacter);
            }
            
            // Save the updated prefab
            PrefabUtility.SaveAsPrefabAsset(prefabStage, customerPrefabPath);
            PrefabUtility.UnloadPrefabContents(prefabStage);
            
            Debug.Log($"Updated existing customer prefab with human character");
            EditorGUIUtility.PingObject(customerPrefab);
        }
        
        private string GetCharacterPrefabPath()
        {
            string genderSuffix = selectedGender == CharacterGender.Male ? "M" : "F";
            string colorName = selectedColor.ToString();
            return $"Assets/Kevin Iglesias/Human Character Dummy/Prefabs/HumanDummy_{genderSuffix} {colorName}.prefab";
        }
        
        private void CopyCustomerSettings(Customer source, Customer target)
        {
            if (source == null || target == null) return;
            
            // Copy serialized fields using SerializedObject
            SerializedObject sourceObj = new SerializedObject(source);
            SerializedObject targetObj = new SerializedObject(target);
            
            // Copy shopping time
            var shoppingTimeProp = sourceObj.FindProperty("shoppingTime");
            if (shoppingTimeProp != null)
                targetObj.FindProperty("shoppingTime").floatValue = shoppingTimeProp.floatValue;
            
            // Copy movement settings
            var movementSpeedProp = sourceObj.FindProperty("movementSpeed");
            if (movementSpeedProp != null)
                targetObj.FindProperty("movementSpeed").floatValue = movementSpeedProp.floatValue;
                
            var stoppingDistanceProp = sourceObj.FindProperty("stoppingDistance");
            if (stoppingDistanceProp != null)
                targetObj.FindProperty("stoppingDistance").floatValue = stoppingDistanceProp.floatValue;
            
            targetObj.ApplyModifiedProperties();
        }
        
        private void CopyNavMeshAgentSettings(UnityEngine.AI.NavMeshAgent source, UnityEngine.AI.NavMeshAgent target)
        {
            if (source == null || target == null) return;
            
            target.speed = source.speed;
            target.acceleration = source.acceleration;
            target.angularSpeed = source.angularSpeed;
            target.stoppingDistance = source.stoppingDistance;
            target.radius = source.radius;
            target.height = source.height;
            target.baseOffset = source.baseOffset;
        }
        
        private void SetupColliders(GameObject customer, GameObject humanCharacter)
        {
            // Keep capsule collider on main customer object for NavMesh
            if (keepCapsuleCollider)
            {
                var capsuleCollider = customer.GetComponent<CapsuleCollider>();
                if (capsuleCollider == null)
                {
                    capsuleCollider = customer.AddComponent<CapsuleCollider>();
                }
                
                // Adjust capsule to match human character bounds
                capsuleCollider.height = 1.8f;
                capsuleCollider.radius = 0.4f;
                capsuleCollider.center = new Vector3(0, 0.9f, 0);
            }
            
            // Add character controller for better physics interaction
            if (addCharacterController)
            {
                var characterController = customer.GetComponent<CharacterController>();
                if (characterController == null)
                {
                    characterController = customer.AddComponent<CharacterController>();
                    characterController.height = 1.8f;
                    characterController.radius = 0.4f;
                    characterController.center = new Vector3(0, 0.9f, 0);
                }
            }
            
            // Remove any colliders from the human character mesh to avoid conflicts
            var humanColliders = humanCharacter.GetComponentsInChildren<Collider>();
            foreach (var collider in humanColliders)
            {
                DestroyImmediate(collider);
            }
        }
        
        private void SetupAnimator(GameObject customer, GameObject humanCharacter)
        {
            // Check if human character already has an Animator
            var humanAnimator = humanCharacter.GetComponent<Animator>();
            
            if (humanAnimator != null)
            {
                // Move animator to main customer object
                var customerAnimator = customer.GetComponent<Animator>();
                if (customerAnimator == null)
                {
                    customerAnimator = customer.AddComponent<Animator>();
                }
                
                // Copy settings
                customerAnimator.runtimeAnimatorController = humanAnimator.runtimeAnimatorController;
                customerAnimator.avatar = humanAnimator.avatar;
                customerAnimator.applyRootMotion = false; // Usually false for NavMesh characters
                
                // Remove animator from child to avoid conflicts
                DestroyImmediate(humanAnimator);
            }
            else
            {
                // Add animator to customer if none exists
                var customerAnimator = customer.GetComponent<Animator>();
                if (customerAnimator == null)
                {
                    customerAnimator = customer.AddComponent<Animator>();
                    customerAnimator.applyRootMotion = false;
                }
            }
        }
    }
}
