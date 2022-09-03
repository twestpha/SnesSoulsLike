using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

class EnemyUnitAIComponent : MonoBehaviour {

    private const int INVALID_ABILITY_INDEX = -1;

    public DetectorComponent detector;

    [Serializable]
    public class AbilityHints {
        public float weight;
        public Vector2 usageRange;
        public bool away;
        public float abilityTime;
    }

    public AbilityHints[] abilityHints;

    public enum EnemyState {
        Idle,
        LookingForTarget,
        SeekingTarget,
        PerformingAbility,
        Fleeing,
    }

    public EnemyState enemyState;

    [Header("Flee Conditions and Behaviour")]
    public float fleeHealthPercentage;
    public GameObject leaderUnit;
    public float fleeTime = 1.0f;

    private UnitComponent unit;
    private HealthComponent health;

    private UnitComponent targetUnit;

    private int pickedAbility = INVALID_ABILITY_INDEX;
    private float totalAbilityWeights;
    private float pickedRange;

    private bool hasFled;
    private bool hasLeaderUnit;

    private Timer fleeTimer;
    private Timer abilityTimer = new Timer();

    private Vector3 fleeDirection;

    void Start(){
        unit = GetComponent<UnitComponent>();
        health = GetComponent<HealthComponent>();

        totalAbilityWeights = 0.0f;
        foreach(AbilityHints abilityHint in abilityHints){
            totalAbilityWeights += abilityHint.weight;
        }

        hasLeaderUnit = leaderUnit != null;

        fleeTimer = new Timer(fleeTime);

        // Disable ourselves until awoken by enemies
        detector.RegisterOnEnemyEnteredDelegate(OnEnemyEntered);
        enemyState = EnemyState.Idle;
        unit.enabled = false;
        enabled = false;
    }

    void Update(){
        // Only flee once ever
        if(!hasFled){
            if(health.GetCurrentHealthPercentage() < fleeHealthPercentage || (hasLeaderUnit && leaderUnit == null)){
                hasFled = true;

                enemyState = EnemyState.Fleeing;
                fleeTimer.Start();

                // If we have a target; flee that specifically.
                // Otherwise flee away from centroid of living player units
                if(targetUnit != null){
                    fleeDirection = transform.position - targetUnit.transform.position;
                } else {
                    Vector3 playerCentroid = Vector3.zero;

                    UnitComponent[] playerUnits = PlayerComponent.player.units;
                    foreach(UnitComponent playerUnit in playerUnits){
                        if(!playerUnit.IsDead()){
                            playerCentroid += playerUnit.transform.position;
                        }
                    }

                    playerCentroid /= ((float) playerUnits.Length);

                    fleeDirection = transform.position - playerCentroid;
                }
            }
        }

        if(enemyState == EnemyState.LookingForTarget){
            UnitComponent[] playerUnits = PlayerComponent.player.units;
            List<UnitComponent> alivePlayerUnits = new List<UnitComponent>();

            foreach(UnitComponent playerUnit in playerUnits){
                if(!playerUnit.IsDead()){
                    alivePlayerUnits.Add(playerUnit);
                }
            }

            // If none alive, go back to idle
            if(alivePlayerUnits.Count > 0){
                enemyState = EnemyState.SeekingTarget;
                targetUnit = alivePlayerUnits[UnityEngine.Random.Range(0, alivePlayerUnits.Count)];
            } else {
                enemyState = EnemyState.Idle;
                unit.enabled = false;
                enabled = false;
            }
        } else if(enemyState == EnemyState.SeekingTarget){
            if(targetUnit == null || targetUnit.IsDead()){
                targetUnit = null;
                enemyState = EnemyState.LookingForTarget;
                return;
            }

            // Pre pick ability so we know the usage range to move there
            if(pickedAbility == INVALID_ABILITY_INDEX){
                float randomWeightedValue = UnityEngine.Random.value * totalAbilityWeights;

                float totalAbilityWeightsIndex = 0.0f;
                for(int i = 0, count = abilityHints.Length; i < count; ++i){
                    totalAbilityWeightsIndex += abilityHints[i].weight;

                    if(randomWeightedValue <= totalAbilityWeightsIndex){
                        pickedAbility = i;

                        Vector2 pickedAbilityRange = abilityHints[i].usageRange;
                        pickedRange = UnityEngine.Random.Range(pickedAbilityRange.x, pickedAbilityRange.y);
                        break;
                    }
                }
            }

            Vector3 fromTargetUnitNormalized = (transform.position - targetUnit.transform.position).normalized;
            Vector3 targetPoint = targetUnit.transform.position + (fromTargetUnitNormalized * pickedRange);
            Vector3 toTargetPoint = targetPoint - transform.position;

            if(toTargetPoint.magnitude < 0.1f){
                enemyState = EnemyState.PerformingAbility;

                // Face the direction towards or away from target as marked
                if(abilityHints[pickedAbility].away){
                    unit.SetInputDirection(transform.position - targetUnit.transform.position);
                } else {
                    unit.SetInputDirection(targetUnit.transform.position - transform.position);
                }
                unit.UseAbility(pickedAbility);
                unit.SetInputDirection(Vector3.zero);

                abilityTimer.SetDuration(abilityHints[pickedAbility].abilityTime);
                abilityTimer.Start();
            } else {
                unit.SetInputDirection(toTargetPoint);
            }
        } else if(enemyState == EnemyState.PerformingAbility){
            if(abilityTimer.Finished()){
                enemyState = EnemyState.SeekingTarget;
            }
        } else if(enemyState == EnemyState.Fleeing){
            unit.SetInputDirection(fleeDirection);

            if(fleeTimer.Finished()){
                enemyState = EnemyState.SeekingTarget;
            }
        }
    }

    private void OnEnemyEntered(){
        enabled = true;
        unit.enabled = true;
        enemyState = EnemyState.LookingForTarget;
    }
}