using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

public class InventoryComponent : MonoBehaviour {
    
    public const string OPAL_ITEM_LOC = "item_opal";
    
    [Header("Initial Items")]
    public ItemData[] startingItems;
    [Header("Equipped Items")]
    public ItemData leftHandEquippedItem;
    public ItemData rightHandEquippedItem;
    public ItemData headEquippedItem;
    public ItemData bodyEquippedItem;
    
    private bool isPlayer;
    private bool didLoad = false;
    private Dictionary<ItemData, int> inventory = new();
    private CharacterRenderable characterRenderable;

    void Start(){
        isPlayer = GetComponent<PlayerComponent>() != null;
        characterRenderable = GetComponentInChildren<CharacterRenderable>();
        
        // Only the player does a save/load
        if(!isPlayer || !didLoad){
            for(int i = 0, count = startingItems.Length; i < count; ++i){
                GiveItem(startingItems[i], 1, true);
            }
            
            if(leftHandEquippedItem != null){ EquipItem(leftHandEquippedItem, EquipLocation.LeftHand); }
            if(rightHandEquippedItem != null){ EquipItem(rightHandEquippedItem, EquipLocation.RightHand); }
            
            // Only player equips head and body on start, NPCs use their default clothing
            if(isPlayer){
                if(headEquippedItem != null){ EquipItem(headEquippedItem, EquipLocation.Head); }
                if(bodyEquippedItem != null){ EquipItem(bodyEquippedItem, EquipLocation.Body); }
            }
        }
    }
    
    public bool CanEquipItem(ItemData item, EquipLocation location){
        return item.equipLocation == location
            || (item.equipLocation == EquipLocation.BothHands && (location == EquipLocation.LeftHand || location == EquipLocation.RightHand));
    }
    
    public void EquipItem(ItemData item, EquipLocation location){
        if(CanEquipItem(item, location)){
            
            // Left Hand
            if(location == EquipLocation.LeftHand){
                if(leftHandEquippedItem != null){ Unequip(EquipLocation.LeftHand); }
                leftHandEquippedItem = item;
            }
            
            // Right Hand
            if(location == EquipLocation.RightHand){
                if(rightHandEquippedItem != null){ Unequip(EquipLocation.RightHand); }
                rightHandEquippedItem = item;
            }
            
            // Both hands
            if(location == EquipLocation.BothHands){
                if(leftHandEquippedItem != null){ Unequip(EquipLocation.LeftHand); }
                if(rightHandEquippedItem != null){ Unequip(EquipLocation.RightHand); }
                
                leftHandEquippedItem = item;
                rightHandEquippedItem = item;
            }
            
            // Head
            if(location == EquipLocation.Head){
                if(headEquippedItem != null){ Unequip(EquipLocation.Head); }
                headEquippedItem = item;
            }
            
            // Body
            if(location == EquipLocation.Body){
                if(bodyEquippedItem != null){ Unequip(EquipLocation.Body); }
                bodyEquippedItem = item;
            }
            
            AbleRenderableMesh(item.itemVisualName, true);
            
            // Refresh UI for player...?
            // if(isPlayer){ PlayerUIComponent.instance.UpdateItemIcons(); }
        }
    }
    
    public void Unequip(EquipLocation location){
        string unequippedItemVisualName = "";
        
        // Left Hand
        if(location == EquipLocation.LeftHand){
            if(leftHandEquippedItem != null){ unequippedItemVisualName = leftHandEquippedItem.itemVisualName; }
            leftHandEquippedItem = null;
        }
        
        // Right Hand
        if(location == EquipLocation.RightHand){
            if(rightHandEquippedItem != null){ unequippedItemVisualName = rightHandEquippedItem.itemVisualName; }
            rightHandEquippedItem = null;
        }
        
        // Head
        if(location == EquipLocation.Head){
            if(headEquippedItem != null){ unequippedItemVisualName = headEquippedItem.itemVisualName; }
            headEquippedItem = null;
        }
        
        // Body
        if(location == EquipLocation.Body){
            if(bodyEquippedItem != null){ unequippedItemVisualName = bodyEquippedItem.itemVisualName; }
            bodyEquippedItem = null;
        }
        
        if(!String.IsNullOrEmpty(unequippedItemVisualName)){
            AbleRenderableMesh(unequippedItemVisualName, false);
        }
    }
    
    public void AbleRenderableMesh(string name, bool able){
        if(characterRenderable != null){
            GameObject renderableMesh = characterRenderable.FindNamedObjectInCharacter(name);
            
            if(renderableMesh != null){
                renderableMesh.SetActive(able);
            } else {
                Debug.LogError("Couldn't find renderable mesh " + name);
            }
        }
    }
    
    public bool HasItem(ItemData item){
        return inventory.ContainsKey(item) && inventory[item] > 0;
    }
    
    public bool HasItem(ItemType itemType){
        foreach( KeyValuePair<ItemData, int> kvp in inventory){
            if(kvp.Key.itemType == itemType){
                return true;
            }
        }
        
        return false;
    }
    
    public bool TakeItem(ItemData item){
        if(HasItem(item)){
            inventory[item]--;
            
            if(inventory[item] <= 0){
                if(leftHandEquippedItem == item){ Unequip(EquipLocation.LeftHand); }
                if(rightHandEquippedItem == item){ Unequip(EquipLocation.RightHand); }
                if(headEquippedItem == item){ Unequip(EquipLocation.Head); }
                if(bodyEquippedItem == item){ Unequip(EquipLocation.Body); }
                
                inventory.Remove(item);
            }
            
            return true;
        }
        
        return false;
    }
    
