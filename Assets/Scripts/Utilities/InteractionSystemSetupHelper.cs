using UnityEngine;

namespace TabletopShop
{
    /// <summary>
    /// Simple runtime setup helper for the interaction system
    /// This script helps you get the interaction system working step by step
    /// </summary>
    public class InteractionSystemSetupHelper : MonoBehaviour
    {
        [Header("Setup Steps")]
        [SerializeField] private bool step1_LayersSetup = false;
        [SerializeField] private bool step2_PlayerSetup = false;
        [SerializeField] private bool step3_TestObjectsCreated = false;
        
        [Header("Manual Setup")]
        [SerializeField] private bool createTestCubes = true;
        [SerializeField] private int numberOfTestCubes = 3;
        
        private void Start()
        {
            // Run automatic setup check
            CheckSetupStatus();
            
            if (!step1_LayersSetup)
            {
                Debug.LogWarning("STEP 1 REQUIRED: Set up layers using menu 'TabletopShop > Setup Interaction Layers'");
            }
            
            if (step1_LayersSetup && !step2_PlayerSetup)
            {
                Debug.Log("STEP 2: Setting up player automatically...");
                SetupPlayer();
            }
            
            if (step1_LayersSetup && step2_PlayerSetup && !step3_TestObjectsCreated && createTestCubes)
            {
                Debug.Log("STEP 3: Creating test objects...");
                CreateTestObjects();
            }
        }
        
        private void CheckSetupStatus()
        {
            // Check if layers are set up
            step1_LayersSetup = InteractionLayers.ValidateLayers();
            
            // Check if player is set up
            PlayerInteraction playerInteraction = FindFirstObjectByType<PlayerInteraction>();
            SimplePlayerController playerController = FindFirstObjectByType<SimplePlayerController>();
            Camera mainCamera = Camera.main;
            
            step2_PlayerSetup = (playerInteraction != null && playerController != null && mainCamera != null);
            
            // Check if test objects exist
            Product[] products = FindObjectsByType<Product>(FindObjectsSortMode.None);
            ShelfSlot[] slots = FindObjectsByType<ShelfSlot>(FindObjectsSortMode.None);
            step3_TestObjectsCreated = (products.Length > 0 || slots.Length > 0);
            
            // Log status
            Debug.Log($"Setup Status: Layers={step1_LayersSetup}, Player={step2_PlayerSetup}, TestObjects={step3_TestObjectsCreated}");
        }
        
        [ContextMenu("Force Setup Player")]
        public void SetupPlayer()
        {
            GameObject player = FindOrCreatePlayer();
            
            // Ensure PlayerInteraction component
            PlayerInteraction interaction = player.GetComponent<PlayerInteraction>();
            if (interaction == null)
            {
                interaction = player.AddComponent<PlayerInteraction>();
                Debug.Log("Added PlayerInteraction component");
            }
            
            // Ensure camera
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
            
            // Add debugger
            InteractionSystemDebugger debugger = player.GetComponent<InteractionSystemDebugger>();
            if (debugger == null)
            {
                debugger = player.AddComponent<InteractionSystemDebugger>();
                Debug.Log("Added interaction debugger (press F1 to open)");
            }
            
            step2_PlayerSetup = true;
            Debug.Log("<color=green>Player setup complete!</color>");
        }
        
        [ContextMenu("Create Test Objects")]
        public void CreateTestObjects()
        {
            Vector3[] positions = {
                new Vector3(-2, 1, 2),
                new Vector3(0, 1, 2),
                new Vector3(2, 1, 2)
            };
            
            for (int i = 0; i < numberOfTestCubes && i < positions.Length; i++)
            {
                CreateTestCube(i, positions[i]);
            }
            
            // Create a test shelf slot
            CreateTestShelfSlot();
            
            step3_TestObjectsCreated = true;
            Debug.Log("<color=green>Test objects created!</color>");
        }
        
