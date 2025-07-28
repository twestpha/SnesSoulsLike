using UnityEngine;
using System;

public enum InteractType {
    Loot,
    Message,
    Shop,
}

public enum InteractCount {
    Once,
    Infinite,
    None,
}

[Serializable]
public class InteractOption {
    public string locText;
    public InteractType type;
    public ItemType requiredItem;
    public InteractCount count;
    
    [Header("Loot Options")]
    public ScriptableObject itemOrLootGroup;
    
    [Header("Message Options")]
    public string messageLocText;
    public ItemData giveItemAfterMessage;
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
                        return options[i].locText;
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
        if(option.count == InteractCount.None){
            return false;
        }
        
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
                        Debug.Log("Interact?");
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
                playerInventory.GiveItem(lootGroup.GetRandomItem());
            }
        } // ...
    }
}