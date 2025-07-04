using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

namespace TabletopShop
{
    /// <summary>
    /// UI controller for the lore terminal interface
    /// Handles faction selection, depth level navigation, and entry display
    /// </summary>
    public class CodexTerminalUI : MonoBehaviour
    {
        [Header("Main UI Panels")]
        [SerializeField] private GameObject mainMenuPanel;
        [SerializeField] private GameObject factionSelectPanel;
        [SerializeField] private GameObject depthLevelPanel;
        [SerializeField] private GameObject entryDisplayPanel;
        
        [Header("Main Menu")]
        [SerializeField] private Button browseLoreButton;
        
        [Header("Faction Selection")]
        [SerializeField] private Button runebladesButton;
        [SerializeField] private Button voidbornButton;
        [SerializeField] private TextMeshProUGUI runebladesProgressText;
        [SerializeField] private TextMeshProUGUI voidbornProgressText;
        [SerializeField] private Image runebladesProgressFill;
        [SerializeField] private Image voidbornProgressFill;
        
        [Header("Depth Level Navigation")]
        [SerializeField] private TextMeshProUGUI factionTitleText;
        [SerializeField] private Transform depthLevelContainer;
        [SerializeField] private GameObject depthLevelButtonPrefab;
        [SerializeField] private Button backToFactionsButton;
        
        [Header("Entry Display")]
        [SerializeField] private TextMeshProUGUI entryTitleText;
        [SerializeField] private TextMeshProUGUI entryCategoryText;
        [SerializeField] private TextMeshProUGUI entryDescriptionText;
        [SerializeField] private Image entryIcon;
        [SerializeField] private Button backToDepthButton;
        [SerializeField] private Button previousEntryButton;
        [SerializeField] private Button nextEntryButton;
        [SerializeField] private TextMeshProUGUI entryCounterText;
        
        [Header("Visual Settings")]
        [SerializeField] private Color lockedEntryColor = Color.gray;
        [SerializeField] private Color unlockedEntryColor = Color.white;
        [SerializeField] private Color completedDepthColor = Color.green;
        [SerializeField] private Sprite defaultEntryIcon;
        
        [Header("Settings")]
        [SerializeField] private bool enableDebugLogs = true;
        
        // Current state
        private FactionType currentFaction;
        private int currentDepthLevel;
        private List<LoreKnowledgeBaseSO.LoreEntry> currentEntries = new List<LoreKnowledgeBaseSO.LoreEntry>();
        private int currentEntryIndex = 0;
        
        // References
        [SerializeField] private LoreProgressTracker progressTracker;
        [SerializeField] private LoreKnowledgeBaseSO knowledgeBase;
        
        #region Unity Lifecycle
        
        private void Start()
        {
            InitializeReferences();
            SetupUI();
            SetupEventListeners();
            ShowMainMenu();
        }
        
        private void OnEnable()
        {
            // Subscribe to progress events
            if (progressTracker != null)
            {
                progressTracker.OnEntryUnlocked += OnEntryUnlocked;
                progressTracker.OnDepthLevelUnlocked += OnDepthLevelUnlocked;
                progressTracker.OnProgressUpdated += OnProgressUpdated;
            }
        }
        
        private void OnDisable()
        {
            // Unsubscribe from progress events
            if (progressTracker != null)
            {
                progressTracker.OnEntryUnlocked -= OnEntryUnlocked;
                progressTracker.OnDepthLevelUnlocked -= OnDepthLevelUnlocked;
                progressTracker.OnProgressUpdated -= OnProgressUpdated;
            }
        }
        
        #endregion
        
        #region Initialization
        
