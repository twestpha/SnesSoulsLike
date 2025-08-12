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
    [Tooltip("What action should this ability do")]
    public AbilityType abilityType;
    
    [Header("General")]
    [Tooltip("Amount of stamina required then taken to cast")]
    public float staminaCost;
    [Tooltip("Type of item required then taken to cast")]
    public ItemType itemCost;
    
    [Header("Hitbox Enable")]
    [Tooltip("What animation to play in sync with the hitbox enabling")]
    public AnimationState hitboxAnimation;
    [Tooltip("Name of the hit box component's gameobject to find in the prefab")]
    public string hitBoxName;
    [Tooltip("How much time before dealing damage starts")]
    public float hitboxWarmupTime;
    [Tooltip("How much time damage can be dealt")]
    public float hitboxDuration;
    [Tooltip("How much time after dealing damage the ability takes to finish")]
    public float hitboxCooldown;
    
    [Header("Fire Projectile")]
    [Tooltip("What projectile should be shot")]
    public GameObject shootProjectile;
    [Tooltip("How fast the projectile should be shot")]
    public ProjectileSpeed shootSpeed;
    [Tooltip("How much time before the shoot starts")]
    public float shootwarmupTime;
    [Tooltip("How long the shoot lasts")]
    public float shootDuration;
    [Tooltip("How much time after shooting the ability takes to finish")]
    public float shootCooldown;
    
    // [Header("Consumable")]
    // Nothing needed? Just apply the effects to the player instantly
    
    [Header("Placeable")]
    [Tooltip("What placeable to put in front of the caster")]
    public GameObject placeablePrefab;
    [Tooltip("What item to give to the caster once placed (For things like getting an empty bottle when placing liquid)")]
    public ItemData giveItemOnPlace;
    
    [Header("Effects")]
    [Tooltip("What effects this ability will apply to the hit creature/player")]
    public EffectData[] effects;
}