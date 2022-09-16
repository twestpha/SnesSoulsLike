using System.Collections;
using System;
using UnityEngine;
using UnityEngine.UI;

class DamagerComponent : MonoBehaviour {

    public float tempDamageAmount;

    private UnitComponent ownerUnit;
    private BoxCollider damagerCollider;

    private WeaponComponent sourceWeapon;

    [SerializeField]
    private bool sharp;

    void Start(){
        ownerUnit = GetComponentInParent<UnitComponent>();
        damagerCollider = GetComponent<BoxCollider>();
    }

    public void SetSourceWeapon(WeaponComponent sourceWeapon_){
        sourceWeapon = sourceWeapon_;
    }

    public void SetSharp(bool sharp_){
        sharp = sharp_;
    }

    private void OnTriggerEnter(Collider other){
        if(sharp){
            // Skip character controllers
            if(other is CharacterController){
                return;
            }

            UnitComponent otherUnit = other.GetComponentInParent<UnitComponent>();

            if(otherUnit != null && otherUnit.team != ownerUnit.team){
                HealthComponent health = otherUnit.GetComponent<HealthComponent>();

                if(health != null){
                    // Derive the collision position roughly using the nearest position on opposite box
                    // colliders centroid. Fall back simply to the other collider's world position
                    Vector3 damagePosition = other.transform.position;

                    if(other is BoxCollider otherBoxCollider){
                        Vector3 selfOntoOther = otherBoxCollider.ClosestPointOnBounds(transform.position);
                        Vector3 otherOntoSelf = damagerCollider.ClosestPointOnBounds(other.transform.position);

                        damagePosition = (selfOntoOther + otherOntoSelf) / 2.0f;
                    }

                    health.DealDamage(tempDamageAmount, damagePosition);

                    // Unsharp if damage is dealt until next time enabled
                    sharp = false;
                }
            }
        }
    }
}