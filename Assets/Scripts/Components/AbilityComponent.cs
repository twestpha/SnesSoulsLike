using System.Collections;
using System;
using UnityEngine;
using UnityEngine.UI;

public enum AbilityState {
    None,
    PreAttack,
    PostAttack,
    CoolingDown,
    WaitingOnInput,
}

class AbilityComponent : MonoBehaviour {
    
    private const int DEFAULT_LAYER_MASK = 1 << 0;

    private Timer abilityTimer = new();
    private AbilityData currentAbility;
    
    private PlayerComponent player;
    private CreatureComponent creature;
    private InventoryComponent inventory;
    
    private AbilityState abilityState;
    
    private HitBoxComponent hitbox;
    
    void Start(){
        player = GetComponent<PlayerComponent>();
        creature = GetComponent<CreatureComponent>();
        inventory = GetComponent<InventoryComponent>();
    }
    
    void Update(){
        if(currentAbility != null){
            // Run a hitbox type of ability
            if(currentAbility.abilityType == AbilityType.HitboxEnable){
                if(abilityState == AbilityState.PreAttack){
                    if(abilityTimer.Finished()){
                        hitbox.EnableForTime(currentAbility.hitboxDuration);
                        
                        abilityTimer.SetDuration(currentAbility.hitboxDuration);
                        abilityTimer.Start();
                        
                        abilityState = AbilityState.PostAttack;
                    }
                } else if(abilityState == AbilityState.PostAttack){
                    if(abilityTimer.Finished()){
                        abilityTimer.SetDuration(currentAbility.hitboxCooldown);
                        abilityTimer.Start();
                        
                        abilityState = AbilityState.CoolingDown;
                    }
                } else if(abilityState == AbilityState.CoolingDown){
                    if(abilityTimer.Finished()){
                        currentAbility = null;
                        return;
                    }
                }
            // Run a fire-projectile type of ability
            } else if(currentAbility.abilityType == AbilityType.FireProjectile){
                if(abilityState == AbilityState.PreAttack){
                    if(abilityTimer.Finished()){
                        abilityState = AbilityState.WaitingOnInput;
                    }
                } else if(abilityState == AbilityState.PostAttack){
                    if(abilityTimer.Finished()){
                        abilityTimer.SetDuration(currentAbility.shootCooldown);
                        abilityTimer.Start();
                
                        abilityState = AbilityState.CoolingDown;
                    }
                } else if(abilityState == AbilityState.CoolingDown){
                    if(abilityTimer.Finished()){
                        currentAbility = null;
                        return;
                    }
                }
            }
        }
    }
    
    public void NotifyOfInput(AbilityData abilityWithInput){
        if(abilityState == AbilityState.WaitingOnInput && currentAbility == abilityWithInput){
            abilityTimer.SetDuration(currentAbility.shootDuration);
            abilityTimer.Start();

            // Play animation
            if(player != null){ player.PlayAnimation(AnimationState.Shoot); }
            if(creature != null){ creature.PlayAnimation(AnimationState.Shoot); }
            
            // Setup projectile
            GameObject newProjectileObject = GameObject.Instantiate(currentAbility.shootProjectile);
            Transform originTransform;
            
            if(player != null){
                originTransform = player.playerSpriteTransform;
            } else {
                originTransform = transform;
            }
            
            newProjectileObject.transform.position = originTransform.position + (Vector3.up * 0.05f) + (originTransform.forward * 0.2f);
            newProjectileObject.transform.rotation = originTransform.rotation;
            
            ProjectileComponent newProjectile = newProjectileObject.GetComponent<ProjectileComponent>();
            newProjectile.Fire(
                currentAbility,
                originTransform.forward * GetProjectileSpeed(currentAbility.shootSpeed), 
                gameObject
            );
    
            // Switch to post-attack and cooldown
            abilityState = AbilityState.PostAttack;
        }
    }
    
    public bool CastingAnyAbility(){
        return currentAbility != null;
    }
    
