using System;
using UnityEngine;
using UnityStandardAssets.ImageEffects;

class ChestComponent : MonoBehaviour {

    private bool opened;

    void OnTriggerEnter(Collider other){
        PlayerComponent player = other.gameObject.GetComponent<PlayerComponent>();

        if(player != null && !opened){
            opened = true;

            GetComponent<MaterialAnimationComponent>().enabled = true;

            // Pretend to give the player an item
            // player.ShowMessage("Obtained Flensed Skull");
        }
    }
}