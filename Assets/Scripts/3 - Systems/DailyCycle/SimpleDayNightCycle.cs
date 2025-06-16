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
        
        // Current time tracking
        private float currentTimeHours = 8f;
        private bool isDayTime = true;
        
        // Visual settings
        private float originalSunIntensity = 1.8f;
        private float originalSkyboxExposure = 1f;
        
        public bool IsDayTime => isDayTime;
        public float CurrentHour => currentTimeHours;
        public string FormattedTime => $"{Mathf.FloorToInt(currentTimeHours):D2}:{Mathf.FloorToInt((currentTimeHours % 1) * 60):D2}";
        
        private void Start()
        {
            currentTimeHours = startTime;
            
            // Store original values
            if (sunLight != null)
                originalSunIntensity = sunLight.intensity;
            if (skyboxMaterial != null)
                originalSkyboxExposure = skyboxMaterial.GetFloat("_Exposure");
            
            UpdateVisuals();
        }
        
        private void Update()
        {
            // Run independently for now - ignore GameManager time conflicts
            currentTimeHours += (24f / dayLengthInSeconds) * Time.deltaTime;
            
            if (currentTimeHours >= 24f)
            {
                currentTimeHours -= 24f;
            }
            
            bool newIsDayTime = currentTimeHours >= 6f && currentTimeHours < 20f;
            
            if (newIsDayTime != isDayTime)
            {
                isDayTime = newIsDayTime;
                OnDayNightChange();
            }
            
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
            
            float targetExposure = isDayTime ? originalSkyboxExposure : 0.3f;
            float currentExposure = skyboxMaterial.GetFloat("_Exposure");
            skyboxMaterial.SetFloat("_Exposure", Mathf.Lerp(currentExposure, targetExposure, Time.deltaTime * 2f));
            
            // Slowly rotate skybox
            float currentRotation = skyboxMaterial.GetFloat("_Rotation");
            skyboxMaterial.SetFloat("_Rotation", currentRotation + 0.1f * Time.deltaTime);
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
    }
}