    public bool CanCast(AbilityData abilityData){
        if(abilityData == null){
            return false;
        }
        
        bool canCast = currentAbility == null;
        
        // Only apply certain requirements to the player
        if(player != null){
            if(abilityData.staminaCost > 0){
                canCast = canCast && player.currentStamina > abilityData.staminaCost;
            }
            
            if(abilityData.itemCost != ItemType.None){
                canCast = canCast && inventory.HasItem(abilityData.itemCost);
            }
            
            // TODO if in overworld and not in a dungeon, cannot place a campfire 
        }
        
        return canCast;
    }
    
    public void Cast(AbilityData abilityData, ItemData castingItem = null){
        // First, pay the stamina or item cost only if it's the player
        if(player != null){
            if(abilityData.staminaCost > 0){
                player.currentStamina = Mathf.Max(player.currentStamina - abilityData.staminaCost, 0.0f);
            }
            
            if(abilityData.itemCost != ItemType.None){
                inventory.TakeItem(abilityData.itemCost);
            }
        }
        
        // Setup per ability type
        if(abilityData.abilityType == AbilityType.HitboxEnable){
            // Find and cache hitbox by name
            hitbox = null;
            HitBoxComponent[] hitBoxComponents = GetComponentsInChildren<HitBoxComponent>();
            
            for(int i = 0, count = hitBoxComponents.Length; i < count; ++i){
                if(hitBoxComponents[i].gameObject.name == abilityData.hitBoxName){
                    hitbox = hitBoxComponents[i];
                }
            }
            
            if(hitbox == null){
                Debug.LogError("Couldn't find hitbox named " + abilityData.hitBoxName);
                return;
            }
            
            hitbox.hitEffects = abilityData.effects;
            
            // Play animation
            if(player != null){ player.PlayAnimation(abilityData.hitboxAnimation); }
            if(creature != null){ creature.PlayAnimation(abilityData.hitboxAnimation); }
            
            // Setup times and state
            abilityTimer.SetDuration(abilityData.hitboxWarmupTime);
            abilityTimer.Start();
            
            abilityState = AbilityState.PreAttack;
            
            currentAbility = abilityData;
        } else if(abilityData.abilityType == AbilityType.FireProjectile){
            // Play animation
            if(player != null){ player.PlayAnimation(AnimationState.Aim); }
            if(creature != null){ creature.PlayAnimation(AnimationState.Aim); }
            
            // Setup times and state
            abilityTimer.SetDuration(abilityData.shootwarmupTime);
            abilityTimer.Start();
            
            abilityState = AbilityState.PreAttack;
            
            currentAbility = abilityData;
        } else if(abilityData.abilityType == AbilityType.Consumable){
            if(player != null){ player.ApplyEffects(abilityData.effects); }
            if(creature != null){ creature.ApplyEffects(abilityData.effects); }
            
            if(castingItem != null){
                inventory.TakeItem(castingItem);
            }
        } else if(abilityData.abilityType == AbilityType.Placeable){
            GameObject newPlaceable = GameObject.Instantiate(abilityData.placeablePrefab);
            
            Transform originTransform = null;
            if(player != null){
                originTransform = player.playerSpriteTransform;
            } else {
                originTransform = transform;
            }
            
            // Place in front of caster raycasting against terrain
            RaycastHit hit;
            if(Physics.Raycast(originTransform.position + (Vector3.up * 0.1f) + (originTransform.forward * 0.2f),
               -Vector3.up, out hit, 1.0f, DEFAULT_LAYER_MASK, QueryTriggerInteraction.Ignore)
            ){
                newPlaceable.transform.position = hit.point;
            } else {
                newPlaceable.transform.position = originTransform.position + (originTransform.forward * 0.2f);
            }
            
            if(castingItem != null){
                inventory.TakeItem(castingItem);
            }
            
            if(abilityData.giveItemOnPlace != null){
                inventory.GiveItem(abilityData.giveItemOnPlace);
            }
        }
    }
    
    public static float GetProjectileSpeed(ProjectileSpeed speed){
        if(speed == ProjectileSpeed.Slow){
            return 1.0f;
        } else if(speed == ProjectileSpeed.Medium){
            return 3.0f;
        } else if(speed == ProjectileSpeed.Fast){
            return 8.0f;
        }
        
        return 0.0f;
    }
}