        private GameObject FindOrCreatePlayer()
        {
            // Try to find existing player
            SimplePlayerController existingController = FindFirstObjectByType<SimplePlayerController>();
            if (existingController != null)
            {
                return existingController.gameObject;
            }
            
            PlayerInteraction existingInteraction = FindFirstObjectByType<PlayerInteraction>();
            if (existingInteraction != null)
            {
                return existingInteraction.gameObject;
            }
            
            // Create new player
            GameObject player = new GameObject("Player");
            player.transform.position = new Vector3(0, 1, -3);
            
            // Add player controller
            SimplePlayerController controller = player.AddComponent<SimplePlayerController>();
            
            Debug.Log("Created new player GameObject");
            return player;
        }
        
        private void CreateTestCube(int index, Vector3 position)
        {
            // Create cube
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.position = position;
            cube.name = $"TestProduct_{index + 1}";
            
            // Add random color
            Color[] colors = { Color.red, Color.blue, Color.green, Color.yellow, Color.magenta };
            Renderer renderer = cube.GetComponent<Renderer>();
            Material material = new Material(Shader.Find("Standard"));
            material.color = colors[index % colors.Length];
            renderer.material = material;
            
            // Add Product component
            Product product = cube.AddComponent<Product>();
            
            // Set layer
            InteractionLayers.SetProductLayer(cube);
            
            Debug.Log($"Created test cube: {cube.name} at {position}");
        }
        
        private void CreateTestShelfSlot()
        {
            // Create cylinder for shelf slot
            GameObject slotGO = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            slotGO.transform.position = new Vector3(4, 0.5f, 2);
            slotGO.transform.localScale = new Vector3(1, 0.1f, 1);
            slotGO.name = "TestShelfSlot";
            
            // Make it green to indicate it's a slot
            Renderer renderer = slotGO.GetComponent<Renderer>();
            Material slotMaterial = new Material(Shader.Find("Standard"));
            slotMaterial.color = Color.green;
            slotMaterial.SetFloat("_Metallic", 0.2f);
            renderer.material = slotMaterial;
            
            // Add ShelfSlot component
            ShelfSlot slot = slotGO.AddComponent<ShelfSlot>();
            
            // Set layer
            InteractionLayers.SetShelfLayer(slotGO);
            
            Debug.Log($"Created test shelf slot: {slotGO.name}");
        }
        
        [ContextMenu("Test Interaction System")]
        public void TestInteractionSystem()
        {
            Debug.Log("=== TESTING INTERACTION SYSTEM ===");
            
            CheckSetupStatus();
            
            if (!step1_LayersSetup)
            {
                Debug.LogError("❌ Layers not set up! Use menu: TabletopShop > Setup Interaction Layers");
                return;
            }
            
            if (!step2_PlayerSetup)
            {
                Debug.LogError("❌ Player not set up properly!");
                return;
            }
            
            if (!step3_TestObjectsCreated)
            {
                Debug.LogWarning("⚠️ No test objects found. Creating some...");
                CreateTestObjects();
            }
            
            // Test raycast
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                Ray ray = new Ray(mainCamera.transform.position, mainCamera.transform.forward);
                RaycastHit hit;
                
                LayerMask interactableLayer = InteractionLayers.AllInteractablesMask;
                bool hitSomething = Physics.Raycast(ray, out hit, 5f, interactableLayer);
                
                if (hitSomething)
                {
                    IInteractable interactable = hit.collider.GetComponent<IInteractable>();
                    if (interactable != null)
                    {
                        Debug.Log($"✅ Raycast test successful! Hit: {hit.collider.name} - {interactable.InteractionText}");
                    }
                    else
                    {
                        Debug.LogWarning($"⚠️ Hit object {hit.collider.name} but no IInteractable component found");
                    }
                }
                else
                {
                    Debug.Log("ℹ️ Raycast didn't hit any interactable objects. Try looking at the test cubes.");
                }
            }
            
            
            Debug.Log("=== TEST COMPLETE ===");
            Debug.Log("Instructions:");
            Debug.Log("1. Move with WASD keys");
            Debug.Log("2. Look around with mouse");
            Debug.Log("3. Look at the colored cubes or green cylinder");
            Debug.Log("4. Press E to interact when crosshair changes color");
            Debug.Log("5. Press F1 to open debug window");
        }
    }
}
