using UnityEngine;
using System.Collections.Generic;

namespace TabletopShop.Dialogue
{
    /// <summary>
    /// Manages dialogue flow and state
    /// </summary>
    public class DialogueManager : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private DialogueLoader dialogueLoader;
        [SerializeField] private DialogueUI dialogueUI;

        [Header("Current State")]
        [SerializeField] private DialogueData currentDialogue;
        [SerializeField] private DialogueNode currentNode;
        [SerializeField] private bool isActive = false;

        // Events
        public System.Action<DialogueNode> OnNodeChanged;
        public System.Action<List<DialogueChoice>> OnChoicesChanged;
        public System.Action OnDialogueStarted;
        public System.Action OnDialogueEnded;

        /// <summary>
        /// Start a dialogue by ID
        /// </summary>
        public bool StartDialogue(string dialogueId)
        {
            if (isActive)
            {
                Debug.LogWarning("Cannot start dialogue - another dialogue is active");
                return false;
            }

            currentDialogue = dialogueLoader.GetDialogue(dialogueId);
            if (currentDialogue == null)
            {
                Debug.LogError($"Dialogue not found: {dialogueId}");
                return false;
            }

            if (!currentDialogue.nodes.TryGetValue(currentDialogue.startNode, out currentNode))
            {
                Debug.LogError($"Start node '{currentDialogue.startNode}' not found in dialogue '{dialogueId}'");
                return false;
            }

            isActive = true;
            ProcessCurrentNode();
            OnDialogueStarted?.Invoke();

            return true;
        }

        /// <summary>
        /// Process a player choice
        /// </summary>
        public void SelectChoice(DialogueChoice choice)
        {
            if (!isActive || choice == null)
                return;

            // Execute choice effects
            ExecuteEffects(choice.effects);

            // Determine next node
            string nextNodeId = GetNextNodeId(choice);

            if (string.IsNullOrEmpty(nextNodeId))
            {
                EndDialogue();
                return;
            }

            // Navigate to next node
            if (currentDialogue.nodes.TryGetValue(nextNodeId, out currentNode))
            {
                ProcessCurrentNode();
            }
            else
            {
                Debug.LogError($"Next node '{nextNodeId}' not found");
                EndDialogue();
            }
        }

        /// <summary>
        /// End the current dialogue
        /// </summary>
        public void EndDialogue()
        {
            isActive = false;
            currentDialogue = null;
            currentNode = null;
            OnDialogueEnded?.Invoke();
        }

        /// <summary>
        /// Process the current node
        /// </summary>
        private void ProcessCurrentNode()
        {
            if (currentNode == null)
                return;

            // Execute node effects
            ExecuteEffects(currentNode.effects);

            // Update UI
            OnNodeChanged?.Invoke(currentNode);

            // Filter available choices
            List<DialogueChoice> availableChoices = GetAvailableChoices();
            OnChoicesChanged?.Invoke(availableChoices);

            // Check if end node
            if (currentNode.isEnd)
            {
                if (currentNode.autoAdvanceDelay > 0)
                {
                    Invoke(nameof(EndDialogue), currentNode.autoAdvanceDelay);
                }
                // Otherwise wait for player to close dialogue
            }
        }

        /// <summary>
        /// Get choices available to player (filtered by conditions)
        /// </summary>
        private List<DialogueChoice> GetAvailableChoices()
        {
            List<DialogueChoice> available = new List<DialogueChoice>();

            foreach (var choice in currentNode.choices)
            {
                if (string.IsNullOrEmpty(choice.condition) || EvaluateCondition(choice.condition))
                {
                    available.Add(choice);
                }
                // Could add logic here to show disabled choices with different styling
            }

            return available;
        }

        /// <summary>
        /// Determine next node ID considering conditions
        /// </summary>
        private string GetNextNodeId(DialogueChoice choice)
        {
            if (string.IsNullOrEmpty(choice.condition))
                return choice.@goto;

            if (EvaluateCondition(choice.condition))
                return choice.@goto;
            else
                return !string.IsNullOrEmpty(choice.failGoto) ? choice.failGoto : choice.@goto;
        }

        /// <summary>
        /// Evaluate dialogue conditions
        /// </summary>
        private bool EvaluateCondition(string condition)
        {
            if (string.IsNullOrEmpty(condition))
                return true;

            // Simple condition parser - could be expanded
            if (condition.StartsWith("reputation."))
            {
                // Parse: "reputation.Runeblades >= 50"
                string[] parts = condition.Split(' ');
                if (parts.Length >= 3)
                {
                    string faction = parts[0].Substring("reputation.".Length);
                    string op = parts[1];
                    float value = float.Parse(parts[2]);

                    float currentRep = GetReputation(faction);

                    switch (op)
                    {
                        case ">=": return currentRep >= value;
                        case ">": return currentRep > value;
                        case "<=": return currentRep <= value;
                        case "<": return currentRep < value;
                        case "==": return Mathf.Approximately(currentRep, value);
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Execute dialogue effects
        /// </summary>
        private void ExecuteEffects(List<DialogueEffect> effects)
        {
            foreach (var effect in effects)
            {
                switch (effect.type)
                {
                    case "reputation":
                        ModifyReputation(effect.faction, effect.value);
                        Debug.Log($"Reputation effect: {effect.faction} +{effect.value} ({effect.reason})");
                        break;

                    case "money":
                        ModifyMoney(effect.value);
                        Debug.Log($"Money effect: +{effect.value} ({effect.reason})");
                        break;

                    case "unlock_lore":
                        UnlockLore(effect.target);
                        Debug.Log($"Unlocked lore: {effect.target}");
                        break;

                    case "set_flag":
                        SetGameFlag(effect.target, effect.value != 0);
                        Debug.Log($"Set flag: {effect.target} = {effect.value != 0}");
                        break;
                }
            }
        }

        // These would integrate with your existing systems
        private float GetReputation(string faction) => 0f;  // TODO: Integrate with ReputationSystem
        private void ModifyReputation(string faction, float value) { }  // TODO: Integrate
        private void ModifyMoney(float value) { }  // TODO: Integrate
        private void UnlockLore(string loreId) { }  // TODO: Integrate
        private void SetGameFlag(string flag, bool value) { }  // TODO: Integrate
        
        [ContextMenu("Start Test Dialogue")]
        public void StartTestDialogue()
        {
            StartDialogue("marcus_collector");
        }
    }
}