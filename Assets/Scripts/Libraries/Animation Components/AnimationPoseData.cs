using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AnimationPose", menuName = "Animation/Animation Pose", order = 1)]
[System.Serializable]
public class AnimationPoseData : ScriptableObject {
    public Vector3[] positions;
    public Quaternion[] rotations;
}
