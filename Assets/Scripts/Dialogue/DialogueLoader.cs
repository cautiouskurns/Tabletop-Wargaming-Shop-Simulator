using UnityEngine;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;  // You'll need to import this

namespace TabletopShop.Dialogue
{
    /// <summary>
    /// Loads and manages JSON dialogue files
    /// </summary>
    public class DialogueLoader : MonoBehaviour
    {
        [Header("Dialogue Configuration")]
        [SerializeField] private string dialogueFolder = "Data/Dialogues";

        private Dictionary<string, DialogueData> loadedDialogues = new Dictionary<string, DialogueData>();

        private void Awake()
        {
            LoadAllDialogues();
        }

        /// <summary>
        /// Load all dialogue files from the specified folder
        /// </summary>
        public void LoadAllDialogues()
        {
            loadedDialogues.Clear();

            string fullPath = Path.Combine(Application.streamingAssetsPath, dialogueFolder);

            if (!Directory.Exists(fullPath))
            {
                Debug.LogWarning($"Dialogue folder not found: {fullPath}");
                return;
            }

            string[] jsonFiles = Directory.GetFiles(fullPath, "*.json");

            foreach (string filePath in jsonFiles)
            {
                try
                {
                    string jsonContent = File.ReadAllText(filePath);
                    DialogueData dialogue = JsonConvert.DeserializeObject<DialogueData>(jsonContent);

                    if (dialogue != null && !string.IsNullOrEmpty(dialogue.dialogueId))
                    {
                        loadedDialogues[dialogue.dialogueId] = dialogue;
                        Debug.Log($"Loaded dialogue: {dialogue.dialogueId} from {Path.GetFileName(filePath)}");
                    }
                    else
                    {
                        Debug.LogError($"Invalid dialogue data in file: {filePath}");
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Failed to load dialogue from {filePath}: {e.Message}");
                }
            }

            Debug.Log($"Loaded {loadedDialogues.Count} dialogue files");
        }

        /// <summary>
        /// Get a specific dialogue by ID
        /// </summary>
        public DialogueData GetDialogue(string dialogueId)
        {
            loadedDialogues.TryGetValue(dialogueId, out DialogueData dialogue);
            return dialogue;
        }

        /// <summary>
        /// Get all loaded dialogue IDs
        /// </summary>
        public List<string> GetAllDialogueIds()
        {
            return new List<string>(loadedDialogues.Keys);
        }

        /// <summary>
        /// Reload dialogues from disk (useful for development)
        /// </summary>
        [ContextMenu("Reload All Dialogues")]
        public void ReloadDialogues()
        {
            LoadAllDialogues();
        }
        
        /// <summary>
        /// Create a sample dialogue file for testing
        /// </summary>
        [ContextMenu("Create Sample Dialogue")]
        public void CreateSampleDialogue()
        {
            var sampleDialogue = new DialogueData
            {
                dialogueId = "marcus_collector",
                title = "Marcus the Collector",
                factionAffinity = "Runeblades",
                startNode = "greeting",
                nodes = new Dictionary<string, DialogueNode>
                {
                    ["greeting"] = new DialogueNode
                    {
                        speaker = "Marcus",
                        text = "Welcome to your shop! I've been looking for Runeblade items.",
                        choices = new List<DialogueChoice>
                        {
                            new DialogueChoice
                            {
                                text = "Tell me about Runeblades",
                                @goto = "about_runeblades"
                            },
                            new DialogueChoice
                            {
                                text = "Goodbye",
                                @goto = "goodbye"
                            }
                        }
                    },
                    ["about_runeblades"] = new DialogueNode
                    {
                        speaker = "Marcus",
                        text = "They're legendary warriors with runic weapons!",
                        choices = new List<DialogueChoice>
                        {
                            new DialogueChoice { text = "Interesting!", @goto = "goodbye" }
                        }
                    },
                    ["goodbye"] = new DialogueNode
                    {
                        speaker = "Marcus",
                        text = "Thanks for chatting!",
                        isEnd = true
                    }
                }
            };
            
            string json = JsonConvert.SerializeObject(sampleDialogue, Formatting.Indented);
            string fullPath = Path.Combine(Application.streamingAssetsPath, dialogueFolder);
            
            if (!Directory.Exists(fullPath))
                Directory.CreateDirectory(fullPath);
            
            string filePath = Path.Combine(fullPath, "marcus_collector.json");
            File.WriteAllText(filePath, json);
            
            Debug.Log($"Created sample dialogue at: {filePath}");
            LoadAllDialogues(); // Reload to include new file
        }
    }
}