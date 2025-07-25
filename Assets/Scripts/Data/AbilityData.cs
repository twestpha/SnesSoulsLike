using System.Collections;
using System.Collections.Generic;
using UnityEngine;
﻿using System;

public enum AbilityType {
    ItemActivated,
    ItemPassive,
    InventoryPassive,
}

[CreateAssetMenu(fileName = "AbilityData", menuName = "Soulsie/AbilityData", order = 2)]
public class AbilityData : ScriptableObject {
    
    [Header("Basic Ability Data")]
    public AbilityType abilityType;
    public float hpCost;
    public float staminaCost;
    public int charges = -1;
    public ItemData consumesItems;
}