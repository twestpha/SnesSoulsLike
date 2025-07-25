using System.Collections;
using System.Collections.Generic;
using UnityEngine;
﻿using System;

public enum EquipLocation {
    None,
    LeftHand,
    RightHand,
    BothHands,
    Body,
    Head,
}

[CreateAssetMenu(fileName = "ItemData", menuName = "Soulsie/ItemData", order = 1)]
public class ItemData : ScriptableObject {
    
    [Header("Basic Item Data")]
    public string nameLoc;
    public string descLoc;
    public int opalValue = 1;
    [Header("Equipping and Using")]
    public AbilityData[] abilities;
    public EquipLocation equipLocation;
    public string itemVisualName;
    [Header("Sprites and Icons")]
    public Sprite inventorySprite;
}
