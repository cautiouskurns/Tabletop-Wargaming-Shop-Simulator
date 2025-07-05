using UnityEngine;

namespace TabletopShop.Core
{
    /// <summary>
    /// Manages cursor visibility and lock state throughout the game
    /// </summary>
    public class CursorManager : MonoBehaviour
    {
        [Header("Cursor Settings")]
        [SerializeField] private bool hideCursorOnStart = true;
        [SerializeField] private bool lockCursorOnStart = true;
        [SerializeField] private CursorLockMode startLockMode = CursorLockMode.Locked;

        [Header("Debug")]
        [SerializeField] private bool debugMode = false;

        private static CursorManager instance;
        public static CursorManager Instance => instance;

        private void Awake()
        {
            // Singleton pattern
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            SetCursorState(hideCursorOnStart, lockCursorOnStart ? startLockMode : CursorLockMode.None);

            if (debugMode)
            {
                Debug.Log($"CursorManager: Initial state set - Visible: {Cursor.visible}, Lock: {Cursor.lockState}");
            }
        }

        private void Update()
        {
            if (debugMode)
            {
                // Debug output every 60 frames
                if (Time.frameCount % 60 == 0)
                {
                    Debug.Log($"Cursor State - Visible: {Cursor.visible}, Lock: {Cursor.lockState}");
                }
            }

            // Handle ESC key to unlock cursor (common for debugging)
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                ToggleCursor();
            }
        }

        /// <summary>
        /// Sets the cursor visibility and lock state
        /// </summary>
        public void SetCursorState(bool visible, CursorLockMode lockMode)
        {
            Cursor.visible = visible;
            Cursor.lockState = lockMode;

            if (debugMode)
            {
                Debug.Log($"CursorManager: Set cursor state - Visible: {visible}, Lock: {lockMode}");
            }
        }

        /// <summary>
        /// Hide and lock the cursor
        /// </summary>
        public void HideCursor()
        {
            SetCursorState(false, CursorLockMode.Locked);
        }

        /// <summary>
        /// Show and unlock the cursor
        /// </summary>
        public void ShowCursor()
        {
            SetCursorState(true, CursorLockMode.None);
        }

        /// <summary>
        /// Toggle between hidden/locked and visible/unlocked
        /// </summary>
        public void ToggleCursor()
        {
            if (Cursor.visible)
            {
                HideCursor();
            }
            else
            {
                ShowCursor();
            }
        }

    }
}