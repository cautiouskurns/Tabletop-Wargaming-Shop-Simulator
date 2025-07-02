using UnityEngine;
using System.Collections.Generic;
using System;

namespace TabletopShop
{
    public enum FactionType
    {
        Runeblades,
        Voidborn
    }
    
    public enum UnlockConditionType
    {
        AlwaysAvailable,
        TimePlayedMinutes,
        CustomersServed,
        PreviousDepthUnlocked,
        GameEventTriggered
    }
    
    /// <summary>
    /// ScriptableObject that stores structured lore entries for the tabletop shop simulation.
    /// Enhanced with faction depth levels and unlock progression system.
    /// </summary>
    [CreateAssetMenu(fileName = "New Lore Knowledge Base", menuName = "Tabletop Shop/Lore Knowledge Base")]
    public class LoreKnowledgeBaseSO : ScriptableObject
    {
        [Header("Lore Knowledge Base")]
        [SerializeField] private List<LoreEntry> loreEntries = new List<LoreEntry>();
        
        /// <summary>
        /// Individual lore entry containing faction-specific information with depth progression
        /// </summary>
        [System.Serializable]
        public class LoreEntry
        {
            [Header("Basic Information")]
            [SerializeField] private FactionType faction;
            [SerializeField] private string entryTitle;
            [SerializeField] private string category;
            
            [Header("Depth & Progression")]
            [SerializeField] [Range(1, 5)] private int depthLevel = 1;
            [SerializeField] public UnlockConditionType unlockCondition = UnlockConditionType.AlwaysAvailable;
            [SerializeField] public float unlockRequirement = 0f;
            [SerializeField] public string unlockDescription = "Available from start";
            
            [Header("Content")]
            [TextArea(3, 8)]
            [SerializeField] private string description;
            [SerializeField] private string previewText = "Mysterious knowledge awaits...";
            
            [Header("Visual")]
            [SerializeField] private Sprite icon;
            
            [Header("Runtime Data")]
            [SerializeField] public bool isUnlocked = true;
            [SerializeField] private int readCount = 0;
            [SerializeField] private DateTime firstReadTime;
            [SerializeField] private DateTime lastReadTime;
            
            // Properties for external access
            public FactionType Faction => faction;
            public string FactionName => faction.ToString();
            public string EntryTitle => entryTitle;
            public string Category => category;
            public int DepthLevel => depthLevel;
            public UnlockConditionType UnlockCondition => unlockCondition;
            public float UnlockRequirement => unlockRequirement;
            public string UnlockDescription => unlockDescription;
            public string Description => description;
            public string PreviewText => previewText;
            public Sprite Icon => icon;
            public bool IsUnlocked => isUnlocked;
            public int ReadCount => readCount;
            public DateTime FirstReadTime => firstReadTime;
            public DateTime LastReadTime => lastReadTime;
            
            /// <summary>
            /// Constructor for creating lore entries programmatically
            /// </summary>
            public LoreEntry(FactionType faction, string title, string category, string description, int depthLevel = 1, Sprite icon = null)
            {
                this.faction = faction;
                this.entryTitle = title;
                this.category = category;
                this.description = description;
                this.depthLevel = depthLevel;
                this.icon = icon;
                this.unlockCondition = depthLevel == 1 ? UnlockConditionType.AlwaysAvailable : UnlockConditionType.PreviousDepthUnlocked;
                this.isUnlocked = depthLevel == 1;
                this.readCount = 0;
            }
            
            /// <summary>
            /// Mark this entry as read (for tracking player progress)
            /// </summary>
            public void MarkAsRead()
            {
                readCount++;
                if (readCount == 1)
                    firstReadTime = DateTime.Now;
                lastReadTime = DateTime.Now;
            }
            
            /// <summary>
            /// Unlock this entry
            /// </summary>
            public void Unlock()
            {
                isUnlocked = true;
            }
            
            /// <summary>
            /// Lock this entry (for testing or resetting)
            /// </summary>
            public void Lock()
            {
                isUnlocked = false;
            }
            
            /// <summary>
            /// Check if this entry has been read before
            /// </summary>
            public bool HasBeenRead => readCount > 0;
        }
        
