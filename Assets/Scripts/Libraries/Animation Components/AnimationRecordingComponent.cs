using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
public class AnimationRecordingComponent : MonoBehaviour {
    #if UNITY_EDITOR

    [Header("● RECORD")]
    public bool recordPose;
    [Header("▲ RESTORE")]
    public bool restorePose;

    [Header("Current Pose")]
    public AnimationPoseData pose;

    [Space(20)]
    [Header("TWEEN TOOL")]
    [Range(0.0f, 1.0f)]
    public float percentage;
    public AnimationPoseData poseA;
    public AnimationPoseData poseB;
    public bool tween;

    private AnimationComponent animationComponent;

    void Update(){
       if(recordPose){
           Debug.LogWarning("Saving pose for " + gameObject.name + " to " + pose);

           if(pose){
               animationComponent = GetComponent<AnimationComponent>();

               // write out all data to pose
               pose.positions = new Vector3[animationComponent.rig.Length];
               pose.rotations = new Quaternion[animationComponent.rig.Length];

               for(int i = 0; i < animationComponent.rig.Length; ++i){
                   pose.positions[i] = animationComponent.rig[i].transform.localPosition;
                   pose.rotations[i] = animationComponent.rig[i].transform.localRotation;
               }

               EditorUtility.SetDirty(pose);
               AssetDatabase.SaveAssets();

               pose = null;
           } else {
               Debug.LogWarning("Saving pose " + pose + " failed.");
           }

           recordPose = false;
       }

       if(restorePose){
           Debug.LogWarning("Restoring pose for " + gameObject.name + " to " + pose);

            if(pose){
                animationComponent = GetComponent<AnimationComponent>();

                for(int i = 0; i < animationComponent.rig.Length; ++i){
                    animationComponent.rig[i].transform.localPosition = pose.positions[i];
                    animationComponent.rig[i].transform.localRotation = pose.rotations[i];
                }

                pose = null;
            } else {
                Debug.LogWarning("Loading pose " + pose + " failed.");
            }

            restorePose = false;
       }

       if(tween){
            Debug.LogWarning("Tweening " + (percentage * 100.0f) + "% between " + poseA + " and " + poseB + " for " + gameObject.name);

            if(poseA && poseB){
                animationComponent = GetComponent<AnimationComponent>();

                for(int i = 0; i < animationComponent.rig.Length; ++i){
                    animationComponent.rig[i].transform.localPosition = Vector3.Lerp(poseA.positions[i], poseB.positions[i], percentage);
                    animationComponent.rig[i].transform.localRotation = Quaternion.Slerp(poseA.rotations[i], poseB.rotations[i], percentage);
                }
            } else {
                Debug.LogWarning("Loading pose " + pose + " failed.");
            }

            tween = false;
       }
    }

   #endif // UNITY_EDITOR
}
