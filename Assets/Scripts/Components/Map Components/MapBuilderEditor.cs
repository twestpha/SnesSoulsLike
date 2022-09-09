
using UnityEngine;
using System.Collections;

#if UNITY_EDITOR
using UnityEditor;
#endif // UNITY_EDITOR

#if UNITY_EDITOR
[CustomEditor(typeof(MapBuilder))]
public class MapBuilderEditor : Editor {
    public override void OnInspectorGUI(){
        DrawDefaultInspector();

        MapBuilder tiledBuilder = (MapBuilder) target;
        if (GUILayout.Button("Generate Tiled Map")){
            tiledBuilder.GenerateMap();
        }
    }
}
#endif // UNITY_EDITOR
