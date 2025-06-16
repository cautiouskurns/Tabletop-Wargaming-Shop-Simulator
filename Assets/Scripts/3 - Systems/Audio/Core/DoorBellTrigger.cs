using UnityEngine;

namespace TabletopShop
{
    /// <summary>
    /// Simple trigger zone that plays door bell sound when customers enter
    /// Place this at your shop entrance
    /// </summary>
    public class DoorBellTrigger : MonoBehaviour
    {
        [Header("Audio Settings")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip bellSound;
        [SerializeField] private float volume = 1f;
        [SerializeField] private float cooldownTime = 2f; // Prevent spam
        
        [Header("Debug")]
        [SerializeField] private bool enableDebugLog = true;
        
        private float lastPlayTime = 0f;
        
        private void Start()
        {
            // Get AudioSource if not assigned
            if (audioSource == null)
                audioSource = GetComponent<AudioSource>();
            
            // Create AudioSource if none exists
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
                audioSource.volume = volume;
            }
            
            // Set the bell sound if assigned
            if (bellSound != null)
                audioSource.clip = bellSound;
            
            // Make sure this has a trigger collider
            Collider col = GetComponent<Collider>();
            if (col != null)
            {
                col.isTrigger = true;
            }
            else
            {
                Debug.LogWarning("DoorBellTrigger: No collider found! Add a collider and set it as trigger.");
            }
        }
        
        private void OnTriggerEnter(Collider other)
        {
            // Check if it's a customer entering
            Customer customer = other.GetComponent<Customer>();
            if (customer != null)
            {
                // Check cooldown to prevent spam
                if (Time.time - lastPlayTime >= cooldownTime)
                {
                    PlayBellSound();
                    lastPlayTime = Time.time;
                    
                    if (enableDebugLog)
                        Debug.Log($"Door bell triggered by customer: {customer.name}");
                }
            }
        }
        
        /// <summary>
        /// Play the door bell sound
        /// </summary>
        private void PlayBellSound()
        {
            if (audioSource != null && audioSource.clip != null)
            {
                audioSource.Play();
            }
            else
            {
                // Fallback: Try AudioManager
                if (AudioManager.Instance != null)
                {
                    // AudioManager.Instance.PlayDoorBell();
                    Debug.Log("*DING* Door bell sound!");
                }
                else
                {
                    Debug.Log("*DING* Door bell sound! (No audio clip assigned)");
                }
            }
        }
        
        /// <summary>
        /// Test the door bell sound
        /// </summary>
        [ContextMenu("Test Bell Sound")]
        public void TestBellSound()
        {
            PlayBellSound();
        }
    }
}
