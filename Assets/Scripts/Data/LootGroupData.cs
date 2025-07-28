using System.Collections;
using System.Collections.Generic;
using UnityEngine;
﻿using System;

[CreateAssetMenu(fileName = "LootGroupData", menuName = "Soulsie/LootGroupData", order = 3)]
public class LootGroupData : ScriptableObject {
    public ItemData[] items;
    
    public ItemData GetRandomItem(){
        return items[(int) UnityEngine.Random.Range(0, items.Length)];
    }
}