        private void InitializeReferences()
        {
            if (enableDebugLogs)
                Debug.Log("[CodexTerminalUI] Initializing references...");
            
            progressTracker = LoreProgressTracker.Instance;
            if (progressTracker != null)
            {
                knowledgeBase = progressTracker.KnowledgeBase;
                if (enableDebugLogs)
                    Debug.Log($"[CodexTerminalUI] Found LoreProgressTracker with knowledge base: {knowledgeBase?.name}");
            }
            else
            {
                Debug.LogError("[CodexTerminalUI] LoreProgressTracker.Instance is null! Make sure LoreProgressTracker exists in the scene.");
            }
            
            if (knowledgeBase == null)
            {
                Debug.LogError("[CodexTerminalUI] No knowledge base found! Make sure LoreProgressTracker has a valid knowledge base assigned.");
            }
            else
            {
                if (enableDebugLogs)
                    Debug.Log($"[CodexTerminalUI] Knowledge base loaded with {knowledgeBase.EntryCount} entries");
            }
        }
        
        private void SetupUI()
        {
            // Hide all panels initially
            HideAllPanels();
            
            // Disable navigation buttons initially
            if (previousEntryButton != null) previousEntryButton.interactable = false;
            if (nextEntryButton != null) nextEntryButton.interactable = false;
        }
        
        private void SetupEventListeners()
        {
            // Main menu button
            if (browseLoreButton != null)
                browseLoreButton.onClick.AddListener(ShowFactionSelection);
            
            // Faction selection buttons
            if (runebladesButton != null)
                runebladesButton.onClick.AddListener(() => SelectFaction(FactionType.Runeblades));
            if (voidbornButton != null)
                voidbornButton.onClick.AddListener(() => SelectFaction(FactionType.Voidborn));
            
            // Navigation buttons
            if (backToFactionsButton != null)
                backToFactionsButton.onClick.AddListener(ShowFactionSelection);
            if (backToDepthButton != null)
                backToDepthButton.onClick.AddListener(ShowDepthLevelSelection);
            
            // Entry navigation
            if (previousEntryButton != null)
                previousEntryButton.onClick.AddListener(ShowPreviousEntry);
            if (nextEntryButton != null)
                nextEntryButton.onClick.AddListener(ShowNextEntry);
        }
        
        #endregion
        
        #region Panel Management
        
        private void HideAllPanels()
        {
            if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
            if (factionSelectPanel != null) factionSelectPanel.SetActive(false);
            if (depthLevelPanel != null) depthLevelPanel.SetActive(false);
            if (entryDisplayPanel != null) entryDisplayPanel.SetActive(false);
        }
        
        private void ShowMainMenu()
        {
            HideAllPanels();
            if (mainMenuPanel != null)
            {
                mainMenuPanel.SetActive(true);
            }
            else
            {
                // If no main menu panel, go straight to faction selection
                ShowFactionSelection();
            }
        }
        
        private void ShowFactionSelection()
        {
            HideAllPanels();
            if (factionSelectPanel != null)
            {
                factionSelectPanel.SetActive(true);
                UpdateFactionProgress();
            }
        }
        
        private void ShowDepthLevelSelection()
        {
            HideAllPanels();
            if (depthLevelPanel != null)
            {
                depthLevelPanel.SetActive(true);
                UpdateDepthLevelDisplay();
            }
        }
        
        private void ShowEntryDisplay()
        {
            HideAllPanels();
            if (entryDisplayPanel != null)
            {
                entryDisplayPanel.SetActive(true);
                UpdateEntryDisplay();
            }
        }
        
        #endregion
        
        #region Faction Selection
        
        private void SelectFaction(FactionType faction)
        {
            currentFaction = faction;
            
            if (enableDebugLogs)
                Debug.Log($"[CodexTerminalUI] Selected faction: {faction}");
            
            ShowDepthLevelSelection();
        }
        
        private void UpdateFactionProgress()
        {
            if (knowledgeBase == null) return;
            
            UpdateFactionProgressDisplay(FactionType.Runeblades, runebladesProgressText, runebladesProgressFill);
            UpdateFactionProgressDisplay(FactionType.Voidborn, voidbornProgressText, voidbornProgressFill);
        }
        
