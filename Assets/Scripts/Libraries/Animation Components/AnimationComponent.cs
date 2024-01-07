using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationComponent : MonoBehaviour {
    public const float MEDIUM_LOD_DISTANCE_SQUARED = 4900.0f; // 70 units
    public const float HIGH_LOD_DISTANCE_SQUARED = 400.0f; // 20 units

    public GameObject[] rig;

    public enum Transition : int {
        Linear,
        EaseIn,
        EaseOut,
        EaseInOut,
        Spring,
        Bounce,
        Jitter,
        Snap,
    }

    public enum Sequence : int {
        Invalid = -1,
        Idle,
        Walk,
        Aim,
        Attack,
        Flinch,
        Dead,
        Surrender,

        Count, // 7 for now
    }

    public Sequence currentAnimationSequence;
    public int currentPoseIndex;

    // TODO in the future, keep lists for Rotation-only joints, and Rotation + Translation joints, and Rotation/Translation/Scale joints?

    private Vector3[] previousPositions;
    private Quaternion[] previousRotations;

    [Header("Animation Count: 7")]
    public AnimationSequenceData[] animations;
    private Timer animationTimer;

    // Only used for "stop" animations, letting external components know the animation finished playing
    public bool finishedAnimation;
    private bool returnToIdleOnFinish;

    // Only used for "ping pong" animation, either incrementing or decrementing index depending on state
    private bool pingPongIncrements;

	void Start(){
        #if UNITY_EDITOR
        for(int i = 0, animCount = animations.Length; i < animCount; ++i){
            if(animations[i]){
                for(int j = 0, poseCount = animations[i].poses.Length; j < poseCount; ++j){
                    if(animations[i].poses[j].rotations.Length != rig.Length){
                        Debug.LogError("Pose '" + animations[i].poses[j] + "' for animation '" + (Sequence) i + "' needs to match rig for actor '" + gameObject + "'.");
                    }
                }
            }
        }
        #endif // UNITY_EDITOR

        previousPositions = new Vector3[rig.Length];
        previousRotations = new Quaternion[rig.Length];

        PlayAnimation(currentAnimationSequence, false);

        // If this animation starts on idle, randomize the starting index so many copies aren't all in sync
        if(currentAnimationSequence == Sequence.Idle){
            currentPoseIndex = Random.Range(0, animations[(int) currentAnimationSequence].poses.Length - 1);
            animationTimer.SetParameterized(Random.Range(0.0f, 1.0f));
        }
    }

    #if UNITY_EDITOR
    [ContextMenu("Test Animation Idle")]
    void TestAnimationIdle(){
        PlayAnimation(Sequence.Idle, false);
    }

    [ContextMenu("Test Animation Walk")]
    void TestAnimationWalk(){
        PlayAnimation(Sequence.Walk, false);
    }

    [ContextMenu("Test Animation Aim")]
    void TestAnimtionAim(){
        PlayAnimation(Sequence.Aim, true);
    }

    [ContextMenu("Test Animation Attack")]
    void TestAnimationAttack(){
        PlayAnimation(Sequence.Attack, true);
    }

    [ContextMenu("Test Animation Flinch")]
    void TestAnimationFlinch(){
        PlayAnimation(Sequence.Flinch, true);
    }

    [ContextMenu("Test Animation Dead")]
    void TestAnimationDead(){
        PlayAnimation(Sequence.Dead, false);
    }

    [ContextMenu("Test Animation Surrender")]
    void TestAnimationCast(){
        PlayAnimation(Sequence.Surrender, true);
    }
    #endif // UNITY_EDITOR

    // Internal Math Functions for transitioning
    private float EaseIn(float t){
        return 1.0f - EaseOut(1.0f - t);
    }

    private float EaseOut(float t){
        return t * t;
    }

    private float EaseInOut(float t){
        if(t < 0.5f){
            return EaseOut(t * 2.0f) / 2.0f;
        }

        return (EaseIn(t) * 2.0f) - 1.0f;
    }

    private float Spring(float t){
        // Goes past 1, then returns
        return 1.2f - 2.5f * (t - 0.7f) * (t - 0.7f);
    }

    private float Bounce(float t){
        // Goes to one and bounces off
        if(t < 0.7f){
            return 2.0408f * t * t;
        } else {
            float b = 8.8888f;
            float c = 0.8f;
            float d = 0.85f;

            return (b * (d - t) * (d - t)) + c;
        }
    }

    private float Jitter(float t){
        // Goes a little past 1, then shudders
        t = Mathf.Clamp(t, 0.0f, 1.0f);
        float g = EaseIn(t);
        float r = (t * t * t * t * t * t);

        float b = 0.044f;
        float c = 62.77f;

        float output = g + ((b * r * Mathf.Sin(c * t)) / t);

        return output;
    }

	void Update(){
        AnimationSequenceData currentSequenceData = animations[(int) currentAnimationSequence];
        float param = animationTimer.Parameterized();

        if(currentSequenceData.transitions[currentPoseIndex] == Transition.EaseIn){
            param = EaseIn(param);
        } else if(currentSequenceData.transitions[currentPoseIndex] == Transition.EaseOut){
            param = EaseOut(param);
        } else if(currentSequenceData.transitions[currentPoseIndex] == Transition.EaseInOut){
            param = EaseInOut(param);
        } else if(currentSequenceData.transitions[currentPoseIndex] == Transition.Spring){
            param = Spring(param);
        } else if(currentSequenceData.transitions[currentPoseIndex] == Transition.Bounce){
            param = Bounce(param);
        } else if(currentSequenceData.transitions[currentPoseIndex] == Transition.Jitter){
            param = Jitter(param);
        } else if(currentSequenceData.transitions[currentPoseIndex] == Transition.Snap){
            param = Mathf.Round(param);
        }

        UpdateJoints(param);
        UpdateSequenceIndex();
	}

    public void UpdateJoints(float parameter){
        AnimationSequenceData currentSequenceData = animations[(int) currentAnimationSequence];

        for(int i = 0, rigLength = rig.Length; i < rigLength; ++i){
            rig[i].transform.localPosition = Vector3.Lerp(
                previousPositions[i],
                currentSequenceData.poses[currentPoseIndex].positions[i],
                parameter
            );

            rig[i].transform.localRotation = Quaternion.SlerpUnclamped(
                previousRotations[i],
                currentSequenceData.poses[currentPoseIndex].rotations[i],
                parameter
            );
        }
    }

    public void UpdateSequenceIndex(){
        AnimationSequenceData currentSequenceData = animations[(int) currentAnimationSequence];

        if(animationTimer.Finished()){
            animationTimer.Start();
            CacheCurrentJointsAndRoot();

            if(currentSequenceData.behavior == AnimationSequenceData.FinishBehavior.Loop){
                currentPoseIndex = (currentPoseIndex + 1) % currentSequenceData.poses.Length;
            } else if(currentSequenceData.behavior == AnimationSequenceData.FinishBehavior.PingPong){
                if(pingPongIncrements){
                    currentPoseIndex++;

                    if(currentPoseIndex == currentSequenceData.poses.Length){
                        currentPoseIndex -= 2;
                        pingPongIncrements = false;
                    }
                } else {
                    currentPoseIndex--;

                    if(currentPoseIndex == -1){
                        currentPoseIndex += 2;
                        pingPongIncrements = true;
                    }
                }
            } else if(currentSequenceData.behavior == AnimationSequenceData.FinishBehavior.Stop){
                currentPoseIndex++;

                if(currentPoseIndex == currentSequenceData.poses.Length){
                    currentPoseIndex--;

                    if(returnToIdleOnFinish){
                        PlayAnimation(Sequence.Idle, false);
                    }

                    // After PlayAnimation() so it doesn't get stomped
                    finishedAnimation = true;
                }
            }
        }
    }

    public void PlayAnimation(Sequence newSequence, bool idleOnFinish){
        // Debug.LogWarning("Playing animation sequence " + newSequence + " on actor " + gameObject.name);

        AnimationSequenceData newSequenceData = animations[(int) newSequence];

        if(newSequenceData){
            currentAnimationSequence = newSequence;
            currentPoseIndex = 0;

            pingPongIncrements = true;
            finishedAnimation = false;

            returnToIdleOnFinish = idleOnFinish;

            animationTimer = new Timer(newSequenceData.timeBetweenPoses);
            animationTimer.Start();

            CacheCurrentJointsAndRoot();
        } else {
            Debug.LogWarning("Trying to play animation sequence " + newSequence + " on actor " + gameObject.name + " but failed.");
        }
    }

    private void CacheCurrentJointsAndRoot(){
        for(int i = 0, rigLength = rig.Length; i < rigLength; ++i){
            previousPositions[i] = rig[i].transform.localPosition;
            previousRotations[i] = rig[i].transform.localRotation;
        }
    }

    public bool IsWalking(){
        return currentAnimationSequence == Sequence.Walk;
    }
}
