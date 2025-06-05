using UnityEngine;
using UnityEditor;

namespace TabletopShop
{
    /// <summary>
    /// Quick setup utility to create a working interaction system test scene
    /// </summary>
    public class QuickInteractionSetup : MonoBehaviour
    {
        [Header("Setup Components")]
        [SerializeField] private bool autoSetupOnStart = true;
        [SerializeField] private bool createTestObjects = true;
        
        [Header("Player Setup")]
        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private Vector3 playerSpawnPosition = new Vector3(0, 1, -3);
        
        [Header("Test Objects")]
        [SerializeField] private ProductData[] testProducts;
        [SerializeField] private Vector3[] testProductPositions = {
            new Vector3(-2, 1, 0),
            new Vector3(0, 1, 0),
            new Vector3(2, 1, 0)
        };
        
        private void Start()
        {
            if (autoSetupOnStart)
            {
                SetupInteractionSystem();
            }
        }
        
        [ContextMenu("Setup Interaction System")]
        public void SetupInteractionSystem()
        {
            Debug.Log("Setting up interaction system...");
            
            // 1. Check layers first
            Debug.Log("1. Checking layers...");
            bool layersValid = InteractionLayers.ValidateLayers();
            if (!layersValid)
            {
                Debug.LogWarning("Layers are not set up correctly. Please use the menu: TabletopShop > Setup Interaction Layers");
            }
            
            // 2. Create or setup player
            Debug.Log("2. Setting up player...");
            SetupPlayer();
            
            // 3. Create test objects if needed
            if (createTestObjects)
            {
                Debug.Log("3. Creating test objects...");
                CreateTestObjects();
            }
            
            // 4. Validate setup
            Debug.Log("4. Validating setup...");
            ValidateSetup();
            
            Debug.Log("<color=green>Interaction system setup complete!</color>");
            Debug.Log("Instructions:");
            Debug.Log("- Move with WASD");
            Debug.Log("- Look with mouse");
            Debug.Log("- Press E to interact");
            Debug.Log("- Press F1 to open debug window");
        }
        
        private void SetupPlayer()
        {
            GameObject player = null;
            
            // Try to find existing player
            SimplePlayerController existingController = FindObjectOfType<SimplePlayerController>();
            if (existingController != null)
            {
                player = existingController.gameObject;
                Debug.Log("Found existing player controller");
            }
            else
            {
                // Create new player
                player = new GameObject("Player");
                player.transform.position = playerSpawnPosition;
                
                // Add player controller
                player.AddComponent<SimplePlayerController>();
                Debug.Log("Created new player with SimplePlayerController");
            }
            
            // Ensure player has interaction system
            PlayerInteraction interaction = player.GetComponent<PlayerInteraction>();
            if (interaction == null)
            {
                interaction = player.AddComponent<PlayerInteraction>();
                Debug.Log("Added PlayerInteraction component");
            }
            
            // Add debugger
            InteractionSystemDebugger debugger = player.GetComponent<InteractionSystemDebugger>();
            if (debugger == null)
            {
                debugger = player.AddComponent<InteractionSystemDebugger>();
                Debug.Log("Added InteractionSystemDebugger component");
            }
            
            // Ensure camera setup
            Camera playerCamera = player.GetComponentInChildren<Camera>();
            if (playerCamera == null)
            {
                GameObject cameraGO = new GameObject("PlayerCamera");
                cameraGO.transform.SetParent(player.transform);
                cameraGO.transform.localPosition = new Vector3(0, 1.8f, 0);
                playerCamera = cameraGO.AddComponent<Camera>();
                playerCamera.tag = "MainCamera";
                Debug.Log("Created player camera");
            }
        }
        
        private void CreateTestObjects()
        {
            // Create some basic test cubes that can be interacted with
            for (int i = 0; i < testProductPositions.Length; i++)
            {
                Vector3 pos = testProductPositions[i];
                
                // Create cube
                GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cube.transform.position = pos;
                cube.name = $"TestProduct_{i}";
                
                // Add Product component
                Product product = cube.AddComponent<Product>();
                
                // Create basic ProductData if we have test products
                if (testProducts != null && i < testProducts.Length && testProducts[i] != null)
                {
                    product.Initialize(testProducts[i]);
                }
                else
                {
                    // Create a basic test product data
                    CreateBasicTestProduct(product, $"Test Product {i + 1}", 10 + i * 5);
                }
                
                // Ensure proper layer
                InteractionLayers.SetProductLayer(cube);
                
                Debug.Log($"Created test product: {cube.name} at {pos}");
            }
            
            // Create a shelf slot test
            GameObject slotGO = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            slotGO.transform.position = new Vector3(4, 0.5f, 0);
            slotGO.transform.localScale = new Vector3(1, 0.1f, 1);
            slotGO.name = "TestShelfSlot";
            
            // Change material to green to indicate it's a slot
            Renderer renderer = slotGO.GetComponent<Renderer>();
            Material slotMaterial = new Material(Shader.Find("Standard"));
            slotMaterial.color = Color.green;
            renderer.material = slotMaterial;
            
            // Add ShelfSlot component
            ShelfSlot slot = slotGO.AddComponent<ShelfSlot>();
            
            // Ensure proper layer
            InteractionLayers.SetShelfLayer(slotGO);
            
            Debug.Log($"Created test shelf slot: {slotGO.name}");
        }
        
        private void CreateBasicTestProduct(Product product, string productName, int price)
        {
            // We'll need to manually set the internal fields since we can't create ScriptableObjects at runtime easily
            // This is a workaround for testing
            
            // You can create ProductData assets in the editor and assign them to testProducts array
            Debug.Log($"Product {productName} needs ProductData asset. Create one in the editor.");
        }
        
        private void ValidateSetup()
        {
            bool isValid = true;
            
            // Check player
            SimplePlayerController player = FindObjectOfType<SimplePlayerController>();
            if (player == null)
            {
                Debug.LogError("No SimplePlayerController found!");
                isValid = false;
            }
            
            PlayerInteraction interaction = FindObjectOfType<PlayerInteraction>();
            if (interaction == null)
            {
                Debug.LogError("No PlayerInteraction found!");
                isValid = false;
            }
            
            Camera mainCamera = Camera.main;
            if (mainCamera == null)
            {
                Debug.LogError("No main camera found!");
                isValid = false;
            }
            
            // Check for interactable objects
            Product[] products = FindObjectsOfType<Product>();
            ShelfSlot[] slots = FindObjectsOfType<ShelfSlot>();
            
            if (products.Length == 0 && slots.Length == 0)
            {
                Debug.LogWarning("No interactable objects found! Create some products or shelf slots.");
            }
            
            // Check layers
            bool layersValid = InteractionLayers.ValidateLayers();
            if (!layersValid)
            {
                Debug.LogError("Interaction layers are not properly set up!");
                isValid = false;
            }
            
            if (isValid)
            {
                Debug.Log("<color=green>✓ Interaction system validation passed!</color>");
            }
            else
            {
                Debug.LogError("<color=red>✗ Interaction system validation failed!</color>");
            }
        }
    }
}
