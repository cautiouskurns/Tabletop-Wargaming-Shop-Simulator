using UnityEngine;

namespace TabletopShop
{
    /// <summary>
    /// Scene setup helper for configuring checkout system components
    /// Provides automated setup and validation for checkout counters and UI elements
    /// </summary>
    public class CheckoutSceneSetup : MonoBehaviour
    {
        [Header("Checkout Prefabs")]
        [SerializeField] private CheckoutCounter checkoutCounterPrefab;
        [SerializeField] private CheckoutUI checkoutUIPrefab;
        [SerializeField] private GameObject checkoutItemPlacementPrefab;
        
        [Header("Spawn Configuration")]
        [SerializeField] private Transform checkoutSpawnPoint;
        [SerializeField] private Transform uiParentTransform;
        [SerializeField] private Vector3 checkoutCounterOffset = Vector3.zero;
        [SerializeField] private Vector3 checkoutUIOffset = new Vector3(0, 2, 0);
        
        [Header("Setup Options")]
        [SerializeField] private bool autoSetupOnStart = false;
        [SerializeField] private bool createCheckoutParentObject = true;
        [SerializeField] private string checkoutParentName = "Checkout System";
        
        [Header("Validation")]
        [SerializeField] private bool validateSetupOnStart = true;
        [SerializeField] private bool showDebugLogs = true;
        
        // Runtime references
        private CheckoutCounter spawnedCheckoutCounter;
        private CheckoutUI spawnedCheckoutUI;
        private GameObject checkoutSystemParent;
        
        #region Unity Lifecycle
        
        private void Start()
        {
            if (validateSetupOnStart)
            {
                ValidateCheckoutSetup();
            }
            
            if (autoSetupOnStart)
            {
                SetupCheckoutSystem();
            }
        }
        
        #endregion
        
        #region Context Menu Methods
        
        [ContextMenu("Setup Checkout System")]
        public void SetupCheckoutSystem()
        {
            if (showDebugLogs)
                Debug.Log("CheckoutSceneSetup: Setting up checkout system...");
            
            // Create parent object if needed
            if (createCheckoutParentObject)
            {
                CreateCheckoutParent();
            }
            
            // Setup checkout counter
            SetupCheckoutCounter();
            
            // Setup item placement system
            SetupCheckoutItemPlacement();
            
            // Configure checkout UI
            ConfigureCheckoutUI();
            
            // Link components together
            LinkCheckoutComponents();
            
            // Final validation
            ValidateCheckoutSetup();
            
            if (showDebugLogs)
                Debug.Log("CheckoutSceneSetup: Checkout system setup completed!");
        }
        
        [ContextMenu("Validate Checkout Setup")]
        public void ValidateCheckoutSetup()
        {
            bool isValid = true;
            
            if (showDebugLogs)
                Debug.Log("CheckoutSceneSetup: Validating checkout setup...");
            
            // Check prefab references
            if (checkoutCounterPrefab == null)
            {
                Debug.LogError("CheckoutSceneSetup: CheckoutCounter prefab is not assigned!");
                isValid = false;
            }
            
            if (checkoutUIPrefab == null)
            {
                Debug.LogError("CheckoutSceneSetup: CheckoutUI prefab is not assigned!");
                isValid = false;
            }
            
            // Check spawn point
            if (checkoutSpawnPoint == null)
            {
                Debug.LogWarning("CheckoutSceneSetup: No checkout spawn point assigned. Will use this object's position.");
            }
            
            // Check for existing checkout components in scene
            CheckoutCounter existingCounter = FindAnyObjectByType<CheckoutCounter>();
            CheckoutUI existingUI = FindAnyObjectByType<CheckoutUI>();
            
            if (existingCounter != null)
            {
                if (showDebugLogs)
                    Debug.Log($"CheckoutSceneSetup: Found existing CheckoutCounter: {existingCounter.name}");
                spawnedCheckoutCounter = existingCounter;
            }
            
            if (existingUI != null)
            {
                if (showDebugLogs)
                    Debug.Log($"CheckoutSceneSetup: Found existing CheckoutUI: {existingUI.name}");
                spawnedCheckoutUI = existingUI;
            }
            
            // Check UI Canvas
            Canvas uiCanvas = FindAnyObjectByType<Canvas>();
            if (uiCanvas == null)
            {
                Debug.LogWarning("CheckoutSceneSetup: No Canvas found in scene. UI components may not display correctly.");
            }
            else if (uiParentTransform == null)
            {
                uiParentTransform = uiCanvas.transform;
                if (showDebugLogs)
                    Debug.Log("CheckoutSceneSetup: Using main Canvas as UI parent.");
            }
            
            // Check player interaction system
            CheckoutPlayerController playerController = FindAnyObjectByType<CheckoutPlayerController>();
            if (playerController == null)
            {
                PlayerInteraction playerInteraction = FindAnyObjectByType<PlayerInteraction>();
                if (playerInteraction == null)
                {
                    Debug.LogWarning("CheckoutSceneSetup: No player interaction system found. Players may not be able to interact with checkout.");
                }
                else
                {
                    Debug.LogWarning("CheckoutSceneSetup: Found PlayerInteraction but no CheckoutPlayerController. Consider using CheckoutPlayerController for full checkout functionality.");
                }
            }
            
            if (isValid && showDebugLogs)
            {
                Debug.Log("CheckoutSceneSetup: All required components are properly configured!");
            }
        }
        
