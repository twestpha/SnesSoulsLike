using System;
using UnityEngine;

class GateComponent : MonoBehaviour {

    public MaterialAnimationComponent materialAnimationA;
    public MaterialAnimationComponent materialAnimationB;

    public Collider blockingCollider;

    void Start(){
        if(GameComponent.gateOpened){
            blockingCollider.enabled = false;

            materialAnimationA.enabled = true;
            materialAnimationB.enabled = true;
        }
    }

    void OnTriggerEnter(Collider other){
        if(!GameComponent.gateOpened){
            GameComponent.gateOpened = true;

            blockingCollider.enabled = false;

            materialAnimationA.enabled = true;
            materialAnimationB.enabled = true;
        }
    }
}