using System;
using UnityEngine;

class HitBoxComponent : MonoBehaviour {

    public float damage;
    public BoxCollider hitBox;

    private bool isPlayer;

    private bool enableDamage;
    private Timer enableDamageTimer = new Timer();

    void Start(){
        isPlayer = GetComponentInParent<PlayerComponent>() != null;
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
                // Look for enemy component
                EnemyComponent enemyComponent = other.gameObject.GetComponent<EnemyComponent>();

                if(enemyComponent){
                    enemyComponent.DealDamage(damage);
                }
            } else {
                // Look for player component
            }
        }
    }
}