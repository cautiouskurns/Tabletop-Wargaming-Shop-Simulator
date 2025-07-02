using UnityEngine;
using System.Collections.Generic;
using System;
using System.IO;

namespace TabletopShop
{
    /// <summary>
    /// Singleton system for tracking lore progression and managing unlock conditions
    /// Handles save/load functionality and progress notifications
    /// </summary>
    public class LoreProgressTracker : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private LoreKnowledgeBaseSO knowledgeBase;
        
        [Header("Progress Tracking")]
        [SerializeField] private float timePlayedMinutes = 0f;
        [SerializeField] private int customersServed = 0;
        [SerializeField] private Dictionary<string, bool> gameEventsTriggered = new Dictionary<string, bool>();
        
        [Header("Settings")]
        [SerializeField] private bool enableDebugLogs = true;
        [SerializeField] private string saveFileName = "lore_progress.json";
        
        // Singleton instance
        public static LoreProgressTracker Instance { get; private set; }
        
        // Events for UI updates
        public event Action<LoreKnowledgeBaseSO.LoreEntry> OnEntryUnlocked;
        public event Action<FactionType, int> OnDepthLevelUnlocked;
        public event Action OnProgressUpdated;
        
        // Progress data for serialization
        [System.Serializable]
        public class ProgressData
        {
            public float timePlayedMinutes;
            public int customersServed;
            public List<string> unlockedEntries = new List<string>();
            public List<string> readEntries = new List<string>();
            public Dictionary<string, int> entryReadCounts = new Dictionary<string, int>();
            public List<string> triggeredEvents = new List<string>();
        }
        
        private ProgressData currentProgress = new ProgressData();
        private float sessionStartTime;
        
        // Properties for external access
        public float TimePlayedMinutes => timePlayedMinutes;
        public int CustomersServed => customersServed;
        public LoreKnowledgeBaseSO KnowledgeBase => knowledgeBase;
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            // Singleton pattern
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                sessionStartTime = Time.time;
                LoadProgress();
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        private void Start()
        {
            // Initialize and check for any unlocks based on current progress
            CheckAllUnlockConditions();
        }
        
