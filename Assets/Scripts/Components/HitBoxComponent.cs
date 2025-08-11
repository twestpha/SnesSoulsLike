using System;
using UnityEngine;

class HitBoxComponent : MonoBehaviour {

    [NonSerialized] public EffectData[] hitEffects;

    private bool isPlayer;
    private BoxCollider hitBox;

    private bool enableDamage;
    private Timer enableDamageTimer = new Timer();

    void Start(){
        isPlayer = GetComponentInParent<PlayerComponent>() != null;
        hitBox = GetComponent<BoxCollider>();
    }

    void Update(){
        if(enableDamage){
            if(enableDamageTimer.Finished()){
                enableDamage = false;
                hitBox.enabled = false;
            }
        }
    }

    public void EnableForTime(float time){
        enableDamage = true;

        enableDamageTimer.SetDuration(time);
        enableDamageTimer.Start();

        hitBox.enabled = true;
    }

    void OnTriggerEnter(Collider other){
        if(enableDamage){
            if(isPlayer){
                // Look for creature component
                CreatureComponent creatureComponent = other.gameObject.GetComponent<CreatureComponent>();

                if(creatureComponent){
                    creatureComponent.ApplyEffects(hitEffects);
                }
            } else {           
                PlayerComponent playerComponent = other.gameObject.GetComponent<PlayerComponent>();

                if(playerComponent){
                    playerComponent.ApplyEffects(hitEffects);
                }
            }
        }
    }
}