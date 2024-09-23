using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu(fileName = "CharacterDialogueTreeData", menuName = "Dialogue/Character Dialogue Tree Data", order = 1)]
[System.Serializable]
public class CharacterDialogueTreeData : ScriptableObject {

    [Serializable]
    public class DialogueLink {
        public string targetId;
        public string unlockTag;
        public string optionLocKey;
    }

    [Serializable]
    public class DialogueChunk {
        public string id;
        public string locKey;

        public DialogueLink[] links;
    }

    public DialogueChunk[] chunks;
}
