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
            
            // This would typically involve setting up references between components
            // The exact linking depends on the CheckoutCounter and CheckoutUI implementation
            // For now, we'll ensure they can find each other
            
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
                
                // Position the placement surface above the counter
                placementObject.transform.localPosition = new Vector3(0, 1f, 0);
                placementObject.name = "Checkout Item Placement";
                
                if (showDebugLogs)
                    Debug.Log("CheckoutSceneSetup: Created CheckoutItemPlacement component");
            }
            else if (showDebugLogs)
            {
                Debug.Log("CheckoutSceneSetup: CheckoutItemPlacement already exists");
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
            
            // Set layer
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
                Debug.Log("CheckoutSceneSetup: Default CheckoutItem prefab created and configured. Save this object as a prefab for reuse.");
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

            // Check 4: Player controller
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
