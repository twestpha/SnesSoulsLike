using System.Collections;
using System;
using UnityEngine;
using UnityEngine.UI;

class CameraRigComponent : MonoBehaviour {

    public float cameraMoveTime = 1.0f;

    private Vector3 localOffset;

    private Vector3 worldPosition;
    private Vector3 worldVelocity;

    void Start(){
        localOffset = transform.localPosition;
        worldPosition = transform.position;
    }

    void Update(){
        worldPosition = Vector3.SmoothDamp(worldPosition, transform.parent.position + localOffset, ref worldVelocity, cameraMoveTime);
        transform.position = worldPosition;
    }
}