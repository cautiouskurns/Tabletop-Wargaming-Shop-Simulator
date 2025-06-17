using UnityEngine;

namespace TabletopShop
{
    /// <summary>
    /// Manages cursor state for seamless transition between first-person movement and UI interaction
    /// </summary>
    public class CursorManager : MonoBehaviour
    {
        [Header("Cursor Settings")]
        [SerializeField] private KeyCode toggleCursorKey = KeyCode.Escape;
        [SerializeField] private bool startWithCursorLocked = true;
        
        private bool isCursorLocked = true;
        private bool hasInitialized = false;
        private PlayerController playerController;
        private InventoryUI inventoryUI;
        
        private void Awake()
        {
            // Set cursor state as early as possible
            if (startWithCursorLocked)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                isCursorLocked = true;
            }
            else
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                isCursorLocked = false;
            }
            
            Debug.Log($"CursorManager Awake: cursor {(startWithCursorLocked ? "locked" : "unlocked")}");
        }
        
        private void Start()
        {
            // Find player controller to communicate with
            playerController = FindAnyObjectByType<PlayerController>();
            if (playerController == null)
            {
                Debug.LogWarning("CursorManager: No PlayerController found! Mouse look control will not work properly.");
            }
            
            // Find inventory UI for integration
            inventoryUI = FindAnyObjectByType<InventoryUI>();
            
            // Set initial cursor state - this should override any other cursor settings
            SetCursorState(startWithCursorLocked);
            hasInitialized = true;
            
            Debug.Log($"CursorManager initialized - cursor {(startWithCursorLocked ? "locked" : "unlocked")}");
        }
        
        private void LateUpdate()
        {
            // Ensure cursor state is maintained - this runs after all other Update() calls
            if (hasInitialized)
            {
                EnforceCursorState();
            }
        }
        
        private void Update()
        {
            // Toggle cursor with Escape key - make this more immediate and reliable
            if (Input.GetKeyDown(toggleCursorKey))
            {
                ToggleCursor();
            }
            
            // Auto-unlock cursor when inventory panel is shown
            if (inventoryUI != null)
            {
                // If inventory is visible and cursor is locked, unlock it
                if (Input.GetKeyDown(KeyCode.Tab) && isCursorLocked)
                {
                    SetCursorState(false);
                }
            }
        }
        /// <summary>
        /// Enforce the current cursor state to prevent other scripts from interfering
        /// </summary>
        private void EnforceCursorState()
        {
            if (isCursorLocked)
            {
                if (Cursor.lockState != CursorLockMode.Locked || Cursor.visible)
                {
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                    Debug.Log("CursorManager: Enforced locked cursor state");
                }
            }
            else
            {
                if (Cursor.lockState != CursorLockMode.None || !Cursor.visible)
                {
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                    Debug.Log("CursorManager: Enforced unlocked cursor state");
                }
            }
        }
        
        /// <summary>
        /// Toggle between locked and unlocked cursor state
        /// </summary>
        public void ToggleCursor()
        {
            SetCursorState(!isCursorLocked);
        }
        
        /// <summary>
        /// Set the cursor state
        /// </summary>
        /// <param name="locked">True to lock cursor for first-person movement, false for UI interaction</param>
        public void SetCursorState(bool locked)
        {
            isCursorLocked = locked;
            
            if (locked)
            {
                // Lock cursor for first-person movement - be forceful about it
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                
                // Force the state immediately to overcome any Unity delays
                UnityEngine.Cursor.lockState = CursorLockMode.Locked;
                UnityEngine.Cursor.visible = false;
                
                // Enable player mouse look
                if (playerController != null)
                {
                    playerController.SetMouseLookEnabled(true);
                }
            }
            else
            {
                // Unlock cursor for UI interaction
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                
                // Force the state immediately to overcome any Unity delays
                UnityEngine.Cursor.lockState = CursorLockMode.None;
                UnityEngine.Cursor.visible = true;
                
                // Disable player mouse look to prevent camera jitter
                if (playerController != null)
                {
                    playerController.SetMouseLookEnabled(false);
                }
            }
            
            Debug.Log($"CursorManager: Cursor {(locked ? "locked" : "unlocked")} for {(locked ? "movement" : "UI interaction")}");
        }
        
        /// <summary>
        /// Force the cursor to the current state immediately - useful for overriding other scripts
        /// </summary>
        public void ForceCursorState()
        {
            SetCursorState(isCursorLocked);
        }
        
        /// <summary>
        /// Check if cursor is currently locked
        /// </summary>
        public bool IsCursorLocked => isCursorLocked;
    }
}
