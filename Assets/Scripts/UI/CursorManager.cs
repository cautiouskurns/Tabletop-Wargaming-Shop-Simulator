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
        private PlayerController playerController;
        private InventoryUI inventoryUI;
        
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
            
            // Set initial cursor state
            SetCursorState(startWithCursorLocked);
        }
        
        private void Update()
        {
            // Toggle cursor with Escape key
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
                // Lock cursor for first-person movement
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                
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
                
                // Disable player mouse look to prevent camera jitter
                if (playerController != null)
                {
                    playerController.SetMouseLookEnabled(false);
                }
            }
            
            Debug.Log($"Cursor {(locked ? "locked" : "unlocked")} for {(locked ? "movement" : "UI interaction")}");
        }
        
        /// <summary>
        /// Check if cursor is currently locked
        /// </summary>
        public bool IsCursorLocked => isCursorLocked;
    }
}
