using System.Collections;
using System;
using UnityEngine;
using UnityEngine.UI;

class WeaponComponent : MonoBehaviour {

    public Transform characterWeaponTransform;

    public WeaponData startingWeaponData;

    private WeaponData currentWeaponData;

    private GameObject weaponInstance;
    private DamagerComponent weaponDamager;

    void Start(){
        if(startingWeaponData != null){
            EquipWeapon(startingWeaponData);
        }
    }

    public void EquipWeapon(WeaponData newWeaponData){
        if(currentWeaponData != newWeaponData){
            currentWeaponData = newWeaponData;

            if(weaponInstance != null){
                Destroy(weaponInstance);
                weaponDamager = null;
            }

            weaponInstance = GameObject.Instantiate(currentWeaponData.weaponPrefab);

            weaponInstance.transform.parent = characterWeaponTransform;
            weaponInstance.transform.localPosition = Vector3.zero;
            weaponInstance.transform.localRotation = Quaternion.identity;

            weaponDamager = weaponInstance.GetComponentInChildren<DamagerComponent>();
            weaponDamager.SetSourceWeapon(this);
            weaponDamager.SetSharp(false); // Start not sharp
        }
    }

    public void SetSharp(bool sharp){
        if(weaponDamager != null){
            weaponDamager.SetSharp(sharp);
        }
    }
}