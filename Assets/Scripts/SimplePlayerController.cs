using UnityEngine;

namespace TabletopShop
{
    /// <summary>
    /// Simple first-person player controller for the shop environment
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class SimplePlayerController : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float walkSpeed = 5f;
        [SerializeField] private float runSpeed = 8f;
        [SerializeField] private float jumpHeight = 1.5f;
        [SerializeField] private float gravity = -9.81f;
        
        [Header("Mouse Look Settings")]
        [SerializeField] private float mouseSensitivity = 2f;
        [SerializeField] private float verticalLookLimit = 80f;
        [SerializeField] private bool invertYAxis = false;
        
        [Header("Camera")]
        [SerializeField] private Camera playerCamera;
        [SerializeField] private float cameraHeight = 1.8f;
        
        // Components
        private CharacterController controller;
        private PlayerInteraction playerInteraction;
        
        // Movement
        private Vector3 velocity;
        private bool isGrounded;
        private float currentSpeed;
        
        // Mouse look
        private float xRotation = 0f;
        private float yRotation = 0f;
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            // Get components
            controller = GetComponent<CharacterController>();
            playerInteraction = GetComponent<PlayerInteraction>();
            
            // Setup camera if not assigned
            if (playerCamera == null)
            {
                SetupCamera();
            }
            
            // Lock cursor
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        
        private void Update()
        {
            HandleMouseLook();
            HandleMovement();
            HandleInput();
        }
        
        #endregion
        
        #region Movement
        
        /// <summary>
        /// Handle player movement input and physics
        /// </summary>
        private void HandleMovement()
        {
            // Ground check
            isGrounded = controller.isGrounded;
            if (isGrounded && velocity.y < 0)
            {
                velocity.y = -2f; // Small downward force to keep grounded
            }
            
            // Get input
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");
            
            // Calculate movement direction
            Vector3 direction = transform.right * horizontal + transform.forward * vertical;
            
            // Determine speed (walk or run)
            bool isRunning = Input.GetKey(KeyCode.LeftShift);
            currentSpeed = isRunning ? runSpeed : walkSpeed;
            
            // Apply movement
            controller.Move(direction * currentSpeed * Time.deltaTime);
            
            // Handle jumping
            if (Input.GetButtonDown("Jump") && isGrounded)
            {
                velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }
            
            // Apply gravity
            velocity.y += gravity * Time.deltaTime;
            controller.Move(velocity * Time.deltaTime);
        }
        
        #endregion
        
        #region Mouse Look
        
        /// <summary>
        /// Handle mouse look for camera rotation
        /// </summary>
        private void HandleMouseLook()
        {
            // Get mouse input
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
            
            // Apply invert Y axis if enabled
            if (invertYAxis)
                mouseY = -mouseY;
            
            // Horizontal rotation (Y-axis) - rotate the player body
            yRotation += mouseX;
            transform.rotation = Quaternion.Euler(0f, yRotation, 0f);
            
            // Vertical rotation (X-axis) - rotate the camera
            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -verticalLookLimit, verticalLookLimit);
            
            if (playerCamera != null)
            {
                playerCamera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
            }
        }
        
        #endregion
        
        #region Input Handling
        
        /// <summary>
        /// Handle other input like cursor lock/unlock
        /// </summary>
        private void HandleInput()
        {
            // Toggle cursor lock with Escape
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                ToggleCursorLock();
            }
        }
        
        /// <summary>
        /// Toggle cursor lock state
        /// </summary>
        private void ToggleCursorLock()
        {
            if (Cursor.lockState == CursorLockMode.Locked)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
        
        #endregion
        
        #region Setup
        
        /// <summary>
        /// Create and setup the player camera
        /// </summary>
        private void SetupCamera()
        {
            // Create camera GameObject
            GameObject cameraObj = new GameObject("PlayerCamera");
            cameraObj.transform.SetParent(transform, false);
            cameraObj.transform.localPosition = new Vector3(0, cameraHeight, 0);
            
            // Add Camera component
            playerCamera = cameraObj.AddComponent<Camera>();
            playerCamera.fieldOfView = 75f;
            playerCamera.nearClipPlane = 0.1f;
            playerCamera.farClipPlane = 1000f;
            
            // Add AudioListener if there isn't one already
            if (FindObjectOfType<AudioListener>() == null)
            {
                cameraObj.AddComponent<AudioListener>();
            }
            
            // Setup PlayerInteraction if it exists
            if (playerInteraction != null)
            {
                // Use reflection to set the camera reference
                var cameraField = typeof(PlayerInteraction).GetField("playerCamera", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                cameraField?.SetValue(playerInteraction, playerCamera);
            }
            
            Debug.Log("Created player camera");
        }
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// Set movement speed
        /// </summary>
        /// <param name="walkSpeed">Walking speed</param>
        /// <param name="runSpeed">Running speed</param>
        public void SetMovementSpeed(float walkSpeed, float runSpeed)
        {
            this.walkSpeed = walkSpeed;
            this.runSpeed = runSpeed;
        }
        
        /// <summary>
        /// Set mouse sensitivity
        /// </summary>
        /// <param name="sensitivity">Mouse sensitivity multiplier</param>
        public void SetMouseSensitivity(float sensitivity)
        {
            mouseSensitivity = sensitivity;
        }
        
        /// <summary>
        /// Get the player camera
        /// </summary>
        /// <returns>The player camera component</returns>
        public Camera GetCamera()
        {
            return playerCamera;
        }
        
        /// <summary>
        /// Check if player is currently moving
        /// </summary>
        /// <returns>True if player is moving</returns>
        public bool IsMoving()
        {
            Vector3 horizontalVelocity = new Vector3(controller.velocity.x, 0, controller.velocity.z);
            return horizontalVelocity.magnitude > 0.1f;
        }
        
        #endregion
    }
}