        private void Update()
        {
            // Update time played
            timePlayedMinutes = currentProgress.timePlayedMinutes + ((Time.time - sessionStartTime) / 60f);
        }
        
        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
                SaveProgress();
        }
        
        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus)
                SaveProgress();
        }
        
        private void OnDestroy()
        {
            if (Instance == this)
                SaveProgress();
        }
        
        #endregion
        
        #region Progress Tracking
        
        /// <summary>
        /// Called when a customer completes their visit
        /// </summary>
        public void OnCustomerServed()
        {
            customersServed++;
            currentProgress.customersServed = customersServed;
            
            if (enableDebugLogs)
                Debug.Log($"[LoreProgressTracker] Customer served. Total: {customersServed}");
            
            CheckAllUnlockConditions();
            OnProgressUpdated?.Invoke();
        }
        
        /// <summary>
        /// Trigger a custom game event for unlock conditions
        /// </summary>
        public void TriggerGameEvent(string eventName)
        {
            if (!gameEventsTriggered.ContainsKey(eventName))
            {
                gameEventsTriggered[eventName] = true;
                currentProgress.triggeredEvents.Add(eventName);
                
                if (enableDebugLogs)
                    Debug.Log($"[LoreProgressTracker] Game event triggered: {eventName}");
                
                CheckAllUnlockConditions();
                OnProgressUpdated?.Invoke();
            }
        }
        
        /// <summary>
        /// Mark a lore entry as read
        /// </summary>
        public void MarkEntryAsRead(LoreKnowledgeBaseSO.LoreEntry entry)
        {
            if (entry == null) return;
            
            string entryKey = $"{entry.FactionName}_{entry.EntryTitle}";
            
            // Update read count
            if (!currentProgress.entryReadCounts.ContainsKey(entryKey))
                currentProgress.entryReadCounts[entryKey] = 0;
            
            currentProgress.entryReadCounts[entryKey]++;
            
            // Add to read entries list if first time
            if (!currentProgress.readEntries.Contains(entryKey))
                currentProgress.readEntries.Add(entryKey);
            
            // Mark the entry as read
            entry.MarkAsRead();
            
            if (enableDebugLogs)
                Debug.Log($"[LoreProgressTracker] Entry read: {entry.EntryTitle} (Count: {currentProgress.entryReadCounts[entryKey]})");
            
            OnProgressUpdated?.Invoke();
        }
        
        #endregion
        
        #region Unlock Logic
        
        /// <summary>
        /// Check all entries for unlock conditions
        /// </summary>
        private void CheckAllUnlockConditions()
        {
            if (knowledgeBase == null) return;
            
            foreach (var entry in knowledgeBase.AllEntries)
            {
                if (!entry.IsUnlocked && ShouldUnlockEntry(entry))
                {
                    UnlockEntry(entry);
                }
            }
        }
        
        /// <summary>
        /// Check if an entry should be unlocked based on current progress
        /// </summary>
        private bool ShouldUnlockEntry(LoreKnowledgeBaseSO.LoreEntry entry)
        {
            switch (entry.UnlockCondition)
            {
                case UnlockConditionType.AlwaysAvailable:
                    return true;
                    
                case UnlockConditionType.TimePlayedMinutes:
                    return timePlayedMinutes >= entry.UnlockRequirement;
                    
                case UnlockConditionType.CustomersServed:
                    return customersServed >= entry.UnlockRequirement;
                    
                case UnlockConditionType.PreviousDepthUnlocked:
                    return IsPreviousDepthUnlocked(entry.Faction, entry.DepthLevel);
                    
                case UnlockConditionType.GameEventTriggered:
                    return gameEventsTriggered.ContainsKey(entry.UnlockDescription) && 
                           gameEventsTriggered[entry.UnlockDescription];
                    
                default:
                    return false;
            }
        }
        
        /// <summary>
        /// Check if the previous depth level is unlocked for a faction
        /// </summary>
        private bool IsPreviousDepthUnlocked(FactionType faction, int currentDepth)
        {
            if (currentDepth <= 1) return true; // First depth is always available
            
            var previousDepthEntries = knowledgeBase.GetEntriesByFactionAndDepth(faction, currentDepth - 1);
            
            // All entries in the previous depth level must be unlocked
            foreach (var entry in previousDepthEntries)
            {
                if (!entry.IsUnlocked)
                    return false;
            }
            
            return previousDepthEntries.Count > 0; // Must have entries in previous depth
        }
        
        /// <summary>
        /// Unlock a specific entry
        /// </summary>
        private void UnlockEntry(LoreKnowledgeBaseSO.LoreEntry entry)
        {
            entry.Unlock();
            
            string entryKey = $"{entry.FactionName}_{entry.EntryTitle}";
            if (!currentProgress.unlockedEntries.Contains(entryKey))
                currentProgress.unlockedEntries.Add(entryKey);
            
            if (enableDebugLogs)
                Debug.Log($"[LoreProgressTracker] Entry unlocked: {entry.EntryTitle} (Faction: {entry.FactionName}, Depth: {entry.DepthLevel})");
            
            // Fire events
            OnEntryUnlocked?.Invoke(entry);
            
            // Check if this unlocks a new depth level
            CheckDepthLevelUnlocked(entry.Faction, entry.DepthLevel);
            
            OnProgressUpdated?.Invoke();
        }
        
        /// <summary>
        /// Check if unlocking this entry completes a depth level
        /// </summary>
        private void CheckDepthLevelUnlocked(FactionType faction, int depthLevel)
        {
            var depthEntries = knowledgeBase.GetEntriesByFactionAndDepth(faction, depthLevel);
            bool allUnlocked = true;
            
            foreach (var entry in depthEntries)
            {
                if (!entry.IsUnlocked)
                {
                    allUnlocked = false;
                    break;
                }
            }
            
            if (allUnlocked && depthEntries.Count > 0)
            {
                if (enableDebugLogs)
                    Debug.Log($"[LoreProgressTracker] Depth level unlocked: {faction} Level {depthLevel}");
                
                OnDepthLevelUnlocked?.Invoke(faction, depthLevel);
            }
        }
        
        #endregion
        
        #region Save/Load System
        
        /// <summary>
        /// Save current progress to file
        /// </summary>
        public void SaveProgress()
        {
            try
            {
                // Update time before saving
                currentProgress.timePlayedMinutes = timePlayedMinutes;
                currentProgress.customersServed = customersServed;
                
                string json = JsonUtility.ToJson(currentProgress, true);
                string filePath = Path.Combine(Application.persistentDataPath, saveFileName);
                File.WriteAllText(filePath, json);
                
                if (enableDebugLogs)
                    Debug.Log($"[LoreProgressTracker] Progress saved to: {filePath}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[LoreProgressTracker] Failed to save progress: {e.Message}");
            }
        }
        
        /// <summary>
        /// Load progress from file
        /// </summary>
        public void LoadProgress()
        {
            try
            {
                string filePath = Path.Combine(Application.persistentDataPath, saveFileName);
                
                if (File.Exists(filePath))
                {
                    string json = File.ReadAllText(filePath);
                    currentProgress = JsonUtility.FromJson<ProgressData>(json);
                    
                    // Apply loaded data
                    timePlayedMinutes = currentProgress.timePlayedMinutes;
                    customersServed = currentProgress.customersServed;
                    
                    // Restore game events
                    gameEventsTriggered.Clear();
                    foreach (string eventName in currentProgress.triggeredEvents)
                    {
                        gameEventsTriggered[eventName] = true;
                    }
                    
                    // Apply unlocked entries to knowledge base
                    ApplyProgressToKnowledgeBase();
                    
                    if (enableDebugLogs)
                        Debug.Log($"[LoreProgressTracker] Progress loaded: {timePlayedMinutes:F1} minutes, {customersServed} customers");
                }
                else
                {
                    if (enableDebugLogs)
                        Debug.Log("[LoreProgressTracker] No save file found, starting fresh");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[LoreProgressTracker] Failed to load progress: {e.Message}");
            }
        }
        
        /// <summary>
        /// Apply loaded progress data to the knowledge base
        /// </summary>
        private void ApplyProgressToKnowledgeBase()
        {
            if (knowledgeBase == null) return;
            
            // Unlock entries based on saved progress
            foreach (string entryKey in currentProgress.unlockedEntries)
            {
                string[] parts = entryKey.Split('_');
                if (parts.Length >= 2)
                {
                    string factionName = parts[0];
                    string entryTitle = string.Join("_", parts, 1, parts.Length - 1);
                    
                    var entry = knowledgeBase.FindEntry(factionName, entryTitle);
                    if (entry != null && !entry.IsUnlocked)
                    {
                        entry.Unlock();
                    }
                }
            }
            
            // Apply read counts
            foreach (var kvp in currentProgress.entryReadCounts)
            {
                string[] parts = kvp.Key.Split('_');
                if (parts.Length >= 2)
                {
                    string factionName = parts[0];
                    string entryTitle = string.Join("_", parts, 1, parts.Length - 1);
                    
                    var entry = knowledgeBase.FindEntry(factionName, entryTitle);
                    if (entry != null)
                    {
                        // Apply read count by calling MarkAsRead multiple times
                        for (int i = entry.ReadCount; i < kvp.Value; i++)
                        {
                            entry.MarkAsRead();
                        }
                    }
                }
            }
        }
        
        #endregion
        
        #region Debug Methods
        
        /// <summary>
        /// Reset all progress (for testing)
        /// </summary>
        [ContextMenu("Reset Progress")]
        public void ResetProgress()
        {
            currentProgress = new ProgressData();
            timePlayedMinutes = 0f;
            customersServed = 0;
            gameEventsTriggered.Clear();
            sessionStartTime = Time.time;
            
            // Lock all entries except those that are always available
            if (knowledgeBase != null)
            {
                foreach (var entry in knowledgeBase.AllEntries)
                {
                    if (entry.UnlockCondition != UnlockConditionType.AlwaysAvailable)
                    {
                        entry.Lock();
                    }
                }
            }
            
            SaveProgress();
            OnProgressUpdated?.Invoke();
            
            Debug.Log("[LoreProgressTracker] Progress reset");
        }
        
        /// <summary>
        /// Unlock all entries (for testing)
        /// </summary>
        [ContextMenu("Unlock All Entries")]
        public void UnlockAllEntries()
        {
            if (knowledgeBase == null) return;
            
            foreach (var entry in knowledgeBase.AllEntries)
            {
                if (!entry.IsUnlocked)
                {
                    UnlockEntry(entry);
                }
            }
            
            Debug.Log("[LoreProgressTracker] All entries unlocked");
        }
        
        #endregion
    }
}