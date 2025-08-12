using System.Collections;
using System.Collections.Generic;
using UnityEngine;
﻿using System;

// TODO add an enum like "any, unowned" and then the latter will only consider unowned items to give
// for stuff like random key items

public enum LootCondition {
    RandomFromList,
    OnlyUnownedItems,
}

[Serializable]
public struct LootEntry {
    public Vector2 quantityRange;
    public ItemData item;
}

[CreateAssetMenu(fileName = "LootGroupData", menuName = "Soulsie/LootGroupData", order = 3)]
public class LootGroupData : ScriptableObject {
    
    [Space(10)]
    public LootCondition lootCondition;
    public LootEntry[] lootEntries;
    public LootGroupData fallback;
    
    public void GiveRandomItem(InventoryComponent inventoryToGiveTo){
        if(lootCondition == LootCondition.RandomFromList){
            int pickedIndex = (int) UnityEngine.Random.Range(0, lootEntries.Length);
            int count = (int) Mathf.Round(UnityEngine.Random.Range(lootEntries[pickedIndex].quantityRange.x, lootEntries[pickedIndex].quantityRange.y));
            
            inventoryToGiveTo.GiveItem(lootEntries[pickedIndex].item, count);
        } else if(lootCondition == LootCondition.OnlyUnownedItems){
            if(fallback == null){
                Debug.LogError("Loot Group " + name + " is marked to give only unknowned items, and therefore MUST have a fallback group");
                return;
            }
            
            Debug.Log("Unimplemented");
        }
        
    }
}