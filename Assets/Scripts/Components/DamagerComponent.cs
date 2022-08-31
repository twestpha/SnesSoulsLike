using System.Collections;
using System;
using UnityEngine;
using UnityEngine.UI;

class DamagerComponent : MonoBehaviour {

    public float tempDamageAmount;

    // Damagers don't care about team, but they can't hurt their owner
    private UnitComponent ownerUnit;

    void Start(){
        ownerUnit = GetComponentInParent<UnitComponent>();
    }

    private void OnTriggerEnter(Collider other){
        UnitComponent otherUnit = other.GetComponentInParent<UnitComponent>();

        if(otherUnit != null && otherUnit != ownerUnit){
            HealthComponent health = otherUnit.GetComponent<HealthComponent>();

            if(health != null){
                health.DealDamage(tempDamageAmount);
            }
        }
    }
}