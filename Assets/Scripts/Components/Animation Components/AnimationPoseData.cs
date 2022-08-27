using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AnimationPose", menuName = "Animation/Animation Pose", order = 1)]
[System.Serializable]
public class AnimationPoseData : ScriptableObject {
    public bool locked;
    [Space(10)]
    public AnimationComponent.Transition transition;
    public float duration = 1.0f;
    [Space(10)]
    public Vector3[] positions;
    public Quaternion[] rotations;
}
