using System;
using UnityEngine;
using UnityStandardAssets.ImageEffects;

class MessageVolumeComponent : MonoBehaviour {

    public string message;

    void OnTriggerEnter(Collider other){
        PlayerComponent player = other.gameObject.GetComponent<PlayerComponent>();

        if(player != null){
            player.ShowMessage(message);
        }
    }
}