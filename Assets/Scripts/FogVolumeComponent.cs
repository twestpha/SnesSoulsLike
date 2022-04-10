using System;
using UnityEngine;
using UnityStandardAssets.ImageEffects;

class FogVolumeComponent : MonoBehaviour {

    public Vector2 fogSetting;

    void OnTriggerEnter(Collider other){
        PlayerComponent player = other.gameObject.GetComponent<PlayerComponent>();

        if(player != null){
            RenderSettings.fogStartDistance = fogSetting.x;
            RenderSettings.fogEndDistance = fogSetting.y;
        }
    }
}