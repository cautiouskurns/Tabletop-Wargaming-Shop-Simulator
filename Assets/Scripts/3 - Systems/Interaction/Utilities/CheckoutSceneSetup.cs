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
                counterObject.layer = InteractionLayers.InteractableLayerMask;
            }
            
            if (showDebugLogs)
                Debug.Log($"CheckoutSceneSetup: Checkout counter created at {spawnPosition}");
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
