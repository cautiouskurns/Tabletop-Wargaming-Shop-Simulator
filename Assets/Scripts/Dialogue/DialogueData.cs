using System.Collections.Generic;
using UnityEngine;

namespace TabletopShop.Dialogue
{
    [System.Serializable]
    public class DialogueData
    {
        public string dialogueId;
        public string title;
        public string factionAffinity;
        public string startNode;
        public Dictionary<string, DialogueNode> nodes;
    }

    [System.Serializable]
    public class DialogueNode
    {
        public string speaker;
        [TextArea(3, 6)]
        public string text;
        public List<DialogueChoice> choices = new List<DialogueChoice>();
        public List<DialogueEffect> effects = new List<DialogueEffect>();
        public bool isEnd = false;
        public float autoAdvanceDelay = 0f;
    }

    [System.Serializable]
    public class DialogueChoice
    {
        [TextArea(1, 3)]
        public string text;
        public string @goto;  // 'goto' is C# keyword, so use @goto
        public string condition = "";
        public string failGoto = "";
        public string style = "normal";  // "normal", "special", "important"
        public List<DialogueEffect> effects = new List<DialogueEffect>();
    }

    [System.Serializable]
    public class DialogueEffect
    {
        public string type;      // "reputation", "money", "unlock_lore", "set_flag"
        public string faction = "";
        public float value = 0f;
        public string target = "";
        public string reason = "";
    }
}