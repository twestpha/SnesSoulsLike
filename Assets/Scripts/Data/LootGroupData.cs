using System.Collections;
using System.Collections.Generic;
using UnityEngine;
﻿using System;

[Serializable]
public struct LootEntry {
    public Vector2 quantityRange;
    public ItemData item;
}

[CreateAssetMenu(fileName = "LootGroupData", menuName = "Soulsie/LootGroupData", order = 3)]
public class LootGroupData : ScriptableObject {
    public LootEntry[] lootEntries;
    
    public void GiveRandomItem(InventoryComponent inventoryToGiveTo){
        int pickedIndex = (int) UnityEngine.Random.Range(0, lootEntries.Length);
        int count = (int) Mathf.Round(UnityEngine.Random.Range(lootEntries[pickedIndex].quantityRange.x, lootEntries[pickedIndex].quantityRange.y));
        
        inventoryToGiveTo.GiveItem(lootEntries[pickedIndex].item, count);
    }
}