using UnityEngine;
using System.Collections.Generic;

namespace TabletopShop
{
    /// <summary>
    /// ScriptableObject that stores structured lore entries for the tabletop shop simulation.
    /// Contains multiple lore entries that can be browsed through the in-game codex system.
    /// </summary>
    [CreateAssetMenu(fileName = "New Lore Knowledge Base", menuName = "Tabletop Shop/Lore Knowledge Base")]
    public class LoreKnowledgeBaseSO : ScriptableObject
    {
        [Header("Lore Knowledge Base")]
        [SerializeField] private List<LoreEntry> loreEntries = new List<LoreEntry>();
        
        /// <summary>
        /// Individual lore entry containing faction-specific information
        /// </summary>
        [System.Serializable]
        public class LoreEntry
        {
            [Header("Basic Information")]
            [SerializeField] private string factionName;
            [SerializeField] private string entryTitle;
            [SerializeField] private string category;
            
            [Header("Content")]
            [TextArea(3, 8)]
            [SerializeField] private string description;
            
            [Header("Visual")]
            [SerializeField] private Sprite icon;
            
            [Header("Metadata")]
            [SerializeField] private bool isUnlocked = true;
            [SerializeField] private int readCount = 0;
            
            // Properties for external access
            public string FactionName => factionName;
            public string EntryTitle => entryTitle;
            public string Category => category;
            public string Description => description;
            public Sprite Icon => icon;
            public bool IsUnlocked => isUnlocked;
            public int ReadCount => readCount;
            
            /// <summary>
            /// Constructor for creating lore entries programmatically
            /// </summary>
            public LoreEntry(string faction, string title, string category, string description, Sprite icon = null)
            {
                this.factionName = faction;
                this.entryTitle = title;
                this.category = category;
                this.description = description;
                this.icon = icon;
                this.isUnlocked = true;
                this.readCount = 0;
            }
            
            /// <summary>
            /// Mark this entry as read (for tracking player progress)
            /// </summary>
            public void MarkAsRead()
            {
                readCount++;
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
        public List<LoreEntry> GetEntriesByFaction(string factionName)
        {
            List<LoreEntry> factionEntries = new List<LoreEntry>();
            
            foreach (LoreEntry entry in loreEntries)
            {
                if (entry.FactionName.Equals(factionName, System.StringComparison.OrdinalIgnoreCase))
                {
                    factionEntries.Add(entry);
                }
            }
            
            return factionEntries;
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
        /// Get all unique faction names in this knowledge base
        /// </summary>
        public List<string> GetAllFactions()
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
        public LoreEntry FindEntry(string factionName, string entryTitle)
        {
            foreach (LoreEntry entry in loreEntries)
            {
                if (entry.FactionName.Equals(factionName, System.StringComparison.OrdinalIgnoreCase) &&
                    entry.EntryTitle.Equals(entryTitle, System.StringComparison.OrdinalIgnoreCase))
                {
                    return entry;
                }
            }
            
            return null;
        }
        
        #region Editor Helper Methods
        
        /// <summary>
        /// Create sample lore entries for testing (called from custom editor or initialization)
        /// </summary>
        [ContextMenu("Add Sample Entries")]
        public void CreateSampleEntries()
        {
            // Clear existing entries
            loreEntries.Clear();
            
            // Runeblades faction entries
            loreEntries.Add(new LoreEntry(
                "Runeblades",
                "The Runeforged Legion",
                "Characters",
                "Elite warriors who have bonded their souls to ancient runic weapons. Each Runeblade carries a sword, axe, or spear inscribed with mystical runes that grant supernatural abilities. The bonding process is dangerous - many who attempt it are consumed by the weapon's power, but those who succeed become nearly unstoppable in battle.\n\nLed by Warmaster Thane Ironrune, the Legion serves as both protector and enforcer for the realm's most sacred sites."
            ));
            
            loreEntries.Add(new LoreEntry(
                "Runeblades",
                "Runic Weapons",
                "Technology",
                "Ancient weapons forged in the First Age, each inscribed with runes of power. These weapons are not merely tools but living entities that choose their wielders. The runes glow with inner fire during combat, channeling elemental forces through the blade.\n\nFive types of runic weapons exist: Flamestrike (fire), Frostbite (ice), Stormcaller (lightning), Earthshaker (earth), and the legendary Voidrender (dark energy). Each requires different techniques and mental fortitude to master."
            ));
            
            loreEntries.Add(new LoreEntry(
                "Runeblades",
                "The Great Forging",
                "History",
                "Three thousand years ago, during the War of Shadows, the greatest smiths and mages united to create weapons capable of standing against the Void corruption. Working in the Forge of Eternal Flames, they inscribed the first runic weapons with protective wards and devastating power.\n\nThe Great Forging lasted seven years and cost the lives of dozens of craftsmen, but the weapons they created turned the tide of war and established the Runeblade tradition that continues today."
            ));
            
            // Voidborn faction entries
            loreEntries.Add(new LoreEntry(
                "Voidborn",
                "The Corrupted",
                "Characters",
                "Once-mortal beings who have been transformed by prolonged exposure to Void energy. Their flesh takes on a darkened, crystalline appearance, and their eyes glow with purple fire. The Corrupted retain their intelligence and memories but gain terrible new abilities - phasing through solid matter, draining life force, and manipulating shadows.\n\nDespite their fearsome appearance, many Corrupted struggle with their humanity, caught between their former lives and their new dark nature."
            ));
            
            loreEntries.Add(new LoreEntry(
                "Voidborn",
                "Void Crystals",
                "Technology",
                "Crystalline formations that grow where the Void's influence is strongest. These purple-black crystals pulse with malevolent energy and can power dark technologies or corrupt living beings who touch them. Void crystals are highly sought after by necromancers and forbidden researchers.\n\nThe crystals seem to have a form of collective intelligence, growing in patterns that suggest purpose. Scholars theorize they may be fragments of a greater Void consciousness trying to manifest in our reality."
            ));
            
            loreEntries.Add(new LoreEntry(
                "Voidborn",
                "The Void Incursion",
                "History",
                "The catastrophic event that began the War of Shadows. Reality itself was torn open, allowing creatures and energy from the Void dimension to pour into our world. Entire cities were consumed in purple fire, and the survivors were forever changed.\n\nThe Incursion lasted only three days, but its effects ripple through history. The tear in reality was sealed by the combined sacrifice of twelve archmages, but smaller rifts still appear randomly, bringing fresh waves of corruption and Voidborn creatures."
            ));
            
            Debug.Log($"Created {loreEntries.Count} sample lore entries for Runeblades and Voidborn factions.");
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