        private void UpdateFactionProgressDisplay(FactionType faction, TextMeshProUGUI progressText, Image progressFill)
        {
            var allEntries = knowledgeBase.GetEntriesByFaction(faction);
            var unlockedEntries = allEntries.Where(e => e.IsUnlocked).ToList();
            
            int totalEntries = allEntries.Count;
            int unlockedCount = unlockedEntries.Count;
            
            if (progressText != null)
            {
                progressText.text = $"{unlockedCount}/{totalEntries} Entries Unlocked";
            }
            
            if (progressFill != null)
            {
                float progress = totalEntries > 0 ? (float)unlockedCount / totalEntries : 0f;
                progressFill.fillAmount = progress;
            }
        }
        
        #endregion
        
        #region Depth Level Display
        
        private void UpdateDepthLevelDisplay()
        {
            if (knowledgeBase == null || depthLevelContainer == null) return;
            
            // Update faction title
            if (factionTitleText != null)
            {
                factionTitleText.text = $"{currentFaction} Lore Archives";
            }
            
            // Clear existing buttons
            foreach (Transform child in depthLevelContainer)
            {
                Destroy(child.gameObject);
            }
            
            // Get max depth level for this faction
            int maxDepth = knowledgeBase.GetMaxDepthLevel(currentFaction);
            
            // Create depth level buttons
            for (int depth = 1; depth <= maxDepth; depth++)
            {
                CreateDepthLevelButton(depth);
            }
        }
        
        private void CreateDepthLevelButton(int depthLevel)
        {
            if (depthLevelButtonPrefab == null) return;
            
            GameObject buttonObj = Instantiate(depthLevelButtonPrefab, depthLevelContainer);
            Button button = buttonObj.GetComponent<Button>();
            TextMeshProUGUI buttonText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
            
            if (button == null || buttonText == null) return;
            
            // Get entries for this depth level
            var depthEntries = knowledgeBase.GetEntriesByFactionAndDepth(currentFaction, depthLevel);
            var unlockedEntries = depthEntries.Where(e => e.IsUnlocked).ToList();
            
            // Update button text and state
            buttonText.text = $"Depth Level {depthLevel}\n{unlockedEntries.Count}/{depthEntries.Count} Unlocked";
            
            // Set button color based on completion
            bool isCompleted = unlockedEntries.Count == depthEntries.Count && depthEntries.Count > 0;
            bool hasUnlockedEntries = unlockedEntries.Count > 0;
            
            if (isCompleted)
            {
                buttonText.color = completedDepthColor;
            }
            else if (hasUnlockedEntries)
            {
                buttonText.color = unlockedEntryColor;
            }
            else
            {
                buttonText.color = lockedEntryColor;
            }
            
            // Set button interactable state
            button.interactable = hasUnlockedEntries;
            
            // Add click listener
            int capturedDepth = depthLevel; // Capture for closure
            button.onClick.AddListener(() => SelectDepthLevel(capturedDepth));
        }
        
        private void SelectDepthLevel(int depthLevel)
        {
            currentDepthLevel = depthLevel;
            currentEntries = knowledgeBase.GetUnlockedEntriesByFactionAndDepth(currentFaction, depthLevel);
            currentEntryIndex = 0;
            
            if (enableDebugLogs)
                Debug.Log($"[CodexTerminalUI] Selected depth level {depthLevel} for {currentFaction}. Found {currentEntries.Count} unlocked entries.");
            
            if (currentEntries.Count > 0)
            {
                ShowEntryDisplay();
            }
            else
            {
                Debug.LogWarning($"[CodexTerminalUI] No unlocked entries found for {currentFaction} depth {depthLevel}");
            }
        }
        
        #endregion
        
        #region Entry Display
        
