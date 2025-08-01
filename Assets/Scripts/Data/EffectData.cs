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
    GiveState,
    RemoveState,
}

[Serializable]
public class EffectData {
    [Header("Effect Attributes")]
    // TODO add who gets targeted, the caster or the attacked?
    public EffectType effectType;
    public Vector2 valueRange;
    public string state;
    public float time;
    
    public float GetFinalValue(){
        return UnityEngine.Random.Range(valueRange.x, valueRange.y);
    }
}