        [ContextMenu("Clear Checkout System")]
        public void ClearCheckoutSystem()
        {
            if (showDebugLogs)
                Debug.Log("CheckoutSceneSetup: Clearing checkout system...");
            
            // Remove spawned components
            if (spawnedCheckoutCounter != null)
            {
                DestroyImmediate(spawnedCheckoutCounter.gameObject);
                spawnedCheckoutCounter = null;
            }
            
            if (spawnedCheckoutUI != null)
            {
                DestroyImmediate(spawnedCheckoutUI.gameObject);
                spawnedCheckoutUI = null;
            }
            
            if (checkoutSystemParent != null)
            {
                DestroyImmediate(checkoutSystemParent);
                checkoutSystemParent = null;
            }
            
            if (showDebugLogs)
                Debug.Log("CheckoutSceneSetup: Checkout system cleared!");
        }
        
        #endregion
        
        #region Setup Methods
        
        /// <summary>
        /// Creates and configures the checkout counter in the scene
        /// </summary>
        public void SetupCheckoutCounter()
        {
            if (checkoutCounterPrefab == null)
            {
                Debug.LogError("CheckoutSceneSetup: Cannot setup checkout counter - prefab is null!");
                return;
            }
            
            // Determine spawn position
            Vector3 spawnPosition = (checkoutSpawnPoint != null) ? 
                checkoutSpawnPoint.position + checkoutCounterOffset : 
                transform.position + checkoutCounterOffset;
            
            // Determine spawn rotation
            Quaternion spawnRotation = (checkoutSpawnPoint != null) ? 
                checkoutSpawnPoint.rotation : 
                transform.rotation;
            
            // Instantiate checkout counter
            GameObject counterObject = Instantiate(checkoutCounterPrefab.gameObject, spawnPosition, spawnRotation);
            spawnedCheckoutCounter = counterObject.GetComponent<CheckoutCounter>();
            
            // Set parent if needed
            if (checkoutSystemParent != null)
            {
                counterObject.transform.SetParent(checkoutSystemParent.transform);
            }
            
            // Configure counter name
            counterObject.name = "Checkout Counter";
            
            // Ensure counter is on proper layer
            if (counterObject.layer == 0) // Default layer
            {
                InteractionLayers.SetInteractableLayer(counterObject);
            }
            
            if (showDebugLogs)
                Debug.Log($"CheckoutSceneSetup: Checkout counter created at {spawnPosition}");
            
            // Setup item placement for the checkout counter
            SetupCheckoutItemPlacement();
        }
        
        /// <summary>
        /// Creates and links checkout UI elements
        /// </summary>
        public void ConfigureCheckoutUI()
        {
            if (checkoutUIPrefab == null)
            {
                Debug.LogError("CheckoutSceneSetup: Cannot configure checkout UI - prefab is null!");
                return;
            }
            
            // Determine UI parent
            Transform uiParent = uiParentTransform;
            if (uiParent == null)
            {
                Canvas canvas = FindAnyObjectByType<Canvas>();
                if (canvas != null)
                {
                    uiParent = canvas.transform;
                }
                else
                {
                    Debug.LogError("CheckoutSceneSetup: No Canvas found for UI parent!");
                    return;
                }
            }
            
            // Instantiate checkout UI
            GameObject uiObject = Instantiate(checkoutUIPrefab.gameObject, uiParent);
            spawnedCheckoutUI = uiObject.GetComponent<CheckoutUI>();
            
            // Configure UI positioning
            RectTransform uiRectTransform = uiObject.GetComponent<RectTransform>();
            if (uiRectTransform != null)
            {
                // Position UI appropriately (you may want to adjust these values)
                uiRectTransform.anchorMin = new Vector2(0.7f, 0.7f);
                uiRectTransform.anchorMax = new Vector2(0.95f, 0.95f);
                uiRectTransform.offsetMin = Vector2.zero;
                uiRectTransform.offsetMax = Vector2.zero;
            }
            
            // Configure UI name
            uiObject.name = "Checkout UI";
            
            // Initially hide the UI
            if (spawnedCheckoutUI != null)
            {
                spawnedCheckoutUI.gameObject.SetActive(false);
            }
            
            if (showDebugLogs)
                Debug.Log("CheckoutSceneSetup: Checkout UI configured and linked to Canvas");
        }
        