        private void UpdateEntryDisplay()
        {
            if (currentEntries.Count == 0 || currentEntryIndex < 0 || currentEntryIndex >= currentEntries.Count)
            {
                Debug.LogWarning("[CodexTerminalUI] Invalid entry index or no entries to display");
                return;
            }
            
            var entry = currentEntries[currentEntryIndex];
            
            // Update entry content
            if (entryTitleText != null)
                entryTitleText.text = entry.EntryTitle;
            
            if (entryCategoryText != null)
                entryCategoryText.text = entry.Category;
            
            if (entryDescriptionText != null)
                entryDescriptionText.text = entry.Description;
            
            if (entryIcon != null)
                entryIcon.sprite = entry.Icon != null ? entry.Icon : defaultEntryIcon;
            
            if (entryCounterText != null)
                entryCounterText.text = $"{currentEntryIndex + 1} / {currentEntries.Count}";
            
            // Update navigation buttons
            if (previousEntryButton != null)
                previousEntryButton.interactable = currentEntryIndex > 0;
            
            if (nextEntryButton != null)
                nextEntryButton.interactable = currentEntryIndex < currentEntries.Count - 1;
            
            // Mark entry as read
            if (progressTracker != null)
            {
                progressTracker.MarkEntryAsRead(entry);
            }
            
            if (enableDebugLogs)
                Debug.Log($"[CodexTerminalUI] Displaying entry: {entry.EntryTitle} ({currentEntryIndex + 1}/{currentEntries.Count})");
        }
        
        private void ShowPreviousEntry()
        {
            if (currentEntryIndex > 0)
            {
                currentEntryIndex--;
                UpdateEntryDisplay();
            }
        }
        
        private void ShowNextEntry()
        {
            if (currentEntryIndex < currentEntries.Count - 1)
            {
                currentEntryIndex++;
                UpdateEntryDisplay();
            }
        }
        
        #endregion
        
        #region Event Handlers
        
        private void OnEntryUnlocked(LoreKnowledgeBaseSO.LoreEntry entry)
        {
            if (enableDebugLogs)
                Debug.Log($"[CodexTerminalUI] Entry unlocked: {entry.EntryTitle}");
            
            // Update UI if we're currently viewing the same faction/depth
            if (entry.Faction == currentFaction)
            {
                if (depthLevelPanel.activeInHierarchy)
                {
                    UpdateDepthLevelDisplay();
                }
                else if (factionSelectPanel.activeInHierarchy)
                {
                    UpdateFactionProgress();
                }
            }
        }
        
        private void OnDepthLevelUnlocked(FactionType faction, int depthLevel)
        {
            if (enableDebugLogs)
                Debug.Log($"[CodexTerminalUI] Depth level unlocked: {faction} Level {depthLevel}");
            
            // Update UI if relevant
            if (faction == currentFaction && depthLevelPanel.activeInHierarchy)
            {
                UpdateDepthLevelDisplay();
            }
            
            if (factionSelectPanel.activeInHierarchy)
            {
                UpdateFactionProgress();
            }
        }
        
        private void OnProgressUpdated()
        {
            // Refresh current display
            if (factionSelectPanel.activeInHierarchy)
            {
                UpdateFactionProgress();
            }
            else if (depthLevelPanel.activeInHierarchy)
            {
                UpdateDepthLevelDisplay();
            }
        }
        
        #endregion
        
        #region Public Interface
        
        /// <summary>
        /// Open the terminal to a specific faction
        /// </summary>
        public void OpenToFaction(FactionType faction)
        {
            currentFaction = faction;
            ShowDepthLevelSelection();
        }
        
        /// <summary>
        /// Close the terminal UI
        /// </summary>
        public void CloseTerminal()
        {
            gameObject.SetActive(false);
        }
        
        #endregion
        
        #region Debug Methods
        
        /// <summary>
        /// Debug method to test faction selection panel
        /// </summary>
        [ContextMenu("Debug: Show Faction Selection")]
        public void DebugShowFactionSelection()
        {
            Debug.Log("[CodexTerminalUI] Debug: Forcing faction selection panel");
            ShowFactionSelection();
        }
        
