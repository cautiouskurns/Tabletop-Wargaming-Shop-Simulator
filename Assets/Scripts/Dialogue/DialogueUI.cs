using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace TabletopShop.Dialogue
{
    /// <summary>
    /// Simple dialogue UI implementation
    /// </summary>
    public class DialogueUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject dialoguePanel;
        [SerializeField] private TextMeshProUGUI speakerText;
        [SerializeField] private TextMeshProUGUI dialogueText;
        [SerializeField] private Transform choicesParent;
        [SerializeField] private Button choiceButtonPrefab;
        [SerializeField] private Button closeButton;
        [SerializeField] private Canvas dialogueCanvas;
        
        private DialogueManager dialogueManager;
        private List<Button> activeChoiceButtons = new List<Button>();
        private UILayerManager uiLayerManager;
        
        private void Awake()
        {
            dialogueManager = FindObjectOfType<DialogueManager>();
            
            // Subscribe to events
            if (dialogueManager != null)
            {
                dialogueManager.OnNodeChanged += ShowNode;
                dialogueManager.OnChoicesChanged += ShowChoices;
                dialogueManager.OnDialogueStarted += ShowDialogue;
                dialogueManager.OnDialogueEnded += HideDialogue;
            }
            
            // Setup close button
            if (closeButton != null)
            {
                closeButton.onClick.AddListener(() => dialogueManager?.EndDialogue());
            }
            
            // Find the dialogue canvas if not assigned
            if (dialogueCanvas == null)
            {
                dialogueCanvas = GetComponentInParent<Canvas>();
            }
            
            HideDialogue();
        }
        
        private void Start()
        {
            RegisterWithUILayerManager();
        }
        
        private void OnDestroy()
        {
            // Unregister from UI layer manager
            if (uiLayerManager != null && dialogueCanvas != null)
            {
                uiLayerManager.UnregisterCanvas(dialogueCanvas);
            }
        }
        
        /// <summary>
        /// Register with UILayerManager for proper canvas layering
        /// </summary>
        private void RegisterWithUILayerManager()
        {
            uiLayerManager = UILayerManager.Instance;
            
            if (uiLayerManager != null && dialogueCanvas != null)
            {
                // Register this canvas with the dialogue layer
                uiLayerManager.RegisterCanvas(dialogueCanvas, "Dialogue", UILayerManager.UILayers.Dialogue, true);
                Debug.Log("[DialogueUI] Registered with UILayerManager at Dialogue layer");
            }
            else
            {
                Debug.LogWarning("[DialogueUI] Could not register with UILayerManager - check that UILayerManager exists in scene");
            }
        }
        
        /// <summary>
        /// Show dialogue panel
        /// </summary>
        private void ShowDialogue()
        {
            dialoguePanel.SetActive(true);
            UpdateRaycastBlocking();
        }
        
        /// <summary>
        /// Hide dialogue panel
        /// </summary>
        private void HideDialogue()
        {
            dialoguePanel.SetActive(false);
            ClearChoices();
            UpdateRaycastBlocking();
        }
        
        /// <summary>
        /// Update raycast blocking for all registered canvases
        /// </summary>
        private void UpdateRaycastBlocking()
        {
            if (uiLayerManager != null)
            {
                uiLayerManager.UpdateRaycastBlocking();
            }
        }
        
        /// <summary>
        /// Display current dialogue node
        /// </summary>
        private void ShowNode(DialogueNode node)
        {
            if (speakerText != null)
                speakerText.text = node.speaker;
            
            if (dialogueText != null)
                dialogueText.text = node.text;
        }
        
        /// <summary>
        /// Display available choices
        /// </summary>
        private void ShowChoices(List<DialogueChoice> choices)
        {
            ClearChoices();
            
            foreach (var choice in choices)
            {
                Button choiceButton = Instantiate(choiceButtonPrefab, choicesParent);
                choiceButton.GetComponentInChildren<TextMeshProUGUI>().text = choice.text;
                
                // Style the button based on choice type
                if (choice.style == "special")
                {
                    choiceButton.GetComponent<Image>().color = Color.yellow;
                }
                
                // Add click listener
                choiceButton.onClick.AddListener(() => dialogueManager.SelectChoice(choice));
                
                activeChoiceButtons.Add(choiceButton);
            }
            
            // Show close button if this is an end node or has no choices
            if (closeButton != null)
            {
                closeButton.gameObject.SetActive(choices.Count == 0);
            }
        }
        
        /// <summary>
        /// Clear all choice buttons
        /// </summary>
        private void ClearChoices()
        {
            foreach (var button in activeChoiceButtons)
            {
                if (button != null)
                    Destroy(button.gameObject);
            }
            activeChoiceButtons.Clear();
        }
    }
}