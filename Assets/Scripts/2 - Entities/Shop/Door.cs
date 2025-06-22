using UnityEngine;
using System.Collections;

namespace TabletopShop
{
    /// <summary>
    /// Simple door that opens and closes when interacted with
    /// Works with your existing IInteractable system
    /// </summary>
    public class Door : MonoBehaviour, IInteractable
    {
        [Header("Door Settings")]
        [SerializeField] private bool isOpen = false;
        [SerializeField] private float openAngle = 90f;
        [SerializeField] private float animationSpeed = 2f;
        [SerializeField] private bool openInward = true;
        
        [Header("Audio")]
        [SerializeField] private AudioClip openSound;
        [SerializeField] private AudioClip closeSound;
        
        // Private variables
        private bool isAnimating = false;
        private float closedAngle = 0f;
        private Quaternion targetRotation;
        private AudioSource audioSource;
        
        // IInteractable properties
        public string InteractionText => isOpen ? "Close Door" : "Open Door";
        public bool CanInteract => !isAnimating;
        
        private void Start()
        {
            // Store the initial rotation as the closed position
            closedAngle = transform.localEulerAngles.y;
            
            // Get or add AudioSource for sound effects
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
            
            // Set initial target rotation
            UpdateTargetRotation();
        }
        
        /// <summary>
        /// Handle player interaction - toggle door open/closed
        /// </summary>
        public void Interact(GameObject player)
        {
            if (!CanInteract) return;
            
            ToggleDoor();
        }
        
        /// <summary>
        /// Toggle the door between open and closed
        /// </summary>
        public void ToggleDoor()
        {
            if (isAnimating) return;
            
            isOpen = !isOpen;
            UpdateTargetRotation();
            StartCoroutine(AnimateDoor());
            
            // Play sound effect
            PlayDoorSound();
            
            Debug.Log($"Door {name}: {(isOpen ? "Opening" : "Closing")}");
        }
        
        /// <summary>
        /// Open the door
        /// </summary>
        public void OpenDoor()
        {
            if (isOpen || isAnimating) return;
            
            isOpen = true;
            UpdateTargetRotation();
            StartCoroutine(AnimateDoor());
            PlayDoorSound();
        }
        
        /// <summary>
        /// Close the door
        /// </summary>
        public void CloseDoor()
        {
            if (!isOpen || isAnimating) return;
            
            isOpen = false;
            UpdateTargetRotation();
            StartCoroutine(AnimateDoor());
            PlayDoorSound();
        }
        
        /// <summary>
        /// Update the target rotation based on door state
        /// </summary>
        private void UpdateTargetRotation()
        {
            float targetAngle = isOpen ? 
                (closedAngle + (openInward ? openAngle : -openAngle)) : 
                closedAngle;
            
            targetRotation = Quaternion.Euler(0f, targetAngle, 0f);
        }
        
        /// <summary>
        /// Animate the door opening/closing
        /// </summary>
        private IEnumerator AnimateDoor()
        {
            isAnimating = true;
            
            Quaternion startRotation = transform.localRotation;
            float journeyLength = Quaternion.Angle(startRotation, targetRotation);
            float journeyTime = journeyLength / (animationSpeed * 90f); // Normalize to degrees per second
            
            float elapsedTime = 0f;
            
            while (elapsedTime < journeyTime)
            {
                elapsedTime += Time.deltaTime;
                float fractionOfJourney = elapsedTime / journeyTime;
                
                // Use smooth step for more natural movement
                fractionOfJourney = Mathf.SmoothStep(0f, 1f, fractionOfJourney);
                
                transform.localRotation = Quaternion.Slerp(startRotation, targetRotation, fractionOfJourney);
                yield return null;
            }
            
            // Ensure we end up exactly at the target rotation
            transform.localRotation = targetRotation;
            isAnimating = false;
            
            Debug.Log($"Door {name}: Animation completed - {(isOpen ? "Open" : "Closed")}");
        }
        
        /// <summary>
        /// Play appropriate door sound
        /// </summary>
        private void PlayDoorSound()
        {
            if (audioSource == null) return;
            
            AudioClip clipToPlay = isOpen ? openSound : closeSound;
            
            if (clipToPlay != null)
            {
                audioSource.PlayOneShot(clipToPlay);
            }
        }
        
        /// <summary>
        /// Called when player starts looking at the door
        /// </summary>
        public void OnInteractionEnter()
        {
            // Optional: Add visual feedback like highlighting
        }
        
        /// <summary>
        /// Called when player stops looking at the door
        /// </summary>
        public void OnInteractionExit()
        {
            // Optional: Remove visual feedback
        }
        
        /// <summary>
        /// Check if door is currently open
        /// </summary>
        public bool IsOpen => isOpen;
        
        /// <summary>
        /// Check if door is currently animating
        /// </summary>
        public bool IsAnimating => isAnimating;
        
        #if UNITY_EDITOR
        /// <summary>
        /// Test door in editor
        /// </summary>
        [ContextMenu("Test Toggle Door")]
        private void TestToggleDoor()
        {
            if (Application.isPlaying)
            {
                ToggleDoor();
            }
            else
            {
                Debug.Log("Door test requires Play mode");
            }
        }
        #endif
    }
}