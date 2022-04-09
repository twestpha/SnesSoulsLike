using System;
using UnityEngine;
using UnityStandardAssets.ImageEffects;

class BonfireComponent : MonoBehaviour {

    public Transform spawnTransform;

    private bool lit;

    void OnTriggerEnter(Collider other){
        PlayerComponent player = other.gameObject.GetComponent<PlayerComponent>();

        if(player != null && !lit){
            lit = true;

            GetComponent<MaterialAnimationComponent>().enabled = true;

            GameComponent gameComponent = GameObject.FindObjectOfType<GameComponent>();
            gameComponent.playerStartTransform = spawnTransform;

            player.ShowMessage("Devotion Accepted");
        }
    }
}