using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

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
        private PauseMenuManager pauseMenuManager;
        
        private void Awake()
        {
            // Set initial cursor state - this will be properly applied in Start()
            isCursorLocked = startWithCursorLocked;
            
            Debug.Log($"CursorManager Awake: cursor will be {(startWithCursorLocked ? "locked" : "unlocked")}");
        }
        
        private void Start()
        {
#if UNITY_EDITOR
            // In editor, immediately simulate a click to enable cursor locking
            var gameWindow = EditorWindow
                .GetWindow(typeof(EditorWindow).Assembly.GetType("UnityEditor.GameView"));
            if (gameWindow != null)
            {
                gameWindow.Focus();
                gameWindow.SendEvent(new Event
                {
                    button = 0,
                    clickCount = 1,
                    type = EventType.MouseDown,
                    mousePosition = gameWindow.rootVisualElement.contentRect.center
                });
            }
#endif
            
            // Find player controller to communicate with
            playerController = FindAnyObjectByType<PlayerController>();
            if (playerController == null)
            {
                Debug.LogWarning("CursorManager: No PlayerController found! Mouse look control will not work properly.");
            }
            
            // Find inventory UI for integration
            inventoryUI = FindAnyObjectByType<InventoryUI>();
            
            // Find pause menu manager for integration
            pauseMenuManager = FindAnyObjectByType<PauseMenuManager>();
            
            // Apply initial cursor state properly
            ForceImmediateCursorState(isCursorLocked);
            hasInitialized = true;
            
            Debug.Log($"CursorManager initialized - cursor {(isCursorLocked ? "locked" : "unlocked")}");
        }
        
        private void LateUpdate()
        {
            // Only enforce cursor state if it has drifted from our intended state
            if (hasInitialized)
            {
                EnforceCursorState();
            }
        }
        
        private void Update()
        {
            // Only handle escape key for cursor toggle when game is not paused
            // If there's no pause manager, allow cursor toggle
            bool shouldHandleEscapeKey = pauseMenuManager == null || !pauseMenuManager.IsPaused;
            
            // Toggle cursor with Escape key - make this more immediate and reliable
            if (Input.GetKeyDown(toggleCursorKey) && shouldHandleEscapeKey)
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
        /// Enforce the current cursor state only if it has drifted from our intended state
        /// </summary>
        private void EnforceCursorState()
        {
            if (isCursorLocked)
            {
                // Only enforce if cursor state has drifted
                if (Cursor.lockState != CursorLockMode.Locked || Cursor.visible)
                {
                    Cursor.visible = false;
                    Cursor.lockState = CursorLockMode.Locked;
                    Debug.Log("CursorManager: Enforced locked cursor state (state had drifted)");
                }
            }
            else
            {
                // Only enforce if cursor state has drifted
                if (Cursor.lockState != CursorLockMode.None || !Cursor.visible)
                {
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                    Debug.Log("CursorManager: Enforced unlocked cursor state (state had drifted)");
                }
            }
        }
        
        /// <summary>
        /// Toggle between locked and unlocked cursor state
        /// </summary>
        public void ToggleCursor()
        {
            bool newLockState = !isCursorLocked;
            
            if (newLockState)
            {
                // When locking, use force immediate to bypass Unity's click requirement
                ForceImmediateCursorState(true);
            }
            else
            {
                // When unlocking, normal method is fine
                SetCursorState(false);
            }
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
                // Proper Unity cursor lock sequence - visibility first, then lock state
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
                
                // Enable player mouse look
                if (playerController != null)
                {
                    playerController.SetMouseLookEnabled(true);
                }
            }
            else
            {
                // Proper Unity cursor unlock sequence - lock state first, then visibility
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                
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
        /// Force immediate cursor state change, bypassing Unity's click requirement
        /// </summary>
        /// <param name="locked">True to lock cursor, false to unlock</param>
        private void ForceImmediateCursorState(bool locked)
        {
            isCursorLocked = locked;
            
            if (locked)
            {
                // Multiple attempts to force cursor lock immediately
                StartCoroutine(ForceImmediateLockCoroutine());
                
                // Enable player mouse look immediately
                if (playerController != null)
                {
                    playerController.SetMouseLookEnabled(true);
                }
            }
            else
            {
                // Unlock is immediate
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                
                // Disable player mouse look
                if (playerController != null)
                {
                    playerController.SetMouseLookEnabled(false);
                }
            }
            
            Debug.Log($"CursorManager: Force immediate cursor {(locked ? "locked" : "unlocked")}");
        }
        
        /// <summary>
        /// Coroutine to force cursor lock over multiple frames to overcome Unity's limitations
        /// </summary>
        private System.Collections.IEnumerator ForceImmediateLockCoroutine()
        {
#if UNITY_EDITOR
            // In editor, simulate a click on the game window to enable cursor locking
            var gameWindow = EditorWindow
                .GetWindow(typeof(EditorWindow).Assembly.GetType("UnityEditor.GameView"));
            if (gameWindow != null)
            {
                gameWindow.Focus();
                gameWindow.SendEvent(new Event
                {
                    button = 0,
                    clickCount = 1,
                    type = EventType.MouseDown,
                    mousePosition = gameWindow.rootVisualElement.contentRect.center
                });
            }
#endif
            
            // Try multiple times over several frames to ensure lock takes effect
            for (int i = 0; i < 10; i++)
            {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
                
                // Wait a frame
                yield return null;
                
                // Check if it worked
                if (Cursor.lockState == CursorLockMode.Locked && !Cursor.visible)
                {
                    Debug.Log($"CursorManager: Cursor locked successfully on attempt {i + 1}");
                    break;
                }
            }
        }
        
        /// <summary>
        /// Check if cursor is currently locked
        /// </summary>
        public bool IsCursorLocked => isCursorLocked;
    }
}
