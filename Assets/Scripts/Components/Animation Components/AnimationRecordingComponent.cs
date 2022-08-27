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

    private AnimationComponent animationComponent;

    void Awake(){

    }

   void Update(){
       if(recordPose){
           Debug.LogWarning("Saving pose for " + gameObject.name + " to " + pose);

           if(pose && !pose.locked){
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
   }

   #endif // UNITY_EDITOR
}