    public bool TakeItem(ItemType itemType){
        foreach(KeyValuePair<ItemData, int> kvp in inventory){
            if(kvp.Key.itemType == itemType){
                TakeItem(kvp.Key);
                return true;
            }
        }
        
        return false;
    }
    
    public void GiveItem(ItemData item, int count = 1, bool squelchNotification = false){
        if(inventory.ContainsKey(item)){
            inventory[item] += count;
        } else {
            inventory[item] = count;
        }
        
        if(isPlayer && !squelchNotification){
            PlayerComponent.player.ShowMessage("Got cool " + count + " unlocalized item(s): " + item.nameLoc);
        }
    }
    
    public void TransferItem(ItemData item, InventoryComponent targetInventory){
        if(TakeItem(item)){
            targetInventory.GiveItem(item);
        }
    }
    
    public void TransferAll(InventoryComponent targetInventory){
        foreach(var kvp in inventory){
            for(int i = 0; i < kvp.Value; ++i){
                targetInventory.GiveItem(kvp.Key);
            }
        }
        
        inventory.Clear();
    }
    
    private const char DELIMETER_C = ':';
    
    // public bool Load(){
    //     Save save = SaveLoadManagerComponent.instance.save;
    // 
    //     if(save.stringLists.ContainsKey("playerInventory")){
    //         // Force clear current inventory
    //         currentOpal = 0;
    //         inventory.Clear();
    //         leftHandEquippedItem = null;
    //         rightHandEquippedItem = null;
    //         headEquippedItem = null;
    //         bodyEquippedItem = null;
    // 
    //         if(leftHandItemInstance != null){ Destroy(leftHandItemInstance); }
    //         if(rightHandItemInstance != null){ Destroy(rightHandItemInstance); }
    //         if(headItemInstance != null){ Destroy(headItemInstance); }
    // 
    //         // Load existing list
    //         List<string> inventoryList = save.stringLists["playerInventory"];
    // 
    //         for(int i = 0, count = inventoryList.Count; i < count; ++i){
    //             // Debug.Log(inventoryList[i]);
    //             string s = inventoryList[i];
    //             string[] tokens = s.Split(DELIMETER_C);
    // 
    //             if(s.Contains("opal")){
    //                 currentOpal = Int32.Parse(tokens[1]);
    //             } else if(s.Contains("equipped")){
    //                 string itemName = tokens[2];
    // 
    //                 if(tokens[1].Contains("left")){
    //                     ItemData foundData = GameManagerComponent.instance.gameData.GetItem(itemName);
    // 
    //                     if(foundData != null){
    //                         EquipItem(foundData, EquipLocation.LeftHand);
    //                     } else {
    //                         Debug.Log("Couldn't find equipped item " + itemName + " for player inventory load!");
    //                     }
    //                 } else if(tokens[1].Contains("right")){
    //                     ItemData foundData = GameManagerComponent.instance.gameData.GetItem(itemName);
    // 
    //                     if(foundData != null){
    //                         EquipItem(foundData, EquipLocation.RightHand);
    //                     } else {
    //                         Debug.Log("Couldn't find equipped item " + itemName + " for player inventory load!");
    //                     }
    //                 } else if(tokens[1].Contains("head")){
    //                     ItemData foundData = GameManagerComponent.instance.gameData.GetItem(itemName);
    // 
    //                     if(foundData != null){
    //                         EquipItem(foundData, EquipLocation.Head);
    //                     } else {
    //                         Debug.Log("Couldn't find equipped item " + itemName + " for player inventory load!");
    //                     }
    //                 } else if(tokens[1].Contains("body")){
    //                     ItemData foundData = GameManagerComponent.instance.gameData.GetItem(itemName);
    // 
    //                     if(foundData != null){
    //                         EquipItem(foundData, EquipLocation.Body);
    //                     } else {
    //                         Debug.Log("Couldn't find equipped item " + itemName + " for player inventory load!");
    //                     }
    //                 }
    //             } else {
    //                 string itemName = tokens[0];
    //                 int itemAmount = Int32.Parse(tokens[1]);
    // 
    //                 ItemData foundData = GameManagerComponent.instance.gameData.GetItem(itemName);
    //                 if(foundData != null){
    //                     for(int j = 0; j < itemAmount; ++j){
    //                         GiveItem(foundData, true);
    //                     }
    //                 } else {
    //                     Debug.Log("Couldn't find item " + itemName + " for player inventory load!");
    //                 }
    //             }
    //         }
    // 
    //         didLoad = true;
    //         return true;
    //     } else {
    //         return false;
    //     }
    // }
    
    // public void UpdateSave(Save save){
    //     List<string> inventoryList = new();
    // 
    //     foreach( KeyValuePair<ItemData, int> kvp in inventory){
    //         // Save as item:count 
    //         inventoryList.Add(kvp.Key.name + DELIMETER_C + kvp.Value);
    //     }
    // 
    //     // Save equipped items
    //     if(leftHandEquippedItem != null){
    //         inventoryList.Add("equipped:left:" + leftHandEquippedItem.name);
    //     }
    //     if(rightHandEquippedItem != null){
    //         inventoryList.Add("equipped:right:" + rightHandEquippedItem.name);
    //     }
    //     if(headEquippedItem != null){
    //         inventoryList.Add("equipped:head:" + headEquippedItem.name);
    //     }
    //     if(bodyEquippedItem != null){
    //         inventoryList.Add("equipped:body:" + bodyEquippedItem.name);
    //     }
    // 
    //     // Save current opal
    //     inventoryList.Add("opal:" + currentOpal); 
    // 
    //     save.stringLists["playerInventory"] = inventoryList;
    // }
}