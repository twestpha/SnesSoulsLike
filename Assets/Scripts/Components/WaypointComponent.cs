using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
class WaypointComponent : MonoBehaviour {
    private const int PHYSICS_ENVIRONMENT_LAYER_MASK = 1 << 3;

    public WaypointComponent[] connectedWaypoints;

    public static List<Vector3> PathfindBetweenPositions(Vector3 origin, Vector3 destination){
        // Get closest in-view waypoints
        WaypointComponent originWaypoint = GetClosestWaypointInView(origin);
        WaypointComponent destinationWaypoint = GetClosestWaypointInView(destination);

        if(originWaypoint == null || destinationWaypoint == null){
            return null;
        }

        Debug.DrawLine(origin, originWaypoint.transform.position, Color.red, 100.0f, false);
        Debug.DrawLine(destination, destinationWaypoint.transform.position, Color.red, 100.0f, false);

        // Recursively iterate connectedWaypoints until destination found. This doesn't find the
        // shortest path, but it finds the path real damn fast, so I'm kinda ok with it...
        HashSet<WaypointComponent> searchedWaypoints = new HashSet<WaypointComponent>();
        Dictionary<WaypointComponent, WaypointComponent> directedPath = new Dictionary<WaypointComponent, WaypointComponent>();

        RecursivePathfind(ref directedPath, ref searchedWaypoints, originWaypoint, destinationWaypoint);

        // Rebuild the graph from the directed path
        List<Vector3> waypointPath = new List<Vector3>();

        if(directedPath.ContainsKey(destinationWaypoint)){
            WaypointComponent backwardWaypoint = destinationWaypoint;

            while(backwardWaypoint != originWaypoint){
                waypointPath.Add(backwardWaypoint.transform.position);
                backwardWaypoint = directedPath[backwardWaypoint];
            }

            waypointPath.Add(originWaypoint.transform.position);
        }

        for(int i = 1, count = waypointPath.Count; i < count; ++i){
            Debug.DrawLine(waypointPath[i - 1], waypointPath[i], Color.red, 100.0f, false);
        }

        return waypointPath;
    }

    public static void RecursivePathfind(ref Dictionary<WaypointComponent, WaypointComponent> directedPath, ref HashSet<WaypointComponent> searchedWaypoints, WaypointComponent current, WaypointComponent destination){
        // RecursivePathfind
        foreach(WaypointComponent connectedWaypoint in current.connectedWaypoints){
            if(!searchedWaypoints.Contains(connectedWaypoint)){
                searchedWaypoints.Add(connectedWaypoint);

                directedPath[connectedWaypoint] = current;

                if(connectedWaypoint != destination){
                    RecursivePathfind(ref directedPath, ref searchedWaypoints, connectedWaypoint, destination);
                }
            }
        }
    }

    public static WaypointComponent GetClosestWaypointInView(Vector3 position){
        WaypointComponent[] allWayPoints = FindObjectsOfType<WaypointComponent>();

        float minWaypointDistance = float.MaxValue;
        WaypointComponent closestWaypointInView = null;

        foreach(WaypointComponent waypoint in allWayPoints){
            Vector3 toWaypoint = waypoint.transform.position - position;
            float distance = toWaypoint.magnitude;

            if(distance < minWaypointDistance){
                RaycastHit hit;
                if(!Physics.Raycast(position, toWaypoint, out hit, distance, PHYSICS_ENVIRONMENT_LAYER_MASK, QueryTriggerInteraction.Ignore)){
                    minWaypointDistance = distance;
                    closestWaypointInView = waypoint;
                }
            }
        }

        return closestWaypointInView;
    }

    #if UNITY_EDITOR
    void OnDrawGizmos(){
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(transform.position, 0.5f);

        Gizmos.color = Color.green;
        foreach(WaypointComponent connectedWaypoint in connectedWaypoints){
            if(connectedWaypoint != null){
                Gizmos.DrawLine(transform.position, connectedWaypoint.transform.position);
            }
        }
    }
    #endif // UNITY_EDITOR
}