        // Properties for external access
        public List<LoreEntry> AllEntries => loreEntries;
        public int EntryCount => loreEntries.Count;
        
        /// <summary>
        /// Get all lore entries for a specific faction
        /// </summary>
        public List<LoreEntry> GetEntriesByFaction(FactionType faction)
        {
            List<LoreEntry> factionEntries = new List<LoreEntry>();
            
            foreach (LoreEntry entry in loreEntries)
            {
                if (entry.Faction == faction)
                {
                    factionEntries.Add(entry);
                }
            }
            
            return factionEntries;
        }
        
        /// <summary>
        /// Get all lore entries for a specific faction by string name (legacy support)
        /// </summary>
        public List<LoreEntry> GetEntriesByFaction(string factionName)
        {
            if (System.Enum.TryParse<FactionType>(factionName, true, out FactionType faction))
            {
                return GetEntriesByFaction(faction);
            }
            return new List<LoreEntry>();
        }
        
        /// <summary>
        /// Get all lore entries for a specific category
        /// </summary>
        public List<LoreEntry> GetEntriesByCategory(string category)
        {
            List<LoreEntry> categoryEntries = new List<LoreEntry>();
            
            foreach (LoreEntry entry in loreEntries)
            {
                if (entry.Category.Equals(category, System.StringComparison.OrdinalIgnoreCase))
                {
                    categoryEntries.Add(entry);
                }
            }
            
            return categoryEntries;
        }
        
        /// <summary>
        /// Get all faction types in this knowledge base
        /// </summary>
        public List<FactionType> GetAllFactions()
        {
            HashSet<FactionType> factions = new HashSet<FactionType>();
            
            foreach (LoreEntry entry in loreEntries)
            {
                factions.Add(entry.Faction);
            }
            
            return new List<FactionType>(factions);
        }
        
        /// <summary>
        /// Get all unique faction names as strings (legacy support)
        /// </summary>
        public List<string> GetAllFactionNames()
        {
            HashSet<string> factions = new HashSet<string>();
            
            foreach (LoreEntry entry in loreEntries)
            {
                factions.Add(entry.FactionName);
            }
            
            return new List<string>(factions);
        }
        
        /// <summary>
        /// Get all unique categories in this knowledge base
        /// </summary>
        public List<string> GetAllCategories()
        {
            HashSet<string> categories = new HashSet<string>();
            
            foreach (LoreEntry entry in loreEntries)
            {
                categories.Add(entry.Category);
            }
            
            return new List<string>(categories);
        }
        
        /// <summary>
        /// Add a new lore entry to the knowledge base
        /// </summary>
        public void AddEntry(LoreEntry entry)
        {
            if (entry != null && !loreEntries.Contains(entry))
            {
                loreEntries.Add(entry);
            }
        }
        
        /// <summary>
        /// Remove a lore entry from the knowledge base
        /// </summary>
        public bool RemoveEntry(LoreEntry entry)
        {
            return loreEntries.Remove(entry);
        }
        
        /// <summary>
        /// Find a specific lore entry by faction and title
        /// </summary>
        public LoreEntry FindEntry(FactionType faction, string entryTitle)
        {
            foreach (LoreEntry entry in loreEntries)
            {
                if (entry.Faction == faction &&
                    entry.EntryTitle.Equals(entryTitle, System.StringComparison.OrdinalIgnoreCase))
                {
                    return entry;
                }
            }
            
            return null;
        }
        
        /// <summary>
        /// Find a specific lore entry by faction name and title (legacy support)
        /// </summary>
        public LoreEntry FindEntry(string factionName, string entryTitle)
        {
            if (System.Enum.TryParse<FactionType>(factionName, true, out FactionType faction))
            {
                return FindEntry(faction, entryTitle);
            }
            return null;
        }
        
        /// <summary>
        /// Get entries by faction and depth level
        /// </summary>
        public List<LoreEntry> GetEntriesByFactionAndDepth(FactionType faction, int depthLevel)
        {
            List<LoreEntry> entries = new List<LoreEntry>();
            
            foreach (LoreEntry entry in loreEntries)
            {
                if (entry.Faction == faction && entry.DepthLevel == depthLevel)
                {
                    entries.Add(entry);
                }
            }
            
            return entries;
        }
        
