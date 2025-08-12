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
    AnimalTool,  // Can be used to gut animals
    Money,       // Can be exchanged for goods and services
    LightSource, // Fog volume component changes the fog distance based on this
    KeyItem,     // Cannot be sold, story critical stuff
    TagItem,     // Hidden in UI, can only have max 1, and used as a status indicator.
    WoodTool,    // Used for harvesting bark from trees
}

[CreateAssetMenu(fileName = "ItemData", menuName = "Soulsie/ItemData", order = 1)]
public class ItemData : ScriptableObject {
    
    [Header("Basic Item Data")]
    [Tooltip("Localization key for the name of item")]
    public string nameLoc;
    [Tooltip("Localization key for the plural name of item")]
    public string pluralNameLoc;
    [Tooltip("Localization key for the description of item")]
    public string descLoc;
    [Tooltip("Type of the item in broad classification. This has mechanical effects")]
    public ItemType itemType;
    [Tooltip("The sprite that is shown as the use sprite in the HUD")]
    public Sprite hudSprite;
    [Tooltip("The sprite that is shown in the player inventory")]
    public Sprite inventorySprite;
    
    [Space(10)]
    [Tooltip("The base money value of the item when selling and buying")]
    public int moneyValue = 1;
    
    [Header("Equipping")]
    [Tooltip("Where the item can be equipped")]
    public EquipLocation equipLocation;
    [Tooltip("The name of the gameobject to enable in the character renderable")]
    public string itemVisualName;
    
    [Header("Abilities")]
    [Tooltip("The ability that this item can enable casting when used")]
    public AbilityData ability;
}