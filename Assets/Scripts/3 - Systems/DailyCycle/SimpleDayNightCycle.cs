using UnityEngine;
using System;

namespace TabletopShop
{
    /// <summary>
    /// Simple day/night cycle script for visual transitions only
    /// Just handles lighting, sun rotation, and basic time display
    /// </summary>
    public class SimpleDayNightCycle : MonoBehaviour
    {
        [Header("Time Settings")]
        [SerializeField] private float dayLengthInSeconds = 300f; // 5 minutes = 1 day
        [SerializeField] private float startTime = 8f; // Start at 8 AM
        
        [Header("Visual Components")]
        [SerializeField] private Transform sunTransform;
        [SerializeField] private Light sunLight;
        [SerializeField] private Material skyboxMaterial;
        
        [Header("Lighting Colors")]
        [SerializeField] private Color dayAmbientColor = Color.white;
        [SerializeField] private Color nightAmbientColor = new Color(0.2f, 0.2f, 0.4f);
        
        [Header("Shop Lights")]
        [SerializeField] private Light[] shopLights;
        
        [Header("GameManager Integration")]
        [SerializeField] private bool updateGameManager = true; // Enable/disable GameManager integration
        
        [Header("Skybox Materials")]
        [SerializeField] private Material daySkybox;
        [SerializeField] private Material nightSkybox;
        [SerializeField] private float skyboxTransitionTime = 30f; // Time in seconds for skybox transition
        
        // Current time tracking
        private float currentTimeHours = 8f;
        private bool isDayTime = true;
        
        // Visual settings
        private float originalSunIntensity = 1.8f;
        private float originalSkyboxExposure = 1f;
        
        // GameManager integration
        private int currentDay = 1;
        private float dayStartTime = 0f; // Time when current day started
        
        // Skybox transition state
        private bool isTransitioningSkybox = false;
        private float skyboxTransitionProgress = 0f;
        
        public bool IsDayTime => isDayTime;
        public float CurrentHour => currentTimeHours;
        public string FormattedTime => $"{Mathf.FloorToInt(currentTimeHours):D2}:{Mathf.FloorToInt((currentTimeHours % 1) * 60):D2}";
        
        // New properties for UI integration
        public int CurrentDay => currentDay;
        public float DayProgress => (currentTimeHours - dayStartTime) / 24f;
        public float TotalDayLengthInSeconds => dayLengthInSeconds;
        
        private void Start()
        {
            currentTimeHours = startTime;
            
            // Store original values
            if (sunLight != null)
                originalSunIntensity = sunLight.intensity;
            
            // Safely get original skybox exposure if the property exists
            if (skyboxMaterial != null && skyboxMaterial.HasProperty("_Exposure"))
                originalSkyboxExposure = skyboxMaterial.GetFloat("_Exposure");
            
            // Set up skybox materials if not assigned
            SetupSkyboxMaterials();
            
            UpdateVisuals();
        }
        
        private void Update()
        {
            // Run independently for now - ignore GameManager time conflicts
            currentTimeHours += (24f / dayLengthInSeconds) * Time.deltaTime;
            
            // Handle day transition
            if (currentTimeHours >= 24f)
            {
                currentTimeHours -= 24f;
                currentDay++;
                dayStartTime = 0f; // Reset day start time
                OnDayTransition();
            }
            
            bool newIsDayTime = currentTimeHours >= 6f && currentTimeHours < 20f;
            
            if (newIsDayTime != isDayTime)
            {
                isDayTime = newIsDayTime;
                OnDayNightChange();
            }
            
            // Update GameManager if integration is enabled
            UpdateGameManagerIntegration();
            
            UpdateVisuals();
        }
        
        private void OnDayNightChange()
        {
            Debug.Log($"Time changed to {(isDayTime ? "Day" : "Night")} at {FormattedTime}");
            
            // Toggle shop lights
            if (shopLights != null)
            {
                foreach (Light light in shopLights)
                {
                    if (light != null)
                        light.enabled = !isDayTime;
                }
            }
            
            // Trigger skybox transition
            StartSkyboxTransition();
        }
        
        /// <summary>
        /// Handle day transition when a new day starts
        /// </summary>
        private void OnDayTransition()
        {
            Debug.Log($"New day started: Day {currentDay} at {FormattedTime}");
            
            // Update GameManager if available
            if (updateGameManager && GameManager.Instance != null)
            {
                // Trigger GameManager day change events if they exist
                // Note: This assumes GameManager has day change functionality
                Debug.Log($"SimpleDayNightCycle: Notifying GameManager of day transition to day {currentDay}");
            }
        }
        
