using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AnimationSequence", menuName = "Animation/Animation Sequence", order = 2)]
[System.Serializable]
public class AnimationSequenceData : ScriptableObject {
    public enum FinishBehavior : int {
        Loop,
        PingPong,
        Stop,
    }

    public FinishBehavior behavior;

    public float speedMultiplier = 1.0f;
    public AnimationPoseData[] poses;
}
