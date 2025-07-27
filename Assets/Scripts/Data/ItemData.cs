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

public enum ItemType {
    None,
    Arrow,
}

[CreateAssetMenu(fileName = "ItemData", menuName = "Soulsie/ItemData", order = 1)]
public class ItemData : ScriptableObject {
    
    [Header("Basic Item Data")]
    public string nameLoc;
    public string descLoc;
    public ItemType itemType;
    public Sprite inventorySprite;
    
    [Space(10)]
    public bool keyItem;
    public int opalValue = 1;
    
    [Header("Equipping")]
    public EquipLocation equipLocation;
    public string itemVisualName;
    
    [Header("Abilities")]
    public AbilityData ability;
}