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
    public float value;
    public float variance;
    public string state;
    public float time;
}