        /// <summary>
        /// Get unlocked entries by faction and depth level
        /// </summary>
        public List<LoreEntry> GetUnlockedEntriesByFactionAndDepth(FactionType faction, int depthLevel)
        {
            List<LoreEntry> entries = new List<LoreEntry>();
            
            foreach (LoreEntry entry in loreEntries)
            {
                if (entry.Faction == faction && entry.DepthLevel == depthLevel && entry.IsUnlocked)
                {
                    entries.Add(entry);
                }
            }
            
            return entries;
        }
        
        /// <summary>
        /// Get the maximum depth level for a faction
        /// </summary>
        public int GetMaxDepthLevel(FactionType faction)
        {
            int maxDepth = 0;
            
            foreach (LoreEntry entry in loreEntries)
            {
                if (entry.Faction == faction && entry.DepthLevel > maxDepth)
                {
                    maxDepth = entry.DepthLevel;
                }
            }
            
            return maxDepth;
        }
        
        #region Editor Helper Methods
        
        /// <summary>
        /// Create sample lore entries with depth progression for testing
        /// </summary>
        [ContextMenu("Add Sample Entries")]
        public void CreateSampleEntries()
        {
            // Clear existing entries
            loreEntries.Clear();
            
            CreateRunebladesEntries();
            CreateVoidbornEntries();
            
            Debug.Log($"Created {loreEntries.Count} lore entries with depth progression.");
        }
        
        private void CreateRunebladesEntries()
        {
            // DEPTH LEVEL 1 - Always Available
            var entry1 = new LoreEntry(FactionType.Runeblades, "The Runeforged Legion", "Characters",
                "Elite warriors who have bonded their souls to ancient runic weapons. Each Runeblade carries a sword, axe, or spear inscribed with mystical runes that grant supernatural abilities.\n\nLed by Warmaster Thane Ironrune, the Legion serves as both protector and enforcer for the realm's most sacred sites.", 1);
            loreEntries.Add(entry1);
            
            var entry2 = new LoreEntry(FactionType.Runeblades, "Runic Weapons", "Technology",
                "Ancient weapons forged in the First Age, each inscribed with runes of power. These weapons are living entities that choose their wielders.\n\nFive types exist: Flamestrike (fire), Frostbite (ice), Stormcaller (lightning), Earthshaker (earth), and Voidrender (dark energy).", 1);
            loreEntries.Add(entry2);
            
            // DEPTH LEVEL 2 - Unlocked after serving customers
            var entry3 = new LoreEntry(FactionType.Runeblades, "The Great Forging", "History",
                "Three thousand years ago, during the War of Shadows, the greatest smiths and mages united to create weapons capable of standing against Void corruption.\n\nThe Great Forging lasted seven years and cost dozens of lives, but turned the tide of war.", 2);
            entry3.unlockCondition = UnlockConditionType.CustomersServed;
            entry3.unlockRequirement = 5f;
            entry3.unlockDescription = "Serve 5 customers";
            entry3.isUnlocked = false;
            loreEntries.Add(entry3);
            
            var entry4 = new LoreEntry(FactionType.Runeblades, "Warmaster Thane Ironrune", "Characters",
                "The legendary leader of the Runeforged Legion, wielding the massive runic sword 'Doombreaker' for over fifty years. His runes channel pure force that can shatter castle walls.\n\nThane's tactical brilliance and unwavering dedication have kept the Legion united through countless battles.", 2);
            entry4.unlockCondition = UnlockConditionType.CustomersServed;
            entry4.unlockRequirement = 5f;
            entry4.unlockDescription = "Serve 5 customers";
            entry4.isUnlocked = false;
            loreEntries.Add(entry4);
            
            // DEPTH LEVEL 3 - Unlocked after time played
            var entry5 = new LoreEntry(FactionType.Runeblades, "The Forge of Eternal Flames", "Locations",
                "Hidden in the Ironpeak Mountains, this forge burns with fires that never die. Legend says the flames were lit by dragons and blessed by gods.\n\nAncient meteoric anvils and perfect-pitch hammers guide smiths in creating the most powerful runic weapons.", 3);
            entry5.unlockCondition = UnlockConditionType.TimePlayedMinutes;
            entry5.unlockRequirement = 10f;
            entry5.unlockDescription = "Play for 10 minutes";
            entry5.isUnlocked = false;
            loreEntries.Add(entry5);
        }
        
