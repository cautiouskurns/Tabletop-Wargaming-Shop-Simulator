using UnityEngine;

namespace TabletopShop
{
    /// <summary>
    /// Simple store hours system that controls when the shop is open/closed
    /// Affects customer spawning, interactions, and UI feedback
    /// </summary>
    public class StoreHours : MonoBehaviour
    {
        [Header("Store Hours")]
        [SerializeField] private float openHour = 8f; // 8 AM
        [SerializeField] private float closeHour = 20f; // 8 PM
        
        [Header("References")]
        [SerializeField] private SimpleDayNightCycle dayNightCycle;
        [SerializeField] private CustomerSpawner customerSpawner;
        
        [Header("Store Closed Indicators")]
        [SerializeField] private GameObject[] closedSigns; // Signs to show when closed
        [SerializeField] private Light[] storeLights; // Extra lights for open hours
        
        // Current state
        private bool isStoreOpen = true;
        private bool wasOpenLastFrame = true;
        
        // Properties
        public bool IsStoreOpen => isStoreOpen;
        public float OpenHour => openHour;
        public float CloseHour => closeHour;
        public string StoreStatus => isStoreOpen ? "OPEN" : "CLOSED";
        
        private void Start()
        {
            // Try to find references if not assigned
            if (dayNightCycle == null)
                dayNightCycle = FindFirstObjectByType<SimpleDayNightCycle>();
            
            if (customerSpawner == null)
                customerSpawner = FindFirstObjectByType<CustomerSpawner>();
            
            // Initial store state check
            UpdateStoreStatus();
        }
        
        private void Update()
        {
            UpdateStoreStatus();
            
            // Handle store open/close transitions
            if (isStoreOpen != wasOpenLastFrame)
            {
                if (isStoreOpen)
                    OnStoreOpened();
                else
                    OnStoreClosed();
                
                wasOpenLastFrame = isStoreOpen;
            }
        }
        
        /// <summary>
        /// Update whether the store should be open based on current time
        /// </summary>
        private void UpdateStoreStatus()
        {
            if (dayNightCycle != null)
            {
                float currentHour = dayNightCycle.CurrentHour;
                isStoreOpen = currentHour >= openHour && currentHour < closeHour;
            }
            else
            {
                // Fallback: use GameManager if available
                if (GameManager.Instance != null)
                {
                    isStoreOpen = GameManager.Instance.IsDayTime;
                }
                else
                {
                    // Default to open if no time system found
                    isStoreOpen = true;
                }
            }
        }
        
        /// <summary>
        /// Called when the store opens
        /// </summary>
        private void OnStoreOpened()
        {
            Debug.Log($"Store OPENED at {dayNightCycle?.FormattedTime ?? "unknown time"}");
            
            // Start customer spawning
            if (customerSpawner != null)
            {
                customerSpawner.StartSpawning();
            }
            
            // Hide closed signs
            SetClosedSignsActive(false);
            
            // Turn on store lights
            SetStoreLightsActive(true);
        }
        
        /// <summary>
        /// Called when the store closes
        /// </summary>
        private void OnStoreClosed()
        {
            Debug.Log($"Store CLOSED at {dayNightCycle?.FormattedTime ?? "unknown time"}");
            
            // Stop customer spawning
            if (customerSpawner != null)
            {
                customerSpawner.StopSpawning();
            }
            
            // Show closed signs
            SetClosedSignsActive(true);
            
            // Turn off store lights (but keep security lighting)
            SetStoreLightsActive(false);
            
            // Optional: Kick out remaining customers
            KickOutRemainingCustomers();
        }
        
        /// <summary>
        /// Toggle closed signs visibility
        /// </summary>
        private void SetClosedSignsActive(bool active)
        {
            if (closedSigns != null)
            {
                foreach (GameObject sign in closedSigns)
                {
                    if (sign != null)
                        sign.SetActive(active);
                }
            }
        }
        
        /// <summary>
        /// Toggle store lights for business hours
        /// </summary>
        private void SetStoreLightsActive(bool active)
        {
            if (storeLights != null)
            {
                foreach (Light light in storeLights)
                {
                    if (light != null)
                        light.enabled = active;
                }
            }
        }
        
        /// <summary>
        /// Force remaining customers to leave when store closes
        /// </summary>
        private void KickOutRemainingCustomers()
        {
            Customer[] remainingCustomers = FindObjectsByType<Customer>(FindObjectsSortMode.None);
            
            foreach (Customer customer in remainingCustomers)
            {
                if (customer != null)
                {
                    // Force customers to leave
                    customer.Behavior.ChangeState(CustomerState.Leaving);
                    Debug.Log($"Kicked out customer {customer.name} - store is closing");
                }
            }
        }
        
        /// <summary>
        /// Check if a customer can enter the store
        /// </summary>
        public bool CanCustomerEnter()
        {
            return isStoreOpen;
        }
        
        /// <summary>
        /// Check if player interactions should be allowed
        /// </summary>
        public bool CanPlayerInteract()
        {
            return isStoreOpen;
        }
        
        /// <summary>
        /// Get time until store opens (if closed)
        /// </summary>
        public float GetTimeUntilOpen()
        {
            if (isStoreOpen) return 0f;
            
            if (dayNightCycle != null)
            {
                float currentHour = dayNightCycle.CurrentHour;
                
                if (currentHour < openHour)
                {
                    // Same day opening
                    return openHour - currentHour;
                }
                else
                {
                    // Next day opening
                    return (24f - currentHour) + openHour;
                }
            }
            
            return 0f;
        }
        
        /// <summary>
        /// Get time until store closes (if open)
        /// </summary>
        public float GetTimeUntilClose()
        {
            if (!isStoreOpen) return 0f;
            
            if (dayNightCycle != null)
            {
                float currentHour = dayNightCycle.CurrentHour;
                return closeHour - currentHour;
            }
            
            return 0f;
        }
        
        /// <summary>
        /// Get formatted store hours for UI display
        /// </summary>
        public string GetStoreHoursText()
        {
            int openHourInt = Mathf.FloorToInt(openHour);
            int closeHourInt = Mathf.FloorToInt(closeHour);
            
            return $"Store Hours: {openHourInt:D2}:00 - {closeHourInt:D2}:00";
        }
        
        #region Manual Controls (for testing)
        
        /// <summary>
        /// Manually open the store (for testing)
        /// </summary>
        [ContextMenu("Force Open Store")]
        public void ForceOpenStore()
        {
            isStoreOpen = true;
            OnStoreOpened();
        }
        
        /// <summary>
        /// Manually close the store (for testing)
        /// </summary>
        [ContextMenu("Force Close Store")]
        public void ForceCloseStore()
        {
            isStoreOpen = false;
            OnStoreClosed();
        }
        
        /// <summary>
        /// Toggle store status (for testing)
        /// </summary>
        [ContextMenu("Toggle Store Status")]
        public void ToggleStoreStatus()
        {
            if (isStoreOpen)
                ForceCloseStore();
            else
                ForceOpenStore();
        }
        
        #endregion
    }
}