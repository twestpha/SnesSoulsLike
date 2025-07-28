using System.Collections;
using System.Collections.Generic;
using UnityEngine;
﻿using System;

public enum EffectType {
    ChangeCurrentHp,
    ChangeCurrentStamina,
    ChangeMaxHp,
    ChangeMaxStamina,
    RegenCurrentHp,
    RegenCurrentStamina,
    GiveState,
    RemoveState,
}

[Serializable]
public class EffectData {
    [Header("Effect Attributes")]
    public EffectType effectType;
    public Vector2 valueRange;
    public string state;
    public float time;
    
    public float GetFinalValue(){
        return UnityEngine.Random.Range(valueRange.x, valueRange.y);
    }
}