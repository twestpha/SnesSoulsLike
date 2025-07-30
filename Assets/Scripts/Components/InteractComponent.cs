using UnityEngine;
using System;

public enum InteractType {
    Loot,
    Message,
    Shop,
    Campfire,
    EnterDoor,
}

public enum InteractCount {
    Once,
    Infinite,
    None,
}

[Serializable]
public class InteractOption {
    public string optionLocText;
    public InteractType type;
    public ItemData[] requiredItems;
    public ItemType[] requiredItemTypes;
    public InteractCount count;
    
    [Header("Loot Options")]
    public ScriptableObject itemOrLootGroup;
    
    [Header("Message Options")]
    public string messageLocText;
    public ItemData giveItemAfterMessage;
    
    [Header("Shop Options")]
    public int notYet;
    
    [Header("Door Options")]
    public int notYet2;
}

class InteractComponent : MonoBehaviour {
    
    public InteractOption[] options;
    
    private CreatureComponent creature;
    
    void Start(){
        creature = GetComponent<CreatureComponent>();
    }
    
    public int GetInteractCount(){
        int interactCount = 0;
        
        for(int i = 0, count = options.Length; i < count; ++i){
            if(CanInteract(options[i])){
                interactCount++;
            }
        }
        
        if(interactCount > 0){
            // Only add "leave" if there's already options
            interactCount++;
        }
        
        return interactCount;
    }
    
    public string GetInteractString(int index){
        int interactCount = 0;
        
        for(int i = 0, count = options.Length; i < count; ++i){
            try {
                if(CanInteract(options[i])){
                    if(interactCount == index){
                        return options[i].optionLocText;
                    }
                    
                    count++;
                }
            } catch {
                break;
            }
        }
        
        if(index >= interactCount){
            return "leave";
        }
        
        return "";
    }
    
    private bool CanInteract(InteractOption option){
        // We're done with that interact
        if(option.count == InteractCount.None){
            return false;
        }
    
        // Check for required items or item types
        // player must have ALL of them, essentially an && operation
        InventoryComponent playerInventory = PlayerComponent.player.GetComponent<InventoryComponent>();
        for(int i = 0, count = option.requiredItems.Length; i < count; ++i){
            if(!playerInventory.HasItem(option.requiredItems[i])){
                return false;
            }
        }
        
        for(int i = 0, count = option.requiredItemTypes.Length; i < count; ++i){
            if(!playerInventory.HasItem(option.requiredItemTypes[i])){
                return false;
            }
        }
        
        // Loot requires the creature to be dead
        if(option.type == InteractType.Loot){
            return creature.Dead();
        }
        
        return true;
    }
    
    public void ChooseInteractOption(int index){
        int interactCount = 0;
        
        for(int i = 0, count = options.Length; i < count; ++i){
            try {
                if(CanInteract(options[i])){
                    if(interactCount == index){
                        Interact(options[i]);
                        return;
                    }
                    
                    count++;
                }
            } catch {
                break;
            }
        }
    }
    
    private void Interact(InteractOption option){
        // If once, set to none
        if(option.count == InteractCount.Once){
            option.count = InteractCount.None;
        }
        
        if(option.type == InteractType.Loot){
            InventoryComponent playerInventory = PlayerComponent.player.GetComponent<InventoryComponent>();

            if(option.itemOrLootGroup is ItemData singleItem){
                playerInventory.GiveItem(singleItem);
            } else if(option.itemOrLootGroup is LootGroupData lootGroup){
                lootGroup.GiveRandomItem(playerInventory);
            }
        } else if(option.type == InteractType.Message){
            PlayerComponent.player.ShowMessage(option.messageLocText, option.giveItemAfterMessage);
        } else if(option.type == InteractType.Shop){
            Debug.Log("TODO!");
        } else if(option.type == InteractType.Campfire){
            PlayerComponent.player.Rest(this);
        } else if(option.type == InteractType.EnterDoor){
            Debug.Log("TODO!");
        }
    }
}