        /// <summary>
        /// Update GameManager with current time data for UI integration
        /// </summary>
        private void UpdateGameManagerIntegration()
        {
            if (!updateGameManager || GameManager.Instance == null) return;
            
            // For now, we'll use a different approach since GameManager might not have these properties
            // The UI will need to get time data directly from this component
            // This is a placeholder for future GameManager time integration
        }
        
        private void UpdateVisuals()
        {
            UpdateSun();
            UpdateLighting();
            UpdateSkybox();
        }
        
        private void UpdateSun()
        {
            if (sunTransform == null) return;
            
            // Rotate sun based on time (sunrise at 6 AM, sunset at 6 PM)
            float sunAngle = ((currentTimeHours - 6f) / 12f) * 180f;
            sunTransform.rotation = Quaternion.Euler(sunAngle - 90f, 30f, 0f);
        }
        
        private void UpdateLighting()
        {
            if (sunLight == null) return;
            
            float targetIntensity;
            Color targetAmbient;
            
            if (isDayTime)
            {
                // Day time - bright
                float dayProgress = (currentTimeHours - 6f) / 14f; // 6 AM to 8 PM
                dayProgress = Mathf.Clamp01(dayProgress);
                
                // Brightest at noon (12 PM)
                float lightCurve = 1f - Mathf.Abs((dayProgress - 0.43f) * 1.5f); // 0.43f ≈ noon
                lightCurve = Mathf.Clamp01(lightCurve);
                
                targetIntensity = Mathf.Lerp(0.3f, originalSunIntensity, lightCurve);
                targetAmbient = Color.Lerp(nightAmbientColor, dayAmbientColor, lightCurve);
            }
            else
            {
                // Night time - dim
                targetIntensity = 0.1f;
                targetAmbient = nightAmbientColor;
            }
            
            // Smooth transitions
            sunLight.intensity = Mathf.Lerp(sunLight.intensity, targetIntensity, Time.deltaTime * 2f);
            RenderSettings.ambientLight = Color.Lerp(RenderSettings.ambientLight, targetAmbient, Time.deltaTime * 2f);
        }
        
        private void UpdateSkybox()
        {
            if (skyboxMaterial == null) return;
            
            // Safely handle exposure property if it exists
            if (skyboxMaterial.HasProperty("_Exposure"))
            {
                float targetExposure = isDayTime ? originalSkyboxExposure : 0.3f;
                float currentExposure = skyboxMaterial.GetFloat("_Exposure");
                skyboxMaterial.SetFloat("_Exposure", Mathf.Lerp(currentExposure, targetExposure, Time.deltaTime * 2f));
            }
            
            // Safely handle rotation property if it exists (some skyboxes don't have this)
            if (skyboxMaterial.HasProperty("_Rotation"))
            {
                float currentRotation = skyboxMaterial.GetFloat("_Rotation");
                skyboxMaterial.SetFloat("_Rotation", currentRotation + 0.1f * Time.deltaTime);
            }
        }
        
        /// <summary>
        /// Set the current time manually
        /// </summary>
        public void SetTime(float hours)
        {
            currentTimeHours = Mathf.Clamp(hours, 0f, 24f);
            isDayTime = currentTimeHours >= 6f && currentTimeHours < 20f;
            UpdateVisuals();
        }
        
        /// <summary>
        /// Skip to next day
        /// </summary>
        [ContextMenu("Skip to Morning")]
        public void SkipToMorning()
        {
            SetTime(8f);
        }
        
        /// <summary>
        /// Skip to night
        /// </summary>
        [ContextMenu("Skip to Night")]
        public void SkipToNight()
        {
            SetTime(22f);
        }
        
