using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationComponent : MonoBehaviour {

    public GameObject[] rig;

    public enum Transition : int {
        Linear,
        EaseIn,
        EaseOut,
        EaseInOut,
        Spring,
        Bounce,
        Jitter,
        Overshoot,
    }

    private int currentPoseIndex;

    private Vector3[] previousPositions;
    private Quaternion[] previousRotations;

    private Vector3[] positionVelocity;
    private Quaternion[] rotationVelocity;

    [System.Serializable]
    public class NamedAnimationSequence {
        public string name;
        public AnimationSequenceData sequence;
    }

    public NamedAnimationSequence[] namedSequences;
    private AnimationSequenceData currentlyPlayingSequence;
    private Transition currentlyPlayingTransition;
    private Timer animationTimer = new Timer();

    // Only used for "stop" animations, letting external components know the animation finished playing
    private bool finishedAnimation;
    private bool returnToIdleOnFinish;

    // Only used for "ping pong" animation, either incrementing or decrementing index depending on state
    private bool pingPongIncrements;

    private Dictionary<string, AnimationSequenceData> namesSequencesLookup = new Dictionary<string, AnimationSequenceData>();

	void Start(){
        previousPositions = new Vector3[rig.Length];
        previousRotations = new Quaternion[rig.Length];

        positionVelocity = new Vector3[rig.Length];
        rotationVelocity = new Quaternion[rig.Length];

        for(int i = 0, count = namedSequences.Length; i < count; ++i){
            namesSequencesLookup.Add(namedSequences[i].name.ToLower(), namedSequences[i].sequence);
        }

        PlayAnimation("idle", false);
    }

    #if UNITY_EDITOR
    [ContextMenu("Test Animation")]
    void TestAnimationIdle(){
        PlayAnimation("test", false);
    }
    #endif

    // Internal Math Functions for transitioning
    private static float EaseIn(float t){
        return 1.0f - EaseOut(1.0f - t);
    }

    private static float EaseOut(float t){
        return t * t;
    }

    private static float EaseInOut(float t){
        if(t < 0.5f){
            return EaseOut(t * 2.0f) / 2.0f;
        }

        return (EaseIn(t) * 2.0f) - 1.0f;
    }

    private static float Spring(float t){
        // Goes past 1, then returns
        return 1.2f - 2.5f * (t - 0.7f) * (t - 0.7f);
    }

    private static float Bounce(float t){
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

    private static float Jitter(float t){
        // Goes a little past 1, then shudders
        t = Mathf.Clamp(t, 0.0f, 1.0f);
        float g = EaseIn(t);
        float r = (t * t * t * t * t * t);

        float b = 0.044f;
        float c = 62.77f;

        float output = g + ((b * r * Mathf.Sin(c * t)) / t);

        return output;
    }

    private static float Overshoot(float t){
        float c = 1.1f;
        float b = 1.9f;
        float a = 0.77f;

        float tMinusA = (t - a);
        float tMinusASquared = tMinusA * tMinusA;

        return c - (b * tMinusASquared);
    }

    // TODO don't manually update; hang our update off of a enemy update or something
    // Or some animation update manager, or something.
	void Update(){
        float param = animationTimer.Parameterized();

        // TODO only use fancy transition evaluation at high-lod
        if(true /* highLOD */){
            if(currentlyPlayingTransition == Transition.EaseIn){
                param = EaseIn(param);
            } else if(currentlyPlayingTransition == Transition.EaseOut){
                param = EaseOut(param);
            } else if(currentlyPlayingTransition == Transition.EaseInOut){
                param = EaseInOut(param);
            } else if(currentlyPlayingTransition == Transition.Spring){
                param = Spring(param);
            } else if(currentlyPlayingTransition == Transition.Bounce){
                param = Bounce(param);
            } else if(currentlyPlayingTransition == Transition.Jitter){
                param = Jitter(param);
            } else if(currentlyPlayingTransition == Transition.Overshoot){
                param = Overshoot(param);
            }
        }

        UpdateJoints(param);
        UpdateSequenceIndex();
	}

    public void UpdateJoints(float parameter){
        for(int i = 0, rigLength = rig.Length; i < rigLength; ++i){
            rig[i].transform.localPosition = Vector3.LerpUnclamped(
                previousPositions[i],
                currentlyPlayingSequence.poses[currentPoseIndex].positions[i],
                parameter
            );

            rig[i].transform.localRotation = Quaternion.SlerpUnclamped(
                previousRotations[i],
                currentlyPlayingSequence.poses[currentPoseIndex].rotations[i],
                parameter
            );
        }
    }

    public void UpdateSequenceIndex(){
        if(animationTimer.Finished()){
            CachePreviousState();

            if(currentlyPlayingSequence.behavior == AnimationSequenceData.FinishBehavior.Loop){
                currentPoseIndex = (currentPoseIndex + 1) % currentlyPlayingSequence.poses.Length;

                currentlyPlayingTransition = currentlyPlayingSequence.poses[currentPoseIndex].transition;

            } else if(currentlyPlayingSequence.behavior == AnimationSequenceData.FinishBehavior.PingPong){
                if(pingPongIncrements){
                    currentPoseIndex++;

                    if(currentPoseIndex == currentlyPlayingSequence.poses.Length){
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

                currentlyPlayingTransition = currentlyPlayingSequence.poses[currentPoseIndex].transition;

            } else if(currentlyPlayingSequence.behavior == AnimationSequenceData.FinishBehavior.Stop){
                currentPoseIndex++;

                if(currentPoseIndex < currentlyPlayingSequence.poses.Length){
                    currentlyPlayingTransition = currentlyPlayingSequence.poses[currentPoseIndex].transition;
                }

                if(currentPoseIndex == currentlyPlayingSequence.poses.Length){
                    currentPoseIndex = currentlyPlayingSequence.poses.Length - 1;

                    if(returnToIdleOnFinish){
                        PlayAnimation("idle", false);
                    }

                    // After PlayAnimation() so it doesn't get stomped
                    finishedAnimation = true;
                }
            }

            animationTimer.SetDuration(currentlyPlayingSequence.poses[currentPoseIndex].duration * currentlyPlayingSequence.speedMultiplier);
            animationTimer.Start();
        }
    }

    public void PlayAnimation(string sequenceName, bool idleOnFinish = false){
        if(namesSequencesLookup.TryGetValue(sequenceName.ToLower(), out AnimationSequenceData newAnimationSequence)){
            // Debug.Log("Playing animation sequence " + sequenceName + " on actor " + gameObject.name);

            currentlyPlayingSequence = newAnimationSequence;

            currentPoseIndex = 0;

            pingPongIncrements = true;
            finishedAnimation = false;

            returnToIdleOnFinish = idleOnFinish;

            currentlyPlayingTransition = currentlyPlayingSequence.poses[0].transition;

            animationTimer.SetDuration(currentlyPlayingSequence.poses[0].duration);
            animationTimer.Start();

            CachePreviousState();
        } else {
            Debug.LogWarning("Trying to play animation sequence " + sequenceName + " on actor " + gameObject.name + " but could not find matching named animation.");
        }
    }

    private void CachePreviousState(){
        for(int i = 0, rigLength = rig.Length; i < rigLength; ++i){
            previousPositions[i] = rig[i].transform.localPosition;
            previousRotations[i] = rig[i].transform.localRotation;
        }
    }

    public bool IsPlayingAnimation(string sequenceName){
        if(namesSequencesLookup.TryGetValue(sequenceName.ToLower(), out AnimationSequenceData querySequenceData)){
            return currentlyPlayingSequence == querySequenceData;
        }

        return false;
    }

    public bool GetAnimationFinished(){
        return finishedAnimation;
    }
}
