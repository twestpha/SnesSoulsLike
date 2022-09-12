
using UnityEngine;
using System.Collections;

#if UNITY_EDITOR
using UnityEditor;
#endif // UNITY_EDITOR

#if UNITY_EDITOR
[CustomEditor(typeof(WaypointBuilder))]
public class WaypointBuilderEditor : Editor {
    public override void OnInspectorGUI(){
        DrawDefaultInspector();

        WaypointBuilder waypointBuilder = (WaypointBuilder) target;
        if (GUILayout.Button("Generate Waypoints")){
            waypointBuilder.BuildWaypoints();
        }
    }
}
#endif // UNITY_EDITOR
