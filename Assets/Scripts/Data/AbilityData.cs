using System.Collections;
using System.Collections.Generic;
using UnityEngine;
﻿using System;

public enum AbilityType {
    HitboxEnable,
    FireProjectile,
    Consumable,
    Placeable,
}

public enum ProjectileSpeed {
    Slow,
    Medium,
    Fast,
}

[CreateAssetMenu(fileName = "AbilityData", menuName = "Soulsie/AbilityData", order = 2)]
public class AbilityData : ScriptableObject {

    [Header("Type")]
    public AbilityType abilityType;
    
    [Header("General")]
    public float staminaCost;
    public ItemType itemCost;
    
    [Header("Hitbox Enable")]
    public AnimationState hitboxAnimation;
    public string hitBoxName;
    public float hitboxWarmupTime;
    public float hitboxDuration;
    public float hitboxCooldown;
    
    [Header("Fire Projectile")]
    public GameObject shootProjectile;
    public ProjectileSpeed shootSpeed;
    public float shootwarmupTime;
    public float shootDuration;
    public float shootCooldown;
    
    // [Header("Consumable")]
    // Nothing needed? Just apply the effects to the player instantly
    
    [Header("Placeable")]
    public GameObject placeablePrefab;
    
    [Header("Effects")]
    public EffectData[] effects;
}