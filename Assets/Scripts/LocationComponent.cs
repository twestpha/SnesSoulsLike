using System;
using System.Collections;
using UnityEngine;

class LocationComponent : MonoBehaviour {

    private bool seen;

    void OnTriggerEnter(Collider other){
        PlayerComponent player = other.gameObject.GetComponent<PlayerComponent>();

        if(player != null && !seen){
            seen = true;
            // player.ShowLocation();
        }
    }
}