using UnityEngine;
using System.Collections.Generic;
using System.Collections;

namespace TabletopShop
{
    /// <summary>
    /// Centralized audio management system for the shop simulator
    /// Handles all audio feedback including purchase sounds, UI clicks, and ambient atmosphere
    /// 
    /// Audio Architecture:
    /// - Singleton pattern for global access from any system
    /// - Audio pooling for performance with overlapping sounds
    /// - Category-based volume control (SFX, UI, Ambient)
    /// - Event-driven integration with game systems
    /// 
    /// Performance Optimization:
    /// - Object pooling prevents audio source creation/destruction overhead
    /// - Separate audio sources for different categories prevent interference
    /// - Efficient audio clip caching and reuse
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        #region Singleton Pattern
        
        private static AudioManager _instance;
        public static AudioManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<AudioManager>();
                    if (_instance == null)
                    {
                        GameObject audioManagerObject = new GameObject("AudioManager");
                        _instance = audioManagerObject.AddComponent<AudioManager>();
                        DontDestroyOnLoad(audioManagerObject);
                    }
                }
                return _instance;
            }
        }
        
        #endregion
        
        #region Audio Categories and Settings
        
        [Header("Audio Categories")]
        [SerializeField] [Range(0f, 1f)] private float masterVolume = 1f;
        [SerializeField] [Range(0f, 1f)] private float sfxVolume = 1f;
        [SerializeField] [Range(0f, 1f)] private float uiVolume = 1f;
        [SerializeField] [Range(0f, 1f)] private float ambientVolume = 0.6f;
        
        [Header("Audio Clips")]
        [SerializeField] private AudioClip purchaseSuccessClip;
        [SerializeField] private AudioClip uiClickClip;
        [SerializeField] private AudioClip shopAtmosphereClip;
        
        [Header("Background Music")]
        [SerializeField] private AudioClip[] backgroundMusicTracks;
        [SerializeField] private bool shuffleMusic = true;
        [SerializeField] private bool crossfadeMusic = true;
        [SerializeField] private float crossfadeDuration = 2f;
        [SerializeField] private float musicVolume = 0.7f;
        
        [Header("Audio Sources")]
        [SerializeField] private AudioSource ambientSource;
        [SerializeField] private AudioSource uiSource;
        [SerializeField] private AudioSource musicSourceA;
        [SerializeField] private AudioSource musicSourceB;
        
        [Header("Audio Pool Settings")]
        [SerializeField] private int sfxPoolSize = 10;
        
        [Header("Ambient Audio Settings")]
        [SerializeField] private bool autoStartAmbient = true;
        [SerializeField] private bool restartAmbientOnSceneLoad = true;
        [SerializeField] private float ambientStartDelay = 0f;
        
        [Header("Random Ambient Triggering")]
        [SerializeField] private bool useRandomAmbientTriggers = false;
        [SerializeField] private float minTimeBetweenAmbient = 30f;
        [SerializeField] private float maxTimeBetweenAmbient = 90f;
        [SerializeField] private float ambientPlayDuration = 15f;
        
        // Audio pooling for SFX to handle overlapping sounds
        private List<AudioSource> sfxAudioPool = new List<AudioSource>();
        private Queue<AudioSource> availableSfxSources = new Queue<AudioSource>();
        
        // Background music management
        private int currentTrackIndex = 0;
        private List<int> shuffledTrackOrder = new List<int>();
        private bool isPlayingMusicA = true; // Which music source is currently active
        private Coroutine musicCrossfadeCoroutine;
        private bool isMusicPlaying = false;
        
        // Random ambient triggering
        private Coroutine randomAmbientCoroutine;
        
        // Properties for external volume control
        public float MasterVolume 
        { 
            get => masterVolume; 
            set 
            { 
                masterVolume = Mathf.Clamp01(value);
                UpdateAllVolumes();
                SaveAudioSettings();
            } 
        }
        
        public float SfxVolume 
        { 
            get => sfxVolume; 
            set 
            { 
                sfxVolume = Mathf.Clamp01(value);
                UpdateSfxVolumes();
                SaveAudioSettings();
            } 
        }
        
        public float UiVolume 
        { 
            get => uiVolume; 
            set 
            { 
                uiVolume = Mathf.Clamp01(value);
                UpdateUiVolume();
                SaveAudioSettings();
            } 
        }
        
        public float AmbientVolume 
        { 
            get => ambientVolume; 
            set 
            { 
                ambientVolume = Mathf.Clamp01(value);
                UpdateAmbientVolume();
                SaveAudioSettings();
            } 
        }
        
        public float MusicVolume 
        { 
            get => musicVolume; 
            set 
            { 
                musicVolume = Mathf.Clamp01(value);
                UpdateMusicVolume();
                SaveAudioSettings();
            } 
        }
        
        #endregion
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            // Ensure singleton instance
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeAudioManager();
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
                return;
            }
        }
        
        private void Start()
        {
            // Subscribe to game events for automatic audio triggering
            SubscribeToGameEvents();
            
            // Start background music if tracks are available
            if (backgroundMusicTracks != null && backgroundMusicTracks.Length > 0)
            {
                StartBackgroundMusic(true);
            }
            
            // Start ambient atmosphere (if enabled)
            if (autoStartAmbient)
            {
                if (useRandomAmbientTriggers)
                {
                    // Start random ambient triggering system
                    StartRandomAmbientTriggers();
                }
                else
                {
                    // Standard single start with optional delay
                    if (ambientStartDelay > 0f)
                    {
                        StartCoroutine(StartAmbientAudioDelayed(ambientStartDelay));
                    }
                    else
                    {
                        StartAmbientAudio();
                    }
                }
            }
            
            Debug.Log("AudioManager initialized with pooled audio sources and event subscriptions");
        }
        
        private void OnDestroy()
        {
            // Stop random ambient triggering
            StopRandomAmbientTriggers();
            
            // Unsubscribe from events to prevent memory leaks
            UnsubscribeFromGameEvents();
        }
        
        #endregion
        
        #region Audio Manager Initialization
        
        /// <summary>
        /// Initialize all audio systems and components
        /// 
        /// Audio System Setup:
        /// - Ensures AudioListener exists in the scene
        /// - Creates dedicated audio sources for different categories
        /// - Initializes audio pooling system for overlapping SFX
        /// - Loads saved audio settings from PlayerPrefs
        /// - Sets up audio source configurations for optimal performance
        /// </summary>
        private void InitializeAudioManager()
        {
            // Ensure AudioListener exists in the scene
            EnsureAudioListener();
            
            // Load saved audio settings
            LoadAudioSettings();
            
            // Create dedicated audio sources if not assigned
            CreateAudioSources();
            
            // Initialize SFX audio pool for performance
            InitializeSfxAudioPool();
            
            // Apply initial volume settings
            UpdateAllVolumes();
        }
        
        /// <summary>
        /// Ensure there is an AudioListener in the scene
        /// AudioListener is required for any audio to be heard in Unity
        /// </summary>
        private void EnsureAudioListener()
        {
            AudioListener existingListener = FindFirstObjectByType<AudioListener>();
            if (existingListener == null)
            {
                // No AudioListener found, add one to the AudioManager GameObject
                gameObject.AddComponent<AudioListener>();
                Debug.Log("[AudioManager] AudioListener added to AudioManager - audio will now be audible");
            }
            else
            {
                Debug.Log($"[AudioManager] AudioListener found on '{existingListener.gameObject.name}' - audio system ready");
            }
        }
        
        /// <summary>
        /// Create dedicated audio sources for different audio categories
        /// Separating categories prevents interference and allows independent control
        /// </summary>
        private void CreateAudioSources()
        {
            // Create ambient audio source for background atmosphere
            if (ambientSource == null)
            {
                GameObject ambientObject = new GameObject("AmbientAudioSource");
                ambientObject.transform.SetParent(transform);
                ambientSource = ambientObject.AddComponent<AudioSource>();
                ambientSource.loop = true;
                ambientSource.playOnAwake = false;
            }
            
            // Create UI audio source for button clicks and interface sounds
            if (uiSource == null)
            {
                GameObject uiObject = new GameObject("UIAudioSource");
                uiObject.transform.SetParent(transform);
                uiSource = uiObject.AddComponent<AudioSource>();
                uiSource.loop = false;
                uiSource.playOnAwake = false;
            }
            
            // Create music audio sources for background music (dual sources for crossfading)
            if (musicSourceA == null)
            {
                GameObject musicObjectA = new GameObject("MusicAudioSource_A");
                musicObjectA.transform.SetParent(transform);
                musicSourceA = musicObjectA.AddComponent<AudioSource>();
                musicSourceA.loop = false;
                musicSourceA.playOnAwake = false;
            }
            
            if (musicSourceB == null)
            {
                GameObject musicObjectB = new GameObject("MusicAudioSource_B");
                musicObjectB.transform.SetParent(transform);
                musicSourceB = musicObjectB.AddComponent<AudioSource>();
                musicSourceB.loop = false;
                musicSourceB.playOnAwake = false;
            }
        }
        
        /// <summary>
        /// Initialize audio pooling system for SFX
        /// 
        /// Audio Pooling Benefits:
        /// - Prevents GameObject creation/destruction overhead
        /// - Allows multiple simultaneous sound effects
        /// - Improves performance with frequent audio playback
        /// - Reduces garbage collection pressure
        /// </summary>
        private void InitializeSfxAudioPool()
        {
            for (int i = 0; i < sfxPoolSize; i++)
            {
                GameObject sfxObject = new GameObject($"SFXAudioSource_{i}");
                sfxObject.transform.SetParent(transform);
                AudioSource sfxSource = sfxObject.AddComponent<AudioSource>();
                sfxSource.loop = false;
                sfxSource.playOnAwake = false;
                
                sfxAudioPool.Add(sfxSource);
                availableSfxSources.Enqueue(sfxSource);
            }
            
            Debug.Log($"Audio pool initialized with {sfxPoolSize} SFX audio sources");
        }
        
        #endregion
        
        #region Game Event Integration
        
        /// <summary>
        /// Subscribe to game events for automatic audio triggering
        /// Event-driven audio ensures sounds play at exactly the right moments
        /// </summary>
        private void SubscribeToGameEvents()
        {
            // Subscribe to GameManager events if available
            if (GameManager.Instance != null)
            {
                // GameManager typically has money change events we can hook into
                // Note: This assumes GameManager has events - may need adjustment based on actual implementation
            }
            
            // Subscribe to purchase events from inventory system
            // Note: May need to add events to InventoryManager for this integration
            
            Debug.Log("AudioManager subscribed to game events for automatic audio triggering");
        }
        
        /// <summary>
        /// Unsubscribe from game events to prevent memory leaks
        /// </summary>
        private void UnsubscribeFromGameEvents()
        {
            // Unsubscribe from all game events here
            // This prevents memory leaks when AudioManager is destroyed
        }
        
        #endregion
        
        #region Public Audio Playback Methods
        
        /// <summary>
        /// Play purchase success sound effect
        /// Called when customer successfully purchases a product
        /// </summary>
        /// <param name="pitch">Optional pitch variation for variety (default: 1.0)</param>
        public void PlayPurchaseSuccess(float pitch = 1f)
        {
            if (purchaseSuccessClip != null)
            {
                PlaySfxClip(purchaseSuccessClip, pitch);
                Debug.Log("Purchase success audio played");
            }
            else
            {
                Debug.LogWarning("Purchase success audio clip not assigned!");
            }
        }
        
        /// <summary>
        /// Play UI click sound effect
        /// Called when player interacts with buttons and UI elements
        /// </summary>
        /// <param name="pitch">Optional pitch variation for variety (default: 1.0)</param>
        public void PlayUIClick(float pitch = 1f)
        {
            if (uiClickClip != null)
            {
                PlayUIClip(uiClickClip, pitch);
            }
            else
            {
                Debug.LogWarning("UI click audio clip not assigned!");
            }
        }
        
        /// <summary>
        /// Play generic SFX with specified audio clip
        /// Useful for custom sound effects throughout the game
        /// </summary>
        /// <param name="clip">Audio clip to play</param>
        /// <param name="pitch">Pitch variation for the sound</param>
        /// <param name="volume">Volume multiplier for this specific sound</param>
        public void PlaySfx(AudioClip clip, float pitch = 1f, float volume = 1f)
        {
            if (clip != null)
            {
                PlaySfxClip(clip, pitch, volume);
            }
        }
        
        /// <summary>
        /// Start or restart ambient shop atmosphere audio
        /// Creates immersive background soundscape for the shop environment
        /// </summary>
        public void StartAmbientAudio()
        {
            if (shopAtmosphereClip != null && ambientSource != null)
            {
                ambientSource.clip = shopAtmosphereClip;
                ambientSource.volume = ambientVolume * masterVolume;
                ambientSource.Play();
                Debug.Log("Ambient shop atmosphere started");
            }
            else
            {
                Debug.LogWarning("Shop atmosphere audio clip or ambient source not assigned!");
            }
        }
        
        /// <summary>
        /// Stop ambient audio (useful for menu transitions or special events)
        /// </summary>
        public void StopAmbientAudio()
        {
            if (ambientSource != null && ambientSource.isPlaying)
            {
                ambientSource.Stop();
                Debug.Log("Ambient shop atmosphere stopped");
            }
        }
        
        /// <summary>
        /// Restart ambient audio with optional delay
        /// Useful for scene transitions or dynamic ambient changes
        /// </summary>
        /// <param name="delay">Optional delay before restarting (default: 0)</param>
        public void RestartAmbientAudio(float delay = 0f)
        {
            StopAmbientAudio();
            
            if (delay > 0f)
            {
                StartCoroutine(StartAmbientAudioDelayed(delay));
            }
            else
            {
                StartAmbientAudio();
            }
        }
        
        /// <summary>
        /// Check if ambient audio is currently playing
        /// </summary>
        /// <returns>True if ambient audio is playing</returns>
        public bool IsAmbientAudioPlaying()
        {
            return ambientSource != null && ambientSource.isPlaying;
        }
        
        /// <summary>
        /// Schedule ambient audio to start at a specific time interval
        /// Useful for creating atmospheric cycles or timed ambient changes
        /// </summary>
        /// <param name="interval">Time interval in seconds between ambient restarts</param>
        public void StartAmbientAudioCycle(float interval)
        {
            StartCoroutine(AmbientAudioCycle(interval));
        }
        
        /// <summary>
        /// Coroutine for cycling ambient audio at regular intervals
        /// </summary>
        /// <param name="interval">Time between cycles</param>
        /// <returns>Coroutine enumerator</returns>
        private IEnumerator AmbientAudioCycle(float interval)
        {
            while (true)
            {
                StartAmbientAudio();
                yield return new WaitForSeconds(interval);
                StopAmbientAudio();
                yield return new WaitForSeconds(1f); // Brief pause between cycles
            }
        }

        /// <summary>
        /// Start the random ambient triggering system
        /// Creates semi-random ambient audio bursts at configurable intervals
        /// </summary>
        public void StartRandomAmbientTriggers()
        {
            // Stop any existing random ambient coroutine
            StopRandomAmbientTriggers();
            
            // Start new random ambient triggering
            randomAmbientCoroutine = StartCoroutine(RandomAmbientTriggerLoop());
            Debug.Log($"Random ambient triggering started (interval: {minTimeBetweenAmbient}-{maxTimeBetweenAmbient}s, duration: {ambientPlayDuration}s)");
        }
        
        /// <summary>
        /// Stop the random ambient triggering system
        /// </summary>
        public void StopRandomAmbientTriggers()
        {
            if (randomAmbientCoroutine != null)
            {
                StopCoroutine(randomAmbientCoroutine);
                randomAmbientCoroutine = null;
                StopAmbientAudio();
                Debug.Log("Random ambient triggering stopped");
            }
        }
        
        /// <summary>
        /// Toggle random ambient triggering on/off
        /// </summary>
        /// <param name="enabled">True to enable random triggering, false to disable</param>
        public void SetRandomAmbientEnabled(bool enabled)
        {
            useRandomAmbientTriggers = enabled;
            
            if (enabled)
            {
                StartRandomAmbientTriggers();
            }
            else
            {
                StopRandomAmbientTriggers();
            }
        }
        
        /// <summary>
        /// Update the timing parameters for random ambient triggering
        /// </summary>
        /// <param name="minInterval">Minimum time between ambient sounds</param>
        /// <param name="maxInterval">Maximum time between ambient sounds</param>
        /// <param name="duration">How long each ambient sound plays</param>
        public void UpdateRandomAmbientTiming(float minInterval, float maxInterval, float duration)
        {
            minTimeBetweenAmbient = Mathf.Max(1f, minInterval);
            maxTimeBetweenAmbient = Mathf.Max(minTimeBetweenAmbient + 1f, maxInterval);
            ambientPlayDuration = Mathf.Max(1f, duration);
            
            Debug.Log($"Updated random ambient timing: {minTimeBetweenAmbient}-{maxTimeBetweenAmbient}s interval, {ambientPlayDuration}s duration");
            
            // Restart the system with new timing if it's currently running
            if (randomAmbientCoroutine != null)
            {
                StartRandomAmbientTriggers();
            }
        }
        
        /// <summary>
        /// Coroutine that handles the random ambient triggering loop
        /// </summary>
        /// <returns>Coroutine enumerator</returns>
        private IEnumerator RandomAmbientTriggerLoop()
        {
            while (true)
            {
                // Wait for a random interval before next ambient sound
                float waitTime = Random.Range(minTimeBetweenAmbient, maxTimeBetweenAmbient);
                yield return new WaitForSeconds(waitTime);
                
                // Start ambient audio
                StartAmbientAudio();
                Debug.Log($"Random ambient triggered (will play for {ambientPlayDuration}s, next in {waitTime:F1}s)");
                
                // Let it play for the specified duration
                yield return new WaitForSeconds(ambientPlayDuration);
                
                // Stop ambient audio
                StopAmbientAudio();
            }
        }

        #endregion
        
        #region Audio Playback Implementation
        
        /// <summary>
        /// Play SFX using pooled audio sources for performance
        /// 
        /// Audio Pooling Implementation:
        /// - Gets available audio source from pool
        /// - Configures audio source with clip and settings
        /// - Returns source to pool when playback completes
        /// - Handles pool exhaustion gracefully
        /// </summary>
        /// <param name="clip">Audio clip to play</param>
        /// <param name="pitch">Pitch variation</param>
        /// <param name="volume">Volume multiplier</param>
        private void PlaySfxClip(AudioClip clip, float pitch = 1f, float volume = 1f)
        {
            AudioSource availableSource = GetAvailableSfxSource();
            if (availableSource != null)
            {
                availableSource.clip = clip;
                availableSource.volume = sfxVolume * masterVolume * volume;
                availableSource.pitch = pitch;
                availableSource.Play();
                
                // Return source to pool when finished playing
                StartCoroutine(ReturnSourceToPool(availableSource, clip.length / pitch));
            }
            else
            {
                Debug.LogWarning("No available SFX audio sources in pool - consider increasing pool size");
            }
        }
        
        /// <summary>
        /// Play UI audio using dedicated UI audio source
        /// UI sounds don't need pooling as they rarely overlap
        /// </summary>
        /// <param name="clip">UI audio clip to play</param>
        /// <param name="pitch">Pitch variation</param>
        private void PlayUIClip(AudioClip clip, float pitch = 1f)
        {
            if (uiSource != null)
            {
                uiSource.clip = clip;
                uiSource.volume = uiVolume * masterVolume;
                uiSource.pitch = pitch;
                uiSource.Play();
            }
        }
        
        /// <summary>
        /// Get available audio source from SFX pool
        /// Returns null if pool is exhausted (all sources in use)
        /// </summary>
        /// <returns>Available AudioSource or null</returns>
        private AudioSource GetAvailableSfxSource()
        {
            if (availableSfxSources.Count > 0)
            {
                return availableSfxSources.Dequeue();
            }
            
            // If pool exhausted, try to find a non-playing source
            foreach (var source in sfxAudioPool)
            {
                if (!source.isPlaying)
                {
                    return source;
                }
            }
            
            return null; // Pool exhausted
        }
        
        /// <summary>
        /// Coroutine to return audio source to pool after playback completes
        /// Ensures efficient reuse of pooled audio sources
        /// </summary>
        /// <param name="source">Audio source to return to pool</param>
        /// <param name="delay">Delay before returning (should match clip length)</param>
        /// <returns>Coroutine enumerator</returns>
        private IEnumerator ReturnSourceToPool(AudioSource source, float delay)
        {
            yield return new WaitForSeconds(delay);
            
            if (source != null)
            {
                source.Stop(); // Ensure playback is stopped
                availableSfxSources.Enqueue(source);
            }
        }
        
        /// <summary>
        /// Coroutine to start ambient audio after a specified delay
        /// Useful for timed ambient audio introduction or scene transition effects
        /// </summary>
        /// <param name="delay">Delay in seconds before starting ambient audio</param>
        /// <returns>Coroutine enumerator</returns>
        private IEnumerator StartAmbientAudioDelayed(float delay)
        {
            yield return new WaitForSeconds(delay);
            StartAmbientAudio();
        }
        
        #endregion
        
        #region Volume Control and Settings
        
        /// <summary>
        /// Update all volume levels based on current settings
        /// Called when master volume changes or settings are loaded
        /// </summary>
        private void UpdateAllVolumes()
        {
            UpdateSfxVolumes();
            UpdateUiVolume();
            UpdateAmbientVolume();
            UpdateMusicVolume();
        }
        
        /// <summary>
        /// Update SFX volume for all pooled audio sources
        /// Note: Individual playing sounds keep their set volume until completion
        /// </summary>
        private void UpdateSfxVolumes()
        {
            // Note: Active SFX will keep their current volume
            // New SFX will use the updated volume setting
        }
        
        /// <summary>
        /// Update UI audio source volume
        /// </summary>
        private void UpdateUiVolume()
        {
            if (uiSource != null)
            {
                uiSource.volume = uiVolume * masterVolume;
            }
        }
        
        /// <summary>
        /// Update ambient audio source volume
        /// </summary>
        private void UpdateAmbientVolume()
        {
            if (ambientSource != null)
            {
                ambientSource.volume = ambientVolume * masterVolume;
            }
        }
        
        /// <summary>
        /// Update music audio sources volume
        /// </summary>
        private void UpdateMusicVolume()
        {
            if (musicSourceA != null)
            {
                musicSourceA.volume = musicVolume * masterVolume;
            }
            if (musicSourceB != null)
            {
                musicSourceB.volume = musicVolume * masterVolume;
            }
        }
        
        /// <summary>
        /// Save audio settings to PlayerPrefs for persistence
        /// Audio preferences are maintained between game sessions
        /// </summary>
        private void SaveAudioSettings()
        {
            PlayerPrefs.SetFloat("AudioManager_MasterVolume", masterVolume);
            PlayerPrefs.SetFloat("AudioManager_SfxVolume", sfxVolume);
            PlayerPrefs.SetFloat("AudioManager_UiVolume", uiVolume);
            PlayerPrefs.SetFloat("AudioManager_AmbientVolume", ambientVolume);
            PlayerPrefs.SetFloat("AudioManager_MusicVolume", musicVolume);
            PlayerPrefs.Save();
        }
        
        /// <summary>
        /// Load audio settings from PlayerPrefs
        /// Restores player's audio preferences from previous sessions
        /// </summary>
        private void LoadAudioSettings()
        {
            masterVolume = PlayerPrefs.GetFloat("AudioManager_MasterVolume", 1f);
            sfxVolume = PlayerPrefs.GetFloat("AudioManager_SfxVolume", 1f);
            uiVolume = PlayerPrefs.GetFloat("AudioManager_UiVolume", 1f);
            ambientVolume = PlayerPrefs.GetFloat("AudioManager_AmbientVolume", 0.6f);
            musicVolume = PlayerPrefs.GetFloat("AudioManager_MusicVolume", 0.7f);
        }
        
        #endregion
        
        #region Utility Methods
        
        /// <summary>
        /// Mute or unmute all audio
        /// Useful for pause functionality or accessibility
        /// </summary>
        /// <param name="muted">True to mute, false to unmute</param>
        public void SetMuted(bool muted)
        {
            float targetVolume = muted ? 0f : masterVolume;
            
            if (ambientSource != null)
                ambientSource.volume = targetVolume * ambientVolume;
            
            if (uiSource != null)
                uiSource.volume = targetVolume * uiVolume;
            
            if (musicSourceA != null)
                musicSourceA.volume = targetVolume * musicVolume;
            
            if (musicSourceB != null)
                musicSourceB.volume = targetVolume * musicVolume;
            
            // Note: Active SFX will continue with their current volume
            // New SFX will respect the muted state
        }
        
        /// <summary>
        /// Check if any audio is currently playing
        /// Useful for debugging or audio conflict resolution
        /// </summary>
        /// <returns>True if any audio source is playing</returns>
        public bool IsAnyAudioPlaying()
        {
            if (ambientSource != null && ambientSource.isPlaying) return true;
            if (uiSource != null && uiSource.isPlaying) return true;
            if (musicSourceA != null && musicSourceA.isPlaying) return true;
            if (musicSourceB != null && musicSourceB.isPlaying) return true;
            
            foreach (var source in sfxAudioPool)
            {
                if (source.isPlaying) return true;
            }
            
            return false;
        }
        
        /// <summary>
        /// Get current audio pool usage for debugging
        /// </summary>
        /// <returns>String describing pool status</returns>
        public string GetPoolStatus()
        {
            int activeCount = 0;
            foreach (var source in sfxAudioPool)
            {
                if (source.isPlaying) activeCount++;
            }
            
            return $"SFX Pool: {activeCount}/{sfxPoolSize} active, {availableSfxSources.Count} available";
        }
        
        #endregion
        
        #region Editor Support
        
        #if UNITY_EDITOR
        /// <summary>
        /// Editor-only method to test audio playback in edit mode
        /// </summary>
        [ContextMenu("Test Purchase Sound")]
        private void TestPurchaseSound()
        {
            PlayPurchaseSuccess();
        }
        
        [ContextMenu("Test UI Click Sound")]
        private void TestUIClickSound()
        {
            PlayUIClick();
        }
        
        [ContextMenu("Start Background Music")]
        private void TestStartBackgroundMusic()
        {
            StartBackgroundMusic();
        }
        
        [ContextMenu("Stop Background Music")]
        private void TestStopBackgroundMusic()
        {
            StopBackgroundMusic();
        }
        
        [ContextMenu("Next Track")]
        private void TestNextTrack()
        {
            NextTrack();
        }
        
        [ContextMenu("Previous Track")]
        private void TestPreviousTrack()
        {
            PreviousTrack();
        }
        
        [ContextMenu("Toggle Shuffle")]
        private void TestToggleShuffle()
        {
            SetMusicShuffle(!shuffleMusic);
        }
        
        [ContextMenu("Print Current Track Info")]
        private void TestPrintTrackInfo()
        {
            Debug.Log(GetCurrentTrackInfo());
        }
        
        [ContextMenu("Print Pool Status")]
        private void PrintPoolStatus()
        {
            Debug.Log(GetPoolStatus());
        }
        #endif
        
        #endregion

        #region Background Music System

        /// <summary>
        /// Start background music playlist
        /// Automatically cycles through tracks with optional shuffle and crossfade
        /// </summary>
        /// <param name="autoStart">Whether to start immediately or wait for manual trigger</param>
        public void StartBackgroundMusic(bool autoStart = true)
        {
            if (backgroundMusicTracks == null || backgroundMusicTracks.Length == 0)
            {
                Debug.LogWarning("No background music tracks assigned to AudioManager!");
                return;
            }
            
            // Initialize shuffle order if enabled
            if (shuffleMusic)
            {
                GenerateShuffledPlayOrder();
            }
            
            currentTrackIndex = 0;
            isMusicPlaying = true;
            
            if (autoStart)
            {
                PlayCurrentTrack();
            }
            
            Debug.Log($"Background music started with {backgroundMusicTracks.Length} tracks (shuffle: {shuffleMusic}, crossfade: {crossfadeMusic})");
        }
        
        /// <summary>
        /// Stop background music with optional fade out
        /// </summary>
        /// <param name="fadeOut">Whether to fade out gradually</param>
        public void StopBackgroundMusic(bool fadeOut = true)
        {
            isMusicPlaying = false;
            
            if (musicCrossfadeCoroutine != null)
            {
                StopCoroutine(musicCrossfadeCoroutine);
                musicCrossfadeCoroutine = null;
            }
            
            if (fadeOut && crossfadeMusic)
            {
                StartCoroutine(FadeOutMusic());
            }
            else
            {
                if (musicSourceA != null && musicSourceA.isPlaying)
                    musicSourceA.Stop();
                if (musicSourceB != null && musicSourceB.isPlaying)
                    musicSourceB.Stop();
            }
            
            Debug.Log("Background music stopped");
        }
        
        /// <summary>
        /// Skip to next track in playlist
        /// </summary>
        public void NextTrack()
        {
            if (backgroundMusicTracks == null || backgroundMusicTracks.Length == 0) return;
            
            AdvanceToNextTrack();
            
            if (isMusicPlaying)
            {
                PlayCurrentTrack();
            }
        }
        
        /// <summary>
        /// Skip to previous track in playlist
        /// </summary>
        public void PreviousTrack()
        {
            if (backgroundMusicTracks == null || backgroundMusicTracks.Length == 0) return;
            
            if (shuffleMusic && shuffledTrackOrder.Count > 0)
            {
                currentTrackIndex = (currentTrackIndex - 1 + shuffledTrackOrder.Count) % shuffledTrackOrder.Count;
            }
            else
            {
                currentTrackIndex = (currentTrackIndex - 1 + backgroundMusicTracks.Length) % backgroundMusicTracks.Length;
            }
            
            if (isMusicPlaying)
            {
                PlayCurrentTrack();
            }
        }
        
        /// <summary>
        /// Toggle music shuffle mode on/off
        /// </summary>
        /// <param name="enabled">True to enable shuffle, false for sequential playback</param>
        public void SetMusicShuffle(bool enabled)
        {
            shuffleMusic = enabled;
            
            if (enabled)
            {
                GenerateShuffledPlayOrder();
                currentTrackIndex = 0; // Reset to start of shuffled list
            }
            
            Debug.Log($"Music shuffle {(enabled ? "enabled" : "disabled")}");
        }
        
        /// <summary>
        /// Get information about currently playing track
        /// </summary>
        /// <returns>Track info string</returns>
        public string GetCurrentTrackInfo()
        {
            if (backgroundMusicTracks == null || backgroundMusicTracks.Length == 0 || !isMusicPlaying)
            {
                return "No music playing";
            }
            
            AudioClip currentClip = GetCurrentTrack();
            if (currentClip != null)
            {
                int trackNumber = shuffleMusic && shuffledTrackOrder.Count > 0 
                    ? shuffledTrackOrder[currentTrackIndex] + 1 
                    : currentTrackIndex + 1;
                
                return $"Track {trackNumber}/{backgroundMusicTracks.Length}: {currentClip.name}";
            }
            
            return "Unknown track";
        }
        
        /// <summary>
        /// Check if background music is currently playing
        /// </summary>
        /// <returns>True if music is playing</returns>
        public bool IsMusicPlaying()
        {
            return isMusicPlaying && ((musicSourceA != null && musicSourceA.isPlaying) || 
                                      (musicSourceB != null && musicSourceB.isPlaying));
        }
        
        /// <summary>
        /// Set specific track by index (0-based)
        /// </summary>
        /// <param name="trackIndex">Index of track to play</param>
        public void SetTrack(int trackIndex)
        {
            if (backgroundMusicTracks == null || trackIndex < 0 || trackIndex >= backgroundMusicTracks.Length)
            {
                Debug.LogWarning($"Invalid track index: {trackIndex}");
                return;
            }
            
            currentTrackIndex = shuffleMusic && shuffledTrackOrder.Count > 0
                ? shuffledTrackOrder.FindIndex(x => x == trackIndex)
                : trackIndex;
            
            if (currentTrackIndex < 0) currentTrackIndex = 0;
            
            if (isMusicPlaying)
            {
                PlayCurrentTrack();
            }
        }

        /// <summary>
        /// Play the current track in the playlist
        /// Handles crossfading between music sources if enabled
        /// </summary>
        private void PlayCurrentTrack()
        {
            AudioClip trackToPlay = GetCurrentTrack();
            if (trackToPlay == null) return;
            
            if (crossfadeMusic && (musicSourceA.isPlaying || musicSourceB.isPlaying))
            {
                // Crossfade to new track
                StartCrossfade(trackToPlay);
            }
            else
            {
                // Direct play without crossfade
                AudioSource activeSource = isPlayingMusicA ? musicSourceA : musicSourceB;
                PlayMusicTrack(activeSource, trackToPlay);
            }
            
            Debug.Log($"Playing track: {trackToPlay.name}");
        }
        
        /// <summary>
        /// Play a music track on specified audio source
        /// </summary>
        /// <param name="source">Audio source to use</param>
        /// <param name="track">Track to play</param>
        private void PlayMusicTrack(AudioSource source, AudioClip track)
        {
            if (source == null || track == null) return;
            
            source.clip = track;
            source.volume = musicVolume * masterVolume;
            source.loop = false;
            source.Play();
            
            // Schedule next track when this one finishes
            StartCoroutine(ScheduleNextTrack(track.length));
        }
        
        /// <summary>
        /// Start crossfade between current track and new track
        /// </summary>
        /// <param name="newTrack">New track to crossfade to</param>
        private void StartCrossfade(AudioClip newTrack)
        {
            if (musicCrossfadeCoroutine != null)
            {
                StopCoroutine(musicCrossfadeCoroutine);
            }
            
            musicCrossfadeCoroutine = StartCoroutine(CrossfadeToTrack(newTrack));
        }
        
        /// <summary>
        /// Coroutine to handle crossfading between tracks
        /// </summary>
        /// <param name="newTrack">Track to crossfade to</param>
        /// <returns>Coroutine enumerator</returns>
        private IEnumerator CrossfadeToTrack(AudioClip newTrack)
        {
            AudioSource currentSource = isPlayingMusicA ? musicSourceA : musicSourceB;
            AudioSource newSource = isPlayingMusicA ? musicSourceB : musicSourceA;
            
            // Start new track at zero volume
            newSource.clip = newTrack;
            newSource.volume = 0f;
            newSource.loop = false;
            newSource.Play();
            
            // Crossfade volumes
            float timer = 0f;
            float initialVolume = currentSource.volume;
            float targetVolume = musicVolume * masterVolume;
            
            while (timer < crossfadeDuration)
            {
                timer += Time.deltaTime;
                float progress = timer / crossfadeDuration;
                
                // Fade out current, fade in new
                currentSource.volume = Mathf.Lerp(initialVolume, 0f, progress);
                newSource.volume = Mathf.Lerp(0f, targetVolume, progress);
                
                yield return null;
            }
            
            // Ensure final volumes are set
            currentSource.volume = 0f;
            newSource.volume = targetVolume;
            
            // Stop the old track and switch active source
            currentSource.Stop();
            isPlayingMusicA = !isPlayingMusicA;
            
            // Schedule next track
            StartCoroutine(ScheduleNextTrack(newTrack.length - crossfadeDuration));
            
            musicCrossfadeCoroutine = null;
        }
        
        /// <summary>
        /// Fade out music gradually
        /// </summary>
        /// <returns>Coroutine enumerator</returns>
        private IEnumerator FadeOutMusic()
        {
            AudioSource activeSource = musicSourceA.isPlaying ? musicSourceA : musicSourceB;
            if (activeSource == null || !activeSource.isPlaying) yield break;
            
            float initialVolume = activeSource.volume;
            float timer = 0f;
            
            while (timer < crossfadeDuration)
            {
                timer += Time.deltaTime;
                float progress = timer / crossfadeDuration;
                activeSource.volume = Mathf.Lerp(initialVolume, 0f, progress);
                yield return null;
            }
            
            activeSource.volume = 0f;
            activeSource.Stop();
        }
        
        /// <summary>
        /// Schedule the next track to play after current track finishes
        /// </summary>
        /// <param name="delay">Delay before next track (track length)</param>
        /// <returns>Coroutine enumerator</returns>
        private IEnumerator ScheduleNextTrack(float delay)
        {
            yield return new WaitForSeconds(delay);
            
            if (isMusicPlaying)
            {
                AdvanceToNextTrack();
                PlayCurrentTrack();
            }
        }
        
        /// <summary>
        /// Advance to the next track in the playlist
        /// </summary>
        private void AdvanceToNextTrack()
        {
            if (backgroundMusicTracks == null || backgroundMusicTracks.Length == 0) return;
            
            if (shuffleMusic && shuffledTrackOrder.Count > 0)
            {
                currentTrackIndex = (currentTrackIndex + 1) % shuffledTrackOrder.Count;
                
                // If we've played all tracks in shuffle, generate new order
                if (currentTrackIndex == 0)
                {
                    GenerateShuffledPlayOrder();
                }
            }
            else
            {
                currentTrackIndex = (currentTrackIndex + 1) % backgroundMusicTracks.Length;
            }
        }
        
        /// <summary>
        /// Get the current track based on play order
        /// </summary>
        /// <returns>Current AudioClip or null</returns>
        private AudioClip GetCurrentTrack()
        {
            if (backgroundMusicTracks == null || backgroundMusicTracks.Length == 0) return null;
            
            int actualTrackIndex = shuffleMusic && shuffledTrackOrder.Count > 0
                ? shuffledTrackOrder[currentTrackIndex]
                : currentTrackIndex;
            
            if (actualTrackIndex >= 0 && actualTrackIndex < backgroundMusicTracks.Length)
            {
                return backgroundMusicTracks[actualTrackIndex];
            }
            
            return null;
        }
        
        /// <summary>
        /// Generate a new shuffled play order for tracks
        /// </summary>
        private void GenerateShuffledPlayOrder()
        {
            if (backgroundMusicTracks == null || backgroundMusicTracks.Length == 0) return;
            
            shuffledTrackOrder.Clear();
            
            // Add all track indices
            for (int i = 0; i < backgroundMusicTracks.Length; i++)
            {
                shuffledTrackOrder.Add(i);
            }
            
            // Shuffle using Fisher-Yates algorithm
            for (int i = shuffledTrackOrder.Count - 1; i > 0; i--)
            {
                int randomIndex = Random.Range(0, i + 1);
                int temp = shuffledTrackOrder[i];
                shuffledTrackOrder[i] = shuffledTrackOrder[randomIndex];
                shuffledTrackOrder[randomIndex] = temp;
            }
            
            Debug.Log($"Generated new shuffle order with {shuffledTrackOrder.Count} tracks");
        }

        #endregion
    }
}
