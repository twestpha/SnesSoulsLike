using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif // UNITY_EDITOR

#if UNITY_EDITOR
[ExecuteInEditMode]
public class WaypointBuilder : MonoBehaviour {
    private const int PHYSICS_ENVIRONMENT_LAYER_MASK = 1 << 3;

    public void BuildWaypoints(){
        WaypointComponent[] childWaypoints = GetComponentsInChildren<WaypointComponent>();

        foreach(WaypointComponent waypointA in childWaypoints){
            // Clear previous
            waypointA.connectedWaypoints = new WaypointComponent[0];

            foreach(WaypointComponent waypointB in childWaypoints){
                if(waypointA != waypointB){
                    Vector3 toWaypointB = waypointB.transform.position - waypointA.transform.position;

                    RaycastHit hit;
                    if(!Physics.Raycast(waypointA.transform.position, toWaypointB, out hit, toWaypointB.magnitude, PHYSICS_ENVIRONMENT_LAYER_MASK, QueryTriggerInteraction.Ignore)){
                        List<WaypointComponent> waypointAList = new List<WaypointComponent>(waypointA.connectedWaypoints);
                        waypointAList.Add(waypointB);
                        waypointA.connectedWaypoints = waypointAList.ToArray();
                    }
                    // } else {
                    //     Debug.Log(waypointA + " -> " + waypointB + " X " + hit.collider.gameObject);
                    // }
                }
            }
        }

        Debug.Log("Generated " + childWaypoints.Length + " waypoints");
    }
}

#endif // UNITY_EDITOR