using UnityEngine;

namespace TabletopShop
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float movementSpeed = 5f;
        [SerializeField] private float jumpHeight = 2f;
        [SerializeField] private float gravity = -9.81f;
        
        [Header("Mouse Look Settings")]
        [SerializeField] private float mouseSensitivity = 2f;
        [SerializeField] private float verticalLookLimit = 80f;
        
        [Header("Ground Check")]
        [SerializeField] private Transform groundCheckPoint;
        [SerializeField] private float groundCheckRadius = 0.2f;
        [SerializeField] private LayerMask groundLayerMask = 1;
        
        [Header("Camera")]
        [SerializeField] private Camera playerCamera;
        
        // Private variables
        private CharacterController characterController;
        private Vector3 velocity;
        private float verticalRotation = 0f;
        private bool isGrounded;
        
        // Input variables
        private float horizontalInput;
        private float verticalInput;
        private float mouseX;
        private float mouseY;
        private bool jumpInput;
        
        // Mouse look control
        private bool mouseLookEnabled = true;
        
        void Start()
        {
            // Get required components
            characterController = GetComponent<CharacterController>();
            
            // Setup camera if not assigned
            if (playerCamera == null)
            {
                playerCamera = GetComponentInChildren<Camera>();
                if (playerCamera == null)
                {
                    // Create camera if none exists
                    GameObject cameraObject = new GameObject("Player Camera");
                    cameraObject.transform.SetParent(transform);
                    cameraObject.transform.localPosition = new Vector3(0, 1.8f, 0);
                    playerCamera = cameraObject.AddComponent<Camera>();
                }
            }
            
            // Setup ground check point if not assigned
            if (groundCheckPoint == null)
            {
                GameObject groundCheck = new GameObject("Ground Check");
                groundCheck.transform.SetParent(transform);
                groundCheck.transform.localPosition = new Vector3(0, -1f, 0);
                groundCheckPoint = groundCheck.transform;
            }
            
            // NOTE: Cursor management is now handled by CursorManager
            // Removed cursor locking to prevent conflicts
        }
        
        void Update()
        {
            HandleInput();
            HandleMouseLook();
            HandleMovement();
            HandleJumping();
        }
        
        private void HandleInput()
        {
            // Movement input
            horizontalInput = Input.GetAxis("Horizontal");
            verticalInput = Input.GetAxis("Vertical");
            
            // Mouse input (only if mouse look is enabled)
            if (mouseLookEnabled)
            {
                mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
                mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
            }
            else
            {
                mouseX = 0f;
                mouseY = 0f;
            }
            
            // Jump input
            jumpInput = Input.GetButtonDown("Jump");
            
            // NOTE: Cursor management is now handled by CursorManager
            // Removed Escape key handling to prevent conflicts
        }
        
        private void HandleMouseLook()
        {
            // Only process mouse look if enabled
            if (!mouseLookEnabled) return;
            
            // Horizontal rotation (Y-axis) - rotate the player body
            transform.Rotate(0, mouseX, 0);
            
            // Vertical rotation (X-axis) - rotate the camera
            verticalRotation -= mouseY;
            verticalRotation = Mathf.Clamp(verticalRotation, -verticalLookLimit, verticalLookLimit);
            
            if (playerCamera != null)
            {
                playerCamera.transform.localRotation = Quaternion.Euler(verticalRotation, 0, 0);
            }
        }
        
        private void HandleMovement()
        {
            // Ground check
            isGrounded = Physics.CheckSphere(groundCheckPoint.position, groundCheckRadius, groundLayerMask);
            
            // Reset vertical velocity when grounded
            if (isGrounded && velocity.y < 0)
            {
                velocity.y = -2f; // Small negative value to keep grounded
            }
            
            // Calculate movement direction
            Vector3 moveDirection = transform.right * horizontalInput + transform.forward * verticalInput;
            moveDirection = moveDirection.normalized * movementSpeed;
            
            // Apply movement
            characterController.Move(moveDirection * Time.deltaTime);
        }
        
        private void HandleJumping()
        {
            // Jump
            if (jumpInput && isGrounded)
            {
                velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }
            
            // Apply gravity
            velocity.y += gravity * Time.deltaTime;
            
            // Apply vertical movement
            characterController.Move(velocity * Time.deltaTime);
        }
        
        // Public methods for external access
        public void SetMovementSpeed(float speed)
        {
            movementSpeed = speed;
        }
        
        public void SetMouseSensitivity(float sensitivity)
        {
            mouseSensitivity = sensitivity;
        }
        
        public bool IsGrounded()
        {
            return isGrounded;
        }
        
        public Vector3 GetVelocity()
        {
            return velocity;
        }
        
        /// <summary>
        /// Enable or disable mouse look for camera rotation
        /// </summary>
        /// <param name="enabled">True to enable mouse look, false to disable</param>
        public void SetMouseLookEnabled(bool enabled)
        {
            mouseLookEnabled = enabled;
        }
        
        /// <summary>
        /// Check if mouse look is currently enabled
        /// </summary>
        /// <returns>True if mouse look is enabled</returns>
        public bool IsMouseLookEnabled()
        {
            return mouseLookEnabled;
        }
        
        // Gizmos for debugging
        private void OnDrawGizmosSelected()
        {
            if (groundCheckPoint != null)
            {
                Gizmos.color = isGrounded ? Color.green : Color.red;
                Gizmos.DrawWireSphere(groundCheckPoint.position, groundCheckRadius);
            }
        }
    }
}