        /// <summary>
        /// Creates parent object for checkout system organization
        /// </summary>
        private void CreateCheckoutParent()
        {
            if (checkoutSystemParent == null)
            {
                checkoutSystemParent = new GameObject(checkoutParentName);
                
                // Position parent at spawn point or this object's position
                if (checkoutSpawnPoint != null)
                {
                    checkoutSystemParent.transform.position = checkoutSpawnPoint.position;
                    checkoutSystemParent.transform.rotation = checkoutSpawnPoint.rotation;
                }
                else
                {
                    checkoutSystemParent.transform.position = transform.position;
                    checkoutSystemParent.transform.rotation = transform.rotation;
                }
                
                if (showDebugLogs)
                    Debug.Log($"CheckoutSceneSetup: Created checkout parent object: {checkoutParentName}");
            }
        }
        
        /// <summary>
        /// Links checkout components together for proper functionality
        /// </summary>
        private void LinkCheckoutComponents()
        {
            if (spawnedCheckoutCounter == null || spawnedCheckoutUI == null)
            {
                Debug.LogWarning("CheckoutSceneSetup: Cannot link components - counter or UI is missing!");
                return;
            }
            
            // Link the CheckoutUI to the CheckoutCounter using reflection
            var checkoutUIField = spawnedCheckoutCounter.GetType().GetField("checkoutUI", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (checkoutUIField != null)
            {
                checkoutUIField.SetValue(spawnedCheckoutCounter, spawnedCheckoutUI);
                if (showDebugLogs)
                    Debug.Log("CheckoutSceneSetup: Linked CheckoutUI to CheckoutCounter");
            }
            else
            {
                Debug.LogError("CheckoutSceneSetup: Could not find checkoutUI field in CheckoutCounter!");
            }
            
            // Set up the UI's association with the counter
            var counterField = spawnedCheckoutUI.GetType().GetField("associatedCounter", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (counterField != null)
            {
                counterField.SetValue(spawnedCheckoutUI, spawnedCheckoutCounter);
                if (showDebugLogs)
                    Debug.Log("CheckoutSceneSetup: Linked CheckoutCounter to CheckoutUI");
            }
            
            if (showDebugLogs)
                Debug.Log("CheckoutSceneSetup: Checkout components linked successfully");
        }
        
        /// <summary>
        /// Sets up CheckoutItemPlacement component for the checkout counter
        /// </summary>
        public void SetupCheckoutItemPlacement()
        {
            if (spawnedCheckoutCounter == null)
            {
                Debug.LogError("CheckoutSceneSetup: Cannot setup item placement - checkout counter not found!");
                return;
            }
            
            // Check if placement component already exists
            CheckoutItemPlacement placement = spawnedCheckoutCounter.GetComponentInChildren<CheckoutItemPlacement>();
            
            if (placement == null)
            {
                // Create placement object as child of checkout counter
                GameObject placementObject;
                
                if (checkoutItemPlacementPrefab != null)
                {
                    placementObject = Instantiate(checkoutItemPlacementPrefab, spawnedCheckoutCounter.transform);
                }
                else
                {
                    placementObject = new GameObject("Checkout Item Placement");
                    placementObject.transform.SetParent(spawnedCheckoutCounter.transform);
                    placement = placementObject.AddComponent<CheckoutItemPlacement>();
                }
                
                // Position the placement system above the counter
                placementObject.transform.localPosition = new Vector3(0, 1f, 0);
                placementObject.name = "Checkout Item Placement";
                
                // Create a placement surface transform
                GameObject surfaceObject = new GameObject("Placement Surface");
                surfaceObject.transform.SetParent(placementObject.transform);
                surfaceObject.transform.localPosition = Vector3.zero; // Same position as placement object
                
                // Configure the placement component to use this surface
                if (placement != null)
                {
                    var surfaceField = placement.GetType().GetField("placementSurface", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (surfaceField != null)
                    {
                        surfaceField.SetValue(placement, surfaceObject.transform);
                    }
                }
                
                if (showDebugLogs)
                    Debug.Log("CheckoutSceneSetup: Created CheckoutItemPlacement component with placement surface");
            }
            else if (showDebugLogs)
            {
                Debug.Log("CheckoutSceneSetup: CheckoutItemPlacement already exists");
                
                // Check if it has a placement surface assigned
                var surfaceField = placement.GetType().GetField("placementSurface", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (surfaceField != null)
                {
                    Transform surface = (Transform)surfaceField.GetValue(placement);
                    if (surface == null)
                    {
                        Debug.LogWarning("CheckoutSceneSetup: CheckoutItemPlacement has no placement surface assigned!");
                    }
                }
            }
        }

        /// <summary>
        /// Creates a default CheckoutItem prefab for testing purposes
        /// </summary>
        [ContextMenu("Create Default CheckoutItem Prefab")]
        public void CreateDefaultCheckoutItemPrefab()
        {
            if (showDebugLogs)
                Debug.Log("CheckoutSceneSetup: Creating default CheckoutItem prefab...");

            // Create a basic cube as checkout item
            GameObject prefabObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            prefabObject.name = "DefaultCheckoutItem";
            prefabObject.transform.localScale = new Vector3(0.5f, 0.1f, 0.5f);
            
            // Add CheckoutItem component
            CheckoutItem checkoutItem = prefabObject.AddComponent<CheckoutItem>();
            
            // Create and assign a visible material
            MeshRenderer renderer = prefabObject.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                Material defaultMaterial = new Material(Shader.Find("Standard"));
                defaultMaterial.color = Color.white;
                defaultMaterial.name = "Default Checkout Item Material";
                renderer.material = defaultMaterial;
            }
            
            // Add basic visuals if ProductVisuals exists
            ProductVisuals visuals = prefabObject.GetComponent<ProductVisuals>();
            if (visuals == null)
            {
                // Try to add ProductVisuals if it exists in the project
                System.Type productVisualsType = System.Type.GetType("TabletopShop.ProductVisuals");
                if (productVisualsType != null)
                {
                    prefabObject.AddComponent(productVisualsType);
                }
            }
            
            // Ensure it has a collider for interaction
            Collider collider = prefabObject.GetComponent<Collider>();
            if (collider == null)
            {
                prefabObject.AddComponent<BoxCollider>();
            }
            
            // Set layer to Product layer for interaction
            InteractionLayers.SetProductLayer(prefabObject);
            
            // Automatically assign this prefab to any CheckoutItemPlacement found
            if (spawnedCheckoutCounter != null)
            {
                CheckoutItemPlacement placement = spawnedCheckoutCounter.GetComponentInChildren<CheckoutItemPlacement>();
                if (placement != null)
                {
                    // Use reflection to set the private field since it's serialized
                    var field = placement.GetType().GetField("checkoutItemPrefab", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (field != null)
                    {
                        field.SetValue(placement, prefabObject);
                        if (showDebugLogs)
                            Debug.Log("CheckoutSceneSetup: Assigned default prefab to CheckoutItemPlacement");
                    }
                }
            }
            
            if (showDebugLogs)
                Debug.Log("CheckoutSceneSetup: Default CheckoutItem prefab created with visible material and proper layer. Save this object as a prefab for reuse.");
        }

        /// <summary>
        /// Diagnoses common issues with the checkout system setup
        /// </summary>
        [ContextMenu("Diagnose Checkout Issues")]
        public void DiagnoseCheckoutIssues()
        {
            if (showDebugLogs)
                Debug.Log("=== CHECKOUT SYSTEM DIAGNOSIS ===");

            bool hasIssues = false;

            // Check 1: CheckoutCounter layer
            if (spawnedCheckoutCounter != null)
            {
                int currentLayer = spawnedCheckoutCounter.gameObject.layer;
                int interactableLayer = InteractionLayers.InteractableLayerIndex;
                
                if (currentLayer != interactableLayer)
                {
                    Debug.LogError($"ISSUE: CheckoutCounter is on layer {LayerMask.LayerToName(currentLayer)} but should be on layer {LayerMask.LayerToName(interactableLayer)}");
                    hasIssues = true;
                }
                else
                {
                    Debug.Log("✓ CheckoutCounter is on correct Interactable layer");
                }
            }
            else
            {
                Debug.LogError("ISSUE: No CheckoutCounter found in scene");
                hasIssues = true;
            }

            // Check 2: CheckoutUI status
            if (spawnedCheckoutUI != null)
            {
                if (!spawnedCheckoutUI.gameObject.activeInHierarchy)
                {
                    Debug.LogWarning("INFO: CheckoutUI is currently inactive (this is normal until a customer arrives)");
                }
                else
                {
                    Debug.Log("✓ CheckoutUI is active");
                }
                
                // Check if CheckoutCounter has reference to this UI
                if (spawnedCheckoutCounter != null)
                {
                    var checkoutUIField = spawnedCheckoutCounter.GetType().GetField("checkoutUI", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (checkoutUIField != null)
                    {
                        CheckoutUI linkedUI = (CheckoutUI)checkoutUIField.GetValue(spawnedCheckoutCounter);
                        if (linkedUI == null)
                        {
                            Debug.LogError("ISSUE: CheckoutCounter's checkoutUI field is null! UI won't show when customers arrive.");
                            Debug.LogWarning("FIX: Use 'Check UI References' context menu to fix this");
                            hasIssues = true;
                        }
                        else if (linkedUI != spawnedCheckoutUI)
                        {
                            Debug.LogWarning("WARNING: CheckoutCounter is linked to a different UI than the one we created");
                        }
                        else
                        {
                            Debug.Log("✓ CheckoutCounter properly linked to CheckoutUI");
                        }
                    }
                }
            }
            else
            {
                Debug.LogError("ISSUE: No CheckoutUI found in scene");
                hasIssues = true;
            }

            // Check 3: CheckoutItemPlacement setup
            if (spawnedCheckoutCounter != null)
            {
                CheckoutItemPlacement placement = spawnedCheckoutCounter.GetComponentInChildren<CheckoutItemPlacement>();
                if (placement == null)
                {
                    Debug.LogError("ISSUE: No CheckoutItemPlacement component found on CheckoutCounter or its children");
                    hasIssues = true;
                }
                else
                {
                    Debug.Log("✓ CheckoutItemPlacement component found");
                    
                    // Check if it has a prefab assigned
                    var field = placement.GetType().GetField("checkoutItemPrefab", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (field != null)
                    {
                        GameObject prefab = (GameObject)field.GetValue(placement);
                        if (prefab == null)
                        {
                            Debug.LogError("ISSUE: CheckoutItemPlacement has no checkoutItemPrefab assigned!");
                            Debug.LogWarning("FIX: Use 'Create Default CheckoutItem Prefab' context menu to create one");
                            hasIssues = true;
                        }
                        else
                        {
                            CheckoutItem checkoutItemComponent = prefab.GetComponent<CheckoutItem>();
                            if (checkoutItemComponent == null)
                            {
                                Debug.LogError("ISSUE: Assigned prefab doesn't have CheckoutItem component!");
                                hasIssues = true;
                            }
                            else
                            {
                                Debug.Log("✓ CheckoutItemPlacement has valid prefab with CheckoutItem component");
                            }
                        }
                    }
                }
            }

            // Check 4: Existing CheckoutItems in scene
            CheckoutItem[] existingItems = FindObjectsByType<CheckoutItem>(FindObjectsSortMode.None);
            if (existingItems.Length > 0)
            {
                int wrongLayerCount = 0;
                int missingMaterialCount = 0;
                int missingColliderCount = 0;
                
                foreach (CheckoutItem item in existingItems)
                {
                    if (item != null)
                    {
                        // Check layer
                        if (item.gameObject.layer != InteractionLayers.ProductLayerIndex)
                        {
                            wrongLayerCount++;
                        }
                        
                        // Check material
                        MeshRenderer renderer = item.GetComponent<MeshRenderer>();
                        if (renderer == null || renderer.material == null || renderer.material.name.Contains("Default-Material"))
                        {
                            missingMaterialCount++;
                        }
                        
                        // Check collider
                        if (item.GetComponent<Collider>() == null)
                        {
                            missingColliderCount++;
                        }
                    }
                }
                
                if (wrongLayerCount > 0)
                {
                    Debug.LogError($"ISSUE: {wrongLayerCount} CheckoutItems are on wrong layer (should be Product layer)!");
                    Debug.LogWarning("FIX: Use 'Fix All Checkout Item Issues' context menu");
                    hasIssues = true;
                }
                
                if (missingMaterialCount > 0)
                {
                    Debug.LogError($"ISSUE: {missingMaterialCount} CheckoutItems have missing or default materials (invisible)!");
                    Debug.LogWarning("FIX: Use 'Fix All Checkout Item Issues' context menu");
                    hasIssues = true;
                }
                
                if (missingColliderCount > 0)
                {
                    Debug.LogError($"ISSUE: {missingColliderCount} CheckoutItems have no collider (can't be interacted with)!");
                    Debug.LogWarning("FIX: Use 'Fix All Checkout Item Issues' context menu");
                    hasIssues = true;
                }
                
                if (wrongLayerCount == 0 && missingMaterialCount == 0 && missingColliderCount == 0)
                {
                    Debug.Log($"✓ All {existingItems.Length} CheckoutItems have proper layers, materials, and colliders");
                }
            }
            else
            {
                Debug.LogWarning("INFO: No CheckoutItems found in scene. Place some items to test interaction.");
            }

            // Check 5: Player controller
            CheckoutPlayerController playerController = FindAnyObjectByType<CheckoutPlayerController>();
            if (playerController == null)
            {
                PlayerInteraction playerInteraction = FindAnyObjectByType<PlayerInteraction>();
                if (playerInteraction == null)
                {
                    Debug.LogError("ISSUE: No player interaction system found!");
                    hasIssues = true;
                }
                else
                {
                    Debug.LogWarning("WARNING: Using PlayerInteraction instead of CheckoutPlayerController. Some checkout features may not work properly.");
                }
            }
            else
            {
                Debug.Log("✓ CheckoutPlayerController found");
            }

            // Check 5: Customer behavior
            CustomerBehavior customerBehavior = FindAnyObjectByType<CustomerBehavior>();
            if (customerBehavior == null)
            {
                Debug.LogWarning("INFO: No CustomerBehavior found. Add customers to test checkout functionality.");
            }
            else
            {
                Debug.Log("✓ CustomerBehavior found");
            }

            // Summary
            if (!hasIssues)
            {
                Debug.Log("=== ✅ ALL CHECKS PASSED! Checkout system should work properly ===");
            }
            else
            {
                Debug.Log("=== ❌ ISSUES FOUND! Fix the above problems to resolve checkout functionality ===");
            }
        }

        [ContextMenu("Fix Checkout Counter Layer")]
        public void FixCheckoutCounterLayer()
        {
            if (spawnedCheckoutCounter != null)
            {
                InteractionLayers.SetInteractableLayer(spawnedCheckoutCounter.gameObject);
                if (showDebugLogs)
                    Debug.Log("CheckoutSceneSetup: Fixed CheckoutCounter layer to Interactable");
            }
            else
            {
                // Try to find any CheckoutCounter in the scene
                CheckoutCounter counter = FindAnyObjectByType<CheckoutCounter>();
                if (counter != null)
                {
                    InteractionLayers.SetInteractableLayer(counter.gameObject);
                    if (showDebugLogs)
                        Debug.Log("CheckoutSceneSetup: Fixed CheckoutCounter layer to Interactable");
                }
                else
                {
                    Debug.LogWarning("CheckoutSceneSetup: No CheckoutCounter found to fix");
                }
            }
        }

        [ContextMenu("Force Show Checkout UI")]
        public void ForceShowCheckoutUI()
        {
            if (spawnedCheckoutUI != null)
            {
                spawnedCheckoutUI.gameObject.SetActive(true);
                if (showDebugLogs)
                    Debug.Log("CheckoutSceneSetup: Forced CheckoutUI to show");
            }
            else
            {
                CheckoutUI ui = FindAnyObjectByType<CheckoutUI>();
                if (ui != null)
                {
                    ui.gameObject.SetActive(true);
                    if (showDebugLogs)
                        Debug.Log("CheckoutSceneSetup: Forced CheckoutUI to show");
                }
                else
                {
                    Debug.LogWarning("CheckoutSceneSetup: No CheckoutUI found");
                }
            }
        }

        /// <summary>
        /// Debug method to test checkout UI manually
        /// </summary>
        [ContextMenu("Test Customer Arrival")]
        public void TestCustomerArrival()
        {
            if (spawnedCheckoutCounter == null)
            {
                Debug.LogError("No CheckoutCounter found. Run setup first.");
                return;
            }

            // Create a mock customer for testing
            GameObject testCustomerObj = new GameObject("Test Customer");
            Customer testCustomer = testCustomerObj.AddComponent<Customer>();
            
            if (showDebugLogs)
                Debug.Log("Testing customer arrival...");
            
            // Call OnCustomerArrival directly
            spawnedCheckoutCounter.OnCustomerArrival(testCustomer);
            
            if (showDebugLogs)
                Debug.Log("Customer arrival test completed. Check if UI appeared.");
                
            // Clean up test customer after a delay
            UnityEngine.Object.Destroy(testCustomerObj, 2f);
        }

        [ContextMenu("Check UI References")]
        public void CheckUIReferences()
        {
            if (spawnedCheckoutCounter == null)
            {
                Debug.LogError("No CheckoutCounter found. Run setup first.");
                return;
            }

            // Use reflection to check the checkoutUI field
            var field = spawnedCheckoutCounter.GetType().GetField("checkoutUI", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                CheckoutUI ui = (CheckoutUI)field.GetValue(spawnedCheckoutCounter);
                if (ui == null)
                {
                    Debug.LogError("ISSUE: CheckoutCounter's checkoutUI field is null!");
                    Debug.LogWarning("FIX: The UI reference needs to be linked. Attempting to fix...");
                    
                    // Try to find and link the UI
                    CheckoutUI foundUI = FindAnyObjectByType<CheckoutUI>();
                    if (foundUI != null)
                    {
                        field.SetValue(spawnedCheckoutCounter, foundUI);
                        Debug.Log("✓ Fixed: Linked CheckoutUI to CheckoutCounter");
                    }
                    else
                    {
                        Debug.LogError("No CheckoutUI found in scene to link!");
                    }
                }
                else
                {
                    Debug.Log($"✓ CheckoutCounter has UI reference: {ui.name}");
                    Debug.Log($"UI GameObject active: {ui.gameObject.activeInHierarchy}");
                    Debug.Log($"UI Canvas Group: {ui.GetComponent<CanvasGroup>() != null}");
                }
            }
            else
            {
                Debug.LogError("Could not find checkoutUI field in CheckoutCounter");
            }
        }

        [ContextMenu("Fix UI Reference Link")]
        public void FixUIReferenceLink()
        {
            if (spawnedCheckoutCounter == null)
            {
                Debug.LogError("No CheckoutCounter found. Run setup first.");
                return;
            }
            
            CheckoutUI uiToLink = spawnedCheckoutUI;
            if (uiToLink == null)
            {
                uiToLink = FindAnyObjectByType<CheckoutUI>();
            }
            
            if (uiToLink == null)
            {
                Debug.LogError("No CheckoutUI found to link!");
                return;
            }
            
            // Link the UI reference
            var checkoutUIField = spawnedCheckoutCounter.GetType().GetField("checkoutUI", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (checkoutUIField != null)
            {
                checkoutUIField.SetValue(spawnedCheckoutCounter, uiToLink);
                if (showDebugLogs)
                    Debug.Log("✅ Fixed: CheckoutUI reference linked to CheckoutCounter");
            }
            else
            {
                Debug.LogError("Could not find checkoutUI field in CheckoutCounter");
            }
        }

        /// <summary>
        /// Creates a visual representation of the placement surface for easier setup
        /// </summary>
        [ContextMenu("Create Visual Placement Surface")]
        public void CreateVisualPlacementSurface()
        {
            if (spawnedCheckoutCounter == null)
            {
                Debug.LogError("No CheckoutCounter found. Run setup first.");
                return;
            }

            CheckoutItemPlacement placement = spawnedCheckoutCounter.GetComponentInChildren<CheckoutItemPlacement>();
            if (placement == null)
            {
                Debug.LogError("No CheckoutItemPlacement found. Run SetupCheckoutItemPlacement first.");
                return;
            }

            // Create a visual representation of the placement surface
            GameObject visualSurface = GameObject.CreatePrimitive(PrimitiveType.Plane);
            visualSurface.name = "Visual Placement Surface";
            visualSurface.transform.SetParent(placement.transform);
            
            // Position and scale the visual surface
            visualSurface.transform.localPosition = new Vector3(0, -0.05f, 0); // Slightly below items
            visualSurface.transform.localRotation = Quaternion.identity;
            
            // Scale to match grid size (default 3x2 with 1.5x1.0 spacing)
            float surfaceWidth = 3 * 1.5f; // gridSize.x * itemSpacing.x
            float surfaceDepth = 2 * 1.0f;  // gridSize.y * itemSpacing.y
            visualSurface.transform.localScale = new Vector3(surfaceWidth * 0.1f, 1f, surfaceDepth * 0.1f);
            
            // Make it semi-transparent and colored
            MeshRenderer renderer = visualSurface.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                Material surfaceMaterial = new Material(Shader.Find("Standard"));
                surfaceMaterial.color = new Color(0f, 1f, 0f, 0.3f); // Green, semi-transparent
                surfaceMaterial.SetFloat("_Mode", 3); // Transparent rendering mode
                surfaceMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                surfaceMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                surfaceMaterial.SetInt("_ZWrite", 0);
                surfaceMaterial.DisableKeyword("_ALPHATEST_ON");
                surfaceMaterial.EnableKeyword("_ALPHABLEND_ON");
                surfaceMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                surfaceMaterial.renderQueue = 3000;
                renderer.material = surfaceMaterial;
            }
            
            // Remove collider since it's just for visualization
            Collider collider = visualSurface.GetComponent<Collider>();
            if (collider != null)
            {
                DestroyImmediate(collider);
            }
            
            if (showDebugLogs)
                Debug.Log("CheckoutSceneSetup: Created visual placement surface. This shows where items will be placed.");
        }

        [ContextMenu("Fix UI Child Objects")]
        public void FixUIChildObjects()
        {
            CheckoutUI ui = spawnedCheckoutUI;
            if (ui == null)
            {
                ui = FindAnyObjectByType<CheckoutUI>();
            }
            
            if (ui == null)
            {
                Debug.LogError("No CheckoutUI found to fix!");
                return;
            }

            // Activate the main UI GameObject
            ui.gameObject.SetActive(true);
            
            // Find and activate the checkout panel
            var panelField = ui.GetType().GetField("checkoutPanel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (panelField != null)
            {
                GameObject panel = (GameObject)panelField.GetValue(ui);
                if (panel != null)
                {
                    panel.SetActive(true);
                    
                    // Activate all immediate children of the panel
                    for (int i = 0; i < panel.transform.childCount; i++)
                    {
                        Transform child = panel.transform.GetChild(i);
                        child.gameObject.SetActive(true);
                    }
                    
                    if (showDebugLogs)
                        Debug.Log($"Fixed CheckoutUI: Activated panel and {panel.transform.childCount} child objects");
                }
            }
            
            // Force call ShowUI to ensure proper initialization
            ui.ShowUI();
        }

        [ContextMenu("Fix All Checkout Item Issues")]
        public void FixAllCheckoutItemIssues()
        {
            if (showDebugLogs)
                Debug.Log("=== FIXING ALL CHECKOUT ITEM ISSUES ===");

            // Find all CheckoutItem objects in the scene
            CheckoutItem[] checkoutItems = FindObjectsByType<CheckoutItem>(FindObjectsSortMode.None);
            
            int fixedLayerCount = 0;
            int fixedMaterialCount = 0;
            
            foreach (CheckoutItem item in checkoutItems)
            {
                if (item != null)
                {
                    // Fix layer
                    if (item.gameObject.layer != InteractionLayers.ProductLayerIndex)
                    {
                        InteractionLayers.SetProductLayer(item.gameObject);
                        fixedLayerCount++;
                    }
                    
                    // Fix material
                    MeshRenderer renderer = item.GetComponent<MeshRenderer>();
                    if (renderer != null && (renderer.material == null || renderer.material.name.Contains("Default-Material")))
                    {
                        Material newMaterial = new Material(Shader.Find("Standard"));
                        newMaterial.color = item.IsScanned ? Color.green : Color.white;
                        newMaterial.name = "Fixed Checkout Item Material";
                        renderer.material = newMaterial;
                        fixedMaterialCount++;
                    }
                    
                    // Ensure it has a collider for interaction
                    Collider collider = item.GetComponent<Collider>();
                    if (collider == null)
                    {
                        item.gameObject.AddComponent<BoxCollider>();
                    }
                }
            }
            
            if (showDebugLogs)
            {
                Debug.Log($"✅ Fixed {fixedLayerCount} items with wrong layers");
                Debug.Log($"✅ Fixed {fixedMaterialCount} items with missing/default materials");
                Debug.Log($"✅ Processed {checkoutItems.Length} total checkout items");
            }
        }

        #endregion
        
        #region Public Getters
        
        public CheckoutCounter GetSpawnedCheckoutCounter() => spawnedCheckoutCounter;
        public CheckoutUI GetSpawnedCheckoutUI() => spawnedCheckoutUI;
        public GameObject GetCheckoutSystemParent() => checkoutSystemParent;
        
        #endregion
        
        #region Editor Helpers
        
        private void OnDrawGizmos()
        {
            // Draw spawn point indicator
            if (checkoutSpawnPoint != null)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawWireCube(checkoutSpawnPoint.position + checkoutCounterOffset, Vector3.one);
                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(checkoutSpawnPoint.position, checkoutSpawnPoint.position + checkoutSpawnPoint.forward * 2f);
            }
            else
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireCube(transform.position + checkoutCounterOffset, Vector3.one);
                Gizmos.color = Color.orange;
                Gizmos.DrawLine(transform.position, transform.position + transform.forward * 2f);
            }
        }
        
        private void OnDrawGizmosSelected()
        {
            // Draw detailed gizmos when selected
            if (checkoutSpawnPoint != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawCube(checkoutSpawnPoint.position + checkoutCounterOffset, Vector3.one * 0.1f);
            }
        }
        
        #endregion
    }
}
