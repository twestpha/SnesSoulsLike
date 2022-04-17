using System;
using UnityEngine;

class HitBoxComponent : MonoBehaviour {

    public float damage;
    public BoxCollider hitBox;

    public GameObject hitEffectsPrefab;

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

                    GameObject newHitEffects = GameObject.Instantiate(hitEffectsPrefab);
                    Vector3 position = Vector3.Lerp(transform.parent.position, enemyComponent.transform.position, 0.5f);
                    position.y = transform.position.y;

                    newHitEffects.transform.position = position;
                }
            } else {           
                PlayerComponent playerComponent = other.gameObject.GetComponent<PlayerComponent>();

                if(playerComponent){
                    if(playerComponent.DealDamage(damage)){
                        GameObject newHitEffects = GameObject.Instantiate(hitEffectsPrefab);
                        Vector3 position = Vector3.Lerp(transform.parent.position, playerComponent.transform.position, 0.5f);
                        position.y = transform.position.y;

                        newHitEffects.transform.position = position;
                    }
                }
            }
        }
    }
}