        /// <summary>
        /// Test time integration with UI system
        /// </summary>
        [ContextMenu("Test Time Integration")]
        public void TestTimeIntegration()
        {
            Debug.Log("=== TIME INTEGRATION TEST ===");
            Debug.Log($"Current Time: {FormattedTime}");
            Debug.Log($"Current Day: {CurrentDay}");
            Debug.Log($"Is Day Time: {IsDayTime}");
            Debug.Log($"Day Progress: {DayProgress:F2}");
            Debug.Log($"Day Length: {TotalDayLengthInSeconds} seconds");
            
            // Test if ShopUI can find this component
            ShopUI shopUI = FindFirstObjectByType<ShopUI>();
            if (shopUI != null)
            {
                Debug.Log("✓ ShopUI found - time integration should work");
            }
            else
            {
                Debug.LogWarning("✗ ShopUI not found - time may not display in UI");
            }
            
            Debug.Log("=============================");
        }
        
        /// <summary>
        /// Set up skybox materials for day/night transitions
        /// </summary>
        private void SetupSkyboxMaterials()
        {
            // If no day skybox is assigned, use the current skybox as the day skybox
            if (daySkybox == null)
            {
                daySkybox = RenderSettings.skybox;
                if (daySkybox != null)
                {
                    Debug.Log($"SimpleDayNightCycle: Using current skybox as day skybox: {daySkybox.name}");
                }
                else
                {
                    Debug.LogWarning("SimpleDayNightCycle: No skybox material found in RenderSettings");
                }
            }
            
            // Validate skybox assignments
            if (daySkybox != null && nightSkybox != null)
            {
                Debug.Log($"SimpleDayNightCycle: Day/Night skybox switching enabled");
                Debug.Log($"  Day Skybox: {daySkybox.name}");
                Debug.Log($"  Night Skybox: {nightSkybox.name}");
            }
            else if (daySkybox != null)
            {
                Debug.Log($"SimpleDayNightCycle: Only day skybox assigned - no skybox switching");
            }
            else
            {
                Debug.LogWarning("SimpleDayNightCycle: No skybox materials available");
            }
        }
        
        /// <summary>
        /// Start a skybox transition when day/night changes
        /// </summary>
        private void StartSkyboxTransition()
        {
            // Only start transition if we have both day and night skyboxes
            if (daySkybox == null || nightSkybox == null)
            {
                Debug.Log("SimpleDayNightCycle: Skybox transition skipped - missing day or night skybox material");
                return;
            }
            
            if (isTransitioningSkybox)
            {
                Debug.Log("SimpleDayNightCycle: Skybox transition already in progress");
                return;
            }
            
            isTransitioningSkybox = true;
            skyboxTransitionProgress = 0f;
            
            string transitionType = isDayTime ? "Night → Day" : "Day → Night";
            Debug.Log($"SimpleDayNightCycle: Starting skybox transition: {transitionType}");
            
            // Start the transition coroutine
            StartCoroutine(SkyboxTransitionCoroutine());
        }
        
        /// <summary>
        /// Coroutine to handle smooth skybox transitions
        /// </summary>
        private System.Collections.IEnumerator SkyboxTransitionCoroutine()
        {
            Material fromSkybox = isDayTime ? nightSkybox : daySkybox;
            Material toSkybox = isDayTime ? daySkybox : nightSkybox;
            
            // Immediately switch to the target skybox
            RenderSettings.skybox = toSkybox;
            
            // Handle exposure blending if the skybox supports it
            if (toSkybox.HasProperty("_Exposure"))
            {
                float startExposure = isDayTime ? 0.3f : originalSkyboxExposure;
                float targetExposure = isDayTime ? originalSkyboxExposure : 0.3f;
                
                while (skyboxTransitionProgress < 1f)
                {
                    skyboxTransitionProgress += Time.deltaTime / skyboxTransitionTime;
                    skyboxTransitionProgress = Mathf.Clamp01(skyboxTransitionProgress);
                    
                    // Smooth transition curve
                    float easedProgress = Mathf.SmoothStep(0f, 1f, skyboxTransitionProgress);
                    
                    // Interpolate exposure
                    float currentExposure = Mathf.Lerp(startExposure, targetExposure, easedProgress);
                    toSkybox.SetFloat("_Exposure", currentExposure);
                    
                    yield return null;
                }
                
                // Ensure final values are set
                toSkybox.SetFloat("_Exposure", targetExposure);
            }
            else
            {
                // If no exposure property, just wait for the transition time
                yield return new WaitForSeconds(skyboxTransitionTime);
            }
            
            isTransitioningSkybox = false;
            string transitionType = isDayTime ? "Day" : "Night";
            Debug.Log($"SimpleDayNightCycle: Skybox transition to {transitionType} completed");
        }
    }
}