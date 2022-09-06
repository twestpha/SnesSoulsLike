using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "UnitAbility", menuName = "Units/Unit Ability", order = 1)]
[System.Serializable]
public class UnitAbilityData : ScriptableObject {

    // TODO loc name, description, sprite, cooldown

    [Header("Casting")]
    public bool castOnButtonUp; // As opposed to button down
    public GameObject castIndicatorPrefab;

    public float snapToEnemyAngle = 0.0f;

    [Header("Duration")]
    public float abilityDuration = 1.0f;

    // Percent after which this ability can be interrupted by other abilities
    public float interruptPercent = 1.0f;

    [Header("Movement During Ability")]
    // How much of the percentage of the abilityDuration the move should occur during
    public float movePercentage = 1.0f;
    // Direction relative to unit's forward when cast
    public Vector3 moveDirection;

    public float maxMoveSpeed;
    public AnimationCurve moveSpeedCurve;

    [Header("Animation")]
    public string animationName;

    [Header("Receiving and Dealing Damage")]
    // How long (if at all) should the unit be invincible
    public Vector2 invincibleRangePercent;
    // How long (if at all) should the unit's weapon be able to deal damage
    public Vector2 weaponSharpRangePercent;
    // TODO weapon motion blur/streak effect amount over time (using line renderer?)

    [Header("Collision")]
    public Vector2 collisionResizeRangePercent;
    public float resizeHeight;

    [Header("Spawning Prefab")]
    public float spawnTimePercent;
    public GameObject spawnPrefab;
    // Relative to unit's forward when cast
    public Vector3 spawnOffset;
}
