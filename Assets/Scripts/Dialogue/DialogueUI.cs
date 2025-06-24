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
        
        private DialogueManager dialogueManager;
        private List<Button> activeChoiceButtons = new List<Button>();
        
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
            
            HideDialogue();
        }
        
        /// <summary>
        /// Show dialogue panel
        /// </summary>
        private void ShowDialogue()
        {
            dialoguePanel.SetActive(true);
        }
        
        /// <summary>
        /// Hide dialogue panel
        /// </summary>
        private void HideDialogue()
        {
            dialoguePanel.SetActive(false);
            ClearChoices();
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