using UnityEngine;
using System.Collections.Generic;

namespace TabletopShop
{
    /// <summary>
    /// Pure data container for dialogue trees - NO LOGIC, just data structures
    /// </summary>
    [CreateAssetMenu(fileName = "New Dialogue Tree", menuName = "Tabletop Shop/Dialogue Tree")]
    public class DialogueTreeSO : ScriptableObject
    {
        [Header("Tree Configuration")]
        [SerializeField] private List<DialogueNode> dialogueNodes = new List<DialogueNode>();
        [SerializeField] private string startNodeID = "start";
        [SerializeField] private string factionAffinity = "";
        [SerializeField] private string treeDescription = "";
        [SerializeField] private bool isRepeatable = true;
        
        // Pure data access - NO LOGIC
        public List<DialogueNode> DialogueNodes => dialogueNodes;
        public string StartNodeID => startNodeID;
        public string FactionAffinity => factionAffinity;
        public string TreeDescription => treeDescription;
        public bool IsRepeatable => isRepeatable;
        
        [System.Serializable]
        public class DialogueNode
        {
            [SerializeField] private string nodeID;
            [SerializeField] private string speakerName;
            [TextArea(2, 6)]
            [SerializeField] private string dialogueText;
            [SerializeField] private List<DialogueChoice> choices = new List<DialogueChoice>();
            [SerializeField] private bool isEndNode = false;
            [SerializeField] private List<DialogueEffect> effects = new List<DialogueEffect>();
            
            // Pure data properties - NO METHODS
            public string NodeID => nodeID;
            public string SpeakerName => speakerName;
            public string DialogueText => dialogueText;
            public List<DialogueChoice> Choices => choices;
            public bool IsEndNode => isEndNode;
            public List<DialogueEffect> Effects => effects;
        }
        
        [System.Serializable]
        public class DialogueChoice
        {
            [TextArea(1, 3)]
            [SerializeField] private string choiceText;
            [SerializeField] private string nextNodeID;
            [SerializeField] private string requiredCondition = "";
            [SerializeField] private string conditionFailFallbackNodeID = "";
            [SerializeField] private bool hideIfConditionFailed = false;
            [SerializeField] private List<DialogueEffect> choiceEffects = new List<DialogueEffect>();
            
            // Pure data properties
            public string ChoiceText => choiceText;
            public string NextNodeID => nextNodeID;
            public string RequiredCondition => requiredCondition;
            public string ConditionFailFallbackNodeID => conditionFailFallbackNodeID;
            public bool HideIfConditionFailed => hideIfConditionFailed;
            public List<DialogueEffect> ChoiceEffects => choiceEffects;
        }
        
        [System.Serializable]
        public class DialogueEffect
        {
            [SerializeField] private EffectType effectType;
            [SerializeField] private string targetFaction = "";
            [SerializeField] private float value = 0f;
            [SerializeField] private string stringValue = "";
            [TextArea(1, 2)]
            [SerializeField] private string effectDescription = "";
            
            public enum EffectType
            {
                None, ModifyReputation, GiveMoney, UnlockLore, SetFlag, 
                TriggerEvent, ChangeCustomerMood, ModifyDiscount
            }
            
            // public EffectType EffectType => effectType;
            public string TargetFaction => targetFaction;
            public float Value => value;
            public string StringValue => stringValue;
            public string EffectDescription => effectDescription;
        }
        
        // ONLY Editor helpers for content creation
        #if UNITY_EDITOR
        [ContextMenu("Create Sample Dialogue")]
        private void CreateSampleDialogue()
        {
            // Same sample creation code as before
            dialogueNodes.Clear();
            startNodeID = "greeting";
            factionAffinity = "Runeblades";
            // ... sample data creation
        }
        #endif
    }
}