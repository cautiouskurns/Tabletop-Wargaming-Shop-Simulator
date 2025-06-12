using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace TabletopShop
{
    /// <summary>
    /// Manages the settings UI panel including audio volume controls
    /// Provides a centralized settings interface accessible from the pause menu
    /// </summary>
    public class SettingsUI : MonoBehaviour
    {
        [Header("Settings Panel")]
        [SerializeField] private GameObject settingsPanel;
        
        [Header("Volume Controls")]
        [SerializeField] private Slider masterVolumeSlider;
        [SerializeField] private Slider sfxVolumeSlider;
        [SerializeField] private Slider uiVolumeSlider;
        [SerializeField] private Slider ambientVolumeSlider;
        
        [Header("Volume Labels")]
        [SerializeField] private TextMeshProUGUI masterVolumeLabel;
        [SerializeField] private TextMeshProUGUI sfxVolumeLabel;
        [SerializeField] private TextMeshProUGUI uiVolumeLabel;
        [SerializeField] private TextMeshProUGUI ambientVolumeLabel;
        
        [Header("Buttons")]
        [SerializeField] private Button closeSettingsButton;
        
        private bool isSettingsPanelVisible = false;
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            // Ensure settings panel starts hidden
            if (settingsPanel != null)
            {
                settingsPanel.SetActive(false);
            }
        }
        
        private void Start()
        {
            SetupVolumeControls();
            SetupButtonEvents();
        }
        
        private void OnDestroy()
        {
            CleanupButtonEvents();
            CleanupVolumeEvents();
        }
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// Show the settings panel
        /// </summary>
        public void ShowSettings()
        {
            if (settingsPanel != null && !isSettingsPanelVisible)
            {
                // Update sliders with current audio values
                UpdateVolumeSliders();
                
                settingsPanel.SetActive(true);
                isSettingsPanelVisible = true;
                
                // Play UI click sound
                AudioManager.Instance?.PlayUIClick();
                
                Debug.Log("[SettingsUI] Settings panel shown");
            }
        }
        
        /// <summary>
        /// Hide the settings panel
        /// </summary>
        public void HideSettings()
        {
            if (settingsPanel != null && isSettingsPanelVisible)
            {
                settingsPanel.SetActive(false);
                isSettingsPanelVisible = false;
                
                // Play UI click sound
                AudioManager.Instance?.PlayUIClick();
                
                Debug.Log("[SettingsUI] Settings panel hidden");
            }
        }
        
        /// <summary>
        /// Toggle settings panel visibility
        /// </summary>
        public void ToggleSettings()
        {
            if (isSettingsPanelVisible)
            {
                HideSettings();
            }
            else
            {
                ShowSettings();
            }
        }
        
        #endregion
        
        #region Volume Control Setup
        
        /// <summary>
        /// Initialize volume sliders with current values and setup event handlers
        /// </summary>
        private void SetupVolumeControls()
        {
            if (AudioManager.Instance == null)
            {
                Debug.LogWarning("[SettingsUI] AudioManager not available, volume controls will not function");
                return;
            }
            
            // Setup Master Volume
            if (masterVolumeSlider != null)
            {
                masterVolumeSlider.minValue = 0f;
                masterVolumeSlider.maxValue = 1f;
                masterVolumeSlider.value = AudioManager.Instance.MasterVolume;
                masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
            }
            
            // Setup SFX Volume
            if (sfxVolumeSlider != null)
            {
                sfxVolumeSlider.minValue = 0f;
                sfxVolumeSlider.maxValue = 1f;
                sfxVolumeSlider.value = AudioManager.Instance.SfxVolume;
                sfxVolumeSlider.onValueChanged.AddListener(OnSfxVolumeChanged);
            }
            
            // Setup UI Volume
            if (uiVolumeSlider != null)
            {
                uiVolumeSlider.minValue = 0f;
                uiVolumeSlider.maxValue = 1f;
                uiVolumeSlider.value = AudioManager.Instance.UiVolume;
                uiVolumeSlider.onValueChanged.AddListener(OnUiVolumeChanged);
            }
            
            // Setup Ambient Volume
            if (ambientVolumeSlider != null)
            {
                ambientVolumeSlider.minValue = 0f;
                ambientVolumeSlider.maxValue = 1f;
                ambientVolumeSlider.value = AudioManager.Instance.AmbientVolume;
                ambientVolumeSlider.onValueChanged.AddListener(OnAmbientVolumeChanged);
            }
            
            // Update labels
            UpdateVolumeLabels();
        }
        
        /// <summary>
        /// Update volume sliders to reflect current AudioManager values
        /// </summary>
        private void UpdateVolumeSliders()
        {
            if (AudioManager.Instance == null) return;
            
            if (masterVolumeSlider != null)
                masterVolumeSlider.value = AudioManager.Instance.MasterVolume;
                
            if (sfxVolumeSlider != null)
                sfxVolumeSlider.value = AudioManager.Instance.SfxVolume;
                
            if (uiVolumeSlider != null)
                uiVolumeSlider.value = AudioManager.Instance.UiVolume;
                
            if (ambientVolumeSlider != null)
                ambientVolumeSlider.value = AudioManager.Instance.AmbientVolume;
                
            UpdateVolumeLabels();
        }
        
        /// <summary>
        /// Update volume percentage labels
        /// </summary>
        private void UpdateVolumeLabels()
        {
            if (masterVolumeLabel != null && masterVolumeSlider != null)
                masterVolumeLabel.text = $"Master: {Mathf.RoundToInt(masterVolumeSlider.value * 100)}%";
                
            if (sfxVolumeLabel != null && sfxVolumeSlider != null)
                sfxVolumeLabel.text = $"SFX: {Mathf.RoundToInt(sfxVolumeSlider.value * 100)}%";
                
            if (uiVolumeLabel != null && uiVolumeSlider != null)
                uiVolumeLabel.text = $"UI: {Mathf.RoundToInt(uiVolumeSlider.value * 100)}%";
                
            if (ambientVolumeLabel != null && ambientVolumeSlider != null)
                ambientVolumeLabel.text = $"Ambient: {Mathf.RoundToInt(ambientVolumeSlider.value * 100)}%";
        }
        
        #endregion
        
        #region Volume Event Handlers
        
        private void OnMasterVolumeChanged(float value)
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.MasterVolume = value;
                UpdateVolumeLabels();
            }
        }
        
        private void OnSfxVolumeChanged(float value)
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.SfxVolume = value;
                UpdateVolumeLabels();
            }
        }
        
        private void OnUiVolumeChanged(float value)
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.UiVolume = value;
                UpdateVolumeLabels();
            }
        }
        
        private void OnAmbientVolumeChanged(float value)
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.AmbientVolume = value;
                UpdateVolumeLabels();
            }
        }
        
        #endregion
        
        #region Button Setup and Cleanup
        
        private void SetupButtonEvents()
        {
            if (closeSettingsButton != null)
            {
                closeSettingsButton.onClick.AddListener(OnCloseSettingsButtonClicked);
            }
        }
        
        private void CleanupButtonEvents()
        {
            if (closeSettingsButton != null)
            {
                closeSettingsButton.onClick.RemoveListener(OnCloseSettingsButtonClicked);
            }
        }
        
        private void CleanupVolumeEvents()
        {
            if (masterVolumeSlider != null)
                masterVolumeSlider.onValueChanged.RemoveListener(OnMasterVolumeChanged);
                
            if (sfxVolumeSlider != null)
                sfxVolumeSlider.onValueChanged.RemoveListener(OnSfxVolumeChanged);
                
            if (uiVolumeSlider != null)
                uiVolumeSlider.onValueChanged.RemoveListener(OnUiVolumeChanged);
                
            if (ambientVolumeSlider != null)
                ambientVolumeSlider.onValueChanged.RemoveListener(OnAmbientVolumeChanged);
        }
        
        private void OnCloseSettingsButtonClicked()
        {
            HideSettings();
        }
        
        #endregion
    }
}