        private void CreateVoidbornEntries()
        {
            // DEPTH LEVEL 1 - Always Available
            var entry1 = new LoreEntry(FactionType.Voidborn, "The Corrupted", "Characters",
                "Once-mortal beings transformed by Void energy. Their flesh becomes crystalline and their eyes glow purple. They retain intelligence but gain terrible abilities.\n\nMany struggle with their lost humanity, caught between their former lives and dark nature.", 1);
            loreEntries.Add(entry1);
            
            var entry2 = new LoreEntry(FactionType.Voidborn, "Void Crystals", "Technology",
                "Purple-black crystals that grow where Void influence is strongest. They pulse with malevolent energy and can corrupt living beings.\n\nThe crystals seem collectively intelligent, growing in purposeful patterns that may be fragments of a greater Void consciousness.", 1);
            loreEntries.Add(entry2);
            
            // DEPTH LEVEL 2 - Unlocked after serving customers
            var entry3 = new LoreEntry(FactionType.Voidborn, "The Void Incursion", "History",
                "The catastrophic event that began the War of Shadows. Reality was torn open, allowing Void creatures and energy to pour into our world.\n\nThe Incursion lasted only three days, but its effects ripple through history. Smaller rifts still appear randomly.", 2);
            entry3.unlockCondition = UnlockConditionType.CustomersServed;
            entry3.unlockRequirement = 5f;
            entry3.unlockDescription = "Serve 5 customers";
            entry3.isUnlocked = false;
            loreEntries.Add(entry3);
            
            var entry4 = new LoreEntry(FactionType.Voidborn, "Shadowlord Malachar", "Characters",
                "The first and most powerful Corrupted, once a noble king. His attempts to harness Void energy for defense transformed him into something beyond mortal comprehension.\n\nNow twelve feet tall with obsidian flesh, he seeks to 'elevate' all life through Void corruption.", 2);
            entry4.unlockCondition = UnlockConditionType.CustomersServed;
            entry4.unlockRequirement = 5f;
            entry4.unlockDescription = "Serve 5 customers";
            entry4.isUnlocked = false;
            loreEntries.Add(entry4);
            
            // DEPTH LEVEL 3 - Unlocked after time played
            var entry5 = new LoreEntry(FactionType.Voidborn, "The Whispering Void", "Locations",
                "A massive crater where reality remains unstable. Purple energy shimmers and whispers echo from shadows. Time moves differently here.\n\nMassive Void Crystals serve as focal points for rituals and gateways for creatures from the Void dimension.", 3);
            entry5.unlockCondition = UnlockConditionType.TimePlayedMinutes;
            entry5.unlockRequirement = 10f;
            entry5.unlockDescription = "Play for 10 minutes";
            entry5.isUnlocked = false;
            loreEntries.Add(entry5);
        }
        
        /// <summary>
        /// Validate all entries and report any issues
        /// </summary>
        [ContextMenu("Validate Entries")]
        public void ValidateEntries()
        {
            int validEntries = 0;
            int invalidEntries = 0;
            
            foreach (LoreEntry entry in loreEntries)
            {
                bool isValid = true;
                
                if (string.IsNullOrEmpty(entry.FactionName))
                {
                    Debug.LogWarning($"Entry '{entry.EntryTitle}' has no faction name");
                    isValid = false;
                }
                
                if (string.IsNullOrEmpty(entry.EntryTitle))
                {
                    Debug.LogWarning($"Entry has no title (Faction: {entry.FactionName})");
                    isValid = false;
                }
                
                if (string.IsNullOrEmpty(entry.Category))
                {
                    Debug.LogWarning($"Entry '{entry.EntryTitle}' has no category");
                    isValid = false;
                }
                
                if (string.IsNullOrEmpty(entry.Description))
                {
                    Debug.LogWarning($"Entry '{entry.EntryTitle}' has no description");
                    isValid = false;
                }
                
                if (isValid)
                    validEntries++;
                else
                    invalidEntries++;
            }
            
            Debug.Log($"Validation complete: {validEntries} valid entries, {invalidEntries} invalid entries");
        }
        
        #endregion
    }
}