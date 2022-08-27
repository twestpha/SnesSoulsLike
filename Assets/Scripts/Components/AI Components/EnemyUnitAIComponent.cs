using System.Collections;
using System;
using UnityEngine;
using UnityEngine.UI;

class EnemyUnitAIComponent : MonoBehaviour {
    // Catch-all for most simple AIs that don't need complex custom behaviour

    public DetectorComponent detector;

    private UnitComponent unit;

    void Start(){
        unit = GetComponent<UnitComponent>();

        // Disable ourselves until awoken by enemies
        detector.RegisterOnEnemyEnteredDelegate(OnEnemyEntered);
        enabled = false;
        unit.enabled = false;
    }

    void Update(){
        // GetEnemyUnits
    }

    private void OnEnemyEntered(){
        enabled = true;
        unit.enabled = true;
    }
}