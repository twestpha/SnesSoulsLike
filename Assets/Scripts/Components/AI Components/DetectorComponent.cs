using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

class DetectorComponent : MonoBehaviour {

    private HashSet<UnitComponent> enemyUnits = new HashSet<UnitComponent>();
    // Later, waypoints too

    private UnitComponent unit;

    public delegate void OnEnemyEntered();
    private List<OnEnemyEntered> enemyEnteredDelegates;

    void Start(){
        unit = GetComponentInParent<UnitComponent>();
    }

    private void OnTriggerEnter(Collider other){
        UnitComponent otherUnit = other.GetComponent<UnitComponent>();

        if(otherUnit != null && otherUnit.team != unit.team){
            enemyUnits.Add(otherUnit);

            if(enemyEnteredDelegates != null){
                foreach(OnEnemyEntered enemyEnteredDelegate in enemyEnteredDelegates){
                    enemyEnteredDelegate();
                }
            }
        }
    }

    private void OnTriggerExit(Collider other){
        UnitComponent otherUnit = other.GetComponent<UnitComponent>();

        if(otherUnit != null && enemyUnits.Contains(otherUnit)){
            enemyUnits.Remove(otherUnit);
        }
    }

    public void RegisterOnEnemyEnteredDelegate(OnEnemyEntered enemyEnteredDelegate){
        if(enemyEnteredDelegates == null){
            enemyEnteredDelegates = new List<OnEnemyEntered>();
        }

        enemyEnteredDelegates.Add(enemyEnteredDelegate);
    }

    public HashSet<UnitComponent> GetEnemyUnits(){
        return enemyUnits;
    }
}