using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

class HealthComponent : MonoBehaviour {

    public float maxHealth;
    public float currentHealth;

    public delegate void OnDamaged(HealthComponent damage);
    public delegate void OnKilled(HealthComponent damage);

    private List<OnDamaged> damagedDelegates;
    private List<OnKilled> killedDelegates;

    void Start(){
        currentHealth = maxHealth;
    }

    public void RegisterOnDamagedDelegate(OnDamaged d){
        if(damagedDelegates == null){
            damagedDelegates = new List<OnDamaged>();
        }

        damagedDelegates.Add(d);
    }

    public void RegisterOnKilledDelegate(OnKilled d){
        if(killedDelegates == null){
            killedDelegates = new List<OnKilled>();
        }

        killedDelegates.Add(d);
    }

    public void DealDamage(float amount){
        currentHealth -= amount;

        foreach(OnDamaged damagedDelegate in damagedDelegates){
            damagedDelegate(this);
        }
    }
}