        /// <summary>
        /// Debug method to test Runeblades faction
        /// </summary>
        [ContextMenu("Debug: Select Runeblades")]
        public void DebugSelectRuneblades()
        {
            Debug.Log("[CodexTerminalUI] Debug: Selecting Runeblades faction");
            SelectFaction(FactionType.Runeblades);
        }
        
        /// <summary>
        /// Debug method to test Voidborn faction
        /// </summary>
        [ContextMenu("Debug: Select Voidborn")]
        public void DebugSelectVoidborn()
        {
            Debug.Log("[CodexTerminalUI] Debug: Selecting Voidborn faction");
            SelectFaction(FactionType.Voidborn);
        }
        
        /// <summary>
        /// Debug method to check all UI references
        /// </summary>
        [ContextMenu("Debug: Check UI References")]
        public void DebugCheckUIReferences()
        {
            Debug.Log("=== UI REFERENCE CHECK ===");
            Debug.Log($"MainMenuPanel: {(mainMenuPanel != null ? "✓" : "✗ MISSING")}");
            Debug.Log($"FactionSelectPanel: {(factionSelectPanel != null ? "✓" : "✗ MISSING")}");
            Debug.Log($"DepthLevelPanel: {(depthLevelPanel != null ? "✓" : "✗ MISSING")}");
            Debug.Log($"EntryDisplayPanel: {(entryDisplayPanel != null ? "✓" : "✗ MISSING")}");
            Debug.Log($"BrowseLoreButton: {(browseLoreButton != null ? "✓" : "✗ MISSING")}");
            Debug.Log($"RunebladesButton: {(runebladesButton != null ? "✓" : "✗ MISSING")}");
            Debug.Log($"VoidbornButton: {(voidbornButton != null ? "✓" : "✗ MISSING")}");
            Debug.Log($"RunebladesProgressText: {(runebladesProgressText != null ? "✓" : "✗ MISSING")}");
            Debug.Log($"VoidbornProgressText: {(voidbornProgressText != null ? "✓" : "✗ MISSING")}");
            Debug.Log($"DepthLevelContainer: {(depthLevelContainer != null ? "✓" : "✗ MISSING")}");
            Debug.Log($"DepthLevelButtonPrefab: {(depthLevelButtonPrefab != null ? "✓" : "✗ MISSING")}");
            Debug.Log($"ProgressTracker: {(progressTracker != null ? "✓" : "✗ MISSING")}");
            Debug.Log($"KnowledgeBase: {(knowledgeBase != null ? "✓" : "✗ MISSING")}");
            Debug.Log("========================");
        }
        
        /// <summary>
        /// Debug method to test knowledge base data
        /// </summary>
        [ContextMenu("Debug: Check Knowledge Base Data")]
        public void DebugCheckKnowledgeBaseData()
        {
            if (knowledgeBase == null)
            {
                Debug.LogError("Knowledge base is null!");
                return;
            }
            
            Debug.Log("=== KNOWLEDGE BASE DATA ===");
            Debug.Log($"Total entries: {knowledgeBase.EntryCount}");
            
            var runebladesEntries = knowledgeBase.GetEntriesByFaction(FactionType.Runeblades);
            var voidbornEntries = knowledgeBase.GetEntriesByFaction(FactionType.Voidborn);
            
            Debug.Log($"Runeblades entries: {runebladesEntries.Count}");
            foreach (var entry in runebladesEntries)
            {
                Debug.Log($"  - {entry.EntryTitle} (Depth {entry.DepthLevel}, Unlocked: {entry.IsUnlocked})");
            }
            
            Debug.Log($"Voidborn entries: {voidbornEntries.Count}");
            foreach (var entry in voidbornEntries)
            {
                Debug.Log($"  - {entry.EntryTitle} (Depth {entry.DepthLevel}, Unlocked: {entry.IsUnlocked})");
            }
            Debug.Log("===========================");
        }
        
        /// <summary>
        /// Public method to open terminal (for button connections)
        /// </summary>
        public void OpenTerminal()
        {
            gameObject.SetActive(true);
            ShowMainMenu();
        }
        
        #endregion
    }
}