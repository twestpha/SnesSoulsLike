using System.Collections;
using System.Collections.Generic;
using UnityEngine;
﻿using System;

public enum EffectType {
    ChangeCurrentHp,
    ChangeCurrentStamina,
    ChangeCurrentMagic,
    ChangeMaxHp,
    ChangeMaxStamina,
    ChangeMaxMagic,
    RegenCurrentHp,
    RegenCurrentStamina,
    RegenCurrentMagic,
    GiveTagItem,
    TakeTagItem,
}

public enum DamageType {
    None,
    Gouging,   // Generic "physical" damage
    Festering, // Rotting, worms, infection
    Searing,   // Fire, heat
    Trauma,    // Mental anguish
}

[Serializable]
public class EffectData {
    [Header("Effect Attributes")]
    // TODO add who gets targeted, the caster or the attacked?
    [Tooltip("The type of effect, how it will manifest in the player/creature")]
    public EffectType effectType;
    [Tooltip("The amount of heals/stamina/magic/etc it will heal/hurt/change. Random from x to y.")]
    public Vector2 valueRange;
    [Tooltip("(UNUSED) If the effect would deal damage, what type of damage")]
    public DamageType damageType;
    [Tooltip("(UNUSED) If the effect would give a tag item, which one")]
    public ItemData tagItem;
    [Tooltip("(UNUSED) If the effect happens over time, for how long")]
    public float time;
    
    public float GetFinalValue(){
        return UnityEngine.Random.Range(valueRange.x, valueRange.y);
    }
}