using System;
using UnityEngine;

class GateComponent : MonoBehaviour {

    public MaterialAnimationComponent materialAnimationA;
    public MaterialAnimationComponent materialAnimationB;

    public Collider blockingCollider;
    public Collider messageCollider;

    void Start(){
        if(GameComponent.gateOpened){
            blockingCollider.enabled = false;

            materialAnimationA.enabled = true;
            materialAnimationB.enabled = true;
        }
    }

    void OnTriggerEnter(Collider other){
        PlayerComponent player = other.gameObject.GetComponent<PlayerComponent>();

        if(player != null && !GameComponent.gateOpened){
            GameComponent.gateOpened = true;

            blockingCollider.enabled = false;
            messageCollider.enabled = false;

            materialAnimationA.enabled = true;
            materialAnimationB.enabled = true;
        }
    }
}