using UnityEngine;

namespace TabletopShop
{
    /// <summary>
    /// Simple animation controller for customer characters
    /// Handles basic movement animations based on NavMeshAgent velocity
    /// </summary>
    public class CustomerAnimationController : MonoBehaviour
    {
        [Header("Animation Parameters")]
        [SerializeField] private string walkSpeedParameter = "WalkSpeed";
        [SerializeField] private string isWalkingParameter = "IsWalking";
        [SerializeField] private float walkSpeedMultiplier = 1f;
        [SerializeField] private float walkThreshold = 0.1f;
        
        [Header("Animation Smoothing")]
        [SerializeField] private float animationSmoothTime = 0.1f;
        
        // Component references
        private Animator animator;
        private UnityEngine.AI.NavMeshAgent navMeshAgent;
        private CustomerMovement customerMovement;
        
        // Animation state
        private float currentAnimatedSpeed;
        private float velocitySmoothing;
        
        private void Awake()
        {
            // Get required components
            animator = GetComponent<Animator>();
            navMeshAgent = GetComponent<UnityEngine.AI.NavMeshAgent>();
            customerMovement = GetComponent<CustomerMovement>();
            
            if (animator == null)
            {
                Debug.LogWarning($"CustomerAnimationController on {name} could not find Animator component!");
            }
            
            if (navMeshAgent == null)
            {
                Debug.LogWarning($"CustomerAnimationController on {name} could not find NavMeshAgent component!");
            }
        }
        
        private void Update()
        {
            UpdateAnimations();
        }
        
        /// <summary>
        /// Update character animations based on movement
        /// </summary>
        private void UpdateAnimations()
        {
            if (animator == null) return;
            
            // Get current movement speed
            float currentSpeed = GetMovementSpeed();
            
            // Smooth the animation speed
            currentAnimatedSpeed = Mathf.SmoothDamp(
                currentAnimatedSpeed, 
                currentSpeed, 
                ref velocitySmoothing, 
                animationSmoothTime
            );
            
            // Set animation parameters
            SetAnimationParameters(currentAnimatedSpeed);
        }
        
        /// <summary>
        /// Get the current movement speed for animation
        /// </summary>
        private float GetMovementSpeed()
        {
            if (navMeshAgent != null && navMeshAgent.isActiveAndEnabled)
            {
                // Use NavMeshAgent velocity magnitude
                return navMeshAgent.velocity.magnitude;
            }
            else if (customerMovement != null)
            {
                // Fallback to CustomerMovement component
                return customerMovement.IsMoving ? 1f : 0f;
            }
            
            return 0f;
        }
        
        /// <summary>
        /// Set animator parameters based on movement speed
        /// </summary>
        private void SetAnimationParameters(float speed)
        {
            // Normalize speed for animation
            float normalizedSpeed = speed * walkSpeedMultiplier;
            bool isWalking = speed > walkThreshold;
            
            // Set parameters if they exist
            if (HasParameter(walkSpeedParameter))
            {
                animator.SetFloat(walkSpeedParameter, normalizedSpeed);
            }
            
            if (HasParameter(isWalkingParameter))
            {
                animator.SetBool(isWalkingParameter, isWalking);
            }
            
            // Optional: Set additional parameters for different states
            SetStateBasedAnimations();
        }
        
        /// <summary>
        /// Set animations based on customer state
        /// </summary>
        private void SetStateBasedAnimations()
        {
            var customer = GetComponent<Customer>();
            if (customer == null) return;
            
            var currentState = customer.CurrentState;
            
            // Set different animation parameters based on customer state
            switch (currentState)
            {
                case CustomerState.Entering:
                    // Could set a "curious" or "entering" animation parameter
                    break;
                    
                case CustomerState.Shopping:
                    // Could set a "browsing" animation parameter
                    if (HasParameter("IsBrowsing"))
                        animator.SetBool("IsBrowsing", !customer.IsMoving);
                    break;
                    
                case CustomerState.Purchasing:
                    // Could set a "purchasing" animation parameter
                    if (HasParameter("IsPurchasing"))
                        animator.SetBool("IsPurchasing", true);
                    break;
                    
                case CustomerState.Leaving:
                    // Could set a "leaving" animation parameter
                    break;
            }
        }
        
        /// <summary>
        /// Check if animator has a specific parameter
        /// </summary>
        private bool HasParameter(string parameterName)
        {
            if (animator == null || string.IsNullOrEmpty(parameterName)) return false;
            
            foreach (AnimatorControllerParameter param in animator.parameters)
            {
                if (param.name == parameterName)
                    return true;
            }
            
            return false;
        }
        
        /// <summary>
        /// Public method to trigger specific animations
        /// </summary>
        public void TriggerAnimation(string triggerName)
        {
            if (animator != null && HasParameter(triggerName))
            {
                animator.SetTrigger(triggerName);
            }
        }
        
        /// <summary>
        /// Public method to set boolean animation parameters
        /// </summary>
        public void SetAnimationBool(string parameterName, bool value)
        {
            if (animator != null && HasParameter(parameterName))
            {
                animator.SetBool(parameterName, value);
            }
        }
        
        /// <summary>
        /// Public method to set float animation parameters
        /// </summary>
        public void SetAnimationFloat(string parameterName, float value)
        {
            if (animator != null && HasParameter(parameterName))
            {
                animator.SetFloat(parameterName, value);
            }
        }
        
        #region Debug
        
        private void OnValidate()
        {
            // Ensure valid values
            walkSpeedMultiplier = Mathf.Max(0f, walkSpeedMultiplier);
            walkThreshold = Mathf.Max(0f, walkThreshold);
            animationSmoothTime = Mathf.Max(0.01f, animationSmoothTime);
        }
        
        #endregion
    }
}
