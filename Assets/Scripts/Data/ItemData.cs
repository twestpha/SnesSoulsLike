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
    Arrow,       // Consumed by bows, etc.
    BladedTool,  // Can be used to gut animals
    Money,       // Can be exchanged for goods and services
    LightSource, // Fog volume component changes the fog distance based on this
    KeyItem,     // Cannot be sold, story critical stuff
    TagItem,     // Hidden in UI, can only have max 1, and used as a status indicator.
}

[CreateAssetMenu(fileName = "ItemData", menuName = "Soulsie/ItemData", order = 1)]
public class ItemData : ScriptableObject {
    
    [Header("Basic Item Data")]
    public string nameLoc;
    public string descLoc;
    public ItemType itemType;
    public Sprite hudSprite;
    public Sprite inventorySprite;
    
    [Space(10)]
    public int moneyValue = 1;
    
    [Header("Equipping")]
    public EquipLocation equipLocation;
    public string itemVisualName;
    
    [Header("Abilities")]
    public AbilityData ability;
}